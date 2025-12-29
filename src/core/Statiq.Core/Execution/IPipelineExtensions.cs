using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    public static class IPipelineExtensions
    {
        public static TPipeline WithInputConfig<TPipeline>(this TPipeline pipeline, Config<object> config)
            where TPipeline : IPipeline
        {
            pipeline.InputModules.Add(new ExecuteConfig(config));
            return pipeline;
        }

        public static TPipeline WithProcessConfig<TPipeline>(this TPipeline pipeline, Config<object> config)
            where TPipeline : IPipeline
        {
            pipeline.ProcessModules.Add(new ExecuteConfig(config));
            return pipeline;
        }

        public static TPipeline WithPostProcessConfig<TPipeline>(this TPipeline pipeline, Config<object> config)
            where TPipeline : IPipeline
        {
            pipeline.PostProcessModules.Add(new ExecuteConfig(config));
            return pipeline;
        }

        public static TPipeline WithOutputConfig<TPipeline>(this TPipeline pipeline, Config<object> config)
            where TPipeline : IPipeline
        {
            pipeline.OutputModules.Add(new ExecuteConfig(config));
            return pipeline;
        }
    }
}
