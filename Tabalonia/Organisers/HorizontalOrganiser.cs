using Avalonia.Layout;

namespace Tabalonia.Organisers;

public sealed class HorizontalOrganiser : StackOrganiser
{
    public HorizontalOrganiser(double itemOffset = 0) : base(Orientation.Horizontal, itemOffset)
    {
    }
}