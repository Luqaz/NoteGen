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

namespace NoteGen
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<byte> fileBytes = new List<byte>();
        private SoundPlayer player;
        private string fileName { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void FileOpenHandler(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog();
            fileDialog.Filter = "MP3 File (*.mp3)|*.mp3|FLAC file (*.flac)|*.flac|ALAC file (*.m4a)|*.m4a";
            if(fileDialog.ShowDialog().Value)
            {
                fileName = fileDialog.FileName;
                player = SoundPlayer.getInstance(fileName);
                try
                {
                    using (var fileStream = fileDialog.OpenFile())
                    {
                        if(String.IsNullOrWhiteSpace(player.Info))
                        {
                            fileInfo.Content = fileDialog.SafeFileName;
                        }
                        else
                        {
                            fileInfo.Content = player.Info;
                        }
                        int fileByte = fileStream.ReadByte();
                        while (fileByte != -1)
                        {
                            fileBytes.Add((byte)fileByte);
                            fileByte = fileStream.ReadByte();
                        }
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message + "\n\n\n" + ex.StackTrace);
                }
                
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
    }
}
