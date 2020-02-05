using Jugnoon.Entity;
using System.Collections.Generic;

namespace Jugnoon.Entity
{
    public class EncodingSettings
    {
        public EncodingSettings()
        { }

        private static string RootPath = SiteConfig.Environment.ContentRootPath;
        public static string FFMPEGPATH = RootPath + "\\wwwroot\\encoder\\ffmpeg-4.1-win64-static\\bin\\ffmpeg.exe";
        public static string MP4BoxPath = RootPath + "\\wwwroot\\encoder\\MP4Box\\MP4Box.exe";

        public static string returnPreset(string presetID)
        {
            var preset = "-s 640x380 -c:v libx264 -preset medium -crf 22 -b:v 500k -b:a 128k -profile:v baseline -level 3.1";
            switch (presetID)
            {
                case "1001":
                    // 360p
                    preset = "-s 640x380 -c:v libx264 -preset medium -crf 22 -b:v 500k -b:a 128k -profile:v baseline -level 3.1";
                    break;
                case "1002":
                    // 480p
                    preset = "-s 854x480 -c:v libx264 -preset fast -crf 22 -b:v 1000k -b:a 128k -profile:v baseline -level 3.1";
                    break;
                case "1003":
                    // 720p
                    preset = "-s 1280x720 -c:v libx264 -preset fast -crf 22 -b:v 5000k -b:a 128k -profile:v baseline -level 3.1";
                    break;
                case "1004":
                    // 1080p
                    preset = "-s 1920x1080 -c:v libx264 -preset fast -crf 22 -b:v 8000k -b:a 128k -profile:v baseline -level 3.1";
                    break;
            }
            return preset;
        }

        public static string returnOutputExtension(string PresetID)
        {
            string outputExtension = ".mp4";
            if (PresetID == "1001" || PresetID == "1002" || PresetID == "1003" || PresetID == "1004")
                outputExtension = ".mp4";
            return outputExtension;
        }

        // send list of supported transcoding templates to uploader transcoder
        public static List<EncodingOptionModelView> returnEncodingTemplates()
        {
            return new List<EncodingOptionModelView>()
            {
                new EncodingOptionModelView()
                {
                    key = "360p",
                    presetID = "1001",
                    prefix = "-360p.mp4",
                    applyMeta = true,
                    generateThumbnails = false
                },
                new EncodingOptionModelView()
                {
                    key = "480p",
                    presetID = "1002",
                    prefix = "-480p.mp4",
                    applyMeta = true,
                    generateThumbnails = false
                },
                new EncodingOptionModelView()
                {
                    key = "720p",
                    presetID = "1003",
                    prefix = "-720p.mp4",
                    applyMeta = true,
                    generateThumbnails = true
                }/*,
                new EncodingOptionModelView()
                {
                    key = "1080p",
                    presetID = "1004",
                    prefix = "-1080p.mp4",
                    applyMeta = true,
                    generateThumbnails = true
                }*/
            };
        }
    }
}
