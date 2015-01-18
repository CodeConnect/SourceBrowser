using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using SourceBrowser.Search.DocumentFields;
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
        private const int HITS_LIMIT = 100;

        private static FSDirectory _directory;

        /// <summary>
        /// Before doing anything with the search index, first make sure there's no existing
        /// write lock on the directory. (This can happen if the application crashes and restarts)
        /// </summary>
        static SearchIndex()
        {
            _directory = FSDirectory.Open(new DirectoryInfo(_luceneDir));

            if (IndexWriter.IsLocked(_directory))
                IndexWriter.Unlock(_directory);

            var lockFilePath = Path.Combine(_luceneDir, "write.lock");

            if (File.Exists(lockFilePath))
                File.Delete(lockFilePath);
        }

        static object _luceneWriteLock = new object();
        public static void AddDeclarationsToIndex(IEnumerable<TokenViewModel> tokenModels)
        {
            lock(_luceneWriteLock)
            {
                using (var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
                using (var writer = new IndexWriter(_directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
                {
                    foreach(var tokenModel in tokenModels)
                    {
                        addDeclarationToIndex(writer, tokenModel);
                    }
                }
            }
        }

        private static void addDeclarationToIndex(IndexWriter writer, TokenViewModel token)
        {
            //remove previous entry
            var searchQuery = new TermQuery(new Term(TokenFields.Id, token.Id));
            writer.DeleteDocuments(searchQuery);

            //add new index entry
            var doc = new Document();

            doc.Add(new Field(TokenFields.Id, token.Id, Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field(TokenFields.Path, token.Path, Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field(TokenFields.Username, token.Username, Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field(TokenFields.Repository, token.Repository, Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field(TokenFields.Name, token.DisplayName ,Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field(TokenFields.FullName, token.FullyQualifiedName, Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field(TokenFields.LineNumber, token.LineNumber.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));

            writer.AddDocument(doc);
        }

        private static TokenViewModel mapDocumentToToken(Document document)
        {
            return new TokenViewModel(
                document.Get(TokenFields.Username),
                document.Get(TokenFields.Repository),
                document.Get(TokenFields.Path),
                document.Get(TokenFields.FullName),
                Convert.ToInt32(document.Get(TokenFields.LineNumber))
                );
        }

        private static IEnumerable<TokenViewModel> MapLuceneToDataList(IEnumerable<ScoreDoc> hits, IndexSearcher searcher)
        {
            return hits.Select(hit => mapDocumentToToken(searcher.Doc(hit.Doc))).ToList();
        }

        private static Query parsePrefixQuery(string searchQuery, QueryParser parser)
        {
            Query query;
            try
            {
                query = parser.Parse(searchQuery.Trim() + "*");
            }
            catch (ParseException)
            {
                query = parser.Parse(QueryParser.Escape(searchQuery.Trim()));
            }
            return query;
        }

        private static Query parseQuery(string searchQuery, QueryParser parser)
        {
            Query query;
            try
            {
                query = parser.Parse(searchQuery.Trim());
            }
            catch (ParseException)
            {
                query = parser.Parse(QueryParser.Escape(searchQuery.Trim()));
            }
            return query;
        }

        public static IEnumerable<TokenViewModel> SearchRepository(string username, string repository, string searchQuery)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException(nameof(username) + " must be provided.");

            if (string.IsNullOrWhiteSpace(repository))
                throw new ArgumentException(nameof(repository) + " must be provided.");

            //If they send an empty query, just return nothing.
            if (string.IsNullOrWhiteSpace(searchQuery))
                return new List<TokenViewModel>();

            //Replace wild-card characters. For now we're not going to let the user
            //construct advanced queries unless it becomes a 'must-have'
            if (string.IsNullOrEmpty(searchQuery.Replace("*", "").Replace("?", "")))
                return new List<TokenViewModel>();

            using (var searcher = new IndexSearcher(_directory, false))
            using (var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
            {
                var userNameParser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, TokenFields.Username, analyzer);
                var usernameQuery = parseQuery(username, userNameParser);

                var repositoryParser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, TokenFields.Repository, analyzer);
                var repositoryQuery = parseQuery(repository, repositoryParser);

                var nameParser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, TokenFields.Name, analyzer);
                var nameQuery = parsePrefixQuery(searchQuery, nameParser);

                //Ensure that it's the user AND the repository AND the query
                var boolQuery = new BooleanQuery();
                boolQuery.Add(usernameQuery, Occur.MUST);
                boolQuery.Add(repositoryQuery, Occur.MUST);
                boolQuery.Add(nameQuery, Occur.MUST);
                    
                var hits = searcher.Search(boolQuery, HITS_LIMIT).ScoreDocs;

                var results = MapLuceneToDataList(hits, searcher);
                return results;
            }
        }
    }
}
