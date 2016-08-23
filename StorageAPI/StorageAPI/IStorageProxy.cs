using System.Threading.Tasks;
using ArtificialNeuralNetwork;
using NeuralNetwork.GeneticAlgorithm;

namespace StorageAPI
{
    public interface IStorageProxy
    {
        ITrainingSession GetBestSession();
        Task<ITrainingSession> GetBestSessionAsync();
        void StoreNetwork(INeuralNetwork network, double eval);
        Task StoreNetworkAsync(INeuralNetwork network, double eval);
    }
}
