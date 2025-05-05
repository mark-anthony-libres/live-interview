//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using Google.Cloud.Speech.V1;
//using NAudio.Wave;
//using System.Diagnostics;
//using LiveInterview.custom_tools;
//using System.Windows.Forms;

//class AudioListener1
//{
//    private const int Rate = 16000;
//    private const int ChunkSize = 1024;
//    private const int Channels = 1;
//    private const int SilenceThreshold = 1000; // Lower = more sensitive
//    private const double SilenceDuration = 1; // seconds

//    private WaveInEvent waveIn;
//    private List<byte[]> currentBuffer = new List<byte[]>();
//    private List<string> buffer_key_list = new List<string>();
//    private Dictionary<string, List<byte[]>> buffers = new Dictionary<string, List<byte[]>>();
//    private List<List<byte[]>> capturedBuffers = new List<List<byte[]>>();
//    private int silentChunkCounter = 0;
//    private OpenAIClient openAIClient;

//    public Panel itemPanel;


//    private readonly SpeechClient speechClient;

//    public AudioListener1()
//    {

     
//        string outputDirectory = AppDomain.CurrentDomain.BaseDirectory;
//        string credentialsPath = Path.Combine(outputDirectory, "google-project.json");

//        if (!File.Exists(credentialsPath))
//        {
//            throw new FileNotFoundException($"Google credentials file not found at: {credentialsPath}");
//        }

//        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsPath);
//        this.speechClient = SpeechClient.Create();
//        this.openAIClient = new OpenAIClient();
//    }

//    private string GenerateRandomAlphanumeric(int length)
//    {
//        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
//        Random random = new Random();
//        char[] result = new char[length];

//        for (int i = 0; i < length; i++)
//        {
//            result[i] = chars[random.Next(chars.Length)];
//        }

//        return $"item{string.Join("",result)}";
//    }

//    public void Start()
//    {
//        int deviceNumber = this.FindVBCableDevice();

//        this.waveIn = new WaveInEvent
//        {
//            DeviceNumber = deviceNumber,
//            WaveFormat = new WaveFormat(Rate, Channels),
//            BufferMilliseconds = (int)((double)ChunkSize / Rate * 1000)
//        };

//        this.waveIn.DataAvailable += this.OnDataAvailable;
//        this.waveIn.StartRecording();

//        Trace.WriteLine("🎙️ Listening...");

//        Task.Run(this.ListenLoop);
//        Task.Run(this.StartTranscription);

//        // Keep main thread alive
//        while (true)
//        {
//            Thread.Sleep(1000);
//        }
//    }

//    private void OnDataAvailable(object sender, WaveInEventArgs e)
//    {
//        var chunk = new byte[e.BytesRecorded];
//        Array.Copy(e.Buffer, chunk, e.BytesRecorded);

//        this.currentBuffer.Add(chunk);

//        if (this.IsSilent(chunk))
//        {
//            this.silentChunkCounter++;
//        }
//        else
//        {
//            this.silentChunkCounter = 0;
//        }
//    }

//    private void ListenLoop()
//    {
//        Trace.WriteLine("Audio listening service started...");
//        int silenceChunkThreshold = (int)((Rate / ChunkSize) * SilenceDuration);

//        while (true)
//        {
//            //Trace.WriteLine($"{this.silentChunkCounter} > {silenceChunkThreshold}");
//            if (this.silentChunkCounter > silenceChunkThreshold && this.currentBuffer.Count > 0)
//            {
//                Trace.WriteLine($"🛑 Silence detected. Capturing buffer. Buffer Count: {this.buffer_key_list.Count}");

//                lock (this.buffer_key_list)
//                {
//                    string key = this.GenerateRandomAlphanumeric(10);
//                    this.buffers[key] = new List<byte[]>(this.currentBuffer);
//                    this.buffer_key_list.Add(key);

//                }

//                this.currentBuffer.Clear();
//                this.silentChunkCounter = 0;
//            }
//            Thread.Sleep(100);
//        }
//    }


//    private void StartTranscription()
//    {
//        Trace.WriteLine("Transcription service started...");

//        while (true)
//        {
//            if (this.buffer_key_list.Count > 0)
//            {
//                List<byte[]> audioBuffer;
//                string key;

//                lock (this.buffer_key_list)
//                {
//                    key = this.buffer_key_list.First();
//                    audioBuffer = this.buffers[key];

//                    this.buffer_key_list.RemoveAt(0);
//                    this.buffers.Remove(key);
//                }
    
//                this.TranscribeAudio(audioBuffer, key);
//            }

//            //Thread.Sleep(100); // small delay
//        }
//    }

