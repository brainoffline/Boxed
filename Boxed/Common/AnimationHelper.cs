using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Brain.Animate;
using Brain.Utils;

namespace Boxed.Common
{
    public class AnimationHelper
    {
        public static void AnimationForegroundColor(
            FrameworkElement element,
            IEnumerable<Color> colors,
            double span = 5, double startSpan = 0,
            bool random = false,
            bool autoReverse = false)
        {
            element.StopAnimations();

            var control = element as Control;
            if (control != null)
            {
                if (control.Foreground == null)
                    control.Foreground = new SolidColorBrush(Colors.Transparent);
            }

            var animation = element.AnimateColorProperty("(Control.Foreground).(SolidColorBrush.Color)");
            var seconds = startSpan;
            foreach (var color in colors)
            {
                animation.AddEasingKeyFrame(seconds, color, new BackEase { EasingMode = EasingMode.EaseIn, Amplitude = 0.4 });
                seconds += span;
            }

            var sb = new Storyboard();
            sb.Children.Add(animation);
            sb.RepeatBehavior = RepeatBehavior.Forever;
            sb.AutoReverse = autoReverse;
            sb.Begin();

            if (random)
            {
                var ts = TimeSpan.FromSeconds(RandomManager.NextDouble() * (seconds - span));
                sb.Seek(ts);
            }
        }



        public static void AnimationBackgroundColor(
            FrameworkElement element,
            IEnumerable<Color> colors,
            double span = 5, double startSpan = 0,
            bool random = false, bool autoReverse = false)
        {
            element.StopAnimations();

            var control = element as Control;
            if (control != null)
            {
                if (control.Background == null)
                    control.Background = new SolidColorBrush(Colors.Transparent);
            }
            else
            {
                var panel = element as Panel;
                if (panel != null)
                {
                    if (panel.Background == null)
                        panel.Background = new SolidColorBrush(Colors.Transparent);
                }
            }

            var animation = element.AnimateColorProperty("(Control.Background).(SolidColorBrush.Color)");
            var seconds = startSpan;
            foreach (var color in colors)
            {
                animation.AddEasingKeyFrame(seconds, color, new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.4 });
                seconds += span;
            }

            var sb = new Storyboard();
            sb.Children.Add(animation);
            sb.RepeatBehavior = RepeatBehavior.Forever;
            sb.AutoReverse = autoReverse;
            sb.Begin();

            if (random)
            {
                var ts = TimeSpan.FromSeconds(RandomManager.NextDouble() * (seconds - span));
                sb.Seek(ts);
            }
        }


        public static void AnimationForegroundColor(
            FrameworkElement element,
            IEnumerable<string> resourceNames,
            double span = 5, double startSpan = 0,
            bool random = false, bool autoReverse = false)
        {
            var colors = resourceNames.Select(
                resourceName => (Color)Application.Current.Resources[resourceName])
                .ToList();

            AnimationForegroundColor(element, colors, span, startSpan, random);
        }


        public static void AnimationBackgroundColor(
            FrameworkElement element,
            IEnumerable<string> resourceNames,
            double span = 5, double startSpan = 0,
            bool random = false, bool autoReverse = false)
        {
            var colors = resourceNames.Select(
                resourceName => (Color)Application.Current.Resources[resourceName])
                .ToList();

            AnimationBackgroundColor(element, colors, span, startSpan, random);
        }

        public static void AnimateBackgroundDark(FrameworkElement element)
        {
            AnimationBackgroundColor(element,
                new[] { "BlueGrey900", "Grey800", "BlueGrey800", "Grey700", "BlueGrey700", "Grey800", "BlueGrey800", "Grey900" },
                5, random: true);
        }

        public static void AnimateBackgroundRed(FrameworkElement element)
        {
            AnimationBackgroundColor(element,
                new[] { "Red50", "Red100", "Red200", "Red300", "Red400", "Red500", "Red600", "Red700", "Red800", "Red900", },
                2, 1, true);
        }

        public static void AnimateBackgroundRainbow(FrameworkElement element)
        {
            AnimationBackgroundColor(element,
                new[] { "Red500", "Purple500", "DeepPurple500", "Indigo500", 
                    "Blue500", "LightBlue500", "Cyan500", "Teal500", "Green500", 
                    "LightGreen500", 
                    // "Lime500", "Yellow500", Too light
                    "Amber500", "Orange500", 
                    "DeepOrange500", "Red500"
                },
                3, random: true);
        }

        public static void AnimateForegroundBlackAndWhite(FrameworkElement element)
        {
            AnimationForegroundColor(element, new[] { Colors.Black, Colors.White }, span: 2, autoReverse: true);
        }

        public static void AnimateForegroundLightRainbow(FrameworkElement element)
        {
            AnimationForegroundColor(element,
                new[] { "Red200", "Purple200", "DeepPurple200", "Indigo200", 
                    "Blue200", "LightBlue200", "Cyan200", "Teal200", "Green200", 
                    "LightGreen200", 
                    // "Lime200", "Yellow200", Too light
                    "Amber200", "Orange200", 
                    "DeepOrange200", "Red200"
                },
                3);
        }

    }
}
