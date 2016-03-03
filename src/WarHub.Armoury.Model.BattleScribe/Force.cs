// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using ModelBases;
    using Nodes;

    public class Force : IdentifiedModelBase<BattleScribeXml.Force>, IForce
    {
        private readonly IdLink<ICatalogue> _catalogueLink;
        private readonly ForceNode _forces;
        private readonly IdLink<IForceType> _forceTypeLink;
        private IRosterContext _context;
        private IForceContext _forceContext;

        public Force(BattleScribeXml.Force xml)
            : base(xml)
        {
            _catalogueLink = new IdLink<ICatalogue>(
                XmlBackend.CatalogueGuid,
                newGuid => XmlBackend.CatalogueGuid = newGuid,
                () => XmlBackend.CatalogueId);
            CategoryMocks = new CategoryMockNode(() => XmlBackend.Categories, this) {Controller = XmlBackend.Controller};
            _forces = new ForceNode(() => XmlBackend.Forces, this) {Controller = XmlBackend.Controller};
            _forceTypeLink = new IdLink<IForceType>(
                XmlBackend.ForceTypeGuid,
                newGuid => XmlBackend.ForceTypeGuid = newGuid,
                () => XmlBackend.ForceTypeId);
            CatalogueLink.PropertyChanged += OnCatalogueChanged;
            CatalogueLink.TargetId.PropertyChanged += OnCatalogueLinkTargetIdPropertyChanged;
            ForceTypeLink.PropertyChanged += OnForceTypeChanged;
            ForceTypeLink.TargetId.PropertyChanged += OnForceTypeLinkTargetIdPropertyChanged;
        }

        internal CategoryMockNode CategoryMocks { get; }

        public IIdLink<ICatalogue> CatalogueLink
        {
            get { return _catalogueLink; }
        }

        public string CatalogueName
        {
            get { return XmlBackend.CatalogueName; }
            private set { Set(XmlBackend.CatalogueName, value, () => XmlBackend.CatalogueName = value); }
        }

        public uint CatalogueRevision
        {
            get { return XmlBackend.CatalogueRevision; }
            private set { Set(XmlBackend.CatalogueRevision, value, () => XmlBackend.CatalogueRevision = value); }
        }

        public IRosterContext Context
        {
            get { return _context; }
            set
            {
                var oldValue = _context;
                if (!Set(ref _context, value))
                {
                    return;
                }
                if (oldValue != null)
                {
                    // deregistration will unsubscribe RosterContext from this.ForceContext's events
                    oldValue.Forces.Deregister(this);
                }
                if (value != null)
                {
                    var systemContext = value.Roster.SystemContext;
                    systemContext.ForceTypes.SetTargetOf(ForceTypeLink);
                    systemContext.Catalogues.SetTargetOf(CatalogueLink);
                    // order of the following instructions is important: On registration,
                    // RosterContext subscribes to this.ForceContext's events
                    ForceContext = new ForceContext(this);
                    value.Forces.Register(this);
                }
                else
                {
                    ForceContext = null;
                }
                Forces.ChangeContext(value);
            }
        }

        public IForceContext ForceContext
        {
            get { return _forceContext; }
            private set
            {
                if (!Set(ref _forceContext, value))
                {
                    return;
                }
                CategoryMocks.ChangeContext(value);
            }
        }

        public INode<IForce, ForceNodeArgument> Forces
        {
            get { return _forces; }
        }

        public IIdLink<IForceType> ForceTypeLink
        {
            get { return _forceTypeLink; }
        }

        public string ForceTypeName
        {
            get { return XmlBackend.ForceTypeName; }
            private set { Set(XmlBackend.ForceTypeName, value, () => XmlBackend.ForceTypeName = value); }
        }

        IEnumerable<ICategoryMock> IForce.CategoryMocks
        {
            get { return CategoryMocks; }
        }

        private void OnCatalogueChanged(object sender, PropertyChangedEventArgs e)
        {
            var catalogue = CatalogueLink.Target;
            if (catalogue != null)
            {
                CatalogueName = catalogue.Name;
                CatalogueRevision = catalogue.Revision;
            }
        }

        private void OnCatalogueLinkTargetIdPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            foreach (var categoryMock in CategoryMocks)
            {
                categoryMock.Selections.Clear();
            }
        }

        private void OnForceTypeChanged(object sender, PropertyChangedEventArgs e)
        {
            var forceType = ForceTypeLink.Target;
            if (forceType != null)
            {
                ForceTypeName = forceType.Name;
            }
        }

        private void OnForceTypeLinkTargetIdPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var noCategory = CategoryMocks.First(x => x.Id.Value == ReservedIdentifiers.NoCategoryId);
            CategoryMocks.Clear();
            noCategory.Selections.Clear();
            CategoryMocks.Add(noCategory);
            var forceType = ForceTypeLink.Target;
            if (forceType == null)
            {
                return;
            }
            foreach (var category in forceType.Categories)
            {
                CategoryMocks.AddNew(category);
            }
        }
    }
}
