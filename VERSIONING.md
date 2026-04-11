# Semantic Versioning Guide

This repository uses automated semantic versioning based on PR labels. Here's how it works:

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

### No Version Bump
- `no-version-bump` - Changes that don't affect the application version

**Examples:** Documentation updates, test changes, CI improvements

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