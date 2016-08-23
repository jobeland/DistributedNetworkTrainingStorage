using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ArtificialNeuralNetwork;
using ArtificialNeuralNetwork.Factories;
using ArtificialNeuralNetwork.Genes;
using NeuralNetwork.GeneticAlgorithm;
using Newtonsoft.Json;

namespace StorageAPI.Proxies.NodeJs
{
    public class NodeJSProxy : IStorageProxy
    {
        private readonly int _networkVersion;
        private readonly string _apiKey;
        private readonly string _baseUrl;

        public NodeJSProxy(int networkVersion, string baseUrl, string apiKey)
        {
            _networkVersion = networkVersion;
            _baseUrl = baseUrl;
            _apiKey = apiKey;
        }

        public ITrainingSession GetBestSession()
        {
            using (var client = new HttpClient())
            {
                // Replace this with the API key for the web service
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
                var networkEndpoint = _baseUrl + "/network/" + _networkVersion;
                var response = client.GetAsync(networkEndpoint).Result;
                var responseObj = response.Content.ReadAsAsync<NodeJsMessage>().Result;
                if (responseObj == null)
                {
                    return null;
                }
                var networkGenes = JsonConvert.DeserializeObject<NeuralNetworkGene>(responseObj.networkGenes);
                var network = NeuralNetworkFactory.GetInstance().Create(networkGenes);
                var session = new FakeTrainingSession(network, responseObj.eval);
                return session;
            }
        }

        public async Task<ITrainingSession> GetBestSessionAsync()
        {
            using (var client = new HttpClient())
            {
                // Replace this with the API key for the web service
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
                var networkEndpoint = _baseUrl + "/network/" + _networkVersion;
                var response = await client.GetAsync(networkEndpoint);
                var responseObj = await response.Content.ReadAsAsync<NodeJsMessage>();
                if (responseObj == null)
                {
                    return null;
                }
                var networkGenes = JsonConvert.DeserializeObject<NeuralNetworkGene>(responseObj.networkGenes);
                var network = NeuralNetworkFactory.GetInstance().Create(networkGenes);
                var session = new FakeTrainingSession(network, responseObj.eval);
                return session;
            }
        }

        public void StoreNetwork(INeuralNetwork network, double eval)
        {
            using (var client = new HttpClient())
            {
                // Replace this with the API key for the web service
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
                var networkEndpoint = _baseUrl + "/network";
                var genesJson = JsonConvert.SerializeObject(network.GetGenes());
                var message = new NodeJsMessage
                {
                    eval = eval,
                    version = _networkVersion,
                    networkGenes = genesJson
                };
                var result = client.PostAsJsonAsync(networkEndpoint, message).Result;
            }
        }

        public async Task StoreNetworkAsync(INeuralNetwork network, double eval)
        {
            using (var client = new HttpClient())
            {
                // Replace this with the API key for the web service
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
                var networkEndpoint = _baseUrl + "/network";
                var genesJson = JsonConvert.SerializeObject(network.GetGenes());
                var message = new NodeJsMessage
                {
                    eval = eval,
                    version = _networkVersion,
                    networkGenes = genesJson
                };
                await client.PostAsJsonAsync(networkEndpoint, message);
            }
        }
    }
}
