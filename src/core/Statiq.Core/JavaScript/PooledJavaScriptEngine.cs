using JSPool;

namespace Statiq.Core
{
    /// <summary>
    /// Wraps a pooled JavaScript engine but overrides the
    /// dispose behavior so that instead of disposing the
    /// underlying engine, it returns the engine to the pool.
    /// </summary>
    internal class PooledJavaScriptEngine : JavaScriptEngine
    {
        public PooledJavaScriptEngine(PooledJsEngine pooled, JsPool pool)
            : base(pooled)
        {
            Pooled = pooled;
            Pool = pool;
        }

        internal PooledJsEngine Pooled { get; }

        internal JsPool Pool { get; }
    }
}
