﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoteGen
{
    [Export(typeof(IVisualizationPlugin))]
    class PolylineWaveFormVisualization : IVisualizationPlugin
    {
        private PolylineWaveFormControl polylineWaveFormControl = new PolylineWaveFormControl();

        public string Name
        {
            get { return "Polyline WaveForm Visualization"; }
        }

        public object Content
        {
            get { return polylineWaveFormControl; }
        }

        public void OnMaxCalculated(float min, float max)
        {
            polylineWaveFormControl.AddValue(max, min);
        }
    }
}
