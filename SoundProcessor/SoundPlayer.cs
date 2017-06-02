using NAudio.Wave;
using System;
using System.Windows;
using System.Threading;

namespace SoundProcessor
{
    public class SoundPlayer
    {
        public WaveStream fileStream;
        public IWavePlayer waveOutDevice;

        public event EventHandler<SampleProcessor.FftEventArgs> FftCalculated;

        protected virtual void OnFftCalculated(SampleProcessor.FftEventArgs e)
        {
            FftCalculated?.Invoke(this, e);
        }

        public string Status
        {
            get
            {
                return waveOutDevice.PlaybackState.ToString();
            }
        }

        public string Info;

        public void Load(string fileName)
        {
            CloseFile();
            Thread.Sleep(1000);
            EnsureDeviceCreated();
            OpenFile(fileName);
        }

        private void OpenFile(string fileName)
        {
            try
            {
                var audioFileReader = new AudioFileReader(fileName);
                fileStream = audioFileReader;
                var sampleProcessor = new SampleProcessor(audioFileReader);
                sampleProcessor.NotificationCount = audioFileReader.WaveFormat.SampleRate / 100;
                sampleProcessor.FftCalculated += (s, a) => OnFftCalculated(a);
                waveOutDevice.Init(sampleProcessor);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Problem opening file");
                CloseFile();
            }
        }

        private void EnsureDeviceCreated()
        {
            if (waveOutDevice == null)
            {
                CreateDevice();
            }
        }

        private void CreateDevice()
        {
            waveOutDevice = new WaveOut { DesiredLatency = 200 };
        }

        private void CloseFile()
        {
            if (waveOutDevice != null)
            {
                waveOutDevice.Stop();
            }
            if (fileStream != null)
            {
                fileStream.Dispose();
                fileStream = null;
            }
            if (waveOutDevice != null)
            {
                waveOutDevice.Dispose();
                waveOutDevice = null;
            }
        }

        public void Play()
        {
            
            if (waveOutDevice != null && fileStream != null && waveOutDevice.PlaybackState != PlaybackState.Playing)
            {
                waveOutDevice.Play();
            }
        }

        public void Stop()
        {
            if (waveOutDevice != null)
            {
                waveOutDevice.Stop();
            }
            if (fileStream != null)
            {
                fileStream.Position = 0;
            }
        }
    }
}
