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

        public string Body { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        //public string Accept { get; set; } = string.Empty;

        public bool CheckIsValid()
        {
            var hasValidData = !string.IsNullOrWhiteSpace(FullEndpoint)
                && !string.IsNullOrWhiteSpace(Method);

            if (!string.IsNullOrEmpty(Body))
            {
                hasValidData &= !string.IsNullOrEmpty(Body)
                            && !string.IsNullOrEmpty(ContentType);
                //&& !string.IsNullOrEmpty(Accept);
            }

            return hasValidData;
        }
    }
}