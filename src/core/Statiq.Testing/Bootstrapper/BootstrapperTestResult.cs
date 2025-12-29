using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using Statiq.Common;

namespace Statiq.Testing
{
    public class BootstrapperTestResult
    {
        public ConcurrentQueue<TestMessage> LogMessages { get; set; }

        public int ExitCode { get; set; }

        public IEngine Engine { get; set; }

        public IDictionary<string, Dictionary<Phase, ImmutableArray<IDocument>>> Inputs { get; set; }

        public IDictionary<string, Dictionary<Phase, ImmutableArray<IDocument>>> Outputs { get; set; }
    }
}
