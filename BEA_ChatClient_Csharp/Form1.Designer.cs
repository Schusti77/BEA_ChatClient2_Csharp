namespace BEA_ChatClient_Csharp
{
    partial class Form1
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.ChannelListe = new System.Windows.Forms.CheckedListBox();
            this.Benutzerliste = new System.Windows.Forms.ListBox();
            this.Chatverlauf = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txt_send = new System.Windows.Forms.TextBox();
            this.btn_Send = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ChannelListe
            // 
            this.ChannelListe.FormattingEnabled = true;
            this.ChannelListe.Location = new System.Drawing.Point(490, 44);
            this.ChannelListe.Name = "ChannelListe";
            this.ChannelListe.Size = new System.Drawing.Size(120, 124);
            this.ChannelListe.TabIndex = 0;
            // 
            // Benutzerliste
            // 
            this.Benutzerliste.FormattingEnabled = true;
            this.Benutzerliste.Location = new System.Drawing.Point(490, 195);
            this.Benutzerliste.Name = "Benutzerliste";
            this.Benutzerliste.Size = new System.Drawing.Size(120, 160);
            this.Benutzerliste.TabIndex = 1;
            // 
            // Chatverlauf
            // 
            this.Chatverlauf.Enabled = false;
            this.Chatverlauf.FormattingEnabled = true;
            this.Chatverlauf.Location = new System.Drawing.Point(13, 39);
            this.Chatverlauf.Name = "Chatverlauf";
            this.Chatverlauf.Size = new System.Drawing.Size(471, 316);
            this.Chatverlauf.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(490, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Chaträume";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(490, 179);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(107, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "anwesende Benutzer";
            // 
            // txt_send
            // 
            this.txt_send.Enabled = false;
            this.txt_send.Location = new System.Drawing.Point(12, 361);
            this.txt_send.Name = "txt_send";
            this.txt_send.Size = new System.Drawing.Size(472, 20);
            this.txt_send.TabIndex = 5;
            // 
            // btn_Send
            // 
            this.btn_Send.Enabled = false;
            this.btn_Send.Location = new System.Drawing.Point(490, 358);
            this.btn_Send.Name = "btn_Send";
            this.btn_Send.Size = new System.Drawing.Size(120, 23);
            this.btn_Send.TabIndex = 6;
            this.btn_Send.Text = "senden";
            this.btn_Send.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Server:";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(59, 6);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(194, 20);
            this.textBox1.TabIndex = 8;
            this.textBox1.Text = "localhost";
            this.textBox1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(259, 4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 9;
            this.button1.Text = "connect";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(622, 392);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btn_Send);
            this.Controls.Add(this.txt_send);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Chatverlauf);
            this.Controls.Add(this.Benutzerliste);
            this.Controls.Add(this.ChannelListe);
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckedListBox ChannelListe;
        private System.Windows.Forms.ListBox Benutzerliste;
        private System.Windows.Forms.ListBox Chatverlauf;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txt_send;
        private System.Windows.Forms.Button btn_Send;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button button1;
    }
}

