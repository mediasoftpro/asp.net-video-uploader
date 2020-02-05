
namespace Jugnoon.Entity
{
    public class UrlConfig
    {
        public static string Source_Video_Path()
        {
            return Upload_Path("source");
        }

        public static string Source_Video_Path(string username)
        {
            return Upload_Path("source");
        }
        public static string Source_Video_Url(string username)
        {
            return Upload_URL("source");
        }

        public static string Published_Video_Path()
        {
            return Upload_Path("published");
        }
        public static string Published_Video_Path(string username)
        {
            return Upload_Path("published");
        }

        public static string Thumbs_Path()
        {
            return Upload_Path("thumbnails");
        }
        public static string Thumbs_Path(string username)
        {
            return Upload_Path("thumbnails");
        }

        public static string Thumb_Url(string username)
        {
            return Upload_URL("thumbnails");
        }

        public static string Upload_Path(string foldername)
        {
            return SiteConfig.Environment.ContentRootPath + "/wwwroot/uploads/" + foldername;
        }

        public static string Upload_URL(string foldername)
        {
            return "/uploads/" + foldername;
        }
    }
}