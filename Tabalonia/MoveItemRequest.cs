namespace Tabalonia;

public class MoveItemRequest
{
    public MoveItemRequest(object item, object? context, AddLocationHint addLocationHint)
    {
        Item = item;
        Context = context;
        AddLocationHint = addLocationHint;
    }

    public object Item { get; }

    public object? Context { get; }

    public AddLocationHint AddLocationHint { get; }
}