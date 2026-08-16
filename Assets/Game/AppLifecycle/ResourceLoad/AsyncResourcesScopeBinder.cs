using System.Collections.Generic;
using VContainer;

public class AsyncResourcesScopeBinder : IResourceBinder
{
    private readonly bool _isEmpty;
    private readonly IReadOnlyList<IResourceBinder> _bindings;

    public AsyncResourcesScopeBinder()
    {
        _bindings = null;
        _isEmpty = true;
    }

    public AsyncResourcesScopeBinder(IReadOnlyList<IResourceBinder> bindings)
    {
        _bindings = bindings;
        var bindingsCount = _bindings.Count;
        if (bindingsCount == 0)
        {
            _isEmpty = true;
            return;
        }

        var emptyCellsCount = 0;
        foreach (var binding in _bindings)
        {
            if (binding == null)
            {
                emptyCellsCount++;
                //UnityEngine.Debug.LogError("null binding");
            }
        }
        _isEmpty = emptyCellsCount == bindingsCount;
    }

    public void Register(IContainerBuilder builder)
    {
        if (_isEmpty)
            return;

        foreach (var binding in _bindings)
        {
            binding?.Register(builder);
        }
    }

}
