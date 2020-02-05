using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.IO;
using System.Collections;

/// <summary>
/// Video converter class that can be used to transcode video from one format to another format.
/// </summary>
namespace Jugnoon.Videos
{
    public class MediaHandler
    {
        // Error Codes
        // 100 - > ffmpeg.exe is not found, path is not validated where ffmpeg.exe exist.
        // 101 - > Source Video File not found
        // 102 - > Output directory ! exist
        // 104 - > Unknown Options -> Main Reasons -> "Unknown format, Unsupported codec, Unknown encoder"
        // 105 - > No such file or directory found
        // 106 - > Error while opening codecs
        // 107 - > Video processing Failed.
        // 108 - > watermark image ! found
        // 109 - > source video extension ! found
        // 110 - > video processing failed.
        // 111 - > Invalid video format - format not supported
        // 112 - > Thumb Directory not found
        // 113 - > FLVTOOL2.exe is not found
        // 114 - > Custom FFMPEG command not supplied
        // 115 - > Unrecognized option, most used while using  build instead of shared build
        // 116 - > Could not open source video
        // 117 - > Permission Denied / Occurs in case of setting Meta Information
        // 118 - > Video Codec not compatible with encoded video format.
        // 119 - > Error while retrieving audio information from video.
        // 120 - > Error while retrieving video information from video.
        // 121 - > General video processing or video information processing error
        // 122 - > total time of all clips plus clip time exceeds from original video time.
        // 123 - > you must setup time between two thumbs in seconds as 1,2,3,4,5,6,7,8,9,10,20 while capturing multiple thumbs
        // 124 - > your thumb settings exceeds from available video duration.
        // 125 - > bad parameter set, please verify all parameters that you set.
        // 126 - > Failed to add vhook information
        // 127 - > Video successfully encoded but failed to set meta information for video.
        // 128 - > Error while opening output stream. Maybe incorrect parameters such as bit_rate, rate, width or height. 
        // 129 - > License of component expired
        // 130 - > Incorrect Parameters (when some parameters provided invalide with video type)
        // 131 - > Unknown video format.
        // 132 - > Unknown video encoder.
        // 133 - > Unknown codec.
        // 134 - > incorrect parameters / Error while opening encoder for output stream.
        // 135 - > vhook information invalid, may be space in watermark image path, or watermark.dll path.
        // 136 - > vhook / watermark.dll path invalid. make sure that you use shared ffmpeg build with vhook support.
        // 137 - > space not allowed in watermark image path.
        // 138 - > Join Video: Please specify filenames.
        // 139 - > Join Video: Input file path validation failed, please make sure that all joining clips / files must be located in input path directory.
        // 140 - > Must specify output media type (OutputExtension)
        // 141 - > Output File Name must specifiy, e.g "sample.avi" or "sample", extension will skip if specified.
        // 142 - > Join Video: Failed to create temp mpg files for attaching videos.
        // 143 - > Join Video: Must contain two or more clips.
        // 144 - > preset file: ffpreset file not found
        // 145 - > Unable to parse value -/ undefined constant or missing value. occurs mostly while sending custom parameters through _obj.Parameters property
        // 146 - > Incorrect codec parameters e.g creating .gif file from .avi file
        // 147 - > mp4box path invalid
        // 148 -> Padding Error (not within the padded area)


        // General Properties

        public MediaHandler()
        {
        }

        #region private PROPERTIES

        // license value-> 0 shows validation on.
        private int _disable_license_validation = 1;

        //private  DateTime _expirydate = new DateTime(2009, 7,10, 9, 30, 0, 0);
        //private  DateTime _lowerexpirydate = new DateTime(2009, 6, 10, 9, 30, 0, 0);
        private DateTime _expirydate = DateTime.Parse("07/07/2009");
        private DateTime _lowerexpirydate = DateTime.Parse("06/06/2009");
        private int _exitprocess = 30000;
        // REQUIRED PROPERTIES
        private string _servicepath = "";
        private string _ffmpegpath = "";
        private string _flvtoolpath = "";
        private string _inputpath = "";
        private string _outputpath = "";
        private string _presetpath = "";
        private string _mp4boxpath = "";
        private string _filename = "";

        // OPTIONAL PROPERTIES
        private string _outputfilename = "";
        private int _width = 0;
        private int _height = 0;
        private double _video_bitrate = 0;
        private double _audio_bitrate = 0;
        private int _audio_samplingrate = 0;
        private double _framerate = 0;
        private string _duration = "";
        private bool _deinterlace = false;
        private bool _maxquality = false;
        private string _vcodec = "";
        private string _acodec = "";
        private bool _disableaudio = false;
        private bool _disablevideo = false;
        private int _channels = 0; // 1 or 2
        private string _aspectratio = "";
        private string _targetfiletype = "";
        private string _mp4videotype = "normal";
        private string _customcommand = "";
        private string _force = "";
        private string _startpoint = "";
        private string _extension = "";
        private int _limit_size = 0;
        private int _pass = 0;
        private int _quality_scale = 0;
        private string[] _filenames;
        private int _thread = 0;
        private string _initialCmmands = "";
        private bool _skipinput = false;
        // THUMBS PROPERTIES
        private string _imagename = "";
        private string _frametime = "";
        private string _image_format = "jpg";
        private int _no_of_thumbs = 0;
        private double _frame_time = 1;
        private int _frame_start_position = 0;
        private bool _grab_multiple_thumbs = false;
        private bool _auto_transition = false;
        private int _thumb_mode = 0; //0: normal 1: // fast


        // WATERMARK PROPERTIES
        private string _watermarkpath = "";
        private string _watermarkimage = "";

        // PADDING PROPERTIES
        private int _padtop = 0;
        private int _padbottom = 0;
        private int _padright = 0;
        private int _padleft = 0;
        private string _padcolor = "";

        // CROPPING PROPERTIES
        private int _croptop = 0;
        private int _cropbottom = 0;
        private int _cropright = 0;
        private int _cropleft = 0;

        // Special Parameters
        private string _params = "";
        private string _fistPassParams = "";
        private string _firstpassoutput = "";

        #endregion

        #region public PROPERTIES
        // public  PROPERTIES
        public int ExitProcess
        {
            get
            {
                return _exitprocess;
            }
            set
            {
                _exitprocess = value;
            }
        }

        public string ServicePath
        {
            get
            {
                return _servicepath;
            }
            set
            {
                _servicepath = value;
            }
        }

        public string FFMPEGPath
        {
            get
            {
                return _ffmpegpath;
            }
            set
            {
                _ffmpegpath = value;
            }
        }

        public string MP4BoxPath
        {
            get
            {
                return _mp4boxpath;
            }
            set
            {
                _mp4boxpath = value;
            }
        }


        public string FLVToolPath
        {
            get
            {
                return _flvtoolpath;
            }
            set
            {
                _flvtoolpath = value;
            }
        }

        // Path that reference source video directory
        public string InputPath
        {
            get
            {
                return _inputpath;
            }
            set
            {
                _inputpath = value;
            }
        }

        // Path that reference encoded video directory
        public string OutputPath
        {
            get
            {
                return _outputpath;
            }
            set
            {
                _outputpath = value;
            }
        }

        public string PresetPath
        {
            get
            {
                return _presetpath;
            }
            set
            {
                _presetpath = value;
            }
        }

        // Name of source video with complete extension e.g abc.avi
        public string FileName
        {
            get
            {
                return _filename;
            }
            set
            {
                _filename = value;
            }
        }

        // Name of encoded video with complete extension e.g abc.avi
        public string OutputFileName
        {
            get
            {
                return _outputfilename;
            }
            set
            {
                _outputfilename = value;
            }
        }

        // Set output extension of video
        public string OutputExtension
        {
            get
            {
                return _extension;
            }
            set
            {
                _extension = value;
            }
        }

        public int Width
        {
            get
            {
                return _width;
            }
            set
            {
                _width = value;
            }
        }

        public int Height
        {
            get
            {
                return _height;
            }
            set
            {
                _height = value;
            }
        }

        public double Video_Bitrate
        {
            get
            {
                return _video_bitrate;
            }
            set
            {
                _video_bitrate = value;
            }
        }

        public double Audio_Bitrate
        {
            get
            {
                return _audio_bitrate;
            }
            set
            {
                _audio_bitrate = value;
            }
        }

        public int Audio_SamplingRate
        {
            get
            {
                return _audio_samplingrate;
            }
            set
            {
                _audio_samplingrate = value;
            }
        }

        public double FrameRate
        {
            get
            {
                return _framerate;
            }
            set
            {
                _framerate = value;
            }
        }


        public int Limit_File_Size
        {
            get
            {
                return _limit_size;
            }
            set
            {
                _limit_size = value;
            }
        }
        public string InitialCommands
        {
            get
            {
                return _initialCmmands;
            }
            set
            {
                _initialCmmands = value;
            }
        }

        public int Pass
        {
            get
            {
                return _pass;
            }
            set
            {
                _pass = value;
            }
        }

        public int Thread
        {
            get
            {
                return _thread;
            }
            set
            {
                _thread = value;
            }
        }

        public int Scale_Quality
        {
            get
            {
                return _quality_scale;
            }
            set
            {
                _quality_scale = value;
            }
        }

        // Duration default infinite, if you set that option, it will restrict video to certain limit, e.g encode only 2 minute video.
        public string Duration
        {
            get
            {
                return _duration;
            }
            set
            {
                _duration = value;
            }
        }

        public bool Deinterlace
        {
            get
            {
                return _deinterlace;
            }
            set
            {
                _deinterlace = value;
            }
        }

        // Setting maxquality true enforce highest quality setting for video encoding and discard video birate and other parameters in action.
        public bool MaxQuality
        {
            get
            {
                return _maxquality;
            }
            set
            {
                _maxquality = value;
            }
        }

        // Time of frame from where you want to caputure thumb in video, format supported : xxx , ss:mm:hr
        public string Frame_Time
        {
            get
            {
                return _frametime;
            }
            set
            {
                _frametime = value;
            }
        }

        public string ImageName
        {
            get
            {
                return _imagename;
            }
            set
            {
                _imagename = value;
            }
        }

        // Format of image to grab and save image, jpg, png is recommended
        public string Image_Format
        {
            get
            {
                return _image_format;
            }
            set
            {
                _image_format = value;
            }
        }

        // Properties related with grabbing multiple thumbs
        public int No_Of_Thumbs
        {
            set
            {
                _no_of_thumbs = value;
            }
            get
            {
                return _no_of_thumbs;
            }

        }

        // Property related to setting multiple thumb grabbing mode.
        // 0: Normal
        // 1: Fast
        public int ThumbMode
        {
            set { _thumb_mode = value; }
            get { return _thumb_mode; }
        }

        public double Thumb_Transition_Time
        {
            set
            {
                _frame_time = value;
            }
            get
            {
                return _frame_time;
            }
        }

        public int Thumb_Start_Position
        {
            set
            {
                _frame_start_position = value;
            }
            get
            {
                return _frame_start_position;
            }
        }

        public bool Multiple_Thumbs
        {
            set
            {
                _grab_multiple_thumbs = value;
            }
            get
            {
                return _grab_multiple_thumbs;
            }
        }

        public bool Auto_Transition_Time
        {
            set
            {
                _auto_transition = value;
            }
            get
            {
                return _auto_transition;
            }
        }


        // Watermark.dll path - Normally available in ffmpeg/vhook directory in shared ffmpeg build
        public string WaterMarkPath
        {
            get
            {
                return _watermarkpath;
            }
            set
            {
                _watermarkpath = value;
            }
        }

        // Path of image that is used as watermark on video ( transparent gif file ):
        public string WaterMarkImage
        {
            get
            {
                return _watermarkimage;
            }
            set
            {
                _watermarkimage = value;
            }
        }

        public string VCodec
        {
            get
            {
                return _vcodec;
            }
            set
            {
                _vcodec = value;
            }
        }

        public string ACodec
        {
            get
            {
                return _acodec;
            }
            set
            {
                _acodec = value;
            }
        }

        public bool DisableAudio
        {
            get
            {
                return _disableaudio;
            }
            set
            {
                _disableaudio = value;
            }
        }

        public bool DisableVideo
        {
            get
            {
                return _disablevideo;
            }
            set
            {
                _disablevideo = value;
            }
        }

        public int Channel
        {
            get
            {
                return _channels;
            }
            set
            {
                _channels = value;
            }
        }

        public string AspectRatio
        {
            get
            {
                return _aspectratio;
            }
            set
            {
                _aspectratio = value;
            }
        }

        public string TargetFileType
        {
            get
            {
                return _targetfiletype;
            }
            set
            {
                _targetfiletype = value;
            }
        }

        public string VideoType
        {
            get
            {
                return _mp4videotype;
            }
            set
            {
                _mp4videotype = value;
            }
        }

        public string Force
        {
            get
            {
                return _force;
            }
            set
            {
                _force = value;
            }
        }

        public string Start_Position
        {
            get
            {
                return _startpoint;
            }
            set
            {
                _startpoint = value;
            }
        }

        public string CustomCommand
        {
            get
            {
                return _customcommand;
            }
            set
            {
                _customcommand = value;
            }
        }

        public string Parameters
        {
            get
            {
                return _params;
            }
            set
            {
                _params = value;
            }
        }

        public string Pass1Parameters
        {
            get
            {
                return _fistPassParams;
            }
            set
            {
                _fistPassParams = value;
            }
        }

        public string FirstPassOutput
        {
            get
            {
                return _firstpassoutput;
            }
            set
            {
                _firstpassoutput = value;
            }
        }

        public int PadTop
        {
            get
            {
                return _padtop;
            }
            set
            {
                _padtop = value;
            }
        }

        public int PadBottom
        {
            get
            {
                return _padbottom;
            }
            set
            {
                _padbottom = value;
            }
        }

