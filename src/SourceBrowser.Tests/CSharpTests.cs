using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.CodeAnalysis.UnitTests;
using Microsoft.CodeAnalysis.UnitTests.SolutionGeneration;
using Microsoft.CodeAnalysis;
using System.Linq;
using SourceBrowser.Generator.Model;
using SourceBrowser.Generator;
using SourceBrowser.Generator.Model.CSharp;

namespace SourceBrowser.Tests
{
    [TestClass]
    public class CSharpTests : MSBuildWorkspaceTestBase
    {
        [TestMethod]
        public void SanityCheck()
        {
            //Set up the absolute minimum
            var solution = base.Solution(
                Project(
                    ProjectName("Project1"),
                    Sign,
                    Document(
                        @"
                        class C1
                        {
                            public void M1 () { }
                        }")));

            WorkspaceModel ws = new WorkspaceModel("Workspace1", "");
            FolderModel fm = new FolderModel(ws, "Project1");

            var document = solution.Projects.SelectMany(n => n.Documents).Where(n => n.Name == "Document1.cs").Single();
            var linkProvider = new ReferencesourceLinkProvider();

            var walker = new SourceBrowser.Generator.DocumentWalkers.CSWalker(fm, document, linkProvider);
            walker.Visit(document.GetSyntaxRootAsync().Result);
            var documentModel = walker.GetDocumentModel();

            //Make sure there's 10 tokens
            Assert.IsTrue(documentModel.Tokens.Count == 12);

            //Make sure they're classified correctly
            Assert.IsTrue(documentModel.Tokens.Count(n => n.Type == CSharpTokenTypes.KEYWORD) == 3);
            Assert.IsTrue(documentModel.Tokens.Count(n => n.Type == CSharpTokenTypes.TYPE) == 1);
            Assert.IsTrue(documentModel.Tokens.Count(n => n.Type == CSharpTokenTypes.IDENTIFIER) == 1);
            Assert.IsTrue(documentModel.Tokens.Count(n => n.Type == CSharpTokenTypes.OTHER) == 7);
        }

        [TestMethod]
        public void BasicLinking()
        {
            var solution = base.Solution(
             Project(
                 ProjectName("Project1"),
                 Sign,
                 Document(
                    @"
                    class C1
                    {
                        public void Method1()
                        {
                            Method2();
                        }
                        public void Method2()
                        {
                            Method1();
                        }
                    }")));

            WorkspaceModel ws = new WorkspaceModel("Workspace1", "");
            FolderModel fm = new FolderModel(ws, "Project1");

            var document = solution.Projects.SelectMany(n => n.Documents).Where(n => n.Name == "Document1.cs").Single();
            var linkProvider = new ReferencesourceLinkProvider();

            var walker = new SourceBrowser.Generator.DocumentWalkers.CSWalker(fm, document, linkProvider);
            walker.Visit(document.GetSyntaxRootAsync().Result);
            var documentModel = walker.GetDocumentModel();

            //Make sure there's two links
            var links = documentModel.Tokens.Select(n => n.Link).Where(n => n != null);
            Assert.IsTrue(links.Count() == 2);

            //Make sure they're both symbol links
            Assert.IsTrue(links.First() is SymbolLink);
            Assert.IsTrue(links.Last() is SymbolLink);

            //Make sure they link correctly
            Assert.IsTrue(((SymbolLink)links.First()).ReferencedSymbolName == "C1.Method2()");
            Assert.IsTrue(((SymbolLink)links.Last()).ReferencedSymbolName == "C1.Method1()");
        }

        [TestMethod]
        public void TestParameters()
        {
            var solution = base.Solution(
             Project(
                 ProjectName("Project1"),
                 Sign,
                 Document(
                    @"
                    class C1
                    {
                        public void M1(string p1, int p2, C1 p3)
                        {
                            p1 = String.Empty;
                            p2 = 0;
                            p3 = null;   
                        }
                    }")));

            WorkspaceModel ws = new WorkspaceModel("Workspace1", "");
            FolderModel fm = new FolderModel(ws, "Project1");

            var document = solution.Projects.SelectMany(n => n.Documents).Where(n => n.Name == "Document1.cs").Single();
            var linkProvider = new ReferencesourceLinkProvider();

            var walker = new SourceBrowser.Generator.DocumentWalkers.CSWalker(fm, document, linkProvider);
            walker.Visit(document.GetSyntaxRootAsync().Result);
            var documentModel = walker.GetDocumentModel();

            var links = documentModel.Tokens.Select(n => n.Link).Where(n => n != null);

            //TODO: Test Parameters once PR #66 is merged.

        }

        public void TestLocals()
        {

            var solution = base.Solution(
             Project(
                 ProjectName("Project1"),
                 Sign,
                 Document(
                    @"
                    class C1
                    {
                        public void M1()
                        {
                            string p1 = String.Empty;
                            int p2 = 0;
                            
                            p2 = p2 + 1;
                            p1 = p1 + ""sample text""
                        }
                    }")));

            WorkspaceModel ws = new WorkspaceModel("Workspace1", "");
            FolderModel fm = new FolderModel(ws, "Project1");

            var document = solution.Projects.SelectMany(n => n.Documents).Where(n => n.Name == "Document1.cs").Single();
            var linkProvider = new ReferencesourceLinkProvider();

            var walker = new SourceBrowser.Generator.DocumentWalkers.CSWalker(fm, document, linkProvider);
            walker.Visit(document.GetSyntaxRootAsync().Result);
            var documentModel = walker.GetDocumentModel();

            var links = documentModel.Tokens.Select(n => n.Link).Where(n => n != null);

            //TODO: Test locals once PR #66 is merged.

        }
    }
}
