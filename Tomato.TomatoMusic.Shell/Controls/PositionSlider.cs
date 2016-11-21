using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

namespace Tomato.TomatoMusic.Shell.Controls
{
    class PositionSlider : Slider
    {
        public static DependencyProperty PositionProperty { get; } = DependencyProperty.Register(nameof(Position), typeof(double),
            typeof(PositionSlider), new PropertyMetadata(0.0, OnPositionPropertyChanged));

        public double Position
        {
            get { return (double)GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        public event EventHandler ValueCommited;
        private bool _isValueSuspending = false;

        protected override void OnApplyTemplate()
        {
            var thumb = (Thumb)GetTemplateChild("HorizontalThumb");
            thumb.PointerReleased += Thumb_PointerReleased;
            thumb.AddHandler(PointerPressedEvent, (PointerEventHandler)Thumb_PointerPressed, true);
            thumb.PointerCanceled += Thumb_PointerCanceled;
            
            base.OnApplyTemplate();
        }

        private void Thumb_PointerCanceled(object sender, PointerRoutedEventArgs e)
        {
            _isValueSuspending = false;
        }

        private void Thumb_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _isValueSuspending = true;
        }

        private static void OnPositionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PositionSlider)d).UpdateValue((double)e.NewValue);
        }

        private void UpdateValue(double value)
        {
            if (!_isValueSuspending)
            {
                _isValueSuspending = true;
                Value = value;
                _isValueSuspending = false;
            }
        }

        protected override void OnValueChanged(double oldValue, double newValue)
        {
            base.OnValueChanged(oldValue, newValue);
            if(!_isValueSuspending)
                ValueCommited?.Invoke(this, EventArgs.Empty);
        }

        private void Thumb_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _isValueSuspending = false;
            ValueCommited?.Invoke(this, EventArgs.Empty);
        }
    }
}
