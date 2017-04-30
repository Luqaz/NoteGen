using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundProcessor
{
    public class MFCCGainer
    {
        private int minFreq = 16;
        private int maxFreq = 20000;
        private List<double> freqs = new List<double>();

        public MFCCGainer(int numberOfFilters, int nFFT, int samplerate)
        {
            for (int i = 0; i < numberOfFilters; i++)
            {
                var maxMel = Freq2Mel(maxFreq);
                var minMel = Freq2Mel(minFreq);
                var avgDiff = (maxFreq - minFreq) / numberOfFilters;
                var freqResult = Mel2Freq(i * avgDiff + minMel);
                freqs.Add(Math.Floor((nFFT + 1) * freqResult / samplerate));
            }
        }

        private double Freq2Mel(double freq)
        {
            var mel = 1127.01048 * Math.Log(1 + freq / 700);
            return mel;
        }

        private double Mel2Freq(double mel)
        {
            var freq = 700 * (Math.Exp(mel / 1127.01048) - 1);
            return freq;
        }

        private double getPower(int filterNumber, double freq)
        {
            double result = 0;

            if(freq >= freqs[filterNumber - 1] && freq < freqs[filterNumber])
            {
                result = (freq - freqs[filterNumber - 1]) / (freqs[filterNumber] - freqs[filterNumber - 1]);
            }
            if (freq >= freqs[filterNumber] && freq <= freqs[filterNumber + 1])
            {
                result = (freqs[filterNumber + 1] - freq) / (freqs[filterNumber + 1] - freqs[filterNumber]);
            }
            return result;
        }

        public List<double> getCoefficents(List<SignalSample> signal)
        {
            var filteredPowers = new List<double>();
            for (int i = 1; i < freqs.Count - 1; i++)
            {
                var sum = 0d;
                foreach (var sample in signal)
                {
                    sum += (Math.Pow(sample.Amplitude, 2)/signal.Count) * getPower(i, sample.Frequency);
                }
                filteredPowers.Add(Math.Log(sum));
            }

            DCT(filteredPowers);

            return filteredPowers;
        }

        private void DCT (List<double> source)
        {
            double[] result = new double[source.Count];
            double c = Math.PI / (2.0 * source.Count);
            double scale = Math.Sqrt(2.0 / source.Count);

            for (int k = 0; k < source.Count; k++)
            {
                double sum = 0;
                for (int n = 0; n < source.Count; n++)
                    sum += source[n] * Math.Cos((2.0 * n + 1.0) * k * c);
                result[k] = scale * sum;
            }
            
            source[0] = result[0] / Math.Sqrt(2);
            for (int i = 1; i < source.Count; i++)
                source[i] = result[i];
        }
    }
}
