//By Robert Pohl, robert@sugarcubesolutions.com

using System.IO;
using System.IO.Compression;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureIndex
{
    public class StorageHandler
    {
        private CloudStorageAccount storageAccount { get; set; }

        /// <summary>
        /// Create the storage handler connected to the storage info
        /// </summary>
        /// <param name="storageInfo">Connection to storage account</param>
        public StorageHandler(string storageInfo)
        {
            storageAccount = CloudStorageAccount.Parse(storageInfo);
        }

        /// <summary>
        /// Get blob reference
        /// </summary>
        /// <param name="container">Container name</param>
        /// <param name="fileName">Blob file name</param>
        /// <returns></returns>
        public CloudBlockBlob GetBlob(string container, string fileName)
        {
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer blobContainer = blobClient.GetContainerReference(container);
            return blobContainer.GetBlockBlobReference(fileName);
        }

        /// <summary>
        /// Put local file to blob storage
        /// </summary>
        /// <param name="container">Container name</param>
        /// <param name="fileName">Path to local file</param>
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

        /// <summary>
        /// Create ZIP archive from folder.
        /// </summary>
        /// <param name="inDir">Local folder path</param>
        /// <param name="outFile">Local out file path</param>
        public static void CreateArchive(string inDir, string outFile)
        {
            if (File.Exists(outFile))
            {
                File.Delete(outFile);
            }
            ZipFile.CreateFromDirectory(inDir, outFile);
        }

        /// <summary>
        /// Extract ZIP file to local folder
        /// </summary>
        /// <param name="outDir">Folder to create</param>
        /// <param name="inFile">ZIP archive to extract</param>
        /// <param name="delDest">Delete desination extract directory (recursive)</param>
        public static void ExtractArchive(string outDir, string inFile, bool delDest = true)
        {
            if (Directory.Exists(outDir) && delDest)
            {
                Directory.Delete(outDir,true);
            }
            ZipFile.ExtractToDirectory(inFile, outDir);
        }
    }
}