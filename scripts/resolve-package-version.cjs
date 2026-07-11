#!/usr/bin/env node
"use strict";

const VERSION_PATTERN = /^(?:release\/v|v)?((?:0|[1-9]\d*)\.(?:0|[1-9]\d*)\.(?:0|[1-9]\d*)(?:-([0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*))?(?:\+[0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*)?)$/;
const RUN_NUMBER_PATTERN = /^[1-9]\d*$/;

function normalizePackageVersion(value) {
    if (typeof value !== "string") {
        throw new Error("Invalid package version: expected a string.");
    }

    const match = VERSION_PATTERN.exec(value);
    const prerelease = match?.[2];
    const hasLeadingZeroNumericIdentifier = prerelease
        ?.split(".")
        .some(identifier => /^\d+$/.test(identifier) && identifier.length > 1 && identifier.startsWith("0"));
    if (match === null || hasLeadingZeroNumericIdentifier) {
        throw new Error(`Invalid package version: ${JSON.stringify(value)}.`);
    }

    return match[1];
}

function versionForBranch(eventName, refName, runNumber) {
    const suffixes = {
        main: "",
        staging: "-beta",
        develop: "-alpha"
    };

    if (Object.hasOwn(suffixes, refName)) {
        if (typeof runNumber !== "string" || !RUN_NUMBER_PATTERN.test(runNumber)) {
            throw new Error(`Invalid run number: ${JSON.stringify(runNumber)}.`);
        }

        return `1.0.${runNumber}${suffixes[refName]}`;
    }

    if (eventName === "workflow_dispatch") {
        try {
            return normalizePackageVersion(refName);
        } catch {
            throw new Error(
                `Unsupported workflow_dispatch ref: ${JSON.stringify(refName)}. ` +
                "Provide an explicit version or use main, staging, develop, release/vX.Y.Z, vX.Y.Z, or X.Y.Z."
            );
        }
    }

    throw new Error(`Unsupported ${eventName} ref: ${JSON.stringify(refName)}.`);
}

function resolvePackageVersion({
    eventName,
    refName = "",
    releaseTag = "",
    requestedVersion = "",
    runNumber = ""
}) {
    if (requestedVersion !== "") {
        if (eventName !== "workflow_dispatch") {
            throw new Error("An explicit package version is supported only for workflow_dispatch.");
        }

        return normalizePackageVersion(requestedVersion);
    }

    if (eventName === "release") {
        return normalizePackageVersion(releaseTag);
    }

    if (eventName === "push" || eventName === "workflow_dispatch") {
        return versionForBranch(eventName, refName, runNumber);
    }

    throw new Error(`Unsupported publication event: ${JSON.stringify(eventName)}.`);
}

if (require.main === module) {
    try {
        const version = resolvePackageVersion({
            eventName: process.env.EVENT_NAME,
            refName: process.env.REF_NAME,
            releaseTag: process.env.RELEASE_TAG,
            requestedVersion: process.env.REQUESTED_VERSION,
            runNumber: process.env.RUN_NUMBER
        });
        console.log(version);
    } catch (error) {
        console.error(error instanceof Error ? error.message : error);
        process.exitCode = 1;
    }
}

module.exports = {
    normalizePackageVersion,
    resolvePackageVersion
};
