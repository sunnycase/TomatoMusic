using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TomatoMusic
{
    /// <summary>
    /// 播放器模块
    /// </summary>
    public static class PlayerModule
    {
        /// <summary>
        /// 添加播放器模块
        /// </summary>
        /// <param name="assemblies">程序集集合</param>
        /// <returns>程序集集合</returns>
        public static ICollection<Assembly> AddApplicationPartPlayer(this ICollection<Assembly> assemblies)
        {
            assemblies.Add(typeof(PlayerModule).Assembly);
            return assemblies;
        }
    }
}
