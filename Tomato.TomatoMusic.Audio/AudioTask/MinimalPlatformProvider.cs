using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.Threading;

namespace Caliburn.Micro
{
    class MinimalPlatformProvider : IPlatformProvider
    {
        public bool InDesignMode => false;
        private readonly TaskScheduler _taskScheduler;
        private readonly TaskFactory _taskFactory;

        public MinimalPlatformProvider()
        {
            _taskScheduler = new LimitedConcurrencyLevelTaskScheduler(1);
            _taskFactory = new TaskFactory(_taskScheduler);
        }

        public void BeginOnUIThread(System.Action action)
        {
            _taskFactory.StartNew(action);
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
            _taskFactory.StartNew(action).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public Task OnUIThreadAsync(System.Action action)
        {
            return _taskFactory.StartNew(action);
        }
    }
}
