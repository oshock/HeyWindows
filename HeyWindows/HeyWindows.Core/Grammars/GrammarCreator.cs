using System.Speech.Recognition;
using System.Text.Json.Serialization;
using HeyWindows.Core.Commands;

namespace HeyWindows.Core.Grammars;

public class CommandProcessor
{
    public GrammarBuilder BuildGrammarFromCommand(Command command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var commandBuilder = new GrammarBuilder(new Choices(command.Triggers.ToArray()));

        if (command.SubCommands.Count > 0)
        {
            var subCommandChoices = new Choices();

            foreach (var subCommand in command.SubCommands)
                subCommandChoices.Add(BuildGrammarFromCommand(subCommand));

            commandBuilder.Append(subCommandChoices);
        }

        return commandBuilder;
    }
}