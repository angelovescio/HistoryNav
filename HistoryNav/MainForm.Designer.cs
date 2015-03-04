namespace HistoryNav
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txtInfo = new System.Windows.Forms.TextBox();
            this.cmdStart = new System.Windows.Forms.Button();
            this.cmbDrives = new System.Windows.Forms.ComboBox();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.dtStart = new System.Windows.Forms.DateTimePicker();
            this.dtEnd = new System.Windows.Forms.DateTimePicker();
            this.lstFields = new System.Windows.Forms.ListBox();
            this.cmdHeat = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openRawImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileDatabaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openHelpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.cmdFilter = new System.Windows.Forms.Button();
            this.txtFilter = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.exportCurentViewToCSVToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtInfo
            // 
            this.txtInfo.Location = new System.Drawing.Point(3, 27);
            this.txtInfo.Multiline = true;
            this.txtInfo.Name = "txtInfo";
            this.txtInfo.Size = new System.Drawing.Size(259, 79);
            this.txtInfo.TabIndex = 0;
            // 
            // cmdStart
            // 
            this.cmdStart.Enabled = false;
            this.cmdStart.Location = new System.Drawing.Point(3, 139);
            this.cmdStart.Name = "cmdStart";
            this.cmdStart.Size = new System.Drawing.Size(135, 45);
            this.cmdStart.TabIndex = 1;
            this.cmdStart.Text = "Start Disk Parse";
            this.cmdStart.UseVisualStyleBackColor = true;
            this.cmdStart.Click += new System.EventHandler(this.cmdStart_Click);
            // 
            // cmbDrives
            // 
            this.cmbDrives.FormattingEnabled = true;
            this.cmbDrives.Location = new System.Drawing.Point(2, 112);
            this.cmbDrives.Name = "cmbDrives";
            this.cmbDrives.Size = new System.Drawing.Size(259, 21);
            this.cmbDrives.TabIndex = 2;
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToOrderColumns = true;
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(269, 27);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.Size = new System.Drawing.Size(663, 570);
            this.dataGridView1.TabIndex = 4;
            // 
            // dtStart
            // 
            this.dtStart.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.dtStart.Location = new System.Drawing.Point(3, 385);
            this.dtStart.Name = "dtStart";
            this.dtStart.Size = new System.Drawing.Size(260, 20);
            this.dtStart.TabIndex = 6;
            // 
            // dtEnd
            // 
            this.dtEnd.Location = new System.Drawing.Point(3, 411);
            this.dtEnd.Name = "dtEnd";
            this.dtEnd.Size = new System.Drawing.Size(259, 20);
            this.dtEnd.TabIndex = 7;
            // 
            // lstFields
            // 
            this.lstFields.FormattingEnabled = true;
            this.lstFields.Location = new System.Drawing.Point(3, 437);
            this.lstFields.Name = "lstFields";
            this.lstFields.Size = new System.Drawing.Size(134, 160);
            this.lstFields.TabIndex = 8;
            // 
            // cmdHeat
            // 
            this.cmdHeat.Enabled = false;
            this.cmdHeat.Location = new System.Drawing.Point(143, 437);
            this.cmdHeat.Name = "cmdHeat";
            this.cmdHeat.Size = new System.Drawing.Size(120, 160);
            this.cmdHeat.TabIndex = 9;
            this.cmdHeat.Text = "Show Heat Map for Selected Dates";
            this.cmdHeat.UseVisualStyleBackColor = true;
            this.cmdHeat.Click += new System.EventHandler(this.button1_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(932, 24);
            this.menuStrip1.TabIndex = 10;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openRawImageToolStripMenuItem,
            this.openFileDatabaseToolStripMenuItem,
            this.exportCurentViewToCSVToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openRawImageToolStripMenuItem
            // 
            this.openRawImageToolStripMenuItem.Name = "openRawImageToolStripMenuItem";
            this.openRawImageToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this.openRawImageToolStripMenuItem.Text = "Open Raw Image";
            this.openRawImageToolStripMenuItem.Click += new System.EventHandler(this.openRawImageToolStripMenuItem_Click);
            // 
            // openFileDatabaseToolStripMenuItem
            // 
            this.openFileDatabaseToolStripMenuItem.Name = "openFileDatabaseToolStripMenuItem";
            this.openFileDatabaseToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this.openFileDatabaseToolStripMenuItem.Text = "Open File Database";
            this.openFileDatabaseToolStripMenuItem.Click += new System.EventHandler(this.openFileDatabaseToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openHelpToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // openHelpToolStripMenuItem
            // 
            this.openHelpToolStripMenuItem.Name = "openHelpToolStripMenuItem";
            this.openHelpToolStripMenuItem.Size = new System.Drawing.Size(131, 22);
            this.openHelpToolStripMenuItem.Text = "Open Help";
            this.openHelpToolStripMenuItem.Click += new System.EventHandler(this.openHelpToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(131, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(5, 190);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(258, 24);
            this.progressBar1.TabIndex = 11;
            // 
            // cmdFilter
            // 
            this.cmdFilter.Location = new System.Drawing.Point(2, 295);
            this.cmdFilter.Name = "cmdFilter";
            this.cmdFilter.Size = new System.Drawing.Size(143, 45);
            this.cmdFilter.TabIndex = 12;
            this.cmdFilter.Text = "Filter Files";
            this.cmdFilter.UseVisualStyleBackColor = true;
            this.cmdFilter.Click += new System.EventHandler(this.cmdFilter_Click);
            // 
            // txtFilter
            // 
            this.txtFilter.Location = new System.Drawing.Point(151, 254);
            this.txtFilter.Multiline = true;
            this.txtFilter.Name = "txtFilter";
            this.txtFilter.Size = new System.Drawing.Size(112, 86);
            this.txtFilter.TabIndex = 13;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(2, 254);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(146, 39);
            this.label1.TabIndex = 14;
            this.label1.Text = "Filter based on file extension. \r\nSeparate multiple extensions \r\nwith a comma.";
            // 
            // exportCurentViewToCSVToolStripMenuItem
            // 
            this.exportCurentViewToCSVToolStripMenuItem.Name = "exportCurentViewToCSVToolStripMenuItem";
            this.exportCurentViewToCSVToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.exportCurentViewToCSVToolStripMenuItem.Text = "Export Curent View to CSV";
            this.exportCurentViewToCSVToolStripMenuItem.Click += new System.EventHandler(this.exportCurentViewToCSVToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(932, 601);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtFilter);
            this.Controls.Add(this.cmdFilter);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.cmdHeat);
            this.Controls.Add(this.lstFields);
            this.Controls.Add(this.dtEnd);
            this.Controls.Add(this.dtStart);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.cmbDrives);
            this.Controls.Add(this.cmdStart);
            this.Controls.Add(this.txtInfo);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "HistoryNav";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtInfo;
        private System.Windows.Forms.Button cmdStart;
        private System.Windows.Forms.ComboBox cmbDrives;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DateTimePicker dtStart;
        private System.Windows.Forms.DateTimePicker dtEnd;
        private System.Windows.Forms.ListBox lstFields;
        private System.Windows.Forms.Button cmdHeat;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openRawImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openHelpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openFileDatabaseToolStripMenuItem;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Button cmdFilter;
        private System.Windows.Forms.TextBox txtFilter;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolStripMenuItem exportCurentViewToCSVToolStripMenuItem;
    }
}

