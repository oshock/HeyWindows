using System.Speech.Recognition;

using (
    SpeechRecognitionEngine recognizer =
    new SpeechRecognitionEngine(
        new System.Globalization.CultureInfo("en-US")))
{

    // Create and load a dictation grammar.  
    recognizer.LoadGrammar(new DictationGrammar());

    // Add a handler for the speech recognized event.  
    recognizer.SpeechRecognized += recognizer_SpeechRecognized;

    // Configure input to the speech recognizer.  
    recognizer.SetInputToDefaultAudioDevice();

    // Start asynchronous, continuous speech recognition.  
    recognizer.RecognizeAsync(RecognizeMode.Single);

    // Keep the console window open.  
    while (true)
    {
        Console.ReadLine();
    }
}

static void recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
{
    Console.WriteLine($"Recognized text: \"{e.Result.Text}\"");
    foreach (var phrase in e.Result.Alternates)
    {
        Console.WriteLine($"    - \"{phrase.Text}\"");
    }
}