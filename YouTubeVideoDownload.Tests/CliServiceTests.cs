using Xunit;
using Moq;
using YouTubeVideoDownload.Services;
using System.IO;
using System;
using YoutubeExplode.Videos; // For dummy Video object
using YoutubeExplode.Videos.Streams; // For dummy StreamManifest object
using System.Collections.Generic; // For List
using YoutubeExplode.Common; // For Author
using YoutubeExplode.Videos.ClosedCaptions; // For ClosedCaptionManifest

namespace YouTubeVideoDownload.Tests
{
    public class CliServiceTests : IDisposable
    {
        private readonly StringWriter _stringWriter;
        private readonly StringReader _stringReaderDefault;
        private readonly TextWriter _originalOutput;
        private readonly TextReader _originalInput;

        private const string TestDirectory = "cliservice_test_dir";

        public CliServiceTests()
        {
            _originalOutput = Console.Out;
            _stringWriter = new StringWriter();
            Console.SetOut(_stringWriter);

            _originalInput = Console.In;
            _stringReaderDefault = new StringReader(string.Empty); // Default empty reader
        }

        private void SetupConsoleInput(string input)
        {
            var stringReader = new StringReader(input);
            Console.SetIn(stringReader);
        }

        [Fact]
        public void EnsureDirectoryExists_CreatesDirectory_WhenItDoesNotExist()
        {
            // Arrange
            var cliService = new CliService();
            var testFilePath = Path.Combine(TestDirectory, "testfile.txt");
            if (Directory.Exists(TestDirectory))
            {
                Directory.Delete(TestDirectory, true);
            }

            // Act
            bool result = cliService.EnsureDirectoryExists(testFilePath);

            // Assert
            Assert.True(result);
            Assert.True(Directory.Exists(TestDirectory));
        }

        [Fact]
        public void EnsureDirectoryExists_DoesNotCreateDirectory_WhenItAlreadyExists()
        {
            // Arrange
            var cliService = new CliService();
            var testFilePath = Path.Combine(TestDirectory, "testfile.txt");
            Directory.CreateDirectory(TestDirectory); // Ensure it exists

            var mockConsole = new StringWriter();
            Console.SetOut(mockConsole); // Capture output to check if "Creating directory" is printed

            // Act
            bool result = cliService.EnsureDirectoryExists(testFilePath);

            // Assert
            Assert.True(result);
            Assert.DoesNotContain("Creating directory", mockConsole.ToString());
        }

        [Fact]
        public void EnsureDirectoryExists_HandlesNullFilePathDirectory()
        {
            // Arrange
            var cliService = new CliService();
            var testFilePath = "testfile_in_current_dir.txt"; // No directory part

            // Act
            bool result = cliService.EnsureDirectoryExists(testFilePath);

            // Assert
            Assert.True(result); // Should return true as current directory exists
        }


        [Theory]
        [InlineData("1\n", 1)] // Simulate user typing "1" and pressing Enter
        [InlineData("3\n", 3)]
        public void PromptForStreamSelection_ReturnsValidSelection(string input, int expectedSelection)
        {
            // Arrange
            var cliService = new CliService();
            SetupConsoleInput(input);
            int streamCount = 5;

            // Act
            int selection = cliService.PromptForStreamSelection(streamCount);

            // Assert
            Assert.Equal(expectedSelection, selection);
        }

        [Fact]
        public void PromptForStreamSelection_Reprompts_OnInvalidTextThenValidNumber()
        {
            // Arrange
            var cliService = new CliService();
            SetupConsoleInput("invalid\n2\n"); // Invalid text, then valid number "2"
            int streamCount = 3;

            // Act
            int selection = cliService.PromptForStreamSelection(streamCount);
            _stringWriter.Flush(); // Ensure all output is written
            var output = _stringWriter.ToString();

            // Assert
            Assert.Equal(2, selection);
            Assert.Contains("Invalid input. Please enter a number.", output);
        }

        [Fact]
        public void PromptForStreamSelection_Reprompts_OnOutOfRangeThenValidNumber()
        {
            // Arrange
            var cliService = new CliService();
            SetupConsoleInput("0\n5\n3\n"); // "0" (too low), "5" (too high for count=3), then "3" (valid)
            int streamCount = 3;

            // Act
            int selection = cliService.PromptForStreamSelection(streamCount);
            _stringWriter.Flush();
            var output = _stringWriter.ToString();


            // Assert
            Assert.Equal(3, selection);
            Assert.Contains("Invalid selection. Please enter a number between 1 and 3.", output);
        }

        [Fact]
        public void DisplayError_PrintsErrorMessageInRed() // Basic check
        {
            // Arrange
            var cliService = new CliService();
            var errorMessage = "Test error message";
            // Note: Testing Console.ForegroundColor is tricky and often skipped in unit tests
            // as it involves platform-specific APIs or more complex console mocking.
            // We'll just check if the message is part of the output.

            // Act
            cliService.DisplayError(errorMessage);
            var output = _stringWriter.ToString();

            // Assert
            Assert.Contains($"Error: {errorMessage}", output);
        }

        [Fact]
        public void DisplayStreams_PrintsVideoDetailsAndStreams()
        {
            // Arrange
            var cliService = new CliService();
            var videoId = new VideoId("testId");
            var video = new Video(videoId, "Test Title", "Test Author", DateTimeOffset.Now, "Test Description",
                                  new Engagement(0,0,0), TimeSpan.FromSeconds(60), new IReadOnlyList<Thumbnail>(),
                                  new IReadOnlyList<string>(), new ClosedCaptionManifest(new IReadOnlyList<ClosedCaptionTrackInfo>()));

            var streamInfos = new List<MuxedStreamInfo>
            {
                new MuxedStreamInfo("testUrl1", Container.Mp4, new FileSize(1024*1024*10), Bitrate.FromBitsPerSecond(1000000), "avc1.64001f", "mp4a.40.2", new VideoQuality(1920, 1080, 30), new VideoResolution(1920,1080)),
                new MuxedStreamInfo("testUrl2", Container.WebM, new FileSize(1024*1024*20), Bitrate.FromBitsPerSecond(2000000), "vp9", "opus", new VideoQuality(1280, 720, 30), new VideoResolution(1280,720))
            };
            var streamManifest = new StreamManifest(streamInfos);

            // Act
            cliService.DisplayStreams(streamManifest, video);
            var output = _stringWriter.ToString();

            // Assert
            Assert.Contains("Video: Test Title", output);
            Assert.Contains("Author: Test Author", output);
            Assert.Contains("1080p30", output); // From VideoQuality.Label
            Assert.Contains("720p30", output);  // From VideoQuality.Label
            Assert.Contains("mp4", output);
            Assert.Contains("webm", output);
        }


        public void Dispose()
        {
            Console.SetOut(_originalOutput);
            Console.SetIn(_originalInput);
            _stringWriter.Dispose();
            _stringReaderDefault.Dispose(); // Dispose the default one too
            if (Directory.Exists(TestDirectory))
            {
                Directory.Delete(TestDirectory, true);
            }
        }
    }
}
