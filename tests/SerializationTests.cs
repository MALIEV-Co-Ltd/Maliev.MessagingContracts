using System.Text.Json;
using Xunit;
using Maliev.MessagingContracts; 

namespace Maliev.MessagingContracts.Tests;

public class SerializationTests
{
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    [Fact]
    public void CanRoundTrip_Envelope()
    {
        // Placeholder test until we generate the classes
        Assert.True(true);
    }
}