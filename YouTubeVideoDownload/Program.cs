using System;
using System.Linq;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace YouTubeVideoDownload
{
    class Program
    {
        static int _tableWidth = 70;
        static bool _IsVideoIdValid = false;
        static bool _IsValidVideoIdSelected = false;
        static bool _IsValidDirectorySelected = false;

        static void Main(string[] args)
        {
            // display texto de introdução
            DisplayIntro();

            VideoId videoId;
        }

        #region Display Information
        /// <summary>
        /// Exibe informações do autor e links 
        /// </summary>
        static void DisplayIntro()
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
        static void DisplayStreamInformation(StreamManifest streamManifest, Video video)
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

        #endregion

        #region Table UI
        /// <summary>
        /// Draws a single table row 
        /// </summary>
        /// <param name="columns">columns to be drawn</param>
        static void PrintRow(params string[] columns)
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
        static string AlignCentre(string text, int width)
        {
            text = text.Length > width ? text.Substring(0, width - 3) + "..." : text;

            if (string.IsNullOrEmpty(text)) return new string(' ', width);
            else return text.PadRight(width - (width - text.Length) / 2).PadLeft(width);
        }

        /// <summary>
        /// Prints a divider in the table
        /// </summary>
        /// <param name="drawCharacter">character representing the divider</param>
        static void PrintDivider(char drawCharacter = '-') => Console.WriteLine(new string(drawCharacter, _tableWidth));
        #endregion
    }
}
