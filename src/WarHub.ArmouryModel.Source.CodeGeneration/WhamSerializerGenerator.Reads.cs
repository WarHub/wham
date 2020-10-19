using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MoreLinq.Extensions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal partial class WhamSerializerGenerator
    {
        private static string ReadRootName(CoreDescriptor core) => "ReadRoot_" + core.Xml.ElementName;

        private static string ReadCoreName(CoreDescriptor core) => "Read_" + core.CoreTypeIdentifier;

        private static string ReadEnumName(INamedTypeSymbol symbol) => "ReadEnum_" + symbol.Name;

        private MethodDeclarationSyntax CreateReadRootMethod(CoreDescriptor core)
        {
            return
                MethodDeclaration(
                    core.CoreType.Nullable(),
                    Identifier(
                        ReadRootName(core)))
                .AddModifiers(SyntaxKind.PublicKeyword)
                .AddParameterListParameters()
                .AddBodyStatements(
                    CreateBody());

            IEnumerable<StatementSyntax> CreateBody()
            {
                yield return ReturnStatement(Null);
            }
        }

        private MethodDeclarationSyntax CreateReadCoreMethod(CoreDescriptor core)
        {
            var isNullable = IdentifierName("isNullable");
            var checkType = IdentifierName("checkType");
            return
                MethodDeclaration(
                    core.CoreType.Nullable(),
                    Identifier(
                        ReadCoreName(core)))
                .AddParameterListParameters(
                    Parameter(isNullable.Identifier).WithType(Bool),
                    Parameter(checkType.Identifier).WithType(Bool))
                .AddBodyStatements(
                    CreateBody());

            IEnumerable<StatementSyntax> CreateBody()
            {
                yield return ReturnStatement(Null);
            }
        }
    }
}
