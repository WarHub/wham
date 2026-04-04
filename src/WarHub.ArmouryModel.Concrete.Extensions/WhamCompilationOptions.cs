namespace WarHub.ArmouryModel.Concrete;

public record WhamCompilationOptions : CompilationOptions
{
    /// <summary>
    /// When <see langword="true"/>, enables reentrancy detection during symbol binding.
    /// If a symbol's binding triggers re-entrance to its own <c>BindReferences</c>,
    /// an <see cref="InvalidOperationException"/> is thrown instead of spinning forever.
    /// <para>
    /// This is a diagnostic/testing aid. Enable in tests to catch binding cycles early.
    /// Defaults to <see langword="false"/> (production behavior: SpinWait as in Roslyn).
    /// </para>
    /// </summary>
    public bool DetectBindingReentrancy { get; init; }
}
