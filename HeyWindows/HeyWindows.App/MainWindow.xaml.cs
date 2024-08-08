global using static HeyWindows.App.Configs.ConfigSystem;
using System.Windows;
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
        Logger.StartLogger(LOG_FILEPATH);
        InitializeComponent();
    }

    public static Commander Commander = new();

    private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        ReloadConfig();
        if (ConfigData is null)
            throw new NullReferenceException($"Config is null. '{CONFIG_FILEPATH}'");
        
        var container = new CommandContainer("Commands", ConfigData.Commands);
        
        Commander.Initialize();
        Commander.InitializeContainer(container);
        Commander.Activate();

        foreach (var command in ConfigData.Commands)
        {
            CommandsPanel.Children.Add(new CommandControl(command));
        }
    }

    private void SaveButton_OnClick(object sender, RoutedEventArgs e)
    {
        CommandsPanel.Children.Insert(0, new CommandControl());
    }
}