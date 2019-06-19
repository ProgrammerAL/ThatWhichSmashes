using System;

namespace RequestorUtilities
{
    public class ExtraQueuedRequest
    {
        public string FullEndpoint { get; set; } = string.Empty;

        /// <summary>
        /// Only GET or POST are implemented right now
        /// </summary>
        public string Method { get; set; } = string.Empty;

        public string BodyJson { get; set; } = string.Empty;

        public bool CheckIsValid()
        {
            return !string.IsNullOrWhiteSpace(FullEndpoint)
                && !string.IsNullOrWhiteSpace(Method)
                && BodyJson != null;
        }
    }
}