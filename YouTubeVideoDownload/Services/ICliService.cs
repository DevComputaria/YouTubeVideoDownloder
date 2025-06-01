using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace YouTubeVideoDownload.Services
{
    /// <summary>
    /// Defines the contract for a service that handles command-line interface (CLI) interactions.
    /// This includes displaying information to the user, prompting for input, and managing console output.
    /// </summary>
    public interface ICliService
    {
        /// <summary>
        /// Displays available video streams to the console in a formatted manner.
        /// </summary>
        /// <param name="manifest">The <see cref="StreamManifest"/> containing stream information.</param>
        /// <param name="video">The <see cref="Video"/> metadata object.</param>
        void DisplayStreams(StreamManifest manifest, Video video);

        /// <summary>
        /// Prompts the user to select a stream number from a list and validates the input.
        /// </summary>
        /// <param name="streamCount">The total number of available streams to choose from.</param>
        /// <returns>The 1-based selected stream number. Returns 0 or throws an exception if input is cancelled or invalid beyond recovery (not current behavior).</returns>
        int PromptForStreamSelection(int streamCount);

        /// <summary>
        /// Ensures that the directory for the given file path exists.
        /// If the directory does not exist, it attempts to create it.
        /// </summary>
        /// <param name="filePath">The full file path for which the directory needs to be checked/created.</param>
        /// <returns><c>true</c> if the directory exists or was successfully created; otherwise, <c>false</c>.</returns>
        bool EnsureDirectoryExists(string filePath);

        /// <summary>
        /// Displays a formatted error message to the console (e.g., in red color).
        /// </summary>
        /// <param name="message">The error message to display.</param>
        void DisplayError(string message);

        /// <summary>
        /// Displays a standard informational message to the console.
        /// </summary>
        /// <param name="message">The message to display.</param>
        void DisplayMessage(string message);
    }
}
