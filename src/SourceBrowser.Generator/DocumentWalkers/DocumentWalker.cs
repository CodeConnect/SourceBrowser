using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using SourceBrowser.Generator.Model;
using SourceBrowser.Generator.Extensions;
using Microsoft.CodeAnalysis.FindSymbols;

namespace SourceBrowser.Generator.DocumentWalkers
{
    class DocumentWalker<TWalkerUtils> : IWalker
        where TWalkerUtils : IWalkerUtils
    {
        private SemanticModel _model;
        private ReferencesourceLinkProvider _refsourceLinkProvider;
        public DocumentModel DocumentModel { get; private set; }
        public string FilePath { get; set; }

        private Document _document;

        private IWalkerUtils _walkerUtils;

        public DocumentWalker(IProjectItem parent, Document document, ReferencesourceLinkProvider refSourceLinkProvider,
            Func<DocumentWalker<TWalkerUtils>, TWalkerUtils> walkerUtilsFactoryMethod)
        {
            _model = document.GetSemanticModelAsync().Result;
            _refsourceLinkProvider = refSourceLinkProvider;
            string containingPath = document.GetRelativeFilePath();

            var numberOfLines = document.GetTextAsync().Result.Lines.Count + 1;
            DocumentModel = new DocumentModel(parent, document.Name, numberOfLines);
            FilePath = document.GetRelativeFilePath();
            _refsourceLinkProvider = refSourceLinkProvider;
            _document = document;

            _walkerUtils = walkerUtilsFactoryMethod(this);
        }

        public void Visit(SyntaxNode syntaxRoot)
        {
            _walkerUtils.Visit(syntaxRoot);
        }

        public void VisitToken(SyntaxToken token)
        {
            Token tokenModel = null;

            if (_walkerUtils.IsKeyword(token))
            {
                tokenModel = ProcessKeyword(token);
            }
            else if (_walkerUtils.IsIdentifier(token))
            {
                tokenModel = ProcessIdentifier(token);
            }
            else if (_walkerUtils.IsLiteral(token))
            {
                tokenModel = ProcessStringLiteral(token);
            }
            else
            {
                //This covers all semantically useless tokens such as punctuation
                tokenModel = ProcessOtherToken(token);
            }

            var leadingTrivia = ProcessTrivia(token.LeadingTrivia);
            var trailingTrivia = ProcessTrivia(token.TrailingTrivia);

            //Add trivia to the token
            tokenModel = tokenModel.WithTrivia(leadingTrivia, trailingTrivia);

            DocumentModel.Tokens.Add(tokenModel);
        }

        public DocumentModel GetDocumentModel()
        {
            return DocumentModel;
        }

        private ICollection<Trivia> ProcessTrivia(SyntaxTriviaList triviaList)
        {
            var triviaModelList = triviaList.Select(n => new Trivia(
                value: n.ToFullString(),
                type: n.CSharpKind().ToString()
            )).ToList();

            return triviaModelList;
        }

        /// <summary>
        /// Creates a Token based on a SyntaxToken for non-keywords and non-identifiers.
        /// </summary>
        private Token ProcessOtherToken(SyntaxToken token)
        {
            string fullName = token.CSharpKind().ToString();
            string value = token.ToString();
            string type = _walkerUtils.OtherTokenTypeName;
            int lineNumber = token.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

            var tokenModel = new Token(this.DocumentModel, fullName, value, type, lineNumber);
            return tokenModel;
        }

        /// <summary>
        /// Creates a Token based on a SyntaxToken for a Keyword.
        /// </summary>
        public Token ProcessKeyword(SyntaxToken token)
        {
            string fullName = _walkerUtils.GetFullName(token);
            string value = token.ToString();
            string type = _walkerUtils.KeywordTokenTypeName;
            int lineNumber = token.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
            var tokenModel = new Token(this.DocumentModel, fullName, value, type, lineNumber);
            return tokenModel;
        }

        private Token ProcessStringLiteral(SyntaxToken token)
        {
            string fullName = _walkerUtils.GetFullName(token);
            string value = token.ToString();
            string type = _walkerUtils.StringTokenTypeName;
            int lineNumber = token.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
            var tokenModel = new Token(this.DocumentModel, fullName, value, type, lineNumber);
            return tokenModel;
        }

        public Token ProcessSymbolUsage(SyntaxToken token, ISymbol symbol, bool isDeclaration)
        {
            string fullName = GetSymbolName(symbol);
            string value = token.ToString();
            string type = String.Empty;
            int lineNumber = token.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
            bool isSearchable = isDeclaration;

            if (symbol is INamedTypeSymbol)
            {
                type = _walkerUtils.TypeTokenTypeName;
            }
            else
            {
                type = _walkerUtils.IdentifierTokenTypeName;
            }

            //Do not allow us to search locals
            if (symbol.Kind == SymbolKind.Local || symbol.Kind == SymbolKind.Parameter)
            {
                isSearchable = false;
            }

            var tokenModel = new Token(this.DocumentModel, fullName, value, type, lineNumber, isDeclaration, isSearchable);

            //If we can find the declaration, we'll link it ourselves
            if (symbol.DeclaringSyntaxReferences.Any()
                && !(symbol is INamespaceSymbol))
            {
                var link = new SymbolLink(referencedSymbolName: fullName);
                tokenModel = tokenModel.WithLink(link);
            }
            //Otherwise, we try to link to the .Net Reference source
            else if (_refsourceLinkProvider.Assemblies.Contains(symbol.ContainingAssembly?.Identity?.Name)
                && !(symbol is INamespaceSymbol))
            {
                var link = new UrlLink(url: _refsourceLinkProvider.GetLink(symbol));
                tokenModel = tokenModel.WithLink(link);
            }

            return tokenModel;
        }

        private string GetSymbolName(ISymbol symbol)
        {
            //If this symbol was derived from another (eg. type substitution with generics)
            //retrieve the original definition of the symbol as it appeared in source code
            symbol = symbol.OriginalDefinition;
            string fullyQualifiedName;
            if (symbol.Kind == SymbolKind.Parameter)
            {
                var containingName = symbol.ContainingSymbol.ToString();
                fullyQualifiedName = containingName + _walkerUtils.ParameterDelimiter + symbol.Name;
            }
            else if (symbol.Kind == SymbolKind.Local)
            {
                var containingName = symbol.ContainingSymbol.ToString();
                fullyQualifiedName = containingName + _walkerUtils.LocalVariableDelimiter + symbol.Name;
            }
            else if(symbol.Kind == SymbolKind.Method)
            {
                var methodSymbol = (IMethodSymbol)symbol;
                if (methodSymbol.ReducedFrom != null)
                {
                    //If this method was reduced from an extension method, we 
                    //want to work with the fully qualified name as it originally 
                    //appeared in source code
                    fullyQualifiedName = methodSymbol.ReducedFrom.ToString();
                }
                else
                {
                    fullyQualifiedName = methodSymbol.ToString();
                }
            }
            else
            {
                fullyQualifiedName = symbol.ToString();
            }

            return fullyQualifiedName;
        }

        public Token ProcessIdentifier(SyntaxToken token)
        {
            //Check if this token is part of a declaration
            bool isDeclaration = false;
            if (_model.GetDeclaredSymbol(token.Parent) != null)
            {
                isDeclaration = true;
            }
            var startPosition = token.GetLocation().SourceSpan.Start;
            //Note: We're using the SymbolFinder as it correctly resolves 
            var symbol = SymbolFinder.FindSymbolAtPosition(_model, startPosition, _document.Project.Solution.Workspace);
            if (symbol != null)
            {
                return ProcessSymbolUsage(token, symbol, isDeclaration);
            }

            //Otherwise it references something we don't
            //have semantic information on...
            return ProcessOtherToken(token);
        }
    }
}
