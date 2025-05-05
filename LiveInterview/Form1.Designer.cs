namespace LiveInterview
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
            itemsPanel = new Panel();
            listen_status = new Label();
            panel1 = new Panel();
            scrollToTopOnNewItem = new CheckBox();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // itemsPanel
            // 
            itemsPanel.AutoScroll = true;
            itemsPanel.AutoSize = true;
            itemsPanel.BackColor = SystemColors.Info;
            itemsPanel.Location = new Point(0, 0);
            itemsPanel.Margin = new Padding(2);
            itemsPanel.Name = "itemsPanel";
            itemsPanel.RightToLeft = RightToLeft.No;
            itemsPanel.Size = new Size(668, 439);
            itemsPanel.TabIndex = 0;
            // 
            // listen_status
            // 
            listen_status.AutoSize = true;
            listen_status.BackColor = Color.Red;
            listen_status.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            listen_status.Location = new Point(12, 8);
            listen_status.Name = "listen_status";
            listen_status.Size = new Size(121, 21);
            listen_status.TabIndex = 1;
            listen_status.Text = "Not Recording";
            // 
            // panel1
            // 
            panel1.Controls.Add(scrollToTopOnNewItem);
            panel1.Controls.Add(listen_status);
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new Point(0, 443);
            panel1.Name = "panel1";
            panel1.Size = new Size(670, 42);
            panel1.TabIndex = 1;
            // 
            // scrollToTopOnNewItem
            // 
            scrollToTopOnNewItem.AutoSize = true;
            scrollToTopOnNewItem.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            scrollToTopOnNewItem.Location = new Point(180, 9);
            scrollToTopOnNewItem.Name = "scrollToTopOnNewItem";
            scrollToTopOnNewItem.Size = new Size(196, 21);
            scrollToTopOnNewItem.TabIndex = 2;
            scrollToTopOnNewItem.Text = "Auto Scroll Top On New Item";
            scrollToTopOnNewItem.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(670, 485);
            Controls.Add(panel1);
            Controls.Add(itemsPanel);
            Margin = new Padding(2);
            MaximizeBox = false;
            Name = "Form1";
            Text = "LiveInterview";
            this.FormClosed += this.Form1_FormClosed;
            this.Load += Form1_Load;
            this.Resize += Form1_Resize;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel itemsPanel;
        private custom_tools.Item item1;
        private custom_tools.Item item2;
        private custom_tools.Item item3;
        private Label listen_status;
        private Panel panel1;
        private CheckBox scrollToTopOnNewItem;
    }
}
