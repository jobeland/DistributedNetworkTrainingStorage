using ArtificialNeuralNetwork;
using NeuralNetwork.GeneticAlgorithm;
using System;
using System.Threading.Tasks;
namespace StorageAPI
{
    public interface IParseProxy
    {
        Task<ITrainingSession> GetBestSession();
        Task StoreNetwork(INeuralNetwork network, double eval);
    }
}
