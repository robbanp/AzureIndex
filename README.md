AzureIndex
==========

C# 4.5, Visual Studio 2012

Solution for handling Lucene.NET index directory files in Windows Azure using BlobStorage by creating a ZIP archive that is uploaded to Azure Blob Storage and synced with a client..

How it works:
Create Index -> ZIP'ed and uploaded to Blob Storage -> Client is pulling to see LastModified date -> Download and extracted when needed.



Example: 
Push index folder to Azure Blob Storage withe the name lucene.zip

```csharp
IndexHandler.PushToStorage(
    ConfigurationManager.AppSettings["blobStorage"],
    "lucenecontainer",
    "lucene.zip",
    "c://luceneindex/");

```

Example: 
Check for new ZIP archive on Blob Storage and download it, extract and replace current lucene index folder
```csharp
AzureIndex.IndexHandler.CheckStorage(
    ConfigurationManager.AppSettings["blobStorage"],
    "lucenecontainer",
    "lucene.zip",
    "c://luceneindex/",
    600); //check "LastModified" every 10 min.

```

By Robert Pohl, robert@sugarcubesolutions.com - http://sugarcubesolutions.com