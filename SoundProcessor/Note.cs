using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundProcessor
{
    public class Note
    {
        public String Name { get; set; }
        public Double Frequency { get; set; }

        public Note(String name, double frequency)
        {
            Name = name;
            Frequency = frequency;
        }
    }
}
