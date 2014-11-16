using Newtonsoft.Json.Linq;

namespace SourceBrowser.SolutionRetriever
{
    public interface IInformationRetriever
    {
        bool TryUpdateUserInformation(string userName);
        bool TryUpdateRepoInformation(string userName, string repoName);
        JObject GetUserInformation(string userName);
        JObject GetRepoInformation(string userName, string repoName);
    }
}

