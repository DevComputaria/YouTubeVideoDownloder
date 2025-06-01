using Xunit;
using Moq;
using YouTubeVideoDownload.Commands;
using YouTubeVideoDownload.Services;
using YouTubeVideoDownload.Exceptions;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;
using System;
using System.Threading.Tasks;
using System.Collections.Generic; // For List
using YoutubeExplode.Common; // For Author
using YoutubeExplode.Videos.ClosedCaptions; // For ClosedCaptionManifest

namespace YouTubeVideoDownload.Tests
{
    public class ListFormatsCommandTests
    {
        private readonly Mock<IVideoService> _mockVideoService;
        private readonly Mock<ICliService> _mockCliService;
        private readonly ListFormatsCommand _listFormatsCommand;

        public ListFormatsCommandTests()
        {
            _mockVideoService = new Mock<IVideoService>();
            _mockCliService = new Mock<ICliService>();
            _listFormatsCommand = new ListFormatsCommand(_mockVideoService.Object, _mockCliService.Object);
        }

        [Fact]
        public async Task ExecuteAsync_ValidVideoId_CallsServicesAndDisplaysStreams()
        {
            // Arrange
            var videoIdStr = "validVideoId";
            var videoId = new VideoId(videoIdStr);
            var video = new Video(videoId, "Test Title", "Test Author", DateTimeOffset.Now, "Desc", new Engagement(0,0,0), TimeSpan.Zero, new List<Thumbnail>(), new List<string>(), new ClosedCaptionManifest(new List<ClosedCaptionTrackInfo>()));
            var streamManifest = new StreamManifest(new List<IStreamInfo>());

            _mockVideoService.Setup(s => s.GetVideoMetadataAsync(It.IsAny<VideoId>())).ReturnsAsync(video);
            _mockVideoService.Setup(s => s.GetStreamManifestAsync(It.IsAny<VideoId>())).ReturnsAsync(streamManifest);

            // Act
            await _listFormatsCommand.ExecuteAsync(videoIdStr, null, true);

            // Assert
            _mockVideoService.Verify(s => s.GetVideoMetadataAsync(It.Is<VideoId>(v => v.Value == videoIdStr)), Times.Once);
            _mockVideoService.Verify(s => s.GetStreamManifestAsync(It.Is<VideoId>(v => v.Value == videoIdStr)), Times.Once);
            _mockCliService.Verify(c => c.DisplayStreams(streamManifest, video), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_InvalidVideoIdFormat_DisplaysError()
        {
            // Arrange
            var invalidVideoIdStr = "!!!invalid-id!!!";

            // Act
            await _listFormatsCommand.ExecuteAsync(invalidVideoIdStr, null, true);

            // Assert
            _mockCliService.Verify(c => c.DisplayError(It.Is<string>(s => s.Contains("Invalid Video URL or ID format"))), Times.Once);
            _mockVideoService.Verify(s => s.GetVideoMetadataAsync(It.IsAny<VideoId>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_VideoServiceThrowsVideoDownloadException_DisplaysError()
        {
            // Arrange
            var videoIdStr = "validVideoId";
            var exceptionMessage = "Service failed";
            _mockVideoService.Setup(s => s.GetVideoMetadataAsync(It.IsAny<VideoId>()))
                             .ThrowsAsync(new VideoDownloadException(exceptionMessage));

            // Act
            await _listFormatsCommand.ExecuteAsync(videoIdStr, null, true);

            // Assert
            _mockCliService.Verify(c => c.DisplayError(exceptionMessage), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_VideoServiceReturnsNull_DisplaysError()
        {
            // Arrange
            var videoIdStr = "validVideoId";
            _mockVideoService.Setup(s => s.GetVideoMetadataAsync(It.IsAny<VideoId>())).ReturnsAsync((Video)null);
            _mockVideoService.Setup(s => s.GetStreamManifestAsync(It.IsAny<VideoId>())).ReturnsAsync((StreamManifest)null);

            // Act
            await _listFormatsCommand.ExecuteAsync(videoIdStr, null, true);

            // Assert
            _mockCliService.Verify(c => c.DisplayError("Could not retrieve necessary video information to list formats."), Times.Once);
        }
    }
}
