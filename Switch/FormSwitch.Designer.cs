namespace Switch
{
    partial class FormSwitch
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
            this.btnOn = new System.Windows.Forms.Button();
            this.btnOff = new System.Windows.Forms.Button();
            this.textBoxContainerName = new System.Windows.Forms.TextBox();
            this.textBoxAppName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.postButton = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxAppContainer = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.textBoxCreateContainer = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOn
            // 
            this.btnOn.Location = new System.Drawing.Point(48, 131);
            this.btnOn.Name = "btnOn";
            this.btnOn.Size = new System.Drawing.Size(174, 75);
            this.btnOn.TabIndex = 0;
            this.btnOn.Text = "ON";
            this.btnOn.UseVisualStyleBackColor = true;
            this.btnOn.Click += new System.EventHandler(this.btnOn_Click);
            // 
            // btnOff
            // 
            this.btnOff.Location = new System.Drawing.Point(48, 226);
            this.btnOff.Name = "btnOff";
            this.btnOff.Size = new System.Drawing.Size(174, 75);
            this.btnOff.TabIndex = 1;
            this.btnOff.Text = "OFF";
            this.btnOff.UseVisualStyleBackColor = true;
            this.btnOff.Click += new System.EventHandler(this.btnOff_Click);
            // 
            // textBoxContainerName
            // 
            this.textBoxContainerName.Location = new System.Drawing.Point(142, 41);
            this.textBoxContainerName.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBoxContainerName.Name = "textBoxContainerName";
            this.textBoxContainerName.Size = new System.Drawing.Size(107, 20);
            this.textBoxContainerName.TabIndex = 2;
            // 
            // textBoxAppName
            // 
            this.textBoxAppName.Location = new System.Drawing.Point(142, 80);
            this.textBoxAppName.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBoxAppName.Name = "textBoxAppName";
            this.textBoxAppName.Size = new System.Drawing.Size(107, 20);
            this.textBoxAppName.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(24, 43);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Container Name";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(24, 82);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "App Name";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.postButton);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.textBoxAppContainer);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.textBoxCreateContainer);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.textBoxAppName);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.textBoxContainerName);
            this.panel1.Location = new System.Drawing.Point(272, 20);
            this.panel1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(263, 406);
            this.panel1.TabIndex = 6;
            // 
            // postButton
            // 
            this.postButton.Location = new System.Drawing.Point(78, 344);
            this.postButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.postButton.Name = "postButton";
            this.postButton.Size = new System.Drawing.Size(103, 19);
            this.postButton.TabIndex = 12;
            this.postButton.Text = "Create/Post Container";
            this.postButton.UseVisualStyleBackColor = true;
            this.postButton.Click += new System.EventHandler(this.postButton_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(92, 237);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(86, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "Create Container";
            this.label4.Click += new System.EventHandler(this.label4_Click);
            // 
            // textBoxAppContainer
            // 
            this.textBoxAppContainer.Location = new System.Drawing.Point(142, 302);
            this.textBoxAppContainer.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBoxAppContainer.Name = "textBoxAppContainer";
            this.textBoxAppContainer.Size = new System.Drawing.Size(107, 20);
            this.textBoxAppContainer.TabIndex = 8;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(24, 305);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(57, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "App Name";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(24, 266);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(83, 13);
            this.label6.TabIndex = 9;
            this.label6.Text = "Container Name";
            // 
            // textBoxCreateContainer
            // 
            this.textBoxCreateContainer.Location = new System.Drawing.Point(142, 263);
            this.textBoxCreateContainer.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBoxCreateContainer.Name = "textBoxCreateContainer";
            this.textBoxCreateContainer.Size = new System.Drawing.Size(107, 20);
            this.textBoxCreateContainer.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(102, 13);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(47, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Connect";
            // 
            // FormSwitch
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(569, 450);
            this.Controls.Add(this.btnOff);
            this.Controls.Add(this.btnOn);
            this.Controls.Add(this.panel1);
            this.Name = "FormSwitch";
            this.Text = "Switch";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnOn;
        private System.Windows.Forms.Button btnOff;
        private System.Windows.Forms.TextBox textBoxContainerName;
        private System.Windows.Forms.TextBox textBoxAppName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBoxAppContainer;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBoxCreateContainer;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button postButton;
    }
}

