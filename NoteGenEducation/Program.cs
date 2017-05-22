using SoundProcessor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeuroNetwork;

namespace NoteGenEducation
{
    class Program
    {
        static SoundPlayer player;

        static Dictionary<String, List<Double>> MarkedValues = new Dictionary<string, List<double>>();
        static String currentInstrument;
        static List<List<double>> buffer = new List<List<double>>();
        static NeuroNetwork.NeuroNetwork network;
        static List<string> instruments = new List<string> { "Acoustic Guitar", "Electric Guitar", "Drums", "Piano", "Violin", "Flute" };
        static bool isTest;
        static bool isValid;
        static Dictionary<String, List<String>> testMap = new Dictionary<string, List<string>>();

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
                    values.Add(0.01);
                }
                values[i] = 1;
                MarkedValues.Add(instruments[i], values);
            }

            network = new NeuroNetwork.NeuroNetwork(MarkedValues.Count);

            player = new SoundPlayer();
            player.FftCalculated += audioGraph_FFTCalculated;

            var random = new Random();
            int generationNumber = 0;
            int filesTrained = 0;

            for(int i = 0; i < instruments.Count; i++)
            {
                testMap.Add(instruments[i], new List<String>());
            }
            int count = 0;
            for(int i = 0; i < instruments.Count; i++)
            {
                count += map[instruments[i]].Count;
            }
            do
            {
                for (int i = 0; i < count; i++)
                {
                    try
                    {
                        var instrumentNumber = (int)Math.Round(random.NextDouble() * (instruments.Count - 1));
                        currentInstrument = instruments[instrumentNumber];
                        var fileNumber = (int)Math.Round(random.NextDouble() * (map[currentInstrument].Count - 1));

                        player.Load(map[currentInstrument][fileNumber]);
                        player.Play();
                        if (currentInstrument == "Piano")
                        {
                            if (player.fileStream.TotalTime.Seconds > 4)
                            {
                                continue;
                            }
                        }
                        //Console.WriteLine("Playing {0}...", map[currentInstrument][j]);
                        while (player.Status == "Playing")
                        {
                        }
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                    
                }
                isTest = true;

                for (int i = 0; i < instruments.Count; i++)
                {
                    testMap[instruments[i]].Clear();
                }

                Console.WriteLine("Generation - {0}", generationNumber);
                generationNumber++;
                for(int i = 0; i < instruments.Count; i++)
                {

                    currentInstrument = instruments[i];
                    var fileNumber = (int)Math.Round(random.NextDouble() * (map[currentInstrument].Count - 1));
                    player.Load(map[instruments[i]][fileNumber]);
                    player.Play();
                    while (player.Status == "Playing")
                    {
                    }
                }
                isTest = false;
                isValid = true;
                for (int i = 0; i < instruments.Count; i++)
                {
                    var valid = .0;
                    for(int j = 0; j < testMap[instruments[i]].Count; j++)
                    {
                        if(testMap[instruments[i]][j] == instruments[i])
                        {
                            valid++;
                        }
                    }
                    isValid &= valid / testMap[instruments[i]].Count > 0.7;
                    Console.WriteLine("{0} - {1}/{2}", instruments[i], valid, testMap[instruments[i]].Count);
                    testMap[instruments[i]].Clear();
                }
            }
            while (!isValid);
            network.SaveWeights("weights");
        }

        static void audioGraph_FFTCalculated(object sender, SampleProcessor.FftEventArgs e)
        {
            var gainer = new MFCCGainer(26, e.Result.Count, e.SampleRate);
            buffer.Add(gainer.getCoefficents(e.Result));

            if (buffer.Count == 15)
            {
                if (!isTest)
                {
                    Console.WriteLine("Current ErrorSum: {0}", network.errorSum);
                    var results = network.Teach(buffer, MarkedValues[currentInstrument]);
                    for (int i = 0; i < results.Count; i++)
                    {
                        Console.WriteLine("{0} - {1:00.00}%", instruments[i], results[i] * 100);
                    }
                }
                else
                {
                    var results = network.GetResults(buffer);
                    for (int i = 0; i < results.Count; i++)
                    {
                        if(results[i] > 0.5)
                        {
                            testMap[currentInstrument].Add(instruments[i]);
                            //Console.WriteLine("{0} - {1}", currentInstrument, instruments[i]);
                        }
                    }
                }
                buffer.Clear();
            }
        }
    }
}
