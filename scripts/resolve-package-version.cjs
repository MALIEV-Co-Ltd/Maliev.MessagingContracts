#!/usr/bin/env node
"use strict";

const PROTECTED_RELEASE_TAG_PATTERN = /^release\/v((?:0|[1-9]\d*)\.(?:0|[1-9]\d*)\.(?:0|[1-9]\d*)(?:-([0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*))?(?:\+[0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*)?)$/;
const RUN_NUMBER_PATTERN = /^[1-9]\d*$/;
const BRANCH_SUFFIXES = new Map([
    ["refs/heads/main", ""],
    ["refs/heads/staging", "-beta"],
    ["refs/heads/develop", "-alpha"]
]);

function normalizeReleaseTag(value) {
    if (typeof value !== "string") {
        throw new Error("Invalid protected release tag: expected a string.");
    }

    const match = PROTECTED_RELEASE_TAG_PATTERN.exec(value);
    const prerelease = match?.[2];
    const hasLeadingZeroNumericIdentifier = prerelease
        ?.split(".")
        .some(identifier => /^\d+$/.test(identifier) && identifier.length > 1 && identifier.startsWith("0"));
    if (match === null || hasLeadingZeroNumericIdentifier) {
        throw new Error(`Invalid protected release tag: ${JSON.stringify(value)}.`);
    }

    return match[1];
}

function versionForBranch(eventName, ref, refType, runNumber) {
    if (refType !== "branch" || !BRANCH_SUFFIXES.has(ref)) {
        throw new Error(`Unsupported ${eventName} ref: ${JSON.stringify(ref)} (${JSON.stringify(refType)}).`);
    }

    if (typeof runNumber !== "string" || !RUN_NUMBER_PATTERN.test(runNumber)) {
        throw new Error(`Invalid run number: ${JSON.stringify(runNumber)}.`);
    }

    return `1.0.${runNumber}${BRANCH_SUFFIXES.get(ref)}`;
}

function versionForDispatch(ref, refType, runNumber) {
    if (refType === "branch" && BRANCH_SUFFIXES.has(ref)) {
        return versionForBranch("workflow_dispatch", ref, refType, runNumber);
    }

    const tagPrefix = "refs/tags/";
    if (refType === "tag" && ref.startsWith(tagPrefix)) {
        const tagName = ref.slice(tagPrefix.length);
        if (PROTECTED_RELEASE_TAG_PATTERN.test(tagName)) {
            return normalizeReleaseTag(tagName);
        }
    }

    throw new Error(
        `Unsupported workflow_dispatch ref: ${JSON.stringify(ref)} (${JSON.stringify(refType)}). ` +
        "Use the main, staging, or develop branch, or a protected release/vX.Y.Z tag."
    );
}

function versionForRelease(ref, refType, releaseTag) {
    if (refType !== "tag" || !ref.startsWith("refs/tags/")) {
        throw new Error(`Unsupported release ref: ${JSON.stringify(ref)} (${JSON.stringify(refType)}).`);
    }

    if (ref !== `refs/tags/${releaseTag}`) {
        throw new Error(
            `Release ref ${JSON.stringify(ref)} does not match release tag ${JSON.stringify(releaseTag)}.`
        );
    }

    return normalizeReleaseTag(releaseTag);
}

function resolvePackageVersion({
    eventName,
    ref = "",
    refType = "",
    releaseTag = "",
    requestedVersion = "",
    runNumber = ""
}) {
    if (requestedVersion !== "") {
        throw new Error(
            "Explicit package versions are not supported; select an authorized branch or protected release/vX.Y.Z tag."
        );
    }

    if (eventName === "release") {
        return versionForRelease(ref, refType, releaseTag);
    }

    if (eventName === "push") {
        return versionForBranch(eventName, ref, refType, runNumber);
    }

    if (eventName === "workflow_dispatch") {
        return versionForDispatch(ref, refType, runNumber);
    }

    throw new Error(`Unsupported publication event: ${JSON.stringify(eventName)}.`);
}

if (require.main === module) {
    try {
        const version = resolvePackageVersion({
            eventName: process.env.EVENT_NAME,
            ref: process.env.REF,
            refType: process.env.REF_TYPE,
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
    normalizeReleaseTag,
    resolvePackageVersion
};