        public int PadLeft
        {
            get
            {
                return _padleft;
            }
            set
            {
                _padleft = value;
            }
        }

        public int PadRight
        {
            get
            {
                return _padright;
            }
            set
            {
                _padright = value;
            }
        }

        public string PadColor
        {
            get
            {
                return _padcolor;
            }
            set
            {
                _padcolor = value;
            }
        }
        public int CropTop
        {
            get
            {
                return _croptop;
            }
            set
            {
                _croptop = value;
            }
        }

        public int CropBottom
        {
            get
            {
                return _cropbottom;
            }
            set
            {
                _cropbottom = value;
            }
        }

        public int CropLeft
        {
            get
            {
                return _cropleft;
            }
            set
            {
                _cropleft = value;
            }
        }

        public int CropRight
        {
            get
            {
                return _cropright;
            }
            set
            {
                _cropright = value;
            }
        }

        public bool SkipInput
        {
            get
            {
                return _skipinput;
            }
            set
            {
                _skipinput = value;
            }
        }

        public string[] FileNames
        {
            set { _filenames = value; }
            get { return _filenames; }
        }

        #endregion

        #region public METHODS
        // Methods

        // check whether current file is in audio or video format
        public string isAudio()
        {
            if (_disable_license_validation == 0)
            {
                if (DateTime.Now.Date > _expirydate.Date || DateTime.Now.Date < _lowerexpirydate.Date)
                    return "129";
            }

            if (!Validate_FFMPEG())
                return "100";

            if (!this.SkipInput)
            {
                if (!Validate_InputPath())
                    return "101";
            }

            // set paths

            // set paths in order to accept spaces
            string _full_ffmpeg_path = "\"" + FFMPEGPath + "\"";
            string _full_input_path = "\"" + InputPath + "\\" + FileName + "\"";
            if (FileName.StartsWith("http"))
                _full_input_path = FileName;

            // process cmd
            string cmd = " -i " + _full_input_path;

            string output = Process_CMD(_full_ffmpeg_path, cmd);
            if (output == "")
                return "110";

            // validate video file
            if (!Validate_Video(output))
                return "111";

            // process video output

            if (isMatch(output, "no such file or directory"))
                return "105";
            else if (isMatch(output, "Unknown format"))
                return "104";
            else if (isMatch(output, "Unknown encoder"))
                return "107";
            else if (!isMatch(output, "Duration"))
                return "107";
            else if (isMatch(output, "unrecognized option"))
                return "115";
            else if (isMatch(output, "Could not open"))
                return "116";
            else
            {
                string _out = output.Remove(0, output.LastIndexOf("Duration"));
                if (_out.Contains("Video:"))
                    return "video";
                else
                    return "audio";
            }
        }
        /// <summary>
        /// Set Meta Information for FLV Videos.
        /// </summary>
        public string Set_Buffering()
        {
            if (_disable_license_validation == 0)
            {
                if (DateTime.Now.Date > _expirydate.Date || DateTime.Now.Date < _lowerexpirydate.Date)
                {
                    return "129";
                }
            }

            if (!Validate_FLVToolPath())
                return "113";

            if (!this.SkipInput)
            {
                if (!Validate_InputPath())
                    return "101";
            }


            // set paths in order to accept spaces
            string _full_flvtoolpath = "\"" + FLVToolPath + "\"";
            string _full_input_path = "\"" + InputPath + "\\" + FileName + "\"";

            string cmd = _full_input_path;

            return Process_FLVTool_CMD(_full_flvtoolpath, cmd);

        }

        /// <summary>
        /// Set Meta Information for MP4 Videos.
        /// </summary>
        public string Set_MP4_Buffering()
        {
            if (_disable_license_validation == 0)
            {
                if (DateTime.Now.Date > _expirydate.Date || DateTime.Now.Date < _lowerexpirydate.Date)
                {
                    return "129";
                }
            }

            if (!Validate_MP4BoxPath())
                return "147";

            //if (!Validate_InputPath())
            //    return "101";

            // set paths in order to accept spaces
            string _full_mp4boxpath = "\"" + MP4BoxPath + "\"";
            string _full_input_path = "\"" + InputPath + "\\" + FileName + "\"";
            if (FileName.StartsWith("http"))
                _full_input_path = FileName;
            string cmd = _full_input_path;

            return Process_MP4Box_CMD(_full_mp4boxpath, cmd);

        }

        /// <summary>
        /// Process any format media to another format -/ Generalize form of media encoding
        /// </summary>
        public VideoInfo Process()
        {
            VideoInfo info = new VideoInfo();
            string output_name = "";

            if (_disable_license_validation == 0)
            {
                if (DateTime.Now.Date > _expirydate.Date || DateTime.Now.Date < _lowerexpirydate.Date)
                {
                    info.ErrorCode = 129;
                    return info;
                }
            }

            if (!Validate_FFMPEG())
            {
                info.ErrorCode = 100;
                return info;
            }

            if (!this.SkipInput)
            {
                if (!Validate_InputPath())
                {
                    info.ErrorCode = 101;
                    return info;
                }
            }


            if (!Validate_OutputPath())
            {
                info.ErrorCode = 102;
                return info;
            }

            // check whether watermark option is also on
            if (WaterMarkPath != "" && WaterMarkImage != "")
            {
                if (!Validate_WaterMarkPath())
                {
                    info.ErrorCode = 108;
                    return info;
                }
            }

            if (OutputFileName == "")
            {
                info.ErrorCode = 141;
                info.ErrorMessage = "Output File Name must specifiy, e.g sample.avi or sample, extension will skip if specified.";
                return info;
            }

            if (OutputExtension == "")
            {
                info.ErrorCode = 140;
                info.ErrorMessage = "Must specify output media type (OutputExtension)";
                return info;
            }

            if (!OutputExtension.StartsWith("."))
                OutputExtension = "." + OutputExtension;

            if (isMatch(OutputFileName, @"\."))
                output_name = OutputFileName.Remove(OutputFileName.LastIndexOf(".")) + OutputExtension;
            else
                output_name = OutputFileName + "" + OutputExtension;

            // Width & Height must be multiple of two
            if (Width % 2 == 1)
                Width = Width + 1;
            if (Height % 2 == 1)
                Height = Height + 1;

            // set paths in order to accept spaces
            string _full_ffmpeg_path = "\"" + FFMPEGPath + "\"";
            string _full_input_path = "\"" + InputPath + "\\" + FileName + "\"";
            string _full_output_path = "\"" + OutputPath + "\\" + output_name + "\"";
            // No default settings in this case

            try
            {
                string cmd = "";
                string output = "";
                if (this.Pass == 2)
                {
                    // Single Command
                    string temp_params = this.Parameters;
                    if (this.Pass1Parameters != "")
                        this.Parameters = this.Pass1Parameters;

                    // 2 Pass encoding
                    // Step I: Pass 1: Encoding
                    this.Pass = 1;
                    string _firstpassspath = "";
                    if (this.FirstPassOutput != "")
                        _firstpassspath = this.FirstPassOutput;
                    else
                        _firstpassspath = _full_output_path;

                    cmd = Prepare_Command(_full_input_path, _firstpassspath, false, true);
                    output = Process_CMD(_full_ffmpeg_path, cmd);
                    this.Pass = 2;
                    this.Parameters = temp_params;
                    cmd = Prepare_Command(_full_input_path, _full_output_path, false, true);
                    output = Process_CMD(_full_ffmpeg_path, cmd);

                }
                else
                {
                    // 1 Pass encoding
                    // prepare command
                    cmd = Prepare_Command(_full_input_path, _full_output_path, false, true);
                    // process command
                    output = Process_CMD(_full_ffmpeg_path, cmd);
                }


                info.FFMPEGOutput = output;
                // parse output and retrieve information
                //info = Generate_Output(output);
                info = PARSE_FFMPEG_OUTPUT(output);

                info.FileName = output_name;

                //info.FFMPEGOutput = output;

                return info;
            }
            catch (Exception ex)
            {
                info.ErrorCode = 121;
                info.ErrorMessage = ex.Message;
                return info;
            }
        }

        /// <summary>
        /// Grab single thumbail from video.
        /// </summary>
        public VideoInfo Grab_Thumb()
        {
            VideoInfo info = new VideoInfo();
            string output_name = "";

            if (_disable_license_validation == 0)
            {
                if (DateTime.Now.Date > _expirydate.Date || DateTime.Now.Date < _lowerexpirydate.Date)
                {
                    info.ErrorCode = 129;
                    return info;
                }
            }

            if (!Validate_FFMPEG())
            {
                info.ErrorCode = 100;
                return info;
            }

            if (!this.SkipInput)
            {
                if (!Validate_InputPath())
                {
                    info.ErrorCode = 101;
                    return info;
                }
            }



            if (!Validate_OutputPath())
            {
                info.ErrorCode = 102;
                return info;
            }

            string cmd = "";
            string _full_ffmpeg_path = "\"" + FFMPEGPath + "\"";
            string _full_input_path = "\"" + InputPath + "\\" + FileName + "\"";
            if (FileName.StartsWith("http"))
                _full_input_path = FileName;
            string _full_output_path = "";
            string output = "";

            // Set Thumbnail Settings
            // Video Codec
            if (VCodec == "")
                VCodec = "image2";
            // Width & Height must be multiple of two
            if (Width % 2 == 1)
                Width = Width + 1;
            if (Height % 2 == 1)
                Height = Height + 1;
            string size = " ";
            if (Width == 0 || Height == 0)
                size = " ";
            else
                size = " -s " + Width + "x" + Height + " ";

            if (Multiple_Thumbs)
            {
                // multiple thumbs enabled
                if (No_Of_Thumbs < 1)
                {
                    No_Of_Thumbs = 1;
                    //info.ErrorCode = 125;
                    //return info;
                }

                // Retrive information from video
                // get source video duration
                info = Get_Info();
                if (info.ErrorCode > 0)
                    return info;

                int total_seconds = info.Duration_Sec;
                // calculate automatic transition time.
                if (total_seconds < 0)
                {
                    info.ErrorCode = 125;
                    return info;
                }

                // Thumb Mode
                if (ThumbMode == 0)
                {
                    //*****************************************
                    // Normal Thumb Grabbing Mode Enabled
                    //*****************************************
                    // set time duration between two thumbs
                    if (Thumb_Start_Position > 0)
                    {
                        if (No_Of_Thumbs == 1)
                            Thumb_Transition_Time = (int)((total_seconds - Thumb_Start_Position) / 2);
                        else
                            Thumb_Transition_Time = (int)((total_seconds - Thumb_Start_Position) / No_Of_Thumbs);
                    }
                    else
                    {
                        if (No_Of_Thumbs == 1)
                            Thumb_Transition_Time = (int)total_seconds / 2;
                        else
                            Thumb_Transition_Time = (int)total_seconds / No_Of_Thumbs;
                    }

                    int i = 0;
                    int counter = 1;
                    int seek_pos = 0;
                    if (Thumb_Start_Position > 0)
                        seek_pos = Thumb_Start_Position;
                    string thumb_index = "";

                    for (i = 0; i <= No_Of_Thumbs - 1; i++)
                    {
                        if (ImageName == "")
                            ImageName = FileName;

                        if (isMatch(ImageName, @"\."))
                            ImageName = ImageName.Remove(ImageName.LastIndexOf("."));

                        if (counter < 10)
                            thumb_index = "00" + counter;
                        else if (counter >= 10 && i < 100)
                            thumb_index = "0" + counter;
                        else
                            thumb_index = counter.ToString();

                        if (!Image_Format.StartsWith("."))
                            Image_Format = "." + Image_Format;

                        output_name = ImageName + "" + thumb_index + "" + Image_Format;

                        _full_output_path = "\"" + OutputPath + "\\" + output_name + "\"";

                        string _parameters = "";
                        if (Parameters != "")
                            _parameters = Parameters;

                        var _init_value = "-ss " + seek_pos;
                        if (this.InitialCommands != "")
                            _init_value = this.InitialCommands;

                        cmd = " " + _init_value + " -i " + _full_input_path + " -vframes 1 " + _parameters + " " + size + "-f " + VCodec + " -y " + _full_output_path;
                        // Execute Code
                        //output = Process_CMD(_full_ffmpeg_path, cmd);
                        output = Process_OGG_CMD(_full_ffmpeg_path, cmd);

                        seek_pos = seek_pos + (int)Thumb_Transition_Time;

                        if (seek_pos > total_seconds)
                            seek_pos = total_seconds - 1;

                        counter++;

                    }

                    //*************************************
                    // End - Normal Thumb Grabbing Mode
                    //*************************************
                }
                else
                {
                    //*****************************************
                    // Fast Thumb Grabbing Mode On
                    //****************************************

                    // manual transition enabled
                    if (Auto_Transition_Time)
                    {
                        // set time duration between two thumbs
                        if (Thumb_Start_Position > 0)
                        {
                            if (No_Of_Thumbs == 1)
                                Thumb_Transition_Time = (int)((total_seconds - Thumb_Start_Position) / 2);
                            else
                                Thumb_Transition_Time = (int)((total_seconds - Thumb_Start_Position) / No_Of_Thumbs);
                        }
                        else
                        {
                            if (No_Of_Thumbs == 1)
                                Thumb_Transition_Time = (int)total_seconds / 2;
                            else
                                Thumb_Transition_Time = (int)total_seconds / No_Of_Thumbs;
                        }
                    }
                    else
                    {
                        // manual settings for thumbs capture
                        int total_video_seconds = (No_Of_Thumbs * (int)Thumb_Transition_Time) + Thumb_Start_Position;
                        if (total_video_seconds > total_seconds)
                        {
                            info.ErrorCode = 124;
                            return info;
                        }
                    }
                    // setup multiple thumb index name
                    Image_Format = "%03d.jpg";
                    output_name = Return_Thumb_Name(FileName, ImageName, Image_Format);
                    _full_output_path = "\"" + OutputPath + "\\" + output_name + "\"";


                    string start_position = "";
                    if (Thumb_Start_Position > 0)
                        start_position = " -ss " + Thumb_Start_Position + " ";

                    string no_of_frames = "";
                    if (No_Of_Thumbs > 0)
                        no_of_frames = " -vframes " + No_Of_Thumbs + " ";
                    else
                        no_of_frames = " -vframes 1 ";

                    string _fr = " -r 1 ";
                    if (Thumb_Transition_Time > 1)
                    {
                        if (Thumb_Transition_Time > 20)
                            _fr = " -r 1/20 ";
                        else
                            _fr = " -r 1/" + Thumb_Transition_Time + " ";
                    }
                    else if (Thumb_Start_Position < 1)
                    {
                        _fr = " -r " + 1 / Thumb_Transition_Time + " ";
                    }

                    // builtin support
                    cmd = " " + start_position + " -i " + _full_input_path + " " + no_of_frames + " " + _fr + " " + size + " -f " + VCodec + " -y " + _full_output_path;
                    // extended support
                    var _init_value = start_position;
                    if (this.InitialCommands != "")
                        _init_value = this.InitialCommands;

                    var _default_param = "" + no_of_frames + " " + _fr + " " + size + " -f " + VCodec;
                    if (this.Parameters != "")
                        _default_param = this.Parameters;
                    cmd = " " + _init_value + " -i " + _full_input_path + " " + _default_param + " -y " + _full_output_path;

                    // Execute Code
                    output = Process_CMD(_full_ffmpeg_path, cmd);

                }

                //***************************************
                // End :-> Fast Grabbing Thumb Script
                //***************************************
            }
            else
            {
                //***************************************
                // Single Thumb
                //************************************

                output_name = Return_Output_Name(FileName, ImageName, Image_Format);
                _full_output_path = "\"" + OutputPath + "\\" + output_name + "\"";

                string seek_pos = "";
                if (Thumb_Start_Position > 0)
                    seek_pos = " -ss " + Thumb_Start_Position;
                else
                    seek_pos = " -ss " + Frame_Time;

                cmd = " " + seek_pos + " -i " + _full_input_path + " -vframes 1 " + size + "-f " + VCodec + " -y " + _full_output_path;
                // Execute Code
                output = Process_CMD(_full_ffmpeg_path, cmd);

                //***************************************
                // End Single Thumb
                //***************************************
            }

            // validate output returned from ffmpeg
            if (output == "")
            {
                info.ErrorCode = 110;
                return info;
            }
            if (output.Contains("Error while opening codec for output stream"))
            {
                info.ErrorCode = 128;
                return info;
            }

            // return name of generated image
            info.FileName = output_name;
            info.FFMPEGOutput = output;
            return info;
        }


