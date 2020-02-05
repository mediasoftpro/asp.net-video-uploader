
using System.Collections.Generic;
/* core classes for handling model views for vUploader */
namespace Jugnoon.Entity
{
    // Uploader Settings Model View
    public class UploaderSettingsModelView
    {
        // if saving media files in user specific directory
        public string username { get; set; } = "";
        // max allowed file size to accept
        public string maxFileSize { get; set; } = "100mb";
        public string extensionTitle { get; set; } = "Video Files";
        // list of allowed file extensions
        public string extensions { get; set; } = "mp4,wmv";
        // backend api path for saving video files
        public string handlerpath { get; set; } = "";
        // backend api path for process transcoding & progress activity
        public string encodingpath { get; set; } = "/api/uploader/encode";
        // max number of allowed files
        public int maxallowedfiles { get; set; } = 0;
    }

    // Supported Encoding Templates Model View
    public class EncodingOptionModelView
    {
        // unique key of template
        public string key { get; set; } = "";
        // preset id (for instruction to execute appropriate commands for publishing videos)
        public string presetID { get; set; } = "";
        // prefix to publish video with (e.g -360p.mp4)
        public string prefix { get; set; } = "";
        // apply meta information on video (in case of mp4 video)
        public bool applyMeta { get; set; } = true;
        // generate thumbnails from video
        public bool generateThumbnails { get; set; } = false;
    }

    /* video uploader template entities */
    public class EncoderSettings
    {
        public string key { get; set; }
        public int tp { get; set; } // action type
        public string pid { get; set; } // process id
        public PublishTemplateSettings template { get; set; }
    } 

    public class PublishTemplateSettings
    {
        public string presetID { get; set; }
        public string prefix { get; set; }
        public string errorText { get; set; }
        public bool applyMeta { get; set; }
        public bool generateThumbnails { get; set; }
    }
}