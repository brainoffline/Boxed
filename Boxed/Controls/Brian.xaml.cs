﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236
using Windows.UI.Xaml.Shapes;
using Brain.Animate;
using Brain.Utils;

namespace Boxed.Controls
{
    public enum BrianAction
    {
        Entrance,
        EntranceQuick,

        RandomWait,

        Swing,
        Color,
        Jump,

        FlippingLeft,
        FlippingRight,
        RotateRight,
        RotateLeft,
        Pulse,

        Abashed,
        Thinking,

        Exit,
        ExitQuick,
    }

    public sealed partial class Brian 
    {
        public Brian()
        {
            InitializeComponent();
        }

        public void Hide()
        {
            OuterGrid.Opacity = 0;
            InnerGrid.Opacity = 0;
        }

        public async Task Do(BrianAction BrianAction)
        {
            if (AnimationManager.IsAnimating(OuterGrid) ||
                AnimationManager.IsAnimating(InnerGrid))
            {
                AnimationManager.PauseAnimations(InnerGrid);
                AnimationManager.PauseAnimations(OuterGrid);
                var tasks = new Task[]
                {
                    OuterGrid.AnimateAsync(new SlowResetAnimation() ),
                    InnerGrid.AnimateAsync(new SlowResetAnimation() ),
                };
                await Task.WhenAll(tasks);
                AnimationManager.StopAnimations(InnerGrid);
                AnimationManager.StopAnimations(OuterGrid);
                AnimationManager.StopAnimations(Inner);
                AnimationManager.StopAnimations(Outer);
            }

            switch (BrianAction)
            {
                case BrianAction.Entrance:
                    await Entrance1();
                    break;
                case BrianAction.EntranceQuick:
                    await Entrance2();
                    break;
                case BrianAction.Exit:
                    await Exit1();
                    break;
                case BrianAction.ExitQuick:
                    await Exit2();
                    break;
                case BrianAction.Abashed:
                    await Abashed1();
                    break;
                case BrianAction.Thinking:
                    await Thinking1();
                    break;
                case BrianAction.RandomWait:
                    await RandomWait(3);
                    break;
                case BrianAction.Swing:
                    await Swing1();
                    break;
                case BrianAction.FlippingLeft:
                    await Flipping(true);
                    break;
                case BrianAction.FlippingRight:
                    await Flipping(false);
                    break;
                case BrianAction.RotateRight:
                    await Rotation(true);
                    break;
                case BrianAction.RotateLeft:
                    await Rotation(false);
                    break;
                case BrianAction.Pulse:
                    await Pulse();
                    break;
                case BrianAction.Color:
                    await Color1();
                    break;
                case BrianAction.Jump:
                    await Jump();
                    break;
            }
        }

        private async Task Entrance1()
        {
            var tasks = new Task[]
            {
                OuterGrid.AnimateAsync(new BounceInAnimation { FromDirection = ZDirection.Closer, Duration = 0.5}),
                InnerGrid.AnimateAsync(new BounceInAnimation { FromDirection = ZDirection.Away, Duration = 0.5}),
            };
            await Task.WhenAll(tasks);
        }

        private async Task Entrance2()
        {
            var tasks = new Task[]
            {
                OuterGrid.AnimateAsync(new BounceInAnimation { FromDirection = ZDirection.Closer, Duration = 0.2}),
                InnerGrid.AnimateAsync(new BounceInAnimation { FromDirection = ZDirection.Away, Duration = 0.2}),
            };
            await Task.WhenAll(tasks);
        }

        private async Task RandomWait(int repeatCount)
        {
            int i = RandomManager.Next(5);
            switch (i)
            {
                case 0:
                    await Rotation(true, repeatCount);
                    break;
                case 1:
                    await Rotation(false, repeatCount);
                    break;
                case 2:
                    await Pulse(repeatCount);
                    break;
                case 3:
                    await Flipping(true, repeatCount);
                    break;
                case 4:
                    await Flipping(false, repeatCount);
                    break;
            }
        }

