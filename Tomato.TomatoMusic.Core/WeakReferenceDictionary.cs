using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tomato
{
    public class WeakReferenceDictionary<TKey, TValue> : Dictionary<TKey, WeakReference<TValue>> where TValue : class
    {
        public WeakReferenceDictionary()
        {

        }

        public TValue GetValue(TKey key, Func<TValue> onCreate)
        {
            TValue value;
            WeakReference<TValue> weak;
            if (base.TryGetValue(key, out weak))
            {
                if (weak.TryGetTarget(out value))
                    return value;
                else
                {
                    value = onCreate();
                    weak.SetTarget(value);
                    return value;
                }
            }
            throw new KeyNotFoundException();
        }

        public TValue GetOrAddValue(TKey key, Func<TValue> onCreate)
        {
            TValue value;
            WeakReference<TValue> weak;
            if (base.TryGetValue(key, out weak))
            {
                if (weak.TryGetTarget(out value))
                    return value;
                else
                {
                    value = onCreate();
                    weak.SetTarget(value);
                    return value;
                }
            }
            else
            {
                value = onCreate();
                base.Add(key, new WeakReference<TValue>(value));
                return value;
            }
        }
    }
}
