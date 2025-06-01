using System;

namespace YouTubeVideoDownload.Exceptions
{
    /// <summary>
    /// Represents errors that occur during YouTube video download operations.
    /// </summary>
    [Serializable] // Good practice for custom exceptions
    public class VideoDownloadException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VideoDownloadException"/> class.
        /// </summary>
        public VideoDownloadException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VideoDownloadException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public VideoDownloadException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VideoDownloadException"/> class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public VideoDownloadException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        // Optional: Add constructor for serialization if targeting .NET Framework or older .NET Core where it was more relevant.
        // For .NET 5+ and modern .NET, [Serializable] is often sufficient for basic purposes,
        // but full ISerializable implementation is needed for complete control across all serialization scenarios (like binary formatter).
        // protected VideoDownloadException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        //     : base(info, context)
        // {
        // }
    }
}
