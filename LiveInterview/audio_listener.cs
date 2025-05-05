using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.Speech.V1;
using NAudio.Wave;
using System.Diagnostics;
using LiveInterview.custom_tools;
using System.Windows.Forms;
using static System.Runtime.InteropServices.JavaScript.JSType;

class AudioListener
{


    protected const int Rate = 16000;
    protected const int ChunkSize = 1024;
    protected const int Channels = 1;
    protected const int SilenceThreshold = 1000; // Lower = more sensitive
    protected const double SilenceDuration = 1; // seconds

    protected WasapiLoopbackCapture waveIn;
    protected List<byte[]> currentBuffer = new List<byte[]>();
    public List<string> bufferKeyList = new List<string>();
    public Dictionary<string, List<byte[]>> buffers = new Dictionary<string, List<byte[]>>();
    private int silentChunkCounter = 0;
    private OpenAIClient openAIClient;
    public bool isListening = false;
    private CancellationTokenSource cancellationTokenSource;
    private Task listenTask;
    private Task transcriptionTask;
    public Action when_stop;
    public Action when_start;
    public Action<string, string, List<byte[]>> on_item;


    public Panel itemPanel;

    protected SpeechClient speechClient;

    public AudioListener()
    {

        // Try to get the credentials path from an environment variable
        string credentialsPath = Environment.GetEnvironmentVariable("GCLOUD_API_KEY_LIVEINTERVIEW");

        if (string.IsNullOrWhiteSpace(credentialsPath))
        {
            // If not found, fallback to a local file in the output directory
            string outputDirectory = AppDomain.CurrentDomain.BaseDirectory;
            credentialsPath = Path.Combine(outputDirectory, "google-project.json");

            if (!File.Exists(credentialsPath))
            {
                throw new FileNotFoundException($"Google credentials file not found at: {credentialsPath}");
            }
        }

        // Set the credentials path for Google APIs
        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsPath);

