using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json.Linq;

namespace SourceBrowser.Generator
{
    /// <summary>
    /// Analyzes the folder structure for a given Solution.
    /// </summary>
    public class SolutionFolderAnalyzer
    {
        Solution _solution;
        public SolutionFolderAnalyzer(Solution solution)
        {
            _solution = solution;
        }

        /// <summary>
        /// Analyzes the folder structure for the provided solution.
        /// </summary>
        /// <returns>The root node of the file system tree.</returns>
        public FolderItem AnalzeSolutionStructure()
        {
            var folderRoot = new FolderItem("test");
            foreach (var doc in _solution.Projects.SelectMany(n => n.Documents))
            {
                var currentPosition = folderRoot;
                var filePathParts = new List<string>();
                filePathParts.Add(doc.Project.Name);
                filePathParts.AddRange(doc.Folders);

                foreach (var part in filePathParts)
                {
                    if (!folderRoot.Children.ContainsKey(part))
                    {
                        var folder = new FolderItem(part);
                        currentPosition.Children[part] = folder;
                    }

                    currentPosition = currentPosition.Children[part] as FolderItem;
                }

                var fileName = doc.Name;
                var file = new FileItem(fileName);
                currentPosition.Children[fileName] = file;
            }

            return folderRoot;
        }
    }
}
