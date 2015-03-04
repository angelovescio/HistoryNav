using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

namespace HistoryNav
{
    class FindFiles
    {
        public Database conn;
        public FindFiles(string RootDir, ref Database Conn)
        {
            conn = Conn;
            rootDrive = RootDir;
        }
        private bool IsDisposed = false;

        public void Dispose()
        {
            Dispose(true);
            //GC.SupressFinalize(this);
        }
        protected void Dispose(bool Diposing)
        {
            if (!IsDisposed)
            {
                if (Diposing)
                {
                    //Clean Up managed resources

                }
                //Clean up unmanaged resources

            }
            IsDisposed = true;
        }
        ~FindFiles()
        {
            Dispose(false);
            counter = 0;
        }
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        internal static extern IntPtr FindFirstFile(string lpFileName, out
                                WIN32_FIND_DATA lpFindFileData);

        string rootDrive;

        static StreamWriter sw;
        FileStream fs;
        IntPtr hHandle = IntPtr.Zero;

        internal static IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
        internal static int FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
        internal const int MAX_PATH = 260;
        uint counter = 0;

        int err = 0;
        bool breakRecursion = false;
        // Assume dirName passed in is already prefixed with \\?\
        /// <summary>
        /// Recursively search a given directory for all files and folders
        /// </summary>
        /// <param name="dirName">Root directory to start in</param>
        /// <param name="os">Type of operating system</param>
        public void FindFilesAndDirs(string dirName, OS os)
        {
            if (breakRecursion)
                return;

            if (MainForm.bgWorkScan.CancellationPending == true)
            {
                return;
            }
            WIN32_FIND_DATA findData;
            if (dirName == rootDrive)
            {
                if (counter > 0)
                {
                    breakRecursion = true;
                    return;
                }
                else
                    counter++;
            }
            //Get all contents of the first directory
            IntPtr findHandle = FindFirstFile(dirName + @"\*", out findData);

            if (findHandle != INVALID_HANDLE_VALUE)
            {
                bool found;
                do
                {
                    string currentFileName = findData.cFileName;
                    // if this is a directory, find its contents
                    if (((int)findData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) != 0)
                    {
                        if (currentFileName != "." && currentFileName != ".." && MainForm.bgWorkScan.CancellationPending == false)
                        {
                            string path = dirName.Replace(@"\\?\", "");
                            

                            FindFilesAndDirs(Path.Combine(dirName, currentFileName), os);

                        }
                        if (MainForm.bgWorkScan.CancellationPending == true)
                        {
                            breakRecursion = true;
                            break;
                        }
                    }

                    // it's a file; add it to the results
                    else
                    {
                        string path = Path.Combine(dirName, currentFileName).Replace(@"\\?\", "");
                        conn.Insert("scandisk", new string[]{"NULL","directory",path,currentFileName,Program.FileTimeToDateTime(findData.ftLastAccessTime),
                                     Program.FileTimeToDateTime(findData.ftCreationTime),Program.FileTimeToDateTime(findData.ftLastWriteTime),"NULL","NULL","NULL"});
                        if (os == OS.XP)
                        {
                            if (MainForm.bgWorkScan.CancellationPending == true)
                            {
                                return;
                            }
                            //XPScanner xpScan = new XPScanner(ref conn);
                            //xpScan.GetStreams(new FileInfo(path));

                        }
                        else
                        {
                            if (MainForm.bgWorkScan.CancellationPending == true)
                            {
                                return;
                            }
                            //FileStreamSearcher streamSearcher = new FileStreamSearcher(ref conn);
                            //streamSearcher.GetStreams(new FileInfo(path));
                        }
                    }
                    // find next file or directory
                    found = Program.pFuncFindNextFile(findHandle, out findData);
                }
                while (found && MainForm.bgWorkScan.CancellationPending == false);
            }
            if (MainForm.bgWorkScan.CancellationPending)
            {
                return;
            }
            return;
        }
    }
}
