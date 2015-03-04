using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace HistoryNav
{
    public partial class Help : Form
    {
        private List<Image> screens = new List<Image>();
        int clickTracker = 0;
        public Help()
        {
            InitializeComponent();
            ChangeScreens();
        }

        private void Help_Load(object sender, EventArgs e)
        {
            pictureBox1.Visible = false;
            clickTracker = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (clickTracker > 0)
            {
                clickTracker--;
            }
            ChangeScreens();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (clickTracker < 4)
            {
                clickTracker++;
            }
            ChangeScreens();
        }
        private void ChangeScreens()
        {
            if (clickTracker == 0)
            {
                textBox1.Visible = true;
                pictureBox1.Visible = false;
            }
            if (clickTracker == 1)
            {
                textBox1.Visible = false;
                textBox2.Visible = false;
                pictureBox1.Visible = true;
                pictureBox1.Image = HistoryNav.Properties.Resources.annotated_first_screen;
            }
            if (clickTracker == 2)
            {
                textBox2.Visible = true;
                pictureBox1.Visible = false;
            }
            if (clickTracker == 3)
            {
                textBox2.Visible = false;
                pictureBox1.Visible = true;
                pictureBox1.Image = HistoryNav.Properties.Resources.annotated_heat_map;
            }
        }
    }
}
