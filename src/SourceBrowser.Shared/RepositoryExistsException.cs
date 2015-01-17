using System;
using System.Runtime.Serialization;

namespace SourceBrowser.Shared
{
    [Serializable]
    public class RepositoryExistsException : Exception, ISerializable
    {
        public RepositoryExistsException() : base()
        {
        }

        public RepositoryExistsException(string message) : base(message)
        {
        }

        public RepositoryExistsException(string message, Exception innerException) : base(message, innerException)
        {
        }

        // This constructor is needed for serialization.
        protected RepositoryExistsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
