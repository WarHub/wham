// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.EntryTreeTests.TestHelpers
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    public class ReadonlyNode<T, TArg> : ObservableList<T>, INode<T, TArg> where T : INotifyPropertyChanged
    {
        public ReadonlyNode()
        {
        }

        public ReadonlyNode(IEnumerable<T> collection) : base(collection)
        {
        }

        public T AddNew(TArg arg)
        {
            throw new NotSupportedException("It's a test substitute.");
        }

        public T InsertNew(int index, TArg arg)
        {
            throw new NotSupportedException("It's a test substitute.");
        }
    }
}
