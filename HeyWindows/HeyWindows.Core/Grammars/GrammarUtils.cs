using System.Speech.Recognition;
using HeyWindows.Core.Commands;
using HeyWindows.Core.Utils;

namespace HeyWindows.Core.Grammars;

public static class GrammarUtils
{
    public static GrammarBuilder BuildGrammarFromCommands(this IEnumerable<Command>? commands)
    {
        ArgumentNullException.ThrowIfNull(commands);

        var builder = new GrammarBuilder();
        var choices = new Choices();

        foreach (var command in commands)
        {
            foreach (var trigger in command.Triggers)
                choices.Add(trigger.Trigger);
        }
        
        builder.Append(choices);
        return builder;
    }
}