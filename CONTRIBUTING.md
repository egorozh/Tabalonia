# Contributing to Tabalonia

Thanks for your interest in contributing!

## Getting started

```bash
git clone https://github.com/egorozh/Tabalonia.git
cd Tabalonia
dotnet build Tabalonia.sln
dotnet test Tabalonia.Tests/Tabalonia.Tests.csproj
dotnet run --project Tabalonia.Demo
```

Requirements: .NET 10 SDK (the library also targets net8.0).

## Workflow

- Base your branch on **`develop`** and open pull requests against `develop`. `main` is the release branch.
- Use [conventional commits](https://www.conventionalcommits.org/): `feat:`, `fix:`, `build:`, `docs:`, `test:`, `refactor:`.
- Code style is enforced by `.editorconfig` — build with `dotnet build` and keep the build free of new warnings.
- If you change control templates, update **both** themes (`Themes/Custom` and `Themes/Fluent`).
- Add or update tests in `Tabalonia.Tests` when changing behavior. Drag-and-drop behavior that can't be covered headlessly should be verified manually in the Demo app — mention what you checked in the PR description.

## Reporting issues

Use the issue templates. For bugs, include the Tabalonia and Avalonia versions, OS, and a minimal reproduction.
