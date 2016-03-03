namespace WarHub.Armoury.Model.BattleScribe
{
    using System;
    using ModelBases;

    public class Link : ModelBase, ILink
    {
        public Link(Guid guid, Action<Guid> guidSetter, Func<string> rawGetter)
        {
            TargetId = new Identifier(guid, guidSetter, rawGetter);
        }

        /// <summary>
        ///     The Identifier of referenced object. May be null if link is an empty reference.
        /// </summary>
        public IIdentifier TargetId { get; }
    }
}
