# Tabalonia

[![Nuget](https://img.shields.io/nuget/v/Tabalonia?label=Tabalonia)](https://www.nuget.org/packages/Tabalonia)

Draggable tab items on Avalonia here!

This is a port of the [Draggablz](https://github.com/ButchersBoy/Dragablz)

![example](https://github.com/egorozh/Tabalonia/blob/master/demo.gif "Example application")

### Getting Started

Install the library as a NuGet package:

```powershell
Install-Package Tabalonia
# Or 'dotnet add package Tabalonia'
```

Done! Use TabsControl like: 
```xml
 <controls:TabsControl ItemsSource="{Binding TabItems}"
                       NewItemFactory="{Binding NewItemFactory}"
                       FixedHeaderCount="3">
        <TabControl.ContentTemplate>
            <DataTemplate DataType="{x:Type dragablzDemo:TabItemViewModel}">
                <Grid>
                    <TextBlock Text="{Binding SimpleContent}"
                               HorizontalAlignment="Center"
                               VerticalAlignment="Center"/>
                </Grid>

            </DataTemplate>
        </TabControl.ContentTemplate>
        <TabControl.ItemTemplate>
            <DataTemplate DataType="{x:Type dragablzDemo:TabItemViewModel}">
                <TextBlock Text="{Binding Header}" />
            </DataTemplate>
        </TabControl.ItemTemplate>
    </controls:TabsControl>
```
