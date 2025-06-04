using Google.Cloud.Speech.V1;
using NAudio.Wave;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using static Google.Cloud.Speech.V1.SpeechClient;

namespace LiveInterview
{
    class MicListener : AudioListener
    {
        private StreamingRecognizeStream streamingCall;
        protected WaveInEvent soundWave;
        protected const int Rate = 16000; // Match with Google config
        protected const int Channels = 1;
        private DateTime lastStreamStartTime;
        private object streamLock = new object();
        public string user_total_speak = "";

        public MicListener() : base() { }

        protected override async void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            if (streamingCall == null) return;

            await RestartStreamIfNeededAsync();

            try
            {
                await streamingCall.WriteAsync(new StreamingRecognizeRequest
                {
                    AudioContent = Google.Protobuf.ByteString.CopyFrom(e.Buffer, 0, e.BytesRecorded)
                });
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error sending audio: {ex.Message}");
            }
        }

        private async Task RestartStreamIfNeededAsync()
        {
            if ((DateTime.UtcNow - lastStreamStartTime).TotalSeconds < 270)
                return;

            Trace.WriteLine("Restarting stream due to time limit...");

            if (soundWave != null)
            {
                soundWave.DataAvailable -= OnDataAvailable;
                soundWave.StopRecording();
                soundWave.Dispose();
                soundWave = null;
                this.isListening = false;
            }

            try
            {
                if (streamingCall != null)
                {
                    streamingCall.WriteCompleteAsync().Wait();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error closing old stream: {ex.Message}");
            }

            Start();
        }

        public override async void Start()
        {
            if (this.isListening)
                return;

            this.isListening = true;

            streamingCall = this.speechClient.StreamingRecognize();

            // Write initial config
            await streamingCall.WriteAsync(new StreamingRecognizeRequest
            {
                StreamingConfig = new StreamingRecognitionConfig
                {
                    Config = new RecognitionConfig
                    {
                        Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
                        SampleRateHertz = Rate,
                        LanguageCode = "en-US",
                        AudioChannelCount = Channels
                    },
                    InterimResults = true
                }
            });

            soundWave = new WaveInEvent
            {
                DeviceNumber = 0,
                WaveFormat = new WaveFormat(Rate, Channels)
            };

            soundWave.DataAvailable += this.OnDataAvailable;
            soundWave.RecordingStopped += (s, e) => Trace.WriteLine("Recording stopped.");
            soundWave.StartRecording();

            lastStreamStartTime = DateTime.UtcNow;
            _ = Task.Run(readResponses); // fire-and-forget
        }

        private async Task readResponses()
        {
            var responseStream = streamingCall.GetResponseStream();

            try
            {
                while (await responseStream.MoveNextAsync())
                {
                    var response = responseStream.Current;

                    foreach (var result in response.Results)
                    {
                        var transcript = result.Alternatives[0].Transcript;

                        if(string.IsNullOrEmpty(transcript.Trim()))
                        {
                            continue;
                        }

                        if(result.IsFinal)
                        {
                            user_total_speak += $" {transcript}";
                        }

                        Trace.WriteLine(result.IsFinal
                            ? $"Final: {transcript}"
                            : $"Interim: {transcript}");
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Stream closed or errored: {ex.Message}");
            }
        }
    }
}
