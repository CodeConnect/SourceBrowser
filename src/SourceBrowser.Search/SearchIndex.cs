using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
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
            doc.Add(new Field("Lines", lineNumber.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));

            writer.AddDocument(doc);
        }

        private static Query parseQuery(string searchQuery, QueryParser parser)
        {
            Query query;
            try
            {
                query = parser.Parse(searchQuery.Trim());
            }
            catch(ParseException)
            {
                query = parser.Parse(QueryParser.Escape(searchQuery.Trim()));
            }
            return query;
        }

        private static TokenViewModel mapDocumentToToken(Document document)
        {
            return new TokenViewModel(
                document.Get("Id"),
                document.Get("Name"),
                Convert.ToInt32(document.Get("Lines"))
                );
        }

        private static IEnumerable<TokenViewModel> MapLuceneToDataList(IEnumerable<ScoreDoc> hits, IndexSearcher searcher)
        {
            return hits.Select(hit => mapDocumentToToken(searcher.Doc(hit.Doc))).ToList();
        }


        private static IEnumerable<TokenViewModel> search(string searchQuery, string searchField = "")
        {
            if (string.IsNullOrEmpty(searchQuery.Replace("*", "").Replace("?", "")))
                return new List<TokenViewModel>();

            using (var searcher = new IndexSearcher(_directory, false))
            using (var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
            {
                var hitsLimit = 100;

                var parser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, "FullName", analyzer);
                var query = parseQuery(searchQuery, parser);
                var hits = searcher.Search(query, hitsLimit).ScoreDocs;

                var results = MapLuceneToDataList(hits, searcher);
                analyzer.Close();
                return results;
            }
        }
    }
}
