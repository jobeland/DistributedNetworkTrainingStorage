using NeuralNetwork.GeneticAlgorithm;
using NeuralNetwork.GeneticAlgorithm.Evolution;
using NeuralNetwork.GeneticAlgorithm.Utils;

namespace StorageAPI
{
    public class BestPerformerUpdater : IEpochAction
    {
        private readonly IStorageProxy _proxy;

        public BestPerformerUpdater(IStorageProxy proxy)
        {
            _proxy = proxy;
        }

        public ITrainingSession UpdateBestPerformer(IGeneration lastGenerationOfEpoch, int epochNumber)
        {
            var bestPerformer = lastGenerationOfEpoch.GetBestPerformer();
            _proxy.StoreNetwork((ArtificialNeuralNetwork.NeuralNetwork)bestPerformer.NeuralNet, bestPerformer.GetSessionEvaluation());
            return _proxy.GetBestSession();
        }
    }
}
