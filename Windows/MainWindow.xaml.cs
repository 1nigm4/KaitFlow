using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Vlc.DotNet.Core.Interops.Signatures;

namespace Flow.Windows
{
    public partial class MainWindow : Window
    {
        private string _currentDirectory;
        public long _lastTime;

        public MainWindow()
        {
            InitializeComponent();

            this.SetBackground();

            var currentAssembly = Assembly.GetEntryAssembly();
            _currentDirectory = new FileInfo(currentAssembly.Location).DirectoryName;
            var libDirectory = new DirectoryInfo(Path.Combine(_currentDirectory, "libvlc", IntPtr.Size == 4 ? "win-x86" : "win-x64"));

            VlcPlayer.SourceProvider.CreatePlayer(libDirectory, null);
            VlcPlayer.SourceProvider.MediaPlayer.Playing += MediaPlayer_Playing;
            VlcPlayer.SourceProvider.MediaPlayer.Stopped += MediaPlayer_Stopped;
            VlcPlayer.SourceProvider.MediaPlayer.EndReached += MediaPlayer_Stopped;
            StartPlayer();

            DispatcherTimer flowTimeout = new DispatcherTimer();
            flowTimeout.Interval = new TimeSpan(0, 0, 3);
            flowTimeout.Tick += FlowTimeout_Tick;
            flowTimeout.Start();
        }

        private void FlowTimeout_Tick(object? sender, EventArgs e)
        {
            var state = VlcPlayer.SourceProvider.MediaPlayer.State;
            long currentTime = VlcPlayer.SourceProvider.MediaPlayer.Time;
            bool isStopped = currentTime != 0 && _lastTime == currentTime;
            _lastTime = currentTime;
            if (state != MediaStates.Playing || isStopped) StartPlayer();
        }

        private void StartPlayer()
        {
            string streamUri = ConfigurationManager.AppSettings.Get("StreamUrl");
            VlcPlayer.SourceProvider.MediaPlayer.SetMedia(streamUri);
            VlcPlayer.SourceProvider.MediaPlayer.Play();
        }

        private void SetBackground()
        {
            string backgroundUrl = ConfigurationManager.AppSettings.Get("BackgroundUrl");
            if (!File.Exists(backgroundUrl))
                backgroundUrl = $"{_currentDirectory}{backgroundUrl}";
            Background.Source = new BitmapImage(new Uri(backgroundUrl));
        }

        private void MediaPlayer_Playing(object? sender, EventArgs e) =>
            Dispatcher.Invoke(() => Background.Visibility = Visibility.Hidden);

        private void MediaPlayer_Stopped(object? sender, EventArgs e) =>
            Dispatcher.Invoke(() => Background.Visibility = Visibility.Visible);

        private void Panel_MouseEnter(object sender, MouseEventArgs e) => Panel.Opacity = 100;
        private void Panel_MouseLeave(object sender, MouseEventArgs e) => Panel.Opacity = 0;
        private void ExitButton_Click(object sender, RoutedEventArgs e) => Environment.Exit(0);
        private void PopupButton_Click(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            OptionsWindow window = new OptionsWindow();
            window.Owner = this;
            var hasChanges = window.ShowDialog() ?? false;
            if (hasChanges) this.SetBackground();
        }
    }
}
