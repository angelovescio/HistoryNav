using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace HistoryNav
{
    //
    public partial class Heat : Form
    {
        public static List<HeatPoint> HeatPoints;
        
        public Heat()
        {
            
            InitializeComponent();
            
        }
        public void FillStarterVars(List<HeatPoint> hts, DateTime start, DateTime end)
        {
            Start = start;
            End = end;
            HeatPoints = hts;
            lblStart.Text = start.Date.ToShortDateString();
            lblEnd.Text = end.Date.ToShortDateString();
        }
        private void Heat_Load(object sender, EventArgs e)
        {
            pictureBox1.MouseClick += new MouseEventHandler(pictureBox1_MouseClick);
            // Create new memory bitmap the same size as the picture box
            Bitmap bMap = new Bitmap(pictureBox1.Width, pictureBox1.Height);

            // Initialize random number generator
            Random rRand = new Random();

            // Call CreateIntensityMask, give it the memory bitmap, and store the result back in the memory bitmap
            bMap = CreateIntensityMask(bMap, HeatPoints);

            // Colorize the memory bitmap and assign it as the picture boxes image
            pictureBox1.Image = Colorize(bMap, 255);
        }
        public string GetDateFromPosition(Point p)
        {
            DateTime dt;
            TimeSpan t = new TimeSpan(23, 59, 59);

            double timepercent = ((((double)pictureBox1.Height-(double)p.Y)) / (double)pictureBox1.Height);
            double timeinterimTicks = ((double)t.Ticks) * timepercent;
            t = new TimeSpan((long)timeinterimTicks);

            double percent = (((double)p.X) / (double)pictureBox1.Width);
            double interimTicks = ((double)End.Date.Ticks - (double)Start.Date.Ticks) * percent;

            double newTicks = Start.Date.Ticks + interimTicks; //+timeinterimTicks;
            dt = new DateTime((long)newTicks);
            
            return DateTime.Parse(dt.Date.ToShortDateString() + " " +t.ToString()).ToString();
        }
        void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                Label lbl = new Label();
                int lblWt = 180;
                int lblHt = 30;
                lbl.Margin = new Padding(0);
                pictureBox1.Controls.Add(lbl);
                lbl.Font = new System.Drawing.Font("Helvetica", 12, FontStyle.Regular);
                lbl.TextAlign = ContentAlignment.MiddleCenter;
                //DateTime dt =
                lbl.Text = GetDateFromPosition(e.Location);
                int x = 0;
                int y = 0;
                if (((MouseEventArgs)e).X + lblWt > pictureBox1.Width)
                {
                    x = ((MouseEventArgs)e).X - lblWt;
                }
                else
                {
                    x= ((MouseEventArgs)e).X;
                }
                if (((MouseEventArgs)e).Y + lblHt > pictureBox1.Height)
                {
                    y = ((MouseEventArgs)e).Y - lblHt;
                }
                else
                {
                    y = ((MouseEventArgs)e).Y;
                }
                lbl.SetBounds(x, y, lblWt, lblHt);
                lbl.Show();
                pictureBox1.Update();
            }
            else
            {
                int w = (this.pictureBox1.Controls.Count - 1);
                for (; w >= 0; w--)
                {
                    this.pictureBox1.Controls.Remove(this.pictureBox1.Controls[w]);
                }
            }
        }
        private double ConvertDegreesToRadians(double degrees)
        {
            double radians = (Math.PI / 180) * degrees;
            return (radians);
        }

        private Bitmap CreateIntensityMask(Bitmap bSurface, List<HeatPoint> aHeatPoints)
        {
            // Create new graphics surface from memory bitmap
            Graphics DrawSurface = Graphics.FromImage(bSurface);

            // Set background color to white so that pixels can be correctly colorized
            DrawSurface.Clear(Color.White);

            // Traverse heat point data and draw masks for each heat point
            foreach (HeatPoint DataPoint in aHeatPoints)
            {
                // Render current heat point on draw surface
                DrawHeatPoint(DrawSurface, DataPoint, 15);
            }

            return bSurface;
        }
        
        private void DrawHeatPoint(Graphics Canvas, HeatPoint HeatPoint, int Radius)
        {
            // Create points generic list of points to hold circumference points
            List<Point> CircumferencePointsList = new List<Point>();

            // Create an empty point to predefine the point struct used in the circumference loop
            Point CircumferencePoint;

            // Create an empty array that will be populated with points from the generic list
            Point[] CircumferencePointsArray;

            // Calculate ratio to scale byte intensity range from 0-255 to 0-1
            float fRatio = 1F / Byte.MaxValue;
            // Precalulate half of byte max value
            byte bHalf = Byte.MaxValue / 2;
            // Flip intensity on it's center value from low-high to high-low
            int iIntensity = (byte)(HeatPoint.Intensity - ((HeatPoint.Intensity - bHalf) * 2));
            // Store scaled and flipped intensity value for use with gradient center location
            float fIntensity = iIntensity * fRatio;

            // Loop through all angles of a circle
            // Define loop variable as a double to prevent casting in each iteration
            // Iterate through loop on 10 degree deltas, this can change to improve performance
            for (double i = 0; i <= 360; i += 10)
            {
                // Replace last iteration point with new empty point struct
                CircumferencePoint = new Point();
                // Plot new point on the circumference of a circle of the defined radius
                // Using the point coordinates, radius, and angle
                // Calculate the position of this iterations point on the circle
                CircumferencePoint.X = Convert.ToInt32(HeatPoint.X + Radius * Math.Cos(ConvertDegreesToRadians(i)));
                CircumferencePoint.Y = pictureBox1.Height - Convert.ToInt32(HeatPoint.Y + Radius * Math.Sin(ConvertDegreesToRadians(i)));
                // Add newly plotted circumference point to generic point list
                CircumferencePointsList.Add(CircumferencePoint);
            }

            // Populate empty points system array from generic points array list
            // Do this to satisfy the datatype of the PathGradientBrush and FillPolygon methods
            CircumferencePointsArray = CircumferencePointsList.ToArray();

            // Create new PathGradientBrush to create a radial gradient using the circumference points
            PathGradientBrush GradientShaper = new PathGradientBrush(CircumferencePointsArray);
            // Create new color blend to tell the PathGradientBrush what colors to use and where to put them
            ColorBlend GradientSpecifications = new ColorBlend(3);

            // Define positions of gradient colors, use intesity to adjust the middle color to
            // show more mask or less mask
            GradientSpecifications.Positions = new float[3] { 0, fIntensity, 1 };
            // Define gradient colors and their alpha values, adjust alpha of gradient colors to match intensity
            GradientSpecifications.Colors = new Color[3]
            {
            Color.FromArgb(0, Color.White),
            Color.FromArgb(HeatPoint.Intensity, Color.Black),
            Color.FromArgb(HeatPoint.Intensity, Color.Black)
            };

            // Pass off color blend to PathGradientBrush to instruct it how to generate the gradient
            GradientShaper.InterpolationColors = GradientSpecifications;
            // Draw polygon (circle) using our point array and gradient brush
            Canvas.FillPolygon(GradientShaper, CircumferencePointsArray);
            foreach (HeatPoint pt in HeatPoints)
            {
                Point h1 = new Point((int)pt.X - 5, pictureBox1.Height-(int)pt.Y);
                Point v1 = new Point((int)pt.X, pictureBox1.Height - (int)pt.Y - 5);
                Point h2 = new Point((int)pt.X + 5, pictureBox1.Height - (int)pt.Y);
                Point v2 = new Point((int)pt.X, pictureBox1.Height - (int)pt.Y + 5);
                Pen p = new Pen(Color.DarkCyan, 1);
                Canvas.DrawLine(p, h1, h2);
                Canvas.DrawLine(p, v1, v2);
            }
        }
        public static Bitmap Colorize(Bitmap Mask, byte Alpha)
        {
            // Create new bitmap to act as a work surface for the colorization process
            Bitmap Output = new Bitmap(Mask.Width, Mask.Height, PixelFormat.Format32bppArgb);

            // Create a graphics object from our memory bitmap so we can draw on it and clear it's drawing surface
            Graphics Surface = Graphics.FromImage(Output);
            Surface.Clear(Color.Transparent);

            // Build an array of color mappings to remap our greyscale mask to full color
            // Accept an alpha byte to specify the transparancy of the output image
            ColorMap[] Colors = CreatePaletteIndex(Alpha);

            // Create new image attributes class to handle the color remappings
            // Inject our color map array to instruct the image attributes class how to do the colorization
            ImageAttributes Remapper = new ImageAttributes();
            Remapper.SetRemapTable(Colors);

            // Draw our mask onto our memory bitmap work surface using the new color mapping scheme
            Surface.DrawImage(Mask, new Rectangle(0, 0, Mask.Width, Mask.Height), 0, 0, Mask.Width, Mask.Height, GraphicsUnit.Pixel, Remapper);

            // Send back newly colorized memory bitmap
            return Output;
        }

        private static ColorMap[] CreatePaletteIndex(byte Alpha)
        {
            ColorMap[] OutputMap = new ColorMap[256];

            // Change this path to wherever you saved the palette image.
            Bitmap Palette = new Bitmap(HistoryNav.Properties.Resources.palette);

            // Loop through each pixel and create a new color mapping
            for (int X = 0; X <= 255; X++)
            {
                OutputMap[X] = new ColorMap();
                OutputMap[X].OldColor = Color.FromArgb(X, X, X);
                OutputMap[X].NewColor = Color.FromArgb(Alpha, Palette.GetPixel(X, 0));
            }

            return OutputMap;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }
    }
    public class HeatPoint : IComparable<HeatPoint>
    {
        public double X;
        public double Y;
        public byte Intensity;
        public DateTime occurence;
        public HeatPoint(double iX, double iY, byte bIntensity, DateTime occur)
        {
            X = iX;
            Y = iY;
            occurence = occur;
            Intensity = bIntensity;
        }

        public int CompareTo(HeatPoint other)
        {
            int comp = this.X.CompareTo(other.X);
            if (comp == 0)
            {
                return this.Y.CompareTo(other.Y);
            }
            return comp;
        }
    }
    
}
