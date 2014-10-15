using System.Collections.Generic;

namespace SourceBrowser.Generator
{
    /// <summary>
    /// The base type for all items within our representation of the file system.
    /// </summary>
    public class FileSystemItem
    {
        string Name { get; set; }
        public FileSystemItem(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// Represents a folder with child file system items.
    /// These items may be files or folders.
    /// </summary>
    public class FolderItem : FileSystemItem
    {
        public Dictionary<string, FileSystemItem> Children { get; set; }
        public FolderItem(string name) : base(name)
        {
            Children = new Dictionary<string, FileSystemItem>();
        }
    }

    /// <summary>
    /// Represents an individual file within our file system tree.
    /// </summary>
    public class FileItem : FileSystemItem
    {
        public FileItem(string name) : base(name)
        {
        }
    }
}