        /// <summary>
        /// Get Information from video.
        /// </summary>
        public VideoInfo Get_Info()
        {
            // RESET: Error Code = 0 > show no error yet found.
            VideoInfo info = new VideoInfo();

            if (_disable_license_validation == 0)
            {
                if (DateTime.Now.Date > _expirydate.Date || DateTime.Now.Date < _lowerexpirydate.Date)
                {
                    info.ErrorCode = 129;
                    return info;
                }
            }

            if (!Validate_FFMPEG())
            {
                info.ErrorCode = 100;
                return info;
            }

            if (!this.SkipInput)
            {
                if (!Validate_InputPath())
                {
                    info.ErrorCode = 101;
                    return info;
                }
            }


            // set paths

            // set paths in order to accept spaces
            string _full_ffmpeg_path = "\"" + FFMPEGPath + "\"";
            string _full_input_path = "\"" + InputPath + "\\" + FileName + "\"";
            if (FileName.StartsWith("http"))
                _full_input_path = FileName;
            // process cmd
            try
            {
                string cmd = " -i " + _full_input_path;
                string output = Process_CMD(_full_ffmpeg_path, cmd);
                info = PARSE_FFMPEG_OUTPUT(output);

                return info;

            }
            catch (Exception ex)
            {
                info.ErrorCode = 121;
                info.ErrorMessage = ex.Message;
                return info;
            }
        }

        /// <summary>
        /// Join two videos
        /// </summary>
        public VideoInfo Join_Videos()
        {
            VideoInfo info = new VideoInfo();
            string output_name = "";

            if (_disable_license_validation == 0)
            {
                if (DateTime.Now.Date > _expirydate.Date || DateTime.Now.Date < _lowerexpirydate.Date)
                {
                    info.ErrorCode = 129;
                    return info;
                }
            }

            if (!Validate_FFMPEG())
            {
                info.ErrorCode = 100;
                return info;
            }

            if (!Validate_OutputPath())
            {
                info.ErrorCode = 102;
                return info;
            }

            if (FileNames == null)
            {
                info.ErrorCode = 138;
                info.ErrorMessage = "Join Video: Please specify filenames.";
                return info;
            }

            if (FileNames.Length < 2)
            {
                info.ErrorCode = 143;
                info.ErrorMessage = "Join Video: Must contain two or more clips.";
                return info;
            }

            if (OutputFileName == "")
            {
                info.ErrorCode = 141;
                info.ErrorMessage = "Join Video: Output File Name must specifiy, e.g sample.avi or sample, extension will skip if specified.";
                return info;
            }

            if (OutputExtension == "")
            {
                info.ErrorCode = 140;
                info.ErrorMessage = "Join Video: Must specify output media type (OutputExtension)";
                return info;
            }



            // Input Files / Clips Validation
            int i = 0;
            string clip_name = "";
            bool flg = true;
            bool ismpg = true;
            for (i = 0; i <= FileNames.Length - 1; i++)
            {
                clip_name = FileNames[i].ToString();
                if (!File.Exists(InputPath + "\\" + clip_name))
                {
                    flg = false;
                }
                if (!clip_name.EndsWith(".mpg"))
                    ismpg = false;
            }
            if (!flg)
            {
                info.ErrorCode = 139;
                info.ErrorMessage = "Join Video: Input file path validation failed, please make sure that all joining clips / files must be located in input path directory";
                return info;
            }

            if (!OutputExtension.StartsWith("."))
                OutputExtension = "." + OutputExtension;

            if (isMatch(OutputFileName, @"\."))
                output_name = OutputFileName.Remove(OutputFileName.LastIndexOf(".")) + OutputExtension;
            else
                output_name = OutputFileName + "" + OutputExtension;


            //***********************************************
            // Validation of files completed
            //***********************************************

            //// set paths in order to accept spaces
            string _full_ffmpeg_path = "\"" + FFMPEGPath + "\"";
            string _full_output_path = "\"" + OutputPath + "\\" + output_name + "\"";
            ArrayList _full_input_path = new ArrayList();
            ArrayList _temp_mpeg_path = new ArrayList();
            ArrayList _temp_mpeg_filename = new ArrayList();
            string _random_name = Guid.NewGuid().ToString().Substring(0, 6);
            string mpg_paths = "";
            // input paths
            for (i = 0; i <= FileNames.Length - 1; i++)
            {
                string _name = _random_name + "" + i + ".mpg";
                string path = "\"" + InputPath + "\\" + FileNames[i].ToString() + "\"";
                string temp_mpg_path = "\"" + OutputPath + "\\" + _name + "\"";
                if (i == 0)
                    mpg_paths = temp_mpg_path;
                else
                    mpg_paths = mpg_paths + " " + temp_mpg_path;

                _temp_mpeg_filename.Add(_name);
                _full_input_path.Add(path);
                _temp_mpeg_path.Add(temp_mpg_path);
            }

            // Parameter Settings
            // Width & Height must be multiple of two
            if (Width % 2 == 1)
                Width = Width + 1;
            if (Height % 2 == 1)
                Height = Height + 1;
            string size = " ";
            if (Width == 0 || Height == 0)
                size = " ";
            else
                size = " -s " + Width + "x" + Height + " ";

            try
            {
                string cmd = "";
                string output = "";
                // If all video clips is in mpg format and output extension is also in mpeg format
                // then skip video encoding and directly join video clips
                if (ismpg && OutputExtension == ".mpg")
                {
                    cmd = mpg_paths + " > " + _full_output_path;
                    Process_CMD("cmd.exe", "/C type " + cmd);
                }
                else
                {
                    //*****************************************
                    // First Step:
                    // MPG Encoding of all clips
                    //*****************************************
                    for (i = 0; i <= _temp_mpeg_path.Count - 1; i++)
                    {
                        info.ErrorCode = 0; // initialize error code
                        string oFile = _full_input_path[i].ToString();
                        if (oFile.EndsWith(".jpg") || oFile.EndsWith(".png") || oFile.EndsWith("jpeg"))
                        {
                            // image file
                            //We set the one slide show last 5 second by setting -r 1/5, and the framerate to be 30 so that we have 5x30=150 frames:
                            string temp_slid_filename = Guid.NewGuid().ToString() + ".mpg";
                            string temp_slide_filename_path = "\"" + InputPath + "\\" + temp_slid_filename + "\"";
                            cmd = " -r 1/5 -i " + oFile + " -r 30 " + temp_slide_filename_path;
                            Process_CMD(_full_ffmpeg_path, cmd);
                            //Then, using the output from the previous step as an input for this step, we put fade-in effect: starting 0 up to 30 frames (1 second):
                            string fade_in_filename = Guid.NewGuid().ToString() + ".mpg";
                            string fade_in_filename_path = "\"" + InputPath + "\\" + fade_in_filename + "\"";
                            cmd = " -i " + temp_slide_filename_path + " -y -vf fade=in:0:30 " + fade_in_filename_path;
                            Process_CMD(_full_ffmpeg_path, cmd);
                            //With the slide that has fade-in, we will add fade-out effect: starting 120th frame to the end (150th frame) using the fade-in output as an input for this fade-out process:
                            cmd = " -i " + fade_in_filename_path + " -y -vf fade=out:120:30 " + _temp_mpeg_path[i].ToString();
                            output = Process_CMD(_full_ffmpeg_path, cmd);
                            info = PARSE_FFMPEG_OUTPUT(output);
                            if (File.Exists(InputPath + "\\" + temp_slid_filename + ""))
                                File.Delete(InputPath + "\\" + temp_slid_filename + "");
                            if (File.Exists(InputPath + "\\" + fade_in_filename + ""))
                                File.Delete(InputPath + "\\" + fade_in_filename + "");
                        }
                        else
                        {
                            // video file
                            cmd = " -i " + _full_input_path[i].ToString() + " -qscale 0 " + size + " -y " + _temp_mpeg_path[i].ToString();
                            output = Process_CMD(_full_ffmpeg_path, cmd);
                            //info = Generate_Output(output);
                            info = PARSE_FFMPEG_OUTPUT(output);
                            if (info.ErrorCode > 0)
                            {
                                // error occurs
                                // delete all encoded mpg files
                                for (i = 0; i <= _temp_mpeg_path.Count - 1; i++)
                                {
                                    if (File.Exists(OutputPath + "\\" + _temp_mpeg_filename[i].ToString() + ""))
                                        File.Delete(OutputPath + "\\" + _temp_mpeg_filename[i].ToString() + "");
                                }
                                // return error
                                info.ErrorCode = 142;
                                info.ErrorMessage = "Join Video: Failed to create temp mpg files for attaching videos.";
                                return info;
                            }
                        }
                    }
                    //***************************************
                    // Step II: Joining all mpg clips in one clip
                    //***************************************

                    string _temp_full_mpeg = "\"" + OutputPath + "\\" + _random_name + "_full.mpg" + "\"";
                    // concatenating all outputs
                    // type file1 file2 file3 > file_full
                    cmd = mpg_paths + " > " + _temp_full_mpeg;
                    Process_CMD("cmd.exe", "/C type " + cmd);
                    //if (File.Exists(OutputPath + "\\" + _random_name + "_full.mpg"))
                    //    info.ErrorMessage = "Concatenation successful";
                    //else
                    //    info.ErrorMessage = "Concatenation failed";
                    //****************************************
                    // Step IV: Publish Video in Final Output
                    //****************************************
                    //cmd = " -i " + _temp_full_mpeg + " -sameq " + size + " -vcodec mpeg4 -acodec libmp3lame -y " + _full_output_path;
                    cmd = Prepare_Command(_temp_full_mpeg, _full_output_path, false, true);
                    output = Process_CMD(_full_ffmpeg_path, cmd);
                    //info = Generate_Output(output);
                    info = PARSE_FFMPEG_OUTPUT(output);
                    if (info.ErrorCode > 0)
                    {
                        // error occurs
                        // delete all encoded mpg files
                        // return error
                        info.ErrorCode = 142;
                        info.ErrorMessage = "Final video publishing failed.";
                        return info;
                    }
                    //***************************************
                    // Step V: Delete all temp videos
                    //***************************************
                    if (File.Exists(OutputPath + "\\" + _random_name + "_full.mpg"))
                        File.Delete(OutputPath + "\\" + _random_name + "_full.mpg");
                    for (i = 0; i <= _temp_mpeg_path.Count - 1; i++)
                    {
                        if (File.Exists(OutputPath + "\\" + _temp_mpeg_filename[i].ToString() + ""))
                            File.Delete(OutputPath + "\\" + _temp_mpeg_filename[i].ToString() + "");
                    }

                }
                return info;

            }
            catch (Exception ex)
            {
                info.ErrorCode = 121;
                info.ErrorMessage = ex.Message;
                return info;
            }

        }

