﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.ProjectSystem
{
    /// <summary>
    /// This represents contents of the <c>.bsr</c> file.
    /// </summary>
    [Record]
    public partial class RepoDistribution
    {
        public IDatafileInfo<DataIndexNode> Index { get; }

        public ImmutableArray<IDatafileInfo<CatalogueBaseNode>> Datafiles { get; }
    }
}
