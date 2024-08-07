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
    public Command? Command { get; set; }
    
    public CommandControl()
    {
        InitializeComponent();
    }
    
    public CommandControl(Command command) : this()
    {
        Command = command;
    }

    private bool _isRecording;

    private CommandTrigger? _trigger;
    public CommandTrigger? Trigger
    {
        get => _trigger;
        protected set
        {
            _trigger = value;
            var selected = (ComboBoxItem)ActionType.SelectionBoxItem;
            var command = Command.Create(selected.Name, new ExecutableCommandArgs(), 
                new List<CommandTrigger>
                {
                    Trigger!
                });
            
            ConfigData.Commands.Add(command);
        }
    }
    
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

    private void ActionType_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var argumentHandler = ActionType.SelectionBoxItem.Cast<ComboBoxItem>()!.Name switch
        {
            "Executable" => new ExecutableExecutor().ArgumentHandler,
            _ => throw new KeyNotFoundException()
        };

        var fields = argumentHandler.GetType().GetFields();
        foreach (var field in fields)
        {
            var argumentInfo = (ArgumentFieldAttribute)field.GetCustomAttributes(typeof(ArgumentFieldAttribute), false).First();
            var header = new TextBlock { Text = argumentInfo.DisplayName };
            var description = new TextBlock { Text = argumentInfo.Description };
            
            if (field.FieldType == typeof(string))
            {
                var textBox = new TextBox { PlaceholderText = "Ex. 'C:\\FolderPath\\File'..." };
            }
        }
    }
}