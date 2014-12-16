using System;
using System.IO;
using SourceBrowser.Generator;
using SourceBrowser.Generator.Transformers;

namespace SourceBrowser.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            // Combined with, or replaced by, provided paths to create absolute paths
            string userProfilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            // For developers that set test variables in code:
            string solutionPath = @"";
            string saveDirectory = @"";
            string username = "";
            string repository = "";

            // For developers that provide test variables in arguments:
            if (args.Length == 4)
            {
                solutionPath = args[0];
                saveDirectory = args[1];
                username = args[2];
                repository = args[3];
            }

            // Combine user's root with relative solution paths, or use absolute paths.
            // (Path.Combine returns just second argument if it is an absolute path)
            string absoluteSolutionPath = Path.Combine(userProfilePath, solutionPath);
            string absoluteSaveDirectory = Path.Combine(userProfilePath, saveDirectory);

            // Open and analyze the solution.
            try
            {
                Console.Write("Opening " + absoluteSolutionPath);
                Console.WriteLine("...");

                var solutionAnalyzer = new SolutionAnalayzer(absoluteSolutionPath);

                Console.Write("Analyzing and saving into " + absoluteSaveDirectory);
                Console.WriteLine("...");
                //NOTE: The rootPath is not always the parent directory of the solution.
                //The root path should be the longest path within which all documents are contained.
                var rootPath = Directory.GetParent(solutionPath).FullName;
                var workspaceModel = solutionAnalyzer.BuildWorkspaceModel(rootPath);

                var typeTransformer = new TokenLookupTransformer();
                typeTransformer.Visit(workspaceModel);
                var tokenLookup = typeTransformer.TokenLookup;

                var htmlTransformer = new HtmlTransformer(tokenLookup, absoluteSaveDirectory);
                htmlTransformer.Visit(workspaceModel);

                var searchTransformer = new SearchIndexTransformer(username, repository);
                searchTransformer.Visit(workspaceModel);

                var treeViewTransformer = new TreeViewTransformer(absoluteSaveDirectory, username, repository); 
                treeViewTransformer.Visit(workspaceModel);

                Console.WriteLine("Job successful!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error:");
                Console.WriteLine(ex.ToString());
                Console.ReadLine();
            }

        }
    }
}