using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using EnvironMint.Styles;

namespace EnvironMint.Controls
{
    public partial class AnimatedCard : UserControl
    {
        public static readonly DependencyProperty IsHoverAnimationEnabledProperty =
            DependencyProperty.Register("IsHoverAnimationEnabled", typeof(bool), typeof(AnimatedCard), new PropertyMetadata(true));

        public static readonly DependencyProperty IsClickAnimationEnabledProperty =
            DependencyProperty.Register("IsClickAnimationEnabled", typeof(bool), typeof(AnimatedCard), new PropertyMetadata(true));

        public static readonly DependencyProperty EntranceAnimationTypeProperty =
            DependencyProperty.Register("EntranceAnimationType", typeof(EntranceAnimationType), typeof(AnimatedCard), new PropertyMetadata(EntranceAnimationType.FadeIn));

        public bool IsHoverAnimationEnabled
        {
            get { return (bool)GetValue(IsHoverAnimationEnabledProperty); }
            set { SetValue(IsHoverAnimationEnabledProperty, value); }
        }

        public bool IsClickAnimationEnabled
        {
            get { return (bool)GetValue(IsClickAnimationEnabledProperty); }
            set { SetValue(IsClickAnimationEnabledProperty, value); }
        }

        public EntranceAnimationType EntranceAnimationType
        {
            get { return (EntranceAnimationType)GetValue(EntranceAnimationTypeProperty); }
            set { SetValue(EntranceAnimationTypeProperty, value); }
        }

        private Storyboard _entranceStoryboard;
        private Storyboard _hoverStoryboard;
        private Storyboard _clickStoryboard;

        public AnimatedCard()
        {
            InitializeComponent();

            this.Opacity = 0;

            this.Loaded += AnimatedCard_Loaded;
            this.MouseEnter += AnimatedCard_MouseEnter;
            this.MouseLeave += AnimatedCard_MouseLeave;
            this.PreviewMouseDown += AnimatedCard_PreviewMouseDown;
        }

        private void AnimatedCard_Loaded(object sender, RoutedEventArgs e)
        {
            PlayEntranceAnimation();
        }

        private void PlayEntranceAnimation()
        {
            switch (EntranceAnimationType)
            {
                case EntranceAnimationType.FadeIn:
                    _entranceStoryboard = Animations.CreateFadeInAnimation(this);
                    break;
                case EntranceAnimationType.SlideInFromRight:
                    _entranceStoryboard = Animations.CreateSlideInFromRightAnimation(this);
                    break;
                case EntranceAnimationType.SlideInFromBottom:
                    _entranceStoryboard = Animations.CreateSlideInFromBottomAnimation(this);
                    break;
                case EntranceAnimationType.Scale:
                    _entranceStoryboard = Animations.CreateScaleAnimation(this);
                    break;
                default:
                    _entranceStoryboard = Animations.CreateFadeInAnimation(this);
                    break;
            }

            _entranceStoryboard.Begin();
        }

        private void AnimatedCard_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!IsHoverAnimationEnabled)
                return;

            _hoverStoryboard = new Storyboard();

            var shadowAnimation = new DoubleAnimation
            {
                To = 10,
                Duration = Animations.ShortDuration,
                EasingFunction = Animations.EaseOut
            };

            Storyboard.SetTarget(shadowAnimation, CardBorder);
            Storyboard.SetTargetProperty(shadowAnimation, new PropertyPath("Effect.(DropShadowEffect.ShadowDepth)"));

            // Animate blur radius
            var blurAnimation = new DoubleAnimation
            {
                To = 15,
                Duration = Animations.ShortDuration,
                EasingFunction = Animations.EaseOut
            };

            Storyboard.SetTarget(blurAnimation, CardBorder);
            Storyboard.SetTargetProperty(blurAnimation, new PropertyPath("Effect.(DropShadowEffect.BlurRadius)"));

            // Animate slight scale
            var scaleTransform = new ScaleTransform(1, 1);
            CardBorder.RenderTransform = scaleTransform;
            CardBorder.RenderTransformOrigin = new Point(0.5, 0.5);

            var scaleXAnimation = new DoubleAnimation
            {
                To = 1.02,
                Duration = Animations.ShortDuration,
                EasingFunction = Animations.EaseOut
            };

            var scaleYAnimation = new DoubleAnimation
            {
                To = 1.02,
                Duration = Animations.ShortDuration,
                EasingFunction = Animations.EaseOut
            };

            Storyboard.SetTarget(scaleXAnimation, CardBorder);
            Storyboard.SetTargetProperty(scaleXAnimation, new PropertyPath("RenderTransform.ScaleX"));

            Storyboard.SetTarget(scaleYAnimation, CardBorder);
            Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath("RenderTransform.ScaleY"));

            _hoverStoryboard.Children.Add(shadowAnimation);
            _hoverStoryboard.Children.Add(blurAnimation);
            _hoverStoryboard.Children.Add(scaleXAnimation);
            _hoverStoryboard.Children.Add(scaleYAnimation);

            _hoverStoryboard.Begin();
        }

        private void AnimatedCard_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!IsHoverAnimationEnabled)
                return;

            _hoverStoryboard = new Storyboard();

            var shadowAnimation = new DoubleAnimation
            {
                To = 5,
                Duration = Animations.ShortDuration,
                EasingFunction = Animations.EaseOut
            };

            Storyboard.SetTarget(shadowAnimation, CardBorder);
            Storyboard.SetTargetProperty(shadowAnimation, new PropertyPath("Effect.(DropShadowEffect.ShadowDepth)"));

            // Animate blur radius
            var blurAnimation = new DoubleAnimation
            {
                To = 10,
                Duration = Animations.ShortDuration,
                EasingFunction = Animations.EaseOut
            };

            Storyboard.SetTarget(blurAnimation, CardBorder);
            Storyboard.SetTargetProperty(blurAnimation, new PropertyPath("Effect.(DropShadowEffect.BlurRadius)"));

            var scaleXAnimation = new DoubleAnimation
            {
                To = 1.0,
                Duration = Animations.ShortDuration,
                EasingFunction = Animations.EaseOut
            };

            var scaleYAnimation = new DoubleAnimation
            {
                To = 1.0,
                Duration = Animations.ShortDuration,
                EasingFunction = Animations.EaseOut
            };

            Storyboard.SetTarget(scaleXAnimation, CardBorder);
            Storyboard.SetTargetProperty(scaleXAnimation, new PropertyPath("RenderTransform.ScaleX"));

            Storyboard.SetTarget(scaleYAnimation, CardBorder);
            Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath("RenderTransform.ScaleY"));

            _hoverStoryboard.Children.Add(shadowAnimation);
            _hoverStoryboard.Children.Add(blurAnimation);
            _hoverStoryboard.Children.Add(scaleXAnimation);
            _hoverStoryboard.Children.Add(scaleYAnimation);

            _hoverStoryboard.Begin();
        }

        private void AnimatedCard_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsClickAnimationEnabled)
                return;

            _clickStoryboard = Animations.CreatePulseAnimation(CardBorder, 0.98);
            _clickStoryboard.Begin();
        }
    }

    public enum EntranceAnimationType
    {
        FadeIn,
        SlideInFromRight,
        SlideInFromBottom,
        Scale
    }
}

