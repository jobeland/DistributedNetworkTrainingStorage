using ArtificialNeuralNetwork;
using NeuralNetwork.GeneticAlgorithm;

namespace StorageAPI
{
    public class FakeTrainingSession : ITrainingSession
    {
        public INeuralNetwork NeuralNet { get; set; }
        private readonly double _eval;

        public FakeTrainingSession(INeuralNetwork network, double eval)
        {
            NeuralNet = network;
            _eval = eval;
        }

        public double GetSessionEvaluation()
        {
            return _eval;
        }

        public void Run()
        {
            return;
        }
    }
}
