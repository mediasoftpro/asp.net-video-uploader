## Installation

ASP.NET Video Uploader Script can be easily integrated and used within asp.net core application.

1.  Download ffmpeg windows build from https://ffmpeg.zeranoe.com/builds/
1. Extract and paste complete directory in `/wwwroot/` directory
2. Download mp4box utlity from https://gpac.wp.imt.fr/mp4box/, it can be used to apply meta information to mp4 video that's required to stream videos over the web.
3.  Unzip and paste it in `/wwwroot/` directory.
4. Open Models/EncoderSettings.cs file and adjust FFMPEG and MP4Box Paths 

```csharp
public static string FFMPEGPATH = RootPath + "\\wwwroot\\encoder\\ffmpeg-4.1-win64-static\\bin\\ffmpeg.exe";
public static string MP4BoxPath = RootPath + "\\wwwroot\\encoder\\MP4Box\\MP4Box.exe"
```

5. Make sure to your appropriate (Write / Execute) permission to FFMPEG / MP4Box and Contents Directories where published media files will be saved.

e.g Sample integration of uploader component with basic usage within your application.

```html
 @{
        // upload settings
        var uploadsettingsModelView = new Jugnoon.Entity.UploaderSettingsModelView()
        {
            maxFileSize = "10mb",
            extensions = "mp4,avi,wmv,flv,mpeg,mpg",
            handlerpath = "/api/uploader/upload",
            maxallowedfiles = 3
        };
        // list of supported encoding options
        // please adjust the list with your requirements
        var encodingTemplates = Jugnoon.Entity.EncodingSettings.returnEncodingTemplates();
    }
    <partial name="~/Views/Home/uploader/UploaderComponent.cshtml" />
```

