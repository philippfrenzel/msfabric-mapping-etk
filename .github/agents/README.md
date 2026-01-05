# Multi-Agent Workflow Example Agents

This directory contains example GitHub Copilot agent definitions for setting up a multi-agent workflow.

## Quick Start

1. **Copy these agent files** to your `.github/agents/` directory
2. **Use the architect agent** to analyze complex tasks
3. **Invoke specialized agents** for specific subtasks

## Available Agents

### Architect Agent (`architect.agent.md`)
The main coordinator that:
- Analyzes complex tasks
- Breaks them into subtasks
- Assigns to specialized agents
- Creates execution plans with dependencies

**Usage**: `@architect [your complex task description]`

### Security Expert (`security-expert.agent.md`)
Handles all security-related tasks:
- Security audits
- Vulnerability assessment
- Secure implementation
- OAuth/authentication
- Input validation

**Usage**: `@security-expert [security task]`

### API Expert (`api-expert.agent.md`)
Handles API design and implementation:
- REST API design
- Request/response models
- Error handling
- API documentation
- Endpoint implementation

**Usage**: `@api-expert [API task]`

## Example Workflow

### Step 1: Submit Complex Task to Architect

```
@architect I need to add OAuth 2.0 authentication to our API with:
- Authorization code flow
- Token refresh mechanism
- Secure token storage
- Comprehensive tests
- Full documentation

Create an execution plan.
```

### Step 2: Architect Creates Plan

The architect agent will analyze and respond with:
```markdown
## Execution Plan

1. [@security-expert] Security audit and OAuth configuration
2. [@api-expert] Design OAuth endpoints and DTOs
3. [@security-expert] Implement OAuth flow securely
4. [@api-expert] Update existing API endpoints
5. [@test-specialist] Create OAuth tests
6. [@doc-writer] Update documentation
```

### Step 3: Execute Each Subtask

For each subtask, invoke the appropriate agent:

```
@security-expert Review current auth implementation and recommend 
OAuth configuration with secure token storage approach
```

```
@api-expert Design OAuth endpoints for /authorize, /token, and /refresh 
with proper request/response models and validation
```

And so on...

## Creating Additional Agents

To add more specialized agents, create a new `.agent.md` file:

```markdown
---
name: "Agent Name"
description: "What this agent specializes in"
model: Claude Opus 4.5
---

# Agent Name

[Agent instructions and guidelines]
```

### Suggested Additional Agents

You may want to create agents for:
- **test-specialist**: Test creation and validation
- **doc-writer**: Documentation updates
- **database-expert**: Database schema and queries
- **frontend-expert**: UI/UX implementation
- **performance-expert**: Performance optimization
- **refactoring-expert**: Code refactoring

## Best Practices

1. **Start with the Architect**: Always begin complex tasks with `@architect`
2. **Follow the Plan**: Execute subtasks in the order recommended
3. **Check Dependencies**: Don't start a subtask until its dependencies are complete
4. **Review Each Output**: Verify each agent's work before proceeding
5. **Integrate Regularly**: Combine outputs frequently to catch issues early

## Customization

Feel free to customize these agents for your specific needs:
- Add project-specific guidelines
- Include links to your coding standards
- Reference your project structure
- Add custom validation rules

## See Also

- [GitHub Copilot Multi-Agent Workflow Guide](../../docs/GITHUB_COPILOT_MULTI_AGENT_WORKFLOW.md)
- [Integrating Custom Agents](../../docs/integrating-custom-agents.md)
- [GitHub Copilot Documentation](https://docs.github.com/en/copilot)
