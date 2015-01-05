using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using SourceBrowser.Generator.Model.VisualBasic;

namespace SourceBrowser.Generator.DocumentWalkers
{
    class VBWalkerUtils : VisualBasicSyntaxWalker, IWalkerUtils
    {
        private readonly DocumentWalker<VBWalkerUtils> _walker;

        internal VBWalkerUtils(DocumentWalker<VBWalkerUtils> walker)
             : base(SyntaxWalkerDepth.Trivia)
        {
            _walker = walker;
        }

        public string IdentifierTokenTypeName { get; } = VisualBasicTokenTypes.IDENTIFIER;

        public string KeywordTokenTypeName { get; } = VisualBasicTokenTypes.KEYWORD;

        public string OtherTokenTypeName { get; } = VisualBasicTokenTypes.OTHER;

        public string StringTokenTypeName { get; } = VisualBasicTokenTypes.STRING;

        public string TypeTokenTypeName { get; } = VisualBasicTokenTypes.TYPE;

        public string ParameterDelimiter { get; } = VBDelimiters.PARAMETER;

        public string LocalVariableDelimiter { get; } = VBDelimiters.LOCAL_VARIABLE;


        public string GetFullName(SyntaxToken token) => token.CSharpKind().ToString();

        public bool IsIdentifier(SyntaxToken token) => token.VBKind() == SyntaxKind.IdentifierToken;

        public bool IsKeyword(SyntaxToken token) => token.IsKeyword();

        public bool IsLiteral(SyntaxToken token) => token.VBKind() == SyntaxKind.StringLiteralToken;

        public override void VisitToken(SyntaxToken token) => _walker.VisitToken(token);
    }
}
