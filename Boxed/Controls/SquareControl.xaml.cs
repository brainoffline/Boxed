using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Shapes;
using Boxed.ViewModels;
using Brain.Animate;


namespace Boxed
{
    public sealed partial class SquareControl : IUpdateable
    {
        private Rectangle _centre;
        private Rectangle _left;
        private Rectangle _leftTop;
        private Rectangle _top;
        private Rectangle _topRight;
        private Rectangle _right;
        private Rectangle _bottomRight;
        private Rectangle _bottom;
        private Rectangle _bottomLeft;

        public SquareControl()
        {
            InitializeComponent();
        }
        
        protected override Size MeasureOverride(Size availableSize)
        {
            var parentGrid = Parent as Grid;
            if (parentGrid != null)
            {
                var columnWidth = parentGrid.ColumnDefinitions[0].ActualWidth;
                var rowHeight = parentGrid.RowDefinitions[0].ActualHeight;
                var len = Math.Min(columnWidth, rowHeight);
                availableSize = new Size(len,len);
            }

            var baseSize = base.MeasureOverride(availableSize);
            double sideLength = Math.Max(baseSize.Width, baseSize.Height);
            return new Size(sideLength, sideLength);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var sideLength = Math.Min(finalSize.Width, finalSize.Height);
            var result = base.ArrangeOverride(new Size(sideLength, sideLength));
            return result;
        }

        public Task AnimateIn(int animation = 0)
        {
            switch (animation)
            {
                case 1: return textGrid.AnimateAsync(new BounceInDownAnimation());
                case 2: return textGrid.AnimateAsync(new BounceInUpAnimation());
                case 3: return textGrid.AnimateAsync(new BounceInLeftAnimation());
                case 4: return textGrid.AnimateAsync(new BounceInRightAnimation());
                default:
                    return textGrid.AnimateAsync(new FadeInAnimation());
            }
        }

        private Binding GetBinding()
        {
            return new Binding
            {
                Path = new PropertyPath("Background"), 
                ElementName = "squareControl"
            };
        }

        private SquareDataViewModel ViewModel
        {
            get { return DataContext as SquareDataViewModel; }
        }

        private Rectangle Centre
        {
            get
            {
                if (_centre == null)
                {
                    _centre = new Rectangle { Margin = new Thickness(3) };
                    _centre.SetBinding(Shape.FillProperty, GetBinding());
                }
                return _centre;
            }
        }
        private Rectangle Left
        {
            get
            {
                if (_left == null)
                {
                    _left = new Rectangle { Margin = new Thickness(-1,3,3,3) };
                    _left.SetBinding(Shape.FillProperty, GetBinding());
                }
                return _left;
            }
        }
        private Rectangle LeftTop
        {
            get
            {
                if (_leftTop == null)
                {
                    _leftTop = new Rectangle { Margin = new Thickness(-1,-1,3,3) };
                    _leftTop.SetBinding(Shape.FillProperty, GetBinding());
                }
                return _leftTop;
            }
        }
        private Rectangle Top
        {
            get
            {
                if (_top == null)
                {
                    _top = new Rectangle { Margin = new Thickness(3,-1,3,3) };
                    _top.SetBinding(Shape.FillProperty, GetBinding());
                }
                return _top;
            }
        }
        private Rectangle TopRight
        {
            get
            {
                if (_topRight == null)
                {
                    _topRight = new Rectangle { Margin = new Thickness(3,-1,-1,3) };
                    _topRight.SetBinding(Shape.FillProperty, GetBinding());
                }
                return _topRight;
            }
        }
        private Rectangle Right
        {
            get
            {
                if (_right == null)
                {
                    _right = new Rectangle { Margin = new Thickness(3,3,-1,3) };
                    _right.SetBinding(Shape.FillProperty, GetBinding());
                }
                return _right;
            }
        }
        private Rectangle BottomRight
        {
            get
            {
                if (_bottomRight == null)
                {
                    _bottomRight = new Rectangle { Margin = new Thickness(3,3,-1,-1) };
                    _bottomRight.SetBinding(Shape.FillProperty, GetBinding());
                }
                return _bottomRight;
            }
        }
        private Rectangle Bottom
        {
            get
            {
                if (_bottom == null)
                {
                    _bottom = new Rectangle { Margin = new Thickness(3,3,3,-1) };
                    _bottom.SetBinding(Shape.FillProperty, GetBinding());
                }
                return _bottom;
            }
        }
        private Rectangle BottomLeft
        {
            get
            {
                if (_bottomLeft == null)
                {
                    _bottomLeft = new Rectangle { Margin = new Thickness(-1,3,3,-1) };
                    _bottomLeft.SetBinding(Shape.FillProperty, GetBinding());
                }
                return _bottomLeft;
            }
        }


        public void Update()
        {
            if (ViewModel == null) return;

            rectangleGrid.Children.Clear();
            if (ViewModel.Color == SquareDataViewModel.TransparentBrush)
                return;

            rectangleGrid.Children.Add(Centre);
            if (ViewModel.LeftVisible) rectangleGrid.Children.Add(Left);
            if (ViewModel.LeftTopVisible) rectangleGrid.Children.Add(LeftTop);
            if (ViewModel.TopVisible) rectangleGrid.Children.Add(Top);
            if (ViewModel.RightTopVisible) rectangleGrid.Children.Add(TopRight);
            if (ViewModel.RightVisible) rectangleGrid.Children.Add(Right);
            if (ViewModel.RightBottomVisible) rectangleGrid.Children.Add(BottomRight);
            if (ViewModel.BottomVisible) rectangleGrid.Children.Add(Bottom);
            if (ViewModel.LeftBottomVisible) rectangleGrid.Children.Add(BottomLeft);

            /*
<Rectangle x:Name="centre" Fill="{Binding Color}" Margin="3" />

<Rectangle x:Name="left" Fill="{Binding Color}" Visibility="{Binding LeftVisible, Converter={StaticResource Visibility}}" Margin="-1,3,3,3" />
<Rectangle x:Name="leftTop" Fill="{Binding Color}" Visibility="{Binding LeftTopVisible, Converter={StaticResource Visibility}}" Margin="-1,-1,3,3" />
<Rectangle x:Name="top" Fill="{Binding Color}" Visibility="{Binding TopVisible, Converter={StaticResource Visibility}}" Margin="3,-1,3,3" />
<Rectangle x:Name="rightTop" Fill="{Binding Color}" Visibility="{Binding RightTopVisible, Converter={StaticResource Visibility}}" Margin="3,-1,-1,3" />
<Rectangle x:Name="right" Fill="{Binding Color}" Visibility="{Binding RightVisible, Converter={StaticResource Visibility}}" Margin="3,3,-1,3" />
<Rectangle x:Name="rightBottom" Fill="{Binding Color}" Visibility="{Binding RightBottomVisible, Converter={StaticResource Visibility}}" Margin="3,3,-1,-1" />
<Rectangle x:Name="bottom" Fill="{Binding Color}" Visibility="{Binding BottomVisible, Converter={StaticResource Visibility}}" Margin="3,3,3,-1" />
<Rectangle x:Name="leftBottom" Fill="{Binding Color}" Visibility="{Binding LeftBottomVisible, Converter={StaticResource Visibility}}" Margin="-1,3,3,-1" />
            */

        }

    }
}
