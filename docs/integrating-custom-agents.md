# Integrating Custom Agents with Sequential Workflow Stages

This guide explains how to set up GitHub Copilot custom agents from `github/awesome-copilot` to work with individual workflow stages.

## Understanding the Relationship

The **sequential multi-agent workflow** (GitHub Actions) is separate from **GitHub Copilot custom agents** (VS Code/IDE agents). However, you can use both together:

- **GitHub Actions workflow stages** = Automated CI/CD pipeline steps
- **GitHub Copilot agents** = IDE assistants that help developers write code for each stage

## Recommended Agent Mapping

Here's how to map agents from `github/awesome-copilot` to each workflow stage:

### Stage 1: Azure Architect
**Recommended Agents:**
- `azure-principal-architect.agent.md` - Azure architecture guidance
- `se-system-architecture-reviewer.agent.md` - System architecture review

**Installation:**
```bash
# Download the agent file
curl -o .github/agents/azure-architect.agent.md \
  https://raw.githubusercontent.com/github/awesome-copilot/main/agents/azure-principal-architect.agent.md
```

**Usage in VS Code:**
- Open Copilot Chat (`Ctrl+Shift+I` or `Cmd+Shift+I`)
- Type `@azure-architect` to use this agent
- Ask: "Review the architecture of this Fabric workload"

### Stage 2: .NET Senior Developer
**Recommended Agents:**
- `expert-dotnet-software-engineer.agent.md` - Expert .NET guidance
- `CSharpExpert.agent.md` - C# best practices (already exists in your repo!)
- `csharp-dotnet-janitor.agent.md` - Code cleanup and modernization

**Installation:**
```bash
curl -o .github/agents/dotnet-senior.agent.md \
  https://raw.githubusercontent.com/github/awesome-copilot/main/agents/expert-dotnet-software-engineer.agent.md
```

**Usage:**
- `@dotnet-senior` - Build and code quality questions
- `@CSharpExpert` - C# specific questions

### Stage 3: DevOps Engineer
**Recommended Agents:**
- `se-system-architecture-reviewer.agent.md` - CI/CD review
- Custom: Create your own `devops-engineer.agent.md` focused on GitHub Actions

**Create Custom Agent:**
```markdown
---
name: "DevOps Engineer"
description: Expert in CI/CD, GitHub Actions, and deployment automation
model: Claude Opus 4.5
---

You are an expert DevOps engineer specializing in:
- GitHub Actions workflow design
- CI/CD pipeline optimization
- Artifact management and deployment strategies
- Infrastructure as Code (IaC)
- Container orchestration

When asked about workflow stages, provide best practices for:
- YAML syntax and structure
- Caching strategies
- Artifact retention policies
- Environment configuration
- Security in CI/CD
```

**Save to:** `.github/agents/devops-engineer.agent.md`

### Stage 4: Blazor Fluent UI Specialist
**Recommended Agents:**
- `expert-react-frontend-engineer.agent.md` - React guidance (for React + Fluent UI)
- `se-ux-ui-designer.agent.md` - UI/UX design principles

**Installation:**
```bash
curl -o .github/agents/react-frontend.agent.md \
  https://raw.githubusercontent.com/github/awesome-copilot/main/agents/expert-react-frontend-engineer.agent.md
```

**Usage:**
- `@react-frontend` - React + Fluent UI questions
- Ask: "Help me build a Fluent UI component for this feature"

### Stage 5: Test Specialist
**Recommended Agents:**
- `playwright-tester.agent.md` - E2E testing with Playwright
- Custom: Create `integration-test-specialist.agent.md`

**Create Custom Agent:**
```markdown
---
name: "Integration Test Specialist"
description: Expert in integration and E2E testing strategies
model: Claude Opus 4.5
---

You are an expert test engineer specializing in:
- Integration testing patterns
- E2E test design with Playwright/Selenium
- API testing
- Test data management
- Test coverage analysis

When helping with tests:
- Follow AAA pattern (Arrange, Act, Assert)
- Write maintainable, readable test code
- Consider edge cases and error scenarios
- Use appropriate test fixtures and mocks
```

**Save to:** `.github/agents/test-specialist.agent.md`

### Stage 6: Unit Test Specialist
**Recommended Agents:**
- `expert-dotnet-software-engineer.agent.md` - Includes xUnit guidance
- `diffblue-cover.agent.md` - Automated unit test generation (Java, but concepts apply)

