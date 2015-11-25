using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace Tomato.TomatoMusic.Shell
{
    class ViewModelBase : Screen
    {
        private readonly INavigationService _pageNavigationService;

        protected ViewModelBase(INavigationService pageNavigationService)
        {
            _pageNavigationService = pageNavigationService;
        }

        public bool CanGoBack
        {
            get { return _pageNavigationService.CanGoBack; }
        }

        protected void NavigateTo<T>()
        {
            _pageNavigationService.Navigate<T>();
        }

        public void GoBack()
        {
            _pageNavigationService.GoBack();
        }
    }
}
