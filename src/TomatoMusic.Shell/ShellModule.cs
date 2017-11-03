using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TomatoMusic
{
    /// <summary>
    /// 外壳模块
    /// </summary>
    public static class ShellModule
    {
        /// <summary>
        /// 添加外壳模块
        /// </summary>
        /// <param name="assemblies">程序集集合</param>
        /// <returns>程序集集合</returns>
        public static ICollection<Assembly> AddApplicationPartShell(this ICollection<Assembly> assemblies)
        {
            assemblies.Add(typeof(ShellModule).Assembly);
            return assemblies;
        }
    }
}
