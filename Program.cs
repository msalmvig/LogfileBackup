using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LogfileBackup
{
    class Program
    {
        static void Main(string[] args)
        {
            // Get setup data from the config file.
            string logsDirectory = Settings1.Default.LogsDirectory;
            string logNamePrefix = Settings1.Default.LogDirNamePrefix;
            string backupRoot = Settings1.Default.LogBackupDirectory;

            // Be sure the Log Backup Directory exists, if not create it.
            if (!Directory.Exists(backupRoot))
            {
                Directory.CreateDirectory(backupRoot);
            }

            // Get a List of directories to crawl for log files.
            string[] logdirectories = Directory.GetDirectories(logsDirectory, 
                String.Format("{0}*", logNamePrefix));

            // For each directory matching the name "log*" capture log files from it.
            foreach(string logDirectory in logdirectories) {
                //Console.WriteLine(logDirectory);

                CaptureLogRoot(logDirectory, backupRoot);
            }
        }


        /// <summary>
        /// Backup the logfile in the provided folder and search for numbered subfolders.
        /// </summary>
        /// <param name="logRootName">The folder name to check.</param>
        /// <param name="backupRootFolder">Folder to backup to</param>
        static public void CaptureLogRoot(string logRootName, string backupRootFolder)
        {
            string[] logFiles = Directory.GetFiles(logRootName, "nwserverlog1.txt", SearchOption.AllDirectories);

            foreach (string fileName in logFiles)
            {
                //Console.WriteLine(fileName);
                CaptureLog(fileName, backupRootFolder);
            }
        }

        /// <summary>
        /// Capture a single log file by looking at its last write time and creating a log file in the proper log folder.
        /// </summary>
        /// <param name="logFile">The filename to examine</param>
        /// <param name="backupRootFolder">The folder for holding backups.</param>
        static public void CaptureLog(string logFile, string backupRootFolder)
        {
            FileInfo fileInfo = new FileInfo(logFile);

            // Look for the .0, .1, .2 at the end and preseve it.
            string appendix = "";
            if (!fileInfo.DirectoryName.EndsWith("g"))
            {
                appendix = fileInfo.DirectoryName[fileInfo.DirectoryName.Length - 1].ToString();
            }

            // Create the Log Name.
            string archiveName = String.Format("{0}\\{1}{2}\\Log{3}{4}{5}-{6}{7}-{8}.txt",
                backupRootFolder,
                fileInfo.LastWriteTime.ToString("MMMM"),  // Full Month Name
                fileInfo.LastWriteTime.ToString("yy"),    // Last two digits of year (folder)
                fileInfo.LastWriteTime.ToString("yy"),    // Last two digits of year (log name)
                fileInfo.LastWriteTime.ToString("MM"),    // Month name in two digits
                fileInfo.LastWriteTime.ToString("dd"),    // Day in two digits
                fileInfo.LastWriteTime.ToString("hh"),    // Hours
                fileInfo.LastWriteTime.ToString("mm"),
                appendix);

            string folderName = String.Format("{0}\\{1}{2}",
                backupRootFolder,
                fileInfo.LastWriteTime.ToString("MMMM"),  // Full Month Name
                fileInfo.LastWriteTime.ToString("yy"));   // Last two digits of year (folder)

            // Compare log name and archive name
            Console.WriteLine(String.Format("{0}, {1}", logFile, archiveName));

            // Create the log folder if it doesn't exist.
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }
 
            // See if a archive file exists that was created at this time already.
            if (File.Exists(archiveName))
            {
                FileInfo archiveInfo = new FileInfo(archiveName);
                if (fileInfo.Length > archiveInfo.Length)
                {
                    // The log file was updated.
                    File.Copy(logFile, archiveName, true);
                }
            }
            else
            {
                // Doesn't exists so just copy it.
                File.Copy(logFile, archiveName, true);
            }
        }
    }
}
