global using static HeyWindows.App.Configs.ConfigSystem;
using System.Windows;
using HeyWindows.App.Configs;
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
        Logger.StartLogger("test.log");
        InitializeComponent();
    }

    public static Commander Commander;

    private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        ReloadConfig();
        
        Commander = new Commander();
        var container = new CommandContainer("Commands", ConfigData!.Commands);
        
        Commander.Initialize();
        Commander.InitializeContainer(container);
        Commander.Activate();
    }
}