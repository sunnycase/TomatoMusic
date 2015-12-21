using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tomato
{
    public static class ExceptionExtensions
    {
        public static string Flatten(this Exception exception)
        {
            var sb = new StringBuilder();
            while (exception != null)
            {
                sb.AppendLine(exception.Message);
                exception = exception.InnerException;
            }
            return sb.ToString();
        }
    }
}
