using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;
using Statiq.Common;

namespace Statiq.Core
{
    public class RetryModules : ParentModule
    {
        private readonly HashSet<Type> _handledExceptionTypes = new HashSet<Type>();
        private int _retryCount = 5;
        private Func<int, TimeSpan> _sleepDurationProvider = x => TimeSpan.FromSeconds(Math.Pow(2, x));
        private Func<int, string> _failureMessageFactory;
        private LogLevel _failureMessageLogLevel;

        public RetryModules(params IModule[] modules)
            : base(modules)
        {
        }

        public RetryModules Handle<TException>()
            where TException : Exception
        {
            _handledExceptionTypes.Add(typeof(TException));
            return this;
        }

        public RetryModules WithRetries(int retryCount)
        {
            _retryCount = retryCount;
            return this;
        }

        public RetryModules WithSleepDuration(Func<int, TimeSpan> sleepDurationProvider)
        {
            _sleepDurationProvider = sleepDurationProvider.ThrowIfNull(nameof(sleepDurationProvider));
            return this;
        }

        public RetryModules WithFailureMessage(Func<int, string> failureMessageFactory, LogLevel logLevel)
        {
            _failureMessageFactory = failureMessageFactory;
            _failureMessageLogLevel = logLevel;
            return this;
        }

        protected override async Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context) =>
            await Policy
                .Handle<Exception>(ex =>
                {
                    if (ex is LoggedException loggedException)
                    {
                        ex = loggedException.InnerException;
                    }
                    return !_handledExceptionTypes.Contains(ex.GetType());
                })
                .WaitAndRetryAsync(
                    _retryCount,
                    attempt =>
                    {
                        if (_failureMessageFactory is object)
                        {
                            string failureMessage = _failureMessageFactory(attempt);
                            if (!string.IsNullOrWhiteSpace(failureMessage))
                            {
                                context.Log(_failureMessageLogLevel, failureMessage);
                            }
                        }
                        return _sleepDurationProvider(attempt);
                    })
                .ExecuteAsync(
                    async _ => await context.ExecuteModulesAsync(Children, context.Inputs),
                    context.CancellationToken);
    }
}