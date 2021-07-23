using System.Windows;
using YoutubeDownloader.Utils;

namespace YoutubeDownloader
{
    /// <summary>
    /// Логика взаимодействия для ConfigWindow.xaml
    /// </summary>
    public partial class ConfigWindow : Window
    {
        public ConfigWindow()
        {
            InitializeComponent();
        }

        public ConfigWindow(DataConfig dataConfig) : this()
        {
            InitializeComponent();
            tbSavePath.Text = dataConfig.SavePath;
        }

        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();

            if (dialog.ShowDialog() ==  System.Windows.Forms.DialogResult.OK)
            {
                tbSavePath.Text = dialog.SelectedPath;
            }

        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var config = new DataConfig(tbSavePath.Text);
            DataLoader.Save(config);
            this.Close();
        }
    }
}
