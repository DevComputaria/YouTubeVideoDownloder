using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;
using YouTubeVideoDownload.Extensions;
using YouTubeVideoDownload.Utilities;

namespace YouTubeVideoDownload
{

    public static class Program
    {
        private static bool _IsValidDirectorySelected = false;
        private static bool _IsValidVideoIdSelected = false;
        private static bool _IsVideoIdValid = false;
        private static int _tableWidth = 70;

        public static async Task Main(string[] args)
        {
            // display texto de introdução
            DisplayIntro();

            VideoId videoId;

            //get video id or url from user
            GetVideoInformationFromInput(ref videoId);

            //prepare youtube client
            var _youtubeClient = new YoutubeClient();

            Console.WriteLine("Fetching streams...\n");

            //get the manifest streams for the video
            var streamManifest = await _youtubeClient.Videos.Streams.GetManifestAsync(videoId);

            if (streamManifest == null)
                throw new Exception(videoId);

            //get the video
            var video = await _youtubeClient.Videos.GetAsync(videoId);

            //display the stream information in a table
            DisplayStreamInformation(streamManifest, video);

            int selectedStreamOption = 0;

            //get the stream number to be downloaded from user input
            GetDownloadStreamNumberFromInput(ref selectedStreamOption, streamManifest?.Streams?.Count ?? 0);

            //find selected stream information
            var selectedStreamInfomation = streamManifest.GetMuxedStreams().ToArray()[selectedStreamOption - 1];

            //prepare filename
            string fileName = $"{video.Title}.{selectedStreamInfomation.Container.Name}";
            if (!ValidateFileName(video.Title)) fileName = $"{video.Title.GenerateSlug()}.{selectedStreamInfomation.Container.Name}";

            //print selected stream information
            PrintRow("#", "Size", "Format", "Bit Rate");
            PrintRow(selectedStreamOption.ToString(), selectedStreamInfomation.Size.ToString(), selectedStreamInfomation.Container.Name, selectedStreamInfomation.Bitrate.ToString());

            var saveDirectoryPath = string.Empty;

            //get the file save directory from user input
            GetSaveDirectoryFromInput(ref saveDirectoryPath, fileName);

            Console.WriteLine("Downloading File...");

            //download the file while showing the progress bar
            using var progress = new ProgressIndicator();
            await _youtubeClient.Videos.Streams.DownloadAsync(selectedStreamInfomation, saveDirectoryPath, progress);
        }

        #region Display Information

        /// <summary>
        /// Exibe informações do autor e links
        /// </summary>
        private static void DisplayIntro()
        {
            Console.Title = "YouTube Video Downloader CLI";
            Console.WriteLine("=========================================");
            Console.WriteLine("=     Youtube Video Downloader CLI      =");
            Console.WriteLine("=             DevComputaria             =");
            Console.WriteLine("=    https://github.com/DevComputaria   =");
            Console.WriteLine("=========================================");
        }

        /// <summary>
        /// Display all the streams of a video id in a table fashion
        /// </summary>
        /// <param name="streamManifest"></param>
        /// <param name="video"></param>
        private static void DisplayStreamInformation(StreamManifest streamManifest, Video video)
        {
            if (streamManifest != null && video != null && streamManifest.Streams.Count > 0)
            {
                Console.WriteLine($"{video.Title} by {video.Author} on {video.UploadDate.DateTime.ToLongDateString()}");
                PrintDivider();
                PrintRow("#", "Size", "Format", "Bit Rate");
                PrintDivider();
                PrintDivider();

                var toPrint = streamManifest.GetMuxedStreams().ToArray();

                for (int i = 0; i < toPrint.Length; i++)
                {
                    var stream = toPrint[i];
                    PrintRow((i + 1).ToString(), stream.Size.ToString(), stream.Container.Name, stream.Bitrate.ToString());
                }
            }
            else
            {
                Console.WriteLine("\nNo streams found for the provided video link");
            }
        }

        #region User Inputs

        /// <summary>
        /// Asks the user to input the youtube video id or the youtube video url
        /// </summary>
        /// <param name="videoId">user input will be validated and binded to this</param>
        private static void GetVideoInformationFromInput(ref VideoId videoId)
        {
            do
            {
                try
                {
                    Console.Write("\nEnter the Video URL/ID: ");
                    _IsVideoIdValid = true;
                    videoId = NewMethod();
                }
                catch (ArgumentException ex)
                {
                    _IsVideoIdValid = false;
                    Console.WriteLine(ex.Message);
                }
            } while (!_IsVideoIdValid);
        }

