using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuroNetwork
{
    public class Neuron
    {
        public List<double> Weights { get; private set; }
        public List<double> Inputs { get; private set; }
        public List<double> PreviousDeltas { get; private set; }
        private double alpha;

        public Neuron(List<double> inputVals, double alpha = 1)
        {
            Random rand = new Random();
            Weights = new List<double>();
            PreviousDeltas = new List<double>();
            Inputs = inputVals;
            foreach(var val in inputVals)
            {
                Weights.Add(rand.NextDouble());
                PreviousDeltas.Add(0);
            }
            this.alpha = alpha;
        }

        public Neuron(List<double> inputVals, List<double> weigtsVals, double alpha = 1)
        {
            Inputs = inputVals;
            Weights = weigtsVals;
            this.alpha = alpha;
        }

        public double CountOutput()
        {
            double summary = 0;
            for(int i = 0; i < Inputs.Count; i++)
            {
                summary += Weights[i] * Inputs[i];
            }
            return activationFunc(summary);
        }

        private double activationFunc(double x)
        {
            return 1 / (1 + Math.Exp(-alpha * x));
        }

        public double Derivative(double x)
        {
            return alpha * x * (1 - x);
        }

        public void CorrectWeigts(List<double> diffWeights)
        {
            for(int i = 0; i < Weights.Count; i++)
            {
                Weights[i] += diffWeights[i];
            }
        }
    }
}
