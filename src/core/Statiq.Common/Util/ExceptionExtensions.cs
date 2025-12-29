using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Statiq.Common
{
    public static class ExceptionExtensions
    {
        public static IEnumerable<Exception> Unwrap(this Exception exception, bool unwrapLoggedExceptions)
        {
            if (exception is object)
            {
                switch (exception)
                {
                    case AggregateException aggregateException:
                        return aggregateException.InnerExceptions.SelectMany(x => x.Unwrap(unwrapLoggedExceptions));
                    case TargetInvocationException invocationException:
                        return invocationException.InnerException.Unwrap(unwrapLoggedExceptions);
                    case LoggedException loggedException when unwrapLoggedExceptions:
                        return loggedException.InnerException.Unwrap(true);
                    case TaskCanceledException taskCanceledException:
                        // Timeouts will throw this and may have an inner exception (I.e. https://stackoverflow.com/a/66694886)
                        return taskCanceledException.InnerException is object
                            ? taskCanceledException.InnerException.Unwrap(unwrapLoggedExceptions)
                            : new Exception[] { taskCanceledException };
                    default:
                        return new Exception[] { exception };
                }
            }

            return Array.Empty<Exception>();
        }
    }
}