        private async Task Exit1()
        {
            var tasks = new Task[]
            {
                OuterGrid.AnimateAsync(new BounceOutAnimation { ToDirection = ZDirection.Closer, Duration = 0.5}),
                InnerGrid.AnimateAsync(new BounceOutAnimation { ToDirection = ZDirection.Away, Duration = 0.5}),
            };
            await Task.WhenAll(tasks);
        }

        private async Task Exit2()
        {
            var tasks = new Task[]
            {
                OuterGrid.AnimateAsync(new BounceOutAnimation { ToDirection = ZDirection.Closer, Duration = 0.2}),
                InnerGrid.AnimateAsync(new BounceOutAnimation { ToDirection = ZDirection.Away, Duration = 0.2}),
            };
            await Task.WhenAll(tasks);
        }

        private async Task Abashed1()
        {
            var tasks = new Task[]
            {
                OuterGrid.AnimateAsync(new FlipXAnimation { From = 0.0, To = 240, Duration = 1}),
                InnerGrid.AnimateAsync(new FlipXAnimation { From = 0.0, To = 120, Duration = 1}),
            };
            await Task.WhenAll(tasks);
            tasks = new Task[]
            {
                OuterGrid.AnimateAsync(new FlipXAnimation { From = 240, To = 0, Duration = 0.3}),
                InnerGrid.AnimateAsync(new FlipXAnimation { From = 120, To = 0, Duration = 0.3}),
            };
            await Task.WhenAll(tasks);
        }

        private async Task Thinking1()
        {
            var tasks = new Task[]
            {
                OuterGrid.AnimateAsync(new FlipYAnimation { From = 0.0, To = 180, Duration = 0.4, Delay = 0.1}),
                InnerGrid.AnimateAsync(new FlipYAnimation { From = 0.0, To = 180, Duration = 0.5}),
            };
            await Task.WhenAll(tasks);
        }

        private async Task Swing1()
        {
            var tasks = new Task[]
            {
                OuterGrid.AnimateAsync(new FlipEnjoyAnimation() ),
                InnerGrid.AnimateAsync(new FlipEnjoyAnimation { Reverse = true} ),
            };
            await Task.WhenAll(tasks);
        }

        private async Task Rotation(bool forward, int repeatCount = -1)
        {
            bool forever = repeatCount < 0;
            var tasks = new Task[]
            {
                OuterGrid.AnimateAsync(new SlowRotateAnimation { Reverse = forward, Forever = forever, RepeatCount = repeatCount} ),
                InnerGrid.AnimateAsync(new SlowRotateAnimation { Reverse = !forward, Forever = forever, RepeatCount = repeatCount } ),
            };
            await Task.WhenAll(tasks);
        }

        private async Task Flipping(bool forward, int repeatCount = -1)
        {
            bool forever = repeatCount < 0;
            double from = forward ? 0.0 : 180.0;
            double to = forward ? 180.0 : 0;

            var tasks = new Task[]
            {
                OuterGrid.AnimateAsync(new FlipYAnimation { From = from, To = to, Duration = 0.8, PauseBefore = 0.2, Forever = forever, RepeatCount = repeatCount}),
                InnerGrid.AnimateAsync(new FlipYAnimation { From = from, To = to, Duration = 1.0, Forever = forever, RepeatCount = repeatCount}),
            };
            await Task.WhenAll(tasks);
        }


        private async Task Pulse(int repeatCount = -1)
        {
            bool forever = repeatCount < 0;
            var tasks = new Task[]
            {
                Outer.AnimateAsync(new LinePulseAnimation { Duration = 5, Forever = forever, RepeatCount = repeatCount}),
                InnerGrid.AnimateAsync(new SizePulseAnimation { Duration = 5, Reverse = true, Forever = forever, RepeatCount = repeatCount}),
            };
            await Task.WhenAll(tasks);
        }

