using Microsoft.CodeAnalysis;
using SourceBrowser.Generator.Model;

namespace SourceBrowser.Generator.DocumentWalkers
{
    public interface IWalker
    {
        void Visit(SyntaxNode syntaxRoot);
        DocumentModel GetDocumentModel();
    }
}
