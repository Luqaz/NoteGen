using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundProcessor
{
    public class SignalSample
    {
        public double Frequency;
        public double Amplitude;

        public SignalSample(double freq, double amp)
        {
            Frequency = freq;
            Amplitude = amp;
        }
    }
}
