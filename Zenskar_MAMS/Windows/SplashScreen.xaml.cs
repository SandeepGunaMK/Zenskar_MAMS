using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Zenskar_MAMS.Windows
{
    public partial class SplashScreen : Window
    {
        private readonly DispatcherTimer _timer;
        private Window _nextWindow;

        public SplashScreen()
        {
            try
            {
                InitializeComponent();
                
                // Start the progress animation
                var storyboard = (Storyboard)FindResource("LoadingAnimation");
                storyboard?.Begin();

                // Set up the timer
                _timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(3)
                };
                _timer.Tick += Timer_Tick;
                _timer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing application: {ex.Message}", 
                    "Initialization Error", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            try
            {
                _timer.Stop();
                _nextWindow = new Login();
                _nextWindow.Show();
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading login window: {ex.Message}", 
                    "Error", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _timer?.Stop();
        }
    }
}