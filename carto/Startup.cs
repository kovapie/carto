using Microsoft.Owin;
using Owin;


[assembly:OwinStartup(typeof(carto.Startup))]

namespace carto
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}