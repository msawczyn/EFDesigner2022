
namespace Ex2_ModelOne2One
{
    partial class FrmOne2One
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
            txtDebug = new System.Windows.Forms.TextBox();
            btnTestOne2One = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // txtDebug
            // 
            txtDebug.Location = new System.Drawing.Point(150, 13);
            txtDebug.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            txtDebug.Multiline = true;
            txtDebug.Name = "txtDebug";
            txtDebug.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            txtDebug.Size = new System.Drawing.Size(328, 346);
            txtDebug.TabIndex = 8;
            // 
            // btnTestOne2One
            // 
            btnTestOne2One.Location = new System.Drawing.Point(12, 13);
            btnTestOne2One.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            btnTestOne2One.Name = "btnTestOne2One";
            btnTestOne2One.Size = new System.Drawing.Size(132, 47);
            btnTestOne2One.TabIndex = 5;
            btnTestOne2One.Text = "Test One to One";
            btnTestOne2One.UseVisualStyleBackColor = true;
            btnTestOne2One.Click += btnTestOne2One_Click;
            // 
            // FrmOne2One
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(483, 372);
            Controls.Add(txtDebug);
            Controls.Add(btnTestOne2One);
            Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            Name = "FrmOne2One";
            Text = "Ex 2: One to One - Person & Address";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.TextBox txtDebug;
      private System.Windows.Forms.Button btnTestOne2One;
   }
}

