using ArtificialNeuralNetwork;
using NeuralNetwork.GeneticAlgorithm;

namespace StorageAPI
{
    public interface IParseProxy
    {
        ITrainingSession GetBestSession();
        void StoreNetwork(INeuralNetwork network, double eval);
    }
}
