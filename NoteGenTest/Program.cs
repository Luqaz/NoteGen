using NeuroNetwork;
using SoundProcessor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoteGenTest
{
    class Program
    {
        static SoundPlayer player;

        static Dictionary<String, List<Double>> MarkedValues = new Dictionary<string, List<double>>();
        static List<List<double>> buffer = new List<List<double>>();
        static NeuroNetwork.NeuroNetwork network;

        static List<string> instruments = new List<string> { "Acoustic Guitar", "Bass Guitar", "Electric Guitar", "Drums", "Piano", "Violin", "Flute" };

        static TimeSpan pianoTime = new TimeSpan(0, 0, 0);

        static void Main(string[] args)
        {
            var instrumentFolders = Directory.GetDirectories(Directory.GetCurrentDirectory() + "/../../Samples");
            
            Dictionary<String, List<String>> map = new Dictionary<string, List<string>>();
            foreach (var folder in instrumentFolders)
            {
                var files = Directory.GetFiles(folder);
                for (int i = 0; i < instruments.Count; i++)
                {
                    if (folder.Contains(instruments[i]))
                    {
                        map.Add(instruments[i], files.ToList());
                    }
                }
            }

            for (int i = 0; i < instruments.Count; i++)
            {
                var values = new List<double>();
                for (int j = 0; j < instruments.Count; j++)
                {
                    values.Add(0.1);
                }
                values[i] = 1;
                MarkedValues.Add(instruments[i], values);
            }

            network = new NeuroNetwork.NeuroNetwork(MarkedValues.Count, "weights");

            player = new SoundPlayer();
            player.FftCalculated += audioGraph_FFTCalculated;

            for (int i = 0; i < instruments.Count; i++)
            {
                for (int j = 0; j < map[instruments[i]].Count; j++)
                {
                    player.Load(map[instruments[i]][j]);
                    Console.WriteLine("Playing {0}", map[instruments[i]][j]);
                    player.Play();
                    while(player.Status == "Playing")
                    {
                    }
                }
            }
        }

        static void audioGraph_FFTCalculated(object sender, SampleProcessor.FftEventArgs e)
        {
            var gainer = new MFCCGainer(26, e.Result.Count, e.SampleRate);
            buffer.Add(gainer.getCoefficents(e.Result));

            if (buffer.Count == 10)
            {
                var result = network.GetResults(buffer);
                for(int i = 0; i < result.Count; i++)
                {
                    if(result[i] > 0.4)
                        Console.WriteLine("{0} - {1}%", instruments[i], result[i]*100);
                }
                buffer.Clear();
            }
        }
    }
}
