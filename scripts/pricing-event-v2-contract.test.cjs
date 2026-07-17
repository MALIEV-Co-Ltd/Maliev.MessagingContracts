"use strict";

const fs = require("node:fs");
const path = require("node:path");
const test = require("node:test");
const assert = require("node:assert/strict");

const Ajv = require("ajv");
const addFormats = require("ajv-formats");
const YAML = require("yaml");

const root = path.resolve(__dirname, "..");

function loadJson(...segments) {
    return JSON.parse(fs.readFileSync(path.join(root, ...segments), "utf8"));
}

function canonicalMessage() {
    return {
        messageId: "11111111-1111-1111-1111-111111111111",
        messageName: "PriceCalculatedEventV2",
        messageType: "Event",
        messageVersion: "2.0.0",
        publishedBy: "PricingService",
        consumedBy: [],
        correlationId: "22222222-2222-2222-2222-222222222222",
        causationId: null,
        occurredAtUtc: "2026-07-17T01:00:00Z",
        isPublic: false,
        payload: {
            pricingAuditId: "33333333-3333-3333-3333-333333333333",
            quotationId: null,
            fileId: "44444444-4444-4444-4444-444444444444",
            customerId: "55555555-5555-5555-5555-555555555555",
            materialId: "66666666-6666-6666-6666-666666666666",
            processId: "77777777-7777-7777-7777-777777777777",
            quantity: 3,
            inputVolumeCm3: 100,
            inputSupportVolumeCm3: 5,
            inputSurfaceAreaCm2: 60,
            strategy: "RuleBased",
            mlModelVersion: null,
            confidenceLevel: 1,
            pricingConfigurationId: "88888888-8888-8888-8888-888888888888",
            breakdown: {
                materialCost: 100,
                supportCost: 20,
                machineTimeCost: 80,
                setupCost: 50,
                variableDfmSurcharge: 10,
                fixedDfmSurcharge: 5,
                complexitySurcharge: 10,
                subtotalBeforeMargin: 715,
                marginAmount: 143,
                marginMultiplier: 1.2,
                volumeDiscountPercent: 10,
                leadTimeMultiplier: 1.1,
                toleranceMultiplier: 1.05,
                minimumOrderPriceFloorThb: 1000,
                exchangeRate: 0.03,
                baseCurrency: "THB",
                totalPrice: 30
            },
            totalUnitPrice: 10,
            totalPrice: 30,
            currency: "USD",
            validUntil: "2026-07-24T01:00:00Z",
            calculatedAt: "2026-07-17T01:00:00Z",
            storagePath: "quotes/part.stl",
            estimatedLeadTimeDays: 3
        }
    };
}

