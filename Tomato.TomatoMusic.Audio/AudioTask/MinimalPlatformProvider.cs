using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caliburn.Micro
{
    class MinimalPlatformProvider : IPlatformProvider
    {
        public bool InDesignMode => false;

        public void BeginOnUIThread(System.Action action)
        {
            Task.Run(action);
        }

        public void ExecuteOnFirstLoad(object view, Action<object> handler)
        {

        }

        public void ExecuteOnLayoutUpdated(object view, Action<object> handler)
        {

        }

        public object GetFirstNonGeneratedView(object view)
        {
            return null;
        }

        public System.Action GetViewCloseAction(object viewModel, ICollection<object> views, bool? dialogResult)
        {
            return null;
        }

        public void OnUIThread(System.Action action)
        {
            action();
        }

        public Task OnUIThreadAsync(System.Action action)
        {
            return Task.Run(action);
        }
    }
}
