using Microsoft.CodeAnalysis;

namespace SourceBrowser.Generator.DocumentWalkers
{
    interface IWalkerUtils
    {
        string OtherTokenTypeName { get; }
        string KeywordTokenTypeName { get; }
        string StringTokenTypeName { get; }
        string TypeTokenTypeName { get; }
        string IdentifierTokenTypeName { get; }
        string LocalVariableDelimiter { get; }
        string ParameterDelimiter { get; }

        bool IsKeyword(SyntaxToken token);
        bool IsIdentifier(SyntaxToken token);
        bool IsLiteral(SyntaxToken token);
        string GetFullName(SyntaxToken token);

        void Visit(SyntaxNode syntaxRoot);
    }
}
