using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TjkYoutubeDL;
using TjkYoutubeDL.Utils;
using static TjkYoutubeDL.VideoFormat;

namespace YoutubeDownloader
{
    /// <summary>
    /// Логика взаимодействия для VideoInfoControl.xaml
    /// </summary>
    public partial class VideoInfoControl : UserControl
    {
        public VideoInfo CurrentVideoInfo { get; private set; }

        private VideoFormat.Formats curFormat = VideoFormat.Formats.Unknow;

        public bool IsBlock { get; private set; }

        private Action<VideoInfoControl> onDownloadClick;
        private Action<VideoInfoControl> onCloseClick;

        public VideoInfoControl()
        {
            InitializeComponent();
            CurrentVideoInfo = null;
            labelLog.Foreground = Brushes.Black;
            IsBlock = false;
        }

        public VideoInfoControl(VideoInfo info) : this()
        {
            SetData(info);
        }

        public void AddOnDownloadClick(Action<VideoInfoControl> onDownloadClick)
        {
            this.onDownloadClick += onDownloadClick;
        }

        public void AddOnCloseClick(Action<VideoInfoControl> onCloseClick)
        {
            this.onCloseClick += onCloseClick;
        }

        public void SetData(VideoInfo videoInfo)
        {
            CurrentVideoInfo = videoInfo;

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(CurrentVideoInfo.Thumbnails[0].Url, UriKind.Absolute);
            bitmap.EndInit();

            image.Stretch = Stretch.Uniform;
            image.Source = bitmap;

            tbName.Text = CurrentVideoInfo.Title;

            foreach (var format in CurrentVideoInfo.AvailableFormats)
            {
                cbFormat.Items.Add(format.ToString());
            }
            cbFormat.SelectedIndex = 0;

            progBar.Value = 0;
            labelLog.Content = "";
        }

        private void BtnDownload_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Reset();
            Block();
            onDownloadClick?.Invoke(this);
        }

        public void Block()
        {
            cbExt.IsEnabled = false;
            cbFormat.IsEnabled = false;
            btnClose.IsEnabled = false;
            btnDownLoad.IsEnabled = false;
            IsBlock = true;
        }

        public VideoFormat.Formats GetDownloadFormat()
        {
            return (VideoFormat.Formats)Enum.Parse(typeof(VideoFormat.Formats), cbFormat.SelectedItem.ToString());
        }

        public VideoFormat.FileExt GetDownloadExt()
        {
            return (VideoFormat.FileExt)Enum.Parse(typeof(VideoFormat.FileExt), cbExt.SelectedItem.ToString());
        }

        public void Reset()
        {
            progBar.Value = 0;
            labelLog.Content = "";
            labelLog.Foreground = Brushes.Black;
        }

        public void SetProgress(float progress, string log)
        {
            Dispatcher.Invoke(() =>
            {
                progBar.Value = progress;
                labelLog.Content = log;
            });
        }

        public void SetLog(string log)
        {
            Dispatcher.Invoke(() =>
            {
                labelLog.Content = log;
            });
        }

        public void SetError(string log)
        {
            Dispatcher.Invoke(() =>
            {
                labelLog.Content = log;
                labelLog.Foreground = Brushes.Red;
                SetEnd();
            });
        }

        public void SetEnd()
        {
            Dispatcher.Invoke(() =>
            {
                cbExt.IsEnabled = true;
                cbFormat.IsEnabled = true;
                btnClose.IsEnabled = true;
                btnDownLoad.IsEnabled = true;
                IsBlock = false;
                progBar.Value = 100;
            });
        }

        private void CbFormat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var nowFormat = GetDownloadFormat();

            if (nowFormat.IsAudio() != curFormat.IsAudio() || curFormat == Formats.Unknow)
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

            Reset();
        }

        private void CbExt_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Reset();
        }

        private void BtnClose_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            onCloseClick?.Invoke(this);
        }

    }
}
