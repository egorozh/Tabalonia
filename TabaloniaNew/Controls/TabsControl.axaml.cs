using Avalonia.Controls;
using Avalonia.Controls.Templates;
using TabaloniaNew.Panels;


namespace TabaloniaNew.Controls
{
    public class TabsControl : TabControl
    {
        /// <summary>
        /// The default value for the <see cref="ItemsControl.ItemsPanel"/> property.
        /// </summary>
        private static readonly FuncTemplate<IPanel> DefaultPanel = new(() => new TabsPanel());

        /// <summary>
        /// Initializes static members of the <see cref="TabControl"/> class.
        /// </summary>
        static TabsControl()
        {
            ItemsPanelProperty.OverrideDefaultValue<TabsControl>(DefaultPanel);
        }
    }
}