using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using CommonsLibrary;
using WnekoTankControlApp.CommandControl;
using WnekoTankControlApp.CommandControl.ComDevices;

namespace WnekoTankControlApp
{
    /// <summary>
    /// Partial class for readability containing all button click handlers
    /// </summary>
    public partial class MainWindow
    {
        List<Button> connectButtons;
        List<IPAddress> iPAddresses;
        string[] serialPorts;

        private void PortRefreshButton_Click(object sender, RoutedEventArgs e)
        {
            GetSerialPorts();
        }

        private void GetSerialPorts()
        {
            serialPorts = SerialPort.GetPortNames().Distinct().ToArray();
            portBox.ItemsSource = serialPorts;
            if (serialPorts.Length > 0) portBox.SelectedItem = portBox.Items[0];
        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (Button btn in connectButtons) btn.IsEnabled = true;
            WiFiStatusBox.Text = "Unknown";
            WiFiStatusBox.Background = Brushes.Transparent;
            outQueue.StopSending();
            DisconnectButton.IsEnabled = false;
            serialCom?.ClosePort();
            udpCom?.ClosePort();
            outQueue = null;
            inQueue = null;
            wifiUdpConnect.IsEnabled = true;
            wifiListBox.IsEnabled = true;
            localPortBox.IsEnabled = true;
            wifiListRefresh.IsEnabled = true;
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            string port = portBox.SelectedItem.ToString();
            try
            {
                if ((bool)usbComRadio.IsChecked) serialCom = new ComPortCommunication(port);
                else serialCom = new HC12Communication(port);
                communication = new CommunicationWrapper();
                (communication as CommunicationWrapper).Com = serialCom;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            await Connect();
        }

        private async void MockConnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                (communication as CommunicationWrapper).Com = new MockCommunication();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            await Connect();
        }

        private async Task Connect()
        {
            foreach (Button btn in connectButtons) btn.IsEnabled = false;
            DisconnectButton.IsEnabled = true;
            OutputBox.Text += "Connecting... \r\n";
            outQueue = new OutgoingMessageQueue(communication, DisplayMessage);
            inQueue = new IncommingMessageQueue();
            communication.SubscribeToMessages(outQueue.DataReceived);
            communication.SubscribeToMessages(inQueue.IncommingMessageHandler);
            RegisterMethods();

            await Task.Delay(1000);
            Send(TankCommandList.handshake);
            await Task.Delay(1000);
            Send(TankCommandList.handshake);
            await Task.Delay(500);
            Send(TankCommandList.hello);
        }

        public void SwitchToUdp()
        {
            try
            {
                string localIp = wifiListBox.SelectedItem.ToString();
                int localPort = int.Parse(localPortBox.Text);
                IPEndPoint lp = new IPEndPoint(IPAddress.Parse(localIp), localPort);
                //Keeping serial port open as emergency way
                //communication.ClosePort();
                udpCom = new WifiUdpCommunication(lp);
                (communication as CommunicationWrapper).Com = udpCom;
                //communication.SubscribeToMessages(inQueue.IncommingMessageHandler);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n\n" + ex.StackTrace, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        public void SwitchToSerial()
        {
            string port = portBox.SelectedItem.ToString();
            try
            {
                //if ((bool)usbComRadio.IsChecked) serialCom = new ComPortCommunication(port);
                //else serialCom = new HC12Communication(port);
                communication.ClosePort();
                (communication as CommunicationWrapper).Com = serialCom;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private void WifiListRefresh_Click(object sender, RoutedEventArgs e)
        {
            GetIPAddresses();
        }

        private void GetIPAddresses()
        {
            iPAddresses = Dns.GetHostAddresses(Dns.GetHostName()).
                            Where(x => ((bool)ipv4box.IsChecked && x.AddressFamily == AddressFamily.InterNetwork) ||
                            ((bool)ipv6box.IsChecked && x.AddressFamily == AddressFamily.InterNetworkV6)).ToList();
            wifiListBox.ItemsSource = iPAddresses;
            if (iPAddresses.Count > 0) wifiListBox.SelectedItem = wifiListBox.Items[0];
            else MessageBox.Show("No adapter found!", "Can't connect!", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void WifiUdpConnect_Click(object sender, RoutedEventArgs e)
        {
            bool parsed = int.TryParse(localPortBox.Text, out int localPort);
            if (parsed && wifiListBox.Items.Count > 0 && ConnectButton.IsEnabled == false)
            {
                string msg = TankCommandList.connectUdp + wifiListBox.SelectedItem.ToString() + ";" + localPort;
                Send(msg);
                wifiUdpConnect.IsEnabled = false;
                wifiListBox.IsEnabled = false;
                localPortBox.IsEnabled = false;
                wifiListRefresh.IsEnabled = false;
            }
            else if (wifiListBox.Items.Count == 0) MessageBox.Show("No IP selected!", "Can't connect!", MessageBoxButton.OK, MessageBoxImage.Error);
            else MessageBox.Show("Incorrect port value!", "Can't connect!", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void CheckWiFiBtn_Click(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.checkWiFiStatus;
            Send(msg);
        }

        private void ConnectToWiFiBtn_Click(object sender, RoutedEventArgs e)
        {
            string msg = TankCommandList.connectToWiFi;
            Send(msg);
        }
    }
}
