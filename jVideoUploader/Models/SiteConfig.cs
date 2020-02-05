using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Jugnoon.Entity
{
    public class SiteConfig
    {
        public static IHostingEnvironment Environment { get; set; }
        public static IHttpContextAccessor HttpContextAccessor { get; set; }
    }

    public class SiteSettings
    {
        // width of video thumbnail
        public static int Width { get; set; } = 800;
        // height of video thumbnail
        public static int Height { get; set; } = 600;
    }

}
