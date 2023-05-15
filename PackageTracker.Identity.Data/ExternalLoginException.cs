using System;

namespace PackageTracker.Identity.Data
{
    public class ExternalLoginException : Exception
    {
        public ExternalLoginException(string message) : base(message) { }
    }
}