        private async Task Color1()
        {
            var tasks = new Task[]
            {
                brian.AnimateAsync(new RainbowAnimation() ),
            };
            await Task.WhenAll(tasks);
        }

        private async Task Jump()
        {
            var tasks = new Task[]
            {
                OuterGrid.AnimateAsync(new JumpAnimation { Distance = -40 } ),
                InnerGrid.AnimateAsync(new JumpAnimation { Distance = -20 } ),
            };
            await Task.WhenAll(tasks);
        }
    }


    public class RainbowAnimation : AnimationDefinition
    {
        public RainbowAnimation()
        {
            Duration = 5;
        }

        public override IEnumerable<Timeline> CreateAnimation(FrameworkElement element)
        {
            var color = Colors.Aquamarine;

            var brush = (SolidColorBrush)element.GetValue(Control.ForegroundProperty);
            if (brush != null)
                color = brush.Color;

            return new Timeline[]
            {
                element.AnimateColorProperty("(Control.Foreground).(SolidColorBrush.Color)")
                    .AddEasingKeyFrame(Duration * 0.25, Colors.Red)
                    .AddEasingKeyFrame(Duration * 0.5, Colors.Yellow)
                    .AddEasingKeyFrame(Duration * 0.75, Colors.Blue)
                    .AddEasingKeyFrame(Duration, color)
            };
        }
    }


    public class FlipEnjoyAnimation : AnimationDefinition
    {
        public bool Reverse;

        public FlipEnjoyAnimation()
        {
            Duration = 2;
        }

        public override IEnumerable<Timeline> CreateAnimation(FrameworkElement element)
        {
            return new Timeline[]
            {
                element.AnimateProperty(Reverse ? AnimationProperty.RotationX : AnimationProperty.RotationY)
                    .AddEasingKeyFrame(0.0, 180)
                    .AddEasingKeyFrame(Duration * 0.75, 0, new QuadraticEase { EasingMode = EasingMode.EaseInOut}),
                element.AnimateProperty(Reverse ? AnimationProperty.RotationY : AnimationProperty.RotationX)
                    .AddEasingKeyFrame(Duration * 0.25, 180)
                    .AddEasingKeyFrame(Duration, 0, new QuadraticEase { EasingMode = EasingMode.EaseInOut}),
            };
        }
    }

    public class SlowRotateAnimation : AnimationDefinition
    {
        public bool Reverse { get; set; }

        public SlowRotateAnimation()
        {
            Duration = 10;
            Forever = true;
        }

        public override IEnumerable<Timeline> CreateAnimation(FrameworkElement element)
        {
            return new Timeline[]
            {
                element.AnimateProperty(AnimationProperty.Rotation)
                    .AddEasingKeyFrame(0, Reverse ? 360 : -360)
                    .AddEasingKeyFrame(Duration, 0),
            };

        }
    }

    public class LinePulseAnimation : AnimationDefinition
    {
        public bool Reverse { get; set; }
        public double MinSize { get; set; }
        public double MaxSize { get; set; }

        public LinePulseAnimation()
        {
            Duration = 7;
            Forever = true;
            MinSize = 10;
            MaxSize = 20;
        }

        public override IEnumerable<Timeline> CreateAnimation(FrameworkElement element)
        {
            var shape = element as Shape;
            if (shape == null)
                return new Timeline[] { };

            var timeline = element.AnimateProperty("(Shape.StrokeThickness)");
            timeline.EnableDependentAnimation = true;
            var defaultThickness = shape.StrokeThickness;

            return new Timeline[]
            {
                timeline
                    .AddEasingKeyFrame(0.0, defaultThickness)
                    .AddEasingKeyFrame(Duration*0.25, Reverse ? MaxSize : MinSize, new QuadraticEase {EasingMode = EasingMode.EaseInOut})
                    .AddEasingKeyFrame(Duration*0.75, Reverse ? MinSize : MaxSize, new QuadraticEase {EasingMode = EasingMode.EaseInOut})
                    .AddEasingKeyFrame(Duration, defaultThickness),
            };
        }
    }

