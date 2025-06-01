using System;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Exceptions;
using System.Net.Http;
using System.IO;
using YouTubeVideoDownload.Exceptions;

namespace YouTubeVideoDownload.Services
{
    /// <summary>
    /// Implements <see cref="IVideoService"/> to provide functionality for interacting with YouTube videos,
    /// such as fetching metadata, stream manifests, and downloading video streams.
    /// It handles exceptions from the YoutubeExplode library and wraps them in <see cref="VideoDownloadException"/>.
    /// </summary>
    public class VideoService : IVideoService
    {
        private readonly YoutubeClient _youtubeClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="VideoService"/> class.
        /// Creates a new <see cref="YoutubeClient"/> for interacting with YouTube.
        /// </summary>
        public VideoService()
        {
            _youtubeClient = new YoutubeClient();
        }

        /// <summary>
        /// Asynchronously retrieves metadata for a specified YouTube video.
        /// </summary>
        /// <param name="videoId">The ID of the YouTube video.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the <see cref="Video"/> metadata object.
        /// </returns>
        /// <exception cref="VideoDownloadException">Thrown when the video is unavailable, requires purchase, a network error occurs, or an unexpected error happens.</exception>
        public async Task<Video> GetVideoMetadataAsync(VideoId videoId)
        {
            try
            {
                return await _youtubeClient.Videos.GetAsync(videoId);
            }
            catch (VideoUnavailableException ex)
            {
                Console.WriteLine($"Debug: VideoUnavailableException - {ex.Message}"); // Logging placeholder
                throw new VideoDownloadException($"The video '{videoId}' is unavailable. It may have been deleted or set to private.", ex);
            }
            catch (VideoRequiresPurchaseException ex)
            {
                Console.WriteLine($"Debug: VideoRequiresPurchaseException - {ex.Message}"); // Logging placeholder
                throw new VideoDownloadException($"The video '{videoId}' requires purchase and cannot be accessed.", ex);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Debug: HttpRequestException - {ex.Message}"); // Logging placeholder
                throw new VideoDownloadException("A network error occurred while trying to fetch video metadata. Please check your internet connection.", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Debug: Generic Exception in GetVideoMetadataAsync - {ex.Message}"); // Logging placeholder
                throw new VideoDownloadException($"An unexpected error occurred while fetching metadata for video '{videoId}'.", ex);
            }
        }

        /// <summary>
        /// Asynchronously retrieves the stream manifest for a specified YouTube video.
        /// </summary>
        /// <param name="videoId">The ID of the YouTube video.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the <see cref="StreamManifest"/> for the video.
        /// </returns>
        /// <exception cref="VideoDownloadException">Thrown if the manifest is unavailable, requires purchase, or a network/library error occurs.</exception>
        public async Task<StreamManifest> GetStreamManifestAsync(VideoId videoId)
        {
            try
            {
                return await _youtubeClient.Videos.Streams.GetManifestAsync(videoId);
            }
            catch (VideoUnavailableException ex)
            {
                Console.WriteLine($"Debug: VideoUnavailableException - {ex.Message}"); // Logging placeholder
                throw new VideoDownloadException($"Stream manifest for video '{videoId}' is unavailable. The video may have been deleted or set to private.", ex);
            }
             catch (VideoRequiresPurchaseException ex)
            {
                Console.WriteLine($"Debug: VideoRequiresPurchaseException - {ex.Message}"); // Logging placeholder
                throw new VideoDownloadException($"Stream manifest for video '{videoId}' requires purchase and cannot be accessed.", ex);
            }
            catch (YoutubeExplodeException ex)
            {
                 Console.WriteLine($"Debug: YoutubeExplodeException in GetStreamManifestAsync - {ex.Message}"); // Logging placeholder
                 throw new VideoDownloadException($"Could not extract stream information for video '{videoId}'. The video might be region-restricted or have other playback limitations.", ex);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Debug: HttpRequestException - {ex.Message}"); // Logging placeholder
                throw new VideoDownloadException("A network error occurred while trying to fetch the stream manifest. Please check your internet connection.", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Debug: Generic Exception in GetStreamManifestAsync - {ex.Message}"); // Logging placeholder
                throw new VideoDownloadException($"An unexpected error occurred while fetching the stream manifest for video '{videoId}'.", ex);
            }
        }

        /// <summary>
        /// Asynchronously downloads a video stream to the specified file path.
        /// </summary>
        /// <param name="streamInfo">The <see cref="IVideoStreamInfo"/> object containing information about the stream to download.</param>
        /// <param name="filePath">The full path where the video file should be saved.</param>
        /// <param name="progress">An <see cref="IProgress{T}"/> handler to report download progress.</param>
        /// <returns>A task that represents the asynchronous download operation.</returns>
        /// <exception cref="VideoDownloadException">Thrown if an error occurs during download, such as an IO error, network error, or an issue with the stream itself.</exception>
        public async Task DownloadVideoStreamAsync(IVideoStreamInfo streamInfo, string filePath, IProgress<double> progress)
        {
            try
            {
                var directoryPath = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                await _youtubeClient.Videos.Streams.DownloadAsync(streamInfo, filePath, progress);
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Debug: IOException - {ex.Message}"); // Logging placeholder
                throw new VideoDownloadException($"An error occurred while writing the video file to '{filePath}'. Please check disk space and permissions.", ex);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Debug: HttpRequestException - {ex.Message}"); // Logging placeholder
                throw new VideoDownloadException("A network error occurred during the video download. Please check your internet connection.", ex);
            }
            catch (YoutubeExplodeException ex)
            {
                 Console.WriteLine($"Debug: YoutubeExplodeException in DownloadVideoStreamAsync - {ex.Message}"); // Logging placeholder
                 throw new VideoDownloadException($"An error related to YouTube's service occurred during download.", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Debug: Generic Exception in DownloadVideoStreamAsync - {ex.Message}"); // Logging placeholder
                throw new VideoDownloadException($"An unexpected error occurred while downloading the video to '{filePath}'.", ex);
            }
        }
    }
}
