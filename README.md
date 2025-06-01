# YouTube Video Downloader CLI

A .NET CLI application to download YouTube videos or list their available formats. This tool allows users to fetch video information, display available download options, and download videos directly from the command line.

## Features

*   **Download YouTube Videos:** Interactively select from available video and audio (muxed) streams to download.
*   **List Available Formats:** Display a list of available muxed streams for a given video, including details like quality, size, format, and bitrate.
*   **Command-Line Interface:** Easy-to-use CLI for specifying video URLs/IDs and options.
    *   Required argument for video URL or ID.
    *   Option to specify an output path for downloads (`-o`, `--output`).
    *   Option to list available formats without downloading (`-l`, `--list-formats`).
*   **Progress Indicator:** Shows download progress in the console.
*   **Error Handling:** Provides user-friendly error messages for common issues (e.g., video unavailable, network errors, invalid input).
*   **Filename Sanitization:** Downloaded video titles are sanitized to create filesystem-friendly names.

## Prerequisites

*   [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0) or later.

## Installation & Setup

1.  **Clone the repository:**
    ```bash
    git clone <repository_url>
    cd YouTubeVideoDownload
    ```
    *(Replace `<repository_url>` with the actual URL of this repository)*

2.  **Build the project:**
    ```bash
    dotnet build
    ```
    (This will build the `YouTubeVideoDownload` project. If you encounter build issues, ensure you are in the `/app` directory or specify the project path if running from the solution root: `dotnet build YouTubeVideoDownload/YouTubeVideoDownload.csproj`)

## Usage

To run the application, use the `dotnet run` command from the solution's root directory (`/app`) or directly from the project directory, followed by the required arguments and options.

**General Syntax:**

```bash
dotnet run --project YouTubeVideoDownload -- <video-url-or-id> [options]
```

### Commands and Options

*   **`video-url-or-id` (Required):** The full URL of the YouTube video or just the Video ID.
*   **`-l, --list-formats` (Optional):** Lists available video formats (muxed streams) without downloading.
*   **`-o, --output <PATH>` (Optional):** Specifies the directory path where the video should be downloaded. If not provided when downloading, the command will prompt or an error will be shown as it's required for download.

### Examples

1.  **List available formats for a video:**
    ```bash
    dotnet run --project YouTubeVideoDownload -- "https://www.youtube.com/watch?v=your_video_id" -l
    ```
    Or using just the Video ID:
    ```bash
    dotnet run --project YouTubeVideoDownload -- "your_video_id" --list-formats
    ```

2.  **Download a video (interactive selection):**
    The application will list available muxed streams, and you'll be prompted to enter the number corresponding to the stream you wish to download. The output path is required.
    ```bash
    dotnet run --project YouTubeVideoDownload -- "your_video_id" -o "path/to/your/downloads"
    ```
    If you run without `-o`:
    ```bash
    dotnet run --project YouTubeVideoDownload -- "your_video_id"
    ```
    The application will show an error because the output path is required for download.

3.  **Download a video to a specific directory:**
    ```bash
    dotnet run --project YouTubeVideoDownload -- "https://www.youtube.com/watch?v=your_video_id" -o "/users/username/videos/youtube"
    ```
    *(The application will create the directory if it doesn't exist.)*

## Project Structure

The project is organized into several key directories:

*   **`Commands/`**: Contains command classes that encapsulate specific actions (e.g., `DownloadVideoCommand`, `ListFormatsCommand`). Implements the Command pattern.
*   **`Services/`**: Holds service classes that provide core functionalities.
    *   `VideoService.cs`: Handles interactions with the YouTube library (YoutubeExplode) for fetching video data and downloading.
    *   `CliService.cs`: Manages command-line interface interactions like displaying information and prompting for user input.
    *   Interfaces (`IVideoService.cs`, `ICliService.cs`) for dependency injection and testability.
*   **`Extensions/`**: Contains extension methods (e.g., `StringExtensions.cs` for filename sanitization).
*   **`Utilities/`**: Includes utility classes like `ProgressIndicator.cs`.
*   **`Exceptions/`**: Defines custom exception classes (e.g., `VideoDownloadException.cs`).
*   **`Program.cs`**: The main entry point of the application, responsible for CLI argument parsing and command dispatching using `System.CommandLine`.

## Contributing

Contributions are welcome! If you have suggestions for improvements or want to report a bug, please feel free to open an issue or submit a pull request.

When contributing, please ensure:
1.  Your code adheres to the existing coding style.
2.  You add relevant XML documentation comments for new public members.
3.  You add unit tests for new features or bug fixes (though the test runner in the current dev environment has shown some issues).

## License

This project is licensed under the MIT License. (Assuming MIT, please update if incorrect).
