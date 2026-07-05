# Tabalonia

[![Nuget](https://img.shields.io/nuget/v/Tabalonia?label=Tabalonia)](https://www.nuget.org/packages/Tabalonia)
[![Build](https://github.com/egorozh/Tabalonia/actions/workflows/build.yml/badge.svg)](https://github.com/egorozh/Tabalonia/actions/workflows/build.yml)

Draggable tab items on Avalonia here!

This is a port of the [Draggablz](https://github.com/ButchersBoy/Dragablz)

![example](https://github.com/egorozh/Tabalonia/blob/main/docs/demo.gif "Example application")

## Features

- Drag tabs to reorder them within the strip (with animations)
- Tear a tab off into its own floating window — just drag it out of the strip
- Drag a tab over another window's strip to dock it there, Chrome-style:
  the tab attaches while you hold the button, keeps following the pointer,
  and detaches again if you drag it away
- Built-in add / close tab buttons
- Pin the first N tabs (`FixedHeaderCount`) so they can't be dragged or closed
- Custom content on both sides of the strip (`LeftContent` / `RightContent`)
- Two shipped themes (Custom with light/dark variants, Fluent), fully restylable via brush resources
- AOT-compatible, no reflection

## Getting Started

Install the library as a NuGet package:

```powershell
Install-Package Tabalonia
# Or 'dotnet add package Tabalonia'
```

Add a theme to your `App.axaml`:

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:custom="clr-namespace:Tabalonia.Themes.Custom;assembly=Tabalonia">
    <Application.Styles>
        <FluentTheme />
        <custom:CustomTheme />
    </Application.Styles>
</Application>
```

Or use the Fluent-styled variant instead of the custom one:

```xml
xmlns:fluent="clr-namespace:Tabalonia.Themes.Fluent;assembly=Tabalonia"
...
<fluent:FluentTheme />
```

Then use `TabsControl` like a regular `TabControl`:

```xml
<Window xmlns:controls="clr-namespace:Tabalonia.Controls;assembly=Tabalonia">

    <controls:TabsControl ItemsSource="{Binding TabItems}"
                          NewItemFactory="{Binding NewItemFactory}"
                          FixedHeaderCount="1">
        <TabControl.ItemTemplate>
            <DataTemplate DataType="vm:TabItemViewModel">
                <TextBlock Text="{Binding Header}" />
            </DataTemplate>
        </TabControl.ItemTemplate>

        <TabControl.ContentTemplate>
            <DataTemplate DataType="vm:TabItemViewModel">
                <TextBlock Text="{Binding SimpleContent}" />
            </DataTemplate>
        </TabControl.ContentTemplate>
    </controls:TabsControl>

</Window>
```

> `ItemsSource` must implement `IList` (e.g. `ObservableCollection<T>`) — adding, closing,
> reordering and dragging tabs between windows mutate the collection directly.

## API Reference

`TabsControl` inherits Avalonia's `TabControl`, so everything you know
(`ItemsSource`, `SelectedItem`, `ItemTemplate`, `ContentTemplate`, …) works as usual.
The additions:

### Tab strip appearance

| Property | Type | Default | Description |
|---|---|---|---|
| `TabItemWidth` | `double` | `140` | Maximum tab width; tabs shrink evenly when the strip runs out of space |
| `AdjacentHeaderItemOffset` | `double` | `0` | Horizontal offset between neighbouring tabs (negative values overlap them) |
| `FixedHeaderCount` | `int` | `0` | Number of leading tabs that are pinned: they can't be dragged, closed or displaced |
| `LeftContent` / `RightContent` | `object?` | `null` | Arbitrary content rendered to the left / right of the tab strip |

### Adding and closing tabs

| Member | Type | Description |
|---|---|---|
| `ShowDefaultAddButton` | `bool`, default `true` | Shows the built-in “+” button after the last tab |
| `ShowDefaultCloseButton` | `bool`, default `true` | Shows the “×” button on each tab (except fixed ones) |
| `NewItemFactory` | `Func<object>?` | Creates the model for a new tab when the add button is clicked |
| `NewItemAsyncFactory` | `Func<Task<object>>?` | Async variant; takes precedence over `NewItemFactory` |
| `AddItemCommand` | `ICommand` (read-only) | Adds a tab via the factory — bind it from your own buttons |
| `CloseItemCommand` | `ICommand` (read-only) | Closes the tab passed as the command parameter (`DragTabItem`) |
| `TabClosing` | `EventHandler<TabClosingEventArgs>?` | Raised before a tab closes; set `e.Cancel = true` to keep it open |
| `TabClosed` | `EventHandler<TabClosedEventArgs>?` | Raised after a tab closed; `e.Item` is the removed model |
| `LastTabClosedAction` | `EventHandler<CloseLastTabEventArgs>?` | What to do when the last tab goes away; default closes the window (`e.Window`) |

```csharp
tabs.TabClosing += (_, e) =>
{
    if (e.Item is DocumentViewModel { IsDirty: true })
        e.Cancel = true; // keep the tab open
};

tabs.TabClosed += (_, e) => Console.WriteLine($"Closed: {e.Item}");

// Keep the window open when the last tab is closed (default closes it):
tabs.LastTabClosedAction = (_, e) => { /* e.Window stays open */ };
```

### Tearing off and docking tabs

| Property | Type | Default | Description |
|---|---|---|---|
| `EnableTabDetaching` | `bool` | `true` | Allows dragging a tab out of the strip into a new floating window |
| `EnableTabAttaching` | `bool` | `true` | Allows this strip to give away and accept tabs dragged from other `TabsControl`s |
| `DetachTriggerDistance` | `double` | `32` | How far (px) the pointer must leave the strip before the tab tears off |
| `DetachedWindowFactory` | `Func<TabsControl, Window>?` | `null` | Creates the host window for a detached tab; by default the source window's size, background and decorations are cloned |

The detached window's `TabsControl` inherits the source control's settings
(templates, factories, events, `Margin`, thumb widths, …), so torn-off tabs behave
exactly like the original ones — including docking back.

```csharp
tabsControl.DetachedWindowFactory = tabs => new MyToolWindow { Content = tabs };
```

### Window drag thumbs

The empty areas to the left and right of the tabs act as window-drag handles
(double-click restores/maximizes the window) — useful with
`ExtendClientAreaToDecorationsHint="True"` for Chrome-like windows:

| Property | Default | Description |
|---|---|---|
| `LeftThumbWidth` | `80` on macOS, `4` elsewhere | Width of the drag area before the first tab (macOS default leaves room for traffic lights) |
| `RightThumbWidth` | `160` on Windows, `50` elsewhere | Width of the drag area after the add button |

## Styling

The custom theme supports light and dark variants out of the box and follows
`Application.RequestedThemeVariant`. All colors are exposed as brush resources
(`TabItemBackgroundBrush`, `SelectedTabItemBackgroundBrush`,
`TabControlWindowActiveBackgroundBrush`, `CloseItemButtonForeground`, …) —
override them in your `App.axaml` to restyle the control without retemplating:

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.ThemeDictionaries>
            <ResourceDictionary x:Key="Dark">
                <SolidColorBrush x:Key="SelectedTabItemBackgroundBrush" Color="#1E1E2E" />
            </ResourceDictionary>
        </ResourceDictionary.ThemeDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

The full list of resource keys lives in
[`Themes/Custom/Brushes.axaml`](Tabalonia/Themes/Custom/Brushes.axaml).

## Building from source

```bash
git clone https://github.com/egorozh/Tabalonia.git
cd Tabalonia
dotnet build Tabalonia.sln
dotnet test Tabalonia.Tests/Tabalonia.Tests.csproj
dotnet run --project Tabalonia.Demo
```

See [CONTRIBUTING.md](CONTRIBUTING.md) for the contribution workflow and
[CHANGELOG.md](CHANGELOG.md) for release history.