**Create Custom Agent:**
```markdown
---
name: "Unit Test Specialist"
description: Expert in unit testing with xUnit and code coverage
model: Claude Opus 4.5
---

You are a unit testing expert specializing in:
- xUnit test framework
- Code coverage with Coverlet
- Test-driven development (TDD)
- Mocking with Moq/NSubstitute
- Assertion best practices

When writing tests:
- One behavior per test method
- Clear test names describing the scenario
- Arrange-Act-Assert pattern
- Appropriate use of mocks and stubs
- Aim for high coverage of critical paths
```

**Save to:** `.github/agents/unit-test-specialist.agent.md`

### Stage 7: Orchestrator
**Recommended Agents:**
- `planner.agent.md` - High-level planning and coordination
- `se-system-architecture-reviewer.agent.md` - Overall review

**Installation:**
```bash
curl -o .github/agents/orchestrator.agent.md \
  https://raw.githubusercontent.com/github/awesome-copilot/main/agents/planner.agent.md
```

## Setting Up Agents in Your Repository

### 1. Create Agents Directory (Already Exists!)
Your repo already has `.github/agents/` with `my-agent.agent.md`.

### 2. Download Agents from awesome-copilot
```bash
# Navigate to your repo
cd /home/runner/work/msfabric-mapping-etk/msfabric-mapping-etk

# Download relevant agents
curl -o .github/agents/azure-architect.agent.md \
  https://raw.githubusercontent.com/github/awesome-copilot/main/agents/azure-principal-architect.agent.md

curl -o .github/agents/dotnet-senior.agent.md \
  https://raw.githubusercontent.com/github/awesome-copilot/main/agents/expert-dotnet-software-engineer.agent.md

curl -o .github/agents/react-frontend.agent.md \
  https://raw.githubusercontent.com/github/awesome-copilot/main/agents/expert-react-frontend-engineer.agent.md

curl -o .github/agents/orchestrator.agent.md \
  https://raw.githubusercontent.com/github/awesome-copilot/main/agents/planner.agent.md
```

### 3. Activate Agents in VS Code

**Option A: Via VS Code Chat UI**
1. Open GitHub Copilot Chat in VS Code
2. Type `@` to see available agents
3. Agents from `.github/agents/` will be automatically detected

**Option B: Via Install Links**
1. Visit: https://github.com/github/awesome-copilot/blob/main/docs/README.agents.md
2. Click "Install in VS Code" button for desired agents
3. Agent will be added to your workspace

### 4. Using Agents During Development

**Example Workflow:**

```bash
# Working on Stage 2 (.NET Senior Developer)
# In VS Code Copilot Chat:

@dotnet-senior How should I structure this .NET build workflow?
@CSharpExpert Review this C# code for best practices
@dotnet-senior What analyzers should I enable?
```

```bash
# Working on Stage 4 (Blazor/React Frontend)
# In VS Code Copilot Chat:

@react-frontend How do I build this Fluent UI component?
@react-frontend Best practices for React + Fluent UI?
@react-frontend Help me optimize this component's performance
```

```bash
# Working on Stage 6 (Unit Tests)
# In VS Code Copilot Chat:

@unit-test-specialist Write xUnit tests for this service
@unit-test-specialist How do I mock this dependency?
@unit-test-specialist Improve test coverage for this class
```

## Agent Usage in GitHub Actions Workflows

**Important:** GitHub Actions workflows run on GitHub runners, **not** in your local IDE. Copilot agents are IDE tools for developers, not workflow automation tools.

However, you can:

1. **Use agents during development** to write better workflow code
2. **Reference agent expertise** in workflow documentation
3. **Create workflow jobs that mimic agent behavior** (automated checks)

### Example: Using Agent Knowledge in Workflow

Instead of directly using an agent in GitHub Actions, encode the agent's knowledge into the workflow:

```yaml
# .github/workflows/reusable-dotnet-senior-developer.yml
- name: Run code analyzers (following .NET Senior Dev agent guidance)
  run: |
    # Based on expert-dotnet-software-engineer.agent.md recommendations:
    dotnet build --no-restore \
      /p:TreatWarningsAsErrors=true \
      /p:WarningLevel=4 \
      /p:EnforceCodeStyleInBuild=true
```

