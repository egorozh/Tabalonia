using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.LogicalTree;
using Avalonia.Reactive;

namespace Tabalonia.Controls;

public class DragTabItemContainerGenerator : ItemContainerGenerator<DragTabItem>
{
    public DragTabItemContainerGenerator(TabControl owner)
        : base(owner, ContentControl.ContentProperty, ContentControl.ContentTemplateProperty)
    {
        Owner = owner;
    }   

    public new TabControl Owner { get; }

    protected override IControl CreateContainer(object item)
    {
        var tabItem = (DragTabItem)base.CreateContainer(item);

        var source = new OwnerBinding<Dock>(tabItem, TabControl.TabStripPlacementProperty);
        tabItem.Bind(TabItem.TabStripPlacementProperty, source);

        if (tabItem.HeaderTemplate == null)
        {
            tabItem.Bind(HeaderedContentControl.HeaderTemplateProperty, new OwnerBinding<IDataTemplate>(
                tabItem, ItemsControl.ItemTemplateProperty));
        }

        if (tabItem.Header == null)
        {
            if (item is IHeadered headered)
            {
                tabItem.Header = headered.Header;
            }
            else
            {
                if (tabItem.DataContext is not IControl)
                {
                    tabItem.Header = tabItem.DataContext;
                }
            }
        }

        if (tabItem.ContextMenu == null)
        {
            if (item is Control control)
            {
                tabItem.ContextMenu = control.ContextMenu;
            }
        }

        if (tabItem.Content is not IControl)
        {
            tabItem.Bind(ContentControl.ContentTemplateProperty, new OwnerBinding<IDataTemplate>(
                tabItem,
                TabControl.ContentTemplateProperty));
        }

        return tabItem;
    }

    private class OwnerBinding<T> : SingleSubscriberObservableBase<T>
    {
        private readonly DragTabItem _item;
        private readonly StyledProperty<T> _ownerProperty;
        private IDisposable? _ownerSubscription;
        private IDisposable? _propertySubscription;

        public OwnerBinding(DragTabItem item, StyledProperty<T> ownerProperty)
        {
            _item = item;
            _ownerProperty = ownerProperty;
        }

        protected override void Subscribed()
        {
            _ownerSubscription = ControlLocator.Track(_item, 0, typeof(TabControl)).Subscribe(OwnerChanged);
        }

        protected override void Unsubscribed()
        {
            _ownerSubscription?.Dispose();
            _ownerSubscription = null;
        }

        private void OwnerChanged(ILogical c)
        {
            _propertySubscription?.Dispose();
            _propertySubscription = null;

            if (c is TabControl tabControl)
            {
                _propertySubscription = tabControl.GetObservable(_ownerProperty)
                    .Subscribe(PublishNext);
            }
        }
    }
}