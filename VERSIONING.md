# Semantic Versioning Guide

This repository uses automated semantic versioning based on PR labels. Here's how it works:

## Application build versions

Normal builds derive a semantic version from the latest reachable `vMAJOR.MINOR.PATCH` Git tag. A clean checkout at that tag uses the tag version exactly. Builds with commits or working-tree changes after the tag use the next patch as a development prerelease, for example `0.0.8-dev.0+build.20261030153000`. The UTC build timestamp changes on each build without modifying source files or creating tags.

The .NET SDK stamps the numeric portion into `AssemblyVersion` and `FileVersion`, and the complete semantic version into `AssemblyInformationalVersion`. The About page reads the latter from the running assembly. Git and PowerShell 7 (`pwsh`) are required for ordinary builds. An explicit `-p:Version=1.2.3` overrides the derived version, as the release workflows already do.

Only a release tag establishes a new stable version. Building locally does not publish a release or advance a tag.

## 🏷️ **PR Labels for Version Bumping**

When creating a Pull Request, add **one** of these labels to indicate the type of change:

### Major Version (x.0.0) - Breaking Changes
- `major` - Breaking changes that require major version bump
- `breaking` - Alias for major 

**Examples:** API changes, removing features, changing behavior

### Minor Version (0.x.0) - New Features  
- `minor` - New features that are backward compatible
- `feature` - Alias for minor

**Examples:** Adding new endpoints, new functionality

### Patch Version (0.0.x) - Bug Fixes
- `patch` - Bug fixes and small improvements
- `fix` - Alias for patch  
- `bugfix` - Alias for patch

**Examples:** Bug fixes, security patches, dependency updates

## 🔄 **How It Works**

1. **Create PR** with appropriate label (`major`, `minor`, `patch`, etc.)
2. **Merge PR** to main branch
3. **Automatic versioning** workflow runs:
   - Reads merged PR labels
   - Calculates next semantic version
   - Updates assembly version in build
   - Creates Git tag (e.g., `v1.2.3`)
   - Creates GitHub release
4. **Release workflow** builds and packages the application

## 📋 **Workflow Examples**

```
Current version: v1.2.0

PR with "patch" label    → v1.2.1
PR with "minor" label    → v1.3.0  
PR with "major" label    → v2.0.0
PR with no version label → v1.2.1 (defaults to patch)
```

## 🛠️ **Assembly Versioning**

The version is automatically stamped into your .NET assembly during build with these MSBuild properties:
- `Version` - NuGet package version
- `AssemblyVersion` - .NET assembly version  
- `FileVersion` - File version
- `InformationalVersion` - Product version

You can access the version in your code:
```csharp
var version = Assembly.GetExecutingAssembly()
    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
    ?.InformationalVersion;
```

## 🚀 **Manual Releases**

You can also create manual releases via GitHub Actions:
1. Go to **Actions** → **Release Build**
2. Click **Run workflow**
3. Enter desired version (e.g., `1.5.0`)
4. Choose whether to create a GitHub release

## 📊 **Version History**

View all versions and releases:
- **Tags:** Repository → Tags
- **Releases:** Repository → Releases
- **Workflow runs:** Actions → Semantic Versioning

## ⚡ **Quick Setup**

The required labels are automatically created when you push this workflow. If you need to create them manually, run the **Setup Repository Labels** workflow.
