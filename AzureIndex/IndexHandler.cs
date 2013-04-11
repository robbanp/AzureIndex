//By Robert Pohl, robert@sugarcubesolutions.com
 
using System;
using System.IO;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureIndex
{
    public class IndexHandler
    {
        private static Object IndexLock = new Object(); //lock index while updating it
        private static DateTimeOffset? LastModified { get; set; } // last updated datetime
        private static DateTime LastChecked { get; set; } // last time we checked index in storage

        /// <summary>
        /// Push file to Blob storage
        /// </summary>
        /// <param name="storageInfo">Connection info</param>
        /// <param name="containerName">Container name</param>
        /// <param name="packageFileName">File name</param>
        /// <param name="indexPath">Path to local folder</param>
        public static void PushToStorage(string storageInfo, string containerName,  string packageFileName,string indexPath)
        {
            string destDir = Directory.GetParent(indexPath).ToString(); //index dir parent to store archive
            string destPath = Path.Combine(destDir, packageFileName);
            StorageHandler.CreateArchive(indexPath, destPath);
            var storage = new StorageHandler(storageInfo);
            storage.SetBlob(containerName, destPath);
        }

        /// <summary>
        /// Check and update folder if it's new
        /// </summary>
        /// <param name="storageInfo">Connection info</param>
        /// <param name="containerName">Container name</param>
        /// <param name="packageFileName">Archive file name</param>
        /// <param name="indexPath">Path to folder</param>
        /// <param name="checkEverySeconds">Number of seconds between blob storage check</param>
        public static void CheckStorage(string storageInfo, string containerName, string packageFileName,
                                        string indexPath, int checkEverySeconds)
        {
            if (LastChecked != null && DateTime.UtcNow.AddSeconds(0 - checkEverySeconds) < LastChecked) //wait xx seconds until next peek in blob storage
            {
                return;
            }
            LastChecked = DateTime.UtcNow;

            var storage = new StorageHandler(storageInfo);
            CloudBlockBlob blob = storage.GetBlob(containerName, packageFileName);

            blob.FetchAttributes();
            DateTimeOffset? modified = blob.Properties.LastModified;
            if (modified > LastModified || LastModified == null)
            {
                string zipPath = Directory.GetParent(indexPath).ToString();
                string archiveDest = Path.Combine(zipPath, packageFileName);
                using (FileStream fileStream = File.OpenWrite(archiveDest))
                {
                    blob.DownloadToStream(fileStream);
                }
                LastModified = modified;
                lock (IndexLock)
                {
                    StorageHandler.ExtractArchive(indexPath + "_tmp", archiveDest);
                    if (Directory.Exists(indexPath))
                    {
                        Directory.Delete(indexPath, true);
                    }
                    Directory.Move(indexPath + "_tmp", indexPath);
                }
                //unlock
            }
        }
    }
}