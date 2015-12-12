using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Windows.UI.Xaml.Controls;
using Tomato.TomatoMusic.Services;

namespace Tomato.TomatoMusic.Shell.ViewModels
{
    class MainViewModel : Screen
    {
        private readonly WinRTContainer _container;
        private readonly IEventAggregator _eventAggregator;
        private INavigationService _navigationService;

        public IPlaySessionService PlaySession { get; }

        public MainViewModel(WinRTContainer container, IEventAggregator eventAggregator,
            IPlaySessionService playSessionService)
        {
            _container = container;
            _eventAggregator = eventAggregator;
            PlaySession = playSessionService;
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