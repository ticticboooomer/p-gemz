using Azure.Storage;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;

namespace Gemz.Api.Collector.Data.Factory;

public class BlobFactory
{
    private readonly IOptions<BlobConfig> _config;

    public BlobFactory(IOptions<BlobConfig> config)
    {
        _config = config;
    
    }

    public BlobContainerClient GetBlobContainer()
    {
        var credential = new StorageSharedKeyCredential(_config.Value.StorageAccount, _config.Value.Key);
        var blobUri = $"https://{_config.Value.StorageAccount}{_config.Value.BlobUri}";
        var blobServiceClient = new BlobServiceClient(new Uri(blobUri), credential);
        return blobServiceClient.GetBlobContainerClient(_config.Value.ContainerName);    
    }
}