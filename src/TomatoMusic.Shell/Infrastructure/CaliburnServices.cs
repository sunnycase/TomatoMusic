using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Microsoft.Extensions.DependencyInjection;

namespace TomatoMusic.Shell.Infrastructure
{
    internal static class CaliburnServices
    {
        public static IServiceCollection AddCaliburn(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IEventAggregator, EventAggregator>();

            return serviceCollection;
        }
    }
}
