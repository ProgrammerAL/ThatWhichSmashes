using System;
using System.Collections.Generic;
using System.Text;

namespace RequestQueueAdder
{
    public class Request
    {
        public int Count { get; set; }
        public string Name { get; set; }
        public string JsonMessage { get; set; }
    }
}
