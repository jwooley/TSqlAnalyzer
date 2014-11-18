using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Linq;

namespace TSqlAnalyzer
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class SqlAnalyzerAnalyzer : DiagnosticAnalyzer
	{
		public const string DiagnosticId = "TSqlAnalyzer";
		internal const string Title = "Illegal T-SQL";
		internal const string MessageFormat = "{0}";
		internal const string Category = "Naming";

		internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

		public override void Initialize(AnalysisContext context)
		{
			// TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
			context.RegisterSyntaxNodeAction(AnalyzeConstructorNode, SyntaxKind.ObjectCreationExpression);
			context.RegisterSyntaxNodeAction(AnalyzeAssignmentNode, SyntaxKind.SimpleAssignmentExpression);
		}

		private static void AnalyzeAssignmentNode(SyntaxNodeAnalysisContext context)
		{
			var assignmentExpression = (AssignmentExpressionSyntax)context.Node;

			if (!assignmentExpression.Left.IsKind(SyntaxKind.SimpleMemberAccessExpression))
				return;

			//TODO Is it possible to detect type info?
			if (!assignmentExpression.Left.ToString().Contains("CommandText"))
				return;

            var literalExpression = assignmentExpression.Right as LiteralExpressionSyntax;

            if (literalExpression != null)
            {
                Diagnostics.LiteralExpressionDiagnostic.Run(context, literalExpression);
                return;
            }
            var binaryExpression = assignmentExpression.Right as BinaryExpressionSyntax;
            if (binaryExpression != null)
            {
                Diagnostics.BinaryExpressionDiagnostic.Run(context, binaryExpression);
                return;
            }
            Diagnostics.ExpressionDiagnostic.Run(context, assignmentExpression.Right as ExpressionSyntax);
        }


		private static void AnalyzeConstructorNode(SyntaxNodeAnalysisContext context)
		{
			var objectCreationExpression = (ObjectCreationExpressionSyntax)context.Node;

			//TODO Is it possible to detect type info?
			if (!objectCreationExpression.Type.ToString().Contains("SqlCommand"))
				return;

			if (objectCreationExpression.ArgumentList.Arguments.Count == 0)
				return;

			ExpressionSyntax expressionSyntax = objectCreationExpression.ArgumentList.Arguments.First().Expression;

			var literalExpression = expressionSyntax as LiteralExpressionSyntax;
            if (literalExpression != null)
            {
                Diagnostics.LiteralExpressionDiagnostic.Run(context, literalExpression);
                return;
            }
            var binaryExpression = expressionSyntax as BinaryExpressionSyntax;
            if(binaryExpression != null)
            {
                Diagnostics.BinaryExpressionDiagnostic.Run(context, binaryExpression);
                return;
            }

            Diagnostics.ExpressionDiagnostic.Run(context, expressionSyntax);
        }
	}
}
