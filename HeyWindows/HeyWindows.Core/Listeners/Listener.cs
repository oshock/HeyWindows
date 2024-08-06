using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Speech.Recognition;
using HeyWindows.Core.Commands;
using HeyWindows.Core.Utils;

namespace HeyWindows.Core.Listeners;

public class Listener
{
    public Commander? CommandingCommander;
    
    public readonly SpeechRecognitionEngine Recognizer;
    public readonly List<GrammarBuilder> Grammars; // Gets cleared after initialization
    
    public Listener(string culture = "en-US")
    {
        Recognizer = new SpeechRecognitionEngine(new CultureInfo(culture));
        Grammars = new List<GrammarBuilder>();
    }
    
    public void Initialize()
    {
        Recognizer.LoadGrammar(new DictationGrammar());
        Grammars.Loop(x => Recognizer.LoadGrammar(new Grammar(x)));
        Grammars.Clear();
    }

    public void Listen()
    {
        Recognizer.SpeechRecognized += Recognizer_OnSpeechRecognized;
        Recognizer.SetInputToDefaultAudioDevice();
        Recognizer.RecognizeAsync(RecognizeMode.Multiple);
        
        Console.WriteLine("Listening...\n");
    }

    private void Recognizer_OnSpeechRecognized(object? sender, SpeechRecognizedEventArgs e)
    {
        Console.WriteLine("==============================================");
        Console.WriteLine($"Recognized text: \"{e.Result.Text}\"");
        Console.WriteLine("Pronunciation: ");
        Console.WriteLine($"{e.Result.Words.Select( x=> x.Pronunciation).MergeStringArray()}\"");
        
        foreach (var phrase in e.Result.Alternates)
        {
            Console.WriteLine("\nAlternative: ");
            Console.WriteLine($"\t- \"{phrase.Text}\"");
            Console.WriteLine($"\tPronunciation: \"{phrase.Words.Select( x=> x.Pronunciation).MergeStringArray()}\"");
        }
        Console.WriteLine("==============================================");

        CommandingCommander!.Execute(e.Result.Text);
    }
}