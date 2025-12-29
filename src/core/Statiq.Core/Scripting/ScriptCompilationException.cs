using System;
using System.Collections.Generic;

namespace Statiq.Core
{
    public class ScriptCompilationException : Exception
    {
        public IReadOnlyList<string> ErrorMessages { get; }

        public ScriptCompilationException(List<string> errorMessages)
        {
            ErrorMessages = errorMessages.AsReadOnly();
        }
    }
}