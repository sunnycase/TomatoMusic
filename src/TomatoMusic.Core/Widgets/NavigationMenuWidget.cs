using System;
using System.Collections.Generic;
using System.Text;
using TomatoMusic.Messages;
using Weingartner.ReactiveCompositeCollections;
using Windows.UI.Xaml.Controls;

namespace TomatoMusic.Widgets
{
    /// <summary>
    /// 导航菜单项内容
    /// </summary>
    public class NavigationMenuItemContent
    {
        /// <summary>
        /// 获取或设置文本
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// 获取或设置导航请求
        /// </summary>
        public RootNavigationRequest NavigationRequest { get; set; }

        /// <inheritdoc/>
        public override string ToString() => Text;
    }

    /// <summary>
    /// 导航菜单组件
    /// </summary>
    public class NavigationMenuWidget
    {
        /// <summary>
        /// 获取或设置菜单项
        /// </summary>
        public CompositeSourceList<NavigationViewItemBase> MenuItems { get; set; }
    }
}
