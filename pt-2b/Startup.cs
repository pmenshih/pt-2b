using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(pt_2b.Startup))]
namespace pt_2b
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
