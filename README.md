AzureIndex
==========

Solution for handling Lucene.NET index directory files in Windows Azure using BlobStorage by creating a ZIP archive that is uploaded to Azure Blob Storage and synced with a client..

How it works:
Create Index -> ZIP'ed and uploaded to Blob Storage -> Client is pulling to see LastModified date -> Download and extracted when needed.



