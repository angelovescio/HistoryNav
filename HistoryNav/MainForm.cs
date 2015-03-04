using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using DataGridViewAutoFilter;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;

namespace HistoryNav
{
    public partial class MainForm : Form
    {
        public static BackgroundWorker bgWorkScan = new BackgroundWorker();
        public static BackgroundWorker bgWorkData = new BackgroundWorker();
        public static BackgroundWorker bgWorkHeat = new BackgroundWorker();

        public static MainForm win;
        bool isCancelled = false;
        static object Locker = new object();
        public static bool ScanWorkerFinished = false;
        Heat ht;

        public delegate void UpdateTextBlockDelegate(string args);
        public delegate void ToggleProgressDelegate(bool args);
        static int Threadsrunning = 0;
        string slectedDisk = "";
        Mount mnt;
        DataSet ds;
        public MainForm()
        {
            InitializeComponent();
            dataGridView1.BindingContextChanged += new EventHandler(dataGridView1_BindingContextChanged);
            bgWorkScan.DoWork += new DoWorkEventHandler(bgWorkScan_DoWork);
            bgWorkScan.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgWork_ScanRunWorkerCompleted);
            bgWorkScan.WorkerSupportsCancellation = true;

            bgWorkData.DoWork += new DoWorkEventHandler(bgWorkData_DoWork);
            bgWorkData.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgWorkData_RunWorkerCompleted);

