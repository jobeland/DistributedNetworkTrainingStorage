using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ArtificialNeuralNetwork;
using ArtificialNeuralNetwork.Factories;
using ArtificialNeuralNetwork.Genes;
using NeuralNetwork.GeneticAlgorithm;
using Newtonsoft.Json;

namespace StorageAPI
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
                var networkGenes = JsonConvert.DeserializeObject<NeuralNetworkGene>(responseObj.NetworkGenes);
                var network = NeuralNetworkFactory.GetInstance().Create(networkGenes);
                var session = new FakeTrainingSession(network, responseObj.Eval);
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
                var networkGenes = JsonConvert.DeserializeObject<NeuralNetworkGene>(responseObj.NetworkGenes);
                var network = NeuralNetworkFactory.GetInstance().Create(networkGenes);
                var session = new FakeTrainingSession(network, responseObj.Eval);
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
                var message = new NodeJsMessage
                {
                    Eval = eval,
                    Version = _networkVersion,
                    NetworkGenes = network.GetGenes().ToString()
                };
                client.PostAsJsonAsync(networkEndpoint, message).Wait();
            }
        }

        public async Task StoreNetworkAsync(INeuralNetwork network, double eval)
        {
            using (var client = new HttpClient())
            {
                // Replace this with the API key for the web service
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
                var networkEndpoint = _baseUrl + "/network";
                var message = new NodeJsMessage
                {
                    Eval = eval,
                    Version = _networkVersion,
                    NetworkGenes = network.GetGenes().ToString()
                };
                await client.PostAsJsonAsync(networkEndpoint, message);
            }
        }
    }
}
