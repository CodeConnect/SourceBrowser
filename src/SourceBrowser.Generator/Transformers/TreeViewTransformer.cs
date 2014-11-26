using System;
using System.IO;
using System.Web.UI;
using System.Text;
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
        HtmlTextWriter _writer;
        private readonly string _userNameAndRepoPrefix;

        private const string _treeViewOutputFile = "treeView.html";

        public TreeViewTransformer(string savePath, string userName, string repoName)
        {
            _savePath = Path.Combine(savePath, _treeViewOutputFile);
            if (String.IsNullOrEmpty(userName) || String.IsNullOrEmpty(repoName))
            {
                throw new ArgumentNullException("TreeViewTransformer needs to be provided the user name and the repo name.");
            }
            _userNameAndRepoPrefix = "/Browse/" + userName + "/" + repoName + "/";
        }

        protected override void VisitWorkspace(WorkspaceModel workspaceModel)
        {
            using (var stringWriter = new StreamWriter(_savePath, false))
            {
                _writer = new HtmlTextWriter(stringWriter);

                _writer.AddAttribute(HtmlTextWriterAttribute.Id, getFullId(workspaceModel));
                _writer.AddAttribute(HtmlTextWriterAttribute.Class, "treeview");
                _writer.RenderBeginTag(HtmlTextWriterTag.Ul);

                base.VisitWorkspace(workspaceModel);

                _writer.RenderEndTag();
                _writer.WriteLine();

                try
                {
                    _writer.Dispose();
                }
                catch
                {
                    throw;
                }
            }
        }

        protected override void VisitProjectItem(IProjectItem projectItem)
        {
            base.VisitProjectItem(projectItem);
        }

        protected override void VisitFolder(FolderModel folderModel)
        {
            // The clicable element with the folder name:
            _writer.AddAttribute(HtmlTextWriterAttribute.Class, "node collapsed");
            // ID will be used to programmatically show the underlying UL tag by removing "collapsed" class
            _writer.AddAttribute(HtmlTextWriterAttribute.Id, getFullId(folderModel)); 
            _writer.RenderBeginTag(HtmlTextWriterTag.Li);

            // Folder item is not a link. It is merely used to hide/show the underlying UL tag
            _writer.AddAttribute(HtmlTextWriterAttribute.Href, "#");
            _writer.RenderBeginTag(HtmlTextWriterTag.A);

            _writer.Write(folderModel.Name);

            _writer.RenderEndTag(); // a

            // li end tag will be written later
            _writer.WriteLine();


            // The underlying tree branch:
            _writer.RenderBeginTag(HtmlTextWriterTag.Ul);

            base.VisitFolder(folderModel);

            _writer.RenderEndTag(); // ul
            _writer.WriteLine();

            _writer.RenderEndTag(); // li
            _writer.WriteLine();
        }

        protected override void VisitDocument(DocumentModel documentModel)
        {
            _writer.AddAttribute(HtmlTextWriterAttribute.Class, "node collapsed");
            _writer.RenderBeginTag(HtmlTextWriterTag.Li);

            _writer.AddAttribute(HtmlTextWriterAttribute.Href, getHyperLink(documentModel));
            _writer.RenderBeginTag(HtmlTextWriterTag.A);

            _writer.Write(documentModel.Name);

            _writer.RenderEndTag(); // a

            base.VisitDocument(documentModel);

            _writer.RenderEndTag(); // li
            _writer.WriteLine();
        }

        private string getFullId(IProjectItem item)
        {
            if (item.Parent != null)
            {
                return getFullId(item.Parent) + "/" + item.Name;
            }
            else
            {
                return item.Name;
            }
        }

        private string getHyperLink(IProjectItem item)
        {
            return _userNameAndRepoPrefix + getFullId(item);
        }
    }
}
