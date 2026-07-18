using System.Reflection;
using System.Text.Json;
using Maliev.MessagingContracts.Generated;

namespace Maliev.MessagingContracts.Tests;

public class GeneratedContractRoundTripTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void AllGeneratedRecordTypes_CanConstructAndRoundTrip()
    {
        var generatedTypes = typeof(BaseMessage).Assembly
            .GetTypes()
            .Where(type =>
                type.IsClass &&
                !type.IsAbstract &&
                type.IsPublic &&
                type.Namespace == typeof(BaseMessage).Namespace)
            .OrderBy(type => type.FullName, StringComparer.Ordinal)
            .ToArray();

        Assert.NotEmpty(generatedTypes);

        foreach (var generatedType in generatedTypes)
        {
            var instance = CreateValue(generatedType, new HashSet<Type>());
            Assert.NotNull(instance);

            var json = JsonSerializer.Serialize(instance, generatedType, JsonOptions);
            var deserialized = JsonSerializer.Deserialize(json, generatedType, JsonOptions);

            Assert.False(string.IsNullOrWhiteSpace(json));
            Assert.NotNull(deserialized);
            Assert.IsType(generatedType, deserialized);

            foreach (var property in generatedType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                _ = property.GetValue(deserialized);
            }
        }
    }

    private static object? CreateValue(Type type, ISet<Type> activeTypes)
    {
        var nullableType = Nullable.GetUnderlyingType(type);
        if (nullableType is not null)
        {
            return null;
        }

        if (type == typeof(string))
        {
            return "contract-test";
        }

        if (type == typeof(Guid))
        {
            return Guid.NewGuid();
        }

        if (type == typeof(DateTimeOffset))
        {
            return DateTimeOffset.UnixEpoch;
        }

        if (type == typeof(DateTime))
        {
            return DateTime.UnixEpoch;
        }

        if (type == typeof(object))
        {
            return new Dictionary<string, object?> { ["source"] = "contract-test" };
        }

        if (type.IsEnum)
        {
            return Enum.GetValues(type).GetValue(0);
        }

        if (type.IsArray)
        {
            var elementType = type.GetElementType()!;
            var array = Array.CreateInstance(elementType, 1);
            array.SetValue(CreateValue(elementType, activeTypes), 0);
            return array;
        }

        if (type.IsGenericType &&
            type.GetGenericTypeDefinition() == typeof(IReadOnlyList<>))
        {
            var elementType = type.GetGenericArguments()[0];
            var array = Array.CreateInstance(elementType, 1);
            array.SetValue(CreateValue(elementType, activeTypes), 0);
            return array;
        }

        if (type.IsValueType)
        {
            return Activator.CreateInstance(type);
        }

        Assert.DoesNotContain(type, activeTypes);
        activeTypes.Add(type);

        try
        {
            var constructor = type
                .GetConstructors(BindingFlags.Instance | BindingFlags.Public)
                .OrderByDescending(candidate => candidate.GetParameters().Length)
                .First();
            var arguments = constructor
                .GetParameters()
                .Select(parameter => CreateValue(parameter.ParameterType, activeTypes))
                .ToArray();

            return constructor.Invoke(arguments);
        }
        finally
        {
            activeTypes.Remove(type);
        }
    }
}
