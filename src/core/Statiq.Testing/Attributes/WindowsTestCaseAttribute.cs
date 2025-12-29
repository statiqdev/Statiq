using System.Runtime.InteropServices;
using NUnit.Framework;

namespace Statiq.Testing
{
    public class WindowsTestCaseAttribute : TestCaseAttribute
    {
        public WindowsTestCaseAttribute(params object[] arguments)
            : base(arguments)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Ignore = "Windows only";
            }
        }

        public WindowsTestCaseAttribute(object arg)
            : base(arg)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Ignore = "Windows only";
            }
        }

        public WindowsTestCaseAttribute(object arg1, object arg2)
            : base(arg1, arg2)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Ignore = "Windows only";
            }
        }

        public WindowsTestCaseAttribute(object arg1, object arg2, object arg3)
            : base(arg1, arg2, arg3)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Ignore = "Windows only";
            }
        }
    }
}