        ///// <summary>
        ///// Create video from image file
        ///// </summary>
        public VideoInfo ImagesToVideo()
        {
            VideoInfo info = new VideoInfo();
            string output_name = "";

            if (_disable_license_validation == 0)
            {
                if (DateTime.Now.Date > _expirydate.Date || DateTime.Now.Date < _lowerexpirydate.Date)
                {
                    info.ErrorCode = 129;
                    return info;
                }
            }

            if (!Validate_FFMPEG())
            {
                info.ErrorCode = 100;
                return info;
            }

            // In case of sequence of images only need start image index file name e.g sample_ / sample_001.jpg
            if (FileName == "")
            {
                info.ErrorCode = 101;
                return info;
            }
            string fname = FileName + "001.jpg";
            string input_path = InputPath + "\\" + fname;
            if (!File.Exists(input_path))
            {
                info.ErrorCode = 101;
                return info;
            }

            if (!Validate_OutputPath())
            {
                info.ErrorCode = 102;
                return info;
            }

            // check whether watermark option is also on
            if (WaterMarkPath != "" && WaterMarkImage != "")
            {
                if (!Validate_WaterMarkPath())
                {
                    info.ErrorCode = 108;
                    return info;
                }
            }

            if (OutputFileName == "")
            {
                info.ErrorCode = 141;
                info.ErrorMessage = "Output File Name must specifiy, e.g sample.avi or sample, extension will skip if specified.";
                return info;
            }

            if (OutputExtension == "")
            {
                info.ErrorCode = 140;
                info.ErrorMessage = "Must specify output media type (OutputExtension)";
                return info;
            }

            if (!OutputExtension.StartsWith("."))
                OutputExtension = "." + OutputExtension;

            if (isMatch(OutputFileName, @"\."))
                output_name = OutputFileName.Remove(OutputFileName.LastIndexOf(".")) + OutputExtension;
            else
                output_name = OutputFileName + "" + OutputExtension;

            // Width & Height must be multiple of two
            if (Width % 2 == 1)
                Width = Width + 1;
            if (Height % 2 == 1)
                Height = Height + 1;

            // set paths in order to accept spaces
            string _full_ffmpeg_path = "\"" + FFMPEGPath + "\"";
            string _full_input_path = "\"" + InputPath + "\\" + FileName + "%03d.jpg" + "\"";
            string _full_output_path = "\"" + OutputPath + "\\" + output_name + "\"";
            // No default settings in this case

            try
            {
                // prepare command
                string cmd = Prepare_Command(_full_input_path, _full_output_path, false, true);
                // process command
                string output = Process_CMD(_full_ffmpeg_path, cmd);
                // parse output and retrieve information
                //info = Generate_Output(output);
                info = PARSE_FFMPEG_OUTPUT(output);

                info.FileName = output_name;

                info.FFMPEGOutput = output;

                return info;
            }
            catch (Exception ex)
            {
                info.ErrorCode = 121;
                info.ErrorMessage = ex.Message;
                return info;
            }
        }

        /// <summary>
        /// Split video in 'n' number of clips with equal length (seconds)
        /// </summary>
        public VideoInfo Split_Video(int length)
        {

            VideoInfo info = new VideoInfo();
            string output_name = "";

            if (_disable_license_validation == 0)
            {
                if (DateTime.Now.Date > _expirydate.Date || DateTime.Now.Date < _lowerexpirydate.Date)
                {
                    info.ErrorCode = 129;
                    return info;
                }
            }

            if (!Validate_FFMPEG())
            {
                info.ErrorCode = 100;
                return info;
            }

            if (!Validate_InputPath())
            {
                info.ErrorCode = 101;
                return info;
            }

            if (!Validate_OutputPath())
            {
                info.ErrorCode = 102;
                return info;
            }

            // check whether watermark option is also on
            if (WaterMarkPath != "" && WaterMarkImage != "")
            {
                if (!Validate_WaterMarkPath())
                {
                    info.ErrorCode = 108;
                    return info;
                }
            }

            if (OutputFileName == "")
            {
                info.ErrorCode = 141;
                info.ErrorMessage = "Split Video: Output File Name must specifiy, e.g sample.avi or sample, extension will skip if specified.";
                return info;
            }

            if (OutputExtension == "")
            {
                info.ErrorCode = 140;
                info.ErrorMessage = "Split Video: Must specify output media type (OutputExtension)";
                return info;
            }

            if (!OutputExtension.StartsWith("."))
                OutputExtension = "." + OutputExtension;

            // Width & Height must be multiple of two
            if (Width % 2 == 1)
                Width = Width + 1;
            if (Height % 2 == 1)
                Height = Height + 1;

            // set paths in order to accept spaces
            string _full_ffmpeg_path = "\"" + FFMPEGPath + "\"";
            string _full_input_path = "\"" + InputPath + "\\" + FileName + "\"";

            // Get source video duration
            info = Get_Info();
            if (info.ErrorCode > 0)
                return info;

            int total_seconds = info.Duration_Sec;
            // validate length of video with length specified
            int no_of_clips = 1;
            if (length < total_seconds)
            {
                no_of_clips = Convert.ToInt32(Math.Floor((double)total_seconds / length));
            }
            try
            {
                int i = 0;
                string _index = "";
                int start_index = 0;
                Start_Position = "0";
                Duration = length.ToString();
                string output = "";
                if (no_of_clips == 1)
                {
                    if (isMatch(OutputFileName, @"\."))
                        output_name = OutputFileName.Remove(OutputFileName.LastIndexOf(".")) + "" + OutputExtension;
                    else
                        output_name = OutputFileName + "" + OutputExtension;

                    // set clip path
                    string path = "\"" + OutputPath + "\\" + output_name + "\"";
                    // prepare command
                    string cmd = Prepare_Command(_full_input_path, path, false, true);
                    // Processing starts
                    output = Process_CMD(_full_ffmpeg_path, cmd);
                    // info = Generate_Output(output);
                    info = PARSE_FFMPEG_OUTPUT(output);
                    if (info.ErrorCode > 0)
                    {
                        // error occured
                        return info;
                    }
                }
                else
                {
                    for (i = 0; i <= no_of_clips - 1; i++)
                    {
                        // set clip name
                        if (i < 10)
                            _index = "0";
                        else
                            _index = "";

                        if (isMatch(OutputFileName, @"\."))
                            output_name = OutputFileName.Remove(OutputFileName.LastIndexOf(".")) + "_" + _index + "_" + i + "" + OutputExtension;
                        else
                            output_name = OutputFileName + "_" + _index + "" + i + "" + OutputExtension;

                        // set clip path
                        string path = "\"" + OutputPath + "\\" + output_name + "\"";
                        // process clip
                        // prepare command
                        string cmd = Prepare_Command(_full_input_path, path, false, true);
                        // Processing starts
                        output = Process_CMD(_full_ffmpeg_path, cmd);
                        // info = Generate_Output(output);
                        info = PARSE_FFMPEG_OUTPUT(output);
                        if (info.ErrorCode > 0)
                        {
                            // error occured
                            return info;
                        }
                        // process and increment start position
                        start_index = start_index + length + 1;
                        Start_Position = start_index.ToString();
                    }
                }

                info.FileName = OutputFileName;

                return info;
            }
            catch (Exception ex)
            {
                info.ErrorCode = 121;
                info.ErrorMessage = ex.Message;
                return info;
            }
        }

        public VideoInfo Split_Video(int length, int total_clips)
        {

            VideoInfo info = new VideoInfo();
            string output_name = "";

            if (_disable_license_validation == 0)
            {
                if (DateTime.Now.Date > _expirydate.Date || DateTime.Now.Date < _lowerexpirydate.Date)
                {
                    info.ErrorCode = 129;
                    return info;
                }
            }

            if (!Validate_FFMPEG())
            {
                info.ErrorCode = 100;
                return info;
            }

            if (!Validate_InputPath())
            {
                info.ErrorCode = 101;
                return info;
            }

            if (!Validate_OutputPath())
            {
                info.ErrorCode = 102;
                return info;
            }

            // check whether watermark option is also on
            if (WaterMarkPath != "" && WaterMarkImage != "")
            {
                if (!Validate_WaterMarkPath())
                {
                    info.ErrorCode = 108;
                    return info;
                }
            }

            if (OutputFileName == "")
            {
                info.ErrorCode = 141;
                info.ErrorMessage = "Split Video: Output File Name must specifiy, e.g sample.avi or sample, extension will skip if specified.";
                return info;
            }

            if (OutputExtension == "")
            {
                info.ErrorCode = 140;
                info.ErrorMessage = "Split Video: Must specify output media type (OutputExtension)";
                return info;
            }

            if (!OutputExtension.StartsWith("."))
                OutputExtension = "." + OutputExtension;

            // Width & Height must be multiple of two
            if (Width % 2 == 1)
                Width = Width + 1;
            if (Height % 2 == 1)
                Height = Height + 1;

            // set paths in order to accept spaces
            string _full_ffmpeg_path = "\"" + FFMPEGPath + "\"";
            string _full_input_path = "\"" + InputPath + "\\" + FileName + "\"";

            // Get source video duration
            info = Get_Info();
            if (info.ErrorCode > 0)
                return info;

            int total_seconds = info.Duration_Sec;
            // validate length of video with length specified
            int no_of_clips = 1;
            if (length < total_seconds)
            {
                no_of_clips = Convert.ToInt32(Math.Floor((double)total_seconds / length));
            }
            if (total_clips > no_of_clips)
            {
                info.ErrorCode = 122;
                info.ErrorMessage = "Total time of all clips plus clip time exceeds from original video time.";
                return info;
            }
            no_of_clips = total_clips;
            try
            {
                int i = 0;
                string _index = "";
                int start_index = 0;
                Start_Position = "0";
                Duration = length.ToString();
                string output = "";
                if (no_of_clips == 1)
                {
                    if (isMatch(OutputFileName, @"\."))
                        output_name = OutputFileName.Remove(OutputFileName.LastIndexOf(".")) + "" + OutputExtension;
                    else
                        output_name = OutputFileName + "" + OutputExtension;

                    // set clip path
                    string path = "\"" + OutputPath + "\\" + output_name + "\"";
                    // prepare command
                    string cmd = Prepare_Command(_full_input_path, path, false, true);
                    // Processing starts
                    output = Process_CMD(_full_ffmpeg_path, cmd);
                    // info = Generate_Output(output);
                    info = PARSE_FFMPEG_OUTPUT(output);
                    if (info.ErrorCode > 0)
                    {
                        // error occured
                        return info;
                    }
                }
                else
                {
                    for (i = 0; i <= no_of_clips - 1; i++)
                    {
                        // set clip name
                        if (i < 10)
                            _index = "0";
                        else
                            _index = "";

                        if (isMatch(OutputFileName, @"\."))
                            output_name = OutputFileName.Remove(OutputFileName.LastIndexOf(".")) + "_" + _index + "_" + i + "" + OutputExtension;
                        else
                            output_name = OutputFileName + "_" + _index + "" + i + "" + OutputExtension;

                        // set clip path
                        string path = "\"" + OutputPath + "\\" + output_name + "\"";
                        // process clip
                        // prepare command
                        string cmd = Prepare_Command(_full_input_path, path, false, true);
                        // Processing starts
                        output = Process_CMD(_full_ffmpeg_path, cmd);
                        // info = Generate_Output(output);
                        info = PARSE_FFMPEG_OUTPUT(output);
                        if (info.ErrorCode > 0)
                        {
                            // error occured
                            return info;
                        }
                        // process and increment start position
                        start_index = start_index + length + 1;
                        Start_Position = start_index.ToString();
                    }
                }

                info.FileName = OutputFileName;

                return info;
            }
            catch (Exception ex)
            {
                info.ErrorCode = 121;
                info.ErrorMessage = ex.Message;
                return info;
            }
        }

        /// <summary>
        /// Generate OGG Files.
        /// </summary>
        public string Encode_OGG()
        {
            if (_disable_license_validation == 0)
            {
                if (DateTime.Now.Date > _expirydate.Date || DateTime.Now.Date < _lowerexpirydate.Date)
                {
                    return "129";
                }
            }

            if (!this.SkipInput)
            {
                if (!Validate_InputPath())
                {
                    return "101";
                }
            }


            // set paths in order to accept spaces
            string _fullpath = "\"" + MP4BoxPath + "\"";
            string _full_input_path = "\"" + InputPath + "\\" + FileName + "\"";
            string _full_outupt_path = "\"" + OutputPath + "\\" + OutputFileName + "\"";

            string cmd = _full_input_path + " " + Parameters + " -o " + _full_outupt_path;

            return Process_OGG_CMD(_fullpath, cmd);
        }
        /// <summary>
        /// Execute Custom FFMPEG commands.
        /// </summary>
        public string Execute_FFMPEG()
        {
            if (_disable_license_validation == 0)
            {
                if (DateTime.Now.Date > _expirydate.Date || DateTime.Now.Date < _lowerexpirydate.Date)
                {
                    return "129";
                }
            }

            if (!Validate_FFMPEG())
                return "100";

            if (CustomCommand == "")
                return "114";

            // set paths in order to accept spaces
            string _full_ffmpeg_path = "\"" + FFMPEGPath + "\"";

            string cmd = CustomCommand;

            string output = Process_CMD_Custom(_full_ffmpeg_path, cmd);

            return output;
        }

        /// <summary>
        /// Generate OGG Files.
        /// </summary>
        public string ProcessCMD()
        {
            if (_disable_license_validation == 0)
            {
                if (DateTime.Now.Date > _expirydate.Date || DateTime.Now.Date < _lowerexpirydate.Date)
                {
                    return "129";
                }
            }

            // set paths in order to accept spaces
            string _fullpath = "\"" + ServicePath + "\"";
            //string _full_input_path = "\"" + InputPath + "\\" + FileName + "\"";
            //string _full_outupt_path = "\"" + OutputPath + "\\" + OutputFileName + "\"";

            //string cmd = Parameters;

            return Process_OGG_CMD(_fullpath, Parameters);
        }
        #endregion

        #region private METHODS


