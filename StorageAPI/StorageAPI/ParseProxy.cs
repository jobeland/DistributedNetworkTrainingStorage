using ArtificialNeuralNetwork;
using NeuralNetwork.GeneticAlgorithm;
using Newtonsoft.Json;
using Parse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageAPI
{
    public class ParseProxy : IParseProxy 
    {
        private readonly string _networkVersion;

        public ParseProxy(string networkVersion, string appId, string dotNetKey)
        {
            _networkVersion = networkVersion;
            ParseClient.Initialize(appId, dotNetKey);
        }

        public async Task StoreNetwork(INeuralNetwork network, double eval)
        {
            var networkParseFormat = new ParseObject(_networkVersion);
            networkParseFormat["jsonNetwork"] = JsonConvert.SerializeObject(network);
            networkParseFormat["eval"] = eval;
            await networkParseFormat.SaveAsync();
        }

        public async Task<ITrainingSession> GetBestSession()
        {
            var result = await ParseCloud.CallFunctionAsync<ParseObject>("bestNetwork", new Dictionary<string, object> { { "networkVersion", _networkVersion } });
            var network = JsonConvert.DeserializeObject<ArtificialNeuralNetwork.NeuralNetwork>((string)result["jsonNetwork"]);
            return new FakeTrainingSession(network, (double)result["eval"]);
        }
    }
}
