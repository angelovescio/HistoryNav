using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace HistoryNav
{
    public partial class Progress : Form
    {
        int progress = 0;
        public Progress()
        {
            InitializeComponent();
        }
        public void StartProgress()
        {
            progressBar1.Style = ProgressBarStyle.Marquee;
            progressBar1.MarqueeAnimationSpeed = 30;
        }
    }
}