        // Process Mp4Box
        private string Process_OGG_CMD(string _fullpath, string cmd)
        {
            Process _process = new Process();
            _process.StartInfo.UseShellExecute = false;
            _process.StartInfo.RedirectStandardInput = true;
            _process.StartInfo.RedirectStandardOutput = true;
            _process.StartInfo.RedirectStandardError = true;
            _process.StartInfo.CreateNoWindow = true;
            _process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            _process.StartInfo.FileName = _fullpath;
            _process.StartInfo.Arguments = cmd;
            _process.Start();
            string _out = _process.StandardError.ReadToEnd();
            //_out = _out + " " + _process.StandardError.ReadToEnd();
            _process.WaitForExit();
            if (!_process.HasExited)
                _process.Kill();

            return _out;
        }
        // private  Methods used for internal purpose

        // Process Video Commands
        private string Process_CMD(string _ffmpegpath, string cmd)
        {
            string _out = "";
            string cdir = new FileInfo(this.FFMPEGPath).Directory.FullName;
            Process _process = new Process();
            _process.StartInfo.UseShellExecute = false;
            _process.StartInfo.RedirectStandardInput = true;
            //_process.StartInfo.RedirectStandardOutput = true;
            _process.StartInfo.RedirectStandardError = true;
            _process.StartInfo.CreateNoWindow = true;
            _process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            _process.StartInfo.WorkingDirectory = cdir;
            _process.StartInfo.FileName = _ffmpegpath;
            _process.StartInfo.Arguments = cmd;
            _process.StartInfo.LoadUserProfile = false;
            if (_process.Start())
            {

                ////_out = _process.StandardOutput.ReadToEnd();
                //_out = _process.StandardError.ReadToEnd();
                //_process.WaitForExit();
                //if (!_process.HasExited)
                //    _process.Kill();

                //return _out;
                // _out = _process.StandardOutput.ReadToEnd();

                _process.WaitForExit(ExitProcess);
                _out = _process.StandardError.ReadToEnd();
                if (!_process.HasExited)
                    _process.Kill();

                return _out;
            }
            else
            {
                return "007";
            }
        }

        // Parse FFMPEG Output Via Regular Expression
        private VideoInfo FFMPEG_OUTPUT_VALIDATION(string ffmpeg_data)
        {
            VideoInfo info = new VideoInfo();
            // watermark validation
            if (WaterMarkImage != "" && WaterMarkPath != "")
            {
                // watermark enabled
                // validate watermark.dll
                string _full_watermark_dll = FFMPEGPath.Remove(_ffmpegpath.LastIndexOf("ffmpeg.exe")) + "vhook\\watermark.dll";
                string _full_watermark_path = "\"" + WaterMarkPath + "\\" + WaterMarkImage + "\"";
                if (!File.Exists(_full_watermark_dll))
                {
                    info.ErrorCode = 136;
                    return info;
                }
                else if (_full_watermark_path.Contains(" "))
                {
                    info.ErrorCode = 137;
                    return info;
                }
            }
            // ffmpeg data validation
            if (ffmpeg_data == "")
            {
                info.ErrorCode = 110;
                return info;
            }
            if (isMatch(ffmpeg_data, "Unknown format"))
            {
                info.ErrorCode = 131;
                return info;
            }
            else if (isMatch(ffmpeg_data, "no such file or directory"))
            {
                info.ErrorCode = 105;
                return info;
            }
            else if (isMatch(ffmpeg_data, "Unsupported codec"))
            {
                info.ErrorCode = 104;
                return info;
            }
            else if (isMatch(ffmpeg_data, "Unknown encoder"))
            {
                info.ErrorCode = 132;
                return info;
            }
            else if (!isMatch(ffmpeg_data, "Duration"))
            {
                info.ErrorCode = 107;
                return info;
            }
            else if (isMatch(ffmpeg_data, "unrecognized option"))
            {
                info.ErrorCode = 115;
                return info;
            }
            else if (isMatch(ffmpeg_data, "Could not open"))
            {
                info.ErrorCode = 116;
                return info;
            }
            else if (isMatch(ffmpeg_data, "video codec not compatible"))
            {
                info.ErrorCode = 118;
                return info;
            }
            else if (isMatch(ffmpeg_data, "Unknown codec"))
            {
                info.ErrorCode = 133;
                return info;
            }
            else if (isMatch(ffmpeg_data, "Video hooking not compiled"))
            {
                info.ErrorCode = 123;
                return info;
            }
            else if (isMatch(ffmpeg_data, "incorrect parameters"))
            {
                info.ErrorCode = 134;
                return info;
            }
            else if (isMatch(ffmpeg_data, "Failed to add video hook"))
            {
                info.ErrorCode = 135;
                return info;
            }
            else if (isMatch(ffmpeg_data, "Unable to parse option value"))
            {
                info.ErrorCode = 145;
                return info;
            }
            else if (isMatch(ffmpeg_data, "incorrect codec parameters"))
            {
                info.ErrorCode = 146;
                return info;
            }
            else if (isMatch(ffmpeg_data, "File for preset") && isMatch(ffmpeg_data, "not found"))
            {
                info.ErrorCode = 144;
                return info;
            }
            else if (isMatch(ffmpeg_data, "not within the padded area"))
            {
                info.ErrorCode = 148;
                return info;
            }
            return info;
        }

        private VideoInfo PARSE_FFMPEG_INPUT_DATA(VideoInfo info, string ffmpeg_data)
        {
            string video_data = "";
            string audio_data = "";

            // FFMPEG Data Parsing Pattern
            //string strm_pattern = @"Stream #(?<number>\d+?\.\d+?)(\((?<language>\w+)\))?: (?<type>\w+): (?<data>.+)"; // Stream Pattern
            string strm_pattern = @"Stream #(?<number>\d+?\.\d+?)(\((?<language>\w+)\))?(\[(?<abc>\w+)\])?:\s(?<type>\w+):\s(?<data>.+)";
            string size_pattern = @"(?<width>\d+)x(?<height>\d+)"; // width / height pattern
            string bt_pattern = @"(?<bitrate>\d+(\.\d+)?(\s+)kb/s)"; // bitrate pattern 200 kb/s
            string codec_pattern = @"(?<codec>\w+),"; // codec pattern
            string fr_pattern = @"(?<framerate>\d+(\.\d+)? (tbr|fps))"; // framerate pattern // 29.97 tbr, 29.97 fps
            string smp_pattern = @"(?<samplingrate>\d+(\.\d+)?(\s+)Hz), (?<channel>\w+)"; // sampling rate, channel pattern 22050 Hz, sterio

            Match VDMatch = Regex.Match(ffmpeg_data, strm_pattern);
            if (VDMatch.Success)
            {
                //input_number = VDMatch.Groups["number"].Value;
                //input_language = VDMatch.Groups["language"].Value;
                info.Input_Type = VDMatch.Groups["type"].Value;
                if (info.Input_Type == "Video")
                    video_data = VDMatch.Groups["data"].Value;
                else
                    audio_data = VDMatch.Groups["data"].Value;

            }
            else
            {
                // if above logic failed -> replace (.) with (:) under Replace Stream #0.0 with Stream #0:0
                strm_pattern = @"Stream #(?<number>\d+:\d+?)(\((?<language>\w+)\))?(\[(?<abc>\w+)\])?:\s(?<type>\w+):\s(?<data>.+)";
                VDMatch = Regex.Match(ffmpeg_data, strm_pattern);
                if (VDMatch.Success)
                {
                    //input_number = VDMatch.Groups["number"].Value;
                    //input_language = VDMatch.Groups["language"].Value;
                    info.Input_Type = VDMatch.Groups["type"].Value;
                    if (info.Input_Type == "Video")
                        video_data = VDMatch.Groups["data"].Value;
                    else
                        audio_data = VDMatch.Groups["data"].Value;

                }
            }
            // PARSE ANOTHER PART DATA
            // Retrieve the Parsed Match Object Using the Regex Object
            string data = "";
            if (audio_data != "")
                data = audio_data;
            else
                data = video_data;
            Match VDMatch_02 = Regex.Match(data, strm_pattern);
            if (VDMatch_02.Success)
            {
                info.Input_Type = VDMatch_02.Groups["type"].Value;
                if (info.Input_Type == "Video")
                    video_data = VDMatch_02.Groups["data"].Value;
                else
                    audio_data = VDMatch_02.Groups["data"].Value;
            }

            // Video Data Parsing
            // width, height parsing
            Match SizeMatch = Regex.Match(video_data, size_pattern);
            if (SizeMatch.Success)
            {
                info.Input_Width = Convert.ToInt32(SizeMatch.Groups["width"].Value);
                info.Input_Height = Convert.ToInt32(SizeMatch.Groups["height"].Value);
                if (info.Input_Width == 0 || info.Input_Width > 4000 || info.Input_Height == 0 || info.Input_Height > 4000)
                {

                    if (info.Input_Width == 0 && info.Input_Height == 0)
                    { }
                    else
                    {
                        info.Input_Width = 0;
                        info.Input_Height = 0;
                        SizeMatch = SizeMatch.NextMatch();
                        info.Input_Width = Convert.ToInt32(SizeMatch.Groups["width"].Value);
                        info.Input_Height = Convert.ToInt32(SizeMatch.Groups["height"].Value);
                    }
                }
            }
            // Video Bitrate Parsing

            Match VDBTMatch = Regex.Match(video_data, bt_pattern);
            if (VDBTMatch.Success)
            {
                info.Input_Video_Bitrate = VDBTMatch.Groups["bitrate"].Value;
            }
            // Codec Parsing
            Match CodecMatch = Regex.Match(video_data, codec_pattern);
            if (CodecMatch.Success)
            {
                info.Input_Vcodec = CodecMatch.Groups["codec"].Value;
            }
            // Frame Rate
            Match FrMatch = Regex.Match(video_data, fr_pattern);
            if (FrMatch.Success)
            {
                info.Input_FrameRate = FrMatch.Groups["framerate"].Value;
            }
            // Audio Data Parsing
            // sampling rate
            Match SMPRMatch = Regex.Match(audio_data, smp_pattern);
            if (SMPRMatch.Success)
            {
                info.Input_SamplingRate = SMPRMatch.Groups["samplingrate"].Value;
                info.Input_Channel = SMPRMatch.Groups["channel"].Value;
            }
            // Channel -> Under Construction
            // Audio Bitrate
            Match ADBTMatch = Regex.Match(audio_data, bt_pattern);
            if (ADBTMatch.Success)
            {
                info.Input_Audio_Bitrate = ADBTMatch.Groups["bitrate"].Value;
            }
            // Codec Parsing
            Match AUCodecMatch = Regex.Match(audio_data, codec_pattern);
            if (AUCodecMatch.Success)
            {
                info.Input_Acodec = AUCodecMatch.Groups["codec"].Value;
            }

            if (video_data != "")
            {
                // video information exist
                info.Input_Type = "video";
            }

            return info;
        }

        private VideoInfo PARSE_FFMPEG_OUTPUT_DATA(VideoInfo info, string ffmpeg_data)
        {
            string video_data = "";
            string audio_data = "";

            // FFMPEG Data Parsing Pattern
            //string strm_pattern = @"Stream #(?<number>\d+?\.\d+?)(\((?<language>\w+)\))?: (?<type>\w+): (?<data>.+)"; // Stream Pattern
            string strm_pattern = @"Stream #(?<number>\d+?\.\d+?)(\((?<language>\w+)\))?(\[(?<abc>\w+)\])?:\s(?<type>\w+):\s(?<data>.+)";
            string size_pattern = @"(?<width>\d+)x(?<height>\d+)"; // width / height pattern
            string bt_pattern = @"(?<bitrate>\d+(\.\d+)?(\s+)kb/s)"; // bitrate pattern 200 kb/s
            string codec_pattern = @"(?<codec>\w+),"; // codec pattern
            string fr_pattern = @"(?<framerate>\d+(\.\d+)? (tbr|fps))"; // framerate pattern // 29.97 tbr, 29.97 fps
            string smp_pattern = @"(?<samplingrate>\d+(\.\d+)?(\s+)Hz), (?<channel>\w+)"; // sampling rate, channel pattern 22050 Hz, sterio

            Match VDMatch = Regex.Match(ffmpeg_data, strm_pattern);
            if (VDMatch.Success)
            {
                //input_number = VDMatch.Groups["number"].Value;
                //input_language = VDMatch.Groups["language"].Value;
                info.Type = VDMatch.Groups["type"].Value;
                if (info.Type == "Video")
                    video_data = VDMatch.Groups["data"].Value;
                else
                    audio_data = VDMatch.Groups["data"].Value;

            }
            else
            {
                // if above logic failed -> replace (.) with (:) under Replace Stream #0.0 with Stream #0:0
                strm_pattern = @"Stream #(?<number>\d+:\d+?)(\((?<language>\w+)\))?(\[(?<abc>\w+)\])?:\s(?<type>\w+):\s(?<data>.+)";
                VDMatch = Regex.Match(ffmpeg_data, strm_pattern);
                if (VDMatch.Success)
                {
                    //input_number = VDMatch.Groups["number"].Value;
                    //input_language = VDMatch.Groups["language"].Value;
                    info.Input_Type = VDMatch.Groups["type"].Value;
                    if (info.Input_Type == "Video")
                        video_data = VDMatch.Groups["data"].Value;
                    else
                        audio_data = VDMatch.Groups["data"].Value;

                }
            }
            // PARSE ANOTHER PART DATA
            // Retrieve the Parsed Match Object Using the Regex Object
            string data = "";
            if (audio_data != "")
                data = audio_data;
            else
                data = video_data;
            Match VDMatch_02 = Regex.Match(data, strm_pattern);
            if (VDMatch_02.Success)
            {
                info.Type = VDMatch_02.Groups["type"].Value;
                if (info.Type == "Video")
                    video_data = VDMatch_02.Groups["data"].Value;
                else
                    audio_data = VDMatch_02.Groups["data"].Value;
            }

            // Video Data Parsing
            // width, height parsing
            Match SizeMatch = Regex.Match(video_data, size_pattern);
            if (SizeMatch.Success)
            {
                info.Width = Convert.ToInt32(SizeMatch.Groups["width"].Value);
                info.Height = Convert.ToInt32(SizeMatch.Groups["height"].Value);
                if (info.Width == 0 || info.Width > 4000 || info.Height == 0 || info.Height > 4000)
                {
                    if (info.Width == 0 && info.Height == 0)
                    { }
                    else
                    {
                        info.Width = 0;
                        info.Height = 0;
                        SizeMatch = SizeMatch.NextMatch();
                        info.Width = Convert.ToInt32(SizeMatch.Groups["width"].Value);
                        info.Height = Convert.ToInt32(SizeMatch.Groups["height"].Value);
                    }
                }
            }
            // Video Bitrate Parsing

            Match VDBTMatch = Regex.Match(video_data, bt_pattern);
            if (VDBTMatch.Success)
            {
                info.Video_Bitrate = VDBTMatch.Groups["bitrate"].Value;
            }
            // Codec Parsing
            Match CodecMatch = Regex.Match(video_data, codec_pattern);
            if (CodecMatch.Success)
            {
                info.Vcodec = CodecMatch.Groups["codec"].Value;
            }
            // Frame Rate
            Match FrMatch = Regex.Match(video_data, fr_pattern);
            if (FrMatch.Success)
            {
                info.FrameRate = FrMatch.Groups["framerate"].Value;
            }
            // Audio Data Parsing
            // sampling rate
            Match SMPRMatch = Regex.Match(audio_data, smp_pattern);
            if (SMPRMatch.Success)
            {
                info.SamplingRate = SMPRMatch.Groups["samplingrate"].Value;
                info.Channel = SMPRMatch.Groups["channel"].Value;
            }
            // Channel -> Under Construction
            // Audio Bitrate
            Match ADBTMatch = Regex.Match(audio_data, bt_pattern);
            if (ADBTMatch.Success)
            {
                info.Audio_Bitrate = ADBTMatch.Groups["bitrate"].Value;
            }
            // Codec Parsing
            Match AUCodecMatch = Regex.Match(audio_data, codec_pattern);
            if (AUCodecMatch.Success)
            {
                info.Acodec = AUCodecMatch.Groups["codec"].Value;
            }

            if (video_data != "")
            {
                // video information exist
                info.Type = "video";
            }

            return info;
        }

