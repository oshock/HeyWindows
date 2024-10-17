global using static HeyWindows.App.Configs.ConfigSystem;
global using static HeyWindows.Core.Logging.Logger;
global using static HeyWindows.Core.Utils.BoolUtils;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using HeyWindows.App.UserControls;
using HeyWindows.Core.Commands;
using HeyWindows.Core.Logging;

namespace HeyWindows.App;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        StartLogger(LOG_FILEPATH);
        InitializeComponent();
    }
    
    public static Commander Commander = new();

    private void CommandCountCheck()
    {
        NoCommandsToShow.Visibility = CommandsPanel.Children.Count == 1 ? Visibility.Visible : Visibility.Collapsed;
    }
    
    private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        ReloadConfig();
        if (ConfigData is null)
            throw new NullReferenceException($"Config is null. '{CONFIG_FILEPATH}'");
        
        var container = new CommandContainer("Commands", ConfigData.Commands);
        
        Commander.Initialize();
        Commander.InitializeContainer(container, true);
        Commander.Activate();

        foreach (var command in ConfigData.Commands)
        {
            CommandsPanel.Children.Add(new CommandControl(command, RemoveCommand));
        }

        CommandCountCheck();
    }

    private void AddButton_OnClick(object sender, RoutedEventArgs e)
    {
        CommandsPanel.Children.Insert(0, new CommandControl(null, RemoveCommand));
        CommandCountCheck();
    }

    public delegate void RemoveCallback(CommandControl command);
    
    public void RemoveCommand(CommandControl command)
    {
        if (command.LoadedCommand is not null)
            ConfigData!.Commands.Remove(command.LoadedCommand);
       
        CommandsPanel.Children.Remove(command);
        
        SaveConfig();
        CommandCountCheck();
    }
    
    private void AddButton_MouseEnter(object sender, MouseEventArgs e)
    {
        ((Button)sender).Background = new SolidColorBrush(Color.FromRgb(77, 149, 255));
    }

    private void AddButton_MouseLeave(object sender, MouseEventArgs e)
    {
        ((Button)sender).Background = new SolidColorBrush(Color.FromRgb(37, 109, 217));
    }
}