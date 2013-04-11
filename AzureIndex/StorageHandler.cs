using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureIndex
{

    public class StorageHandler
    {
        private CloudStorageAccount storageAccount { get; set; }

        public StorageHandler(string storageInfo)
        {
            storageAccount = CloudStorageAccount.Parse(storageInfo);
        }

        public CloudBlockBlob GetBlob(string container, string fileName)
        {
            var blobClient = storageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(container);
            return blobContainer.GetBlockBlobReference(fileName);
        }

        public void SetBlob(string container, string fileName)
        {
            var blobClient = storageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(container);
            blobContainer.CreateIfNotExists();
            blobContainer.SetPermissions(
                new BlobContainerPermissions
                {
                    PublicAccess =
                        BlobContainerPublicAccessType.Blob
                });

            var blockBlob = blobContainer.GetBlockBlobReference(Path.GetFileName(fileName));
            blockBlob.UploadFromStream(File.OpenRead(fileName));
        }

        public static void CreateArchive(string inDir, string outFile)
        {
            if (File.Exists(outFile))
            {
                File.Delete(outFile);
            }
            ZipFile.CreateFromDirectory(inDir, outFile);
        }

        public static void ExtractArchive(string outDir, string inFile)
        {
            ZipFile.ExtractToDirectory(inFile, outDir);
        }
    }
}