        private static VideoId NewMethod()
        {
            return new VideoId();
        }

        /// <summary>
        /// Get the stream number which needs to be downloaded from the stream list of the youtube video id or youtube video url provided
        /// </summary>
        /// <param name="selectedStreamOption">a reference to the selected stream option</param>
        /// <param name="streamCount">the number of streams from the stream manifest</param>
        private static void GetDownloadStreamNumberFromInput(ref int selectedStreamOption, int streamCount = 0)
        {
            do
            {
                Console.Write("\nSelect the # you want to download: ");
                int.TryParse(Console.ReadLine(), out int selectedOption);
                selectedStreamOption = selectedOption;

                if (selectedOption > streamCount || selectedOption < 1)
                {
                    _IsValidVideoIdSelected = false;
                    Console.WriteLine("Invalid # selected. Make sure you select a valid number from the # column of the above table\n");
                }
                else
                {
                    _IsValidVideoIdSelected = true;
                }
            } while (!_IsValidVideoIdSelected);
        }

        /// <summary>
        /// Gets the save directory for the download file from the user
        /// </summary>
        /// <param name="directoryPath">a reference to the directory path</param>
        /// <param name="fileName">file name of the download file</param>
        private static void GetSaveDirectoryFromInput(ref string directoryPath, string fileName)
        {
            do
            {
                Console.Write("\nDirectory to save the downloaded file (leave blank if current directory): ");
                directoryPath = Console.ReadLine();

                var directoryValidationResult = ValidateSaveDirectory(directoryPath);

                if (directoryValidationResult.Item1)
                {
                    _IsValidDirectorySelected = true;
                    directoryPath = Path.Combine(directoryPath, fileName);
                }
                else
                {
                    _IsValidDirectorySelected = false;
                    Console.WriteLine(directoryValidationResult.Item2);
                }
            } while (!_IsValidDirectorySelected);
        }

        #endregion User Inputs

        #region Validations

        /// <summary>
        /// Validates a given save directory path
        /// </summary>
        /// <param name="directory">name of the directory path which needs to be validated</param>
        /// <returns>returns a tuple countaining information about whether the directory is valid or not and error messages</returns>
        private static Tuple<bool, string> ValidateSaveDirectory(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory)) return Tuple.Create(true, string.Empty);

            if (!Directory.Exists(directory))
            {
                try
                {
                    Directory.CreateDirectory(directory);
                }
                catch (Exception ex)
                {
                    return Tuple.Create(false, ex.Message);
                }
            }
            return Tuple.Create(true, string.Empty);
        }

        /// <summary>
        /// Validates a given file name. Currently validated against windows file system naming conventions
        /// </summary>
        /// <param name="name">name of the file which is going to be validated</param>
        /// <returns>returns if the validation succeeded or not</returns>
        private static bool ValidateFileName(string name)
        {
            Regex containsABadCharacter = new Regex("[" + Regex.Escape(string.Join("", Path.GetInvalidFileNameChars())) + "]");
            if (containsABadCharacter.IsMatch(name)) return false;
            return true;
        }

        #endregion Validations

        #endregion Display Information

        #region Table UI

        /// <summary>
        /// Draws a single table row
        /// </summary>
        /// <param name="columns">columns to be drawn</param>
        private static void PrintRow(params string[] columns)
        {
            int width = (_tableWidth - columns.Length) / columns.Length;
            string row = "|";

            foreach (string column in columns)
            {
                row += AlignCentre(column, width) + "|";
            }

            Console.WriteLine(row);
        }

        /// <summary>
        /// Aligning text in a column
        /// </summary>
        /// <param name="text">text of the colum</param>
        /// <param name="width">width of the column</param>
        /// <returns></returns>
        private static string AlignCentre(string text, int width)
        {
            text = text.Length > width ? text.Substring(0, width - 3) + "..." : text;

            if (string.IsNullOrEmpty(text)) return new string(' ', width);
            else return text.PadRight(width - (width - text.Length) / 2).PadLeft(width);
        }

        /// <summary>
        /// Prints a divider in the table
        /// </summary>
        /// <param name="drawCharacter">character representing the divider</param>
        private static void PrintDivider(char drawCharacter = '-') => Console.WriteLine(new string(drawCharacter, _tableWidth));
        
        #endregion Table UI
    }
}