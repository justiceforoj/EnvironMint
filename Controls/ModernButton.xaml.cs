using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using EnvironMint.Styles;

namespace EnvironMint.Controls
{
    public partial class ModernButton : UserControl
    {
        public static readonly DependencyProperty ButtonTextProperty =
            DependencyProperty.Register("ButtonText", typeof(string), typeof(ModernButton), new PropertyMetadata("Button"));

        public static readonly DependencyProperty ButtonTypeProperty =
            DependencyProperty.Register("ButtonType", typeof(ButtonType), typeof(ModernButton), new PropertyMetadata(ButtonType.Primary, OnButtonTypeChanged));

        public static readonly DependencyProperty ClickCommandProperty =
            DependencyProperty.Register("ClickCommand", typeof(System.Windows.Input.ICommand), typeof(ModernButton), new PropertyMetadata(null));

        public static readonly DependencyProperty ClickCommandParameterProperty =
            DependencyProperty.Register("ClickCommandParameter", typeof(object), typeof(ModernButton), new PropertyMetadata(null));

        public static readonly DependencyProperty IsAnimatedProperty =
            DependencyProperty.Register("IsAnimated", typeof(bool), typeof(ModernButton), new PropertyMetadata(true));

        public string ButtonText
        {
            get { return (string)GetValue(ButtonTextProperty); }
            set { SetValue(ButtonTextProperty, value); }
        }

        public ButtonType ButtonType
        {
            get { return (ButtonType)GetValue(ButtonTypeProperty); }
            set { SetValue(ButtonTypeProperty, value); }
        }

        public System.Windows.Input.ICommand ClickCommand
        {
            get { return (System.Windows.Input.ICommand)GetValue(ClickCommandProperty); }
            set { SetValue(ClickCommandProperty, value); }
        }

        public object ClickCommandParameter
        {
            get { return GetValue(ClickCommandParameterProperty); }
            set { SetValue(ClickCommandParameterProperty, value); }
        }

        public bool IsAnimated
        {
            get { return (bool)GetValue(IsAnimatedProperty); }
            set { SetValue(IsAnimatedProperty, value); }
        }

        public ModernButton()
        {
            InitializeComponent();
        }

        private static void OnButtonTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var button = d as ModernButton;
            if (button != null)
            {
                button.UpdateButtonStyle();
            }
        }

        private void UpdateButtonStyle()
        {
            switch (ButtonType)
            {
                case ButtonType.Primary:
                    InnerButton.Style = (Style)FindResource("PrimaryButtonStyle");
                    break;
                case ButtonType.Secondary:
                    InnerButton.Style = (Style)FindResource("SecondaryButtonStyle");
                    break;
                case ButtonType.Accent:
                    InnerButton.Style = (Style)FindResource("AccentButtonStyle");
                    break;
                case ButtonType.Danger:
                    InnerButton.Style = (Style)FindResource("DangerButtonStyle");
                    break;
                case ButtonType.Success:
                    InnerButton.Style = (Style)FindResource("SuccessButtonStyle");
                    break;
                default:
                    InnerButton.Style = (Style)FindResource("PrimaryButtonStyle");
                    break;
            }
        }

        private void InnerButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsAnimated)
            {
                var clickAnimation = Animations.CreatePulseAnimation(InnerButton, 0.95, Animations.ShortDuration);
                clickAnimation.Begin();
            }

            if (ClickCommand != null && ClickCommand.CanExecute(ClickCommandParameter))
            {
                ClickCommand.Execute(ClickCommandParameter);
            }

            RaiseEvent(new RoutedEventArgs(Button.ClickEvent, this));
        }
    }

    public enum ButtonType
    {
        Primary,
        Secondary,
        Accent,
        Danger,
        Success
    }
}

