using SourceBrowser.Generator.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceBrowser.Generator.Transformers
{
    /// <summary>
    /// Converts a WorkspaceModel into HTML
    /// </summary>
    public class HtmlTransformer : AbstractWorkspaceVisitor
    {
        private WorkspaceModel _root;
        private string _savePath;
        public HtmlTransformer(WorkspaceModel root, string savePath)
        {
            _root = root;
            _savePath = savePath;
        }

        protected override void VisitFolder(FolderModel folderModel)
        {
            base.VisitFolder(folderModel);
        }

        protected override void VisitDocument(DocumentModel documentModel)
        {
            var path = documentModel.ContainingPath;
            //TODO: Write the HTML to the appropriate path

            base.VisitDocument(documentModel);
        }
    }
}