            bgWorkHeat.DoWork += new DoWorkEventHandler(bgWorkHeat_DoWork);
            bgWorkHeat.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgWorkHeat_RunWorkerCompleted);

            cmbDrives.SelectedIndexChanged += new EventHandler(cmbDrives_SelectedIndexChanged);
            win = this;

            DriveInfo[] drives = DriveInfo.GetDrives();
            for (int i = 0; i < drives.Length; i++)
            {
                if (drives[i].DriveType == DriveType.Fixed)
                {
                    cmbDrives.Items.Add(drives[i].ToString());
                }
            }

        }
        delegate void HeatClickDelegate(bool enabled);
        void HeatClick(bool enabled)
        {
            if (this.cmdHeat.InvokeRequired)
            {
                this.cmdHeat.Invoke(new HeatClickDelegate(HeatClick), new object[] { enabled });
            }
            else
            {
                cmdHeat.Enabled = enabled;
            }
        }
        void bgWorkHeat_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ToggleProgress(false);

        }
        private string selectionForHeatMap = "";
        void bgWorkHeat_DoWork(object sender, DoWorkEventArgs e)
        {
            ToggleProgress(true);
            DateTime start;
            DateTime end;
            if (dtStart.Value < dtEnd.Value)
            {
                start = dtStart.Value;
                end = dtEnd.Value;
            }
            else
            {
                end = dtStart.Value;
                start = dtEnd.Value;
            }
            start = start.Date;
            end = end.Date;
            end = end.Add(new TimeSpan(1, 0, 0, 0));
            try
            {
                List<HeatPoint> hts = GetMinMaxLine(start, end, selectionForHeatMap);
                ShowHEatForm(hts, start, end);
            }
            catch (NullReferenceException ex)
            {
                MessageBox.Show("An error has occurred when trying to render the map, did you remember to pick a field to search on?",
                    "Uh oh, what happened?", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        private delegate void ShowHeatFormDelegate(List<HeatPoint> hts, DateTime start, DateTime end);
        void ShowHEatForm(List<HeatPoint> hts, DateTime start, DateTime end)
        {
            if (this.ht.InvokeRequired == true)
            {
                this.ht.Invoke(new ShowHeatFormDelegate(ShowHEatForm), new object[] { hts,start,end });
            }
            else
            {
                ht.FillStarterVars(hts, start, end);
                ThreadUpdateTextBocks("New heat map generated: Please be patient, several data points may take several seconds to a few minutes");
                Application.Run(ht);
                ToggleProgress(false);
                HeatClick(true);
            }
        }
        DataGridView dgs;
        void bgWorkData_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.dataGridView1 = dgs;
            FillDataGrids();
            ToggleProgress(false);
        }
        void ToggleProgress(bool start)
        {
            if (progressBar1.InvokeRequired == true)
            {
                this.Invoke(new ToggleProgressDelegate(ToggleProgress), new object[] { start });
            }
            else
            {
                if (start)
                {
                    progressBar1.MarqueeAnimationSpeed = 30;
                    progressBar1.Show();
                }
                else
                {
                    progressBar1.Value = 0;
                    progressBar1.MarqueeAnimationSpeed = 0;
                }
            }
        }
        void bgWorkData_DoWork(object sender, DoWorkEventArgs e)
        {
            ToggleProgress(true);
            dgs = ThreadDsCreate();
            
        }

        void dataGridView1_BindingContextChanged(object sender, EventArgs e)
        {
            if (dataGridView1.DataSource == null) return;

            foreach (DataGridViewColumn col in dataGridView1.Columns)
            {
                col.HeaderCell = new
                    DataGridViewAutoFilterColumnHeaderCell(col.HeaderCell);
            }
            dataGridView1.AutoResizeColumns();
        }
        void cmbDrives_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbDrives.SelectedItem.ToString() != "")
            {
                slectedDisk = cmbDrives.SelectedItem.ToString();
                cmdStart.Enabled = true;
                cmdStart.Text = "Start Scanning "+slectedDisk;
            }
        }
        #region UpdateTextBlocks
        public void UpdateTextBlock(string args)
        {
            txtInfo.AppendText(args);
            txtInfo.AppendText(Environment.NewLine);
        }
        public void ThreadUpdateTextBocks(string args)
        {
            if (this.txtInfo.InvokeRequired)
            {
                this.txtInfo.Invoke(new UpdateTextBlockDelegate(UpdateTextBlock), new object[] { args });
            }
            else
            {
                UpdateTextBlock(args);
            }
        }
        #endregion
        #region scan_events
        void bgWork_ScanRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Monitor.Enter(Locker);
            ScanWorkerFinished = true;
            if (!isCancelled)
            {

                txtInfo.AppendText("HistoryNav has completed successfully at " + DateTime.Now.ToString());
                txtInfo.AppendText(Environment.NewLine);
                //DeployThePackage();

            }
            else
            {
                Threadsrunning--;
                if (Threadsrunning == 0)
                {
                    DeployThePackage();
                }
            }
            Monitor.Exit(Locker);
            progressBar1.MarqueeAnimationSpeed = 0;
            FileInfo fi = new FileInfo("history");
            if (mnt == null)
            {
                mnt = new Mount();
            }
            //mnt.conn.MemoryMerge("history");
            mnt.conn.AttachDB("temp");
            ds = mnt.conn.GetDBTables();
        }

        public void DeployThePackage()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                FileInfo fi = new FileInfo(dlg.FileName);
                if (mnt == null)
                {
                    mnt = new Mount();
                }
                //mnt.conn.AttachDB(dlg.FileName);
                //Database db = new Database("temp2");
                mnt.conn.AttachDB(dlg.FileName);
                ds = mnt.conn.GetDBTables();
            }
            else
            {
                return;
            }
            bgWorkData.RunWorkerAsync();
        }
        void bgWorkScan_DoWork(object sender, DoWorkEventArgs e)
        {
            if (txtInfo.InvokeRequired == true)
            {
                this.Invoke(new UpdateTextBlockDelegate(UpdateTextBlock), new object[] { "Scan worker started " + DateTime.Now.ToString() + " THREAD::" + Thread.CurrentThread.ManagedThreadId });
            }
            else
            {
                txtInfo.AppendText("Scan worker started " + DateTime.Now.ToString() + " THREAD::" + Thread.CurrentThread.ManagedThreadId);
                txtInfo.AppendText(Environment.NewLine);
            }
            mnt = new Mount();
            mnt.RawRead(slectedDisk);
        }
        #endregion
        private DataGridView ThreadDsCreate()
        {
            DataGridView dg = new DataGridView();
            dg.ReadOnly = true;
            dg.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCellsExceptHeader);
            BindingSource bs = new BindingSource();
            bs.DataSource = ds.Tables["nav"];
            dg.DataSource = bs;
            return dg;
        }
        private void FillDataGrids()
        {
            List<string> colNames = new List<string>();
            foreach (Control page in this.Controls)
            {
                if (page.GetType() == typeof(DataGridView))
                {
                    DataGridView dg = (DataGridView)page;
                    dg.ReadOnly = true;
                    dg.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCellsExceptHeader);
                    BindingSource bs = new BindingSource();
                    bs.DataSource = ds.Tables["mft"];
                    dg.DataSource = bs;
                    foreach (DataGridViewColumn col in dg.Columns)
                    {
                        if (col.Name.ToLower().Contains("time"))
                        {
                            colNames.Add(col.Name);
                        }
                    }
                }
            }
            foreach (Control page in this.Controls)
            {
                if (page.GetType() == typeof(ListBox))
                {
                    ListBox dg = (ListBox)page;
                    
                    foreach (string col in colNames)
                    {
                        lstFields.Items.Add(col);
                    }
                }
            }
            cmdHeat.Enabled = true;

        }
        private void cmdStart_Click(object sender, EventArgs e)
        {
            cmdStart.Enabled = false;
            progressBar1.Style = ProgressBarStyle.Marquee; 
            progressBar1.MarqueeAnimationSpeed = 30; 
            
            bgWorkScan.RunWorkerAsync();
        }

        public static Point GetPositions(DateTime dt, DateTime start, DateTime end)
        {
            DateTime t = new DateTime(1, 1, 1, 23, 59, 59);
            long dticks = dt.Date.Ticks - start.Date.Ticks;
            long eticks = end.Date.Ticks - start.Date.Ticks;
            // calculateX(dticks);
            int percent = 800 - (int)((((double)eticks - (double)dticks) / (double)eticks) * 800);
            int partOfDay = 600 - (int)((((double)t.Ticks - (double)dt.TimeOfDay.Ticks) / (double)t.Ticks) * 600);
            return new Point(percent, partOfDay);
        }
        int CalculateDateRegionSize(DateTime start, DateTime end)
        {
            TimeSpan t = new TimeSpan(end.Date.Ticks - start.Date.Ticks);
            int retval = Convert.ToInt32(t.TotalDays)+1;
            if (retval < 1)
            {
                retval++;
            }
            return retval;
        }
        /// <summary>
        /// Retrieve max dates for timeline of graph, the list of keyvaluepairs 
        /// represents how many events happened at each distinct position on the timeline
        /// </summary>
        /// <returns>array of min max values</returns>
        public List<HeatPoint> GetMinMaxLine(DateTime start, DateTime end, string columnName)
        {
            List<HeatPoint> heatpoints = new List<HeatPoint>();
            List<Point> pos = new List<Point>();
            List<DateTime> dates = new List<DateTime>();
            int regions = CalculateDateRegionSize(start, end);
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                string s = dr[columnName].ToString();
                DateTime dt = DateTime.Parse(s);
                dates.Add(dt);
            }
            foreach (DateTime dr in dates)
            {
                if (dr >= start && dr <= end)
                {
                    Point p = GetPositions(dr,start,end);
                    byte luminosity = CalculateLumens(dr,dates);
                    HeatPoint ht = new HeatPoint(p.X+(800/regions), p.Y, luminosity,dr);
                    heatpoints.Add(ht);
                }
            }
            return heatpoints;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            cmdHeat.Enabled = false;
            selectionForHeatMap = lstFields.SelectedItem.ToString();
            ht = new Heat();
            bgWorkHeat.RunWorkerAsync();
        }
        private byte CalculateLumens(DateTime dt, List<DateTime> dates)
        {
            byte lumens = 0;
            foreach (DateTime date in dates)
            {
                DateTime start = dt.Subtract(new TimeSpan(0, 10, 0));
                DateTime end = dt.Add(new TimeSpan(0, 10, 0));
                if (dt <= end && dt >= start && lumens < 0xFF)
                {
                    lumens++;
                }
            }
            return lumens;
        }
        private void openRawImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                cmbDrives.Items.Add(new FileInfo(dlg.FileName).FullName);
            }
        }
        
        private void openFileDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeployThePackage();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Version 0.9 - 2012\nDeveloped by \nDark Particle Labs", "About HistoryNav", MessageBoxButtons.OK);
        }

        private void openHelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Help help = new Help();
            help.Show();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void cmdFilter_Click(object sender, EventArgs e)
        {
            string lstFilter = "(";
            if (lstFields.SelectedItem == null)
            {
                lstFilter += "siaaccessTime >= #"+dtStart.Value.Date.ToShortDateString()+"# AND "
                    +"siaaccessTime <= #"+dtEnd.Value.Date.ToShortDateString()+"#)";
            }
            else
            {
                lstFilter += lstFields.SelectedItem.ToString()+" >= #" + dtStart.Value.Date.ToShortDateString() + "# AND "
                    + lstFields.SelectedItem.ToString()+" <= #" + dtEnd.Value.Date.ToShortDateString() + "#)";
            }
            string filter = lstFilter + " AND (";
            string[] exts = txtFilter.Text.Split(new char[] { ',' });
            for(int i = 0; i<exts.Length;i++)
            {
                filter += "filename LIKE '*"+exts[i]+"'";
                if (i == exts.Length - 1)
                {
                    filter += ")";
                }
                else
                {
                    filter += " OR ";
                }
            }
            //(dataGridView1.DataSource as DataTable).DefaultView.RowFilter = filter;
            ds.Tables[0].DefaultView.RowFilter = filter;
            dataGridView1.DataSource = ds.Tables[0].DefaultView;
        }
        private void exportToCsv()
        {
            DataTable temp = ds.Tables[0].Clone();
            foreach (DataRowView drv in ds.Tables[0].DefaultView)
            {
                temp.ImportRow(drv.Row);
            }
            SaveFileDialog sv = new SaveFileDialog();
            if (sv.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Stream fs = sv.OpenFile();
                using (StreamWriter txt = new StreamWriter(fs))
                {
                    StringBuilder sbHead = new StringBuilder();
                    string hWrite = "";
                    foreach (DataColumn col in temp.Columns)
                    {
                        sbHead.Append(col.ColumnName + ",");
                        hWrite = sbHead.ToString();
                        if (hWrite.EndsWith(","))
                        {
                            hWrite = hWrite.Remove(hWrite.Length - 1, 1);
                        }
                    }
                    txt.WriteLine(hWrite);
                    foreach (DataRow row in temp.Rows)
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (DataColumn col in temp.Columns)
                        {
                            sb.Append(row[col].ToString() + ",");
                        }
                        string rowWrite = sb.ToString();
                        if(rowWrite.EndsWith(","))
                        {
                            rowWrite = rowWrite.Remove(rowWrite.Length-1,1);
                        }
                        txt.WriteLine(rowWrite);
                    }
                }
            }
        }

        private void exportCurentViewToCSVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            exportToCsv();
        }
    }
    public class Pair<T, U> : IComparable<Pair<double, double>>
    {
        public Pair()
        {
        }

        public Pair(double first, double second)
        {
            this.First = first;
            this.Second = second;
        }

        public double First { get; set; }
        public double Second { get; set; }

        public int CompareTo(Pair<double, double> other)
        {
            return this.First.CompareTo(other.First);
        }
    };
}
