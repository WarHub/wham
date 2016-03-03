// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe
{
    using System;
    using BattleScribeXml;
    using ModelBases;

    public class BookIndex : ModelBase, IBookIndex
    {
        private readonly Func<string> _pageGet;
        private readonly Action<string> _pageSet;
        private readonly Func<string> _titleGet;
        private readonly Action<string> _titleSet;

        public BookIndex(Func<string> pageGetter, Action<string> pageSetter,
            Func<string> titleGetter, Action<string> titleSetter)
        {
            _pageGet = pageGetter;
            _pageSet = pageSetter;
            _titleGet = titleGetter;
            _titleSet = titleSetter;
        }

        public BookIndex(IBookIndexed xml)
            : this(
                () => xml.Page, x => xml.Page = x,
                () => xml.Book, x => xml.Book = x)
        {
        }

        public string Page
        {
            get { return _pageGet(); }
            set { Set(_pageGet(), value, _pageSet); }
        }

        public string Title
        {
            get { return _titleGet(); }
            set { Set(_titleGet(), value, _titleSet); }
        }
    }
}
