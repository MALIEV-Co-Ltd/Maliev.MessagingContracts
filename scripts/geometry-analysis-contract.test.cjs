"use strict";

const fs = require("node:fs");
const path = require("node:path");
const test = require("node:test");
const assert = require("node:assert/strict");

const Ajv = require("ajv");
const addFormats = require("ajv-formats");
const YAML = require("yaml");

const root = path.resolve(__dirname, "..");

function loadGeometrySchema() {
    const schemaPath = path.join(root, "contracts", "schemas", "geometry", "geometry-events.json");
    return JSON.parse(fs.readFileSync(schemaPath, "utf8"));
}

function geometryValidator(definitionName) {
    const schema = loadGeometrySchema();
    const ajv = new Ajv({ allErrors: true, strict: false });
    addFormats(ajv);
    ajv.addSchema(schema);
    return {
        ajv,
        validate: ajv.compile({
            $ref: `maliev/geometry-events#/definitions/${definitionName}`
        })
    };
}

test("Geometry analysis schemas accept the exact nullable GeometryService producer payloads", () => {
    const payloads = {
        FileMetricsReadyEvent: {
            fileId: "file-123",
            storagePath: "uploads/file-123.step",
            metrics: {
                volumeCm3: 12.5,
                supportVolumeCm3: 1.25,
                surfaceAreaCm2: 42.75,
                boundingBox: { x: 10, y: 20, z: 30 },
                isManifold: true,
                triangleCount: 120,
                eulerNumber: 2,
                nonManifoldReason: null,
                nonManifoldFaceCount: null
            },
            processedAt: "2026-07-13T12:00:00Z",
            bodyCount: 1,
            bodies: null
        },
        FileAnalysisFailedEvent: {
            fileId: "file-123",
            storagePath: "uploads/file-123.step",
            errorCode: "ANALYSIS_FAILED",
            details: "Geometry worker could not read the uploaded model."
        },
        DfmAnalysisReadyEvent: {
            fileId: "file-123",
            storagePath: "uploads/file-123.step",
            fdmReport: null,
            slaReport: null,
            cncReport: null,
            analyzedAt: "2026-07-13T12:00:00Z",
            overlayPaths: null,
            bodyCount: null,
            nonManifoldReason: null,
            nonManifoldFaceCount: null
        }
    };

    for (const [definitionName, payload] of Object.entries(payloads)) {
        const { ajv, validate } = geometryValidator(definitionName);
        assert.equal(validate(payload), true, `${definitionName}: ${ajv.errorsText(validate.errors)}`);
    }
});

test("Geometry analysis AsyncAPI channels document the exact GeometryService topic routes", () => {
    const asyncApiPath = path.join(root, "asyncapi", "asyncapi.yaml");
    const document = YAML.parse(fs.readFileSync(asyncApiPath, "utf8"));
    const expectedBinding = {
        is: "routingKey",
        exchange: {
            name: "maliev.events",
            type: "topic",
            durable: true,
            autoDelete: false,
            vhost: "/"
        },
        bindingVersion: "0.3.0"
    };
    const expectedChannels = {
        "geometry/file-metrics-ready": {
            address: "maliev.geometryservice.v1.metrics.ready",
            message: "FileMetricsReadyEvent"
        },
        "geometry/file-analysis-failed": {
            address: "maliev.geometryservice.v1.analysis.failed",
            message: "FileAnalysisFailedEvent"
        },
        "geometry/dfm-analysis-ready": {
            address: "maliev.geometryservice.v1.dfm.ready",
            message: "DfmAnalysisReadyEvent"
        }
    };

    for (const [channelName, expected] of Object.entries(expectedChannels)) {
        const channel = document.channels[channelName];
        assert.ok(channel, `Missing AsyncAPI channel ${channelName}`);
        assert.equal(channel.address, expected.address);
        assert.deepEqual(channel.bindings.amqp, expectedBinding);
        assert.deepEqual(channel.messages[expected.message], {
            $ref: `#/components/messages/${expected.message}`
        });
        assert.deepEqual(document.components.messages[expected.message].payload, {
            $ref: `../contracts/schemas/geometry/geometry-events.json#/definitions/${expected.message}`
        });
    }
});
