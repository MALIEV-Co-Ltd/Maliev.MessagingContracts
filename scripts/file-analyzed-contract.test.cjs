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

test("FileAnalyzedEvent requires only fields present in the GeometryService v1 core payload", () => {
    const schema = loadGeometrySchema();
    const ajv = new Ajv({ allErrors: true, strict: false });
    addFormats(ajv);
    ajv.addSchema(schema);

    const validate = ajv.compile({
        $ref: "maliev/geometry-events#/definitions/FileAnalyzedEvent"
    });
    const geometryPayload = {
        fileId: "file-123",
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
        glbStoragePath: "processed/file-123.glb",
        viewerStoragePath: "processed/file-123.glb",
        viewerFileExtension: ".glb",
        thumbnailStoragePath: null,
        storagePath: "uploads/file-123.stl",
        bodyCount: 1,
        bodies: []
    };

    assert.equal(validate(geometryPayload), true, ajv.errorsText(validate.errors));
    assert.deepEqual(
        schema.definitions.FileAnalyzedEvent.required,
        ["fileId", "metrics", "processedAt"]
    );
});

test("FileAnalyzedEvent AsyncAPI channel documents the real GeometryService topic route", () => {
    const asyncApiPath = path.join(root, "asyncapi", "asyncapi.yaml");
    const document = YAML.parse(fs.readFileSync(asyncApiPath, "utf8"));
    const channel = document.channels["geometry/file-analyzed"];

    assert.equal(channel.address, "maliev.geometryservice.v1.analysis.completed");
    assert.deepEqual(channel.bindings.amqp, {
        is: "routingKey",
        exchange: {
            name: "maliev.events",
            type: "topic",
            durable: true,
            autoDelete: false,
            vhost: "/"
        },
        bindingVersion: "0.3.0"
    });
});
