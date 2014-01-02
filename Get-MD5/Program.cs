using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Get_MD5
{
    class Program
    {
        [DllImport("user32.dll")]
        internal static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll")]
        internal static extern bool CloseClipboard();

        [DllImport("user32.dll")]
        internal static extern bool SetClipboardData(uint uFormat, IntPtr data);

        private static string thrownExceptions = String.Empty;

        static void Main(string[] args)
        {
        TryOverGoto:
            Console.WriteLine("Give up a directory or filename to obtain the MD5 hash for.");
            Console.Write("Dir or file: ");
            string dirOrFileName = Console.ReadLine();

            if (Path.HasExtension(dirOrFileName))
            {
                if (!File.Exists(dirOrFileName))
                {
                    Console.WriteLine("\n\nThe given file could not be located. Press any key to try over.\n\n");
                    Console.ReadLine();
                    goto TryOverGoto;
                }

                string md5Hash = GetMD5HashFromFile(dirOrFileName);
                Console.WriteLine("The MD5 hash of file '" + Path.GetFileName(dirOrFileName) + "' is " + md5Hash);

                if (!String.IsNullOrEmpty(thrownExceptions))
                    Console.WriteLine("The following exceptions were thrown:\n\n" + thrownExceptions + "\n\n");

                thrownExceptions = String.Empty;
                Console.WriteLine("Press any key to start from the beginning.");

                //! Set clipboard
                OpenClipboard(IntPtr.Zero);
                var ptr = Marshal.StringToHGlobalUni(md5Hash);
                SetClipboardData(13, ptr);
                CloseClipboard();
                Marshal.FreeHGlobal(ptr);

                Console.ReadLine();
                goto TryOverGoto;
            }
            else
            {
                if (!Directory.Exists(dirOrFileName))
                {
                    Console.WriteLine("\n\nThe given directory could not be located. Press any key to try over.\n\n");
                    Console.ReadLine();
                    goto TryOverGoto;
                }

                string allFiles = String.Empty, clipboardMessage = String.Empty;
                GetAllFilesFromDirectory(dirOrFileName, ref allFiles);
                string[] _arrayFiles = allFiles.Split('\n');

                foreach (string file in _arrayFiles)
                {
                    if (!Path.HasExtension(file))
                        continue;

                    string md5Hash = GetMD5HashFromFile(file);
                    Console.WriteLine("The MD5 hash of file '" + Path.GetFileName(file) + "' is " + md5Hash);
                    clipboardMessage += file + "," + md5Hash + "\n";
                }

                //! Set clipboard
                OpenClipboard(IntPtr.Zero);
                var ptr = Marshal.StringToHGlobalUni(clipboardMessage);
                SetClipboardData(13, ptr);
                CloseClipboard();
                Marshal.FreeHGlobal(ptr);

                Console.WriteLine("\n\n\nThe MD5 hashes have been saved to your clipboard along with their filenames.");

                if (!String.IsNullOrEmpty(thrownExceptions))
                    Console.WriteLine("The following exceptions were thrown:\n\n" + thrownExceptions + "\n\n");

                thrownExceptions = String.Empty;
                Console.WriteLine("\nPress any key to start from the beginning.");
                Console.ReadLine();
                goto TryOverGoto;
            }
        }

        static private void GetAllFilesFromDirectory(string directorySearch, ref string allFiles)
        {
            string[] directories = Directory.GetDirectories(directorySearch);
            string[] files = Directory.GetFiles(directorySearch);

            for (int i = 0; i < files.Length; i++)
                if (!files[i].Contains("merged_") && files[i] != String.Empty)
                    if ((File.GetAttributes(files[i]) & FileAttributes.Hidden) != FileAttributes.Hidden)
                        allFiles += files[i] + "\n";

            //! If we include sub directories, recursive call this function up to every single directory.
            for (int i = 0; i < directories.Length; i++)
                GetAllFilesFromDirectory(directories[i], ref allFiles);
        }

        static protected string GetMD5HashFromFile(string fileName)
        {
            if (String.IsNullOrEmpty(fileName))
                return String.Empty;

            byte[] retVal = null;

            try
            {
                using (FileStream file = new FileStream(fileName, FileMode.Open))
                {
                    MD5 md5 = new MD5CryptoServiceProvider();
                    retVal = md5.ComputeHash(file);
                    file.Close();
                }
            }
            catch (Exception ex)
            {
                thrownExceptions += "File '" + Path.GetFileName(fileName) + "' threw the following exception:\n\n" + ex.Message + "\n";
            }

            if (retVal == null)
                return String.Empty;

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < retVal.Length; i++)
                sb.Append(retVal[i].ToString("x2"));

            return sb.ToString();
        }
    }
}
