using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace SourceBrowser.Generator
{
    /// <summary>
    /// A number of extension methods for dealing with the relative path of the
    /// document. Often, we're not interested in the document's absolute file path, 
    /// just its path relative to the root of the project.
    /// These extension methods allow us to deal with this relative path.
    /// </summary>
    public static class DocumentExtensions
    {
        /// <summary>
        /// Retrieves the document's containing path relative to the root of the project.
        /// </summary>
        public static string GetContainingFolderPath(this Document document)
        {
            string path = String.Empty;
            path += document.Project.Name;
            foreach (var folder in document.Folders)
            {
                path += "\\";
                path += folder;
            }

            path += "\\";
            return path;
        }
        
        /// <summary>
        /// Retrieves the document's path relative to the root of the project
        /// </summary>
        public static string GetRelativeFilePath(this Document document)
        {
            string path = document.GetContainingFolderPath();
            path += document.Name;
            return path;
        }
    }
}
