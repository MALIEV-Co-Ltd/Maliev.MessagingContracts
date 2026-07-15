const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const test = require('node:test');
const YAML = require('yaml');

const repositoryRoot = path.resolve(__dirname, '..');
const workflowPath = path.join(repositoryRoot, '.github', 'workflows', 'validate-contracts.yaml');
const publishWorkflowPath = path.join(repositoryRoot, '.github', 'workflows', 'publish.yaml');
const globalJsonPath = path.join(repositoryRoot, 'global.json');
const approvedWorkflowSha = '183ddf5d7b841aa3583f7961a21084d2f4e54b23';
const checkoutAction = 'actions/checkout@df4cb1c069e1874edd31b4311f1884172cec0e10';
const setupNodeAction = 'actions/setup-node@48b55a011bda9f5d6aeb4c2d9c7362e8dae4041e';
const setupDotnetAction = 'actions/setup-dotnet@26b0ec14cb23fa6904739307f278c14f94c95bf1';

function readWorkflow() {
  return YAML.parse(fs.readFileSync(workflowPath, 'utf8'));
}

function findStep(steps, name) {
  const matches = steps.filter((step) => step.name === name);
  assert.equal(matches.length, 1, `expected exactly one '${name}' step`);
  return matches[0];
}

test('validation workflow consumes the approved central gates with bounded inputs', () => {
  const workflow = readWorkflow();
  const dotnetGate = workflow.jobs['dotnet-pr-gate'];
  const codeqlGate = workflow.jobs['codeql-dotnet'];

  assert.ok(dotnetGate, 'dotnet-pr-gate job must exist');
  assert.ok(codeqlGate, 'codeql-dotnet job must exist');
  assert.equal(
    dotnetGate.uses,
    `MALIEV-Co-Ltd/Maliev.Workflows/.github/workflows/dotnet-pr-gate.yml@${approvedWorkflowSha}`,
  );
  assert.equal(
    codeqlGate.uses,
    `MALIEV-Co-Ltd/Maliev.Workflows/.github/workflows/codeql-dotnet.yml@${approvedWorkflowSha}`,
  );
  assert.deepEqual(dotnetGate.permissions, { contents: 'read' });
  assert.deepEqual(codeqlGate.permissions, {
    contents: 'read',
    'security-events': 'write',
  });
  assert.equal(Object.hasOwn(dotnetGate, 'secrets'), false);
  assert.equal(Object.hasOwn(codeqlGate, 'secrets'), false);
  assert.deepEqual(dotnetGate.with, {
    'target-path': 'Maliev.MessagingContracts.slnx',
    'working-directory': '.',
    'dotnet-version': '10.0.302',
    configuration: 'Release',
    'coverage-threshold': 80,
    'artifact-retention-days': 7,
  });
  assert.deepEqual(codeqlGate.with, {
    'target-path': 'Maliev.MessagingContracts.slnx',
    'working-directory': '.',
    'dotnet-version': '10.0.302',
  });
});

test('global.json locks the same exact SDK used by both central gates', () => {
  const workflow = readWorkflow();
  const globalJson = JSON.parse(fs.readFileSync(globalJsonPath, 'utf8'));

  assert.deepEqual(globalJson, {
    sdk: {
      version: '10.0.302',
      rollForward: 'disable',
    },
  });
  assert.equal(workflow.jobs['dotnet-pr-gate'].with['dotnet-version'], globalJson.sdk.version);
  assert.equal(workflow.jobs['codeql-dotnet'].with['dotnet-version'], globalJson.sdk.version);
});

test('publish workflow uses the repository SDK lock for every .NET setup', () => {
  const publishWorkflow = YAML.parse(fs.readFileSync(publishWorkflowPath, 'utf8'));
  const globalJson = JSON.parse(fs.readFileSync(globalJsonPath, 'utf8'));
  const setupSteps = Object.values(publishWorkflow.jobs)
    .flatMap((job) => job.steps ?? [])
    .filter((step) => typeof step.uses === 'string' && step.uses.startsWith('actions/setup-dotnet@'));

  assert.equal(setupSteps.length, 2);
  for (const step of setupSteps) {
    assert.equal(step.uses, setupDotnetAction);
    assert.deepEqual(step.with, { 'dotnet-version': globalJson.sdk.version });
  }
});

test('schema-first validation remains local without duplicated .NET gate steps', () => {
  const workflow = readWorkflow();
  const schemaJob = workflow.jobs.validate;
  const steps = schemaJob.steps ?? [];

  assert.equal(schemaJob.name, 'Validate schemas and generated contracts');
  assert.deepEqual(schemaJob.permissions, { contents: 'read' });
  assert.ok(Number.isInteger(schemaJob['timeout-minutes']) && schemaJob['timeout-minutes'] > 0);

  const checkout = steps.find((step) => step.uses === checkoutAction);
  assert.ok(checkout, 'exact checkout action must remain present');
  assert.deepEqual(checkout.with, {
    'fetch-depth': 0,
    'persist-credentials': false,
  });

  const setupNode = findStep(steps, 'Setup Node.js');
  assert.equal(setupNode.uses, setupNodeAction);
  assert.deepEqual(setupNode.with, { 'node-version': '24' });
  assert.deepEqual(findStep(steps, 'Install dependencies'), {
    name: 'Install dependencies',
    run: 'npm ci',
  });
  assert.deepEqual(findStep(steps, 'Validate AsyncAPI and schemas'), {
    name: 'Validate AsyncAPI and schemas',
    run: 'npm test',
  });
  assert.deepEqual(findStep(steps, 'Detect schema changes'), {
    name: 'Detect schema changes',
    env: { BASE_REF: '${{ github.base_ref }}' },
    run: `# Only check if base_ref is available (pull requests)
if [ -n "$BASE_REF" ]; then
  git diff --quiet "origin/$BASE_REF" -- contracts/schemas/ || {
    echo "::warning::Schema changes detected. Ensure messageVersion is incremented if this were a production environment."
  }
fi
`,
    shell: 'bash',
  });
  assert.deepEqual(findStep(steps, 'Validate consumers'), {
    name: 'Validate consumers',
    run: './scripts/validate-consumers.ps1',
    shell: 'pwsh',
  });
  assert.deepEqual(findStep(steps, 'Generate contracts'), {
    name: 'Generate contracts',
    run: './scripts/build.ps1',
    shell: 'pwsh',
  });
  assert.deepEqual(findStep(steps, 'Verify generated C# is current'), {
    name: 'Verify generated C# is current',
    run: 'node scripts/verify-generated-clean.cjs',
  });

  const duplicatedDotnetCommands = steps.filter((step) =>
    typeof step.run === 'string' && /(?:^|\s)dotnet\s+(?:restore|build|test)(?:\s|$)/m.test(step.run),
  );
  assert.deepEqual(duplicatedDotnetCommands, []);
  assert.equal(
    steps.some((step) => typeof step.uses === 'string' && step.uses.startsWith('actions/setup-dotnet@')),
    false,
  );
});

test('workflow references are immutable and concurrency cancels stale runs', () => {
  const workflow = readWorkflow();
  const reusableJobs = Object.values(workflow.jobs).filter((job) => typeof job.uses === 'string');

  assert.equal(reusableJobs.length, 2);
  for (const job of reusableJobs) {
    assert.match(job.uses, /@[0-9a-f]{40}$/);
  }
  assert.equal(workflow.concurrency['cancel-in-progress'], true);
  assert.match(workflow.concurrency.group, /github\.workflow/);
});
