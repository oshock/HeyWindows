using System.Speech.Recognition;

namespace HeyWindows.Core.Grammars;

public static class GrammarUtils
{
    public static GrammarBuilder ToBuilder(this Choices choices) => new(choices);
}