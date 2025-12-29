using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Statiq.Common;

namespace Statiq.Images.Operations
{
    internal class OutputAction
    {
        private readonly Action<Image<Rgba32>, Stream> _action;
        private readonly Func<NormalizedPath, NormalizedPath> _pathModifier;

        public OutputAction(
            Action<Image<Rgba32>, Stream> action,
            Func<NormalizedPath, NormalizedPath> pathModifier)
        {
            _action = action;
            _pathModifier = pathModifier;
        }

        public void Invoke(Image<Rgba32> image, Stream stream) =>
            _action?.Invoke(image, stream);

        public NormalizedPath GetPath(in NormalizedPath path) =>
            _pathModifier is null ? path : _pathModifier(path);
    }
}
