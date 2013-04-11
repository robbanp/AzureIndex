using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureIndex
{

    public class IndexHandler
    {
        public static DateTimeOffset? LastModified { get; set; }
        public static Object IndexLock = new Object();

        public static void PushToStorage(string indexPath, string packageFileName, string storageInfo, string containerName)
        {
            var destDir = System.IO.Directory.GetParent(indexPath).ToString();//index dir parent to store archive
            var destPath = System.IO.Path.Combine(destDir, packageFileName);
            StorageHandler.CreateArchive(indexPath,destPath);
            var storage = new StorageHandler(storageInfo);
            storage.SetBlob(containerName, destPath);
        }

        public static void CheckStorage(string storageInfo, string containerName, string packageFileName, string indexPath)
        {
            var storage = new StorageHandler(storageInfo);
            var blob = storage.GetBlob(containerName, packageFileName);

            blob.FetchAttributes();
            var modified = blob.Properties.LastModified;
            if (modified > LastModified || LastModified == null)
            {
                var zipPath = Directory.GetParent(indexPath).ToString();
                var archiveDest = Path.Combine(zipPath, packageFileName);
                using (var fileStream = System.IO.File.OpenWrite(archiveDest))
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
