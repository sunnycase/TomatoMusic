using Microsoft.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;

namespace Tomato.TomatoMusic.Shell.Behaviors
{
    [ContentProperty(Name = nameof(Brushes))]
    public class BindItemBackgroundWithIndexBehavior : Behavior<ListViewBase>
    {
        public Collection<Brush> Brushes { get; } = new Collection<Brush>();

        protected override void OnAttached()
        {
            AssociatedObject.ContainerContentChanging += listView_ContainerContentChanging;
            base.OnAttached();
        }

        private void listView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            args.ItemContainer.Background = Brushes[args.ItemIndex % Brushes.Count];
        }

        protected override void OnDetaching()
        {
            AssociatedObject.ContainerContentChanging -= listView_ContainerContentChanging;
            base.OnDetaching();
        }
    }
}
