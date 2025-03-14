﻿using IronHive.Storages.AzureBlob;
using IronHive.Storages.LiteDB;
using IronHive.Storages.LocalDisk;
using IronHive.Storages.Qdrant;

namespace IronHive.Stack.WebApi.Configurations;

public enum VectorStorageTypes
{
    LiteDB,
    Qdrant
}

public enum DocumentStorageTypes
{
    LocalDisk,
    AzureBlob
}

public partial class StoragesConfig
{
    public VectorStorageConfig Vectors { get; set; } = new VectorStorageConfig();

    public DocumentStorageConfig Documents { get; set; } = new DocumentStorageConfig();

    public class VectorStorageConfig
    {
        public VectorStorageTypes Type { get; set; }
        public LiteDBConfig LiteDB { get; set; } = new LiteDBConfig();
        public QdrantConfig Qdrant { get; set; } = new QdrantConfig();
    }

    public class DocumentStorageConfig
    {
        public DocumentStorageTypes Type { get; set; }
        public LocalDiskConfig LocalDisk { get; set; } = new LocalDiskConfig();
        public AzureBlobConfig AzureBlob { get; set; } = new AzureBlobConfig();
    }
}
