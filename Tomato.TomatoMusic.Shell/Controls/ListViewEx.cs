using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Tomato.TomatoMusic.Shell.Controls
{
    public class PrepareContainerForItemEventArgs : EventArgs
    {
        public DependencyObject Element { get; }
        public object Item { get; }

        public PrepareContainerForItemEventArgs(DependencyObject element, object item)
        {
            Element = element;
            Item = item;
        }
    }

    class ListViewEx : ListView
    {
        public event EventHandler<PrepareContainerForItemEventArgs> PrepareContainerForItem;

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            PrepareContainerForItem?.Invoke(this, new PrepareContainerForItemEventArgs(element, item));
        }
    }
}
