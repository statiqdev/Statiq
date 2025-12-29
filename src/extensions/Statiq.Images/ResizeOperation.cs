using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Statiq.Common;

namespace Statiq.Images.Operations
{
    internal class ResizeOperation : IImageOperation
    {
        private readonly int? _width;
        private readonly int? _height;
        private readonly AnchorPositionMode _anchor;
        private readonly ResizeMode _mode;

        public ResizeOperation(int? width, int? height, AnchorPositionMode anchor, ResizeMode mode)
        {
            _width = width;
            _height = height;
            _mode = mode;
            _anchor = anchor;
        }

        public IImageProcessingContext Apply(IImageProcessingContext image)
        {
            Size? size = GetSize();
            if (size is null)
            {
                return image;
            }
            return image.Resize(new ResizeOptions
            {
                Size = size.Value,
                Position = _anchor,
                Mode = _mode
            });
        }

        public NormalizedPath GetPath(NormalizedPath path)
        {
            if (_width.HasValue && _height.HasValue)
            {
                return path.InsertSuffix($"-w{_width.Value}-h{_height.Value}");
            }
            else if (_width.HasValue)
            {
                return path.InsertSuffix($"-w{_width.Value}");
            }
            else if (_height.HasValue)
            {
                return path.InsertSuffix($"-h{_height.Value}");
            }
            return path;
        }

        private Size? GetSize()
        {
            if (_width.HasValue && _height.HasValue)
            {
                return new Size(_width.Value, _height.Value);
            }
            else if (_width.HasValue)
            {
                return new Size(_width.Value, 0);
            }
            else if (_height.HasValue)
            {
                return new Size(0, _height.Value);
            }

            return null;
        }
    }
}
