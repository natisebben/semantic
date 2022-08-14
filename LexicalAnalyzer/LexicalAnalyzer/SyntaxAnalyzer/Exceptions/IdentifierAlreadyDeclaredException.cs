using System;

namespace Analyzer.Exceptions
{
    public class IdentifierAlreadyDeclaredException : Exception
    {
        public IdentifierAlreadyDeclaredException(int currentLine, string identifier) : base($"Line {currentLine}: {identifier} already declared.")
        {

        }
    }
}
