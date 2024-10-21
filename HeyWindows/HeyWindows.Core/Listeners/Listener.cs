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

    public void AddWord(string word)
    {
        var grammar = new GrammarBuilder(word);
        Grammars.Add(grammar);
    }

    public void Initialize()
    {
        Recognizer.UnloadAllGrammars();
        Recognizer.LoadGrammar(new DictationGrammar());

        if (Grammars.Count > 0)
        {
            Grammars.Loop(x => Recognizer.LoadGrammar(new Grammar(x)));
            Grammars.Clear();
        }
    }

    public void Listen()
    {
        Recognizer.SpeechRecognized += Recognizer_OnSpeechRecognized;
        Recognizer.SetInputToDefaultAudioDevice();
        Recognizer.RecognizeAsync(RecognizeMode.Multiple);
        
        Console.WriteLine("Listening...\n");
    }

    public string LastPhrase; 
    public string LastPronunciation; 
    
    public async void ListenSingleAsync(Action<string, string> callback)
    {
        LogInfo("Listening...\n");
        Recognizer.SpeechRecognized += (_, e) =>
        {
            LogInfo("==============================================");
            LogInfo($"Recognized text: \"{e.Result.Text}\"");
            LogInfo("Pronunciation: ");
            var pronunciation = e.Result.Words.Select(x => x.Pronunciation).MergeStringArray();
            LogInfo($"\"{pronunciation}\"");
        
            foreach (var phrase in e.Result.Alternates)
            {
                LogInfo("\nAlternative: ");
                LogInfo($"\t- \"{phrase.Text}\"");
                LogInfo($"\tPronunciation: \"{phrase.Words.Select( x=> x.Pronunciation).MergeStringArray()}\"");
            }
            LogInfo("==============================================");

            LastPhrase = e.Result.Text;
            LastPronunciation = pronunciation;
            callback(LastPhrase, LastPronunciation);
        };
        
        Recognizer.SetInputToDefaultAudioDevice();
        Recognizer.RecognizeAsync();
    }
    
    public async void ListenAsync(Action<SpeechRecognizedEventArgs> callback)
    {
        LogInfo("Listening...\n");
        Recognizer.SpeechRecognized += (_, e) =>
        {
            LogInfo("\n============================================");
            LogInfo(e.Result.Text);
            LogInfo("============================================\n");
            
            callback(e);
        };
        
        Recognizer.SetInputToDefaultAudioDevice();
        Recognizer.RecognizeAsync(RecognizeMode.Multiple);
    }

    private void Recognizer_OnSpeechRecognized(object? sender, SpeechRecognizedEventArgs e)
    {
        LogInfo("==============================================");
        LogInfo($"Recognized text: \"{e.Result.Text}\"");
        LogInfo("Pronunciation: ");
        var pronunciation = e.Result.Words.Select(x => x.Pronunciation).MergeStringArray();
        LogInfo($"\"{pronunciation}\"");
        
        foreach (var phrase in e.Result.Alternates)
        {
            LogInfo("\nAlternative: ");
            LogInfo($"\t- \"{phrase.Text}\"");
            LogInfo($"\tPronunciation: \"{phrase.Words.Select( x=> x.Pronunciation).MergeStringArray()}\"");
        }
        LogInfo("==============================================");

        CommandingCommander!.Execute(e.Result.Text, pronunciation);
    }
}