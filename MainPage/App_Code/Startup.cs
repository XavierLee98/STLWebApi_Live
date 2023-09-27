using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(LoginScreen.Startup))]
namespace LoginScreen
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);
        }
    }
}
