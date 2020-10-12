namespace WarHub.ArmouryModel.Source
{
    /// <summary>
    /// This immutable object can create a builder object.
    /// </summary>
    /// <typeparam name="T">The immutable type.</typeparam>
    /// <typeparam name="TBuilder">Type of the builder created.</typeparam>
    public interface IBuildable<T, TBuilder> where TBuilder : IBuilder<T>
    {
        /// <summary>
        /// Creates the builder of this instance, with all properties copied over into it.
        /// </summary>
        /// <returns>The created builder instance.</returns>
        TBuilder ToBuilder();
    }
}
