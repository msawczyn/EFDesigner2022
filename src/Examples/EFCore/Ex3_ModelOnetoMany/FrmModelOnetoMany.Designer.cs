
namespace Ex3_ModelOnetoMany
{
    partial class FrmModelOnetoMany
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
            btnRead = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // txtDebug
            // 
            txtDebug.Location = new System.Drawing.Point(156, 13);
            txtDebug.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            txtDebug.Multiline = true;
            txtDebug.Name = "txtDebug";
            txtDebug.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            txtDebug.Size = new System.Drawing.Size(577, 312);
            txtDebug.TabIndex = 12;
            // 
            // btnTestOne2One
            // 
            btnTestOne2One.Location = new System.Drawing.Point(12, 13);
            btnTestOne2One.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            btnTestOne2One.Name = "btnTestOne2One";
            btnTestOne2One.Size = new System.Drawing.Size(132, 47);
            btnTestOne2One.TabIndex = 9;
            btnTestOne2One.Text = "Test One to Many";
            btnTestOne2One.UseVisualStyleBackColor = true;
            btnTestOne2One.Click += btnMany2Many_Click;
            // 
            // btnRead
            // 
            btnRead.Location = new System.Drawing.Point(12, 93);
            btnRead.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            btnRead.Name = "btnRead";
            btnRead.Size = new System.Drawing.Size(132, 47);
            btnRead.TabIndex = 13;
            btnRead.Text = "Read records";
            btnRead.UseVisualStyleBackColor = true;
            btnRead.Click += btnRead_Click;
            // 
            // FrmModelOnetoMany
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(745, 338);
            Controls.Add(btnRead);
            Controls.Add(txtDebug);
            Controls.Add(btnTestOne2One);
            Margin = new System.Windows.Forms.Padding(2);
            Name = "FrmModelOnetoMany";
            Text = "Ex 3: One 2 Many - Authors & Books";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TextBox txtDebug;
      private System.Windows.Forms.Button btnTestOne2One;
        private System.Windows.Forms.Button btnRead;
    }
}