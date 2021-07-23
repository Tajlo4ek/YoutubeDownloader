using System;
using System.IO;

namespace YoutubeDownloader.Utils
{
    public static class DataLoader
    {
        private static readonly string directory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/tjk/youtubeDownloader/";

        private static readonly string saveFileName = "initConfig.json";

        private static readonly string fullPath = directory + saveFileName;

        public static DataConfig Load()
        {
            try
            {
                using (StreamReader sr = new StreamReader(fullPath))
                {
                    var json = sr.ReadToEnd();
                    return DataConfig.Parse(json);
                }
            }
            catch (Exception)
            {
                var config = DataConfig.GetDefault();
                Save(config);
                return config;
            }
        }

        public static void Save(DataConfig dataConfig)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (StreamWriter sw = new StreamWriter(fullPath))
            {
                var json = dataConfig.GetJson();
                sw.WriteLine(json);
            }

        }

    }
}
