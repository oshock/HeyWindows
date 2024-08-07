global using static HeyWindows.App.Configs.ConfigSystem;
using System.Windows;
using HeyWindows.App.Configs;
using HeyWindows.Core.Commands;

namespace HeyWindows.App;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public static Commander Commander;

    private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        ReloadConfig();
        
        Commander = new Commander();
        var container = new CommandContainer("Commands", ConfigSystem.ConfigData?.Commands ?? new List<Command>());
        container.Commands.Add(new Command()
        {
            Triggers = new()
            {
                new CommandTrigger("Root")
            }
        });
        
        Commander.Initialize();
        Commander.InitializeContainer(container);
        Commander.Activate();
    }
}