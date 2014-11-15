using SourceBrowser.Generator.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceBrowser.Generator.Transformers
{
    /// <summary>
    /// The abstract base class for all transformers.
    /// Visits all nodes within the WorkspaceModel.
    /// </summary>
    public abstract class AbstractWorkspaceVisitor
    {
        private WorkspaceModel _workspaceModel;
        public AbstractWorkspaceVisitor()
        {
        }

        public virtual void Visit(WorkspaceModel workspaceModel)
        {
            _workspaceModel = workspaceModel;
            VisitProjectItem(_workspaceModel);
        }

        protected virtual void VisitWorkspace(WorkspaceModel workspaceModel)
        {
            foreach(var child in _workspaceModel.Children)
            {
                VisitProjectItem(child);
            }
        }

        protected virtual void VisitProjectItem(IProjectItem projectItem)
        {
            if(projectItem is FolderModel)
            {
                VisitFolder((FolderModel)projectItem);
            }
            else if(projectItem is DocumentModel)
            {
                VisitDocument((DocumentModel)projectItem);
            }
            else if(projectItem is WorkspaceModel)
            {
                VisitWorkspace((WorkspaceModel)projectItem);
            }
            else
            {
                throw new InvalidOperationException("Unhandled: " + nameof(projectItem));
            }
        }

        protected virtual void VisitFolder(FolderModel folderModel)
        {
            foreach(var child in folderModel.Children)
            {
                VisitProjectItem(child);
            }
        }

        protected virtual void VisitDocument(DocumentModel documentModel)
        {
            foreach(var child in documentModel.Children)
            {
                VisitProjectItem(child);
            }
        }
    }
}
