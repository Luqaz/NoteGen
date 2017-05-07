using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuroNetwork
{
    public class NeuroNetwork
    {
        private List<Neuron> inputLayer = new List<Neuron>();
        private List<Neuron> hiddenLayer = new List<Neuron>();
        private List<Neuron> outputLayer = new List<Neuron>();

        private List<double> input2hidden = new List<double>();
        private List<double> hidden2output = new List<double>();

        public NeuroNetwork(List<List<double>> inputs, int numOfOutputs)
        {
            for(int j = 0; j < inputs[0].Count; j++)
            {
                var init = new List<double>();
                for(int i = 0; i < inputs.Count; i++)
                {
                    init.Add(inputs[i][j]);
                }
                inputLayer.Add(new Neuron(init));
                input2hidden.Add(0);
            }
            input2hidden.Add(1);

            int k = (int)Math.Round(Math.Sqrt(inputLayer.Count * numOfOutputs));

            for(int i = 0; i < k; i++)
            {
                hiddenLayer.Add(new Neuron(input2hidden));
                hidden2output.Add(0);
            }
            hidden2output.Add(1);

            for (int i = 0; i < numOfOutputs; i++)
            {
                outputLayer.Add(new Neuron(hidden2output));
            }
        }
    }
}
