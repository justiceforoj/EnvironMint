using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using EnvironMint.Styles;
using System.ComponentModel;

namespace EnvironMint.Controls
{
    public partial class NotificationBanner : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(string), typeof(NotificationBanner),
                new PropertyMetadata("Notification", OnMessageChanged));

        public static readonly DependencyProperty NotificationTypeProperty =
            DependencyProperty.Register("NotificationType", typeof(NotificationType), typeof(NotificationBanner), new PropertyMetadata(NotificationType.Info, OnNotificationTypeChanged));

        public static readonly DependencyProperty AutoCloseProperty =
            DependencyProperty.Register("AutoClose", typeof(bool), typeof(NotificationBanner), new PropertyMetadata(true));

        public static readonly DependencyProperty AutoCloseTimeProperty =
            DependencyProperty.Register("AutoCloseTime", typeof(int), typeof(NotificationBanner), new PropertyMetadata(5000));

        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set
            {
                SetValue(MessageProperty, value);
                OnPropertyChanged("Message");
                if (MessageText != null)
                    MessageText.Text = value;
            }
        }

        public NotificationType NotificationType
        {
            get { return (NotificationType)GetValue(NotificationTypeProperty); }
            set { SetValue(NotificationTypeProperty, value); }
        }

        public bool AutoClose
        {
            get { return (bool)GetValue(AutoCloseProperty); }
            set { SetValue(AutoCloseProperty, value); }
        }

        public int AutoCloseTime
        {
            get { return (int)GetValue(AutoCloseTimeProperty); }
            set { SetValue(AutoCloseTimeProperty, value); }
        }

        public event EventHandler Closed;

        private System.Windows.Threading.DispatcherTimer _autoCloseTimer;

        public NotificationBanner()
        {
            InitializeComponent();

            this.Opacity = 0;

            this.Loaded += NotificationBanner_Loaded;

            MessageText.Text = Message;

            this.PropertyChanged += (s, e) => {
                if (e.PropertyName == "Message")
                {
                    MessageText.Text = Message;
                }
            };
        }

        private void NotificationBanner_Loaded(object sender, RoutedEventArgs e)
        {
            var entranceAnimation = Animations.CreateSlideInFromRightAnimation(this);
            entranceAnimation.Begin();

            if (AutoClose)
            {
                _autoCloseTimer = new System.Windows.Threading.DispatcherTimer();
                _autoCloseTimer.Interval = TimeSpan.FromMilliseconds(AutoCloseTime);
                _autoCloseTimer.Tick += (s, args) => Close();
                _autoCloseTimer.Start();
            }
        }

        private static void OnNotificationTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var banner = d as NotificationBanner;
            if (banner != null)
            {
                banner.UpdateNotificationStyle();
            }
        }

        private static void OnMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var banner = d as NotificationBanner;
            if (banner != null && banner.MessageText != null)
            {
                banner.MessageText.Text = e.NewValue as string;
            }
        }

        private void UpdateNotificationStyle()
        {
            switch (NotificationType)
            {
                case NotificationType.Success:
                    MainBorder.Background = ColorScheme.SuccessBrush;
                    IconPath.Data = Geometry.Parse("M9,20.42L2.79,14.21L5.62,11.38L9,14.77L18.88,4.88L21.71,7.71L9,20.42Z");
                    IconPath.Fill = Brushes.White;
                    MessageText.Foreground = Brushes.White;
                    break;
                case NotificationType.Warning:
                    MainBorder.Background = ColorScheme.WarningBrush;
                    IconPath.Data = Geometry.Parse("M13,14H11V10H13M13,18H11V16H13M1,21H23L12,2L1,21Z");
                    IconPath.Fill = Brushes.White;
                    MessageText.Foreground = Brushes.White;
                    break;
                case NotificationType.Error:
                    MainBorder.Background = ColorScheme.ErrorBrush;
                    IconPath.Data = Geometry.Parse("M13,13H11V7H13M13,17H11V15H13M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2Z");
                    IconPath.Fill = Brushes.White;
                    MessageText.Foreground = Brushes.White;
                    break;
                case NotificationType.Info:
                default:
                    MainBorder.Background = ColorScheme.InfoBrush;
                    IconPath.Data = Geometry.Parse("M13,9H11V7H13M13,17H11V11H13M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2Z");
                    IconPath.Fill = Brushes.White;
                    MessageText.Foreground = Brushes.White;
                    break;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public void Close()
        {
            if (_autoCloseTimer != null && _autoCloseTimer.IsEnabled)
            {
                _autoCloseTimer.Stop();
            }

            var exitAnimation = Animations.CreateFadeOutAnimation(this);
            exitAnimation.Completed += (s, e) =>
            {
                // Raise closed event
                Closed?.Invoke(this, EventArgs.Empty);
            };
            exitAnimation.Begin();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Error
    }
}
