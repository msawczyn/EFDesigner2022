# GitHub Copilot Instructions — EFDesigner2022

## What this project is

**Entity Framework Visual Editor** is a Visual Studio extension (VSIX) that provides a visual DSL designer for generating Entity Framework (EF6 and EFCore) code-first models. Users drag entities, enums, associations, and inheritances onto a design surface; the tool generates C# partial classes, a DbContext, and all EF configuration via T4 templates.

The extension targets **Visual Studio 2022 (17.x) and Visual Studio 2026 (18.x)**. The active development branch targeting VS2026 is `VS2026`.

---

## Solution structure

```
src/
  Dsl/                  Core DSL model — domain classes, shapes, rules, serialization, validation
  DslPackage/           VS integration — package, commands, options, toolwindows, T4 template includes
  Utilities/
	 ParsingModels/      Shared data-transfer types between parsers and the designer
	 EF6Parser/          Console app: parses compiled EF6 assemblies, outputs JSON
	 EFCore5Parser/      Console app: parses compiled EFCore 5 assemblies, outputs JSON
	 EFCore6Parser/      Console app: parses compiled EFCore 6 assemblies, outputs JSON
	 EFCore7Parser/      Console app: parses compiled EFCore 7 assemblies, outputs JSON
	 EFCore8Parser/      Console app: parses compiled EFCore 8 assemblies, outputs JSON
```

---

## Key architectural rules

### Dsl vs DslPackage separation
- **`Dsl`** is purely computational: domain model logic, rules, validation, serialization, parsers. It has **no dependency on any VS UI API**. Keep it that way.
- **`DslPackage`** owns everything that touches Visual Studio: package registration, commands, tool windows, option pages, T4 include paths.
- Never add VS Shell / EnvDTE references to `Dsl`.

### DSL-generated code
- Files under `GeneratedCode/` in both `Dsl` and `DslPackage` are **T4-generated from `.tt` templates**. Do not hand-edit them.
- Hand-written extensions live in `CustomCode/` as `partial class` additions to the generated types.
- `source.extension.vsixmanifest` is generated from `source.extension.tt` — edit the `.tt`, not the manifest.

### Parser architecture
- Each `EF*Parser` project is a self-contained console app (published as a single-file executable).
- They are bundled inside the VSIX under `Parsers/` and launched as child processes by `AssemblyProcessor.cs`.
- The parser binary path is resolved at runtime via `Assembly.GetExecutingAssembly().Location`, so it works regardless of where the VSIX is installed.
- `ParsingModels` is a shared library linked into both the parsers and `Dsl` — keep it free of VS dependencies.

### T4 templates
- User-facing code generation templates are in `DslPackage/TextTemplates/` (`.ttinclude` files).
- They are registered as a T4 include path via `additional.pkgdef` so user projects can write `<#@ include file="EFDesigner.ttinclude" #>` without a full path.
- `EditingOnly/` contains helpers used only at template-authoring time inside VS, not at runtime.

---

## Target frameworks

| Project | Framework |
|---|---|
| Dsl, DslPackage | .NET Framework 4.7.2 |
| ParsingModels | .NET Standard 2.0 / 2.1 |
| EF6Parser | .NET Core 3.1 + net472 |
| EFCore5Parser | .NET 5 |
| EFCore6Parser | .NET 6 |
| EFCore7Parser | .NET 7 |
| EFCore8Parser | .NET 8 |

---

## Code style conventions

- **C# 7.3** in `Dsl` and `DslPackage` (constrained by net472 and DSL tooling).
- Prefer `partial class` extensions over modifying generated code.
- ReSharper suppressions (`// ReSharper disable`) are already in use — follow the same pattern for new suppressions rather than pragma warnings.
- `ThreadHelper.ThrowIfNotOnUIThread()` guards are required for any VS shell API call that must be on the UI thread.
- Async package initialization uses `InitializeAsync` with `CancellationToken` + `IProgress<ServiceProgressData>`.

---

## VS extension specifics

- **Minimum VS version**: 17.0. The manifest `InstallationTarget` version range is `[17.0,19.0)`.
- **`AllUsers="false"`** in `source.extension.vsixmanifest` — required for the experimental instance debug deployment to work in VS2026.
- **`DeployExtension=True`** in the Debug build config deploys to the VS experimental instance (`RootSuffix=Exp`) for F5 debugging.
- The `DeployVsixExtensionFiles` MSBuild target is overridden in `DslPackage.csproj` for two VS2026 incompatibilities:
  1. `EnableExtension` is called with `FailIfNotInstalled="false"` — VS2026's extension manager API no longer supports the old build-time registration mechanism and would otherwise produce a spurious fatal build error.
  2. `VSIXInstaller.exe /quiet /rootSuffix:Exp` is called after deployment — VS2026 only auto-loads extensions installed into randomly-named directories by VSIXInstaller (e.g. `Extensions\y3w1v3po.dit\`); the old `Publisher\Name\Version\` deployment path from the `Unzip` step is silently ignored at runtime.
- `additional.pkgdef` registers the T4 include folder and `.efmodel` file icon — entries use `$RootKey$` (HKCU for per-user installs) and `$PackageFolder$`.

---

## What to avoid

- Do not add `InstallRoot` metadata to any VSIX content item — it requires `AllUsers="true"` which breaks experimental instance deployment in VS2026.
- Do not hand-edit `source.extension.vsixmanifest` — changes are overwritten when `source.extension.tt` is re-run.
- Do not hand-edit any file in `GeneratedCode/` — re-run the parent `.tt` template instead.
- Do not introduce UI dependencies in `Dsl` or `ParsingModels`.
- Do not use `dotnet build` to build the VSIX — VSIX packaging requires MSBuild via the full VS build pipeline (`MSBuildRuntimeType != Core`).
