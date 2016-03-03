// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe
{
    using XmlLink = BattleScribeXml.Link;

    public class RootLink : EntryLink, IRootLink
    {
        public RootLink(XmlLink xml)
            : base(xml)
        {
            if (XmlBackend.CategoryId != null)
            {
                CategoryLink = new IdLink<ICategory>(
                    XmlBackend.CategoryGuid,
                    x => XmlBackend.CategoryGuid = x,
                    () => XmlBackend.CategoryId);
            }
            else
            {
                // TODO log error event, no category was assigned
                CategoryLink = new IdLink<ICategory>(
                    ReservedIdentifiers.NoCategoryId,
                    x => XmlBackend.CategoryGuid = x,
                    () => CategoryLink.TargetId.Value == ReservedIdentifiers.NoCategoryId
                        ? ReservedIdentifiers.NoCategoryName
                        : XmlBackend.CategoryId);
            }
        }

        public override ICatalogueContext Context
        {
            get { return base.Context; }
            set
            {
                var old = base.Context;
                if (!Set(base.Context, value, () => base.Context = value))
                {
                    return;
                }
                old?.RootLinks.Deregister(this);
                Target = null;
                CategoryLink.Target = null;
                value?.RootLinks.Register(this);
                value?.Entries.SetTargetOf(this);
                value?.Catalogue.SystemContext.Categories.SetTargetOf(CategoryLink);
                Modifiers.ChangeContext(value);
            }
        }

        public IIdLink<ICategory> CategoryLink { get; }
    }
}
