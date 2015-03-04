using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace HistoryNav
{
    public class Block
    {
        public int X;
        public int Y;
        public Block(int x, int y)
        {
            X = x;
            Y = y;
        }
        public void Draw(ref Panel pnl)
        {
            System.Drawing.SolidBrush myBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Red);
            System.Drawing.Graphics formGraphics = pnl.CreateGraphics();
            formGraphics.FillRectangle(myBrush, new Rectangle(X, Y, 50, 50));
            myBrush.Dispose();
            formGraphics.Dispose();
        }
    }
    public class Connection
    {
        Block A;
        Block B;

        public Connection(Block a, Block b)
        {
            A = a;
            B = b;
        }
        public void Draw(ref Panel pnl)
        {
            Pen myPen;
            myPen = new System.Drawing.Pen(System.Drawing.Color.Red);
            System.Drawing.Graphics formGraphics = pnl.CreateGraphics();
            formGraphics.DrawLine(myPen, A.X, A.Y, B.X, B.Y);
            myPen.Dispose();
            formGraphics.Dispose();
        }
    }
}
