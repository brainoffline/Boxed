using Boxed.Common;

namespace Boxed
{
    public sealed partial class HowToPlayPage 
    {
        public HowToPlayPage()
        {
            InitializeComponent();

            AnimationHelper.AnimateBackgroundRainbow(root);
            AnimationHelper.AnimateBackgroundRainbow(imageGrid);
        }

    }
}
