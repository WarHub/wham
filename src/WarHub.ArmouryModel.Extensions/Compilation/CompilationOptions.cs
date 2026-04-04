namespace WarHub.ArmouryModel;

public abstract record CompilationOptions
{
    /// <summary>
    /// When <see langword="true"/>, allows binder to search for entry reference targets
    /// in nested entries - entries deeper than only shared/root level.
    /// Defaults to <see langword="true"/>.
    /// </summary>
    public bool BindEntryReferencesToNestedEntries { get; init; } = true;

    /// <summary>
    /// When <see langword="true"/>, enables same-thread reentrancy detection during symbol binding.
    /// If a symbol's binding triggers re-entrance to its own <c>BindReferences</c>,
    /// an <see cref="InvalidOperationException"/> is thrown instead of spinning forever.
    /// <para>
    /// This is a diagnostic/testing aid. Enable in tests to catch binding cycles early.
    /// Defaults to <see langword="false"/> (production behavior: SpinWait as in Roslyn).
    /// </para>
    /// </summary>
    public bool DetectBindingReentrancy { get; init; }
}
