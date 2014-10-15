using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using System.IO;

namespace SourceBrowser.Generator
{
    class Program
    {
        static void Main(string[] args)
        {
            string relativeSolutionPath = @"Documents\GitHub\Kiwi\TestSolution\TestSolution.sln";
            string userProfilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string absoluteSolutionPath = Path.Combine(userProfilePath, relativeSolutionPath);

            //var solnAnalyzer = new SolutionAnalayzer(absoluteSolutionPath);
            //solnAnalyzer.Start();
        }
    }
}
