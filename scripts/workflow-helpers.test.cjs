const assert = require('node:assert/strict');
const { spawnSync } = require('node:child_process');
const {
  mkdirSync,
  mkdtempSync,
  rmSync,
  writeFileSync,
} = require('node:fs');
const os = require('node:os');
const path = require('node:path');
const test = require('node:test');

const {
  normalizePackageVersion,
  resolvePackageVersion,
} = require('./resolve-package-version.cjs');
const { verifyGeneratedClean } = require('./verify-generated-clean.cjs');

const versionScript = path.join(__dirname, 'resolve-package-version.cjs');

test('normalizePackageVersion accepts and normalizes supported release forms', () => {
  for (const input of ['release/v1.2.3', 'v1.2.3', '1.2.3']) {
    assert.equal(normalizePackageVersion(input), '1.2.3');
  }

  assert.equal(normalizePackageVersion('release/v1.2.3-alpha.1+build.5'), '1.2.3-alpha.1+build.5');
  assert.equal(normalizePackageVersion('v1.2.3+01'), '1.2.3+01');
});

test('resolvePackageVersion derives branch versions only for supported push refs', () => {
  assert.equal(resolvePackageVersion({ eventName: 'push', refName: 'main', runNumber: '42' }), '1.0.42');
  assert.equal(resolvePackageVersion({ eventName: 'push', refName: 'staging', runNumber: '42' }), '1.0.42-beta');
  assert.equal(resolvePackageVersion({ eventName: 'push', refName: 'develop', runNumber: '42' }), '1.0.42-alpha');
  assert.throws(
    () => resolvePackageVersion({ eventName: 'push', refName: 'feature/unsafe', runNumber: '42' }),
    /Unsupported push ref/);
});

test('resolvePackageVersion normalizes all supported release-event tag forms', () => {
  for (const releaseTag of ['release/v1.2.3', 'v1.2.3', '1.2.3']) {
    assert.equal(
      resolvePackageVersion({ eventName: 'release', releaseTag }),
      '1.2.3');
  }
});

test('workflow dispatch derives versions only from authorized channel or release refs', () => {
  assert.equal(
    resolvePackageVersion({ eventName: 'workflow_dispatch', refName: 'release/v1.2.3', runNumber: '42' }),
    '1.2.3');
  assert.equal(
    resolvePackageVersion({ eventName: 'workflow_dispatch', refName: 'develop', runNumber: '42' }),
    '1.0.42-alpha');
});

test('workflow dispatch rejects unsupported refs and explicit-version escape hatches', () => {
  for (const refName of ['feature/unsafe', 'v1.2.3', '1.2.3']) {
    assert.throws(
      () => resolvePackageVersion({ eventName: 'workflow_dispatch', refName, runNumber: '42' }),
      /Unsupported workflow_dispatch ref/);
  }

  assert.throws(
    () => resolvePackageVersion({
      eventName: 'workflow_dispatch',
      refName: 'feature/manual',
      requestedVersion: 'v1.2.3',
      runNumber: '42',
    }),
    /Explicit package versions are not supported/);
});

test('version resolution rejects invalid or malicious values', () => {
  for (const value of [
    '',
    ' 1.2.3',
    '1.2.3 ',
    '1.2.3\nINJECTED=1',
    '1.2.3;echo compromised',
    '$(echo compromised)',
    'release/1.2.3',
    'release/v1.2',
    'release/v1.2.3/../../unsafe',
    '01.2.3',
    '1.2.3-01',
    'v1.2.3-alpha.01',
  ]) {
    assert.throws(() => normalizePackageVersion(value), /Invalid package version/);
  }

  assert.throws(
    () => resolvePackageVersion({ eventName: 'push', refName: 'main', runNumber: '4;2' }),
    /Invalid run number/);
});

test('version CLI writes only the normalized version and exits nonzero on invalid input', () => {
  const valid = spawnSync(process.execPath, [versionScript], {
    encoding: 'utf8',
    env: {
      ...process.env,
      EVENT_NAME: 'release',
      REF_NAME: '',
      RELEASE_TAG: 'release/v1.2.3',
      REQUESTED_VERSION: '',
      RUN_NUMBER: '42',
    },
  });
  assert.equal(valid.status, 0, valid.stderr);
  assert.equal(valid.stdout, '1.2.3\n');

  const invalid = spawnSync(process.execPath, [versionScript], {
    encoding: 'utf8',
    env: {
      ...process.env,
      EVENT_NAME: 'workflow_dispatch',
      REF_NAME: 'feature/unsafe',
      RELEASE_TAG: '',
      REQUESTED_VERSION: '',
      RUN_NUMBER: '42',
    },
  });
  assert.notEqual(invalid.status, 0);
  assert.match(invalid.stderr, /Unsupported workflow_dispatch ref/);
});

test('verifyGeneratedClean accepts a clean generated tree', () => {
  withTemporaryRepository((repositoryRoot) => {
    assert.doesNotThrow(() => verifyGeneratedClean({ cwd: repositoryRoot }));
  });
});

test('verifyGeneratedClean rejects tracked generated changes', () => {
  withTemporaryRepository((repositoryRoot) => {
    writeFileSync(path.join(repositoryRoot, 'generated', 'csharp', 'Tracked.cs'), 'changed\n');

    assert.throws(
      () => verifyGeneratedClean({ cwd: repositoryRoot }),
      /Tracked generated C# differs from HEAD/);
  });
});

test('verifyGeneratedClean rejects untracked generated files', () => {
  withTemporaryRepository((repositoryRoot) => {
    writeFileSync(path.join(repositoryRoot, 'generated', 'csharp', 'Untracked.cs'), 'untracked\n');

    assert.throws(
      () => verifyGeneratedClean({ cwd: repositoryRoot }),
      (error) => {
        assert.match(error.message, /Generated C# working tree is not clean/);
        assert.match(error.message, /Untracked\.cs/);
        return true;
      });
  });
});

function withTemporaryRepository(callback) {
  const repositoryRoot = mkdtempSync(path.join(os.tmpdir(), 'messaging-contracts-workflow-test-'));
  const generatedRoot = path.join(repositoryRoot, 'generated', 'csharp');

  try {
    mkdirSync(generatedRoot, { recursive: true });
    writeFileSync(path.join(generatedRoot, 'Tracked.cs'), 'tracked\n');
    runGit(repositoryRoot, 'init');
    runGit(repositoryRoot, 'config', 'user.email', 'workflow-test@example.invalid');
    runGit(repositoryRoot, 'config', 'user.name', 'Workflow Test');
    runGit(repositoryRoot, 'add', 'generated/csharp/Tracked.cs');
    runGit(repositoryRoot, 'commit', '-m', 'test fixture');
    callback(repositoryRoot);
  } finally {
    rmSync(repositoryRoot, { force: true, recursive: true });
  }
}

function runGit(cwd, ...args) {
  const result = spawnSync('git', args, { cwd, encoding: 'utf8' });
  assert.equal(result.status, 0, result.stderr);
}
