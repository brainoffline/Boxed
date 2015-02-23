﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;
using Brain.Animate.NavigationAnimations;
#if WINDOWS_81_PORTABLE || NETFX_CORE

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

#if WINDOWS_PHONE_APP
using Windows.Phone.UI.Input;
#endif


namespace Brain.Animate
{
    [TemplatePart(Name = FirstTemplatePartName, Type = typeof(ContentPresenter))]
    [TemplatePart(Name = SecondTemplatePartName, Type = typeof(ContentPresenter))]
    [TemplatePart(Name = MediaPartName, Type= typeof(MediaElement))]
    public class AnimationFrame : Frame
    {
        private const string FirstTemplatePartName = "FirstContentPresenter";
        private const string SecondTemplatePartName = "SecondContentPresenter";
        private const string MediaPartName = "media";
        internal static readonly CacheMode BitmapCacheMode = new BitmapCache();

        private ContentPresenter _firstContentPresenter;
        private ContentPresenter _secondContentPresenter;
        private ContentPresenter _newContentPresenter;
        private ContentPresenter _oldContentPresenter;

        private bool _isForwardNavigation;
        private bool _useFirstAsNew;

        private bool _animating;

        public NavigationAnimationDefinition NavigationAnimation { get; set; }

        public MediaElement Media { get; private set; }

        private bool _oneOff;
        private AnimationDefinition OneOffAnimation_Close { get; set; }
        private AnimationDefinition OneOffkAnimation_Open { get; set; }
        private bool OneOffAnimation_Sequential { get; set; }

        public AnimationFrame()
        {
            DefaultStyleKey = typeof(AnimationFrame);

            Navigating += OnNavigating;

#if WINDOWS_PHONE_APP
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
#endif
        }

#if WINDOWS_PHONE_APP

        private bool animatingBack;

        private async void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            if (_animating)
                e.Handled = true;

            if (e.Handled) 
                return;

            if (CanGoBack)
            {
                e.Handled = true;
                animatingBack = true;

                await AnimationTrigger.AnimateClose();

                animatingBack = false;
                GoBack();
            }
        }
#endif

        private void OnBackKeyPress(object sender, CancelEventArgs cancelEventArgs)
        {
            if (_animating)
                cancelEventArgs.Cancel = true;
        }


        private void SwapPresenters()
        {
            _newContentPresenter = _useFirstAsNew ? _firstContentPresenter : _secondContentPresenter;
            _oldContentPresenter = _useFirstAsNew ? _secondContentPresenter : _firstContentPresenter;
            _useFirstAsNew = !_useFirstAsNew;
        }


        private void OnNavigating(object sender, NavigatingCancelEventArgs e)
        {
            _isForwardNavigation = e.NavigationMode != NavigationMode.Back;

            var oldElement = Content as UIElement;
            if (oldElement == null)
                return;

            // TODO: Make sure previous animation has complete

            SwapPresenters();
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _firstContentPresenter = GetTemplateChild(FirstTemplatePartName) as ContentPresenter;
            _secondContentPresenter = GetTemplateChild(SecondTemplatePartName) as ContentPresenter;
            Media = GetTemplateChild(MediaPartName) as MediaElement;
            _newContentPresenter = _secondContentPresenter;
            _oldContentPresenter = _firstContentPresenter;
            _useFirstAsNew = true;

            if (Content != null)
                OnContentChanged(null, Content);
        }


        protected override async void OnContentChanged(object oldContent, object newContent)
        {
            if (NavigationAnimation == null)
                NavigationAnimation = new JumpLeftNavigationAnimation();

            base.OnContentChanged(oldContent, newContent);

            var oldElement = oldContent as UIElement;
            var newElement = newContent as UIElement;

            // Check if the template has been applied correctly
            if (_firstContentPresenter == null || _secondContentPresenter == null || newElement == null)
                return;

            _animating = true;

            //var statusBar = StatusBar.GetForCurrentView();
            //statusBar.HideAsync();

            try
            {
                AnimationDefinition closeAnimation;
                AnimationDefinition openAnimation;
                bool sequential;

                if (_oneOff)
                {
                    _oneOff = false;
                    closeAnimation = OneOffAnimation_Close;
                    openAnimation = OneOffkAnimation_Open;
                    sequential = OneOffAnimation_Sequential;
                }
                else if (_isForwardNavigation)
                {
                    closeAnimation = NavigationAnimation.ForwardAnimationCloseOld;
                    openAnimation = NavigationAnimation.ForwardAnimationOpenNew;
                    sequential = NavigationAnimation.ForwardAnimationSequential;
                }
                else
                {
                    closeAnimation = NavigationAnimation.BackAnimationCloseTop;
                    openAnimation = NavigationAnimation.BackAnimationReOpenBottom;
                    sequential = NavigationAnimation.BackAnimationSequential;
                }

                AnimationManager.ClearAnimationProperties(_oldContentPresenter);
                _oldContentPresenter.Opacity = 1;
                _oldContentPresenter.CacheMode = BitmapCacheMode;
                _oldContentPresenter.Visibility = Visibility.Visible;
                _oldContentPresenter.Content = oldElement;
                _oldContentPresenter.IsHitTestVisible = false;

                AnimationManager.ClearAnimationProperties(_newContentPresenter);
                _newContentPresenter.Opacity = 0;
                _newContentPresenter.CacheMode = BitmapCacheMode;
                _newContentPresenter.Visibility = Visibility.Visible;
                _newContentPresenter.Content = newElement;
                _newContentPresenter.IsHitTestVisible = false;

                if (openAnimation == null)
                    _newContentPresenter.Opacity = 1;

                if ((_isForwardNavigation && !NavigationAnimation.ForwardAnimationReveal) ||
                    (!_isForwardNavigation && NavigationAnimation.BackAnimationReveal))
                {
                    Canvas.SetZIndex(_newContentPresenter, 1);
                    Canvas.SetZIndex(_oldContentPresenter, 0);
                }
                else
                {
                    Canvas.SetZIndex(_newContentPresenter, 0);
                    Canvas.SetZIndex(_oldContentPresenter, 1);
                }



                if (sequential)
                {
                    if (closeAnimation != null)
                        await _oldContentPresenter.AnimateAsync(closeAnimation);
                    if (openAnimation != null)
                        await _newContentPresenter.AnimateAsync(openAnimation);
                }
                else
                {
                    var list = new List<Task<FrameworkElement>>();

                    if (closeAnimation != null) 
                        list.Add(_oldContentPresenter.AnimateAsync(closeAnimation));
                    if (openAnimation != null) 
                        list.Add(_newContentPresenter.AnimateAsync(openAnimation));
                    await Task.WhenAll( list );
                }
            }
            finally
            {
                _oldContentPresenter.Visibility = Visibility.Collapsed;
                _oldContentPresenter.Content = null;

                _newContentPresenter.CacheMode = null;
                _newContentPresenter.Opacity = 1;
                _newContentPresenter.IsHitTestVisible = true;

                _animating = false;
            }
        }

        internal void SetNextNavigationAnimation(
            AnimationDefinition closeAnimation, 
            AnimationDefinition openAnimation, 
            bool sequential)
        {
            OneOffAnimation_Close = closeAnimation;
            OneOffkAnimation_Open = openAnimation;
            OneOffAnimation_Sequential = sequential;
            _oneOff = true;
        }
    }
}

#endif
