using Xunit;
using Moq;
using YouTubeVideoDownload.Commands;
using YouTubeVideoDownload.Services;
using YouTubeVideoDownload.Exceptions;
using YouTubeVideoDownload.Utilities; // For ProgressIndicator, though not directly tested via command
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Common; // For Author, Thumbnail
using YoutubeExplode.Videos.ClosedCaptions; // For ClosedCaptionManifest
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic; // For List
using System.Linq;


namespace YouTubeVideoDownload.Tests
{
    public class DownloadVideoCommandTests
    {
        private readonly Mock<IVideoService> _mockVideoService;
        private readonly Mock<ICliService> _mockCliService;
        private readonly DownloadVideoCommand _downloadCommand;

        // Dummy video and stream manifest for setup
        private readonly Video _dummyVideo;
        private readonly StreamManifest _dummyStreamManifest;
        private readonly MuxedStreamInfo _dummyMuxedStreamInfo;

        public DownloadVideoCommandTests()
        {
            _mockVideoService = new Mock<IVideoService>();
            _mockCliService = new Mock<ICliService>();
            _downloadCommand = new DownloadVideoCommand(_mockVideoService.Object, _mockCliService.Object);

            var videoId = new VideoId("dummyId");
            _dummyVideo = new Video(videoId, "Dummy Title", "Dummy Author", DateTimeOffset.Now, "Desc",
                                  new Engagement(0,0,0), TimeSpan.FromSeconds(60),
                                  new List<Thumbnail>(), new List<string>(),
                                  new ClosedCaptionManifest(new List<ClosedCaptionTrackInfo>()));

            _dummyMuxedStreamInfo = new MuxedStreamInfo("http://dummyurl.com/video.mp4", Container.Mp4,
                                                      new FileSize(12345), Bitrate.FromMegaBitsPerSecond(1), "h264", "aac",
                                                      new VideoQuality(1080, "1080p", 30), new VideoResolution(1920,1080));
            var streamInfos = new List<MuxedStreamInfo> { _dummyMuxedStreamInfo };
            _dummyStreamManifest = new StreamManifest(streamInfos);
        }

        private void SetupDefaultServiceMocks(string videoIdStr = "dummyId")
        {
            _mockVideoService.Setup(s => s.GetVideoMetadataAsync(It.Is<VideoId>(v => v.Value == videoIdStr)))
                             .ReturnsAsync(_dummyVideo);
            _mockVideoService.Setup(s => s.GetStreamManifestAsync(It.Is<VideoId>(v => v.Value == videoIdStr)))
                             .ReturnsAsync(_dummyStreamManifest);
            _mockCliService.Setup(c => c.EnsureDirectoryExists(It.IsAny<string>())).Returns(true);
            _mockCliService.Setup(c => c.PromptForStreamSelection(It.IsAny<int>())).Returns(1); // Default to selecting the first stream
        }

