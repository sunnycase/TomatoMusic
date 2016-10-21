using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Tomato.TomatoMusic.Shell.Controls
{
    public class ActionExtension
    {
        public static DependencyProperty TargetWithoutContextPathProperty { get; } = DependencyProperty.Register("TargetWithoutContextPath",
            typeof(string), typeof(ActionExtension), new PropertyMetadata(null, OnTargetWithoutContextPathPropertyChanged));

        public static string GetTargetWithoutContextPath(DependencyObject d) => (string)d.GetValue(TargetWithoutContextPathProperty);

        public static void SetTargetWithoutContextPath(DependencyObject d, string value) => d.SetValue(TargetWithoutContextPathProperty, value);

        private static void OnTargetWithoutContextPathPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BindingOperations.SetBinding(d, Caliburn.Micro.Action.TargetWithoutContextProperty, new Binding
            {
                Path = new PropertyPath((string)e.NewValue),
                RelativeSource = new RelativeSource { Mode = RelativeSourceMode.Self }
            });
        }
    }
}
