using System;
using System.Threading.Tasks;
using YouTubeVideoDownload.Services;
using YoutubeExplode.Videos;
using YouTubeVideoDownload.Exceptions;

namespace YouTubeVideoDownload.Commands
{
    /// <summary>
    /// Represents a command to list available video formats (streams) for a given YouTube video.
    /// It uses <see cref="IVideoService"/> to fetch video information and <see cref="ICliService"/>
    /// to display the results.
    /// </summary>
    public class ListFormatsCommand : ICommand
    {
        private readonly IVideoService _videoService;
        private readonly ICliService _cliService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListFormatsCommand"/> class.
        /// </summary>
        /// <param name="videoService">The video service implementation to use for fetching video data.</param>
        /// <param name="cliService">The CLI service implementation to use for displaying information.</param>
        public ListFormatsCommand(IVideoService videoService, ICliService cliService)
        {
            _videoService = videoService ?? throw new ArgumentNullException(nameof(videoService));
            _cliService = cliService ?? throw new ArgumentNullException(nameof(cliService));
        }

        /// <summary>
        /// Asynchronously executes the command to list video formats.
        /// It fetches video metadata and stream manifest, then displays them.
        /// The <paramref name="outputPath"/> and <paramref name="listFormats"/> parameters are noted but may not directly influence
        /// this command's primary behavior if it's solely for listing. The `listFormats` flag is implicitly true for this command.
        /// </summary>
        /// <param name="videoId">The URL or ID of the YouTube video for which to list formats.</param>
        /// <param name="outputPath">Not directly used by this command for listing formats, but part of the <see cref="ICommand"/> interface.</param>
        /// <param name="listFormats">Expected to be true for this command's execution; if false, the command might behave as a no-op or defer to other logic if combined.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ExecuteAsync(string videoId, string outputPath, bool listFormats)
        {
            // This command is specifically for listing formats, so it proceeds regardless of the listFormats flag's value,
            // assuming if this command is chosen, the intent is to list.
            try
            {
                VideoId videoIdParsed;
                try
                {
                    videoIdParsed = VideoId.Parse(videoId);
                }
                catch (ArgumentException)
                {
                    _cliService.DisplayError($"Invalid Video URL or ID format: '{videoId}'. Please provide a valid YouTube video URL or ID.");
                    return;
                }

                _cliService.DisplayMessage("Fetching video metadata and stream information...");
                var videoMetadata = await _videoService.GetVideoMetadataAsync(videoIdParsed);
                var streamManifest = await _videoService.GetStreamManifestAsync(videoIdParsed);

                if (videoMetadata != null && streamManifest != null)
                {
                    _cliService.DisplayStreams(streamManifest, videoMetadata);
                }
                else
                {
                    // This path should ideally not be hit if VideoService throws exceptions on failure.
                    _cliService.DisplayError("Could not retrieve necessary video information to list formats.");
                }
            }
            catch (VideoDownloadException vde)
            {
                _cliService.DisplayError(vde.Message); // Custom exceptions from VideoService are user-friendly.
            }
            catch (Exception ex) // Catch-all for any other unexpected errors.
            {
                Console.WriteLine($"Debug: Unexpected error in ListFormatsCommand - {ex.ToString()}"); // For debug purposes
                _cliService.DisplayError("An unexpected error occurred while trying to list video formats.");
            }
        }
    }
}