        this.speechClient = SpeechClient.Create();
        this.openAIClient = new OpenAIClient();
    }

    protected string GenerateRandomAlphanumeric(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        Random random = new Random();
        char[] result = new char[length];

        for (int i = 0; i < length; i++)
        {
            result[i] = chars[random.Next(chars.Length)];
        }

        return $"item{string.Join("", result)}";
    }

    public virtual void Start()
    {
        if (isListening)
            return;

        isListening = true;
        cancellationTokenSource = new CancellationTokenSource();

        this.waveIn = new WasapiLoopbackCapture
        {
            WaveFormat = new WaveFormat(Rate, Channels),
        };

        this.waveIn.DataAvailable += this.OnDataAvailable;
        this.waveIn.StartRecording();

        Trace.WriteLine("🎙️ Listening started...");
        this.when_start();

        listenTask = Task.Run(() => this.ListenLoop(cancellationTokenSource.Token));
        transcriptionTask = Task.Run(() => this.StartTranscription(cancellationTokenSource.Token));
    }

    public virtual void Stop()
    {
        if (!isListening)
            return;

        isListening = false;
        cancellationTokenSource.Cancel();

        this.waveIn.StopRecording();
        this.waveIn.Dispose();
        this.waveIn = null;
        this.when_stop();

        Trace.WriteLine("🛑 Listening stopped. Flushing remaining buffers...");
        FlushCurrentBufferToBuffers();
    }


    private void FlushCurrentBufferToBuffers()
    {
        List<byte[]> snapshot = null;

        lock (this.currentBuffer)
        {
            if (this.currentBuffer.Count > 0)
            {
                snapshot = new List<byte[]>(this.currentBuffer);
                this.currentBuffer.Clear();
            }
        }

        if (snapshot != null)
        {
            string key = this.GenerateRandomAlphanumeric(10);
            this.buffers[key] = snapshot.Select(chunk =>
            {
                byte[] copy = new byte[chunk.Length];
                Array.Copy(chunk, copy, chunk.Length);
                return copy;
            }).ToList();

            lock (this.bufferKeyList)
            {
                this.bufferKeyList.Add(key);
                Trace.WriteLine($"🟡 Flushed buffer to key {key} with {snapshot.Count} chunks.");
            }
        }
    }



    protected virtual void OnDataAvailable(object sender, WaveInEventArgs e)
    {
        var chunk = new byte[e.BytesRecorded];
        Array.Copy(e.Buffer, chunk, e.BytesRecorded);

        this.currentBuffer.Add(chunk);

        if (this.IsSilent(chunk))
        {
            this.silentChunkCounter++;
        }
        else
        {
            this.silentChunkCounter = 0;
        }
    }


    private void ListenLoop(CancellationToken token)
    {
        Trace.WriteLine("Audio listening service started...");
        int silenceChunkThreshold = (int)((Rate / ChunkSize) * SilenceDuration);

        while (!token.IsCancellationRequested)
        {
            List<byte[]> currentBufferSnapshot = null; // 👈 snapshot outside

            lock (this.currentBuffer) // Lock the list once
            {
                if (this.silentChunkCounter > silenceChunkThreshold && this.currentBuffer.Count > 0)
                {
                    // Take a snapshot copy of currentBuffer
                    currentBufferSnapshot = new List<byte[]>(this.currentBuffer);
                    this.currentBuffer.Clear(); // Clear safely
                    this.silentChunkCounter = 0;
                }
            }

            if (currentBufferSnapshot != null)
            {
                Trace.WriteLine($"🛑 Silence detected. Capturing buffer. Buffer Count: {this.bufferKeyList.Count}");

                lock (this.bufferKeyList)
                {
                    string key = this.GenerateRandomAlphanumeric(10);
                    this.buffers[key] = new List<byte[]>();

                    foreach (var buffer in currentBufferSnapshot)
                    {
                        if (buffer != null)
                        {
                            byte[] bufferCopy = new byte[buffer.Length];
                            Array.Copy(buffer, bufferCopy, buffer.Length);
                            this.buffers[key].Add(bufferCopy);
                        }
                        else
                        {
                            Trace.WriteLine("Null buffer found in snapshot.");
                        }
                    }

                    this.bufferKeyList.Add(key);
                }
            }


            Task.Delay(100).Wait();  // Adding small delay to avoid busy-waiting
        }

        Trace.WriteLine("Audio listening service ENDED...");

    }


    private void StartTranscription(CancellationToken token)
    {

        while (true)
        {

            lock (this.bufferKeyList)
            {
                if (this.bufferKeyList.Count > 0)
                {
                    List<byte[]> audioBuffer;
                    string key;


                    key = this.bufferKeyList.First();
                    audioBuffer = this.buffers[key];

                    this.bufferKeyList.RemoveAt(0);
                    this.buffers.Remove(key);

                    this.TranscribeAudioAsync(audioBuffer, key);

                }
            }

            Task.Delay(300).Wait();



        }

        
    }

    private void AddItemToPanel(Item transcribeItem)
    {
        if (this.itemPanel.InvokeRequired)
        {
            this.itemPanel.BeginInvoke(new Action(() => AddItemToPanel(transcribeItem)));
        }
        else
        {
            this.itemPanel.Controls.Add(transcribeItem);
        }
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

    protected virtual async Task TranscribeAudioAsync(List<byte[]> audioChunks, string key)
    {
        if (audioChunks == null || !audioChunks.Any() || audioChunks.All(c => c == null || c.Length == 0))
        {
            Trace.WriteLine("No audio data provided in audioChunks.");
            return;
        }

        try
        {
            var audioBytes = audioChunks.SelectMany(chunk => chunk).ToArray();
            var recognitionAudio = RecognitionAudio.FromBytes(audioBytes);

            var config = new RecognitionConfig
            {
                Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
                SampleRateHertz = Rate,
                LanguageCode = "en-US"
            };

            Trace.WriteLine("Sending audio to Google Speech-to-Text...");

            var response = await speechClient.RecognizeAsync(config, recognitionAudio);
            string fullText = "";

            foreach (var result in response.Results)
            {
                var transcript = result.Alternatives.FirstOrDefault()?.Transcript;
                if (!string.IsNullOrWhiteSpace(transcript))
                {
                    fullText += transcript + " ";
                }
            }

            fullText = fullText.Trim();
            Trace.WriteLine($"System audio: {fullText}");

            if (!string.IsNullOrWhiteSpace(fullText))
            {
                this.on_item?.Invoke(key, fullText, audioChunks);
            }
            else
            {
                Trace.WriteLine("Transcription is null, empty, or only whitespace.");
            }

                Trace.WriteLine("Transcription success.");
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Transcription error: {ex.Message}");
        }
    }


    protected bool IsSilent(byte[] chunk)
    {
        double sumSquares = 0;
        int sampleCount = chunk.Length / 2;

        for (int i = 0; i < chunk.Length; i += 2)
        {
            short sample = BitConverter.ToInt16(chunk, i);
            sumSquares += sample * sample;
        }

        double rms = Math.Sqrt(sumSquares / sampleCount);
        return rms < SilenceThreshold;
    }
}
