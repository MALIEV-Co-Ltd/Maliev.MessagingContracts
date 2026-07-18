#!/usr/bin/env node
"use strict";

const fs = require("node:fs");
const path = require("node:path");
const { pathToFileURL } = require("node:url");

const Ajv = require("ajv");
const addFormats = require("ajv-formats");
const asyncapiSpecs = require("@asyncapi/specs");
const YAML = require("yaml");

const root = path.resolve(__dirname, "..");
const args = new Set(process.argv.slice(2));
const runAll = args.size === 0;
const runAsyncApi = runAll || args.has("--asyncapi");
const runSchemas = runAll || args.has("--schemas");

let failureCount = 0;

function createAjv() {
    const ajv = new Ajv({
        allErrors: true,
        meta: false,
        strict: false,
        validateSchema: false
    });
    addFormats(ajv);
    return ajv;
}

function fail(message) {
    failureCount += 1;
    console.error(message);
}

function collectJsonFiles(directory) {
    const entries = fs.readdirSync(directory, { withFileTypes: true });
    const files = [];

    for (const entry of entries) {
        const fullPath = path.join(directory, entry.name);
        if (entry.isDirectory()) {
            files.push(...collectJsonFiles(fullPath));
            continue;
        }

        if (entry.isFile() && entry.name.endsWith(".json")) {
            files.push(fullPath);
        }
    }

    return files.sort((left, right) => left.localeCompare(right));
}

function formatAjvErrors(ajv, errors) {
    return ajv.errorsText(errors, { separator: "\n" });
}

function withoutDialectMarker(schema) {
    const clone = JSON.parse(JSON.stringify(schema));
    stripDialectMarkers(clone);
    return clone;
}

function stripDialectMarkers(value) {
    if (Array.isArray(value)) {
        for (const item of value) {
            stripDialectMarkers(item);
        }
        return;
    }

    if (value && typeof value === "object") {
        delete value.$schema;
        for (const item of Object.values(value)) {
            stripDialectMarkers(item);
        }
    }
}

function validateAsyncApi() {
    const filePath = path.join(root, "asyncapi", "asyncapi.yaml");
    const source = fs.readFileSync(filePath, "utf8");
    const document = YAML.parseDocument(source, { prettyErrors: true });

    if (document.errors.length > 0) {
        fail(`AsyncAPI YAML parse failed:\n${document.errors.map(error => error.message).join("\n")}`);
        return;
    }

    const value = document.toJS();
    const version = value && value.asyncapi;
    const schema = asyncapiSpecs.schemas[version] || asyncapiSpecs.schemasWithoutId[version];

    if (!schema) {
        fail(`No AsyncAPI JSON Schema is available for version '${version}'.`);
        return;
    }

    const ajv = createAjv();
    const validate = ajv.compile(withoutDialectMarker(schema));

    if (!validate(value)) {
        fail(`AsyncAPI validation failed:\n${formatAjvErrors(ajv, validate.errors)}`);
        return;
    }

    console.log(`AsyncAPI ${version} document is valid.`);
}

function validateJsonSchemas() {
    const schemaRoot = path.join(root, "contracts", "schemas");
    const files = collectJsonFiles(schemaRoot);
    const ajv = createAjv();
    const schemas = [];

    for (const filePath of files) {
        try {
            const schema = withoutDialectMarker(JSON.parse(fs.readFileSync(filePath, "utf8")));
            const schemaUri = pathToFileURL(filePath).href;
            ajv.addSchema(schema, schemaUri);
            schemas.push({ filePath, schema });
        }
        catch (error) {
            fail(`JSON Schema parse failed for ${path.relative(root, filePath)}:\n${error.message}`);
        }
    }

    for (const { filePath, schema } of schemas) {
        try {
            ajv.compile(schema);
        }
        catch (error) {
            fail(`JSON Schema validation failed for ${path.relative(root, filePath)}:\n${error.message}`);
        }
    }

    if (failureCount === 0) {
        console.log(`${schemas.length} JSON Schema files are valid.`);
    }
}

if (!runAsyncApi && !runSchemas) {
    fail("Usage: node scripts/validate-contracts.cjs [--asyncapi] [--schemas]");
}

if (runAsyncApi) {
    validateAsyncApi();
}

if (runSchemas) {
    validateJsonSchemas();
}

process.exitCode = failureCount === 0 ? 0 : 1;
