//using CSCore;
//using CSCore.Codecs;
//using CSCore.SoundOut;
using NAudio;
using NAudio.Wave;
using NAudio.Flac;
using NAudio.Vorbis;
using NAudio.WindowsMediaFormat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace SoundProcessor
{
    public class SoundPlayer
    {
        public WaveStream fileStream;
        private IWavePlayer waveOutDevice;

        public event EventHandler<SampleProcessor.MaxSampleEventArgs> VolumeCalculated;

        protected virtual void OnMaximumCalculated(SampleProcessor.MaxSampleEventArgs e)
        {
            VolumeCalculated?.Invoke(this, e);
        }

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
            Stop();
            CloseFile();
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
                sampleProcessor.MaximumCalculated += (s, a) => OnMaximumCalculated(a);
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
            if (fileStream != null)
            {
                fileStream.Dispose();
                fileStream = null;
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

        public Stream getRawAudioDataStream()
        {
            Stream rawAudio = new MemoryStream();
            fileStream.CopyTo(rawAudio);
            return rawAudio;
        }

        public void Dispose()
        {
            Stop();
            CloseFile();
            if (waveOutDevice != null)
            {
                waveOutDevice.Dispose();
                waveOutDevice = null;
            }
        }
    }
}
