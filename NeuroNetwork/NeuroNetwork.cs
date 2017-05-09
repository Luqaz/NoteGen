using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace NeuroNetwork
{
    public class NeuroNetwork
    {
        private List<Neuron> inputLayer = new List<Neuron>();
        private List<Neuron> hiddenLayer = new List<Neuron>();
        private List<Neuron> outputLayer = new List<Neuron>();

        private List<double> input2hidden = new List<double>();
        private List<double> hidden2output = new List<double>();

        private const double eta = 0.1;
        private List<List<double>> inits = new List<List<double>>();

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
                inits.Add(init);
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

        public NeuroNetwork(List<List<double>> inputs, int numOfOutputs, string FileName)
        {
            var json = File.ReadAllText(FileName + ".json");
            var weights = new JavaScriptSerializer().Deserialize<List<List<double>>>(json);

            for (int j = 0; j < inputs[0].Count; j++)
            {
                var init = new List<double>();
                for (int i = 0; i < inputs.Count; i++)
                {
                    init.Add(inputs[i][j]);
                }
                inputLayer.Add(new Neuron(init, weights[0]));
                weights.RemoveAt(0);
                inits.Add(init);
                input2hidden.Add(0);
            }
            input2hidden.Add(1);

            int k = (int)Math.Round(Math.Sqrt(inputLayer.Count * numOfOutputs));

            for (int i = 0; i < k; i++)
            {
                hiddenLayer.Add(new Neuron(input2hidden, weights[0]));
                weights.RemoveAt(0);
                hidden2output.Add(0);
            }
            hidden2output.Add(1);

            for (int i = 0; i < numOfOutputs; i++)
            {
                outputLayer.Add(new Neuron(hidden2output, weights[0]));
                weights.RemoveAt(0);
            }
        }

        public List<double> GetResults()
        {
            var result = new List<double>();
            for(int i = 0; i < inputLayer.Count; i++)
            {
                input2hidden[i] = inputLayer[i].CountOutput();
            }
            for (int i = 0; i < hiddenLayer.Count; i++)
            {
                hidden2output[i] = hiddenLayer[i].CountOutput();
            }
            for (int i = 0; i < outputLayer.Count; i++)
            {
                result.Add(outputLayer[i].CountOutput());
            }
            return result;
        }

        public void Teach(List<double> expectedOutput)
        {
            var outputs = GetResults();
            double errorSum = 0;
            for(int i = 0; i < outputs.Count; i++)
            {
                errorSum += Math.Pow(expectedOutput[i] - outputs[i], 2);
            }
            errorSum = errorSum / 2;

            var outputDeltas = new List<List<double>>();

            for (int i = 0; i < outputLayer.Count; i++)
            {
                var dEdO = outputs[i] - expectedOutput[i];
                var dOdX = outputLayer[i].Derivative(outputs[i]);
                var deltasForNeuron = new List<double>();
                for (int j = 0; j < hidden2output.Count; j++)
                {
                    var dEdW = dEdO * dOdX * hidden2output[j];

                    deltasForNeuron.Add(-eta * dEdW);
                }
                outputDeltas.Add(deltasForNeuron);
            }
            var hiddenDEDSum = new List<double>();
            for(int i = 0; i < hiddenLayer.Count; i++)
            {
                double dETotaldO = 0;
                for(var j = 0; j < outputLayer.Count; j++)
                {
                    var dEdO = outputs[j] - expectedOutput[j];
                    var dOdSumO = outputLayer[j].Derivative(outputs[j]);
                    var dEdSum = dEdO * dOdSumO;
                    dETotaldO += dEdSum * outputLayer[j].Weights[i];
                }
                var dOdSum = hiddenLayer[i].Derivative(hidden2output[i]);
                hiddenDEDSum.Add(dETotaldO*dOdSum);
                var deltasForNeuron = new List<double>();
                for(int j = 0; j < input2hidden.Count; j++)
                {
                    var dEdW = dETotaldO * dOdSum * input2hidden[j];
                    deltasForNeuron.Add(-eta * dEdW);
                }
                outputDeltas.Add(deltasForNeuron);
            }

            for (int i = 0; i < inputLayer.Count; i++)
            {
                double dETotaldO = 0;
                for (var j = 0; j < hiddenLayer.Count; j++)
                {
                    dETotaldO += hiddenDEDSum[j] * hiddenLayer[j].Weights[i];
                }
                var dOdSum = inputLayer[i].Derivative(input2hidden[i]);
                var deltasForNeuron = new List<double>();
                for (int j = 0; j < inits[i].Count; j++)
                {
                    var dEdW = dETotaldO * dOdSum * inits[i][j];
                    deltasForNeuron.Add(-eta * dEdW);
                }
                outputDeltas.Add(deltasForNeuron);
            }

            for(int i = 0; i< outputLayer.Count; i++)
            {
                outputLayer[i].CorrectWeigts(outputDeltas[0]);
                outputDeltas.RemoveAt(0);
            }
            for (int i = 0; i < hiddenLayer.Count; i++)
            {
                hiddenLayer[i].CorrectWeigts(outputDeltas[0]);
                outputDeltas.RemoveAt(0);
            }
            for (int i = 0; i < inputLayer.Count; i++)
            {
                inputLayer[i].CorrectWeigts(outputDeltas[0]);
                outputDeltas.RemoveAt(0);
            }
        }

        public void SaveWeights(String FileName)
        {
            var result = new List<List<double>>();
            for (int i = 0; i < inputLayer.Count; i++)
            {
                result.Add(inputLayer[i].Weights);
            }
            for (int i = 0; i < hiddenLayer.Count; i++)
            {
                result.Add(hiddenLayer[i].Weights);
            }
            for (int i = 0; i < outputLayer.Count; i++)
            {
                result.Add(outputLayer[i].Weights);
            }

            var json = new JavaScriptSerializer().Serialize(result);
            File.WriteAllText(FileName + ".json", json);
        }
    }
}
