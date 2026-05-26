using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Overlay_Redux
{
    public partial class RespawnWindow : Window
    {
        public RespawnWindow()
        {
            InitializeComponent();
        }

        public void AddBanner(string team, List<string> players, int duration = 5000)
        {
            var banner = new TextBlock
            {
                Text = $"{team} respawned {string.Join(", and ", players)}",
                FontFamily = new FontFamily("Arial"),
                FontSize = 24,
                Foreground = new SolidColorBrush(Colors.Black),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#90EE90")),
                Padding = new Thickness(8)
            };

            BannerStack.Children.Add(banner);

            if (duration > 0)
            {
                var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(duration) };
                timer.Tick += (s, e) =>
                {
                    timer.Stop();
                    RemoveBanner(banner);
                };
                timer.Start();
            }
        }

        private void RemoveBanner(TextBlock banner)
        {
            BannerStack.Children.Remove(banner);
        }
    }
}