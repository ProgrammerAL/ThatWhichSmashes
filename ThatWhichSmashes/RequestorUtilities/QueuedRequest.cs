using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Linq;

namespace RequestorUtilities
{
    public class QueuedRequest
    {
        private const int DefaultTimeBetweenRequestsInMilliseconds = 500;

        public string InitialUrl { get; set; } = string.Empty;
        public int TimeBetweenRequestsInMs { get; set; } = DefaultTimeBetweenRequestsInMilliseconds;

        public ImmutableList<ExtraQueuedRequest> ExtraRequests { get; set; } = ImmutableList.Create<ExtraQueuedRequest>();

        public bool CheckIsValid()
        {
            return !string.IsNullOrWhiteSpace(InitialUrl)
                && ExtraRequests != null
                && ExtraRequests.All(x => x.CheckIsValid());
        }
    }
}
