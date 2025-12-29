using SixLabors.ImageSharp.Processing;
using Statiq.Common;

namespace Statiq.Images.Operations
{
    internal interface IImageOperation
    {
        IImageProcessingContext Apply(IImageProcessingContext image);
        NormalizedPath GetPath(NormalizedPath path);
    }
}