## Creating Custom Agents for Your Workflow

### Template for Custom Agent

Create `.github/agents/YOUR-STAGE.agent.md`:

```markdown
---
name: "Your Stage Name"
description: Brief description of agent's expertise
model: Claude Opus 4.5
# version: 2026-01-04
---

# Your Stage Name Agent

You are an expert in [domain] specializing in:
- Area 1
- Area 2
- Area 3

## Responsibilities
When working on this stage, you:
1. Responsibility 1
2. Responsibility 2
3. Responsibility 3

## Guidelines
- Guideline 1
- Guideline 2

## Code Examples
[Provide examples relevant to your stage]

## Best Practices
- Practice 1
- Practice 2

## Common Pitfalls
- Pitfall 1 and how to avoid it
- Pitfall 2 and how to avoid it

## Integration with Sequential Workflow
This agent supports Stage X of the sequential multi-agent workflow:
- Workflow file: `.github/workflows/reusable-YOUR-STAGE.yml`
- Purpose: [Description of what this stage does]
- Key checks: [List of validations this stage performs]
```

## Complete Agent Setup Script

Save this as `scripts/setup-agents.sh`:

```bash
#!/bin/bash
# Setup GitHub Copilot agents for sequential workflow stages

AGENTS_DIR=".github/agents"
AWESOME_COPILOT="https://raw.githubusercontent.com/github/awesome-copilot/main/agents"

echo "Setting up GitHub Copilot agents..."

# Stage 1: Azure Architect
curl -sS -o "$AGENTS_DIR/azure-architect.agent.md" \
  "$AWESOME_COPILOT/azure-principal-architect.agent.md"
echo "✅ Azure Architect agent installed"

# Stage 2: .NET Senior Developer
curl -sS -o "$AGENTS_DIR/dotnet-senior.agent.md" \
  "$AWESOME_COPILOT/expert-dotnet-software-engineer.agent.md"
echo "✅ .NET Senior Developer agent installed"

# Stage 3: DevOps Engineer (custom - create your own)
echo "⚠️  DevOps Engineer: Create custom agent at $AGENTS_DIR/devops-engineer.agent.md"

# Stage 4: React Frontend
curl -sS -o "$AGENTS_DIR/react-frontend.agent.md" \
  "$AWESOME_COPILOT/expert-react-frontend-engineer.agent.md"
echo "✅ React Frontend agent installed"

# Stage 5: Test Specialist (custom - create your own)
echo "⚠️  Test Specialist: Create custom agent at $AGENTS_DIR/test-specialist.agent.md"

# Stage 6: Unit Test Specialist (custom - create your own)
echo "⚠️  Unit Test Specialist: Create custom agent at $AGENTS_DIR/unit-test-specialist.agent.md"

# Stage 7: Orchestrator
curl -sS -o "$AGENTS_DIR/orchestrator.agent.md" \
  "$AWESOME_COPILOT/planner.agent.md"
echo "✅ Orchestrator agent installed"

echo ""
echo "🎉 Agent setup complete!"
echo "Agents are available in: $AGENTS_DIR"
echo ""
echo "To use agents in VS Code:"
echo "  1. Open GitHub Copilot Chat (Ctrl+Shift+I or Cmd+Shift+I)"
echo "  2. Type @ to see available agents"
echo "  3. Select an agent and ask questions"
```

**Make executable and run:**
```bash
chmod +x scripts/setup-agents.sh
./scripts/setup-agents.sh
```

## Summary

- **Workflow stages** = Automated CI/CD steps (GitHub Actions)
- **Copilot agents** = IDE assistants for developers (VS Code)
- **Integration**: Use agents during development to write better code for each stage
- **Installation**: Download agent files from `github/awesome-copilot` to `.github/agents/`
- **Usage**: Type `@agent-name` in Copilot Chat to invoke agents

The workflow runs automatically; agents help developers write code that passes the workflow stages.

## Additional Resources

- [awesome-copilot Repository](https://github.com/github/awesome-copilot)
- [Custom Agents Documentation](https://github.com/github/awesome-copilot/blob/main/docs/README.agents.md)
- [VS Code Copilot Documentation](https://code.visualstudio.com/docs/copilot/overview)
- [Sequential Workflow Documentation](docs/sequential-handoff-workflow.md)
