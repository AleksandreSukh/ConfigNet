using System;
using System.Runtime.Serialization;

namespace ConfigNet
{
    public class ConfigReaderException : Exception
    {
        public ConfigReaderException()
        {
        }

        public ConfigReaderException(string message) : base(message)
        {
        }

        public ConfigReaderException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ConfigReaderException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}