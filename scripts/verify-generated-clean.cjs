#!/usr/bin/env node
"use strict";

const { spawnSync } = require("node:child_process");

function runGit(cwd, args) {
    const result = spawnSync("git", args, {
        cwd,
        encoding: "utf8"
    });

    if (result.error !== undefined) {
        throw result.error;
    }

    if (result.status === null) {
        throw new Error(`Git did not report an exit code for: git ${args.join(" ")}`);
    }

    return result;
}

function verifyGeneratedClean({ cwd = process.cwd() } = {}) {
    const diff = runGit(cwd, [
        "diff",
        "--exit-code",
        "HEAD",
        "--",
        "generated/csharp"
    ]);
    if (diff.status === 1) {
        throw new Error(
            `Tracked generated C# differs from HEAD.\n${diff.stdout}${diff.stderr}`.trimEnd()
        );
    }
    if (diff.status !== 0) {
        throw new Error(
            `git diff failed with exit code ${diff.status}.\n${diff.stdout}${diff.stderr}`.trimEnd()
        );
    }

    const status = runGit(cwd, [
        "status",
        "--porcelain",
        "--untracked-files=all",
        "--",
        "generated/csharp"
    ]);
    if (status.status !== 0) {
        throw new Error(
            `git status failed with exit code ${status.status}.\n${status.stdout}${status.stderr}`.trimEnd()
        );
    }
    if (status.stdout.length !== 0) {
        throw new Error(
            `Generated C# working tree is not clean.\n${status.stdout}`.trimEnd()
        );
    }
}

if (require.main === module) {
    try {
        verifyGeneratedClean();
        console.log("Generated C# matches HEAD and contains no untracked files.");
    } catch (error) {
        console.error(error instanceof Error ? error.message : error);
        process.exitCode = 1;
    }
}

module.exports = {
    verifyGeneratedClean
};
