//using CSCore;
//using CSCore.Codecs;
//using CSCore.SoundOut;
using NAudio;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SoundProcessor
{
    public class SoundPlayer
    {
        private AudioFileReader audioFileReader;
        private IWavePlayer waveOutDevice;
        private static SoundPlayer player;
        public string Status
        {
            get
            {
                return waveOutDevice.PlaybackState.ToString();
            }
        }

        public string Info;


        private SoundPlayer(string fileName)
        {
            waveOutDevice = new WaveOut();
            getTag(fileName);
            audioFileReader = new AudioFileReader(fileName);
            var Tag = Id3v2Tag.ReadTag(audioFileReader);
            //Info = GetInfo(Tag.RawData);
        }

        private void getTag(string fileName)
        {
            Info = "";
            if (fileName.EndsWith(".mp3"))
            {
                var reader = new Mp3FileReader(fileName);
                Info = GetInfo(reader.Id3v1Tag);
            }
            else
            {
                var reader = new MediaFoundationReader(fileName);
                var tag = Id3v2Tag.ReadTag(reader);
            }
        }

        public static SoundPlayer getInstance(string fileName)
        {
            if (player == null)
            {
                player = new SoundPlayer(fileName);
            }
            else
            {
                player.audioFileReader = new AudioFileReader(fileName);
                player.getTag(fileName);
                //player.Info = player.GetInfo(Tag.RawData);
            }
            return player;
        }

        public void Play()
        {
            waveOutDevice.Init(audioFileReader);
            waveOutDevice.Play();
        }

        public void Stop()
        {
            waveOutDevice.Stop();
        }

        public Stream getRawAudioDataStream()
        {
            Stream rawAudio = new MemoryStream();
            audioFileReader.CopyTo(rawAudio);
            return rawAudio;
        }

        public string GetInfo(byte[] b)
        {
            string sTitle = "unknown";
            string sSinger = "unknown";
            string sAlbum = "unknown";
            string sYear = "unknown";
            string sComm = "unknown";

            var isSet = false;
            var sFlag = Encoding.Default.GetString(b, 0, 3);
            if (sFlag.CompareTo("TAG") == 0)
            {
                isSet = true;
            }
            if (isSet)
            {
                sTitle = Encoding.Default.GetString(b, 3, 30);
                sSinger = Encoding.Default.GetString(b, 33, 30);
                sAlbum = Encoding.Default.GetString(b, 63, 30);
                sYear = Encoding.Default.GetString(b, 93, 4);
                sComm = Encoding.Default.GetString(b, 97, 30);
            }

            return String.Format("{0} - {1}", sSinger.Trim(), sTitle.Trim());
        }
    }
}
