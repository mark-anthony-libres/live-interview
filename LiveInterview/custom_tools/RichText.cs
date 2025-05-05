using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LiveInterview.custom_tools
{
    [ToolboxItem(true)]
    public class RichText : RichTextBox
    {
        public RichText()
        {
            this.BorderStyle = BorderStyle.None;
            this.BackColor = SystemColors.Info;
            this.ScrollBars = RichTextBoxScrollBars.None;
            this.TabStop = false;
            this.ReadOnly = true;
            this.ContextMenuStrip = null;

        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            AdjustHeightToContent();
        }




        public void AdjustHeightToContent()
        {
            if (string.IsNullOrEmpty(this.Text))
                return;

            // Measure the size of the text, respecting word-wrap
            var textSize = TextRenderer.MeasureText(this.Text, this.Font, new Size(this.Width, int.MaxValue),
                                                     TextFormatFlags.WordBreak);

            this.Height = textSize.Height;
        }

        private ScrollableControl get_item_panel()
        {

            Control parent = this.Parent;

            while (parent.Name != "itemsPanel")
            {
                parent = parent.Parent;

            }

            if (parent is ScrollableControl itemPanel)
            {
                return itemPanel;
            }

            throw new InvalidOperationException("Parent control named 'itemsPanel' not found or is not a ScrollableControl.");

        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {

            // Get the parent scrollable control (itemPanel)
            ScrollableControl itemPanel = this.get_item_panel();

            // Adjust the scroll position directly
            itemPanel.VerticalScroll.Value = Math.Max(0, Math.Min(
                itemPanel.VerticalScroll.Maximum,
                itemPanel.VerticalScroll.Value - e.Delta
            ));

            // Suppress further processing of the MouseWheel event
            ((HandledMouseEventArgs)e).Handled = true;

        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode == Keys.Down || e.KeyCode == Keys.Up)
            {
                ScrollableControl itemPanel = this.get_item_panel();
                Point caretPosition = this.GetPositionFromCharIndex(this.SelectionStart);
                Point caretScreenPosition = this.PointToScreen(caretPosition);
                Point caretInItemPanel = itemPanel.PointToClient(caretScreenPosition);
                Rectangle caretBounds = new Rectangle(caretInItemPanel, new Size(1, this.Font.Height));

                if (caretInItemPanel.Y < 0 || caretInItemPanel.Y > itemPanel.ClientSize.Height)
                {
                    itemPanel.AutoScrollPosition = new Point(
                        itemPanel.AutoScrollPosition.X,
                        itemPanel.VerticalScroll.Value + caretInItemPanel.Y - (itemPanel.ClientSize.Height / 2)
                    );
                }

            }

        }


    }
}
