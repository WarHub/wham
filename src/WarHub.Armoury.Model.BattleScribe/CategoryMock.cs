// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe
{
    using ModelBases;
    using Nodes;

    public class CategoryMock : IdentifiedNamedModelBase<BattleScribeXml.CategoryMock>, ICategoryMock
    {
        private readonly IdLink<ICategory> _categoryLink;
        private readonly SelectionNode _selectionsNode;
        private IForceContext _forceContext;

        public CategoryMock(BattleScribeXml.CategoryMock xml)
            : base(xml)
        {
            _categoryLink = new IdLink<ICategory>(
                XmlBackend.CategoryGuid,
                newGuid => XmlBackend.CategoryGuid = newGuid,
                () => XmlBackend.CategoryId);
            _selectionsNode = new SelectionNode(() => XmlBackend.Selections, this) {Controller = XmlBackend.Controller};
        }

        public IIdLink<ICategory> CategoryLink
        {
            get { return _categoryLink; }
        }

        public IForceContext ForceContext
        {
            get { return _forceContext; }
            set
            {
                if (!Set(ref _forceContext, value))
                {
                    return;
                }
                if (value != null)
                {
                    value.Roster.SystemContext.Categories.SetTargetOf(CategoryLink);
                }
                else
                {
                    CategoryLink.Target = null;
                }
                Selections.ChangeContext(value);
            }
        }

        public INode<ISelection, CataloguePath> Selections
        {
            get { return _selectionsNode; }
        }
    }
}
