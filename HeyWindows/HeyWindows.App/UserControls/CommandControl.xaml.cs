﻿using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using HeyWindows.Core.Commands;
using HeyWindows.Core.Listeners;
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

    public CommandTrigger? Trigger;
    
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
}