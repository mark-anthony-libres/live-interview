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
        private DateTime lastStreamStartTime;
        protected const int Rate = 8000;
        public MicListener() : base() {}

        protected override async void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            
            await streamingCall.WriteAsync(new StreamingRecognizeRequest
            {
                AudioContent = Google.Protobuf.ByteString.CopyFrom(e.Buffer, 0, e.BytesRecorded)
            });
        }

   


 
        public override async void Start()
        {

            if (this.isListening)
                return;

            this.isListening = true;

            streamingCall = this.speechClient.StreamingRecognize();

            // Write the initial request with the config
            await streamingCall.WriteAsync(
                new StreamingRecognizeRequest
                {
                    StreamingConfig = new StreamingRecognitionConfig
                    {
                        Config = new RecognitionConfig
                        {
                            Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
                            SampleRateHertz = 16000,
                            LanguageCode = "en-US"
                        },
                        InterimResults = false
                    }
                });


            soundWave = new WaveInEvent
            {
                DeviceNumber = 0,
                WaveFormat = new WaveFormat(Rate, Channels)
            };

            soundWave.DataAvailable += this.OnDataAvailable;
            this.soundWave.StartRecording();

            Task.Run(readResponses);


        }

        private async Task readResponses()
        {
            var responseStream = streamingCall.GetResponseStream();

            while (await responseStream.MoveNextAsync()) // MoveNextAsync() moves to the next response
            {
                var response = responseStream.Current;

                // Process each result in the response
                foreach (var result in response.Results)
                {
                    if (result.IsFinal)
                    {
                        // Output the final transcription
                        Trace.WriteLine($"Final Transcription: {result.Alternatives[0].Transcript}");
                    }
                    else
                    {
                        // Output the interim transcription
                        Trace.WriteLine($"Interim Transcription: {result.Alternatives[0].Transcript}");
                    }
                }
            }

        }
    }
}
