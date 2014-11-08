using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SourceBrowser.Site.Startup))]
namespace SourceBrowser.Site
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
