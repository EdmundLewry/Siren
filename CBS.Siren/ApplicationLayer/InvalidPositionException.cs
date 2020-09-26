using System;
using System.Runtime.Serialization;

namespace CBS.Siren.Application
{
    public class InvalidPositionException : ArgumentException
    {
        public InvalidPositionException()
        {
        }

        public InvalidPositionException(string message) : base(message)
        {
        }

        public InvalidPositionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public InvalidPositionException(string message, string paramName) : base(message, paramName)
        {
        }

        public InvalidPositionException(string message, string paramName, Exception innerException) : base(message, paramName, innerException)
        {
        }

        protected InvalidPositionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

    }
}