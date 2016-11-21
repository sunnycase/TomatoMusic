using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tomato.TomatoMusic
{
    public static class IReadOnlyListExtensions
    {
        public static int IndexOf<T>(this IReadOnlyList<T> col, T value)
        {
            for (int i = 0; i < col.Count; i++)
            {
                if (EqualityComparer<T>.Default.Equals(col[i], value))
                    return i;
            }
            return -1;
        }
    }
}
