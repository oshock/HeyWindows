using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using HeyWindows.App.Configs;
using HeyWindows.Core.Commands;
using HeyWindows.Core.Commands.Attributes;
using HeyWindows.Core.Commands.Executors;
using HeyWindows.Core.Listeners;
using HeyWindows.Core.Utils;
using Wpf.Ui.Controls;
using TextBlock = Wpf.Ui.Controls.TextBlock;
using TextBox = Wpf.Ui.Controls.TextBox;

namespace HeyWindows.App.UserControls;

public partial class CommandControl : UserControl
{
    
    public CommandControl()
    {
        InitializeComponent();
    }
    
    public CommandControl(Command command) : this()
    {
        Command = command;
    }

    public void UploadCommand()
    {
        var selected = (ComboBoxItem)ActionType.SelectionBoxItem;
        var command = Command.Create(selected.Name, new ExecutableCommandArgs(), 
            new List<CommandTrigger>
            {
                Trigger!
            });

        if (ConfigData is null)
            throw new InvalidDataException("Config has not been initialized.");
            
        ConfigData.Commands.Add(command);
    }
    
    public Command? Command { get; set; }
    public ICommandArgs? ArgumentHandler { get; set; }
    public CommandTrigger? Trigger { get; protected set; }
    
    private bool _isRecording;

    private void Record_OnClick(object sender, MouseButtonEventArgs e)
    {
        if (_isRecording)
            return;
        
        _isRecording = true;
        recordText.Foreground = new SolidColorBrush(Colors.Red);
        recordButton.Foreground = new SolidColorBrush(Colors.Red);
        recordButton.Symbol = SymbolRegular.RecordStop20;

        var listener = new Listener();
        listener.Initialize();
        listener.ListenSingleAsync((phrase, pronunciation) =>
        {
            Trigger = new CommandTrigger(null, pronunciation);
            RecordResult.Text = phrase;
        
            _isRecording = false;
            recordText.Foreground = new SolidColorBrush(Colors.White);
            recordButton.Foreground = new SolidColorBrush(Colors.White);
            recordButton.Symbol = SymbolRegular.Record20;
        });
    }

    private void Border_OnMouseEnter(object sender, MouseEventArgs e)
    {
        ((Border)sender).Background = new SolidColorBrush(Color.FromArgb(20, 255, 255, 255));
    }

    private void Border_OnMouseLeave(object sender, MouseEventArgs e)
    {
        ((Border)sender).Background = new SolidColorBrush(Color.FromArgb(10, 255, 255, 255));
    }

    private void ActionType_OnSelectionChanged(object obj, SelectionChangedEventArgs e)
    {
        ArgumentHandler = ActionType.SelectionBoxItem.Cast<ComboBoxItem>()!.Name switch
        {
            "Executable" => new ExecutableExecutor().ArgumentHandler,
            _ => throw new KeyNotFoundException()
        };
        
        var panel = new StackPanel();
        
        var fields = ArgumentHandler.GetType().GetFields();
        foreach (var field in fields)
        {
            var argumentInfo = field.GetArgumentAttribute();
            
            panel.Children.Add(new TextBlock { Text = argumentInfo.DisplayName });
            if (!string.IsNullOrEmpty(argumentInfo.Description))
                panel.Children.Add(new TextBlock { Text = argumentInfo.Description });
            
            if (field.FieldType == typeof(string))
            {
                var textBox = new TextBox { Name = argumentInfo.DisplayName };
                textBox.TextChanged += (s, _) =>
                {
                    var sender = (TextBox)s;
                    var handlerFields = ArgumentHandler.GetType().GetFields();

                    foreach (var f in handlerFields)
                    {
                        var info = f.GetArgumentAttribute();
                        if (info.DisplayName != sender.Name)
                            continue;

                        if (f.FieldType != typeof(string))
                            throw new InvalidDataException($"'{info.DisplayName}' is not a string. Unable to set.");
                        
                        f.SetValue(ArgumentHandler, sender.Text);
                        return;
                    }
                };
                
                if (!string.IsNullOrEmpty(argumentInfo.Placeholder))
                    textBox.PlaceholderText = argumentInfo.Placeholder;

                panel.Children.Add(textBox);
            }
            else if (field.FieldType == typeof(bool))
            {
                var checkBox = new CheckBox { Name = argumentInfo.DisplayName };
                void setValue(object checkBox)
                {
                    var sender = (CheckBox)checkBox;
                    var handlerFields = ArgumentHandler!.GetType().GetFields();

                    foreach (var f in handlerFields)
                    {
                        var info = f.GetArgumentAttribute();
                        if (info.DisplayName != sender.Name)
                            continue;

                        if (f.FieldType != typeof(bool))
                            throw new InvalidDataException($"'{info.DisplayName}' is not a string. Unable to set.");
                        
                        f.SetValue(ArgumentHandler, sender.IsChecked ?? false);
                        return;
                    }
                }

                checkBox.Checked += (s, _) => setValue(s);
                checkBox.Unchecked += (s, _) => setValue(s);
                
                panel.Children.Add(checkBox);
            } 
            else if (field.FieldType == typeof(int))
            {
                var textBox = new TextBox { Name = argumentInfo.DisplayName };
                textBox.PreviewTextInput += (_, e) =>
                {
                    var regex = new Regex("[^0-9.-]+");
                    e.Handled = regex.IsMatch(e.Text);
                };
                
                textBox.TextChanged += (s, _) =>
                {
                    var sender = (TextBox)s;
                    var handlerFields = ArgumentHandler.GetType().GetFields();

                    foreach (var f in handlerFields)
                    {
                        var info = f.GetArgumentAttribute();
                        if (info.DisplayName != sender.Name)
                            continue;

                        if (f.FieldType != typeof(string))
                            throw new InvalidDataException($"'{info.DisplayName}' is not a string. Unable to set.");

                        if (!int.TryParse(sender.Text, out var number))
                            throw new InvalidDataException(
                                $"TextBox '{info.DisplayName}' does not contain a int32 value. This shouldn't be possible??");
                            
                        f.SetValue(ArgumentHandler, number);
                        return;
                    }
                };
                
                panel.Children.Add(textBox);
            }
        }
        
        Arguments.Children.Clear();
        Arguments.Children.Add(panel);
    }
}