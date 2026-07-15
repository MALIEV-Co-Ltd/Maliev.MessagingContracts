const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const test = require('node:test');
const YAML = require('yaml');

const repositoryRoot = path.resolve(__dirname, '..');
const workflowPath = path.join(repositoryRoot, '.github', 'workflows', 'validate-contracts.yaml');
const globalJsonPath = path.join(repositoryRoot, 'global.json');
const approvedWorkflowSha = '183ddf5d7b841aa3583f7961a21084d2f4e54b23';

function readWorkflow() {
  return YAML.parse(fs.readFileSync(workflowPath, 'utf8'));
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

test('schema-first validation remains local without duplicated .NET gate steps', () => {
  const workflow = readWorkflow();
  const schemaJob = workflow.jobs.validate;
  const steps = schemaJob.steps ?? [];
  const stepNames = steps.map((step) => step.name);

  assert.equal(schemaJob.name, 'Validate schemas and generated contracts');
  assert.deepEqual(schemaJob.permissions, { contents: 'read' });
  assert.ok(Number.isInteger(schemaJob['timeout-minutes']) && schemaJob['timeout-minutes'] > 0);
  assert.ok(stepNames.includes('Setup Node.js'));
  assert.ok(stepNames.includes('Install dependencies'));
  assert.ok(stepNames.includes('Validate AsyncAPI and schemas'));
  assert.ok(stepNames.includes('Detect schema changes'));
  assert.ok(stepNames.includes('Validate consumers'));
  assert.ok(stepNames.includes('Generate contracts'));
  assert.ok(stepNames.includes('Verify generated C# is current'));

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
