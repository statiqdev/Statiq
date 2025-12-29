using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Statiq.Common
{
    public static partial class Config
    {
        // This just adds a space to the front of error details so it'll format nicely
        // Used by the extensions that convert values from a DocumentConfig<object> or ContextConfig<object>
        internal static string GetErrorDetails(string errorDetails)
        {
            if (errorDetails?.StartsWith(" ") == false)
            {
                errorDetails = " " + errorDetails;
            }
            return errorDetails;
        }
    }
}
