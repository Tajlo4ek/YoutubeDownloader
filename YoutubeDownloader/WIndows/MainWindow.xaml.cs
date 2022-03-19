using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using TjkYoutubeDL;
using TjkYoutubeDL.Utils;
using YoutubeDownloader.Utils;

namespace YoutubeDownloader
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly YoutubeDL ytdl;

        private VideoFormat.Formats curFormat = VideoFormat.Formats.Unknow;

        private DataConfig currentConfig;

        public MainWindow()
        {
            InitializeComponent();

            ytdl = new YoutubeDL();
            ytdl.SetFFMpegPath("Resources/ffmpeg.exe");
            ytdl.SetYoutubeDLPath("Resources/youtube-dl.exe");
            ytdl.SetMaxThreadCount(3);

            System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("en-US");
            System.Threading.Thread.CurrentThread.CurrentCulture = ci;
            System.Threading.Thread.CurrentThread.CurrentUICulture = ci;

            foreach (VideoFormat.Formats format in Enum.GetValues(typeof(VideoFormat.Formats)))
            {
                if (format != VideoFormat.Formats.Unknow)
                {
                    cbFormat.Items.Add(format.ToString());
                }
            }
            cbFormat.SelectedIndex = 0;
            LoadConfig();
        }

        private void LoadConfig()
        {
            currentConfig = DataLoader.Load();
            ytdl.DownloadPath = currentConfig.SavePath;
        }

        private void BtnInfo_Click(object sender, RoutedEventArgs e)
        {
            btnFind.IsEnabled = false;

            ytdl.GetInfo(tbUrl.Text, (info) => AddVideoData(info),
                (res) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        btnFind.IsEnabled = true;
                    });

                    if (!res)
                    {
                        Dispatcher.Invoke(() => { labelFindRes.Content = "Some video not available"; });
                    }
                });
        }

        private void RemoveVideoInfo(VideoInfoControl control)
        {
            Dispatcher.Invoke(() =>
            {
                stackPanel.Children.Remove(control);
            });
        }

        private void Download(VideoInfoControl control)
        {
            Download(new VideoInfoControl[] { control }, control.GetDownloadFormat(), control.GetDownloadExt());
        }

        private void Download(VideoInfoControl[] controls, VideoFormat.Formats videoFormat, VideoFormat.FileExt fileExt)
        {
            for (int i = 0; i < controls.Length; i++)
            {
                var current = controls[i];

                if (!current.CurrentVideoInfo.IsFormatAvailable(videoFormat))
                {
                    current.Reset();
                    current.SetLog(videoFormat.ToString() + " not available for this");
                    continue;
                }

                ytdl.Download(current.CurrentVideoInfo, videoFormat, fileExt, (logType, args) =>
                {

                    switch (logType)
                    {
                        case YoutubeDL.LogType.Begin:
                            current.SetLog(args[0]);
                            break;
                        case YoutubeDL.LogType.Convert:
                            current.SetProgress(float.Parse(args[0]), "converting (~ " + args[1] + " )");
                            break;
                        case YoutubeDL.LogType.End:
                            if (args.Length == 0)
                            {
                                current.SetLog("downloaded");
                            }
                            else
                            {
                                current.SetLog(args[0]);
                            }
                            current.SetEnd();
                            break;
                        case YoutubeDL.LogType.Download:
                            current.SetProgress(float.Parse(args[0]), args[1] + " (~" + args[2] + ")");
                            break;
                        case YoutubeDL.LogType.Abort:
                            if (args.Length == 0 || args[0].Length == 0)
                            {
                                current.SetError("error");
                            }
                            else
                            {
                                current.SetError(args[0]);
                            }
                            break;
                        case YoutubeDL.LogType.Wait:
                            current.SetLog("wait");
                            current.Block();
                            break;
                    }

                    if (logType == YoutubeDL.LogType.End)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            if (cbRemoveAfter.IsChecked == true)
                            {
                                RemoveVideoInfo(current);
                            }
                        });
                    }

                });
            }
        }

        private void AddVideoData(VideoInfo info)
        {
            Dispatcher.Invoke(() =>
            {
                var infoControl = new VideoInfoControl(info);
                infoControl.AddOnDownloadClick(Download);
                infoControl.AddOnCloseClick(RemoveVideoInfo);
                stackPanel.Children.Add(infoControl);
            });
        }

        private void BtnDownloadAll_Click(object sender, RoutedEventArgs e)
        {
            if (stackPanel.Children.Count == 0)
            {
                return;
            }

            var list = new List<VideoInfoControl>(stackPanel.Children.Count);

            foreach (VideoInfoControl control in stackPanel.Children)
            {
                if (!control.IsBlock)
                {
                    control.Reset();
                    list.Add(control);
                }
            }

            Download(
                list.ToArray(),
                curFormat,
                (VideoFormat.FileExt)Enum.Parse(typeof(VideoFormat.FileExt), cbExt.SelectedItem.ToString()));
        }

        private void CbFormat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var nowFormat = (VideoFormat.Formats)Enum.Parse(typeof(VideoFormat.Formats), cbFormat.SelectedItem.ToString());

            if (nowFormat.IsAudio() != curFormat.IsAudio() || curFormat == VideoFormat.Formats.Unknow)
            {
                curFormat = nowFormat;

                cbExt.Items.Clear();

                if (curFormat.IsAudio())
                {
                    foreach (var ext in FormatsExtension.GetAudioExt())
                    {
                        cbExt.Items.Add(ext.ToString());
                    }
                }

                if (curFormat.IsVideo())
                {
                    foreach (var ext in FormatsExtension.GetVideoExt())
                    {
                        cbExt.Items.Add(ext.ToString());
                    }
                }

                cbExt.SelectedIndex = 0;
            }
        }

        private void BtnConfig_Click(object sender, RoutedEventArgs e)
        {
            var configWindow = new ConfigWindow(currentConfig);
            configWindow.ShowDialog();
            LoadConfig();
        }

    }
}
