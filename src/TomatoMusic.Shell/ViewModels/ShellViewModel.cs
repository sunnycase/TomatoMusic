using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using ReactiveUI;
using TomatoMusic.Messages;
using TomatoMusic.Widgets;
using Weingartner.ReactiveCompositeCollections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace TomatoMusic.Shell.ViewModels
{
    /// <summary>
    /// 外壳视图模型
    /// </summary>
    public class ShellViewModel : PropertyChangedBase, IHandle<RootNavigationRequest>
    {
        /// <summary>
        /// 获取导航菜单项集合
        /// </summary>
        public ReadOnlyObservableCollection<NavigationViewItemBase> NavigationMenuItems { get; }

        private readonly IEventAggregator _eventAggregator;
        private INavigationService _navigationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShellViewModel"/> class.
        /// </summary>
        /// <param name="eventAggregator">事件聚合器</param>
        /// <param name="menuWidgets">导航菜单组件集合</param>
        public ShellViewModel(IEventAggregator eventAggregator, IEnumerable<NavigationMenuWidget> menuWidgets)
        {
            _eventAggregator = eventAggregator;
            eventAggregator.Subscribe(this);
            NavigationMenuItems = LoadNavigationMenuItems(menuWidgets);
        }

        /// <summary>
        /// 设置导航框架
        /// </summary>
        /// <param name="sender">框架</param>
        /// <param name="e">参数</param>
        public void SetupNavigationFrame(object sender, RoutedEventArgs e)
        {
            _navigationService = new FrameAdapter((Frame)sender, treatViewAsLoaded: true);
        }

        /// <summary>
        /// 当导航菜单触发时
        /// </summary>
        /// <param name="sender">导航容器</param>
        /// <param name="args">事件参数</param>
        public void OnNavigationMenuItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
            }
            else
            {
                _eventAggregator.BeginPublishOnUIThread(((NavigationMenuItemContent)args.InvokedItem).NavigationRequest);
            }
        }

        void IHandle<RootNavigationRequest>.Handle(RootNavigationRequest message)
        {
            _navigationService.NavigateToViewModel(message.ViewModelType);
        }

        private ReadOnlyObservableCollection<NavigationViewItemBase> LoadNavigationMenuItems(IEnumerable<NavigationMenuWidget> menuWidgets)
        {
            return (from w in menuWidgets
                    select w.MenuItems).Concat().CreateObservableCollection(EqualityComparer<NavigationViewItemBase>.Default);
        }
    }
}
