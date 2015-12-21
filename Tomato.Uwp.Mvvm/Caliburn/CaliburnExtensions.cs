using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Caliburn.Micro;

namespace Tomato
{
    public static class CaliburnExtensions
    {
        public static bool SetProperty<T>(this PropertyChangedBase obj, ref T property, T value, [CallerMemberName]string propertyName = null)
        {
            if (!object.Equals(property, value))
            {
                property = value;
                obj.NotifyOfPropertyChange(propertyName);
                return true;
            }
            return false;
        }
    }
}
