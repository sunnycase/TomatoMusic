using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Windows.UI.Xaml.Controls;

namespace Tomato.TomatoMusic.Shell.ViewModels
{
    class MainViewModel : Screen
    {
        private readonly WinRTContainer _container;
        private readonly IEventAggregator _eventAggregator;

        public MainViewModel(WinRTContainer container, IEventAggregator eventAggregator)
        {
            _container = container;
            _eventAggregator = eventAggregator;
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
        //    _navigationService = _container.RegisterNavigationService(frame);

        //    Template10.Services.NavigationService.FrameFacade
        //    if (_resume)
        //        _navigationService.ResumeState();
        }

        public void ExecuteHamburger()
        {

        }
    }
}