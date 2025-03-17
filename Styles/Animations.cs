using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace EnvironMint.Styles
{
    public static class Animations
    {
        // durations
        public static readonly Duration ShortDuration = new Duration(TimeSpan.FromMilliseconds(150));
        public static readonly Duration MediumDuration = new Duration(TimeSpan.FromMilliseconds(300));
        public static readonly Duration LongDuration = new Duration(TimeSpan.FromMilliseconds(500));

        // easing
        public static readonly IEasingFunction EaseOut = new CubicEase { EasingMode = EasingMode.EaseOut };
        public static readonly IEasingFunction EaseIn = new CubicEase { EasingMode = EasingMode.EaseIn };
        public static readonly IEasingFunction EaseInOut = new CubicEase { EasingMode = EasingMode.EaseInOut };

        public static Storyboard CreateFadeInAnimation(UIElement element, Duration? duration = null)
        {
            Duration animDuration = duration ?? MediumDuration;

            var storyboard = new Storyboard();
            var animation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = animDuration,
                EasingFunction = EaseOut
            };

            Storyboard.SetTarget(animation, element);
            Storyboard.SetTargetProperty(animation, new PropertyPath("Opacity"));

            storyboard.Children.Add(animation);
            return storyboard;
        }

        public static Storyboard CreateFadeOutAnimation(UIElement element, Duration? duration = null)
        {
            Duration animDuration = duration ?? MediumDuration;

            var storyboard = new Storyboard();
            var animation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = animDuration,
                EasingFunction = EaseIn
            };

            Storyboard.SetTarget(animation, element);
            Storyboard.SetTargetProperty(animation, new PropertyPath("Opacity"));

            storyboard.Children.Add(animation);
            return storyboard;
        }

        public static Storyboard CreateSlideInFromRightAnimation(FrameworkElement element, double offset = 50, Duration? duration = null)
        {
            Duration animDuration = duration ?? MediumDuration;

            var storyboard = new Storyboard();

            if (element.RenderTransform == null || !(element.RenderTransform is TranslateTransform))
            {
                element.RenderTransform = new TranslateTransform();
            }

            var xAnimation = new DoubleAnimation
            {
                From = offset,
                To = 0,
                Duration = animDuration,
                EasingFunction = EaseOut
            };

            Storyboard.SetTarget(xAnimation, element);
            Storyboard.SetTargetProperty(xAnimation, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));

            var opacityAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = animDuration,
                EasingFunction = EaseOut
            };

            Storyboard.SetTarget(opacityAnimation, element);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("Opacity"));

            storyboard.Children.Add(xAnimation);
            storyboard.Children.Add(opacityAnimation);

            return storyboard;
        }

        public static Storyboard CreateSlideInFromBottomAnimation(FrameworkElement element, double offset = 30, Duration? duration = null)
        {
            Duration animDuration = duration ?? MediumDuration;

            var storyboard = new Storyboard();

            if (element.RenderTransform == null || !(element.RenderTransform is TranslateTransform))
            {
                element.RenderTransform = new TranslateTransform();
            }

            var yAnimation = new DoubleAnimation
            {
                From = offset,
                To = 0,
                Duration = animDuration,
                EasingFunction = EaseOut
            };

            Storyboard.SetTarget(yAnimation, element);
            Storyboard.SetTargetProperty(yAnimation, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));

            var opacityAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = animDuration,
                EasingFunction = EaseOut
            };

            Storyboard.SetTarget(opacityAnimation, element);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("Opacity"));

            storyboard.Children.Add(yAnimation);
            storyboard.Children.Add(opacityAnimation);

            return storyboard;
        }

        public static Storyboard CreateScaleAnimation(FrameworkElement element, double fromScale = 0.9, double toScale = 1.0, Duration? duration = null)
        {
            Duration animDuration = duration ?? MediumDuration;

            var storyboard = new Storyboard();

            if (element.RenderTransform == null || !(element.RenderTransform is ScaleTransform))
            {
                element.RenderTransformOrigin = new Point(0.5, 0.5);
                element.RenderTransform = new ScaleTransform(1, 1);
            }

            var xScaleAnimation = new DoubleAnimation
            {
                From = fromScale,
                To = toScale,
                Duration = animDuration,
                EasingFunction = EaseOut
            };

            Storyboard.SetTarget(xScaleAnimation, element);
            Storyboard.SetTargetProperty(xScaleAnimation, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleX)"));

            var yScaleAnimation = new DoubleAnimation
            {
                From = fromScale,
                To = toScale,
                Duration = animDuration,
                EasingFunction = EaseOut
            };

            Storyboard.SetTarget(yScaleAnimation, element);
            Storyboard.SetTargetProperty(yScaleAnimation, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));

            storyboard.Children.Add(xScaleAnimation);
            storyboard.Children.Add(yScaleAnimation);

            return storyboard;
        }

        public static Storyboard CreatePulseAnimation(FrameworkElement element, double pulseScale = 1.05, Duration? duration = null)
        {
            Duration animDuration = duration ?? ShortDuration;

            var storyboard = new Storyboard();

            if (element.RenderTransform == null || !(element.RenderTransform is ScaleTransform))
            {
                element.RenderTransformOrigin = new Point(0.5, 0.5);
                element.RenderTransform = new ScaleTransform(1, 1);
            }

            var xScaleAnimation = new DoubleAnimationUsingKeyFrames();
            xScaleAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.Zero)));
            xScaleAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(pulseScale, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(animDuration.TimeSpan.TotalMilliseconds / 2)), EaseOut));
            xScaleAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(1, KeyTime.FromTimeSpan(animDuration.TimeSpan), EaseIn));

            Storyboard.SetTarget(xScaleAnimation, element);
            Storyboard.SetTargetProperty(xScaleAnimation, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleX)"));

            var yScaleAnimation = new DoubleAnimationUsingKeyFrames();
            yScaleAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.Zero)));
            yScaleAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(pulseScale, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(animDuration.TimeSpan.TotalMilliseconds / 2)), EaseOut));
            yScaleAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(1, KeyTime.FromTimeSpan(animDuration.TimeSpan), EaseIn));

            Storyboard.SetTarget(yScaleAnimation, element);
            Storyboard.SetTargetProperty(yScaleAnimation, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));

            storyboard.Children.Add(xScaleAnimation);
            storyboard.Children.Add(yScaleAnimation);

            return storyboard;
        }

        public static Storyboard CreateRotateAnimation(FrameworkElement element, Duration? duration = null)
        {
            Duration animDuration = duration ?? new Duration(TimeSpan.FromSeconds(1));

            var storyboard = new Storyboard();

            if (element.RenderTransform == null || !(element.RenderTransform is RotateTransform))
            {
                element.RenderTransformOrigin = new Point(0.5, 0.5);
                element.RenderTransform = new RotateTransform();
            }

            var rotateAnimation = new DoubleAnimation
            {
                From = 0,
                To = 360,
                Duration = animDuration,
                RepeatBehavior = RepeatBehavior.Forever
            };

            Storyboard.SetTarget(rotateAnimation, element);
            Storyboard.SetTargetProperty(rotateAnimation, new PropertyPath("(UIElement.RenderTransform).(RotateTransform.Angle)"));

            storyboard.Children.Add(rotateAnimation);

            return storyboard;
        }
    }
}
