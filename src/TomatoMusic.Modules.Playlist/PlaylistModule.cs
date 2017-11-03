using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using TomatoMusic.Widgets;

namespace TomatoMusic
{
    /// <summary>
    /// 播放列表模块
    /// </summary>
    public static class PlaylistModule
    {
        internal static NavigationMenuWidget MenuWidget { get; } = new PlaylistNavigationMenuWidget();

        /// <summary>
        /// 添加播放列表模块
        /// </summary>
        /// <param name="assemblies">程序集集合</param>
        /// <returns>程序集集合</returns>
        public static ICollection<Assembly> AddApplicationPartPlaylist(this ICollection<Assembly> assemblies)
        {
            assemblies.Add(typeof(PlaylistModule).Assembly);
            return assemblies;
        }

        internal class Module : Autofac.Module
        {
            protected override void Load(ContainerBuilder builder)
            {
                builder.RegisterInstance(MenuWidget);
            }
        }
    }
}
