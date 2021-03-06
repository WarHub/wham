﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WarHub.ArmouryModel.Source.CodeGeneration
{
    internal abstract class PartialGeneratorBase : GeneratorBase
    {
        protected PartialGeneratorBase(CoreDescriptor descriptor, CancellationToken cancellationToken)
        {
            Descriptor = descriptor;
            CancellationToken = cancellationToken;
            BaseType = Descriptor.TypeSymbol.BaseType ?? throw new ArgumentException("Type must have any base type");
            IsDerived = BaseType.SpecialType != SpecialType.System_Object;
        }

        protected bool IsAbstract => Descriptor.TypeSymbol.IsAbstract;

        protected INamedTypeSymbol BaseType { get; }

        protected bool IsDerived { get; }

        protected CoreDescriptor Descriptor { get; }

        protected CancellationToken CancellationToken { get; }

        public virtual TypeDeclarationSyntax GenerateTypeDeclaration()
        {
            var baseList = BaseList(SeparatedList(GenerateBaseTypes()));
            var typeParameterList = TypeParameterList(SeparatedList(GenerateTypeParameters()));
            return
                GenerateTypeDeclarationCore(GenerateTypeIdentifier())
                .WithTypeParameterList(typeParameterList.Parameters.Count == 0 ? null : typeParameterList)
                .WithBaseList(baseList.Types.Count == 0 ? null : baseList)
                .WithModifiers(
                    GenerateModifiers())
                .WithMembers(
                    List(
                        GenerateMembers()))
                .WithLeadingTrivia(
                    GenerateLeadingTrivia())
                .WithTrailingTrivia(
                    GenerateTrailingTrivia());
        }

        protected virtual TypeDeclarationSyntax GenerateTypeDeclarationCore(SyntaxToken typeIdentifier) =>
            ClassDeclaration(typeIdentifier);

        protected virtual SyntaxTriviaList GenerateLeadingTrivia()
        {
            return
                TriviaList(
                    Comment($"// Generated by {GetType().FullName}"),
                    Trivia(NullableDirectiveTrivia(Token(SyntaxKind.EnableKeyword), true)));
        }

        protected virtual SyntaxTriviaList GenerateTrailingTrivia()
        {
            return
                TriviaList(
                    Trivia(NullableDirectiveTrivia(Token(SyntaxKind.RestoreKeyword), true)),
                    Comment($"// Generated by {GetType().FullName}"));
        }

        protected virtual IEnumerable<TypeParameterSyntax> GenerateTypeParameters()
        {
            return Enumerable.Empty<TypeParameterSyntax>();
        }

        protected abstract SyntaxToken GenerateTypeIdentifier();

        protected virtual SyntaxTokenList GenerateModifiers()
        {
            return TokenList(Token(SyntaxKind.PartialKeyword));
        }

        protected virtual IEnumerable<MemberDeclarationSyntax> GenerateMembers()
        {
            return Enumerable.Empty<MemberDeclarationSyntax>();
        }

        protected virtual IEnumerable<BaseTypeSyntax> GenerateBaseTypes()
        {
            return Enumerable.Empty<BaseTypeSyntax>();
        }

        private static Lazy<AttributeSyntax> DebuggerBrowsableNeverAttributeLazy { get; } = new Lazy<AttributeSyntax>(() =>
        {
            return
                Attribute(
                    IdentifierName(Names.DebuggerBrowsable))
                .AddArgumentListArguments(
                    AttributeArgument(
                        IdentifierName(Names.DebuggerBrowsableState)
                        .Dot(
                            IdentifierName(Names.DebuggerBrowsableStateNever))));
        });

        private static Lazy<AttributeListSyntax> MaybeNullReturnAttributeListLazy { get; } = new Lazy<AttributeListSyntax>(() =>
        {
            return
                AttributeList()
                .WithTarget(
                    AttributeTargetSpecifier(
                        Token(SyntaxKind.ReturnKeyword)))
                .AddAttributes(
                    Attribute(
                        IdentifierName(Names.MaybeNull)));
        });

        protected static AttributeSyntax DebuggerBrowsableNeverAttribute => DebuggerBrowsableNeverAttributeLazy.Value;
        protected static AttributeListSyntax MaybeNullReturnAttributeList => MaybeNullReturnAttributeListLazy.Value;
    }
}
