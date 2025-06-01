using System;
using System.Threading.Tasks;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace YouTubeVideoDownload.Services
{
    /// <summary>
    /// Defines the contract for a service that handles video-related operations such as fetching metadata,
    /// stream manifests, and downloading video streams.
    /// </summary>
    public interface IVideoService
    {
        /// <summary>
        /// Asynchronously retrieves metadata for a specified YouTube video.
        /// </summary>
        /// <param name="videoId">The ID of the YouTube video.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the <see cref="Video"/> metadata object
        /// if found; otherwise, this method may throw a <see cref="YouTubeVideoDownload.Exceptions.VideoDownloadException"/>
        /// if the video is unavailable or another error occurs.
        /// </returns>
        /// <exception cref="YouTubeVideoDownload.Exceptions.VideoDownloadException">Thrown when the video is unavailable, requires purchase, or a network error occurs.</exception>
        Task<Video> GetVideoMetadataAsync(VideoId videoId);

        /// <summary>
        /// Asynchronously retrieves the stream manifest for a specified YouTube video.
        /// The manifest contains information about available video streams (different qualities, formats, etc.).
        /// </summary>
        /// <param name="videoId">The ID of the YouTube video.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the <see cref="StreamManifest"/> for the video.
        /// This method may throw a <see cref="YouTubeVideoDownload.Exceptions.VideoDownloadException"/> if the manifest is unavailable or an error occurs.
        /// </returns>
        /// <exception cref="YouTubeVideoDownload.Exceptions.VideoDownloadException">Thrown when the stream manifest is unavailable, requires purchase, or a network/library error occurs.</exception>
        Task<StreamManifest> GetStreamManifestAsync(VideoId videoId);

        /// <summary>
        /// Asynchronously downloads a video stream to the specified file path.
        /// </summary>
        /// <param name="streamInfo">The <see cref="IVideoStreamInfo"/> object containing information about the stream to download.</param>
        /// <param name="filePath">The full path where the video file should be saved.</param>
        /// <param name="progress">An <see cref="IProgress{T}"/> handler to report download progress (typically a value between 0.0 and 1.0).</param>
        /// <returns>A task that represents the asynchronous download operation.</returns>
        /// <exception cref="YouTubeVideoDownload.Exceptions.VideoDownloadException">Thrown if an error occurs during download, such as an IO error, network error, or an issue with the stream itself.</exception>
        Task DownloadVideoStreamAsync(IVideoStreamInfo streamInfo, string filePath, IProgress<double> progress);
    }
}
