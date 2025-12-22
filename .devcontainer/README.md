# Dev Container for Fabric Mapping Service

This directory contains the configuration for a development container that can be used with:
- GitHub Codespaces
- Visual Studio Code with Remote - Containers extension
- Any other container-based development environment

## What's Included

The dev container includes:
- .NET 10.0 SDK
- Azure CLI (for deployment)
- PowerShell 7
- Git
- VS Code extensions for C# development

## Usage

### GitHub Codespaces

1. Click "Code" → "Open with Codespaces" on the GitHub repository
2. Create a new codespace or select an existing one
3. The container will automatically build and configure the environment
4. Start coding!

### Visual Studio Code

1. Install the "Remote - Containers" extension
2. Open the repository in VS Code
3. Click "Reopen in Container" when prompted
4. The container will build and configure automatically

## Configuration

The `devcontainer.json` file configures:
- Base image: .NET 10.0 SDK
- Additional tools: Azure CLI, PowerShell, Git
- VS Code extensions for C# and Azure development
- Port forwarding: 5000 (HTTP), 5001 (HTTPS)
- Post-create command: Restores packages and builds the solution
- Post-start command: Trusts the HTTPS development certificate

## Features

- **Consistent Environment**: Everyone uses the same development environment
- **Quick Setup**: No manual installation of tools and SDKs
- **Isolated**: Container-based, doesn't affect your local machine
- **Integrated**: Works seamlessly with VS Code and GitHub

## Customization

You can customize the dev container by editing `devcontainer.json`:

- Add more VS Code extensions in `customizations.vscode.extensions`
- Add more tools with `features`
- Change port forwarding in `forwardPorts`
- Modify post-creation scripts in `postCreateCommand`

## Requirements

- Docker Desktop (for local development)
- VS Code with Remote - Containers extension (for local development)
- Or a GitHub account (for Codespaces)

## Learn More

- [Dev Containers Documentation](https://containers.dev/)
- [VS Code Remote - Containers](https://code.visualstudio.com/docs/remote/containers)
- [GitHub Codespaces](https://github.com/features/codespaces)