        private VideoInfo PARSE_FFMPEG_OUTPUT(string ffmpeg_data)
        {
            VideoInfo info = new VideoInfo();

            ffmpeg_data = FixCode(ffmpeg_data);
            info.FFMPEGOutput = ffmpeg_data;

            // validate ffmpeg output
            info = FFMPEG_OUTPUT_VALIDATION(ffmpeg_data);
            if (info.ErrorCode > 0)
            {
                // ffmpeg output validation failed
                return info;
            }

            // Regular Expression Patterns for FFMPEG DATA Parsing
            string input_pattern = @"Input #(?<data>.+)"; // INPUT PATTERN
            string output_pattern = @"Output #(?<data>.+)";   //OUTPUT PATTERN
            string minfo_pattern = @"Duration: (?<hours>\d{1,3}):(?<minutes>\d{2}):(?<seconds>\d{2})(.(?<fractions>\d{1,3}))?, start: (?<start>\d+(\.\d+)?), bitrate: (?<bitrate>\d+(\.\d+)?(\s+)kb/s)?";  //Duration: 00:20:04.08, start: 30.000000, bitrate: 513 kb/s
                                                                                                                                                                                                           // VARIABLES
            string input = "";
            string output = "";
            // Retrieve the Parsed Match Object Using the Regex Object
            Match InputMatch = Regex.Match(ffmpeg_data, input_pattern);
            if (InputMatch.Success)
            {
                // match found
                input = InputMatch.Groups["data"].Value;
            }
            // IF INPUT DATA NOT EXIST
            if (input == "")
            {
                info.ErrorCode = 110;
                info.ErrorMessage = "No data retrieved from ffmpeg output";
                return info;
            }
            // Retrieve general information from ffmpeg data
            Match MInfoMatch = Regex.Match(input, minfo_pattern);
            if (MInfoMatch.Success)
            {
                info.Hours = Convert.ToInt32(MInfoMatch.Groups["hours"].Value);
                info.Minutes = Convert.ToInt32(MInfoMatch.Groups["minutes"].Value);
                info.Seconds = Convert.ToInt32(MInfoMatch.Groups["seconds"].Value);
                int fractions = Convert.ToInt32(MInfoMatch.Groups["fractions"].Value);
                if (fractions > 5)
                    info.Seconds = info.Seconds + 1;

                info.Start = MInfoMatch.Groups["start"].Value;
                info.Video_Bitrate = MInfoMatch.Groups["bitrate"].Value;

                info.Duration = info.Hours + ":" + info.Minutes + ":" + info.Seconds;
                info.Duration_Sec = (info.Hours * 3600) + (info.Minutes * 60) + info.Seconds;
            }

            // check for output data
            Match OutputMatch = Regex.Match(ffmpeg_data, output_pattern);
            if (OutputMatch.Success)
            {
                // match found
                output = OutputMatch.Groups["data"].Value;
            }

            if (output != "")
            {
                // both encoding and data retrieval done
                // Parse both input and output data
                // store input data in fields starting with input
                PARSE_FFMPEG_INPUT_DATA(info, input);

                PARSE_FFMPEG_OUTPUT_DATA(info, output);
            }
            else
            {
                // only information retrieved from video
                // parse only input data
                // Parse input data and retrieve it as output values
                PARSE_FFMPEG_OUTPUT_DATA(info, input);
            }

            // Script parsing completed
            return info;
        }


        private string Process_Mencoder_CMD(string _ffmpegpath, string cmd)
        {
            string _out = "";
            Process _process = new Process();
            _process.StartInfo.UseShellExecute = false;
            _process.StartInfo.RedirectStandardInput = true;
            _process.StartInfo.RedirectStandardOutput = true;
            _process.StartInfo.RedirectStandardError = true;
            _process.StartInfo.CreateNoWindow = true;
            _process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            _process.StartInfo.FileName = _ffmpegpath;
            _process.StartInfo.Arguments = cmd;
            if (_process.Start())
            {

                // _out = _process.StandardOutput.ReadToEnd();

                _process.WaitForExit(ExitProcess);
                if (!_process.HasExited)
                    _process.Kill();
                // _out = _process.StandardOutput.ReadToEnd();
                _out = _process.StandardError.ReadToEnd();
                return _out;
            }
            else
            {
                return "007";
            }


        }

        private string Process_CMD_Custom(string _ffmpegpath, string cmd)
        {

            string _out = "";
            Process _process = new Process();
            _process.StartInfo.UseShellExecute = false;
            _process.StartInfo.RedirectStandardInput = true;
            //_process.StartInfo.RedirectStandardOutput = true;
            _process.StartInfo.RedirectStandardError = true;
            _process.StartInfo.CreateNoWindow = true;

            _process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            _process.StartInfo.FileName = _ffmpegpath;
            _process.StartInfo.Arguments = cmd;
            if (_process.Start())
            {

                ////_out = _process.StandardOutput.ReadToEnd();
                //_out = _process.StandardError.ReadToEnd();
                //_process.WaitForExit();
                //if (!_process.HasExited)
                //    _process.Kill();

                //return _out;
                // _out = _process.StandardOutput.ReadToEnd();

                _process.WaitForExit(ExitProcess);
                _out = _process.StandardError.ReadToEnd();
                if (!_process.HasExited)
                    _process.Kill();

                return _out;
            }
            else
            {
                return "007";
            }


            //string _out = "";
            //Process _process = new Process();
            //_process.StartInfo.UseShellExecute = false;
            //_process.StartInfo.RedirectStandardInput = true;
            //_process.StartInfo.RedirectStandardOutput = true;
            //_process.StartInfo.RedirectStandardError = true;
            //_process.StartInfo.CreateNoWindow = true;
            //_process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            //_process.StartInfo.FileName = _ffmpegpath;
            //_process.StartInfo.Arguments = " " + cmd;
            ////_process.Start();
            //////_process.StandardOutput.ReadToEnd();
            ////_out = _process.StandardError.ReadToEnd();
            ////_process.WaitForExit();
            ////if (!_process.HasExited)
            ////    _process.Kill();

            ////return _out;
            //if (_process.Start())
            //{

            //    // _out = _process.StandardOutput.ReadToEnd();
            //    _process.WaitForExit(ExitProcess);
            //    if (!_process.HasExited)
            //        _process.Kill();

            //    _out = _process.StandardError.ReadToEnd();
            //    return _out;
            //}
            //else
            //{
            //    return "007";
            //}
        }

        // Process FLVTool
        private string Process_FLVTool_CMD(string _flvtoolpath, string cmd)
        {
            Process _process = new Process();
            _process.StartInfo.UseShellExecute = false;
            _process.StartInfo.RedirectStandardInput = true;
            _process.StartInfo.RedirectStandardOutput = true;
            _process.StartInfo.RedirectStandardError = true;
            _process.StartInfo.CreateNoWindow = true;
            _process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            _process.StartInfo.FileName = _flvtoolpath;

            _process.StartInfo.Arguments = " -U " + cmd;
            _process.Start();
            string _out = _process.StandardOutput.ReadToEnd();
            _out = _out + " " + _process.StandardError.ReadToEnd();
            _process.WaitForExit();
            if (!_process.HasExited)
                _process.Kill();

            if (isMatch(_out, "Permission denied"))
                return "117";
            else
                return "success";


        }

        // Process Mp4Box
        private string Process_MP4Box_CMD(string _mp4boxpath, string cmd)
        {
            Process _process = new Process();
            _process.StartInfo.UseShellExecute = false;
            _process.StartInfo.RedirectStandardInput = true;
            _process.StartInfo.RedirectStandardOutput = true;
            _process.StartInfo.RedirectStandardError = true;
            _process.StartInfo.CreateNoWindow = true;

            _process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            _process.StartInfo.FileName = _mp4boxpath;
            string _params = "-isma -hint";
            if (Parameters != "")
                _params = Parameters;
            _process.StartInfo.Arguments = " " + _params + " " + cmd;
            _process.Start();
            string _out = _process.StandardOutput.ReadToEnd();
            _out = _out + " " + _process.StandardError.ReadToEnd();
            _process.WaitForExit();
            if (!_process.HasExited)
                _process.Kill();

            if (isMatch(_out, "Permission denied"))
                return "117";
            else
                return "success";


        }

        // Validate FFMPEG Path
        private bool Validate_FFMPEG()
        {
            bool flg = false;
            if (FFMPEGPath == "")
                flg = false;
            else if (!File.Exists(FFMPEGPath))
                flg = false;
            else
                flg = true;

            return flg;
        }

        private bool Validate_FLVToolPath()
        {
            bool flg = false;
            if (FLVToolPath == "")
                flg = false;
            else if (!File.Exists(FLVToolPath))
                flg = false;
            else
                flg = true;

            return flg;
        }

        private bool Validate_MP4BoxPath()
        {
            bool flg = false;
            if (MP4BoxPath == "")
                flg = false;
            else if (!File.Exists(MP4BoxPath))
                flg = false;
            else
                flg = true;

            return flg;
        }

        // Validate Source Video Path
        private bool Validate_InputPath()
        {
            bool flg = false;
            if (FileName.StartsWith("http"))
            {
                return true;
            }
            else if (InputPath == "")
                flg = false;
            else if (FileName.Contains("%"))
                flg = true; // in some case filename may include % e.g sample_%4d.jpg
            else if (!File.Exists(InputPath + "\\" + FileName))
                flg = false;
            else
                flg = true;

            return flg;
        }

        // Validate Output Video Directory Path
        private bool Validate_OutputPath()
        {
            bool flg = false;
            if (OutputPath == "")
                flg = false;
            else if (!Directory.Exists(OutputPath))
                flg = false;
            else
                flg = true;

            return flg;
        }

        // Validate Watermark Path
        private bool Validate_WaterMarkPath()
        {
            bool flg = false;
            if (WaterMarkPath == "")
                flg = false;
            else if (!File.Exists(WaterMarkPath + "\\" + WaterMarkImage))
                flg = false;
            else
                flg = true;

            return flg;
        }

        // Validate Preset Path
        private bool Validate_PresetPath()
        {
            bool flg = false;
            if (PresetPath == "") // no need for validation - return true
                flg = true;
            else if (!File.Exists(PresetPath))
                flg = false;
            else
                flg = true;

            return flg;
        }

        // Validate Video File
        private bool Validate_Video(string output)
        {
            if (isMatch(output, "Unknown Format"))
                return false;
            else
                return true;
        }

        // Rename and shift video to other place
        private bool Rename_Video(string oldpath, string newpath)
        {
            bool flg = false;
            FileInfo thefile = new FileInfo(oldpath);
            if (thefile.Exists)
            {
                File.Move(oldpath, newpath);
                flg = true;
            }
            else
            {
                flg = false;
            }
            return flg;
        }

        private string Return_Output_Name(string filename, string outputfilename, string format)
        {
            string _ext = format;
            if (OutputExtension != "")
            {
                if (OutputExtension.Contains("."))
                    _ext = OutputExtension;
                else
                    _ext = "." + OutputExtension;
            }
            else
            {
                if (format.Contains("."))
                    _ext = format;
                else
                    _ext = "." + format;
            }
            string output_name = "";
            if (outputfilename != "")
            {
                if (isMatch(outputfilename, @"\."))
                    output_name = outputfilename.Remove(outputfilename.LastIndexOf(".")) + _ext;
                else
                    output_name = outputfilename + _ext;
            }
            else
            {
                if (isMatch(filename, @"\."))
                    output_name = filename.Remove(filename.LastIndexOf(".")) + _ext;
                else
                    output_name = filename + _ext;
            }
            return output_name;
        }

