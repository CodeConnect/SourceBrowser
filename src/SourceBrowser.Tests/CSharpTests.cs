using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.CodeAnalysis.UnitTests;
using Generate = Microsoft.CodeAnalysis.UnitTests.SolutionGeneration;
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
              Generate.Project(
                Generate.ProjectName("Project1"),
                Generate.Sign,
                Generate.Document(
                        @"
                        class C1
                        {
                            public void M1 () { }
                        }")));

            WorkspaceModel ws = new WorkspaceModel("Workspace1", "");
            FolderModel fm = new FolderModel(ws, "Project1");

            var document = solution.Projects.SelectMany(n => n.Documents).Where(n => n.Name == "Document1.cs").Single();
            var linkProvider = new ReferencesourceLinkProvider();

            var walker = SourceBrowser.Generator.DocumentWalkers.WalkerSelector.GetWalker(fm, document, linkProvider);
            
            walker.Visit(document.GetSyntaxRootAsync().Result);
            var documentModel = walker.GetDocumentModel();

            //Make sure there's 12 tokens
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
              Generate.Project(
                Generate.ProjectName("Project1"),
                Generate.Sign,
                Generate.Document(
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

            var walker = SourceBrowser.Generator.DocumentWalkers.WalkerSelector.GetWalker(fm, document, linkProvider);
            walker.Visit(document.GetSyntaxRootAsync().Result);
            var documentModel = walker.GetDocumentModel();

            //Make sure there's two links
            var links = documentModel.Tokens.Select(n => n.Link).Where(n => n != null);
            Assert.IsTrue(links.Count() == 5);

            //Make sure they're all symbol links
            Assert.IsTrue(links.All(n => n is SymbolLink));

            //Make sure they link correctly
            Assert.IsTrue(links.Count(n => ((SymbolLink)(n)).ReferencedSymbolName == "C1.Method1()") == 2);
            Assert.IsTrue(links.Count(n => ((SymbolLink)(n)).ReferencedSymbolName == "C1.Method2()") == 2);
            Assert.IsTrue(links.Count(n => ((SymbolLink)(n)).ReferencedSymbolName == "C1") == 1);
        }

        [TestMethod]
        public void TestParameters()
        {
            var solution = base.Solution(
              Generate.Project(
                Generate.ProjectName("Project1"),
                Generate.Sign,
                Generate.Document(
                    @"
                    class C1
                    {
                        public void M1(string p1, int p2, C1 p3)
                        {
                            p1 = null;
                            p2 = 0;
                            p3 = null;   
                        }
                    }")));

            WorkspaceModel ws = new WorkspaceModel("Workspace1", "");
            FolderModel fm = new FolderModel(ws, "Project1");

            var document = solution.Projects.SelectMany(n => n.Documents).Where(n => n.Name == "Document1.cs").Single();
            var linkProvider = new ReferencesourceLinkProvider();

            var walker = SourceBrowser.Generator.DocumentWalkers.WalkerSelector.GetWalker(fm, document, linkProvider);
            walker.Visit(document.GetSyntaxRootAsync().Result);
            var documentModel = walker.GetDocumentModel();

            var links = documentModel.Tokens.Select(n => n.Link).Where(n => n != null);
            var symbolLinks = links.Select(n => n as SymbolLink);

            Assert.IsTrue(links.Count() == 9);

            Assert.IsTrue(symbolLinks.Where(n => n.ReferencedSymbolName == "C1").Count() == 2);
            Assert.IsTrue(symbolLinks.Where(n => n.ReferencedSymbolName == "C1.M1(string, int, C1)").Count() == 1);
            Assert.IsTrue(symbolLinks.Where(n => n.ReferencedSymbolName == "C1.M1(string, int, C1)::p1").Count() == 2);
            Assert.IsTrue(symbolLinks.Where(n => n.ReferencedSymbolName == "C1.M1(string, int, C1)::p2").Count() == 2);
            Assert.IsTrue(symbolLinks.Where(n => n.ReferencedSymbolName == "C1.M1(string, int, C1)::p3").Count() == 2);
        }

        [TestMethod]
        public void TestLocals()
        {
            var solution = base.Solution(
              Generate.Project(
                Generate.ProjectName("Project1"),
                Generate.Sign,
                Generate.Document(
                    @"
                    class C1
                    {
                        public void M1()
                        {
                            string l1 = ""hello""
                            l1 = l1 + "" world""

                            int l2 = 0;
                            l2 = l2 + 1;
                        }
                    }")));

            WorkspaceModel ws = new WorkspaceModel("Workspace1", "");
            FolderModel fm = new FolderModel(ws, "Project1");

            var document = solution.Projects.SelectMany(n => n.Documents).Where(n => n.Name == "Document1.cs").Single();
            var linkProvider = new ReferencesourceLinkProvider();

            var walker = SourceBrowser.Generator.DocumentWalkers.WalkerSelector.GetWalker(fm, document, linkProvider);
            walker.Visit(document.GetSyntaxRootAsync().Result);
            var documentModel = walker.GetDocumentModel();

            var links = documentModel.Tokens.Select(n => n.Link).Where(n => n != null);
            var symbolLinks = links.Select(n => n as SymbolLink);

            Assert.IsTrue(links.Count() == 8);

            Assert.IsTrue(symbolLinks.Where(n => n.ReferencedSymbolName == "C1").Count() == 1);
            Assert.IsTrue(symbolLinks.Where(n => n.ReferencedSymbolName == "C1.M1()").Count() == 1);
            Assert.IsTrue(symbolLinks.Where(n => n.ReferencedSymbolName == "C1.M1()::l1").Count() == 3);
            Assert.IsTrue(symbolLinks.Where(n => n.ReferencedSymbolName == "C1.M1()::l2").Count() == 3);
        }

        [TestMethod]
        public void TestExtensionMethods()
        {
            var solution = base.Solution(
              Generate.Project(
                Generate.ProjectName("Project1"),
                Generate.Sign,
                Generate.Document(
                  @"
                    public static class MyExtensions
                    {
                        public string ExtensionMethod(this string myParam)
                        {
                        }
                    }

                    class MyClass
                    {
                        public void MyMethod()
                        {
                            ""string"".ExtensionMethod();
                        }
                    }
                   ")));

            WorkspaceModel ws = new WorkspaceModel("Workspace1", "");
            FolderModel fm = new FolderModel(ws, "Project1");

            var document = solution.Projects.SelectMany(n => n.Documents).Where(n => n.Name == "Document1.cs").Single();
            var linkProvider = new ReferencesourceLinkProvider();

            var walker = SourceBrowser.Generator.DocumentWalkers.WalkerSelector.GetWalker(fm, document, linkProvider);
            walker.Visit(document.GetSyntaxRootAsync().Result);
            var documentModel = walker.GetDocumentModel();

            var links = documentModel.Tokens.Select(n => n.Link).Where(n => n != null);
            var symbolLinks = links.Select(n => n as SymbolLink);

            Assert.IsTrue(symbolLinks.Count() == 6);

            Assert.IsTrue(symbolLinks.Where(n => n.ReferencedSymbolName == "MyExtensions").Count() == 1);
            Assert.IsTrue(symbolLinks.Where(n => n.ReferencedSymbolName == "MyExtensions.ExtensionMethod(string)").Count() == 2);
            Assert.IsTrue(symbolLinks.Where(n => n.ReferencedSymbolName == "MyExtensions.ExtensionMethod(string)::myParam").Count() == 1);
            Assert.IsTrue(symbolLinks.Where(n => n.ReferencedSymbolName == "MyClass").Count() == 1);
            Assert.IsTrue(symbolLinks.Where(n => n.ReferencedSymbolName == "MyClass.MyMethod()").Count() == 1);
        }
    }
}
