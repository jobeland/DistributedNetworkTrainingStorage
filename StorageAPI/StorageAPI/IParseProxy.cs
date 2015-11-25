using ArtificialNeuralNetwork;
using System;
using System.Threading.Tasks;
namespace StorageAPI
{
    public interface IParseProxy
    {
        Task<INeuralNetwork> GetBestNetwork();
        Task StoreNetwork(INeuralNetwork network, double eval);
    }
}
