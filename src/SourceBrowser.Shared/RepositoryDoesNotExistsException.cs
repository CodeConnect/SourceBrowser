using System;
using System.Runtime.Serialization;

namespace SourceBrowser.Shared
{
    [Serializable]
    public class RepositoryDoesNotExistException : Exception, ISerializable
    {
        public RepositoryDoesNotExistException() : base()
        {
        }

        public RepositoryDoesNotExistException(string message) : base(message)
        {
        }

        public RepositoryDoesNotExistException(string message, Exception innerException) : base(message, innerException)
        {
        }

        // This constructor is needed for serialization.
        protected RepositoryDoesNotExistException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
