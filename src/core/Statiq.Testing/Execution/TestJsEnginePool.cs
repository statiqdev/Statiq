using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Statiq.Common;

namespace Statiq.Testing
{
    public class TestJsEnginePool : IJavaScriptEnginePool
    {
        private readonly Func<IJavaScriptEngine> _engineFunc;
        private readonly Action<IJavaScriptEngine> _initializer;

        public TestJsEnginePool(Func<IJavaScriptEngine> engineFunc, Action<IJavaScriptEngine> initializer)
        {
            _engineFunc = engineFunc;
            _initializer = initializer;
        }

        public IJavaScriptEngine GetEngine(TimeSpan? timeout = null)
        {
            IJavaScriptEngine engine = _engineFunc();
            _initializer?.Invoke(engine);
            return engine;
        }

        public void Dispose()
        {
        }

        public void RecycleEngine(IJavaScriptEngine engine)
        {
            throw new NotImplementedException();
        }

        public void RecycleAllEngines()
        {
            throw new NotImplementedException();
        }
    }
}
