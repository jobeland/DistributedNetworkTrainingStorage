using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArtificialNeuralNetwork;
using ArtificialNeuralNetwork.Factories;
using ArtificialNeuralNetwork.Genes;
using Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using NeuralNetwork.GeneticAlgorithm;
using Newtonsoft.Json;
using Polly;
using LogLevel = Logging.LogLevel;

namespace StorageAPI.Proxies.AzureBlob
{
    public class AzureBlobProxy : IStorageProxy
    {
        private readonly string _connectionString;
        private readonly string _containerName;
        private readonly string _evalDirectory;
        private readonly int _version;
        private static Policy policy = Policy
            .Handle<TimeoutException>()
            .Or<StorageException>()
            .WaitAndRetry(
                5,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, timeSpan, retryCount, context) => {
                        LoggerFactory.GetLogger().Log(LogLevel.Info, $"Retry attempt: {retryCount}, Timespan to wait: {timeSpan}, Exception: {exception.Message}");
                        LoggerFactory.GetLogger().Log(LogLevel.Info, exception.StackTrace);
                    }
                );


        public AzureBlobProxy(string connectionString, string containerName, string evalDirectory, int version)
        {
            _connectionString = connectionString;
            _containerName = containerName;
            _evalDirectory = evalDirectory;
            _version = version;
        }

        public ITrainingSession GetBestSession()
        {
            return policy.Execute(() =>
            {
                var container = GetBlobContainer();
                var topEval = GetTopEval(container);
                if (Math.Abs(topEval - double.MinValue) < 0.000000000001)
                {
                    return null;
                }
                var serialized = DownloadBlob(container, _evalDirectory + "/" + _version + "/" + topEval);
                //convert to session
                return ExtractSession(serialized, topEval);
            });
        }

        public async Task<ITrainingSession> GetBestSessionAsync()
        {
            return await policy.ExecuteAsync(async () =>
            {
                var container = GetBlobContainer();
                var topEval = GetTopEval(container);
                if (Math.Abs(topEval - double.MinValue) < 0.000000000001)
                {
                    return null;
                }
                var serialized = await DownloadBlobAsync(container, _evalDirectory + "/" + _version + "/" + topEval);
                //convert to session
                return ExtractSession(serialized, topEval);
            });
        }

        internal ITrainingSession ExtractSession(string serialized, double eval)
        {
            var networkGenes = JsonConvert.DeserializeObject<NeuralNetworkGene>(serialized);
            var network = NeuralNetworkFactory.GetInstance().Create(networkGenes);
            var session = new FakeTrainingSession(network, eval);
            return session;
        }

        internal double GetTopEval(CloudBlobContainer container)
        {
            var stringEvals = GetBlobNamesForDirectory(container, _evalDirectory + "/" + _version);
            var evals = stringEvals.Select(se => double.Parse(se)).OrderByDescending(e => e).ToList();
            if (!evals.Any())
            {
                return double.MinValue;
            }
            return evals[0];
        }

        internal CloudBlobContainer GetBlobContainer()
        {
            var storageAccount = CloudStorageAccount.Parse(_connectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            //Ensure container name is all lower case, or else Blob Storage will return a 400:Bad Request
            var container = blobClient.GetContainerReference(_containerName.ToLower());
            container.CreateIfNotExists();
            return container;
        }

        internal List<string> GetBlobNamesForDirectory(CloudBlobContainer container, string directoryName)
        {
            var folder = container.GetDirectoryReference(directoryName);
            var blobs = folder.ListBlobs(true);
            var blobNames = new List<string>();
            foreach (var item in blobs)
            {
                if (item.GetType() == typeof(CloudBlockBlob))
                {
                    var blob = (CloudBlockBlob)item;
                    var blobNameParts = blob.Name.Split('/');
                    blobNames.Add(blobNameParts[blobNameParts.Length-1]);
                }
                else if (item.GetType() == typeof(CloudPageBlob))
                {
                    var pageBlob = (CloudPageBlob)item;
                    var blobNameParts = pageBlob.Name.Split('/');
                    blobNames.Add(blobNameParts[blobNameParts.Length - 1]);
                }
            }
            return blobNames;
        }

        internal string DownloadBlob(CloudBlobContainer container, string blobName)
        {
            var blockBlob = container.GetBlockBlobReference(blobName);
            string data;
            using (var memoryStream = new MemoryStream())
            {
                blockBlob.DownloadToStream(memoryStream);
                data = DecompressBugs(memoryStream.ToArray());
            }
            return data;
        }

        internal async Task<string> DownloadBlobAsync(CloudBlobContainer container, string blobName)
        {
            var blockBlob = container.GetBlockBlobReference(blobName);
            string data;
            using (var memoryStream = new MemoryStream())
            {
                await blockBlob.DownloadToStreamAsync(memoryStream);
                data = DecompressBugs(memoryStream.ToArray());
            }
            return data;
        }

        internal string DecompressBugs(byte[] compressedData)
        {
            string output;
            using (var inStream = new MemoryStream(compressedData))
            {
                using (var bigStream = new GZipStream(inStream, CompressionMode.Decompress))
                {
                    using (var bigStreamOut = new MemoryStream())
                    {
                        bigStream.CopyTo(bigStreamOut);
                        output = Encoding.UTF8.GetString(bigStreamOut.ToArray());
                    }
                }
            }
            return output;
        }

        internal void UploadToBlob(CloudBlobContainer container, string blobName, string dataToWrite)
        {
            var compressedData = CompressData(dataToWrite);

            var blockBlob = container.GetBlockBlobReference(blobName);
            blockBlob.Properties.ContentEncoding = "gzip";
            using (var stream = new MemoryStream(compressedData))
            {
                blockBlob.UploadFromStream(stream);
            }
        }

        internal async Task UploadToBlobAsync(CloudBlobContainer container, string blobName, string dataToWrite)
        {
            var compressedData = CompressData(dataToWrite);

            var blockBlob = container.GetBlockBlobReference(blobName);
            blockBlob.Properties.ContentEncoding = "gzip";
            using (var stream = new MemoryStream(compressedData))
            {
                await blockBlob.UploadFromStreamAsync(stream);
            }
        }

        internal byte[] CompressData(string data)
        {
            byte[] bytes;

            using (var memStream = new MemoryStream())
            {
                using (var tinyStream = new GZipStream(memStream, CompressionMode.Compress))
                {
                    using (var mStream = new MemoryStream(Encoding.UTF8.GetBytes(data)))
                    {
                        mStream.CopyTo(tinyStream);
                    }
                }
                bytes = memStream.ToArray();
            }

            return bytes;
        }

        public void StoreNetwork(INeuralNetwork network, double eval)
        {
            policy.Execute(() =>
            {
                var container = GetBlobContainer();
                var topEval = GetTopEval(container);
                if (topEval >= eval)
                {
                    return;
                }
                var serialized = JsonConvert.SerializeObject(network.GetGenes());
                UploadToBlob(container, _evalDirectory + "/" + _version + "/" + eval, serialized);
            });
        }

        public async Task StoreNetworkAsync(INeuralNetwork network, double eval)
        {
            await policy.ExecuteAsync(async () =>
            {
                var container = GetBlobContainer();
                var topEval = GetTopEval(container);
                if (topEval >= eval)
                {
                    return;
                }
                var serialized = JsonConvert.SerializeObject(network.GetGenes());
                await UploadToBlobAsync(container, _evalDirectory + "/" + _version + "/" + eval, serialized);
            });
        }
    }
}
