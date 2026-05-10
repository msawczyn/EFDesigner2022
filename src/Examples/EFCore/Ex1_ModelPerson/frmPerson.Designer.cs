namespace Ex1_ModelPerson
{
    partial class FrmPerson
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
            btnTestPerson = new Button();
            txtDebug = new TextBox();
            SuspendLayout();
            // 
            // btnTestPerson
            // 
            btnTestPerson.Location = new Point(41, 12);
            btnTestPerson.Name = "btnTestPerson";
            btnTestPerson.Size = new Size(75, 23);
            btnTestPerson.TabIndex = 0;
            btnTestPerson.Text = "Test Person";
            btnTestPerson.UseVisualStyleBackColor = true;
            btnTestPerson.Click += btnTestPerson_Click;
            // 
            // txtDebug
            // 
            txtDebug.Location = new Point(159, 12);
            txtDebug.Multiline = true;
            txtDebug.Name = "txtDebug";
            txtDebug.ScrollBars = ScrollBars.Both;
            txtDebug.Size = new Size(321, 329);
            txtDebug.TabIndex = 3;
            // 
            // FrmPerson
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(492, 353);
            Controls.Add(txtDebug);
            Controls.Add(btnTestPerson);
            Name = "FrmPerson";
            Text = "FrmPerson";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnTestPerson;
        private TextBox txtDebug;
    }
}