using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

// Import namespaces
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

namespace speaking_clock
{
    class Program
    {
        private static SpeechConfig speechConfig;
        static async Task Main(string[] args)
        {
            try
            {
                // Get config settings from AppSettings
                IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
                IConfigurationRoot configuration = builder.Build();
                string cogSvcKey = configuration["CognitiveServiceKey"];
                string cogSvcRegion = configuration["CognitiveServiceRegion"];

                // Configure speech service
                speechConfig = SpeechConfig.FromSubscription(cogSvcKey, cogSvcRegion);
                Console.WriteLine("Ready to use speech service in " + speechConfig.Region);

                string command = "";
                while (command.ToLower() != "stop.")
                {
                    command = await TranscribeCommand();
                    // if (command.ToLower() == "what time is it?")
                    if (command.ToLower() == "今の時刻は？")
                    {
                        await TellTime();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static async Task<string> TranscribeCommand()
        {
            string command = "stop.";

            // Configure speech recognition
            using AudioConfig audioConfig = AudioConfig.FromDefaultMicrophoneInput();
            // using SpeechRecognizer speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);
            using SpeechRecognizer speechRecognizer = new SpeechRecognizer(speechConfig, "ja-JP", audioConfig);


            // Process speech input
            Console.WriteLine(speechRecognizer);
            Console.WriteLine("Say 'ストップ' to end...");
            SpeechRecognitionResult speech = await speechRecognizer.RecognizeOnceAsync();
            Console.WriteLine(speech);
            if (speech.Reason == ResultReason.RecognizedSpeech)
            {
                command = speech.Text;
                Console.WriteLine(command);
            }
            else
            {
                Console.WriteLine(speech.Reason);
                if (speech.Reason == ResultReason.Canceled)
                {
                    var cancellation = CancellationDetails.FromResult(speech);
                    Console.WriteLine(cancellation.Reason);
                    Console.WriteLine(cancellation.ErrorDetails);
                }
            }

            // Return the command
            return command;
        }

        static async Task TellTime()
        {
            var now = DateTime.Now;
            // string responseText = "The time is " + now.Hour.ToString() + ":" + now.Minute.ToString("D2");
            string responseText = "今の時刻は" + now.Hour.ToString() + ":" + now.Minute.ToString("D2") + "です";

            var language = "ja-JP";
            speechConfig.SpeechSynthesisLanguage = language;

            // Configure speech synthesis
            using SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer(speechConfig);

            // Synthesize spoken output
            SpeechSynthesisResult speak = await speechSynthesizer.SpeakTextAsync(responseText);
            if (speak.Reason != ResultReason.SynthesizingAudioCompleted)
            {
                Console.WriteLine(speak.Reason);
            }

            // Print the response
            Console.WriteLine(responseText);
        }

    }
}