//    // Make sure the update happens on the UI thread
//    private void AddItemToPanel(Item transcribeItem)
//    {
//        if (this.itemPanel.InvokeRequired)
//        {
//            // Asynchronously add the Item control to itemPanel on the UI thread
//            this.itemPanel.BeginInvoke(new Action(() => AddItemToPanel(transcribeItem)));
//        }
//        else
//        {
//            // If we're already on the UI thread, proceed with adding the Item control
//            this.itemPanel.Controls.Add(transcribeItem);
//        }
//    }

//    public void SafeInvoke(Control control, Action action)
//    {
//        if (control.InvokeRequired)
//        {
//            control.BeginInvoke(action);
//        }
//        else
//        {
//            action();
//        }
//    }


//    private void TranscribeAudio(List<byte[]> audioChunks, string key)
//    {
//        var audioBytes = audioChunks.SelectMany(chunk => chunk).ToArray();

//        var recognitionAudio = RecognitionAudio.FromBytes(audioBytes);
//        var config = new RecognitionConfig
//        {
//            Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
//            SampleRateHertz = Rate,
//            LanguageCode = "en-US"
//        };

//        var response = this.speechClient.Recognize(config, recognitionAudio);

//        foreach (var result in response.Results)
//        {
//            var transcript = result.Alternatives.FirstOrDefault()?.Transcript;
//            if (!string.IsNullOrWhiteSpace(transcript))
//            {

//                this.SafeInvoke(this.itemPanel, () =>
//                {
//                    Point scrollPos = itemPanel.AutoScrollPosition;
//                    itemPanel.SuspendLayout();

//                    Item transcribeItem = new Item();
//                    transcribeItem.Name = key;
//                    transcribeItem.Dock = DockStyle.Top;
//                    transcribeItem.AutoSize = true;
//                    //transcribeItem.MinimumSize = new Size(911, 92);
//                    transcribeItem.header_title.Text = transcript;
//                    transcribeItem.body_content.Text = "";
//                    transcribeItem.Main_Resize(this.itemPanel.Width);
//                    this.itemPanel.Controls.Add(transcribeItem);

                    

//                    itemPanel.ResumeLayout();
//                    itemPanel.AutoScrollPosition = new Point(Math.Abs(scrollPos.X), Math.Abs(scrollPos.Y));

//                });


//                Trace.WriteLine($"System audio: {transcript}");
//                this.HandleGptSuggestion(transcript, key);
//            }
//        }
//    }

//    private async void HandleGptSuggestion(string text, string key)
//    {
//        // 🧠 GPT logic can be added here
//        Trace.WriteLine($"(GPT Suggestion placeholder for: {text})");

//        try
//        {
//            string result = await openAIClient.GetGPTResponseWithHistory(text);

//            this.SafeInvoke(this.itemPanel, () =>
//            {
//                Point scrollPos = itemPanel.AutoScrollPosition;
//                itemPanel.SuspendLayout();

//                Item transcribeItem = this.itemPanel.Controls[key] as Item;
//                transcribeItem.body_content.Text = result;
//                transcribeItem.header_status.Image = LiveInterview.Properties.Resources.check_stat;
//                transcribeItem.Main_Resize(this.itemPanel.Width);

//                itemPanel.ResumeLayout();
//                itemPanel.AutoScrollPosition = new Point(Math.Abs(scrollPos.X), Math.Abs(scrollPos.Y));

//            });

//        }
//        catch(Exception ex)
//        {
//            Trace.WriteLine($"Error in HandleGptSuggestion: {ex.Message}");

//            this.SafeInvoke(this.itemPanel, () =>
//            {

//                Item transcribeItem = this.itemPanel.Controls[key] as Item;
//                transcribeItem.body_content.Visible = false;
//                //transcribeItem.header_status.Image = LiveInterview.Properties.Resources.error_stat;

//            });
//        }

        

//    }

//    private bool IsSilent(byte[] chunk)
//    {
//        double sumSquares = 0;
//        int sampleCount = chunk.Length / 2; // 2 bytes per sample (16-bit)

//        for (int i = 0; i < chunk.Length; i += 2)
//        {
//            short sample = BitConverter.ToInt16(chunk, i);
//            sumSquares += sample * sample;
//        }

//        double rms = Math.Sqrt(sumSquares / sampleCount);

//        //Trace.WriteLine($"RMS: {rms}");

//        return rms < SilenceThreshold;
//    }


//    private int FindVBCableDevice()
//    {
//        for (int i = 0; i < WaveIn.DeviceCount; i++)
//        {
//            var device = WaveIn.GetCapabilities(i);

//            if (device.ProductName.ToLower().Contains("cable output") && device.Channels > 0)
//            {
//                Trace.WriteLine($"✅ Found VB-Cable device: {device.ProductName} (index {i})");
//                return i;
//            }
//        }

//        throw new Exception("❌ VB-Cable input device not found!");
//    }
//}
