using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using HeyWindows.Core.Commands;
using Wpf.Ui.Controls;

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
    
    private void Record_OnClick(object sender, MouseButtonEventArgs e)
    {
        _isRecording = !_isRecording;
        if (_isRecording)
        {
            recordText.Foreground = new SolidColorBrush(Colors.Red);
            recordButton.Foreground = new SolidColorBrush(Colors.Red);
            recordButton.Symbol = SymbolRegular.RecordStop20;
        }
        else
        {
            recordText.Foreground = new SolidColorBrush(Colors.White);
            recordButton.Foreground = new SolidColorBrush(Colors.White);
            recordButton.Symbol = SymbolRegular.Record20;
        }
    }
}