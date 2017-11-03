using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Microsoft.Extensions.Logging;

namespace TomatoMusic.Shell.Infrastructure
{
    /// <summary>
    /// Caliburn.Micro 的 Log wrapper
    /// </summary>
    public class CaliburnLogger : ILog
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CaliburnLogger"/> class.
        /// </summary>
        /// <param name="logger">logger</param>
        public CaliburnLogger(ILogger logger)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public void Error(Exception exception)
        {
            _logger.LogError(default(EventId), exception, exception.Message);
        }

        /// <inheritdoc/>
        public void Info(string format, params object[] args)
        {
            _logger.LogInformation(format, args);
        }

        /// <inheritdoc/>
        public void Warn(string format, params object[] args)
        {
            _logger.LogWarning(format, args);
        }
    }
}
