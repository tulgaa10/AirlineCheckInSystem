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
            SuspendLayout();
            // 
            // txtPassport
            // 
            txtPassport.Location = new Point(154, 149);
            txtPassport.Name = "txtPassport";
            txtPassport.Size = new Size(200, 39);
            txtPassport.TabIndex = 0;
            // 
            // btnSearch
            // 
            btnSearch.Location = new Point(191, 394);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(150, 46);
            btnSearch.TabIndex = 1;
            btnSearch.Text = "btnSearch";
            btnSearch.UseVisualStyleBackColor = true;
            // 
            // lstSeats
            // 
            lstSeats.FormattingEnabled = true;
            lstSeats.Location = new Point(672, 241);
            lstSeats.Name = "lstSeats";
            lstSeats.Size = new Size(240, 164);
            lstSeats.TabIndex = 2;
            // 
            // btnAssign
            // 
            btnAssign.Location = new Point(802, 475);
            btnAssign.Name = "btnAssign";
            btnAssign.Size = new Size(150, 46);
            btnAssign.TabIndex = 3;
            btnAssign.Text = "btnAssign";
            btnAssign.UseVisualStyleBackColor = true;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(913, 119);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(78, 32);
            lblStatus.TabIndex = 4;
            lblStatus.Text = "label1";
            // 
            // btnPrint
            // 
            btnPrint.Location = new Point(1179, 540);
            btnPrint.Name = "btnPrint";
            btnPrint.Size = new Size(150, 46);
            btnPrint.TabIndex = 5;
            btnPrint.Text = "btnPrint";
            btnPrint.UseVisualStyleBackColor = true;
            // 
            // cmbStatus
            // 
            cmbStatus.FormattingEnabled = true;
            cmbStatus.Location = new Point(420, 148);
            cmbStatus.Name = "cmbStatus";
            cmbStatus.Size = new Size(242, 40);
            cmbStatus.TabIndex = 6;
            // 
            // btnUpdate
            // 
            btnUpdate.Location = new Point(632, 475);
            btnUpdate.Name = "btnUpdate";
            btnUpdate.Size = new Size(150, 46);
            btnUpdate.TabIndex = 7;
            btnUpdate.Text = "btnUpdate";
            btnUpdate.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1696, 1221);
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
    }
}
