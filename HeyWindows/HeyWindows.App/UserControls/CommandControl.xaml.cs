using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using HeyWindows.App.Utils;
using HeyWindows.Core.Commands;
using HeyWindows.Core.Commands.Attributes;
using HeyWindows.Core.Commands.Executors;
using HeyWindows.Core.Listeners;
using HeyWindows.Core.Utils;
using Microsoft.Win32;
using Wpf.Ui.Controls;
using Button = System.Windows.Controls.Button;
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
    
    public CommandControl(Command? command, MainWindow.RemoveCallback callback) : this()
    {
        Remove = callback;
        if (command is null)
            return;
        
        Command = command;
        LoadedCommand = Command;
        ArgumentHandler = Command.Arguments;
        ActionType.SelectedItem = command.Executor switch
        {
            "Executable" => Executable,
            "Internet" => Internet,
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

            var text = (string?)field.GetValue(ArgumentHandler);
            if (text is not null)
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

    private void ActionType_OnSelectionChanged(object s, SelectionChangedEventArgs e)
    {
        IsUnsaved = true;
        Arguments.Children.Clear();

        ExecutorName = ActionType.SelectedItem.Cast<ComboBoxItem>()!.Name;
        if (Command is null)
        {
            ArgumentHandler = ExecutorName switch
            {
                "Executable" => new ExecutableExecutor().ArgumentHandler,
                "Internet" => new InternetRequestExecutor().ArgumentHandler,
                _ => throw new KeyNotFoundException()
            };
        }

        if (ArgumentHandler is null)
            throw new NullReferenceException("ArgumentHandler is null but Command is not.");

        void SetArgumentValue(Type type, string name, object? value)
        {
            var fields = ArgumentHandler.GetType().GetFields();
            foreach (var f in fields)
            {
                var info = f.GetArgumentAttribute();
                if (info.DisplayName.MakeFriendly() != name)
                    continue;

                if (f.FieldType != type)
                    throw new InvalidDataException($"'{info.DisplayName}' is not a string. Unable to set.");

                f.SetValue(ArgumentHandler, value);
                return;
            }
        }

        bool IsCommandValid()
        {
            return Command is not null && ArgumentHandler is not null;
        }

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
                    SetArgumentValue(typeof(string), sender.Name, sender.Text);
                };
                
                textBox.Loaded += (_, _) =>
                {
                    if (!IsCommandValid())
                        return;

                    textBox.Text = (string)field.GetValue(ArgumentHandler)!;
                    IsUnsaved = false;
                };
                
                if (!string.IsNullOrEmpty(argumentInfo.Placeholder))
                    textBox.PlaceholderText = argumentInfo.Placeholder;

                switch (argumentInfo.Type)
                {
                    case StringInputType.File:
                    case StringInputType.Directory:
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
                            var isFile = argumentInfo.Type == StringInputType.File;
                            dynamic dialog = isFile
                                ? new OpenFileDialog { Filter = "Executable Files (*.exe)|*.exe" }
                                : new OpenFolderDialog();
                            
                            var result = dialog.ShowDialog();
                            if (!result)
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

                                textBox.Text = isFile ? dialog.FileName : dialog.FolderName;
                                return;
                            }
                        };
                    }
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
                    SetArgumentValue(typeof(bool), sender.Name, sender.IsChecked ?? false);
                }
                
                checkBox.Loaded += (_, _) =>
                {
                    if (!IsCommandValid())
                        return;
                    
                    checkBox.IsChecked = (bool)field.GetValue(ArgumentHandler)!;
                    IsUnsaved = false;
                };

                checkBox.Checked += (s, _) => setValue(s);
                checkBox.Unchecked += (s, _) => setValue(s);
                
                subPanel.Children.Add(checkBox);
            } 
            else if (field.FieldType == typeof(int))
            {
                var textBox = new TextBox { Name = argumentInfo.DisplayName.MakeFriendly() };
                if (!string.IsNullOrEmpty(argumentInfo.Placeholder))
                    textBox.PlaceholderText = argumentInfo.Placeholder;
                
                bool IsNumber(string text)
                {
                    var regex = new Regex("[^0-9.-]+");
                    return regex.IsMatch(text);
                }
                
                textBox.PreviewTextInput += (_, e) =>
                {
                    e.Handled = IsNumber(e.Text);
                };
                
                textBox.TextChanged += (s, _) =>
                {
                    // https://karlhulme.wordpress.com/2007/02/15/masking-input-to-a-wpf-textbox/
                    var selectionStart = textBox.SelectionStart;
                    var newText = string.Empty;
                    foreach (var c in textBox.Text)
                    {
                        if (char.IsDigit(c) || char.IsControl(c))
                            newText += c;
                    }

                    textBox.Text = newText;
                    textBox.SelectionStart =
                        selectionStart <= textBox.Text.Length ? selectionStart : textBox.Text.Length;
                    
                    IsUnsaved = true;
                    var sender = (TextBox)s;
                    var handlerFields = ArgumentHandler.GetType().GetFields();

                    foreach (var f in handlerFields)
                    {
                        var info = f.GetArgumentAttribute();
                        if (info.DisplayName.MakeFriendly() != sender.Name)
                            continue;

                        if (f.FieldType != typeof(int))
                            throw new InvalidDataException($"'{info.DisplayName}' is not a string. Unable to set.");

                        if (!int.TryParse(sender.Text, out var number))
                        {
                            LogWarn($"TextBox '{info.DisplayName}' tried to enter non-int32 value. '{sender.Text}'");

                            var oldText = sender.Text;
                            sender.Text = string.Empty;
                            sender.PlaceholderText = string.IsNullOrEmpty(oldText) ? "Please enter a valid integer." : $"'{oldText}' is not a valid integer (int32).";

                            if (argumentInfo.Placeholder is not null)
                            {
                                TaskUtils.StartTaskWithDelay(
                                    () =>
                                    {
                                        Dispatcher.Invoke(() => textBox.PlaceholderText = argumentInfo.Placeholder);
                                    }, 2000);
                            }

                            return;
                        }

                        f.SetValue(ArgumentHandler, number);
                        return;
                    }
                };
                
                textBox.Loaded += (_, _) =>
                {
                    if (!IsCommandValid())
                        return;

                    textBox.Text = Convert.ToString((int)field.GetValue(ArgumentHandler)!);
                    IsUnsaved = false;
                };
                
                subPanel.Children.Add(textBox);
            }
            else if (field.FieldType.IsEnum)
            {
                var comboBox = new ComboBox { Name = argumentInfo.DisplayName.MakeFriendly() };
                var names = field.FieldType.GetEnumNames();

                foreach (var item in names)
                    comboBox.Items.Add(new ComboBoxItem { Name = item, Content = item });

                comboBox.SelectionChanged += (s, _) =>
                {
                    IsUnsaved = true;
                    var sender = (ComboBox)s;
                    var value = Enum.ToObject(field.FieldType, sender.SelectedIndex);
                    SetArgumentValue(field.FieldType, sender.Name, value);
                };

                comboBox.Loaded += (_, _) =>
                {
                    var value = (int)field.GetValue(ArgumentHandler)!;
                    comboBox.SelectedIndex = value;
                };
                
                subPanel.Children.Add(comboBox);
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

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        if (_isRecording)
            return;
        
        _isRecording = true;
        RecordIcon.Foreground = new SolidColorBrush(Colors.Red);
        RecordIcon.Foreground = new SolidColorBrush(Colors.Red);
        RecordIcon.Symbol = SymbolRegular.RecordStop20;

        var listener = new Listener();
        listener.Initialize();
        listener.ListenSingleAsync((phrase, pronunciation) =>
        {
            Trigger = new CommandTrigger(null, pronunciation);
            RecordResult.Text = phrase;
        
            _isRecording = false;
            RecordIcon.Foreground = new SolidColorBrush(Colors.White);
            RecordIcon.Foreground = new SolidColorBrush(Colors.White);
            RecordIcon.Symbol = SymbolRegular.Record20;
        });
    }
}