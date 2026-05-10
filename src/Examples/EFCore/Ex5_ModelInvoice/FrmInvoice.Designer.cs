
namespace Ex4_ModelInvoice
{
    partial class FrmInvoice
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
            btnInvoice = new System.Windows.Forms.Button();
            txtDebug = new System.Windows.Forms.TextBox();
            SuspendLayout();
            // 
            // btnInvoice
            // 
            btnInvoice.Location = new System.Drawing.Point(6, 13);
            btnInvoice.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            btnInvoice.Name = "btnInvoice";
            btnInvoice.Size = new System.Drawing.Size(153, 65);
            btnInvoice.TabIndex = 0;
            btnInvoice.Text = "Create and read back Invoice header and detail";
            btnInvoice.UseVisualStyleBackColor = true;
            btnInvoice.Click += button1_Click;
            // 
            // txtDebug
            // 
            txtDebug.Location = new System.Drawing.Point(163, 13);
            txtDebug.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            txtDebug.Multiline = true;
            txtDebug.Name = "txtDebug";
            txtDebug.Size = new System.Drawing.Size(685, 244);
            txtDebug.TabIndex = 11;
            // 
            // FrmInvoice
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(860, 270);
            Controls.Add(txtDebug);
            Controls.Add(btnInvoice);
            Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            Name = "FrmInvoice";
            Text = "Ex 4: Invoice - Header and Detail";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button btnInvoice;
      private System.Windows.Forms.TextBox txtDebug;
   }
}

