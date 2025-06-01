using System;
using System.Threading.Tasks;
using YouTubeVideoDownload.Services;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;
using YouTubeVideoDownload.Exceptions;
using System.IO;
using System.Linq;
using YouTubeVideoDownload.Extensions;
using YouTubeVideoDownload.Utilities;

namespace YouTubeVideoDownload.Commands
{
    /// <summary>
    /// Represents a command to download a YouTube video.
    /// This command handles fetching video metadata, selecting a stream (interactively),
    /// and downloading the video to a specified output path.
    /// It uses <see cref="IVideoService"/> for video operations and <see cref="ICliService"/> for user interactions.
    /// </summary>
    public class DownloadVideoCommand : ICommand
    {
        private readonly IVideoService _videoService;
        private readonly ICliService _cliService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DownloadVideoCommand"/> class.
        /// </summary>
        /// <param name="videoService">The video service implementation for YouTube operations.</param>
        /// <param name="cliService">The CLI service implementation for user interactions and display.</param>
        public DownloadVideoCommand(IVideoService videoService, ICliService cliService)
        {
            _videoService = videoService ?? throw new ArgumentNullException(nameof(videoService));
            _cliService = cliService ?? throw new ArgumentNullException(nameof(cliService));
        }

        /// <summary>
        /// Asynchronously executes the video download command.
        /// </summary>
        /// <param name="videoId">The URL or ID of the YouTube video to download.</param>
        /// <param name="outputPath">The directory path where the video file should be saved.</param>
        /// <param name="listFormats">If true, this command will only list available formats and not proceed with download.
        /// If false (default intent for this command), it will attempt to download the video.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ExecuteAsync(string videoId, string outputPath, bool listFormats)
        {
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

                _cliService.DisplayMessage($"Fetching video information for: {videoId}...");
                var videoMetadata = await _videoService.GetVideoMetadataAsync(videoIdParsed);
                var streamManifest = await _videoService.GetStreamManifestAsync(videoIdParsed);

                if (videoMetadata == null || streamManifest == null)
                {
                    _cliService.DisplayError("Could not retrieve essential video metadata or stream information. Aborting download.");
                    return;
                }

                if (listFormats) // If user explicitly wants to list formats, even via DownloadCommand path
                {
                    _cliService.DisplayStreams(streamManifest, videoMetadata);
                    _cliService.DisplayMessage("\nList formats requested. To download, run the command without the --list-formats option or ensure it's not set when download is intended.");
                    return;
                }

                // Proceed with download if not just listing formats
                if (string.IsNullOrWhiteSpace(outputPath))
                {
                    _cliService.DisplayError("Output path is required for downloading a video. Please specify with -o or --output.");
                    return;
                }

                var muxedStreams = streamManifest.GetMuxedStreams().OrderByDescending(s => s.VideoQuality).ToArray();
                if (!muxedStreams.Any())
                {
                    _cliService.DisplayError("No suitable (muxed video+audio) streams found to download for this video.");
                    return;
                }

                _cliService.DisplayStreams(streamManifest, videoMetadata); // Show streams before asking for selection

                int selectedStreamIndex = _cliService.PromptForStreamSelection(muxedStreams.Length) -1; // Adjust to 0-based index
                var selectedStreamInfo = muxedStreams[selectedStreamIndex];

                string cleanTitle = videoMetadata.Title.GenerateSlug();
                string fileName = $"{cleanTitle}.{selectedStreamInfo.Container.Name}";
                string fullFilePath = Path.Combine(outputPath, fileName);

                if (!_cliService.EnsureDirectoryExists(fullFilePath))
                {
                    // Error message already displayed by EnsureDirectoryExists in CliService
                    return;
                }

                _cliService.DisplayMessage($"\nStarting download for '{videoMetadata.Title}'");
                _cliService.DisplayMessage($"Selected quality: {selectedStreamInfo.VideoQuality.Label}");
                _cliService.DisplayMessage($"Output file: {fullFilePath}");

                // Using 'using' for IDisposable ProgressIndicator
                using (var progressIndicator = new ProgressIndicator())
                {
                    await _videoService.DownloadVideoStreamAsync(selectedStreamInfo, fullFilePath, progressIndicator);
                }

                _cliService.DisplayMessage($"\nDownload of '{videoMetadata.Title}' complete!");
                _cliService.DisplayMessage($"File saved to: {fullFilePath}");

            }
            catch (VideoDownloadException vde)
            {
                _cliService.DisplayError(vde.Message); // User-friendly messages from VideoService
            }
            catch (Exception ex) // Catch-all for other unexpected errors during command execution.
            {
                Console.WriteLine($"Debug: Unexpected error in DownloadVideoCommand - {ex.ToString()}"); // For debug
                _cliService.DisplayError("An unexpected error occurred while trying to download the video.");
            }
        }
    }
}
