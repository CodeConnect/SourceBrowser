using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SourceBrowser.Generator.Model.CSharp;

namespace SourceBrowser.Generator.DocumentWalkers
{
    class CSWalkerUtils : CSharpSyntaxWalker, IWalkerUtils
    {
        private readonly DocumentWalker<CSWalkerUtils> _walker;

        internal CSWalkerUtils(DocumentWalker<CSWalkerUtils> walker)
            : base(SyntaxWalkerDepth.Trivia)
        {
            _walker = walker;
        }

        public string IdentifierTokenTypeName {  get; } = CSharpTokenTypes.IDENTIFIER;

        public string KeywordTokenTypeName { get; } = CSharpTokenTypes.KEYWORD;

        public string OtherTokenTypeName { get; } = CSharpTokenTypes.OTHER;

        public string StringTokenTypeName { get; } = CSharpTokenTypes.STRING;

        public string TypeTokenTypeName { get; } = CSharpTokenTypes.TYPE;

        public string ParameterDelimiter { get; } = CSharpDelimiters.PARAMETER;

        public string LocalVariableDelimiter { get; } = CSharpDelimiters.LOCAL_VARIABLE;


        public string GetFullName(SyntaxToken token) => token.CSharpKind().ToString();

        public bool IsIdentifier(SyntaxToken token) => token.CSharpKind() == SyntaxKind.IdentifierToken;

        public bool IsKeyword(SyntaxToken token) => token.IsKeyword();

        public bool IsLiteral(SyntaxToken token) => token.CSharpKind() == SyntaxKind.StringLiteralToken;

        public override void VisitToken(SyntaxToken token) => _walker.VisitToken(token);
    }
}
