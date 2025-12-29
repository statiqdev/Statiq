using System;
using SixLabors.ImageSharp.Processing;
using Statiq.Common;

namespace Statiq.Images.Operations
{
    internal class ActionOperation : IImageOperation
    {
        private readonly Func<IImageProcessingContext, IImageProcessingContext> _operation;
        private readonly Func<NormalizedPath, NormalizedPath> _pathModifier;

        public ActionOperation(
            Func<IImageProcessingContext, IImageProcessingContext> operation,
            Func<NormalizedPath, NormalizedPath> pathModifier)
        {
            _operation = operation;
            _pathModifier = pathModifier;
        }

        public IImageProcessingContext Apply(IImageProcessingContext image) =>
            _operation is null ? image : _operation(image);

        public NormalizedPath GetPath(NormalizedPath path) =>
            _pathModifier is null ? path : _pathModifier(path);
    }
}
