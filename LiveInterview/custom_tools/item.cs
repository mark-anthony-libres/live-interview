using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace LiveInterview.custom_tools
{
    public class Item : UserControl
    {
        public Panel main;
        public Panel body;
        public CLabel body_content;
        public Panel header_panel;
        public CHeader header_title;
        public PictureBox header_status;

        public Item()
        {
            InitializeComponent();
            //main.Resize += Main_Resize;
        }

        public void Main_Resize(int itemPanelWidth)
        {
            header_panel.Width = itemPanelWidth;
            //header_panel.MinimumSize = new Size(itemPanelWidth, header_panel.Height);

            body.Width = itemPanelWidth;
            //body.MinimumSize = new Size(itemPanelWidth, body.Height);

            header_title.Width = itemPanelWidth - 68;
            header_title.AdjustHeightToContent();

            header_panel.Height = header_title.Height + header_panel.Padding.Top + header_panel.Padding.Bottom;


            body_content.Width = itemPanelWidth - 50;
            body_content.AdjustHeightToContent();


            Trace.WriteLine("body content : " + body_content.Width + " " +body_content.Height);

            body.Height = body_content.Height + body.Padding.Top + body.Padding.Bottom;


        }

        private void InitializeComponent()
        {
            main = new Panel();
            body = new Panel();
            body_content = new CLabel();
            header_panel = new Panel();
            header_title = new CHeader();
            header_status = new PictureBox();
            main.SuspendLayout();
            body.SuspendLayout();
            header_panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)header_status).BeginInit();
            SuspendLayout();
            // 
            // main
            // 
            main.AutoSize = true;
            main.BackColor = SystemColors.ControlLight;
            main.Controls.Add(body);
            main.Controls.Add(header_panel);
            main.Dock = DockStyle.Top;
            main.Location = new Point(0, 0);
            main.Margin = new Padding(0);
            main.Name = "main";
            main.Padding = new Padding(0, 12, 0, 10);
            main.Size = new Size(918, 80);
            main.TabIndex = 1;
            // 
            // body
            // 
            body.AutoSize = true;
            body.BackColor = SystemColors.ControlLightLight;
            body.Controls.Add(body_content);
            body.Dock = DockStyle.Top;
            body.Location = new Point(0, 43);
            body.Margin = new Padding(2);
            body.Name = "body";
            body.Padding = new Padding(0, 6, 0, 6);
            body.Size = new Size(918, 27);
            body.TabIndex = 5;
            // 
            // body_content
            // 
            body_content.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            body_content.Location = new Point(37, 2);
            body_content.Margin = new Padding(2, 0, 2, 0);
            body_content.Name = "body_content";
            body_content.Size = new Size(846, 19);
            body_content.TabIndex = 2;
            body_content.Text = "Sample text";
            // 
            // header_panel
            // 
            header_panel.AutoSize = true;
            header_panel.BackColor = SystemColors.ControlDark;
            header_panel.Controls.Add(header_title);
            header_panel.Controls.Add(header_status);
            header_panel.Dock = DockStyle.Top;
            header_panel.Location = new Point(0, 12);
            header_panel.Margin = new Padding(2);
            header_panel.Name = "header_panel";
            header_panel.Padding = new Padding(0, 6, 0, 6);
            header_panel.Size = new Size(918, 31);
            header_panel.TabIndex = 4;
            // 
            // header_title
            // 
            header_title.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            header_title.Location = new Point(37, 4);
            header_title.Margin = new Padding(2, 0, 2, 0);
            header_title.Name = "header_title";
            header_title.Size = new Size(850, 21);
            header_title.TabIndex = 3;
            header_title.Text = "Tell me about yourself ?";
            header_title.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // header_status
            // 
            header_status.Dock = DockStyle.Left;
            header_status.Image = Properties.Resources.loading_img;
            header_status.Location = new Point(0, 6);
            header_status.Margin = new Padding(2);
            header_status.Name = "header_status";
            header_status.Padding = new Padding(7, 6, 7, 6);
            header_status.Size = new Size(32, 19);
            header_status.SizeMode = PictureBoxSizeMode.Zoom;
            header_status.TabIndex = 0;
            header_status.TabStop = false;
            // 
            // Item
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(main);
            Margin = new Padding(2);
            Name = "Item";
            Size = new Size(918, 94);
            main.ResumeLayout(false);
            main.PerformLayout();
            body.ResumeLayout(false);
            header_panel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)header_status).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
