using System;
using System.IO;
using System.Web.UI;
using System.Text;
using SourceBrowser.Generator.Model;
using System.Web;

namespace SourceBrowser.Generator.Transformers
{
    /// <summary>
    /// Converts a WorkspaceModel into HTML code
    /// representing the tree view of the workspace's folder and file structure
    /// </summary>
    public class TreeViewTransformer : AbstractWorkspaceVisitor
    {
        private string _savePath;
        private StreamWriter _sw;
        private HtmlTextWriter _writer;
        private readonly string _userNameAndRepoPrefix;

        private const string _treeViewOutputFile = "treeView.html";
        private int depth = 0;

        public TreeViewTransformer(string savePath, string userName, string repoName)
        {
            _savePath = Path.Combine(savePath, _treeViewOutputFile);
            Directory.CreateDirectory(Directory.GetParent(_savePath).FullName);

            if (String.IsNullOrEmpty(userName) || String.IsNullOrEmpty(repoName))
            {
                throw new ArgumentNullException("TreeViewTransformer needs to be provided the user name and the repo name.");
            }
            _userNameAndRepoPrefix = Path.Combine("/Browse", userName, repoName);
        }

        protected override void VisitWorkspace(WorkspaceModel workspaceModel)
        {
            // The first WorkspaceModel that is visited is the root of the tree view
            // and its children are the solutions.

            bool disposeWriters = false;

            // Create the writers only if they're null
            if (_sw == null)
            {
                _sw = new StreamWriter(_savePath, false);

                if (_writer != null)
                    _writer.Dispose();

                _writer = new HtmlTextWriter(_sw);

                disposeWriters = true;
            }

            if (disposeWriters)
            {
                // The current WorkspaceModel is the root node, no need to increase the depth
                _writer.AddAttribute(HtmlTextWriterAttribute.Id, "browserTree");
                _writer.AddAttribute(HtmlTextWriterAttribute.Class, "treeview");
                _writer.AddAttribute("data-role", "treeview");
                _writer.RenderBeginTag(HtmlTextWriterTag.Ul);
            }
            else
            {
                // The current WorkspaceModel is a Child of the root node.
                depth++;
            }
            
            base.VisitWorkspace(workspaceModel);

            if (disposeWriters)
            {
                // The current WorkspaceModel is the root node.
                // Every child has been visited: dispose the writers.

                disposeWriters = false;
                _writer.RenderEndTag();
                _writer.WriteLine();
                _writer.Dispose();
                _sw.Dispose();
            }
            else
            {
                // The current WorkspaceModel is a Child of the root node.
                depth--;
            }
        }


        protected override void VisitFolder(FolderModel folderModel)
        {
            // The clicable element with the folder name:
            _writer.AddAttribute(HtmlTextWriterAttribute.Class, "node collapsed");
            // ID will be used to programmatically show the underlying UL tag by removing "collapsed" class

            //NOTE: We need to correct the backslashes to forward slashes... Thanks Windows...
            var urlStylePath = folderModel.RelativePath.Replace('\\','/');
            _writer.AddAttribute(HtmlTextWriterAttribute.Id, urlStylePath);
            _writer.AddAttribute(HtmlTextWriterAttribute.Title, HttpUtility.HtmlEncode(folderModel.Name)); // Tooltip
            _writer.RenderBeginTag(HtmlTextWriterTag.Li);

            // Folder item is not a link. It is merely used to hide/show the underlying UL tag
            _writer.AddAttribute(HtmlTextWriterAttribute.Href, "#");
            _writer.AddAttribute(HtmlTextWriterAttribute.Style, "margin-left: " + depth * 10 + "px;");
            _writer.RenderBeginTag(HtmlTextWriterTag.A);

            // The expander:
            _writer.AddAttribute(HtmlTextWriterAttribute.Class, "node-toggle");
            _writer.RenderBeginTag(HtmlTextWriterTag.Span);
            _writer.RenderEndTag(); // span

            _writer.Write(HttpUtility.HtmlEncode(folderModel.Name));

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
            _writer.AddAttribute(HtmlTextWriterAttribute.Title, HttpUtility.HtmlEncode(documentModel.Name)); // Tooltip
            _writer.RenderBeginTag(HtmlTextWriterTag.Li);

            //NOTE: We need to correct the backslashes to forward slashes... Thanks Windows...

            string linkPath = Path.Combine(_userNameAndRepoPrefix, documentModel.RelativePath);
            var urlStylePath = linkPath.Replace('\\','/');
            _writer.AddAttribute(HtmlTextWriterAttribute.Href, urlStylePath);
            _writer.AddAttribute(HtmlTextWriterAttribute.Style, "margin-left: " + depth * 10 + "px;");
            _writer.RenderBeginTag(HtmlTextWriterTag.A);

            _writer.Write(HttpUtility.HtmlEncode(documentModel.Name));

            _writer.RenderEndTag(); // a

            base.VisitDocument(documentModel);

            _writer.RenderEndTag(); // li
            _writer.WriteLine();
        }
    }
}
