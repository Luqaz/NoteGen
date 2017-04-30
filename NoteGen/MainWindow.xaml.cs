using CSCore.Codecs;
using Microsoft.Win32;
using SoundProcessor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.ComponentModel.Composition;

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

        public MainWindow()
        {
            InitializeComponent();
            player = new SoundPlayer();
            player.VolumeCalculated += audioGraph_MaximumCalculated;
            player.FftCalculated += audioGraph_FFTCalculated;

            this.visualizations = new List<IVisualizationPlugin> { new PolylineWaveFormVisualization() };
            this.selectedVisualization = this.visualizations.FirstOrDefault();
            presenter.Content = Visualization;
        }

        public IList<IVisualizationPlugin> Visualizations { get { return this.visualizations; } }

        public object Visualization
        {
            get
            {
                return this.selectedVisualization.Content;
            }
        }

        public IVisualizationPlugin SelectedVisualization
        {
            get
            {
                return this.selectedVisualization;
            }
            set
            {
                if (this.selectedVisualization != value)
                {
                    this.selectedVisualization = value;
                }
            }
        }

        private void FileOpenHandler(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog();
            fileDialog.Filter = "MP3 File (*.mp3)|*.mp3|FLAC file (*.flac)|*.flac|ALAC file (*.m4a)|*.m4a";
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

        void audioGraph_MaximumCalculated(object sender, SampleProcessor.MaxSampleEventArgs e)
        {
            this.xValue.Content = e.MaxSample;
            this.yValue.Content = e.MinSample;
            this.SelectedVisualization.OnMaxCalculated(e.MinSample, e.MaxSample);
        }

        void audioGraph_FFTCalculated(object sender, SampleProcessor.FftEventArgs e)
        {

        }
    }
}
