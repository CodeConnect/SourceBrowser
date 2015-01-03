using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.CodeAnalysis.UnitTests;
using Microsoft.CodeAnalysis.UnitTests.SolutionGeneration;
using Microsoft.CodeAnalysis;
using System.Linq;
using SourceBrowser.Generator.Model;
using SourceBrowser.Generator;

namespace SourceBrowser.Tests
{
    [TestClass]
    public class CSharpTests : MSBuildWorkspaceTestBase
    {
        [TestMethod]
        public void TestMethod1()
        {
            var solution = base.Solution(
                Project(
                    ProjectName("TestProject"),
                    Sign,
                    Document(string.Format(
                        @"
                        class C1
                        {{
                        }}", PublicKey))));

            //var document = solution.Projects.SelectMany(n => n.Documents).Single();
            //var linkProvider = new ReferencesourceLinkProvider();

            //var walker = new SourceBrowser.Generator.DocumentWalkers.CSWalker(null, document, linkProvider);
            //walker.Visit(document.GetSyntaxRootAsync().Result);
            //var documentModel = walker.GetDocumentModel();



        }
    }
}
