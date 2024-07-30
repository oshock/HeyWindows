using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Speech.Recognition;
using HeyWindows.Core.Utils;

namespace HeyWindows.Core.Listeners;

public class Listener
{
    public readonly SpeechRecognitionEngine Recognizer;
    public readonly List<GrammarBuilder> Grammars;
    
    public Listener(string culture = "en-US")
    {
        Recognizer = new SpeechRecognitionEngine(new CultureInfo(culture));
        Grammars = new List<GrammarBuilder>();
    }

    
    public void Initialize()
    {
        Recognizer.LoadGrammar(new DictationGrammar());
        Grammars.Loop(x => Recognizer.LoadGrammar(x));
    }
    
}