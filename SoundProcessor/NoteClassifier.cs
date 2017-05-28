using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundProcessor
{
    public class NoteClassifier
    {
        private List<Note> notes = new List<Note>();

        public NoteClassifier()
        {
            var noteNames = new List<String> { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A#", "B" };
            var basicFreq = 16.352;
            for (int i = 0; i < 11; i++)
            {
                for(int j = 0; j < 11; j++)
                {
                    notes.Add(new Note(noteNames[j] + i.ToString(), basicFreq));
                    basicFreq *= Math.Pow(2, 1.0 / 12);
                }
            }
        }

        private List<SignalSample> getDerivative(List<SignalSample> signal)
        {
            var result = new List<SignalSample>();
            for (int i = 0; i < signal.Count - 1; i++)
            {
                var derivative = (signal[i + 1].Amplitude - signal[i].Amplitude) / (signal[i + 1].Frequency - signal[i].Frequency);
                result.Add(new SignalSample(signal[i].Frequency, derivative));
            }
            return result;
        }

        private List<SignalSample> getLocalMaximums(List<SignalSample> signal)
        {
            var derivative = getDerivative(signal);
            var result = new List<SignalSample>();
            for(int i = 1; i < derivative.Count - 1; i++)
            {
                if(derivative[i-1].Amplitude > 0 && derivative[i+1].Amplitude < 0)
                {
                    result.Add(signal[i]);
                }
            }
            SignalSample temp;
            for(int i = 0; i < result.Count; i++)
            {
                for(int j = i; j < result.Count; j++)
                {
                    if(result[j].Amplitude > result[i].Amplitude)
                    {
                        temp = result[i];
                        result[i] = result[j];
                        result[j] = temp;
                    }
                }
            }
            return result;
        }

        public List<String> getNotes(List<SignalSample> signal)
        {
            var result = new List<String>();
            foreach(var sample in signal)
            {
                if(Math.Abs(sample.Amplitude) < 0.0001)
                {
                    sample.Amplitude = 0;
                }
            }
            var maximums = getLocalMaximums(signal);
            for(int i = 0; i < maximums.Count; i++)
            {
                if (maximums[i].Amplitude > 0.0001)
                {
                    var freq = maximums[i].Frequency;
                    for (int j = 0; j < notes.Count - 1; j++)
                    {
                        if (freq > notes[j].Frequency && freq < notes[j + 1].Frequency)
                        {
                            if (Math.Abs(freq - notes[j].Frequency) < Math.Abs(freq - notes[j + 1].Frequency))
                            {
                                result.Add(notes[j].Name);
                            }
                            else
                            {
                                result.Add(notes[j + 1].Name);
                            }
                        }
                    }
                }
            }
            return result;
        }
    }
}
