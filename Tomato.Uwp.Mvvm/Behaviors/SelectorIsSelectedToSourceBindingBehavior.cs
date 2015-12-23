using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Xaml.Interactivity;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;

namespace Tomato.Uwp.Mvvm.Behaviors
{
    public class SelectorIsSelectedToSourceBindingBehavior : DependencyObject, IBehavior
    {
        public DependencyObject AssociatedObject { get; private set; }

        public event Action<object, bool> Setter;

        public void Attach(DependencyObject associatedObject)
        {
            if (Setter == null)
                throw new InvalidOperationException($"{nameof(Setter)} property is invalid.");

            AssociatedObject = associatedObject;
            ((Selector)associatedObject).SelectionChanged += Selector_SelectionChanged;
        }

        private void Selector_SelectionChanged(object sender, Windows.UI.Xaml.Controls.SelectionChangedEventArgs e)
        {
            e.AddedItems.Apply(o => Setter?.Invoke(o, true));
            e.RemovedItems.Apply(o => Setter?.Invoke(o, false));
        }

        public void Detach()
        {
            ((Selector)AssociatedObject).SelectionChanged -= Selector_SelectionChanged;
            AssociatedObject = null;
        }
    }
}
