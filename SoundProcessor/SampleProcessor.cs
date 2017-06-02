using NAudio.Dsp;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SoundProcessor
{
    public class SampleProcessor : ISampleProvider
    {
        private ISampleProvider source;
        private int channels;
        public int NotificationCount { get; set; }

        public event EventHandler<FftEventArgs> FftCalculated;
        private readonly Complex[] fftBuffer;
        private FftEventArgs fftArgs;
        private int fftPos;
        private readonly int fftLength;
        private int m;

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

        private void Add(float value)
        {
            fftBuffer[fftPos].X = (float)(value * FastFourierTransform.HammingWindow(fftPos, fftLength));
            fftBuffer[fftPos].Y = 0;
            fftPos++;
            if (fftPos >= fftBuffer.Length)
            {
                fftPos = 0;
                FastFourierTransform.FFT(true, m, fftBuffer);
                fftArgs = new FftEventArgs(fftBuffer, NotificationCount*100);
                FftCalculated(this, fftArgs);
            }
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int samplesRead;
            try
            {
                samplesRead = source.Read(buffer, offset, count);
            }
            catch(Exception)
            {
                return 0;
            }
            for (int n = 0; n < samplesRead; n += channels)
            {

                Add(buffer[n + offset]);
            }
            return samplesRead;
        }

        public class FftEventArgs : EventArgs
        {
            [DebuggerStepThrough]
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
