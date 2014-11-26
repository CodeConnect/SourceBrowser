using System;
using System.IO;
using System.Web.UI;
using SourceBrowser.Generator.Model;

namespace SourceBrowser.Generator.Transformers
{
    /// <summary>
    /// Converts a WorkspaceModel into HTML code
    /// representing the tree view of the workspace's folder and file structure
    /// </summary>
    public class TreeViewTransformer : AbstractWorkspaceVisitor
    {
        private string _savePath;
        StringWriter _stringWriter;
        HtmlTextWriter _writer;

        public TreeViewTransformer(string savePath)
        {
            _savePath = savePath;
            _stringWriter = new StringWriter();
            _writer = new HtmlTextWriter(_stringWriter);
        }

        protected override void VisitWorkspace(WorkspaceModel workspaceModel)
        {
            base.VisitWorkspace(workspaceModel);
            _writer.RenderEndTag();
        }

        protected override void VisitProjectItem(IProjectItem projectItem)
        {
            _writer.AddAttribute(HtmlTextWriterAttribute.Id, getFullPath(projectItem));
            _writer.RenderBeginTag(HtmlTextWriterTag.Ul);
            base.VisitProjectItem(projectItem);
            _writer.RenderEndTag();
        }

        protected override void VisitFolder(FolderModel folderModel)
        {
            _writer.AddAttribute(HtmlTextWriterAttribute.Id, getFullPath(folderModel));
            _writer.RenderBeginTag(HtmlTextWriterTag.Ul);
            base.VisitFolder(folderModel);
            _writer.RenderEndTag();
        }

        protected override void VisitDocument(DocumentModel documentModel)
        {
            _writer.RenderBeginTag(HtmlTextWriterTag.Li);
            base.VisitDocument(documentModel);
            _writer.RenderEndTag();
        }

        private string getFullPath(IProjectItem item)
        {
            if (item.Parent != null)
            {
                return getFullPath(item.Parent) + "/" + item.Name;
            }
            else
            {
                return item.Name;
            }
        }
    }
}
