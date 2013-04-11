using System;
using System.IO;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureIndex
{
    public class IndexHandler
    {
        public static Object IndexLock = new Object(); //lock index while updating it
        public static DateTimeOffset? LastModified { get; set; } // last updated datetime
        public static DateTime LastChecked { get; set; } // last time we checked index in storage

        public static void PushToStorage(string storageInfo, string containerName,  string packageFileName,string indexPath)
        {
            string destDir = Directory.GetParent(indexPath).ToString(); //index dir parent to store archive
            string destPath = Path.Combine(destDir, packageFileName);
            StorageHandler.CreateArchive(indexPath, destPath);
            var storage = new StorageHandler(storageInfo);
            storage.SetBlob(containerName, destPath);
        }

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