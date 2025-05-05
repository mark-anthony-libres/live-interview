using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Design; // Required for design-time support

// Add ToolboxItem for drag-and-drop functionality in the Toolbox


namespace LiveInterview.custom_tools
{

    [ToolboxItem(true)]
    public class CLabel : Label
    {
        public CLabel()
        {
            // Set AutoSize to false so we can control the height
            this.AutoSize = false;
        }

        // Handle text change events and adjust height
        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            AdjustHeightToContent();
        }

        // Adjust height based on content
        public void AdjustHeightToContent()
        {
            if (string.IsNullOrEmpty(this.Text))
                return;

            // Measure the size of the text, respecting word-wrap
            var textSize = TextRenderer.MeasureText(this.Text, this.Font, new Size(this.Width, int.MaxValue),
                                                     TextFormatFlags.WordBreak);

            this.Height = textSize.Height;
        }

        // Make AutoSize property visible in the designer
        [Browsable(true)]
        public override bool AutoSize
        {
            get { return false; }
            set { base.AutoSize = false; }
        }
    }

}