using System.Speech.Recognition;
using System.Text.Json.Serialization;

namespace HeyWindows.Core.Grammars;

public class GrammarCreator
{
    public static GrammarCreator Create()
    {
        return new GrammarCreator();
    }

    public readonly GrammarBuilder Builder = new();

    public void IncorporatePhrase(string text) => Builder.Append(text);

    public static Choices CreateChoice(params string[] strings) => new(strings);

    public void AppendBuilder(GrammarBuilder builder) => Builder.Append(builder);
}