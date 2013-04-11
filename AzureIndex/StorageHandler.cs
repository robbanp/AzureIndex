using System.IO;
using System.IO.Compression;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureIndex
{
    public class StorageHandler
    {
        public StorageHandler(string storageInfo)
        {
            storageAccount = CloudStorageAccount.Parse(storageInfo);
        }

        private CloudStorageAccount storageAccount { get; set; }

        public CloudBlockBlob GetBlob(string container, string fileName)
        {
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer blobContainer = blobClient.GetContainerReference(container);
            return blobContainer.GetBlockBlobReference(fileName);
        }

        public void SetBlob(string container, string fileName)
        {
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer blobContainer = blobClient.GetContainerReference(container);
            blobContainer.CreateIfNotExists();
            blobContainer.SetPermissions(
                new BlobContainerPermissions
                    {
                        PublicAccess =
                            BlobContainerPublicAccessType.Blob
                    });

            CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(Path.GetFileName(fileName));
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