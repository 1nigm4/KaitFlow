using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Vlc.DotNet.Core.Interops.Signatures;

namespace Flow
{
    public partial class MainWindow : Window
    {
        private string _streamUri;
        public long _lastTime;

        public MainWindow()
        {
            InitializeComponent();

            var currentAssembly = Assembly.GetEntryAssembly();
            var currentDirectory = new FileInfo(currentAssembly.Location).DirectoryName;
            var libDirectory = new DirectoryInfo(Path.Combine(currentDirectory, "libvlc", IntPtr.Size == 4 ? "win-x86" : "win-x64"));
            VlcPlayer.SourceProvider.CreatePlayer(libDirectory, null);

            _streamUri = ConfigurationManager.AppSettings.Get("StreamUri");

            var files = Directory.GetFiles(currentDirectory, "Screen.*");
            if (files.Length != 0)
            {
                string screenUri = files[0];
                Screen.Source = new BitmapImage(new Uri(screenUri));
            }

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 3);
            timer.Tick += Timer_Tick;
            timer.Start();
            StartPlayer();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            var state = VlcPlayer.SourceProvider.MediaPlayer?.State;
            long currentTime = VlcPlayer.SourceProvider.MediaPlayer?.Time ?? 0;
            bool isStopped = currentTime != 0 && _lastTime == currentTime;
            _lastTime = currentTime;
            if (state != MediaStates.Playing || isStopped) StartPlayer();
        }

        private void StartPlayer()
        {
            VlcPlayer.SourceProvider.MediaPlayer.SetMedia(_streamUri);
            VlcPlayer.SourceProvider.MediaPlayer.Play();
            VlcPlayer.SourceProvider.MediaPlayer.Playing += (o, e) => Dispatcher.Invoke(() => Screen.Visibility = Visibility.Hidden);
            VlcPlayer.SourceProvider.MediaPlayer.EndReached += (o, e) => Dispatcher.Invoke(() => Screen.Visibility = Visibility.Visible);
        }

        private void Panel_MouseEnter(object sender, MouseEventArgs e) => Panel.Opacity = 100;
        private void Panel_MouseLeave(object sender, MouseEventArgs e) => Panel.Opacity = 0;
        private void ExitButton_Click(object sender, RoutedEventArgs e) => Environment.Exit(0);
        private void PopupButton_Click(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;
    }
}
