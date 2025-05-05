using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LiveInterview
{
    class Listen : AudioListener
    {
        protected int totalBytesRecorded = 0;
        public Listen() : base() { }

        public override void Start()
        {

            if (this.isListening)
                return;

            this.isListening = true;

            this.waveIn = new WasapiLoopbackCapture
            {
                WaveFormat = new WaveFormat(Rate, Channels),
            };

            this.waveIn.DataAvailable += this.OnDataAvailable;
            this.waveIn.StartRecording();

            Trace.WriteLine("🎙️ Listening started...");
            this.when_start();

        }

        public override void Stop()
        {
            if (!isListening)
                return;

            Stopwatch stopwatch = new Stopwatch();

            // Start measuring time
            stopwatch.Start();

            isListening = false;

            this.waveIn.StopRecording();
            Task disposeTask = Task.Run(() => this.waveIn.Dispose());
            if (!disposeTask.Wait(5000)) // Wait for 5 seconds
            {
                Trace.WriteLine("Dispose timed out.");
            }
            else
            {
                Trace.WriteLine("Disposed successfully.");
            }

            this.waveIn = null;
            this.when_stop();

            string key = this.GenerateRandomAlphanumeric(10);
            Trace.WriteLine("🛑 Listening stopped. Flushing remaining buffers...");

            lock(this.currentBuffer)
            {
                if (this.currentBuffer.Count == 0)
                {
                    Trace.WriteLine("No data to process.");
                }
                else
                {

                    this.TranscribeAudioAsync(this.currentBuffer, key);

                }
            }

            stopwatch.Stop();

            Trace.WriteLine($"Elapsed time: {stopwatch.ElapsedMilliseconds} ms");


            this.currentBuffer = new List<byte[]>();


        }

        protected override void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            var chunk = new byte[e.BytesRecorded];
            Array.Copy(e.Buffer, chunk, e.BytesRecorded);
            this.currentBuffer.Add(chunk);

            this.totalBytesRecorded += e.BytesRecorded;

            // 16-bit audio = 2 bytes per sample
            int bytesPerSecond = Rate * Channels * 2;

            if (totalBytesRecorded >= bytesPerSecond * 59) // 1 minute
            {
                Trace.WriteLine("⏰ Max duration reached. Stopping...");
                this.totalBytesRecorded = 0;
                this.Stop();
            }
        

        }

        

    }
}
