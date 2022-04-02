namespace Tabalonia.Comparers;

internal class FuncComparer<TObject> : IComparer<TObject>
{
    private readonly Func<TObject, TObject, int> _comparer;

    public FuncComparer(Func<TObject, TObject, int> comparer)
    {
        _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
    }

    public int Compare(TObject x, TObject y) => _comparer(x, y);
}