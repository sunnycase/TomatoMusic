using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tomato.Uwp.Mvvm.Threading
{
    public class OperationQueue
    {
        private readonly TaskScheduler _scheduler;
        private readonly TaskFactory _taskFactory;

        public OperationQueue(int maxDegreeOfParallelism)
        {
            _scheduler = new LimitedConcurrencyLevelTaskScheduler(maxDegreeOfParallelism);
            _taskFactory = new TaskFactory(_scheduler);
        }

        public Task Queue(Action action)
        {
            return _taskFactory.StartNew(action);
        }

        public Task Queue(Func<Task> action)
        {
            return _taskFactory.StartNew(action).Unwrap();
        }
    }
}
