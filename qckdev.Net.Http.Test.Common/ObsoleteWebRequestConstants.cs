using System;

namespace qckdev.Net.Http.Test.Common
{
    /// <summary>
    /// Provides centralized constants for WebRequest and WebClient obsolescence attributes.
    /// This allows the obsolete message to be maintained in a single location.
    /// </summary>
    public static class ObsoleteWebRequestConstants
    {
        /// <summary>
        /// The standard obsolescence message for WebRequest and WebClient related code.
        /// </summary>
        public const string Message = "WebRequest, HttpWebRequest, ServicePoint, and WebClient are obsolete. Use HttpClient instead.";
    }
}
