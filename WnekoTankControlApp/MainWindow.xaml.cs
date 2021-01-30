using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CommonsLibrary;

namespace WnekoTankControlApp
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CommandsList comList = new CommandsList();
        ICommunication communication;
        MessageQueue queue;
        ObservableCollection<string> commandList = new ObservableCollection<string>();
        private Boolean AutoScroll = true;
        public MainWindow()
        {
            InitializeComponent();
            queueList.ItemsSource = commandList;
            commandList.Add(CommandList.emergencyPrefix + CommandList.stopInvoking);
            commandList.Add(CommandList.emergencyPrefix + CommandList.clearQueue);
            connectButtons = new List<Button>();
            connectButtons.Add(ConnectButton);
            connectButtons.Add(mockConnectButton);
        }

        /// <summary>
        /// Allows other classes to easily print text in main app window
        /// </summary>
        /// <param name="msg">String to be printed</param>
        public void DisplayMessage(string msg)
        {
            Dispatcher.Invoke(() => outputBox.Text += msg + "\r\n");
        }
        
        /// <summary>
        /// Makes sure on app closing communication port is closed and app is correctly stopped
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            communication?.ClosePort();
            Environment.Exit(0);
        }

        /// <summary>
        /// Scrolls message box
        /// Based on: https://stackoverflow.com/questions/2984803/how-to-automatically-scroll-scrollviewer-only-if-the-user-did-not-change-scrol
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Args</param>
        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // User scroll event : set or unset auto-scroll mode
            if (e.ExtentHeightChange == 0)
            {   // Content unchanged : user scroll event
                if (ScrollView.VerticalOffset == ScrollView.ScrollableHeight)
                {   // Scroll bar is in bottom
                    // Set auto-scroll mode
                    AutoScroll = true;
                }
                else
                {   // Scroll bar isn't in bottom
                    // Unset auto-scroll mode
                    AutoScroll = false;
                }
            }

            // Content scroll event : auto-scroll eventually
            if (AutoScroll && e.ExtentHeightChange != 0)
            {   // Content changed and auto-scroll mode set
                // Autoscroll
                ScrollView.ScrollToVerticalOffset(ScrollView.ExtentHeight);
            }
        }
    }
}
