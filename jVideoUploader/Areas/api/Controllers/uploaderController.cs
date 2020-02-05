using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using Jugnoon.Entity;
using Newtonsoft.Json;
using Jugnoon.Videos;

namespace jVideoUploader.Areas.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class uploaderController : ControllerBase
    {
        private static readonly FormOptions _defaultFormOptions = new FormOptions();
        public uploaderController(
            IHostingEnvironment _environment,
            IHttpContextAccessor _httpContextAccessor
        )
        {
            SiteConfig.Environment = _environment;
            SiteConfig.HttpContextAccessor = _httpContextAccessor;
        }

        // GET: api/ffmpeg
        // Store all encoding processes in static (shared) list of mediahandler objects
        // Each mediahandler object, control single encoding process accross application
        // static object required to manage each encoding separately in shared environment 
        //where there is chances of concurrent encoding request at a time.
        public static List<MediaHandler> _lst = new List<MediaHandler>();

        //
        // GET: /api/uploader/encode
        [HttpPost("encode")]
        public ActionResult encode()
        {
            var json = new StreamReader(Request.Body).ReadToEnd();
            var data = JsonConvert.DeserializeObject<List<EncoderSettings>>(json);
            int ActionType = data[0].tp;
            var _response = new Dictionary<string, string>();
            _response["encodeoutput"] = "2.0";

            string Source = "";
            string Published = "";
            string ProcessID = "";
            switch (ActionType)
            {
                case 0:
                    // encode video
                    Source = data[0].key;
                    Published = Path.GetFileNameWithoutExtension(data[0].key) + data[0].template.prefix;

                    if (Source != "" && Published != null)
                    {
                        var _mhandler = new MediaHandler();
                        string RootPath = SiteConfig.Environment.ContentRootPath;
                        _mhandler.FFMPEGPath = EncodingSettings.FFMPEGPATH;
                        _mhandler.InputPath = UrlConfig.Upload_Path("source");
                        _mhandler.OutputPath = UrlConfig.Upload_Path("published");
                        _mhandler.BackgroundProcessing = true;
                        _mhandler.FileName = Source;
                        _mhandler.OutputFileName = Published.Replace(Path.GetExtension(Published), "");
                        _mhandler.Parameters = EncodingSettings.returnPreset(data[0].template.presetID); //"-s 640x380 -c:v libx264 -preset medium -crf 22 -b:v 500k -b:a 128k -profile:v baseline -level 3.1"; // Site_Settings.MP4_480p_Settings;
                        _mhandler.OutputExtension = EncodingSettings.returnOutputExtension(data[0].template.presetID); // ".mp4";
                        _mhandler.vinfo = _mhandler.ProcessMedia();
                        if (_mhandler.vinfo.ErrorCode > 0)
                        {
                            // remove file if failed to publish properly
                            if (System.IO.File.Exists(RootPath + "/" + _mhandler.InputPath))
                                System.IO.File.Delete(RootPath + "/" + _mhandler.InputPath);

                            _response["encodeoutput"] = "2.0";
                            _response["ecode"] = _mhandler.vinfo.ErrorCode.ToString();
                            _response["edesc"] = _mhandler.vinfo.FFMPEGOutput.ToString();
                            
                            var _message = new System.Text.StringBuilder();
                            _message.Append("<h4>Video Upload Error</h4>");
                            _message.Append("<p>Error:" + _mhandler.vinfo.ErrorCode + " _ _ " + _mhandler.vinfo.ErrorMessage + "</p>");
                            _message.Append("<p>Source FileName: " + Source);
                            _message.Append("<p>Published FileName: " + Published);

                            return Ok(_response);
                        }
                        else
                        {
                            // _mhandler.vinfo.ProcessID = Guid.NewGuid().ToString(); // unique guid to attach with each process to identify proper object on progress bar and get info request
                            _lst.Add(_mhandler);
                            _response["encodeoutput"] = "2.0";
                            _response["ecode"] = _mhandler.vinfo.ErrorCode.ToString();
                            _response["procid"] = _mhandler.vinfo.ProcessID; // _mhandler.vinfo.ProcessID;
                            return Ok(_response);
                        }
                    }
                    break;
                case 1:
                    // get progress status
                    ProcessID = data[0].pid;
                    if (ProcessID != "")
                    {
                        string completed_process = "0";
                        if (_lst.Count > 0)
                        {
                            int i = 0;
                            for (i = 0; i <= _lst.Count - 1; i++)
                            {
                                if (_lst[i].vinfo.ProcessID == ProcessID)
                                {
                                    completed_process = Math.Round(_lst[i].vinfo.ProcessingCompleted, 2).ToString();
                                }
                            }
                        }

                        _response["encodeoutput"] = "2.0";
                        _response["status"] = completed_process;
                        return Ok(_response);
                    }

                    break;
                case 2:
                    // get information
                    ProcessID = data[0].pid;
                    Published = Path.GetFileNameWithoutExtension(data[0].key) + data[0].template.prefix;
                    if (ProcessID != "")
                    {
                        if (_lst.Count > 0)
                        {
                            int i = 0;
                            for (i = 0; i <= _lst.Count - 1; i++)
                            {
                                if (_lst[i].vinfo.ProcessID == ProcessID)
                                {
                                    _response["status"] = "OK";
                                    _response["ecode"] = _lst[i].vinfo.ErrorCode.ToString();
                                    _response["fname"] = Published;

                                    _response["dur"] = _lst[i].vinfo.Duration.ToString();
                                    _response["dursec"] = _lst[i].vinfo.Duration_Sec.ToString();


                                    // remove from list of corrent processes if processes reach this point
                                    // store all information of completed process and remove it from list of concurrent processes
                                    // e.g
                                    VideoInfo current_uploaded_video_info = _lst[i].vinfo;
                                    _lst.Remove(_lst[i]);

                                    // Validation 
                                    int plength = 0;
                                    var pub_file = Path.GetFileNameWithoutExtension(data[0].key) + data[0].template.prefix;
                                    string path = UrlConfig.Upload_Path("source") + "\\" + pub_file;
                                    if (System.IO.File.Exists(path))
                                    {
                                        FileInfo flv_info = new FileInfo(path);
                                        plength = (int)flv_info.Length;
                                    }
                                    if (plength == 0)
                                    {
                                        var _message = new System.Text.StringBuilder();
                                        _message.Append("<h4>Video Publishing Error</h4>");
                                        _message.Append("<p>Error: 0kb file generated</p>");
                                        _message.Append("<p>Source FileName: " + Source);
                                        _message.Append("<p>Published FileName: " + Published);
                                    }

                                    // ii: add meta information to mp4 video
                                    if (data[0].template.applyMeta)
                                    {
                                        try
                                        {
                                            var mp4med = new MediaHandler();
                                            mp4med.MP4BoxPath = EncodingSettings.MP4BoxPath;
                                            string _mp4_temp_path = "\"" + UrlConfig.Upload_Path("source") + "/" + pub_file + "\"";
                                            string meta_filename = data[0].key.Replace(".mp4", "_meta.mp4");
                                            mp4med.Parameters = "-isma -hint -add " + _mp4_temp_path + "";
                                            mp4med.FileName = meta_filename;
                                            mp4med.InputPath = UrlConfig.Upload_Path("source");
                                            mp4med.Set_MP4_Buffering();

                                            // check whether file created
                                            string pubPath = UrlConfig.Upload_Path("source");
                                            if (System.IO.File.Exists(pubPath + "\\" + meta_filename))
                                            {
                                                // remove temp mp4 file
                                                if (System.IO.File.Exists(pubPath + "" + pub_file))
                                                    System.IO.File.Delete(pubPath + "\\" + pub_file);

                                                _response["fname"] = meta_filename;
                                            }
                                            else
                                            {
                                                // file not created by mp4box
                                                // rename published mp4 as _meta.mp4
                                                System.IO.File.Move(pubPath + "\\" + pub_file, pubPath + "\\" + meta_filename);
                                                _response["fname"] = meta_filename;
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            var _message = new System.Text.StringBuilder();
                                            _message.Append("<h4>Video Meta Information Error</h4>");
                                            _message.Append("<p>Error: " + ex.Message + "</p>");
                                            _message.Append("<p>Source FileName: " + Source);
                                            _message.Append("<p>Published FileName: " + Published);
                                        }
                                    }

                                    _response["isenable"] = "1";

                                    // Thumb Grabbing Script
                                    if (data[0].template.generateThumbnails)
                                    {
                                        string thumb_start_index = "";
                                        try
                                        {
                                            var med = new MediaHandler();
                                            med.FFMPEGPath = EncodingSettings.FFMPEGPATH;
                                            med.InputPath = UrlConfig.Upload_Path("source"); // RootPath + "\\" + SourcePath;
                                            med.OutputPath = UrlConfig.Upload_Path("thumbnails"); // RootPath + "\\" + PublishedPath;
                                            med.FileName = data[0].key; // source file
                                            thumb_start_index = med.FileName.Replace(Path.GetExtension(med.FileName), "_");
                                            med.Image_Format = "jpg";
                                            med.VCodec = "image2"; //optional
                                            med.ACodec = "";
                                            med.ImageName = thumb_start_index;
                                            med.Multiple_Thumbs = true;
                                            med.ThumbMode = 0;
                                            med.No_Of_Thumbs = 15;
                                            med.Thumb_Start_Position = 5; // start grabbing thumbs from 5th second
                                                                          //if (this.BackgroundProcessing)
                                                                          //    med.BackgroundProcessing = true;
                                            int width = SiteSettings.Width;
                                            if (width > 0)
                                                med.Width = width;
                                            int height = SiteSettings.Height;
                                            if (height > 0)
                                                med.Height = height;
                                            var tinfo = med.Grab_Thumb();
                                            if (tinfo.ErrorCode > 0)
                                            {
                                                // Error occured in grabbing thumbs - Rollback process
                                                _response["ecode"] = "1006";
                                                _response["edesc"] = "Grabbing thumbs from video failed";

                                                var _message = new System.Text.StringBuilder();
                                                _message.Append("<h4>Thumb Generation Error</h4>");
                                                _message.Append("<p>Error: " + _response["edesc"] + "</p>");
                                                _message.Append("<p>Source FileName: " + Source);
                                                _message.Append("<p>Published FileName: " + Published);

                                                // call rollback script here
                                                return Ok(_response);
                                            }

                                            // Validate Thumbs
                                            path = UrlConfig.Upload_Path("thumbnails") + "/" + thumb_start_index;
                                            if (!System.IO.File.Exists(path + "004.jpg") || !System.IO.File.Exists(path + "008.jpg") || !System.IO.File.Exists(path + "011.jpg"))
                                            {
                                                // thumb failed try again grabbing thumbs from published video
                                                med.InputPath = UrlConfig.Upload_Path("published");
                                                med.FileName = pub_file; // grab thumb from encoded video
                                                tinfo = med.Grab_Thumb();
                                                if (tinfo.ErrorCode > 0)
                                                {
                                                    // Error occured in grabbing thumbs - Rollback process
                                                    _response["ecode"] = "1006";
                                                    _response["edesc"] = "Grabbing thumbs from video failed";
                                                    // rollback script here
                                                    var _message = new System.Text.StringBuilder();
                                                    _message.Append("<h4>Thumb Generation Error</h4>");
                                                    _message.Append("<p>Error: " + _response["edesc"] + "</p>");
                                                    _message.Append("<p>Source FileName: " + Source);
                                                    _message.Append("<p>Published FileName: " + Published);
                                                    return Ok(_response);
                                                }
                                                // Disable Video
                                                if (!System.IO.File.Exists(path + "004.jpg") || !System.IO.File.Exists(path + "008.jpg") || !System.IO.File.Exists(path + "011.jpg"))
                                                {
                                                    _response["isenable"] = "0"; // disable video - thumbs not grabbed properly.
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            _response["ecode"] = "1010";
                                            _response["edesc"] = ex.Message;

                                            var _message = new System.Text.StringBuilder();
                                            _message.Append("<h4>Thumb Generation Error</h4>");
                                            _message.Append("<p>Error: " + ex.Message + "</p>");
                                            _message.Append("<p>Source FileName: " + Source);
                                            _message.Append("<p>Published FileName: " + Published);
                                        }

                                        _response["tfile"] = thumb_start_index + "" + "008.jpg";
                                        _response["fIndex"] = thumb_start_index;
                                        _response["img_url"] = "/uploads/thumbnails/"; // + _response["tfile"]);
                                    }
                                    else
                                    {
                                        // generate thumbnail is disabled
                                        _response["tfile"] = "";
                                        _response["fIndex"] = "";
                                        _response["img_url"] = "";
                                    }
                                }
                            }
                        }
                        return Ok(_response);
                    }

                    break;
                case 3:
                    // final check
                    ProcessID = data[0].pid;
                    Source = data[0].key;
                    Published = Path.GetFileNameWithoutExtension(data[0].key) + data[0].template.prefix;

                    if (ProcessID != "" && Source != "" && Published != "")
                    {
                        if (_lst.Count > 0)
                        {
                            int i = 0;
                            for (i = 0; i <= _lst.Count - 1; i++)
                            {
                                if (_lst[i].vinfo.ProcessID == ProcessID)
                                {
                                    if (_lst[i].vinfo.ProcessingCompleted >= 100)
                                    {
                                        // check whether published file uploaded properly
                                        string publishedPath = UrlConfig.Upload_Path("published");

                                        if (!System.IO.File.Exists(publishedPath + "/" + Published))
                                        {
                                            _response["status"] = "INVALID";// published file not found
                                        }
                                        else
                                        {
                                            _response["encodeoutput"] = "2.0";
                                            _response["status"] = "OK";
                                        }
                                    }
                                }
                            }
                        }

                        return Ok(_response);
                    }
                    break;
            }
            _response["status"] = "INVALID";
            return Ok(_response);
        }


        public static string GetErrorCode(string ProcessID)
        {
            string ErrorCode = "0";
            if (_lst.Count > 0)
            {
                int i = 0;
                for (i = 0; i <= _lst.Count - 1; i++)
                {
                    if (_lst[i].vinfo.ProcessID == ProcessID)
                    {
                        ErrorCode = _lst[i].vinfo.ErrorCode.ToString();
                    }
                }
            }

            return ErrorCode;
        }

        public static string GetFFMPEGOutPut(string ProcessID)
        {
            string Output = "";
            if (_lst.Count > 0)
            {
                int i = 0;
                for (i = 0; i <= _lst.Count - 1; i++)
                {
                    if (_lst[i].vinfo.ProcessID == ProcessID)
                    {
                        Output = _lst[i].vinfo.FFMPEGOutput.ToString();
                    }
                }
            }

            return Output;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> upload()
        {
            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
            {
                return BadRequest($"Expected a multipart request, but got {Request.ContentType}");
            }

            StringValues UserName;
            SiteConfig.HttpContextAccessor.HttpContext.Request.Headers.TryGetValue("UName", out UserName);

            // Used to accumulate all the form url encoded key value pairs in the 
            // request.
            var formAccumulator = new KeyValueAccumulator();
            // string targetFilePath = null;

            var boundary = MultipartRequestHelper.GetBoundary(
                  MediaTypeHeaderValue.Parse(Request.ContentType),
                  _defaultFormOptions.MultipartBoundaryLengthLimit);

            var reader = new MultipartReader(boundary, HttpContext.Request.Body);

            var section = await reader.ReadNextSectionAsync();

            var uploadPath = SiteConfig.Environment.ContentRootPath + "/wwwroot/uploads/source/";
            
            var fileName = "";
            while (section != null)
            {
                ContentDispositionHeaderValue contentDisposition;
                var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition,
                    out contentDisposition);

                if (hasContentDispositionHeader)
                {
                    if (MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
                    {
                        var output = formAccumulator.GetResults();
                        var chunk = "0";
                        foreach (var item in output)
                        {
                            if (item.Key == "name")
                                fileName = item.Value;
                            else if (item.Key == "chunk")
                                chunk = item.Value;
                        }

                        var Path = uploadPath + "" + fileName;
                        using (var fs = new FileStream(Path, chunk == "0" ? FileMode.Create : FileMode.Append))
                        {
                            await section.Body.CopyToAsync(fs);
                            fs.Flush();
                        }
                    }
                    else if (MultipartRequestHelper.HasFormDataContentDisposition(contentDisposition))
                    {
                        var key = HeaderUtilities.RemoveQuotes(contentDisposition.Name);
                        var encoding = GetEncoding(section);
                        using (var streamReader = new StreamReader(
                            section.Body,
                            encoding,
                            detectEncodingFromByteOrderMarks: true,
                            bufferSize: 1024,
                            leaveOpen: true))
                        {
                            // The value length limit is enforced by MultipartBodyLengthLimit
                            var value = await streamReader.ReadToEndAsync();
                            if (String.Equals(value, "undefined", StringComparison.OrdinalIgnoreCase))
                            {
                                value = String.Empty;
                            }
                            formAccumulator.Append(key.ToString(), value);

                            if (formAccumulator.ValueCount > _defaultFormOptions.ValueCountLimit)
                            {
                                throw new InvalidDataException($"Form key count limit {_defaultFormOptions.ValueCountLimit} exceeded.");
                            }
                        }
                    }
                }

                var result = formAccumulator.GetResults();

                // Drains any remaining section body that has not been consumed and
                // reads the headers for the next section.
                section = await reader.ReadNextSectionAsync();
            }


            string url = "/uploads/source/" + fileName;
            string fileType = System.IO.Path.GetExtension(fileName);
            string fileIndex = fileName.Replace(fileType, "");

            return Ok(new { jsonrpc = "2.0", result = "OK", fname = fileName, url = url, filetype = fileType, filename = fileName, fileIndex = fileIndex });
        }

        private static Encoding GetEncoding(MultipartSection section)
        {
            MediaTypeHeaderValue mediaType;
            var hasMediaTypeHeader = MediaTypeHeaderValue.TryParse(section.ContentType, out mediaType);
            // UTF-7 is insecure and should not be honored. UTF-8 will succeed in 
            // most cases.
            if (!hasMediaTypeHeader || Encoding.UTF7.Equals(mediaType.Encoding))
            {
                return Encoding.UTF8;
            }
            return mediaType.Encoding;
        }

    }
}