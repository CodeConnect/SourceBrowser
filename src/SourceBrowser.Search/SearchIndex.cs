using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using SourceBrowser.Search.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceBrowser.Search
{
    public class SearchIndex
    {
        private static string _luceneDir = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~"), "luceneIndex");
        private static FSDirectory _directoryTemp;
        
        private static FSDirectory _directory
        {
            get
            {
                if (_directoryTemp == null)
                    _directoryTemp = FSDirectory.Open(new DirectoryInfo(_luceneDir));

                if (IndexWriter.IsLocked(_directoryTemp))
                    IndexWriter.Unlock(_directoryTemp);

                var lockFilePath = Path.Combine(_luceneDir, "write.lock");

                if (File.Exists(lockFilePath))
                    File.Delete(lockFilePath);

                return _directoryTemp;
            }
        }

        public static void AddDeclarationToIndex(TokenViewModel token)
        {
            using (var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
            using (var writer = new IndexWriter(_directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                addDeclarationToIndex(writer, token.DocumentId, token.FullName, token.LineNumber);
            }
        }

        private static void addDeclarationToIndex(IndexWriter writer, string documentId, string fullName, int lineNumber)
        {
            //remove previous entry
            var searchQuery = new TermQuery(new Term("Id", documentId));
            writer.DeleteDocuments(searchQuery);

            //add new index entry
            var doc = new Document();

            doc.Add(new Field("Id", documentId, Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("Name", fullName, Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field("Line", lineNumber.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));

            writer.AddDocument(doc);
        }
    }
}
