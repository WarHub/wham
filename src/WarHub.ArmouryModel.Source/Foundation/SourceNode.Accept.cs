using System;
using System.Collections.Generic;
using System.Text;

namespace WarHub.ArmouryModel.Source
{
    partial class SourceNode
    {

        public abstract void Accept(SourceVisitor visitor);

        public abstract TResult Accept<TResult>(SourceVisitor<TResult> visitor);
    }
}
