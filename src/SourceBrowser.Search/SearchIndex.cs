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
        private static string basePath = System.Web.Hosting.HostingEnvironment.MapPath("~") ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        private static string _luceneDir =  Path.Combine(basePath, "luceneIndex");
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
                addDeclarationToIndex(writer, token);
            }
        }

        private static void addDeclarationToIndex(IndexWriter writer, TokenViewModel token)
        {
            //remove previous entry
            var searchQuery = new TermQuery(new Term("Id", token.DocumentId));
            writer.DeleteDocuments(searchQuery);

            //add new index entry
            var doc = new Document();

            doc.Add(new Field("Id", token.DocumentId, Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("Username", token.Username, Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field("Repository", token.Repository, Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field("Name", token.FullName, Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field("Lines", token.LineNumber.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));

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
                document.Get("Username"),
                document.Get("Repository"),
                document.Get("Id"),
                document.Get("Name"),
                Convert.ToInt32(document.Get("Lines"))
                );
        }

        private static IEnumerable<TokenViewModel> MapLuceneToDataList(IEnumerable<ScoreDoc> hits, IndexSearcher searcher)
        {
            return hits.Select(hit => mapDocumentToToken(searcher.Doc(hit.Doc))).ToList();
        }


        public static IEnumerable<TokenViewModel> SearchRepository(string username, string repository, string searchQuery)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException(nameof(username) + " must be provided.");

            if (string.IsNullOrWhiteSpace(repository))
                throw new ArgumentException(nameof(repository) + " must be provided.");

            if (string.IsNullOrEmpty(searchQuery.Replace("*", "").Replace("?", "")))
                return new List<TokenViewModel>();

            using (var searcher = new IndexSearcher(_directory, false))
            using (var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
            {
                var usernameQuery = new TermQuery(new Term("Username", username));
                var repositoryQuery = new TermQuery(new Term("Repository", repository));

                var boolQuery = new BooleanQuery();
                boolQuery.Add(usernameQuery, Occur.MUST);
                boolQuery.Add(repositoryQuery, Occur.MUST);
                    
                var hitsLimit = 100;

                var parser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, "Name", analyzer);

                var query = parseQuery(searchQuery, parser);
                boolQuery.Add(query, Occur.MUST);
                var hits = searcher.Search(query, hitsLimit).ScoreDocs;

                var results = MapLuceneToDataList(hits, searcher);
                analyzer.Close();
                return results;
            }
        }
    }
}
