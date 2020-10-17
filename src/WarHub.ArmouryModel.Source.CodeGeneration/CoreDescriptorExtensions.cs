using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal static class CoreDescriptorExtensions
    {
        public static IEnumerable<T> Select<T>(
            this IEnumerable<CoreChildBase> entries,
            Func<CoreValueChild, T> simpleSelect,
            Func<CoreObjectChild, T> complexSelect,
            Func<CoreListChild, T> collectionSelect)
        {
            foreach (var entry in entries)
            {
                if (entry is CoreValueChild simple)
                {
                    yield return simpleSelect(simple);
                }
                else if (entry is CoreObjectChild complex)
                {
                    yield return complexSelect(complex);
                }
                else if (entry is CoreListChild collection)
                {
                    yield return collectionSelect(collection);
                }
            }
        }

        public static string GetNodeTypeName(this CoreDescriptor descriptor)
        {
            return descriptor.CoreTypeIdentifier.ValueText.GetNodeTypeNameCore();
        }

        public static string GetListNodeTypeName(this CoreDescriptor descriptor)
        {
            return descriptor.CoreTypeIdentifier.ValueText.GetListNodeTypeNameCore();
        }

        public static IdentifierNameSyntax GetNodeTypeIdentifierName(this CoreDescriptor descriptor)
        {
            return IdentifierName(descriptor.GetNodeTypeName());
        }

        public static IdentifierNameSyntax GetListNodeTypeIdentifierName(this CoreDescriptor descriptor)
        {
            return IdentifierName(descriptor.GetListNodeTypeName());
        }

        public static IdentifierNameSyntax GetNodeTypeIdentifierName(this CoreObjectChild entry)
        {
            return IdentifierName(entry.Type.ToString().GetNodeTypeNameCore());
        }

        public static IdentifierNameSyntax GetNodeTypeIdentifierName(this CoreListChild entry)
        {
            return IdentifierName(entry.CollectionTypeParameter.ToString().GetNodeTypeNameCore());
        }

        public static IdentifierNameSyntax GetListNodeTypeIdentifierName(this CoreListChild entry)
        {
            return IdentifierName(entry.CollectionTypeParameter.ToString().GetListNodeTypeNameCore());
        }

        public static string GetNodeTypeNameCore(this string typeName)
        {
            return typeName.StripSuffixes() + Names.NodeSuffix;
        }

        public static string GetListNodeTypeNameCore(this string typeName)
        {
            return typeName.StripSuffixes() + Names.ListNodeSuffix;
        }

        public static string StripSuffixes(this string typeName)
        {
            return typeName.StripSuffix("?").StripSuffix(Names.CoreSuffix).StripSuffix(Names.NodeSuffix);
        }

        private static string StripSuffix(this string text, string suffix)
        {
            return text.EndsWith(suffix, StringComparison.Ordinal) ? text.Substring(0, text.Length - suffix.Length) : text;
        }

        public static TypeSyntax ToListOfBuilderType(this CoreListChild entry)
        {
            return
                GenericName(Names.ListGeneric)
                .AddTypeArgumentListArguments(
                    QualifiedName(
                        entry.CollectionTypeParameter,
                        IdentifierName(Names.Builder)));
        }

        public static TypeSyntax ToIEnumerableType(this TypeSyntax typeArgument)
        {
            return
                GenericName(Names.IEnumerable)
                .AddTypeArgumentListArguments(typeArgument);
        }

        public static TypeSyntax ToNodeListType(this TypeSyntax typeArgument)
        {
            return
                GenericName(Names.NodeList)
                .AddTypeArgumentListArguments(typeArgument);
        }

        public static string ToLowerFirstLetter(this string name)
        {
            return string.IsNullOrEmpty(name)
                ? name
                : $"{char.ToLowerInvariant(name[0])}{name.Substring(1)}";
        }
    }
}
