using NeuralNetwork.GeneticAlgorithm;
using NeuralNetwork.GeneticAlgorithm.Evolution;
using NeuralNetwork.GeneticAlgorithm.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageAPI
{
    public class BestPerformerUpdater : IEpochAction
    {
        private readonly IParseProxy _proxy;

        public BestPerformerUpdater(IParseProxy proxy)
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
