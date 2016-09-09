using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.Media;
using Tomato.TomatoMusic.Core;
using Windows.ApplicationModel.Background;
using Windows.Media.Playback;

namespace Tomato.TomatoMusic.AudioTask
{
    public sealed class BackgroundAudioTask : IBackgroundTask
    {
        private AudioController _audioController;
        private BackgroundTaskDeferral _deferral;
        
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();
            taskInstance.Canceled += TaskInstance_Canceled;
            taskInstance.Task.Completed += Task_Completed;

            _audioController = new AudioController();
        }

        private void Task_Completed(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            _deferral.Complete();
        }

        private void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            try
            {
                _audioController?.OnCanceled();
            }
            finally
            {
                _deferral.Complete();
            }
        }
    }
}
