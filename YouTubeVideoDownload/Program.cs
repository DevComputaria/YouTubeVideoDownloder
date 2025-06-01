using System;
using System.CommandLine;
// System.CommandLine.Binding is implicitly used by SetHandler's model binding.
// using System.CommandLine.Binding;
// System.IO is not directly used here anymore.
// using System.IO;
using System.Threading.Tasks;
using YouTubeVideoDownload.Commands;
using YouTubeVideoDownload.Services;
using YouTubeVideoDownload.Exceptions;

namespace YouTubeVideoDownload
{
    /// <summary>
    /// Main entry point for the YouTube Video Downloader application.
    /// Parses command-line arguments and dispatches to appropriate commands.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point of the application.
        /// Configures command-line arguments, sets up command handlers,
        /// and executes the appropriate command based on user input.
        /// Includes global error handling.
        /// </summary>
        /// <param name="args">Command-line arguments passed to the application.</param>
        /// <returns>
        /// Returns 0 on successful execution.
        /// Returns 1 if an error occurs and is caught by the global exception handlers.
        /// Other non-zero codes might be returned by System.CommandLine for parsing errors.
        /// </returns>
        public static async Task<int> Main(string[] args)
        {
            // Define command-line arguments and options
            var videoUrlArgument = new Argument<string>("video-url", "The URL or ID of the YouTube video.");
            // System.CommandLine will make this argument required by default unless configured otherwise.

            var outputOption = new Option<string>(
                aliases: new[] { "-o", "--output" },
                description: "The output directory path for the downloaded video. Required if downloading.");

            var listFormatsOption = new Option<bool>(
                aliases: new[] { "-l", "--list-formats" },
                description: "Lists available audio/video formats for the video instead of downloading.");

            var rootCommand = new RootCommand("A .NET CLI application to download YouTube videos or list their available formats.")
            {
                videoUrlArgument,
                outputOption,
                listFormatsOption
            };

            // Global CLI service instance for displaying errors from the Main method's catch blocks
            var cliServiceGlobal = new CliService();

            try
            {
                // Set the handler for the root command
                rootCommand.SetHandler(async (videoUrlValue, outputValue, listFormatsValue) =>
                {
                    // Instantiate services here to be fresh for each command execution,
                    // or manage them as singletons if appropriate (not strictly necessary for this app's scale).
                    var videoService = new VideoService();
                    var cliService = new CliService();

                    // System.CommandLine automatically makes arguments required.
                    // Explicit check for null/whitespace is a fallback or for non-argument<T> types.
                    // For `Argument<string>` like videoUrlArgument, if it's not provided, System.CommandLine's help/error runs first.
                    // This validation is thus somewhat redundant if System.CommandLine handles it.
                    if (string.IsNullOrWhiteSpace(videoUrlValue))
                    {
                        cliService.DisplayError("Video URL/ID cannot be empty. This should be caught by argument parsing.");
                        // To influence exit code from here, an exception could be thrown, or System.CommandLine configured.
                        // For now, returning if this somehow gets hit.
                        return;
                    }

                    ICommand command;
                    if (listFormatsValue)
                    {
                        command = new ListFormatsCommand(videoService, cliService);
                    }
                    else
                    {
                        // The DownloadVideoCommand itself will check if outputValue is provided.
                        command = new DownloadVideoCommand(videoService, cliService);
                    }

                    // Execute the selected command
                    await command.ExecuteAsync(videoUrlValue, outputValue, listFormatsValue);
                },
                videoUrlArgument, // These are bound to the handler's parameters by System.CommandLine
                outputOption,
                listFormatsOption);

                // Invoke System.CommandLine parsing and execution
                return await rootCommand.InvokeAsync(args);
            }
            catch (VideoDownloadException ex) // Catch custom application-specific exceptions
            {
                // Logging placeholder for ex.ToString() or structured logging
                Console.WriteLine($"Debug (Global Handler - VideoDownloadException): {ex.ToString()}");
                cliServiceGlobal.DisplayError($"A video processing error occurred: {ex.Message}");
                return 1; // Standard error code
            }
            catch (Exception ex) // Catch-all for any other unexpected unhandled exceptions
            {
                // Logging placeholder
                Console.WriteLine($"Debug (Global Handler - GenericException): {ex.ToString()}");
                cliServiceGlobal.DisplayError("An unexpected application error occurred. Please check logs or run with more verbose output if available.");
                return 1; // Standard error code
            }
        }
    }
}
