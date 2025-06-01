using System;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;
using System.Linq;
using System.IO;

namespace YouTubeVideoDownload.Services
{
    /// <summary>
    /// Implements <see cref="ICliService"/> to provide command-line interface functionalities,
    /// such as displaying data to the console, prompting for user input, and managing console output formatting.
    /// </summary>
    public class CliService : ICliService
    {
        private static int _tableWidth = 70; // Default width for table formatting

        /// <summary>
        /// Displays video metadata and available muxed streams in a formatted table to the console.
        /// </summary>
        /// <param name="manifest">The <see cref="StreamManifest"/> containing stream information.</param>
        /// <param name="video">The <see cref="Video"/> metadata object.</param>
        public void DisplayStreams(StreamManifest manifest, Video video)
        {
            try
            {
                if (manifest != null && video != null && manifest.Streams.Any())
                {
                    Console.WriteLine($"\nVideo: {video.Title}");
                    Console.WriteLine($"Author: {video.Author}");
                    Console.WriteLine($"Duration: {video.Duration}");
                    Console.WriteLine($"Uploaded: {video.UploadDate.DateTime.ToShortDateString()}");
                    PrintDivider();
                    Console.WriteLine("Available Muxed Streams (Video + Audio):");
                    PrintRow("#", "Quality", "Size (MB)", "Format", "Bitrate (kbps)"); // Clarified units
                    PrintDivider();

                    var muxedStreams = manifest.GetMuxedStreams().OrderByDescending(s => s.VideoQuality).ToArray();

                    if (!muxedStreams.Any())
                    {
                        Console.WriteLine("No muxed (video+audio) streams found for this video.");
                        return;
                    }

                    for (int i = 0; i < muxedStreams.Length; i++)
                    {
                        var stream = muxedStreams[i];
                        PrintRow(
                            (i + 1).ToString(),
                            stream.VideoQuality.Label,
                            $"{stream.Size.MegaBytes:F2}", // Format size to MB with 2 decimal places
                            stream.Container.Name,
                            $"{stream.Bitrate.KiloBitsPerSecond:F0}" // Format bitrate to kbps, no decimals
                        );
                    }
                    PrintDivider();
                }
                else
                {
                    Console.WriteLine("\nNo streams found or video metadata is missing.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Debug: An unexpected error occurred in DisplayStreams: {ex.Message}"); // Logging placeholder
                DisplayError("An unexpected error occurred while trying to display video stream information.");
            }
        }

        /// <summary>
        /// Prompts the user to enter a selection from a numbered list of streams.
        /// Continues to prompt until a valid number within the specified range is entered.
        /// </summary>
        /// <param name="streamCount">The total number of available streams.</param>
        /// <returns>The 1-based integer representing the user's valid selection.</returns>
        public int PromptForStreamSelection(int streamCount)
        {
            int selectedOption = 0;
            bool isValidSelection = false;
            do
            {
                Console.Write($"\nEnter the # of the stream to download (1-{streamCount}): ");
                string input = Console.ReadLine();

                if (int.TryParse(input, out selectedOption))
                {
                    if (selectedOption >= 1 && selectedOption <= streamCount)
                    {
                        isValidSelection = true;
                    }
                    else
                    {
                        DisplayError($"Invalid selection. Please enter a number between 1 and {streamCount}.");
                    }
                }
                else
                {
                    DisplayError("Invalid input. Please enter a number.");
                }
            } while (!isValidSelection);
            return selectedOption;
        }

        /// <summary>
        /// Ensures that the directory for the specified file path exists.
        /// If the directory does not exist, it attempts to create it.
        /// </summary>
        /// <param name="filePath">The full file path for which the parent directory's existence is to be ensured.</param>
        /// <returns><c>true</c> if the directory exists or is successfully created; otherwise, <c>false</c>.</returns>
        public bool EnsureDirectoryExists(string filePath)
        {
            try
            {
                var directoryPath = Path.GetDirectoryName(filePath);
                if (string.IsNullOrEmpty(directoryPath))
                {
                    // If directoryPath is null or empty, it implies the file is in the current directory or path is invalid.
                    // Assuming current directory always "exists" for this check.
                    return true;
                }

                if (!Directory.Exists(directoryPath))
                {
                    DisplayMessage($"Creating directory: {directoryPath}");
                    Directory.CreateDirectory(directoryPath);
                }
                return true;
            }
            catch (IOException ex)
            {
                DisplayError($"Could not create directory '{Path.GetDirectoryName(filePath)}'. IO Error: {ex.Message}");
                return false;
            }
            catch (Exception ex) // Handles other exceptions like UnauthorizedAccessException, ArgumentException for invalid chars in path
            {
                DisplayError($"Could not create directory '{Path.GetDirectoryName(filePath)}'. Error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Displays an error message to the console, typically formatted in red.
        /// </summary>
        /// <param name="message">The error message to display.</param>
        public void DisplayError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nError: {message}");
            Console.ResetColor();
        }

        /// <summary>
        /// Displays a standard informational message to the console.
        /// </summary>
        /// <param name="message">The message to display.</param>
        public void DisplayMessage(string message)
        {
            Console.WriteLine(message);
        }

        /// <summary>
        /// Prints a single row of data in a table format to the console.
        /// Columns are automatically aligned.
        /// </summary>
        /// <param name="columns">An array of strings, where each string is a column in the row.</param>
        private static void PrintRow(params string[] columns)
        {
            if (columns == null || !columns.Any()) return;

            // Calculate width per column, ensure it's at least a minimum to avoid issues.
            int width = (_tableWidth - columns.Length) / columns.Length;
            if (width <= 0) width = 10;

            string row = "|";
            foreach (string column in columns)
            {
                row += AlignCentre(column ?? string.Empty, width) + "|";
            }
            Console.WriteLine(row);
        }

        /// <summary>
        /// Aligns text within a given width, primarily for table cell formatting.
        /// Text is truncated with "..." if it exceeds the width.
        /// </summary>
        /// <param name="text">The text to align.</param>
        /// <param name="width">The total width for the text.</param>
        /// <returns>The aligned and potentially truncated string.</returns>
        private static string AlignCentre(string text, int width)
        {
            text = text ?? string.Empty;
            if (text.Length > width)
            {
                text = text.Substring(0, Math.Max(0, width - 3)) + "...";
            }

            if (string.IsNullOrEmpty(text)) return new string(' ', width);

            int spaces = width - text.Length;
            int padLeft = spaces / 2 + text.Length;
            return text.PadLeft(padLeft).PadRight(width);
        }

        /// <summary>
        /// Prints a horizontal divider line to the console, typically used in table formatting.
        /// </summary>
        /// <param name="drawCharacter">The character to use for drawing the divider line. Defaults to '-'.</param>
        private static void PrintDivider(char drawCharacter = '-') => Console.WriteLine(new string(drawCharacter, _tableWidth));
    }
}
