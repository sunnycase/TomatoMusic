using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace Tomato.Uwp.Mvvm.Controls
{
    [TemplatePart(Name = "PART_HamburgerButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_SplitView", Type = typeof(SplitView))]
    [ContentProperty(Name = nameof(Content))]
    public sealed class HamburgerMenu : Control
    {
        public static DependencyProperty PaneProperty { get; } = DependencyProperty.Register(nameof(Pane), typeof(UIElement),
            typeof(HamburgerMenu), new PropertyMetadata(DependencyProperty.UnsetValue));
        public static DependencyProperty ContentProperty { get; } = DependencyProperty.Register(nameof(Content), typeof(UIElement),
            typeof(HamburgerMenu), new PropertyMetadata(DependencyProperty.UnsetValue));
        public static DependencyProperty IsPaneOpenProperty { get; } = DependencyProperty.Register(nameof(IsPaneOpen), typeof(bool),
            typeof(HamburgerMenu), new PropertyMetadata(true));

        public UIElement Pane
        {
            get { return (UIElement)GetValue(PaneProperty); }
            set { SetValue(PaneProperty, value); }
        }

        public UIElement Content
        {
            get { return (UIElement)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        public bool IsPaneOpen
        {
            get { return (bool)GetValue(IsPaneOpenProperty); }
            set { SetValue(IsPaneOpenProperty, value); }
        }

        public HamburgerMenu()
        {
            this.DefaultStyleKey = typeof(HamburgerMenu);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var hamburgerButton = (Button)GetTemplateChild("PART_HamburgerButton");
            hamburgerButton.Click += HamburgerButton_Click;
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            IsPaneOpen = !IsPaneOpen;
        }
    }
}
