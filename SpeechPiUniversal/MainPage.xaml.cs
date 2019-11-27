using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using RaspberryModules.App.Modules;
using Sbs.Cognitive.Luis;
using Sbs.Cognitive.Luis.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Media.Playback;
using Windows.Media.SpeechRecognition;
using Windows.Media.SpeechSynthesis;
using Windows.Storage;
using Windows.System.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SpeechPiUniversal
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            _rgbLed.Init();
            _spiDisplay.InitAll();
            StartVideoPreviewAsync();
            yellowLEd.Init();
            _spiDisplay.WriteLinesToScreen(new List<string> { "Starting Program.  Please say a command." });
        }
        private SpeechRecognizer _contSpeechRecognizer;
        private readonly RGBLed _rgbLed = new RGBLed();
        private readonly string _luiskey = "4c5947d53aff40a8bf6ef88aeed2aa72";
        private readonly string _appId = "d7476472-d1d2-4c37-a4d6-3720a0318e04";
        private readonly SpeechSynthesizer _synthesizer = new SpeechSynthesizer();
        private readonly MediaPlayer _speechPlayer = new MediaPlayer();
        private SPIDisplay _spiDisplay = new SPIDisplay();

        private readonly DisplayRequest _displayRequest = new DisplayRequest();

        private readonly MediaCapture _mediaCapture = new MediaCapture();

        private FaceServiceClient _faceServiceClient = new FaceServiceClient("4a960c12f5294375ac6606223a65223f", "https://live360face.cognitiveservices.azure.com/face/v1.0");

        private readonly string _personGroup = "kcdc2018";
        private SingleLED yellowLEd = new SingleLED();


        private async Task StartVideoPreviewAsync()
        {
            await _mediaCapture.InitializeAsync();
            _displayRequest.RequestActive();

            PreviewControl.Source = _mediaCapture;
            await _mediaCapture.StartPreviewAsync();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            _contSpeechRecognizer = new SpeechRecognizer();
            await _contSpeechRecognizer.CompileConstraintsAsync();
            _contSpeechRecognizer.ContinuousRecognitionSession.ResultGenerated += ContinuousRecognitionSession_ResultGenerated;
            _contSpeechRecognizer.ContinuousRecognitionSession.Completed += ContinuousRecognitionSession_Completed;
            await _contSpeechRecognizer.ContinuousRecognitionSession.StartAsync();
            var voice = SpeechSynthesizer.AllVoices.FirstOrDefault(i => i.Gender == VoiceGender.Female) ?? SpeechSynthesizer.DefaultVoice;
            _synthesizer.Voice = voice;
        }

        private async void ContinuousRecognitionSession_Completed(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionCompletedEventArgs args)
        {
            Debug.WriteLine($"Completed > Restart listening");
            await _contSpeechRecognizer.ContinuousRecognitionSession.StartAsync();
        }

        private async void ContinuousRecognitionSession_ResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            string speechResult = args.Result.Text;
            Debug.WriteLine($"Text: {speechResult}");
            //_spiDisplay.WriteLinesToScreen(new List<string> { speechResult , "Line 2", "Line 3" , "Line 4", "Line 5" });
            _spiDisplay.WriteToScreen(speechResult);
            LuisClient client = new LuisClient(_appId, _luiskey);
            LuisResult result = await client.Predict(speechResult);
            if(speechResult.Contains("penelope"))
                HandleLuisResult(result);
            Debug.WriteLine($"LUIS Result: {result.Intents.First().Name} {string.Join(",", result.Entities.Select(a => $"{a.Key}:{a.Value.First().Value}"))}");
        }

        public void HandleLuisResult(LuisResult result)
        {
            if (!result.Intents.Any())
            {
                return;
            }

            switch (result.Intents.First().Name)
            {
                case "ControlLED":

                    if (result.Entities.Any(a => a.Key == "LedState"))
                    {
                        string ledState = result.Entities.First(a => a.Key == "LedState").Value.First().Value;

                        if (ledState == "on")
                        {
                            if (result.Entities.Any(a => a.Key == "LedColor"))
                            {
                                string ledColor = result.Entities.First(a => a.Key == "LedColor").Value.First().Value;

                                SayAsync($"Turning on the {ledColor} light.");

                                switch (ledColor)
                                {
                                    case "red":
                                        _rgbLed.TurnOnLed(LedStatus.Red);
                                        yellowLEd.TurnOff();
                                        break;
                                    case "green":
                                        _rgbLed.TurnOnLed(LedStatus.Green);
                                        yellowLEd.TurnOff();
                                        break;
                                    case "blue":
                                        _rgbLed.TurnOnLed(LedStatus.Blue);
                                        yellowLEd.TurnOff();
                                        break;
                                    case "purple":
                                        _rgbLed.TurnOnLed(LedStatus.Purple);
                                        yellowLEd.TurnOff();
                                        break;
                                    case "yellow":
                                        yellowLEd.TurnOn();
                                        _rgbLed.TurnOffLed();
                                        break;
                                }
                            }
                        }
                        else if (ledState == "off")
                        {
                            SayAsync("Turning the light off.");
                            yellowLEd.TurnOff();
                            _rgbLed.TurnOffLed();
                        }
                    }
                    else //if (result.Entities.Any(a => a.Key == "Authentication"))
                    {
                        //turn on the camera
                        //take a picture
                        //verify the user
                        (bool, List<string>) resultAuth = AuthenticatePerson().GetAwaiter().GetResult();
                        if(resultAuth.Item1)
                        {
                            //Green for yes
                            SayAsync("You have been verified.");
                            BlinkXTimes("Green", 5);
                        }
                        else
                        {
                            //Red for no
                            SayAsync("You are unknown.");
                            _rgbLed.TurnOnLed(LedStatus.Red);
                            BlinkXTimes("Red", 5);
                        }
                    }
                    break;

            }
        }
        public async Task SayAsync(string text)
        {
            using (var stream = await _synthesizer.SynthesizeTextToStreamAsync(text))
            {
                _speechPlayer.Source = MediaSource.CreateFromStream(stream, stream.ContentType);
            }
            _speechPlayer.Play();
        }

        private async Task<(bool, List<string>)> AuthenticatePerson()
        {
            try
            {
                ImageEncodingProperties imgFormat = ImageEncodingProperties.CreateJpeg();

                var file = await ApplicationData.Current.LocalFolder.CreateFileAsync("TestPhoto.jpg", CreationCollisionOption.GenerateUniqueName);
                await _mediaCapture.CapturePhotoToStorageFileAsync(imgFormat, file);

                // Call the Face API.
                Face[] faces = await _faceServiceClient.DetectAsync(await file.OpenStreamForReadAsync(), true);

                if (faces.Length > 0)
                {
                    // Get all Temp. face Ids
                    var faceIds = faces.Select(a => a.FaceId).ToArray();

                    // Identify the faces
                    IdentifyResult[] identifyResult = await _faceServiceClient.IdentifyAsync(_personGroup, faceIds);

                    if (identifyResult.Any())
                    {
                        List<Person> persons = new List<Person>();

                        foreach (IdentifyResult result in identifyResult)
                        {
                            // Load the person information
                            var person = await _faceServiceClient.GetPersonAsync(_personGroup, result.Candidates.First().PersonId);
                            persons.Add(person);
                            Debug.WriteLine($"{person.Name} | {result.Candidates.First().Confidence} | {person.PersonId}");
                        }

                        return (true, persons.Select(a => a.Name).ToList());
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            return (false, new List<string>());
        }

        private void BlinkXTimes(string Color, int NumberOfTimes = 3)
        {
            var led = LedStatus.Green;
            if (Color == "Red")
                led = LedStatus.Red;
            yellowLEd.TurnOff();

            for (int i = 0; i < NumberOfTimes; i++)
            {
                _rgbLed.TurnOnLed(led);
                Thread.Sleep(500);
                _rgbLed.TurnOffLed();
                Thread.Sleep(500);
            }
        }
    }
}
