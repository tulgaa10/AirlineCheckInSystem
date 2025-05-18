namespace Airline.WinApp
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            txtPassport = new TextBox();
            btnSearch = new Button();
            lstSeats = new ListBox();
            btnAssign = new Button();
            lblStatus = new Label();
            btnPrint = new Button();
            cmbStatus = new ComboBox();
            btnUpdate = new Button();
            btnDebug = new Button();
            cmbFlights = new ComboBox();
            SuspendLayout();
            // 
            // txtPassport
            // 
            txtPassport.Location = new Point(33, 52);
            txtPassport.Margin = new Padding(2);
            txtPassport.Name = "txtPassport";
            txtPassport.PlaceholderText = "txtPassport";
            txtPassport.Size = new Size(249, 27);
            txtPassport.TabIndex = 0;
            // 
            // btnSearch
            // 
            btnSearch.BackColor = Color.DarkGray;
            btnSearch.Location = new Point(33, 120);
            btnSearch.Margin = new Padding(2);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(122, 42);
            btnSearch.TabIndex = 1;
            btnSearch.Text = "Search";
            btnSearch.UseVisualStyleBackColor = false;
            btnSearch.Click += btnSearch_Click_1;
            // 
            // lstSeats
            // 
            lstSeats.FormattingEnabled = true;
            lstSeats.Location = new Point(560, 52);
            lstSeats.Margin = new Padding(2);
            lstSeats.Name = "lstSeats";
            lstSeats.Size = new Size(376, 524);
            lstSeats.TabIndex = 2;
            // 
            // btnAssign
            // 
            btnAssign.BackColor = SystemColors.ActiveBorder;
            btnAssign.Location = new Point(33, 181);
            btnAssign.Margin = new Padding(2);
            btnAssign.Name = "btnAssign";
            btnAssign.Size = new Size(122, 36);
            btnAssign.TabIndex = 3;
            btnAssign.Text = "Assign";
            btnAssign.UseVisualStyleBackColor = false;
            btnAssign.Click += btnAssign_Click_1;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(347, 59);
            lblStatus.Margin = new Padding(2, 0, 2, 0);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(50, 20);
            lblStatus.TabIndex = 4;
            lblStatus.Text = "label1";
            // 
            // btnPrint
            // 
            btnPrint.BackColor = SystemColors.ActiveBorder;
            btnPrint.Location = new Point(33, 242);
            btnPrint.Margin = new Padding(2);
            btnPrint.Name = "btnPrint";
            btnPrint.Size = new Size(122, 52);
            btnPrint.TabIndex = 5;
            btnPrint.Text = "Print Flight Information";
            btnPrint.UseVisualStyleBackColor = false;
            btnPrint.Click += btnPrint_Click_1;
            // 
            // cmbStatus
            // 
            cmbStatus.FormattingEnabled = true;
            cmbStatus.Location = new Point(347, 149);
            cmbStatus.Margin = new Padding(2);
            cmbStatus.Name = "cmbStatus";
            cmbStatus.Size = new Size(150, 28);
            cmbStatus.TabIndex = 6;
            cmbStatus.Text = "cmbStatus";
            // 
            // btnUpdate
            // 
            btnUpdate.BackColor = Color.Bisque;
            btnUpdate.Location = new Point(194, 242);
            btnUpdate.Margin = new Padding(2);
            btnUpdate.Name = "btnUpdate";
            btnUpdate.Size = new Size(90, 52);
            btnUpdate.TabIndex = 7;
            btnUpdate.Text = "Update";
            btnUpdate.UseVisualStyleBackColor = false;
            btnUpdate.Click += btnUpdate_Click_1;
            // 
            // btnDebug
            // 
            btnDebug.Location = new Point(192, 120);
            btnDebug.Margin = new Padding(2);
            btnDebug.Name = "btnDebug";
            btnDebug.Size = new Size(92, 42);
            btnDebug.TabIndex = 8;
            btnDebug.Text = "btnDebug";
            btnDebug.UseVisualStyleBackColor = true;
            btnDebug.Click += btnDebug_Click;
            // 
            // cmbFlights
            // 
            cmbFlights.FormattingEnabled = true;
            cmbFlights.Location = new Point(347, 97);
            cmbFlights.Margin = new Padding(2);
            cmbFlights.Name = "cmbFlights";
            cmbFlights.Size = new Size(150, 28);
            cmbFlights.TabIndex = 9;
            cmbFlights.Text = "cmbFlights";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ButtonHighlight;
            ClientSize = new Size(1044, 659);
            Controls.Add(cmbFlights);
            Controls.Add(btnDebug);
            Controls.Add(btnUpdate);
            Controls.Add(cmbStatus);
            Controls.Add(btnPrint);
            Controls.Add(lblStatus);
            Controls.Add(btnAssign);
            Controls.Add(lstSeats);
            Controls.Add(btnSearch);
            Controls.Add(txtPassport);
            Margin = new Padding(2);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtPassport;
        private Button btnSearch;
        private ListBox lstSeats;
        private Button btnAssign;
        private Label lblStatus;
        private Button btnPrint;
        private ComboBox cmbStatus;
        private Button btnUpdate;
        private Button btnDebug;
        private ComboBox cmbFlights;
    }
}
