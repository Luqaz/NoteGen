using Microsoft.Win32;
using SoundProcessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.ComponentModel;

namespace NoteGen
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SoundPlayer player;
        private string fileName { get; set; }
        private List<IVisualizationPlugin> visualizations;
        private IVisualizationPlugin selectedVisualization;
        private List<List<double>> buffer = new List<List<double>>();
        private List<String> instruments = new List<string> { "Acoustic Guitar", "Electric Guitar", "Drums", "Piano", "Violin", "Flute" };
        private NeuroNetwork.NeuroNetwork network; 

        public MainWindow()
        {
            InitializeComponent();
            network = new NeuroNetwork.NeuroNetwork(instruments.Count, "weights2");
            network.SaveWeights("weights");
            player = new SoundPlayer();
            player.FftCalculated += audioGraph_FFTCalculated;

            this.visualizations = new List<IVisualizationPlugin> { new PolylineWaveFormVisualization() };
            this.selectedVisualization = this.visualizations.FirstOrDefault();
        }

        private void FileOpenHandler(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog();
            fileDialog.Filter = "MP3 File (*.mp3)|*.mp3|FLAC file (*.flac)|*.flac|ALAC file (*.m4a)|*.m4a|OGG file (*.ogg)|*.ogg|WAV file (*.wav)|*.wav";
            if(fileDialog.ShowDialog().Value)
            {
                fileName = fileDialog.FileName;
                player.Load(fileName);                
            }
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            if(player != null)
            {
                if(player.Status == "Playing")
                {
                    playButton.Content = "Play";
                    player.Stop();
                }
                else
                {
                    playButton.Content = "Pause";
                    player.Play();
                }
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            player.Stop();
        }
        NoteClassifier classifier = new NoteClassifier();
        void audioGraph_FFTCalculated(object sender, SampleProcessor.FftEventArgs e)
        {
            var gainer = new MFCCGainer(26, e.Result.Count, e.SampleRate);
            buffer.Add(gainer.getCoefficents(e.Result));
            if (buffer.Count == 15)
            {
                var notes = classifier.getNotes(e.Result);
                var instrumentsValues = network.GetResults(buffer);
                notesLabel.Content = String.Join("\n", notes);
                String instrs = "";
                for(int i = 0; i < instrumentsValues.Count; i++)
                {
                        instrs += String.Format("{0} - {1}\n", instruments[i], instrumentsValues[i]);
                }
                instrumentsLabel.Content = instrs;
                buffer.RemoveAt(0);
            }
        }
    }
}
