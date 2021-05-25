using System;

namespace YouTubeVideoDownload
{
    class Program
    {
        static void Main(string[] args)
        {
            // display texto de introdução
            DisplayIntro();
        }

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
    }
}
