using System;
using System.Net;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Net.NetworkInformation;
using System.Threading;

namespace ConsoleApplication1
{
    class Program
    {
        static FileStream fsLog;
        static StreamWriter swLog;
        static FileStream fsImg;
        static string imgName;
        static string folder = @"C:\Users\...\Wallpapers\"; \\ path to the Wallpapers folder

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);
        const int SPI_SETDESKWALLPAPER = 20;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDWININICHANGE = 0x2;

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        static void Main(string[] args)
        {
            var handle = GetConsoleWindow();  // hides program 
            ShowWindow(handle, SW_HIDE);      // from user
            fsLog = new FileStream(folder + "log.txt", FileMode.Append);
            swLog = new StreamWriter(fsLog);
            swLog.WriteLine(GetDateTime() + "The program is running!");

            while (true)
            {
                swLog.WriteLine(GetDateTime() + "Checking Internet connection...");
                if (CheckInternetConnection())
                {
                    swLog.WriteLine(GetDateTime() + "Internet connected!");
                    break;
                }
                Console.WriteLine("Internet connection is missing!");
                Thread.Sleep(30000);
            }

            Start();

            swLog.WriteLine(GetDateTime() + "Program stopped");

            swLog.Close();
            fsLog.Close();
            return;
        }

        /// <summary>
        /// Starts the process of changing Wallpapers
        /// </summary>
        static void Start()
        {
            if (DownLoadWallpaper())
                if (Convert2bmp())
                    ChangeWallpaper();
        }

        /// <summary>
        /// Checks for Internet connection
        /// </summary>
        /// <returns>true - Internet connected, false - Internet missing</returns>
        static bool CheckInternetConnection()
        {
            IPStatus status = IPStatus.TimedOut;
            try
            {
                Ping ping = new Ping();
                PingReply reply = ping.Send(@"www.google.ru");
                status = reply.Status;
            }
            catch
            {
                return false;
            }
            if (status == IPStatus.Success)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// Set new wallpapers
        /// </summary>
        static void ChangeWallpaper()
        {
            swLog.WriteLine(GetDateTime() + "Set wallpapers...");
            try
            {
                SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, folder + @"wp_bmp\" + imgName + ".bmp", (SPIF_UPDATEINIFILE + SPIF_SENDWININICHANGE));
                swLog.WriteLine(GetDateTime() + "Wallpapers set up!");
                return;
            }
            catch (Exception ex)
            {
                swLog.WriteLine(GetDateTime() + "Error wallpapers setting up: " + ex.Message);
                Thread.Sleep(30000);
                Start();
                return;
            }
        }

        /// <summary>
        /// Converts new picture to *.bmp
        /// </summary>
        /// <returns>true - converts done, false - error</returns>
        static bool Convert2bmp()
        {
            swLog.WriteLine(GetDateTime() + "Converting to bmp...");
            try
            {
                fsImg = new FileStream(folder + imgName + ".jpg", FileMode.Open);
                Image img = Image.FromStream(fsImg);
                img.Save(folder + @"wp_bmp\" + imgName + ".bmp", ImageFormat.Bmp);
                swLog.WriteLine(GetDateTime() + "Converting done!");
                return true;
            }
            catch (Exception ex)
            {
                swLog.WriteLine(GetDateTime() + "Error converting: " + ex.Message);
            }
            return false;
        }

        /// <summary>
        /// Downloads day's picture from Yandex
        /// </summary>
        /// <returns>true - downloading done, false - error downloading</returns>
        static bool DownLoadWallpaper()
        {
            string url = @"https://yandex.ru/images/today?size=1920x1080";
            string url2 = @"http://yandex.ru/images/today?size=1920x1080";

            imgName = GetDate();
            string path = folder + imgName;

            WebClient client = new WebClient();
            try
            {
                swLog.WriteLine(GetDateTime() + "download https...");
                client.DownloadFile(url, path + ".jpg");
                swLog.WriteLine(GetDateTime() + "downloading done!");
                return true;
            }
            catch (Exception ex)
            {
                swLog.WriteLine(GetDateTime() + "Error downloading https: " + ex.Message);
            }

            try
            {
                swLog.WriteLine(GetDateTime() + "download http...");
                client.DownloadFile(url2, path);
                swLog.WriteLine(GetDateTime() + "downloading done!");
                return true;
            }
            catch (Exception ex)
            {
                swLog.WriteLine(GetDateTime() + "Error downloading http: " + ex.Message);
            }

            return false;
        }

        /// <summary>
        /// Returns current date and time
        /// for log.
        /// </summary>
        /// <returns>d.m.y h:m:s</returns>
        static string GetDateTime()
        {
            return DateTime.Now.Day + "." + DateTime.Now.Month + "." + DateTime.Now.Year + " " + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + " -- ";
        }

        /// <summary>
        /// Returns current date
        /// for wallpaper's file name.
        /// </summary>
        /// <returns>y-m-d</returns>
        static string GetDate()
        {
            return DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day;
        }
    }
}
