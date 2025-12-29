using System.IO;
using System.Text;

namespace Statiq.Common
{
    public class NullTextWriter : TextWriter
    {
        public static NullTextWriter Instance { get; } = new NullTextWriter();

        private NullTextWriter()
        {
        }

        public override Encoding Encoding => Encoding.Default;
    }
}