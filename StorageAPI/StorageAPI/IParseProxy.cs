using ArtificialNeuralNetwork;
using NeuralNetwork.GeneticAlgorithm;
using System;
using System.Threading.Tasks;
namespace StorageAPI
{
    public interface IParseProxy
    {
        ITrainingSession GetBestSession();
        void StoreNetwork(INeuralNetwork network, double eval);
    }
}
