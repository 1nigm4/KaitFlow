using System.Configuration;
using System.Windows;

namespace Flow.Windows
{
    public partial class OptionsWindow : Window
    {
        public OptionsWindow()
        {
            InitializeComponent();
            StreamUrl.Text = ConfigurationManager.AppSettings.Get("StreamUrl");
            BackgroundUrl.Text = ConfigurationManager.AppSettings.Get("BackgroundUrl");
        }

        private void BackgroundUrl_DragOver(object sender, DragEventArgs e) => e.Handled = true;
        private void BackgroundUrl_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                BackgroundUrl.Text = files[0];
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string streamUrl = StreamUrl.Text;
            string backgroundUrl = BackgroundUrl.Text;
            if (string.IsNullOrWhiteSpace(streamUrl) || string.IsNullOrWhiteSpace(backgroundUrl)) return;

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["StreamUrl"].Value = streamUrl;
            config.AppSettings.Settings["BackgroundUrl"].Value = backgroundUrl;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");

            this.DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}
