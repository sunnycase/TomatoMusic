using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Tomato.TomatoMusic.AudioTask;
using Windows.UI.Xaml.Controls;

namespace Tomato.TomatoMusic.Shell.ViewModels
{
    class MainViewModel : Screen
    {
        private readonly WinRTContainer _container;
        private readonly IEventAggregator _eventAggregator;
        private INavigationService _navigationService;
        private readonly Tomato.Media.BackgroundMediaPlayerClient _client;

        public MainViewModel(WinRTContainer container, IEventAggregator eventAggregator)
        {
            _container = container;
            _eventAggregator = eventAggregator;

            _client = new Tomato.Media.BackgroundMediaPlayerClient(typeof(BackgroundAudioPlayerHandler).FullName);
        }

        protected override void OnActivate()
        {
            _eventAggregator.Subscribe(this);
        }

        protected override void OnDeactivate(bool close)
        {
            _eventAggregator.Unsubscribe(this);
        }

        public void SetupNavigationService(Frame frame)
        {
            _navigationService = _container.RegisterNavigationService(frame);
            
            //if (_resume)
            //    _navigationService.ResumeState();
        }
    }
}