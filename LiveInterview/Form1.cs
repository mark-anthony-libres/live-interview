using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using LiveInterview.custom_tools;
using OpenAI;
using static System.Net.Mime.MediaTypeNames;


namespace LiveInterview
{

    class FormKeyboard : GlobalKeyListener
    {
        private readonly HashSet<int> _pressedKeys = new();
        private Task pressRun;
        private CancellationTokenSource _cts;

        private async Task BeforeTriggerAsync(int key, CancellationToken token)
        {
            try
            {
                await Task.Delay(1500, token); // Delay can be adjusted
                Trace.WriteLine(string.Join(",", _pressedKeys));

                if (_pressedKeys.SetEquals(new[] { (int)Keys.LControlKey, (int)Keys.LShiftKey }))
                {
                    _onTrigger?.Invoke(key); // 🔥 Trigger
                }
                else
                {
                    _pressedKeys.Clear(); // Reset if other keys are pressed
                }
            }
            catch (TaskCanceledException)
            {
                // Ignore cancellation
            }
            finally
            {
                pressRun = null;
            }
        }

        protected override void OnKeyDownPress(int key)
        {
            _pressedKeys.Add(key);

            // Cancel previous task if running
            if (pressRun != null && !pressRun.IsCompleted)
            {
                _cts?.Cancel();
            }

            _cts = new CancellationTokenSource();
            pressRun = BeforeTriggerAsync(key, _cts.Token);
        }
        protected override void OnKeyUp(int key)
        {
        }


    }
    public partial class Form1 : Form
    {
        private AudioListener audiolistener;
        private MicListener micListener;
        private bool IsAudioListening = false;
        private bool IsMicListening = false;
        private System.Windows.Forms.Timer resizeTimer;
        private OpenAIClient openAIClient;
        private FormKeyboard formKeyboard;

        public Form1()
        {
            DotNetEnv.Env.Load();

            InitializeComponent();
            this.KeyPreview = true;
            this.openAIClient = new OpenAIClient();
            //this.TopMost = true;


            resizeTimer = new System.Windows.Forms.Timer { Interval = 50 }; // Adjust interval as needed
            resizeTimer.Tick += (s, e) =>
            {
                resizeTimer.Stop();
                PerformItemsPanelResize();
            };
            itemsPanel.Resize += (s, e) =>
            {
                resizeTimer.Stop();
                resizeTimer.Start();
            };


        }

        private void PerformItemsPanelResize()
        {
            itemsPanel.SuspendLayout();
            foreach (Control control in itemsPanel.Controls)
            {
                ItemS per_item = (control as ItemS);
                per_item.main.Width = itemsPanel.Width;
                per_item.Main_Resize(itemsPanel.Width);
            }
            itemsPanel.ResumeLayout();
        }

      


        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.audiolistener = new Listen();
            this.audiolistener.itemPanel = itemsPanel;
            this.audiolistener.when_stop = this.audio_listener_stop;
            this.audiolistener.when_start = this.audio_listener_start;
            this.audiolistener.on_item = this.InsertItem;

            //this.micListener = new MicListener();
            //this.micListener.Start();

            formKeyboard = new FormKeyboard();
            formKeyboard.Start(this.ListenKeyboard);

            this.Form1_Resize(sender, e);

        }



        public void SafeInvoke(Control control, Action action)
        {
            if (control.InvokeRequired)
            {
                control.BeginInvoke(action);
            }
            else
            {
                action();
            }
        }

        private void ListenKeyboard(int keyCode)
        {
            if (this.IsAudioListening)
            {
                this.audiolistener.Stop();
            }
            else
            {
                this.audiolistener.Start();
            }

        }

        private void audio_listener_stop()
        {
            this.SafeInvoke(this, () =>
            {
                this.listen_status.Text = "Not Recording";
                this.listen_status.BackColor = Color.Red;
            });

            this.IsAudioListening = false;
        }

        private void audio_listener_start()
        {

            this.SafeInvoke(this, () =>
            {
                this.listen_status.Text = "Recording...";
                this.listen_status.BackColor = Color.Green;
            });
            this.IsAudioListening = true;
        }

        private void InsertItem(string key, string text, List<byte[]> audiobuffer)
        {

            bool exists = itemsPanel.Controls.ContainsKey(key);

            if (!exists)
            {
                this.SafeInvoke(itemsPanel, () =>
                {
                    Point scrollPos = itemsPanel.AutoScrollPosition;
                    itemsPanel.SuspendLayout();

                    ItemS transcribeItem = new ItemS();
                    transcribeItem.Name = key;
                    transcribeItem.Dock = DockStyle.Top;
                    transcribeItem.AutoSize = true;
                    transcribeItem.MinimumSize = new Size(911, 92);
                    transcribeItem.header_title.Text = text;
                    transcribeItem.body_content.Text = "";
                    transcribeItem.Main_Resize(itemsPanel.Width);
                    this.itemsPanel.Controls.Add(transcribeItem);

                    itemsPanel.ResumeLayout();

                    if (scrollToTopOnNewItem.Checked)
                    {
                        itemsPanel.AutoScrollPosition = new Point(Math.Abs(itemsPanel.AutoScrollPosition.X), 0);
                    }
                    else
                    {
                        itemsPanel.AutoScrollPosition = new Point(Math.Abs(scrollPos.X), Math.Abs(scrollPos.Y));
                    }

                    itemsPanel.ResumeLayout();
                });

                this.GptAnswer(key, text, audiobuffer);

            }
        }
        private async void GptAnswer(string key, string text, List<byte[]> audiobuffer)
        {

            try
            {
                string result = await this.openAIClient.GetGPTResponseWithHistory(text);

                this.SafeInvoke(itemsPanel, () =>
                {
                    Point scrollPos = itemsPanel.AutoScrollPosition;
                    itemsPanel.SuspendLayout();

                    ItemS transcribeItem = itemsPanel.Controls[key] as ItemS;
                    transcribeItem.body_content.Text = result;
                    transcribeItem.header_status.Image = LiveInterview.Properties.Resources.check_stat;
                    transcribeItem.Main_Resize(itemsPanel.Width);
                    //transcribeItem.body_content.GotFocus += (s, e) =>
                    //{
                    //    Trace.WriteLine("Success");
                    //    itemsPanel.Select();
                    //};

                    itemsPanel.ResumeLayout();
                    itemsPanel.AutoScrollPosition = new Point(Math.Abs(scrollPos.X), Math.Abs(scrollPos.Y));

                });

            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error in HandleGptSuggestion: {ex.Message}");
                this.SafeInvoke(itemsPanel, () =>
                {

                    ItemS transcribeItem = itemsPanel.Controls[key] as ItemS;
                    transcribeItem.body_content.Visible = false;


                    Task.Delay(2000).Wait();

                    this.audiolistener.buffers[key] = audiobuffer;
                    this.audiolistener.bufferKeyList.Add(key);

                });
            }


        }

        private void cLabel1_Click(object sender, EventArgs e)
        {

        }

        private void cLabel2_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            itemsPanel.MaximumSize = new Size(this.ClientSize.Width, this.ClientSize.Height - 60);
            itemsPanel.MinimumSize = new Size(this.ClientSize.Width, this.ClientSize.Height - 40);

            Trace.WriteLine($"{itemsPanel.Width} {itemsPanel.Height}");
            Trace.WriteLine($"form {this.Width} {this.Height}");
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {

            formKeyboard.Stop();
            base.OnFormClosed(e);

        }

    }
}
