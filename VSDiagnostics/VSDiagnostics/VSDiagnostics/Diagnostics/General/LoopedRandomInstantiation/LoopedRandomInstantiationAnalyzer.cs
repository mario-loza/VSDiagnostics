﻿using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using VSDiagnostics.Utilities;

namespace VSDiagnostics.Diagnostics.General.LoopedRandomInstantiation
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class LoopedRandomInstantiationAnalyzer : DiagnosticAnalyzer
    {
        private const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;
        private static readonly string Category = VSDiagnosticsResources.GeneralCategory;
        private static readonly string Message = VSDiagnosticsResources.LoopedRandomInstantiationAnalyzerMessage;
        private static readonly string Title = VSDiagnosticsResources.LoopedRandomInstantiationAnalyzerTitle;

        private readonly SyntaxKind[] _loopTypes = { SyntaxKind.ForEachStatement, SyntaxKind.ForStatement, SyntaxKind.WhileStatement, SyntaxKind.DoStatement };

        internal static DiagnosticDescriptor Rule =>
                new DiagnosticDescriptor(DiagnosticId.LoopedRandomInstantiation, Title, Message, Category, Severity, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context) => context.RegisterSyntaxNodeAction(AnalyzeSymbol, SyntaxKind.VariableDeclaration);

        private void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
        {
            var variableDeclaration = (VariableDeclarationSyntax) context.Node;

            var type = variableDeclaration.Type;
            if (type == null)
            {
                return;
            }

            var typeInfo = context.SemanticModel.GetTypeInfo(type).Type;

            if (typeInfo?.OriginalDefinition.ContainingNamespace == null ||
                typeInfo.OriginalDefinition.ContainingNamespace.Name != nameof(System) ||
                typeInfo.Name != nameof(Random))
            {
                return;
            }

            SyntaxNode currentNode = variableDeclaration;
            while (!currentNode.IsAnyKind(SyntaxKind.ClassDeclaration, SyntaxKind.StructDeclaration))
            {
                if (_loopTypes.Contains(currentNode.Kind()))
                {
                    foreach (var declarator in variableDeclaration.Variables)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Rule, declarator.GetLocation(), declarator.Identifier.Text));
                    }
                    return;
                }

                currentNode = currentNode.Parent;
            }
        }
    }
}