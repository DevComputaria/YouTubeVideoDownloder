using System.Threading.Tasks;

namespace YouTubeVideoDownload.Commands
{
    /// <summary>
    /// Defines the contract for a command that can be executed within the application.
    /// Commands encapsulate specific actions or operations.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Asynchronously executes the command with the specified parameters.
        /// </summary>
        /// <param name="videoId">The URL or ID of the YouTube video to operate on.</param>
        /// <param name="outputPath">The file system path for output, typically for downloads. May be null or ignored by some commands.</param>
        /// <param name="listFormats">A flag indicating whether to list available video formats. Behavior may vary by command.</param>
        /// <returns>A task representing the asynchronous operation of the command.</returns>
        Task ExecuteAsync(string videoId, string outputPath, bool listFormats);
    }
}
