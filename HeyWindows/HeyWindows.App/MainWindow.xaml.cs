using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
        ConfigReader.Reload();
        
        Commander = new Commander();
        var container = new CommandContainer("Commands", ConfigReader.Config?.Commands ?? new List<Command>());
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