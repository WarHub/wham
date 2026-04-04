using WarHub.ArmouryModel.Concrete;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.RosterEngine.Spec;

/// <summary>
/// Lazily-built lookup from SourceNode instances (ForceNode, SelectionNode)
/// to their corresponding ISymbol instances. Built by walking the compilation's
/// symbol tree. Used by <see cref="StateMapper"/>.
/// </summary>
internal sealed class NodeSymbolLookup
{
    private Dictionary<ForceNode, IForceSymbol>? _forceSymbols;
    private Dictionary<SelectionNode, ISelectionSymbol>? _selectionSymbols;
    private readonly WhamCompilation _compilation;

    public NodeSymbolLookup(WhamCompilation compilation)
    {
        _compilation = compilation;
    }

    public ISelectionSymbol? GetSelection(SelectionNode? node)
    {
        if (node is null) return null;
        EnsureBuilt();
        return _selectionSymbols!.GetValueOrDefault(node);
    }

    public IForceSymbol? GetForce(ForceNode? node)
    {
        if (node is null) return null;
        EnsureBuilt();
        return _forceSymbols!.GetValueOrDefault(node);
    }

    private void EnsureBuilt()
    {
        if (_forceSymbols is not null) return;
        _forceSymbols = new Dictionary<ForceNode, IForceSymbol>();
        _selectionSymbols = new Dictionary<SelectionNode, ISelectionSymbol>();
        foreach (var rosterSym in _compilation.SourceGlobalNamespace.Rosters)
        {
            foreach (var forceSym in rosterSym.Forces)
                IndexForce(forceSym);
        }
    }

    private void IndexForce(ForceSymbol forceSym)
    {
        _forceSymbols![forceSym.Declaration] = forceSym;
        foreach (var selSym in forceSym.ChildSelections)
            IndexSelection(selSym);
        foreach (var childForce in forceSym.Forces)
            IndexForce(childForce);
    }

    private void IndexSelection(SelectionSymbol selSym)
    {
        _selectionSymbols![selSym.Declaration] = selSym;
        foreach (var childSel in selSym.ChildSelections)
            IndexSelection(childSel);
    }
}