        private string Return_Thumb_Name(string filename, string outputfilename, string format)
        {
            string _ext = format;
            if (OutputExtension != "")
            {
                if (OutputExtension.Contains("."))
                    _ext = OutputExtension;
                else
                    _ext = "." + OutputExtension;
            }
            else
            {
                if (format.Contains("."))
                    _ext = format;
                else
                    _ext = "." + format;
            }
            string output_name = "";
            if (outputfilename != "")
            {
                if (isMatch(outputfilename, @"\."))
                    output_name = outputfilename.Remove(outputfilename.LastIndexOf(".")) + _ext;
                else
                    output_name = outputfilename + _ext;
            }
            else
            {
                if (isMatch(filename, @"\."))
                    output_name = filename.Remove(filename.LastIndexOf(".")) + _ext;
                else
                    output_name = filename + _ext;
            }
            return output_name;
        }

        private string Prepare_Command(string inputpath, string outputpath, bool isaudio, bool iswatermark)
        {
            string cmd = "";

            // set width and height
            string size = "";
            if (Width > 0 && Height > 0)
                size = " -s " + Width + "x" + Height + " ";

            // set max quality
            string _quality = "";
            if (MaxQuality)
                _quality = " -qscale 0 ";

            // disable video
            string _disable_video = "";
            if (DisableVideo)
                _disable_video = " -vn ";

            // disable audio
            string _disable_audio = "";
            if (DisableAudio)
                _disable_audio = " -an ";

            // aspect ratio settings
            string _aspect_ratio = "";
            if (AspectRatio != "")
                _aspect_ratio = " -aspect " + AspectRatio + " ";

            // set audio channel
            string _channel = "";
            if (Channel == 1 || Channel == 2)
                _channel = " -ac " + Channel + " ";

            // set audio sampling rate
            string _samplingrate = "";
            if (Audio_SamplingRate != 0)
                _samplingrate = " -ar " + Audio_SamplingRate + " ";

            // set audio bitrate
            string _audiobitrate = "";
            if (Audio_Bitrate != 0)
                _audiobitrate = " -b:a " + Audio_Bitrate + "k ";

            // set video bitrate
            string _video_bitrate = "";
            if (Video_Bitrate != 0)
                _video_bitrate = " -b:v " + Video_Bitrate + "k ";

            // set video framerate
            string _framerate = "";
            if (FrameRate != 0)
                _framerate = " -r " + FrameRate + " ";

            // set video duration
            string _duration = "";
            if (Duration != "")
                _duration = " -t " + Duration + " ";

            // set video codecs
            string _vcodec = "";
            if (VCodec != "")
                _vcodec = " -c:v " + VCodec + " ";

            // set audio codecs
            string _acodec = "";
            if (ACodec != "")
                _acodec = " -c:a " + ACodec + " ";

            // set deinterlace option
            string _deinterlace = "";
            if (Deinterlace)
                _deinterlace = " -deinterlace ";

            // set paddings
            string padtop = "";
            if (PadTop > 0)
                padtop = " -padtop " + PadTop + " ";

            string padbottom = "";
            if (PadBottom > 0)
                padbottom = " -padbottom " + PadBottom + " ";

            string padleft = "";
            if (PadLeft > 0)
                padleft = " -padleft " + PadLeft + " ";

            string padright = "";
            if (PadRight > 0)
                padright = " -padright " + PadRight + " ";

            string padcolor = "";
            if (PadColor != "")
                padcolor = " -padcolor " + PadColor + " ";
            // set cropping options
            string croptop = "";
            if (CropTop > 0)
                croptop = " -croptop " + CropTop + " ";

            string cropbottom = "";
            if (CropBottom > 0)
                cropbottom = " -cropbottom " + CropBottom + " ";

            string cropleft = "";
            if (CropLeft > 0)
                cropleft = " -cropleft " + CropLeft + " ";

            string cropright = "";
            if (CropRight > 0)
                cropright = " -cropright " + CropRight + " ";

            string _stpoint = "";
            if (Start_Position != "")
                _stpoint = " -ss " + Start_Position + " ";

            // set audio codecs
            string _frc = "";
            if (Force != "")
                _frc = " -f " + Force + " ";

            // set file size limit
            string _limit_filesize = "";
            if (Limit_File_Size > 0)
                _limit_filesize = " -fs " + Limit_File_Size + " ";

            // set video encoding pass (1-2)
            string _encoding_pass = "";
            if (Pass > 0 && Pass < 3)
                _encoding_pass = " -pass " + Pass + " ";

            // set quality factor (1-31) 
            string _video_quality_scale = "";
            if (Scale_Quality > 0 && Scale_Quality < 32)
                _video_quality_scale = " -qscale " + Scale_Quality + " ";

            // Preset Path - added in version 4.2.1.0
            string _preset = "";
            if (PresetPath != "")
                _preset = " -vpre \"" + PresetPath + "\"";

            // set target filetype
            string _target_file_type = "";
            if (TargetFileType != "")
                _target_file_type = " -target " + TargetFileType + " ";

            // optional parameter settings
            string _param = "";
            if (Parameters != "")
                _param = " " + Parameters + " ";

            string _inputCommand = " -i " + inputpath + "";
            if (this.SkipInput)
                _inputCommand = "";
            if (isaudio)
            {
                cmd = " " + InitialCommands + "" + _inputCommand + " " + _stpoint + "" + _duration + "" + _encoding_pass + "" + _limit_filesize + "" + _vcodec + "" + _video_quality_scale + "" + _param + "" + _channel + "" + _preset + "" + _acodec + "" + _video_bitrate + "" + _samplingrate + "" + _audiobitrate + "" + _frc + " -vn -y " + outputpath;
            }
            else
            {
                // normal video encoding mode
                if (DisableVideo)
                {
                    cmd = " " + InitialCommands + "" + _inputCommand + " " + _stpoint + "" + _duration + "" + _encoding_pass + "" + _limit_filesize + "" + _vcodec + "" + _video_quality_scale + "" + _target_file_type + "" + _preset + "" + _acodec + "" + _param + "" + _channel + "" + _audiobitrate + "" + _samplingrate + "" + _disable_video + "" + _disable_audio + "" + _frc + "" + " -y " + outputpath;
                }
                else
                {
                    cmd = " " + InitialCommands + " " + _stpoint + "" + _duration + "" + _inputCommand + " " + _stpoint + "" + _duration + "" + _encoding_pass + "" + _limit_filesize + "" + _vcodec + "" + _video_quality_scale + "" + _target_file_type + "" + _preset + "" + _acodec + "" + _param + " " + _channel + "" + padtop + "" + padbottom + "" + padleft + "" + padright + "" + croptop + "" + cropbottom + "" + cropleft + "" + cropright + "" + padcolor + "" + _aspect_ratio + "" + _framerate + "" + _video_bitrate + "" + _audiobitrate + "" + _samplingrate + "" + size + "" + _quality + "" + _frc + "" + _disable_video + "" + _disable_audio + "" + _deinterlace + " -y " + outputpath;
                }
            }
            return cmd;
        }

        private int Return_Total_Seconds(string Duration)
        {
            TimeSpan duration = TimeSpan.Zero;
            TimeSpan.TryParse(Duration, out duration);
            return (int)duration.TotalSeconds;

        }

        private bool isMatch(string value, string expression)
        {
            Match _match = Regex.Match(value, expression);
            if (_match.Success)
                return true;
            else
                return false;
        }

        private string FixCode(string html)
        {
            html = html.Replace("  ", "&nbsp; ");
            html = html.Replace("  ", " &nbsp;");
            html = html.Replace("\t", "&nbsp;&nbsp;&nbsp;");
            //html = html.Replace("[", "&#91;");
            //html = html.Replace("]", "&#93;");
            html = html.Replace("<", "&lt;");
            html = html.Replace(">", "&gt;");
            html = html.Replace("\n\n\n\n", "<br/><br/>");
            html = html.Replace("\n\n\n", "<br/><br/>");
            html = html.Replace("\n\n", "<br/><br/>");
            html = html.Replace("\n", "<br/>");
            return html;
        }

        #endregion

        #region ADVANCEPROCESSING

        private StringBuilder _processinglog = new StringBuilder();
        private string _processlog = "";
        private bool _backgroundprocessing = false;

        public string ProcessingLog
        {
            set { _processlog = value; }
            get { return _processlog; }
        }

        public bool BackgroundProcessing
        {
            set { _backgroundprocessing = value; }
            get { return _backgroundprocessing; }
        }

        // Regulax Expressions
        string _title_expression = @"(\s+|)title(\s+|):(\s+|)(?<vtitle>.+)"; // media title
        string _comment_expression = @"(\s+|)comment(\s+|):(\s+|)Footage(\s+|):(\s+|)(?<comment_footage>.+?\|)(\s+|)Producer(\s+|):(\s+|)(?<comment_producer>.+?\|)(\s+|)Music(\s+|):(\s+|)(?<comment_music>.+)"; // complete comment
        string minfo_pattern = @"Duration: (?<hours>\d{1,3}):(?<minutes>\d{2}):(?<seconds>\d{2})(.(?<fractions>\d{1,3}))?, start: (?<start>\d+(\.\d+)?), bitrate: (?<bitrate>\d+(\.\d+)?(\s+)kb/s)?";  //Duration: 00:20:04.08, start: 30.000000, bitrate: 513 kb/s
        string strm_pattern = @"Stream(\s|)#(?<number>\d+?\.\d+?)(\((?<language>\w+)\))?(\s|):(\s|)(?<type>\w+)(\s|):(\s|)(?<data>.+)"; // Stream #0.0(eng): Audio: wmav2, 44100 Hz, 2 channels, s16, 192 kb/s OR Stream #0.1(eng): Video: vc1 (Advanced), yuv420p, 1280x720, 5942 kb/s, 29.97 tbr, 1k tbn, 1k tbc
        string frame_info_pattern = @"frame=(\s+|)(?<frame>\d+)?(\s+|)fps=(\s+|)(?<fps>\d+)?(\s+|)q=(?<q>\d+.\d+)?(\s+|)size=(\s+|)(?<fsize>\d+)?kB(\s+|)time=(?<time_hr>\d+)?:(?<time_min>\d+)?:(?<time_sec>\d+)?.(?<time_frac>\d+)?\sbitrate=(\s+|)(?<bitrate>\d+.\d+)kbits/s"; //frame= 34 fps= 0 q=31.0 size= 0kB time=00:00:00.00 bitrate= 0.0kbits/s

        public VideoInfo vinfo = new VideoInfo();

        /// <summary>
        /// Generate OGG Files.
        /// </summary>
        public string Process_FFMPEG2Theora()
        {
            if (!Validate_InputPath())
            {
                return "101";
            }

            // set paths in order to accept spaces
            string _fullpath = "\"" + FFMPEGPath + "\"";
            string _full_input_path = "\"" + InputPath + "\\" + FileName + "\"";
            string _full_outupt_path = "\"" + OutputPath + "\\" + OutputFileName + "\"";

            string cmd = _full_input_path + " " + Parameters + " -o " + _full_outupt_path;

            ADVPROC(_fullpath, cmd);
            return "success";
        }
        /// <summary>
        /// Process any format media to another format -/ Generalize form of media encoding
        /// </summary>
        public VideoInfo ProcessMedia()
        {
            string output_name = "";

            if (!Validate_FFMPEG())
            {
                vinfo.ErrorCode = 100;
                return vinfo;
            }

            if (!Validate_InputPath())
            {
                vinfo.ErrorCode = 101;
                return vinfo;
            }

            if (!Validate_OutputPath())
            {
                vinfo.ErrorCode = 102;
                return vinfo;
            }

            // check whether watermark option is also on
            if (WaterMarkPath != "" && WaterMarkImage != "")
            {
                if (!Validate_WaterMarkPath())
                {
                    vinfo.ErrorCode = 108;
                    return vinfo;
                }
            }

            if (OutputFileName == "")
            {
                vinfo.ErrorCode = 141;
                vinfo.ErrorMessage = "Output File Name must specifiy, e.g sample.avi or sample, extension will skip if specified.";
                return vinfo;
            }

            if (OutputExtension == "")
            {
                vinfo.ErrorCode = 140;
                vinfo.ErrorMessage = "Must specify output media type (OutputExtension)";
                return vinfo;
            }

            string _path = InputPath + "\\" + FileName;
            if (FileName.Contains("\\") || FileName.Contains("/"))
                _path = FileName;

            // get video information
            vinfo = Get_Info();

            if (!OutputExtension.StartsWith("."))
                OutputExtension = "." + OutputExtension;

            if (isMatch(OutputFileName, @"\."))
                output_name = OutputFileName.Remove(OutputFileName.LastIndexOf(".")) + OutputExtension;
            else
                output_name = OutputFileName + "" + OutputExtension;

            vinfo.FileName = output_name;
            // Width & Height must be multiple of two
            if (Width % 2 == 1)
                Width = Width + 1;
            if (Height % 2 == 1)
                Height = Height + 1;

            // set paths in order to accept spaces
            string _full_ffmpeg_path = "\"" + FFMPEGPath + "\"";
            string _full_input_path = "\"" + _path + "\"";
            string _full_output_path = "\"" + OutputPath + "\\" + output_name + "\"";
            // No default settings in this case

            try
            {
                // prepare command
                string cmd = "";

                string _firstpassspath = "";
                if (this.FirstPassOutput != "")
                    _firstpassspath = this.FirstPassOutput;
                else
                    _firstpassspath = _full_output_path;

                // 1 Pass encoding
                // prepare command
                cmd = Prepare_Command(_full_input_path, _firstpassspath, false, true);
                //cmd = Prepare_Command(_full_input_path, _full_output_path, false, true);
                // process command
                vinfo.ProcessID = ADVPROC(_full_ffmpeg_path, cmd);

                /*if (this.Pass == 2)
                {
                    // Single Command
                    string temp_params = this.Parameters;
                    if (this.Pass1Parameters != "")
                        this.Parameters = this.Pass1Parameters;

                    // 2 Pass encoding
                    // Step I: Pass 1: Encoding
                    this.Pass = 1;

                    cmd = Prepare_Command(_full_input_path, _firstpassspath, false, true);
                    vinfo.ProcessID = ADVPROC(_full_ffmpeg_path, cmd);
                    this.Pass = 2;
                    this.Parameters = temp_params;
                    cmd = Prepare_Command(_full_input_path, _full_output_path, false, true);
                    vinfo.ProcessID = ADVPROC(_full_ffmpeg_path, cmd);
                }
                else
                {

                } */
                //string cmd = Prepare_Command(_full_input_path, _full_output_path, false, true);
                // vinfo.ProcessID = ADVPROC(_full_ffmpeg_path, cmd);
                return vinfo;
            }
            catch (Exception ex)
            {
                vinfo.ErrorCode = 121;
                vinfo.ErrorMessage = ex.Message;
                return vinfo;
            }
        }

