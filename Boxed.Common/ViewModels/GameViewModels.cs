using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Boxed.DataModel;
using PropertyChanged;

namespace Boxed.ViewModels
{
    [ImplementPropertyChanged]
    public class SquareDataViewModel
    {
        private string _state;
        private string _touchState;

        public bool Fixed { get; set; }
        public string Text { get; set; }

        [AlsoNotifyFor("Color")]
        public string TouchState
        {
            get { return _touchState; }
            set
            {
                _touchState = value;

                if (value == "1")
                {
                    LeftVisible = false;
                    TopVisible = false;
                    RightVisible = false;
                    BottomVisible = false;
                    LeftTopVisible = false;
                    RightTopVisible = false;
                    LeftBottomVisible = false;
                    RightBottomVisible = false;
                }
                View.Update();
            }
        }

        public GameDefinition.GameShape Shape { get; set; }

        public string State
        {
            get { return _state; }
            set
            {
                _state = value;
                if ((_state != null) && (_state.Length != 4))
                    return;
                LeftVisible = State[0] == '1';
                TopVisible = State[1] == '1';
                RightVisible = State[2] == '1';
                BottomVisible = State[3] == '1';
                View.Update();
            }
        }

        public bool LeftVisible { get; set; }
        public bool LeftTopVisible { get; set; }
        public bool TopVisible { get; set; }
        public bool RightTopVisible { get; set; }
        public bool RightVisible { get; set; }
        public bool LeftBottomVisible { get; set; }
        public bool BottomVisible { get; set; }
        public bool RightBottomVisible { get; set; }

        public bool BorderVisible { get; set; }

        public Brush HighColor { get; set; }
        public Brush MedColor { get; set; }
        public static Brush TransparentBrush = new SolidColorBrush(Colors.Transparent);
        public static Brush WhiteBrush = new SolidColorBrush(Colors.White);
        public static Brush BlackBrush = new SolidColorBrush(Colors.Black);

        public Brush Color
        {
            get
            {
                switch (TouchState)
                {
                    case "2":
                        return HighColor;
                    case "1":
                        return MedColor;
                    default:
                        return TransparentBrush;
                }
            }
        }

        public IUpdateable View { get; set; }

        public override string ToString()
        {
            return string.Format("{0}-{1}:{2}",
                Text,
                TouchState,
                Shape == null ? "" : Shape.ToString());
        }
    }
}
