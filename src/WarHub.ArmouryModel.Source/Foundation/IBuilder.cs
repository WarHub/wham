namespace WarHub.ArmouryModel.Source
{
    /// <summary>
    /// This object can create an immutable object with
    /// this instance's properties copied into it.
    /// </summary>
    /// <typeparam name="T">Type of the immutable object.</typeparam>
    public interface IBuilder<T>
    {
        /// <summary>
        /// Creates a new instance of the immutable object with this
        /// instance's properties copied into it.
        /// </summary>
        /// <returns>The created immutable object.</returns>
        T ToImmutable();
    }
}