        Process process = null;
        private int elapsedTime;
        private bool eventHandled;

        public string ADVPROC(string _ffmpegpath, string cmd)
        {
            string currentdir = new FileInfo(this.FFMPEGPath).Directory.FullName;
            int _procid = 0;
            elapsedTime = 0;
            eventHandled = false;
            try
            {
                process = new Process();
                _procid = System.Diagnostics.Process.GetCurrentProcess().Id;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.LoadUserProfile = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.EnableRaisingEvents = true;
                process.StartInfo.FileName = _ffmpegpath;
                process.StartInfo.WorkingDirectory = currentdir;
                process.StartInfo.Arguments = " " + cmd;
                process.ErrorDataReceived += new DataReceivedEventHandler(process_ErrorDataReceived);
                process.OutputDataReceived += new DataReceivedEventHandler(process_OutputDataReceived);
                process.Exited += new EventHandler(process_Exited);
                process.Start();
                _processinglog.Append("Processing Started<br />.................................<br />");
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
            }
            catch (Exception ex)
            {
                _processinglog.Append("Error Occured<br />" + ex.Message + "<br />.................................<br />");
                string msg = ex.Message;
                if (process != null) process.Dispose();
            }

            if (!BackgroundProcessing)
            {
                const int SLEEP_AMOUNT = 100;
                while (!eventHandled)
                {
                    elapsedTime += SLEEP_AMOUNT;
                    if (elapsedTime > 30000)
                    {
                        break;
                    }
                    System.Threading.Thread.Sleep(SLEEP_AMOUNT);
                }
            }
            return _procid.ToString();
        }

        private bool _processstarted = false; // check whether media processing started properly
        private void process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
                return;
            int totalprocessingleft = 0;
            Match _match = null;
            if (e.Data.Contains("Press [q] to stop,"))
            {
                // processing started properly
                _processstarted = true;
            }
            else if (e.Data.Contains("title"))
            {
                _match = Regex.Match(e.Data.Trim(), _title_expression);
                if (_match.Success)
                {
                    // fetch media title information
                    vinfo.Title = _match.Groups["vtitle"].Value;
                }
            }
            else if (e.Data.Contains("comment"))
            {
                _match = Regex.Match(e.Data.Trim(), _comment_expression);
                if (_match.Success)
                {
                    // fetch media comment information
                    vinfo.Footage = _match.Groups["comment_footage"].Value;
                    vinfo.Producer = _match.Groups["comment_producer"].Value;
                    vinfo.Music = _match.Groups["comment_music"].Value;
                }
            }
            else if (e.Data.Contains("Duration"))
            {
                _match = Regex.Match(e.Data, minfo_pattern);
                if (_match.Success)
                {
                    vinfo.Hours = Convert.ToInt32(_match.Groups["hours"].Value);
                    vinfo.Minutes = Convert.ToInt32(_match.Groups["minutes"].Value);
                    vinfo.Seconds = Convert.ToInt32(_match.Groups["seconds"].Value);
                    int fractions = Convert.ToInt32(_match.Groups["fractions"].Value);
                    if (fractions > 5)
                        vinfo.Seconds = vinfo.Seconds + 1;

                    vinfo.Start = _match.Groups["start"].Value;
                    vinfo.Video_Bitrate = _match.Groups["bitrate"].Value;

                    vinfo.Duration = vinfo.Hours + ":" + vinfo.Minutes + ":" + vinfo.Seconds;
                    vinfo.Duration_Sec = (vinfo.Hours * 3600) + (vinfo.Minutes * 60) + vinfo.Seconds;
                }
            }
            else if (e.Data.Contains("frame") && e.Data.Contains("size"))
            {
                _match = Regex.Match(e.Data, frame_info_pattern);
                if (_match.Success)
                {
                    int frame = Convert.ToInt32(_match.Groups["frame"].Value);
                    vinfo.ProcessedSize = Convert.ToInt64(_match.Groups["fsize"].Value);

                    int phours = Convert.ToInt32(_match.Groups["time_hr"].Value);
                    int pminutes = Convert.ToInt32(_match.Groups["time_min"].Value);
                    int pseconds = Convert.ToInt32(_match.Groups["time_sec"].Value);
                    int fractions = Convert.ToInt32(_match.Groups["time_frac"].Value);
                    if (fractions > 5)
                        pseconds = pseconds + 1;

                    int hr_sec = phours * 3600;
                    int min_sec = pminutes * 60;
                    vinfo.ProcessedTime = hr_sec + min_sec + pseconds;

                    // based on time rather than content size
                    totalprocessingleft = vinfo.Duration_Sec - vinfo.ProcessedTime;

                    vinfo.ProcessingLeft = Math.Round((double)(totalprocessingleft * 100) / vinfo.Duration_Sec, 2);
                    if (vinfo.ProcessingLeft < 0.5)
                        vinfo.ProcessingLeft = 0;
                    vinfo.ProcessingCompleted = 100 - vinfo.ProcessingLeft;
                    //long totalprocessingleft = TotalSize - ProcessedSize;
                }
                // frame information
            }
            else if (e.Data.Contains("Stream"))
            {
                string video_data = "";
                string audio_data = "";
                _match = Regex.Match(e.Data, strm_pattern);
                if (_match.Success)
                {
                    vinfo.Input_Type = _match.Groups["type"].Value;
                    if (vinfo.Input_Type == "Video")
                        video_data = _match.Groups["data"].Value;
                    else
                        audio_data = _match.Groups["data"].Value;
                }
                else
                {
                    // if above logic failed -> replace (.) with (:) under Replace Stream #0.0 with Stream #0:0
                    strm_pattern = @"Stream #(?<number>\d+:\d+?)(\((?<language>\w+)\))?(\[(?<abc>\w+)\])?:\s(?<type>\w+):\s(?<data>.+)";
                    _match = Regex.Match(e.Data, strm_pattern);
                    if (_match.Success)
                    {
                        vinfo.Input_Type = _match.Groups["type"].Value;
                        if (vinfo.Input_Type == "Video")
                            video_data = _match.Groups["data"].Value;
                        else
                            audio_data = _match.Groups["data"].Value;
                    }
                }
                string bt_pattern = @"(?<bitrate>\d+(\.\d+)?(\s+)kb/s)"; // bitrate pattern 200 kb/s
                string codec_pattern = @"(?<codec>\w+)?,"; // codec pattern
                string fr_pattern = @"(?<framerate>\d+(\.\d+)?(\s|)(tbr|fps))"; // framerate pattern // 29.97 tbr, 29.97 fps
                string smp_pattern = @"(?<samplingrate>\d+(\.\d+)?(\s+)Hz),(\s|)(?<channel>\w+)"; // sampling rate, channel pattern 22050 Hz, sterio
                if (video_data != "")
                {
                    // get video width and height
                    string size_pattern = @"(?<width>\d+)x(?<height>\d+)"; // width / height pattern
                    _match = Regex.Match(video_data, size_pattern);
                    if (_match.Success)
                    {
                        vinfo.Width = Convert.ToInt32(_match.Groups["width"].Value);
                        vinfo.Height = Convert.ToInt32(_match.Groups["height"].Value);
                        if (vinfo.Width == 0 || vinfo.Width > 4000 || vinfo.Height == 0 || vinfo.Height > 4000)
                        {
                            if (vinfo.Width == 0 && vinfo.Height == 0)
                            { }
                            else
                            {
                                vinfo.Width = 0;
                                vinfo.Height = 0;
                                _match = _match.NextMatch();
                                vinfo.Width = Convert.ToInt32(_match.Groups["width"].Value);
                                vinfo.Height = Convert.ToInt32(_match.Groups["height"].Value);
                            }
                        }
                    }

                    _match = Regex.Match(video_data, bt_pattern);
                    if (_match.Success)
                    {
                        vinfo.Video_Bitrate = _match.Groups["bitrate"].Value;
                    }
                    // Codec Parsing
                    _match = Regex.Match(video_data, codec_pattern);
                    if (_match.Success)
                    {
                        vinfo.Vcodec = _match.Groups["codec"].Value;
                    }
                    // Frame Rate
                    _match = Regex.Match(video_data, fr_pattern);
                    if (_match.Success)
                    {
                        vinfo.FrameRate = _match.Groups["framerate"].Value;
                    }
                }
                if (audio_data != "")
                {
                    // Audio Data Parsing
                    // sampling rate
                    _match = Regex.Match(audio_data, smp_pattern);
                    if (_match.Success)
                    {
                        vinfo.SamplingRate = _match.Groups["samplingrate"].Value;
                        vinfo.Channel = _match.Groups["channel"].Value;
                    }
                    // Channel -> Under Construction
                    // Audio Bitrate
                    _match = Regex.Match(audio_data, bt_pattern);
                    if (_match.Success)
                    {
                        vinfo.Audio_Bitrate = _match.Groups["bitrate"].Value;
                    }
                    // Codec Parsing
                    _match = Regex.Match(audio_data, codec_pattern);
                    if (_match.Success)
                    {
                        vinfo.Acodec = _match.Groups["codec"].Value;
                    }
                }
            }
            //_processinglog.Append("<br />Total Processing Left: " + totalprocessingleft + "<br />Processed Time: " + vinfo.ProcessedTime + "<br />");
            //_processinglog.Append("<br />Processing Completed: " + vinfo.ProcessingCompleted + " - Processing Left: " + vinfo.ProcessingLeft + "<br />");
            _processinglog.Append("<br />.................................<br />Error Output<br />.....................................<br />" + e.Data + "<br />.....................................<br />");
            //HttpContext.Current.Response.Write(e.Data);
        }

        private void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            _processinglog.Append("<br />.................................<br />Data Output<br />.....................................<br />" + e.Data + "<br />.....................................<br />");
        }

        private void process_Exited(object sender, EventArgs e)
        {
            if (!_processstarted)
            {
                // execution completed but no process started
                ParseValidationInfo(_processinglog.ToString());
            }

            vinfo.ProcessingCompleted = 100; // 100% processing completed
            vinfo.ProcessingLeft = 0; // 0% processing left at the end of process
            eventHandled = true;
            vinfo.FFMPEGOutput = _processinglog.ToString();
            process.Dispose();
        }

        private void ParseValidationInfo(string data)
        {
            vinfo.ErrorMessage = data;
            if (data == "")
            {
                vinfo.ErrorCode = 110;

            }
            if (isMatch(data, "Unknown format"))
            {
                vinfo.ErrorCode = 131;
            }
            else if (isMatch(data, "no such file or directory"))
            {
                vinfo.ErrorCode = 105;
            }
            else if (isMatch(data, "Unsupported codec"))
            {
                vinfo.ErrorCode = 104;
            }
            else if (isMatch(data, "Unknown encoder"))
            {
                vinfo.ErrorCode = 132;
            }
            else if (!isMatch(data, "Duration"))
            {
                vinfo.ErrorCode = 107;
            }
            else if (isMatch(data, "unrecognized option"))
            {
                vinfo.ErrorCode = 115;
            }
            else if (isMatch(data, "Could not open"))
            {
                vinfo.ErrorCode = 116;
            }
            else if (isMatch(data, "video codec not compatible"))
            {
                vinfo.ErrorCode = 118;
            }
            else if (isMatch(data, "Unknown codec"))
            {
                vinfo.ErrorCode = 133;
            }
            else if (isMatch(data, "Video hooking not compiled"))
            {
                vinfo.ErrorCode = 123;
            }
            else if (isMatch(data, "incorrect parameters"))
            {
                vinfo.ErrorCode = 134;
            }
            else if (isMatch(data, "Failed to add video hook"))
            {
                vinfo.ErrorCode = 135;
            }
            else if (isMatch(data, "Unable to parse option value"))
            {
                vinfo.ErrorCode = 145;
            }
            else if (isMatch(data, "incorrect codec parameters"))
            {
                vinfo.ErrorCode = 146;
            }
            else if (isMatch(data, "File for preset") && isMatch(data, "not found"))
            {
                vinfo.ErrorCode = 144;
            }
            else if (isMatch(data, "not within the padded area"))
            {
                vinfo.ErrorCode = 148;
            }
        }
        #endregion
    }
}

/*
 * This file is subject to the terms and conditions defined in
 * file 'LICENSE.md', which is part of this source code package.
 * Copyright 2007 - 2020 MediaSoftPro
 * For more information email at support@mediasoftpro.com
 */
