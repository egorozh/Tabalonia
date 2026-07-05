# Tabalonia

Avalonia library: tab control with draggable, detachable and reattachable tabs. Port of [Dragablz](https://github.com/ButchersBoy/Dragablz) (WPF). Published to NuGet as `Tabalonia`.

## Structure

- `Tabalonia/` — the library (the NuGet package)
  - `Controls/TabsControl.cs` — main control, inherits `TabControl`; drag, detach/reattach, add/close logic
  - `Controls/DragTabItem.cs` — tab container; `Controls/LeftPressedThumb.cs` — drag thumb primitive
  - `Panels/TabsPanel.cs`, `Panels/TopPanel.cs` — tab strip layout math
  - `Themes/Custom/` and `Themes/Fluent/` — two shipped themes
  - `GlobalUsings.cs` — Avalonia namespaces are globally imported; don't add `using Avalonia.*` in library files
- `Tabalonia.Demo/` — demo app (CommunityToolkit.Mvvm)
- `Tabalonia.Tests/` — xunit v3 + `Avalonia.Headless.XUnit` (use `[AvaloniaFact]`, not `[Fact]`)

## Commands

```bash
dotnet build Tabalonia.sln
dotnet test Tabalonia.Tests/Tabalonia.Tests.csproj
dotnet run --project Tabalonia.Demo   # visual check of drag/detach behavior
```

## Rules

- **Both themes must stay in sync**: any change to template parts (`PART_*` names, template structure) in `Themes/Custom/*.axaml` must be mirrored in `Themes/Fluent/*.axaml` and vice versa. `OnApplyTemplate` in `TabsControl` requires `PART_TopPanel`, `PART_LeftDragWindowThumb`, `PART_RightDragWindowThumb`.
- Shared build settings (LangVersion, Nullable, analyzers, `$(AvaloniaVersion)`) live in `Directory.Build.props` — don't duplicate them in csproj files.
- The library keeps the **lowest** supported Avalonia version (`12.0.0`) as its dependency; only Demo/Tests reference `$(AvaloniaVersion)`. Don't bump the library's Avalonia reference without a reason — it raises the floor for consumers.
- Library is AOT-compatible (`IsAotCompatible`) — avoid reflection-based APIs.
- Drag/detach math in `TabsPanel`/`TabsControl` is not covered by headless tests — verify interactively via the Demo app when touching it.
- Commits: conventional commits (`feat:`, `fix:`, `build:`, `docs:`, `test:`). Work happens on `develop`; `main` is the release branch.
- Releases: bump `<Version>` in `Tabalonia/Tabalonia.csproj`, update `CHANGELOG.md`, tag `v<version>` — CI publishes to NuGet.
