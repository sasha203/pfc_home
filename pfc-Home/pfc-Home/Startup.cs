using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(pfc_Home.Startup))]
namespace pfc_Home
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
