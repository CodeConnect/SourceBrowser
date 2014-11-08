using Newtonsoft.Json.Linq;

namespace SourceBrowser.Site.Extensions
{
    public static class JsonExtensions
    {
        /// <summary>
        /// Creates a branch of a tree view
        /// </summary>
        /// <param name="node"></param>
        /// <param name="breadcrumbPath">This controls which branches are expanded or collapsed</param>
        /// <param name="filePath">This indicates the file</param>
        /// <returns></returns>
        public static string GetHtmlTreeView(this JObject node, string breadcrumbPath, string filePath)
        {
            string html = string.Empty;

            var currentNode = node["Children"] as JObject;

            if (currentNode.Count > 0)
            {
                html += "<ul>";

                foreach (var child in currentNode)
                {
                    var tempPath = "/Browse/" + breadcrumbPath;
                    if (tempPath.StartsWith(filePath + child.Key))
                    {
                        html += "<li class='node'>";
                    }
                    else
                    {
                        html += "<li class='node collapsed'>";
                    }
                    if (!child.Value.HasValues)
                    {
                        html += "<a href='" + filePath + child.Key + "'>" + child.Key + "</a>";
                    }
                    else
                    {
                        html += "<a href='#'><span class='node-toggle'></span>" + child.Key +"</a>";

                        var value = child.Value as JObject;
                        var newPath = filePath + child.Key + '/';
                        html += value.GetHtmlTreeView(breadcrumbPath, newPath);
                    }
                    html += "</li>";
                }

                html += "</ul>";
            }

            return html;
        }

    }
}