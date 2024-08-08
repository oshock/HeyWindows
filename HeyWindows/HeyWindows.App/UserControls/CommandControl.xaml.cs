using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
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
        IsUnsaved = false;
    }
    
    public CommandControl(Command command) : this()
    {
        Command = command;
    }

    public void UploadCommand()
    {
        if (ExecutorName is null)
            throw new InvalidDataException("Cannot create command when is null");
        
        if (ArgumentHandler is null)
            throw new InvalidDataException("Cannot create command when is null");
        
        if (Trigger is null)
            throw new InvalidDataException("Cannot create command when is null");
        
        Command = Command.Create(ExecutorName, ArgumentHandler, new List<CommandTrigger> { Trigger });
        MainWindow.Commander.InitializeCommand(Command);

        ConfigData!.Commands.Add(Command);
        SaveConfig();
        
        IsUnsaved = false;
    }
    
    public Command? Command { get; set; }
    public string? ExecutorName;
    public ICommandArgs? ArgumentHandler { get; set; }
    public CommandTrigger? Trigger { get; protected set; }
    
    private bool _isRecording;

    /*private void Record_OnClick(object sender, MouseButtonEventArgs e)
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
    }*/

    private void Border_OnMouseEnter(object sender, MouseEventArgs e)
    {
        ((Border)sender).Background = new SolidColorBrush(Color.FromArgb(20, 255, 255, 255));
    }

    private void Border_OnMouseLeave(object sender, MouseEventArgs e)
    {
        ((Border)sender).Background = new SolidColorBrush(Color.FromArgb(10, 255, 255, 255));
    }

    private bool _isUnsaved;
    
    private bool IsUnsaved
    {
        get => _isUnsaved;
        set
        {
            _isUnsaved = value;
            SaveChanges.Background = value ? new SolidColorBrush(Color.FromRgb(44, 160, 209))
                : SaveChanges.Background = new SolidColorBrush(Color.FromArgb(10, 255, 255, 255));
        }
    }
    
    private void SaveButton_OnMouseEnter(object sender, MouseEventArgs e)
    {
        if (IsUnsaved)
            ((Border)sender).Background = new SolidColorBrush(Color.FromArgb(20, 49, 165, 214));
    }

    private void SaveButton_OnMouseLeave(object sender, MouseEventArgs e)
    {
        if (IsUnsaved) 
            ((Border)sender).Background = new SolidColorBrush(Color.FromArgb(255, 44, 160, 209));
    }

    private void ActionType_OnSelectionChanged(object obj, SelectionChangedEventArgs e)
    {
        Arguments.Children.Clear();

        ExecutorName = ActionType.SelectedItem.Cast<ComboBoxItem>()!.Name;
        ArgumentHandler = ExecutorName switch
        {
            "Executable" => new ExecutableExecutor().ArgumentHandler,
            _ => throw new KeyNotFoundException()
        };
        
        var fields = ArgumentHandler.GetType().GetFields();
        foreach (var field in fields)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(10, 255,255,255)),
                CornerRadius = new CornerRadius(8.0),
                Margin = new Thickness(0, 20, 0, 0)
            };
            
            var subPanel = new StackPanel { Margin = new Thickness(10, 5, 10, 10) };
            var argumentInfo = field.GetArgumentAttribute();

            border.Child = subPanel;
            subPanel.Children.Add(new TextBlock
            {
                Text = argumentInfo.DisplayName, 
                Foreground = Brushes.White,
                FontSize = 15,
                Margin = new Thickness(0, 10, 0, 0)
            });
            
            if (!string.IsNullOrEmpty(argumentInfo.Description))
                subPanel.Children.Add(new TextBlock
                {
                    Text = argumentInfo.Description, 
                    Foreground = Brushes.White,
                    Margin = new Thickness(0, 0, 0, 8)
                });
            
            if (field.FieldType == typeof(string))
            {
                var textBox = new TextBox { Name = argumentInfo.DisplayName.MakeFriendly() };
                textBox.TextChanged += (s, _) =>
                {
                    IsUnsaved = true;
                    var sender = (TextBox)s;
                    var handlerFields = ArgumentHandler.GetType().GetFields();

                    foreach (var f in handlerFields)
                    {
                        var info = f.GetArgumentAttribute();
                        if (info.DisplayName.MakeFriendly() != sender.Name)
                            continue;

                        if (f.FieldType != typeof(string))
                            throw new InvalidDataException($"'{info.DisplayName}' is not a string. Unable to set.");
                        
                        f.SetValue(ArgumentHandler, sender.Text);
                        return;
                    }
                };
                
                if (!string.IsNullOrEmpty(argumentInfo.Placeholder))
                    textBox.PlaceholderText = argumentInfo.Placeholder;

                subPanel.Children.Add(textBox);
            }
            else if (field.FieldType == typeof(bool))
            {
                var checkBox = new CheckBox { Name = argumentInfo.DisplayName.MakeFriendly() };
                void setValue(object checkBox)
                {
                    IsUnsaved = true;
                    var sender = (CheckBox)checkBox;
                    var handlerFields = ArgumentHandler!.GetType().GetFields();

                    foreach (var f in handlerFields)
                    {
                        var info = f.GetArgumentAttribute();
                        if (info.DisplayName.MakeFriendly() != sender.Name)
                            continue;

                        if (f.FieldType != typeof(bool))
                            throw new InvalidDataException($"'{info.DisplayName}' is not a string. Unable to set.");
                        
                        f.SetValue(ArgumentHandler, sender.IsChecked ?? false);
                        return;
                    }
                }

                checkBox.Checked += (s, _) => setValue(s);
                checkBox.Unchecked += (s, _) => setValue(s);
                
                subPanel.Children.Add(checkBox);
            } 
            else if (field.FieldType == typeof(int))
            {
                var textBox = new TextBox { Name = argumentInfo.DisplayName.MakeFriendly() };
                textBox.PreviewTextInput += (_, e) =>
                {
                    var regex = new Regex("[^0-9.-]+");
                    e.Handled = regex.IsMatch(e.Text);
                };
                
                textBox.TextChanged += (s, _) =>
                {
                    IsUnsaved = true;
                    var sender = (TextBox)s;
                    var handlerFields = ArgumentHandler.GetType().GetFields();

                    foreach (var f in handlerFields)
                    {
                        var info = f.GetArgumentAttribute();
                        if (info.DisplayName.MakeFriendly() != sender.Name)
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
                
                subPanel.Children.Add(textBox);
            }
            
            Arguments.Children.Add(border);
        }
    }

    private void SaveButton_OnClick(object sender, MouseButtonEventArgs e)
    {
        UploadCommand();
    }

    private void RecordResult_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        Trigger ??= new CommandTrigger();

        var textBox = (TextBox)sender;
        Trigger.Trigger = textBox.Text;
    }
}