        [Fact]
        public async Task ExecuteAsync_ListFormatsIsTrue_ListsFormatsAndExits()
        {
            // Arrange
            SetupDefaultServiceMocks();

            // Act
            await _downloadCommand.ExecuteAsync("dummyId", "/output/path", true);

            // Assert
            _mockCliService.Verify(c => c.DisplayStreams(_dummyStreamManifest, _dummyVideo), Times.Once);
            _mockCliService.Verify(c => c.DisplayMessage(It.Is<string>(s => s.Contains("List formats requested"))), Times.Once);
            _mockVideoService.Verify(s => s.DownloadVideoStreamAsync(It.IsAny<IVideoStreamInfo>(), It.IsAny<string>(), It.IsAny<IProgress<double>>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_NoOutputPath_DisplaysError()
        {
            // Arrange
            SetupDefaultServiceMocks();

            // Act
            await _downloadCommand.ExecuteAsync("dummyId", " ", false); // Empty/whitespace output path

            // Assert
            _mockCliService.Verify(c => c.DisplayError(It.Is<string>(s => s.Contains("Output path is required"))), Times.Once);
            _mockVideoService.Verify(s => s.DownloadVideoStreamAsync(It.IsAny<IVideoStreamInfo>(), It.IsAny<string>(), It.IsAny<IProgress<double>>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_ValidDownloadPath_PerformsDownload()
        {
            // Arrange
            var videoIdStr = "dummyId";
            var outputPath = "/output/path";
            SetupDefaultServiceMocks(videoIdStr);
            _mockVideoService.Setup(s => s.DownloadVideoStreamAsync(It.IsAny<IVideoStreamInfo>(), It.IsAny<string>(), It.IsAny<IProgress<double>>()))
                             .Returns(Task.CompletedTask);

            // Act
            await _downloadCommand.ExecuteAsync(videoIdStr, outputPath, false);

            // Assert
            _mockVideoService.Verify(s => s.GetVideoMetadataAsync(It.Is<VideoId>(v => v.Value == videoIdStr)), Times.Once);
            _mockVideoService.Verify(s => s.GetStreamManifestAsync(It.Is<VideoId>(v => v.Value == videoIdStr)), Times.Once);
            _mockCliService.Verify(c => c.DisplayStreams(_dummyStreamManifest, _dummyVideo), Times.Once); // Streams are displayed before selection
            _mockCliService.Verify(c => c.PromptForStreamSelection(1), Times.Once); // Assuming 1 muxed stream from setup
            _mockCliService.Verify(c => c.EnsureDirectoryExists(It.Is<string>(s => s.StartsWith(outputPath))), Times.Once);

            string expectedFilename = $"{_dummyVideo.Title.GenerateSlug()}.{_dummyMuxedStreamInfo.Container.Name}";
            string expectedFullFilePath = Path.Combine(outputPath, expectedFilename);
            _mockVideoService.Verify(s => s.DownloadVideoStreamAsync(_dummyMuxedStreamInfo, expectedFullFilePath, It.IsAny<IProgress<double>>()), Times.Once);
            _mockCliService.Verify(c => c.DisplayMessage(It.Is<string>(s => s.Contains("Download of 'Dummy Title' complete!"))), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_VideoServiceThrowsException_DisplaysError()
        {
            // Arrange
            var videoIdStr = "errorId";
            var outputPath = "/output/path";
            var exceptionMessage = "Test Video Service Exception";
            _mockVideoService.Setup(s => s.GetVideoMetadataAsync(It.Is<VideoId>(v => v.Value == videoIdStr)))
                             .ThrowsAsync(new VideoDownloadException(exceptionMessage));
            // No need to setup other mocks as it should fail early

            // Act
            await _downloadCommand.ExecuteAsync(videoIdStr, outputPath, false);

            // Assert
            _mockCliService.Verify(c => c.DisplayError(exceptionMessage), Times.Once);
            _mockVideoService.Verify(s => s.DownloadVideoStreamAsync(It.IsAny<IVideoStreamInfo>(), It.IsAny<string>(), It.IsAny<IProgress<double>>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_NoMuxedStreams_DisplaysError()
        {
            // Arrange
            var videoIdStr = "noMuxedId";
            var outputPath = "/output/path";
            var emptyStreamManifest = new StreamManifest(new List<IVideoStreamInfo>()); // No streams

            _mockVideoService.Setup(s => s.GetVideoMetadataAsync(It.Is<VideoId>(v => v.Value == videoIdStr)))
                             .ReturnsAsync(_dummyVideo); // Return a dummy video
            _mockVideoService.Setup(s => s.GetStreamManifestAsync(It.Is<VideoId>(v => v.Value == videoIdStr)))
                             .ReturnsAsync(emptyStreamManifest); // Return manifest with no streams

            // Act
            await _downloadCommand.ExecuteAsync(videoIdStr, outputPath, false);

            // Assert
            _mockCliService.Verify(c => c.DisplayError(It.Is<string>(s => s.Contains("No suitable (muxed video+audio) streams found"))), Times.Once);
            _mockVideoService.Verify(s => s.DownloadVideoStreamAsync(It.IsAny<IVideoStreamInfo>(), It.IsAny<string>(), It.IsAny<IProgress<double>>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_EnsureDirectoryFails_DoesNotAttemptDownload()
        {
            // Arrange
            var videoIdStr = "dirFailId";
            var outputPath = "/output/path";
            SetupDefaultServiceMocks(videoIdStr);
            _mockCliService.Setup(c => c.EnsureDirectoryExists(It.IsAny<string>())).Returns(false); // Simulate directory creation failure

            // Act
            await _downloadCommand.ExecuteAsync(videoIdStr, outputPath, false);

            // Assert
            _mockCliService.Verify(c => c.EnsureDirectoryExists(It.IsAny<string>()), Times.Once);
            // Error message for this case is handled within CliService.EnsureDirectoryExists
            _mockVideoService.Verify(s => s.DownloadVideoStreamAsync(It.IsAny<IVideoStreamInfo>(), It.IsAny<string>(), It.IsAny<IProgress<double>>()), Times.Never);
        }
    }
}
