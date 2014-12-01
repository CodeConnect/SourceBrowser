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
        private int depth = 0;

        public TreeViewTransformer(string savePath, string userName, string repoName)
        {
            _savePath = Path.Combine(savePath, _treeViewOutputFile);
            if (String.IsNullOrEmpty(userName) || String.IsNullOrEmpty(repoName))
            {
                throw new ArgumentNullException("TreeViewTransformer needs to be provided the user name and the repo name.");
            }
            _userNameAndRepoPrefix = Path.Combine("/Browse", userName, repoName);
        }

        protected override void VisitWorkspace(WorkspaceModel workspaceModel)
        {
            using (var stringWriter = new StreamWriter(_savePath, false))
            using(_writer = new HtmlTextWriter(stringWriter))
            {
                _writer.AddAttribute(HtmlTextWriterAttribute.Id, "browserTree");
                _writer.AddAttribute(HtmlTextWriterAttribute.Class, "treeview");
                _writer.AddAttribute("data-role", "treeview");
                _writer.RenderBeginTag(HtmlTextWriterTag.Ul);

                depth++;
                base.VisitWorkspace(workspaceModel);
                depth--;

                _writer.RenderEndTag();
                _writer.WriteLine();
            }
        }


        protected override void VisitFolder(FolderModel folderModel)
        {
            // The clicable element with the folder name:
            _writer.AddAttribute(HtmlTextWriterAttribute.Class, "node collapsed");
            // ID will be used to programmatically show the underlying UL tag by removing "collapsed" class
            _writer.AddAttribute(HtmlTextWriterAttribute.Id, folderModel.GetPath());
            _writer.RenderBeginTag(HtmlTextWriterTag.Li);

            // Folder item is not a link. It is merely used to hide/show the underlying UL tag
            _writer.AddAttribute(HtmlTextWriterAttribute.Href, "#");
            _writer.AddAttribute(HtmlTextWriterAttribute.Style, "margin-left: " + depth * 10 + "px;");
            _writer.RenderBeginTag(HtmlTextWriterTag.A);

            // The expander:
            _writer.AddAttribute(HtmlTextWriterAttribute.Class, "node-toggle");
            _writer.RenderBeginTag(HtmlTextWriterTag.Span);
            _writer.RenderEndTag(); // span

            _writer.Write(folderModel.Name);

            _writer.RenderEndTag(); // a

            // li end tag will be written later
            _writer.WriteLine();


            // The underlying tree branch:
            _writer.RenderBeginTag(HtmlTextWriterTag.Ul);

            depth++;
            base.VisitFolder(folderModel);
            depth--;

            _writer.RenderEndTag(); // ul
            _writer.WriteLine();

            _writer.RenderEndTag(); // li
            _writer.WriteLine();
        }

        protected override void VisitDocument(DocumentModel documentModel)
        {
            _writer.AddAttribute(HtmlTextWriterAttribute.Class, "node collapsed");
            _writer.RenderBeginTag(HtmlTextWriterTag.Li);

            string linkPath = Path.Combine(_userNameAndRepoPrefix, documentModel.RelativePath);
            _writer.AddAttribute(HtmlTextWriterAttribute.Href, linkPath);
            _writer.AddAttribute(HtmlTextWriterAttribute.Style, "margin-left: " + depth * 10 + "px;");
            _writer.RenderBeginTag(HtmlTextWriterTag.A);

            _writer.Write(documentModel.Name);

            _writer.RenderEndTag(); // a

            base.VisitDocument(documentModel);

            _writer.RenderEndTag(); // li
            _writer.WriteLine();
        }
    }
}
