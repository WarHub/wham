// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.EntryTreeTests.TestHelpers
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    public class ReadonlyNodeSimple<T> : ObservableList<T>, INodeSimple<T> where T : INotifyPropertyChanged
    {
        public ReadonlyNodeSimple()
        {
        }

        public ReadonlyNodeSimple(IEnumerable<T> collection) : base(collection)
        {
        }

        public T AddNew()
        {
            throw new NotSupportedException("It's a test substitute.");
        }

        public T InsertNew(int index)
        {
            throw new NotSupportedException("It's a test substitute.");
        }
    }
}
