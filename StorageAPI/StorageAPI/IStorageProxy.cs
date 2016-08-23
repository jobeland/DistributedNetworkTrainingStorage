using ArtificialNeuralNetwork;
using NeuralNetwork.GeneticAlgorithm;

namespace StorageAPI
{
    public interface IStorageProxy
    {
        ITrainingSession GetBestSession();
        void StoreNetwork(INeuralNetwork network, double eval);
    }
}
