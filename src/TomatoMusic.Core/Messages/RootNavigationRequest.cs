using System;
using System.Collections.Generic;
using System.Text;

namespace TomatoMusic.Messages
{
    /// <summary>
    /// 请求根视图导航
    /// </summary>
    public sealed class RootNavigationRequest
    {
        /// <summary>
        /// 获取或设置视图模型类型
        /// </summary>
        public Type ViewModelType { get; set; }
    }
}
