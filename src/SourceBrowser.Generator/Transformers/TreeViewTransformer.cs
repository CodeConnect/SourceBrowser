using SourceBrowser.Generator.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceBrowser.Generator.Transformers
{
    /// <summary>
    /// Converts a WorkspaceModel into HTML code
    /// representing the tree view of the workspace's folder and file structure
    /// </summary>
    public class TreeViewTransformer : AbstractWorkspaceVisitor
    {
        private string _savePath;

        public TreeViewTransformer(string savePath)
        {
            _savePath = savePath;
        }

        protected override void VisitWorkspace(WorkspaceModel workspaceModel)
        {
            base.VisitWorkspace(workspaceModel);
        }

        protected override void VisitProjectItem(IProjectItem projectItem)
        {
            base.VisitProjectItem(projectItem);
        }

        protected override void VisitFolder(FolderModel folderModel)
        {
            base.VisitFolder(folderModel);
        }

        protected override void VisitDocument(DocumentModel documentModel)
        {
            base.VisitDocument(documentModel);
        }
    }
}
