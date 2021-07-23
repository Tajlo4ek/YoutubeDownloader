using Newtonsoft.Json;
using System;
using System.IO;

namespace YoutubeDownloader.Utils
{
    public class DataConfig
    {
        private static readonly string defaultPath = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);


        [JsonProperty]
        public string SavePath { get; private set; }

        private DataConfig()
        {
            SavePath = defaultPath;
        }

        public DataConfig(string savePath)
        {
            if (Directory.Exists(savePath))
            {
                SavePath = savePath;
            }
            else
            {
                SavePath = defaultPath;
            }
        }

        public string GetJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static DataConfig Parse(string json)
        {
            return JsonConvert.DeserializeObject<DataConfig>(json);
        }

        public static DataConfig GetDefault()
        {
            var config = new DataConfig(defaultPath);
            return config;
        }


    }

}
