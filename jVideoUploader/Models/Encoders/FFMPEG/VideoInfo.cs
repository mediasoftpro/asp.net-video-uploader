namespace Jugnoon.Videos
{
    public class VideoInfo
    {
        public string ProcessID { get; set; } = "";
        public long ProcessedSize { get; set; } = 0;
        public int ProcessedTime { get; set; } = 0;
        public long TotalSize { get; set; } = 0;
        public double ProcessingLeft { get; set; } = 100;
        public double ProcessingCompleted { get; set; } = 0;
        public string FileName { get; set; } = "";
        public string Duration { get; set; } = "";
        public int Duration_Sec { get; set; } = 0;
        public int Hours { get; set; } = 0;
        public int Minutes { get; set; } = 0;
        public int Seconds { get; set; } = 0;
        public string Start { get; set; } = "";
        public int ErrorCode { get; set; } = 0;
        public string ErrorMessage { get; set; } = "";
        public string FFMPEGOutput { get; set; } = "";

        // Output Properties
        public string Acodec { get; set; } = "";
        public string Vcodec { get; set; } = "";
        public string SamplingRate { get; set; } = "";
        public string Channel { get; set; } = "";
        public string Audio_Bitrate { get; set; } = "";
        public string Video_Bitrate { get; set; } = "";
        public int Width { get; set; } = 0;
        public int Height { get; set; } = 0;
        public string FrameRate { get; set; } = "";
        public string Type { get; set; } = "";

        // Input Properties
        public string Input_Acodec { get; set; } = "";
        public string Input_Vcodec { get; set; } = "";
        public string Input_SamplingRate { get; set; } = "";
        public string Input_Channel { get; set; } = "";
        public string Input_Audio_Bitrate { get; set; } = "";
        public string Input_Video_Bitrate { get; set; } = "";
        public int Input_Width { get; set; } = 0;
        public int Input_Height { get; set; } = 0;
        public string Input_FrameRate { get; set; } = "";
        public string Input_Type { get; set; } = "";
        public string Music { get; set; } = "";
        public string Footage { get; set; } = "";
        public string Producer { get; set; } = "";
        public string Title { get; set; } = "";
    }
}

/*
 * This file is subject to the terms and conditions defined in
 * file 'LICENSE.md', which is part of this source code package.
 * Copyright 2007 - 2020 MediaSoftPro
 * For more information email at support@mediasoftpro.com
 */
