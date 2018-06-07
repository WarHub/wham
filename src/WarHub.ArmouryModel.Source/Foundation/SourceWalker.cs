using System;
using System.Collections.Generic;
using System.Text;

namespace WarHub.ArmouryModel.Source
{
    public abstract class SourceWalker : SourceVisitor
    {
        public override void DefaultVisit(SourceNode node)
        {
            var childCount = node.ChildrenCount;
            for (var i = 0; i < childCount; i++)
            {
                var child = node.GetChild(i);
                Visit(child);
            }
        }
    }
}
