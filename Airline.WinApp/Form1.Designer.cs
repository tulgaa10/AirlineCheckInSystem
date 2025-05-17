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
            txtPassport.Location = new Point(53, 84);
            txtPassport.Name = "txtPassport";
            txtPassport.PlaceholderText = "txtPassport";
            txtPassport.Size = new Size(402, 39);
            txtPassport.TabIndex = 0;
            // 
            // btnSearch
            // 
            btnSearch.Location = new Point(53, 192);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(199, 103);
            btnSearch.TabIndex = 1;
            btnSearch.Text = "btnSearch";
            btnSearch.UseVisualStyleBackColor = true;
            btnSearch.Click += btnSearch_Click_1;
            // 
            // lstSeats
            // 
            lstSeats.FormattingEnabled = true;
            lstSeats.Location = new Point(482, 84);
            lstSeats.Name = "lstSeats";
            lstSeats.Size = new Size(609, 836);
            lstSeats.TabIndex = 2;
            // 
            // btnAssign
            // 
            btnAssign.Location = new Point(53, 367);
            btnAssign.Name = "btnAssign";
            btnAssign.Size = new Size(225, 177);
            btnAssign.TabIndex = 3;
            btnAssign.Text = "btnAssign";
            btnAssign.UseVisualStyleBackColor = true;
            btnAssign.Click += btnAssign_Click_1;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(1228, 18);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(78, 32);
            lblStatus.TabIndex = 4;
            lblStatus.Text = "label1";
            // 
            // btnPrint
            // 
            btnPrint.Location = new Point(53, 663);
            btnPrint.Name = "btnPrint";
            btnPrint.Size = new Size(225, 164);
            btnPrint.TabIndex = 5;
            btnPrint.Text = "btnPrint";
            btnPrint.UseVisualStyleBackColor = true;
            btnPrint.Click += btnPrint_Click_1;
            // 
            // cmbStatus
            // 
            cmbStatus.FormattingEnabled = true;
            cmbStatus.Location = new Point(1179, 168);
            cmbStatus.Name = "cmbStatus";
            cmbStatus.Size = new Size(242, 40);
            cmbStatus.TabIndex = 6;
            cmbStatus.Text = "cmbStatus";
            // 
            // btnUpdate
            // 
            btnUpdate.Location = new Point(1179, 367);
            btnUpdate.Name = "btnUpdate";
            btnUpdate.Size = new Size(146, 126);
            btnUpdate.TabIndex = 7;
            btnUpdate.Text = "btnUpdate";
            btnUpdate.UseVisualStyleBackColor = true;
            btnUpdate.Click += btnUpdate_Click_1;
            // 
            // btnDebug
            // 
            btnDebug.Location = new Point(305, 189);
            btnDebug.Name = "btnDebug";
            btnDebug.Size = new Size(150, 109);
            btnDebug.TabIndex = 8;
            btnDebug.Text = "btnDebug";
            btnDebug.UseVisualStyleBackColor = true;
            btnDebug.Click += btnDebug_Click;
            // 
            // cmbFlights
            // 
            cmbFlights.FormattingEnabled = true;
            cmbFlights.Location = new Point(1179, 84);
            cmbFlights.Name = "cmbFlights";
            cmbFlights.Size = new Size(242, 40);
            cmbFlights.TabIndex = 9;
            cmbFlights.Text = "cmbFlights";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1696, 1221);
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