    public class SizePulseAnimation : AnimationDefinition
    {
        public bool Reverse { get; set; }
        public double MinScale { get; set; }
        public double MaxScale { get; set; }

        public SizePulseAnimation()
        {
            Forever = true;
            MinScale = 0.9;
            MaxScale = 1.1;
        }

        public override IEnumerable<Timeline> CreateAnimation(FrameworkElement element)
        {
            return new Timeline[]
            {
                element.AnimateProperty(AnimationProperty.ScaleX)
                    .AddEasingKeyFrame(0.0, 1)
                    .AddEasingKeyFrame(Duration*0.25, Reverse ? MaxScale : MinScale, new QuadraticEase {EasingMode = EasingMode.EaseInOut})
                    .AddEasingKeyFrame(Duration*0.75, Reverse ? MinScale : MaxScale, new QuadraticEase {EasingMode = EasingMode.EaseInOut})
                    .AddEasingKeyFrame(Duration, 1),
                element.AnimateProperty(AnimationProperty.ScaleY)
                    .AddEasingKeyFrame(0.0, 1)
                    .AddEasingKeyFrame(Duration*0.25, Reverse ? MaxScale : MinScale, new QuadraticEase {EasingMode = EasingMode.EaseInOut})
                    .AddEasingKeyFrame(Duration*0.75, Reverse ? MinScale : MaxScale, new QuadraticEase {EasingMode = EasingMode.EaseInOut})
                    .AddEasingKeyFrame(Duration, 1)
            };
        }
    }


    public class SlowResetAnimation : AnimationDefinition
    {
        public bool Reverse { get; set; }

        public SlowResetAnimation()
        {
            Duration = 0.3;
        }

        public override IEnumerable<Timeline> CreateAnimation(FrameworkElement element)
        {
            return new Timeline[]
            {
                element.AnimateProperty(AnimationProperty.Rotation).AddEasingKeyFrame(Duration, 0, new QuadraticEase()),
                element.AnimateProperty(AnimationProperty.RotationX).AddEasingKeyFrame(Duration, 0, new QuadraticEase()),
                element.AnimateProperty(AnimationProperty.RotationY).AddEasingKeyFrame(Duration, 0, new QuadraticEase()),
            };

        }
    }

    public class FlipXAnimation : AnimationDefinition
    {
        public FlipXAnimation()
        {
            Centre = 0.5;
            From = 0.0;
            To = 180.0;
        }

        public Double Centre { get; set; }
        public Double From { get; set; }
        public Double To { get; set; }


        public override IEnumerable<Timeline> CreateAnimation(FrameworkElement element)
        {
            var list = new List<Timeline>
            {
                element.AnimateProperty(AnimationProperty.CentreOfRotationY)
                    .AddDiscreteKeyFrame(0.0, Centre),
                element.AnimateProperty(AnimationProperty.RotationX)
                    .AddEasingKeyFrame(0.0, From)
                    .AddEasingKeyFrame(Duration, To, new QuadraticEase() )
            };
            return list;
        }
    }

    public class FlipYAnimation : AnimationDefinition
    {
        public FlipYAnimation()
        {
            Centre = 0.5;
            From = 0.0;
            To = 180.0;
        }

        public Double Centre { get; set; }
        public Double From { get; set; }
        public Double To { get; set; }


        public override IEnumerable<Timeline> CreateAnimation(FrameworkElement element)
        {
            var list = new List<Timeline>
            {
                element.AnimateProperty(AnimationProperty.CentreOfRotationX)
                    .AddDiscreteKeyFrame(0.0, Centre),
                element.AnimateProperty(AnimationProperty.RotationY)
                    .AddEasingKeyFrame(0.0, From)
                    .AddEasingKeyFrame(Duration, To, new QuadraticEase() )
            };
            return list;
        }

    }
}
