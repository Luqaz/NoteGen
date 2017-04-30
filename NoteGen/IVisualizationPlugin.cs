using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoteGen
{
    public interface IVisualizationPlugin
    {
        string Name { get; }
        object Content { get; }

        void OnMaxCalculated(float min, float max);
    }
}
