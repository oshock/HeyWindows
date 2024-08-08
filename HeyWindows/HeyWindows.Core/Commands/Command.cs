﻿using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using HeyWindows.Core.Commands.Executors;
using HeyWindows.Core.Utils;
using Newtonsoft.Json;

namespace HeyWindows.Core.Commands;

public class CommandTrigger
{
    public string? Trigger;
    public string? Pronunciation;

    public CommandTrigger()
    { }
    
    public CommandTrigger(string? trigger, string? pronunciation = null)
    {
        Trigger = trigger;
        Pronunciation = pronunciation;
    }

    public bool IsCommand(string? phrase)
    {
        if (phrase is null)
            return false;
        return Trigger.ContainsOrFalse(phrase) || Pronunciation.ContainsOrFalse(phrase);
    }
}

public class Command
{
    public string Id => GetGuid().ToString();
    
    public List<CommandTrigger> Triggers = new(); // "Hey Windows"
    
    [JsonIgnore]
    private ICommandExecutor? _executor; // Command action.. duh...

    public ICommandArgs? Arguments; // Command action.. duh...

    public Guid GetGuid()
    {
        if (Triggers.Count == 0)
            throw new NotSupportedException("Cannot create guid without any triggers to the command.");

        var trigger = Triggers.First();
        var hash = SHA256.HashData(
            Encoding.UTF8.GetBytes((trigger.Trigger ?? string.Empty) + (trigger.Pronunciation ?? string.Empty)));

        var guid = new Guid(hash.SelectRange(0, 15));
        return guid;
    }

    public static Command Create(string executor, ICommandArgs arguments, List<CommandTrigger> triggers)
    {
        return new Command
        {
            Executor = executor,
            Arguments = arguments,
            Triggers = triggers
        };
    }
    
    public string Executor
    {
        get
        {
            return Commander.ExecutorByName.FirstOrDefault(x => x.Value.GetType() == _executor?.GetType()).Key;
        }
        set
        {
            if (!Commander.ExecutorByName.TryGetValue(value, out var executor))
                throw new KeyNotFoundException(
                    $"Could not find executor named '{value}'. \nAvailable executors: \n{Commander.ExecutorByName.Keys.MergeStringArray("\n")}");

            _executor = executor;
        }
    }
    
    public bool IsActive = true;

    public bool TryExecute() => _executor?.TryExecute(Arguments ?? throw new ArgumentNullException($"Arguments were null for command '{Id}'.")) 
                                ?? throw new InvalidDataException("Command does not have an action associated with it.");

    public void AddTrigger(CommandTrigger trigger) => Triggers.Add(trigger);

    public bool IsCommand(string? trigger) => Triggers.Any(x => x.IsCommand(trigger));
}