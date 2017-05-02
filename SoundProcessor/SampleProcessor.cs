using NAudio.Dsp;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundProcessor
{
    public class SampleProcessor : ISampleProvider
    {
        private ISampleProvider source;
        private int channels;
        public event EventHandler<MaxSampleEventArgs> MaximumCalculated;
        private float maxValue;
        private float minValue;
        public int NotificationCount { get; set; }

        public event EventHandler<FftEventArgs> FftCalculated;
        public bool PerformFFT { get; set; }
        private readonly Complex[] fftBuffer;
        private FftEventArgs fftArgs;
        private int fftPos;
        private readonly int fftLength;
        private int m;

        int count;

        public SampleProcessor(ISampleProvider source, int fftLength = 1024)
        {
            channels = source.WaveFormat.Channels;

            if (!IsPowerOfTwo(fftLength))
            {
                throw new ArgumentException("FFT Length must be a power of two");
            }
            this.m = (int)Math.Log(fftLength, 2.0);
            this.fftLength = fftLength;
            this.fftBuffer = new Complex[fftLength];
            

            this.source = source;
        }

        bool IsPowerOfTwo(int x)
        {
            return (x & (x - 1)) == 0;
        }

        public WaveFormat WaveFormat
        {
            get
            {
                return source.WaveFormat;
            }
        }

        public void Reset()
        {
            count = 0;
            maxValue = minValue = 0;
        }

        private void Add(float value)
        {
            fftBuffer[fftPos].X = (float)(value * FastFourierTransform.HammingWindow(fftPos, fftLength));
            fftBuffer[fftPos].Y = 0;
            fftPos++;
            if (fftPos >= fftBuffer.Length)
            {
                fftPos = 0;
                // 1024 = 2^10
                FastFourierTransform.FFT(true, m, fftBuffer);
                fftArgs = new FftEventArgs(fftBuffer, NotificationCount*100);
                FftCalculated(this, fftArgs);
            }

            maxValue = Math.Max(maxValue, value);
            minValue = Math.Min(minValue, value);
            count++;
            if (count >= NotificationCount && NotificationCount > 0)
            {
                MaximumCalculated?.Invoke(this, new MaxSampleEventArgs(minValue, maxValue));
                Reset();
            }
        }

        public int Read(float[] buffer, int offset, int count)
        {
            var samplesRead = source.Read(buffer, offset, count);

            for (int n = 0; n < samplesRead; n += channels)
            {

                Add(buffer[n + offset]);
            }

            //for(double x = 0; x <= 1; x+=1.0/44100)
            //{
            //    Add((float)sin600hz(x));    
            //}
            return samplesRead;
        }

        private double sin600hz(double x)
        {
            return Math.Sin(2 * Math.PI * 600 * x);
        }

        public class MaxSampleEventArgs : EventArgs
        {
            [DebuggerStepThrough]
            public MaxSampleEventArgs(float minValue, float maxValue)
            {
                this.MaxSample = maxValue;
                this.MinSample = minValue;
            }
            public float MaxSample { get; private set; }
            public float MinSample { get; private set; }
        }

        public class FftEventArgs : EventArgs
        {
            //[DebuggerStepThrough]
            public FftEventArgs(Complex[] result, int sampleRate)
            {
                this.Result = new List<SignalSample>();
                this.SampleRate = sampleRate;
                int numUniquePts = (int)System.Math.Ceiling((result.Length + 1) / 2.0);
                for (int i = 0; i < numUniquePts; i++)
                {
                    var amp = (result[i].X * result[i].X + result[i].Y * result[i].Y);
                    var freq = i * (double)sampleRate / result.Length;
                    Result.Add(new SignalSample(freq, amp));
                }
            }
            public List<SignalSample> Result { get; private set; }
            public int SampleRate { get; private set; }
        }
    }
}