test("PriceCalculatedEventV2 accepts canonical metadata and reconstructable variable and fixed DFM costs", () => {
    const baseSchema = loadJson("contracts", "schemas", "shared", "base-message.json");
    const schema = loadJson("contracts", "schemas", "pricing", "price-calculated-event-v2.json");
    const ajv = new Ajv({ allErrors: true, strict: false });
    addFormats(ajv);
    ajv.addSchema(baseSchema);
    const validate = ajv.compile(schema);
    const canonical = canonicalMessage();

    assert.equal(validate(canonical), true, ajv.errorsText(validate.errors));

    const wrongVersion = structuredClone(canonical);
    wrongVersion.messageVersion = "1.0.0";
    assert.equal(validate(wrongVersion), false, "The v2 contract must reject a v1 version marker.");

    const missingFixedAllocation = structuredClone(canonical);
    delete missingFixedAllocation.payload.breakdown.fixedDfmSurcharge;
    assert.equal(validate(missingFixedAllocation), false, "The fixed DFM allocation is required in v2.");

    const missingVariableDfm = structuredClone(canonical);
    delete missingVariableDfm.payload.breakdown.variableDfmSurcharge;
    assert.equal(validate(missingVariableDfm), false, "The variable DFM surcharge is required in v2.");

    const breakdown = canonical.payload.breakdown;
    const variableUnitCost = breakdown.materialCost
        + breakdown.supportCost
        + breakdown.machineTimeCost
        + breakdown.variableDfmSurcharge
        + breakdown.complexitySurcharge;
    const fixedLineCost = breakdown.setupCost + breakdown.fixedDfmSurcharge;
    assert.equal(
        breakdown.subtotalBeforeMargin,
        variableUnitCost * canonical.payload.quantity + fixedLineCost,
        "The v2 breakdown must reconstruct its pre-margin subtotal without semantic relabeling."
    );
    assert.ok(
        Math.abs(
            breakdown.marginAmount
            - breakdown.subtotalBeforeMargin * (breakdown.marginMultiplier - 1)
        ) < 1e-9,
        "The line margin amount must be derived from the full pre-margin line subtotal."
    );

    const discountedVariableUnitPrice = variableUnitCost
        * breakdown.marginMultiplier
        * (1 - breakdown.volumeDiscountPercent / 100);
    const marginedFixedLineCost = fixedLineCost * breakdown.marginMultiplier;
    const rawBaseCurrencyLineTotal = (discountedVariableUnitPrice * canonical.payload.quantity
        + marginedFixedLineCost)
        * breakdown.leadTimeMultiplier
        * breakdown.toleranceMultiplier;
    const flooredBaseCurrencyLineTotal = Math.max(
        rawBaseCurrencyLineTotal,
        breakdown.minimumOrderPriceFloorThb
    );
    const reconstructedTotal = flooredBaseCurrencyLineTotal * breakdown.exchangeRate;

    assert.ok(Math.abs(rawBaseCurrencyLineTotal - 899.514) < 1e-9);
    assert.equal(breakdown.baseCurrency, "THB");
    assert.equal(reconstructedTotal, canonical.payload.totalPrice);
    assert.equal(canonical.payload.totalPrice, breakdown.totalPrice);
    assert.equal(reconstructedTotal / canonical.payload.quantity, canonical.payload.totalUnitPrice);

    for (const requiredFactor of [
        "marginMultiplier",
        "volumeDiscountPercent",
        "leadTimeMultiplier",
        "toleranceMultiplier",
        "minimumOrderPriceFloorThb",
        "exchangeRate",
        "baseCurrency"
    ]) {
        const missingFactor = structuredClone(canonical);
        delete missingFactor.payload.breakdown[requiredFactor];
        assert.equal(validate(missingFactor), false, `${requiredFactor} is required for deterministic reconstruction.`);
    }

    const wrongBaseCurrency = structuredClone(canonical);
    wrongBaseCurrency.payload.breakdown.baseCurrency = "USD";
    assert.equal(validate(wrongBaseCurrency), false, "Breakdown inputs must remain THB-denominated.");

    const zeroQuantity = structuredClone(canonical);
    zeroQuantity.payload.quantity = 0;
    assert.equal(validate(zeroQuantity), false, "Commercial reconstruction requires a positive quantity.");

    const invalidDiscount = structuredClone(canonical);
    invalidDiscount.payload.breakdown.volumeDiscountPercent = 101;
    assert.equal(validate(invalidDiscount), false, "Volume discount percentage must be bounded.");
});

test("PriceCalculatedEventV2 AsyncAPI channel references the additive v2 schema", () => {
    const document = YAML.parse(fs.readFileSync(path.join(root, "asyncapi", "asyncapi.yaml"), "utf8"));
    const channel = document.channels["pricing/price-calculated-v2"];

    assert.equal(channel.address, "maliev.pricing.events.price-calculated-v2");
    assert.deepEqual(channel.messages.PriceCalculatedEventV2, {
        $ref: "#/components/messages/PriceCalculatedEventV2"
    });
    assert.deepEqual(document.components.messages.PriceCalculatedEventV2.payload, {
        $ref: "../contracts/schemas/pricing/price-calculated-event-v2.json"
    });
});

test("Base message permits pre-consumer events but keeps routed messages fail closed", () => {
    const schema = loadJson("contracts", "schemas", "shared", "base-message.json");
    const ajv = new Ajv({ allErrors: true, strict: false });
    addFormats(ajv);
    const validate = ajv.compile(schema);
    const envelope = {
        messageId: "11111111-1111-1111-1111-111111111111",
        messageName: "ContractProbe",
        messageType: "Event",
        messageVersion: "1.0.0",
        publishedBy: "PricingService",
        consumedBy: [],
        correlationId: "22222222-2222-2222-2222-222222222222",
        causationId: null,
        occurredAtUtc: "2026-07-17T01:00:00Z",
        isPublic: false
    };

    assert.equal(validate(envelope), true, "An event may precede its first implemented consumer.");

    for (const routedType of ["Command", "Request", "Response"]) {
        const routed = { ...envelope, messageType: routedType };
        assert.equal(validate(routed), false, `${routedType} must retain at least one consumer.`);

        const routedWithConsumer = { ...routed, consumedBy: ["PricingService"] };
        assert.equal(validate(routedWithConsumer), true, `${routedType} accepts a routed consumer.`);
    }
});
