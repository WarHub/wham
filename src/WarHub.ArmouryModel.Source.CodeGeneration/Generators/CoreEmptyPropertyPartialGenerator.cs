using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal class CoreEmptyPropertyPartialGenerator : CorePartialGeneratorBase
    {
        protected CoreEmptyPropertyPartialGenerator(CoreDescriptor descriptor, CancellationToken cancellationToken) : base(descriptor, cancellationToken)
        {
        }

        public static TypeDeclarationSyntax Generate(CoreDescriptor descriptor, CancellationToken cancellationToken)
        {
            var generator = new CoreEmptyPropertyPartialGenerator(descriptor, cancellationToken);
            return generator.GenerateTypeDeclaration();
        }

        protected override IEnumerable<MemberDeclarationSyntax> GenerateMembers()
        {
            yield return
                PropertyDeclaration(Descriptor.CoreType, Names.Empty)
                .AddModifiers(SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword)
                .AddAccessorListAccessors(
                    AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithSemicolonTokenDefault())
                .WithInitializer(
                    EqualsValueClause(
                        ImplicitObjectCreationExpression()))
                .WithSemicolonToken(
                    Token(SyntaxKind.SemicolonToken));
        }
    }
}
