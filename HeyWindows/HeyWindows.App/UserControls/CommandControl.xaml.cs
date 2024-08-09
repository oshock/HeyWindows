using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using HeyWindows.Core.Commands;
using HeyWindows.Core.Commands.Attributes;
using HeyWindows.Core.Commands.Executors;
using HeyWindows.Core.Utils;
using Microsoft.Win32;
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

    private MainWindow.RemoveCallback Remove;
    
    public CommandControl(Command command, MainWindow.RemoveCallback callback) : this()
    {
        Command = command;
        LoadedCommand = Command;
        Remove = callback;
        ArgumentHandler = Command.Arguments;
        ActionType.SelectedItem = command.Executor switch
        {
            "Executable" => Executable,
            _ => throw new KeyNotFoundException()
        };

        RecordResult.Text = Command.Triggers.First().Trigger;
    }

    private void UploadCommand()
    {
        if (ExecutorName is null)
            throw new InvalidDataException("Cannot create command when ExecutorName is null");
        
        if (ArgumentHandler is null)
            throw new InvalidDataException("Cannot create command when ArgumentHandler is null");
        
        if (Trigger is null)
            throw new InvalidDataException("Cannot create command when Trigger is null");

        foreach (var field in ArgumentHandler.GetType().GetFields())
        {
            var argumentInfo = field.GetArgumentAttribute();
            if (argumentInfo.Type != StringInputType.File && argumentInfo.Type != StringInputType.Directory)
                continue;

            var text = (string)field.GetValue(ArgumentHandler)!;
            field.SetValue(ArgumentHandler, text.Replace("/", "\\").Replace("\"", string.Empty));
        }
        
        var newCommand = Command.Create(ExecutorName, ArgumentHandler, new List<CommandTrigger> { Trigger });
        MainWindow.Commander.InitializeCommand(newCommand);

        if (Command is not null)
        {
            MainWindow.Commander.DeinitializeCommand(Command);
            newCommand.SetOverrideGuid(Guid.Parse(Command.Id));
        }

        var command = ConfigData!.Commands.FindIndex(x => x.Id == newCommand.Id);
        if (command >= 0)
        {
            ConfigData.Commands.RemoveAt(command);
            ConfigData.Commands.Insert(command, newCommand);
            Command = newCommand;
        }
        else
            ConfigData.Commands.Add(newCommand);

        SaveConfig();

        LoadedCommand = newCommand;
        IsUnsaved = false;
    }
    
    public Command? Command { get; set; }
    public Command? LoadedCommand { get; set; }
    public string? ExecutorName;
    public ICommandArgs? ArgumentHandler { get; set; }
    public CommandTrigger? Trigger { get; protected set; }
    
    private bool _isRecording;

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
            SaveButton.Background = IsUnsaved ? new SolidColorBrush(Color.FromRgb(47, 119, 227))
                    : new SolidColorBrush(Color.FromArgb(10, 255, 255, 255));
        }
    }

    private void ActionType_OnSelectionChanged(object obj, SelectionChangedEventArgs e)
    {
        IsUnsaved = true;
        Arguments.Children.Clear();

        ExecutorName = ActionType.SelectedItem.Cast<ComboBoxItem>()!.Name;
        if (Command is null)
        {
            ArgumentHandler = ExecutorName switch
            {
                "Executable" => new ExecutableExecutor().ArgumentHandler,
                _ => throw new KeyNotFoundException()
            };
        }

        if (ArgumentHandler is null)
            throw new NullReferenceException("ArgumentHandler is null but Command is not.");
        
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
                    Foreground = new SolidColorBrush(Color.FromArgb(200,255,255,255)),
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
                
                textBox.Loaded += (_, _) =>
                {
                    if (Command is null || ArgumentHandler is null)
                        return;

                    textBox.Text = (string)field.GetValue(ArgumentHandler)!;
                    IsUnsaved = false;
                };
                
                if (!string.IsNullOrEmpty(argumentInfo.Placeholder))
                    textBox.PlaceholderText = argumentInfo.Placeholder;

                switch (argumentInfo.Type)
                {
                    case StringInputType.File:
                    {
                        var grid = new Grid();
                        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10) });
                        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });

                        var button = new Button
                        {
                            Content = "...",
                            Background = new SolidColorBrush(Color.FromArgb(10, 255, 255, 255)),
                            Foreground = Brushes.White,
                            Name = argumentInfo.DisplayName.MakeFriendly() + "_button"
                        };

                        grid.Children.Add(textBox);
                        grid.Children.Add(button);
                        subPanel.Children.Add(grid);
                        Grid.SetColumn(button, 2);

                        button.Click += (s, e) =>
                        {
                            var dialog = new OpenFileDialog { Filter = "Executable Files (*.exe)|*.exe" };
                            var result = dialog.ShowDialog();
                            if (!result.HasValue || !result.Value)
                                return;
                            
                            IsUnsaved = true;
                            var sender = (Button)s;
                            var name = sender.Name.Replace("_button", string.Empty);
                            var handlerFields = ArgumentHandler.GetType().GetFields();
                            
                            
                            foreach (var f in handlerFields)
                            {
                                var info = f.GetArgumentAttribute();
                                if (info.DisplayName.MakeFriendly() != name)
                                    continue;

                                if (f.FieldType != typeof(string))
                                    throw new InvalidDataException($"'{info.DisplayName}' is not a string. Unable to set.");

                                textBox.Text = dialog.FileName;
                                return;
                            }
                        };
                    }
                        break;
                    case StringInputType.Directory:
                        break;
                    case StringInputType.Regular:
                        subPanel.Children.Add(textBox);
                        break;
                }
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

    private void SaveButton_OnMouseEnter(object sender, MouseEventArgs e)
    {
        if (IsUnsaved)
            SaveButton.Background = new SolidColorBrush(Color.FromRgb(67, 139, 247));
    }

    private void SaveButton_OnMouseLeave(object sender, MouseEventArgs e)
    {
        if (IsUnsaved) 
            SaveButton.Background = new SolidColorBrush(Color.FromRgb(37, 109, 217));
    }
    
    private void SaveButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (IsUnsaved)
            UploadCommand();
    }

    private void DeleteButton_OnMouseEnter(object sender, MouseEventArgs e)
    {
        DeleteButton.Background = new SolidColorBrush(Color.FromRgb(255, 87, 87));
    }

    private void DeleteButton_OnMouseLeave(object sender, MouseEventArgs e)
    {
        DeleteButton.Background = new SolidColorBrush(Color.FromRgb(237, 57, 57));
    }
    
    private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
    {
        Remove(this);
    }
    
    private void RecordResult_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        IsUnsaved = true;
        Trigger ??= new CommandTrigger();

        var textBox = (TextBox)sender;
        Trigger.Trigger = textBox.Text;
    }
}