﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Net.NetworkInformation;
using HandyControl.Controls;
using System.Configuration;

namespace LocalProxy
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        [DllImport("wininet.dll")]
        private static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);
        private const int INTERNET_OPTION_SETTINGS_CHANGED = 39;
        private const int INTERNET_OPTION_REFRESH = 37;

        private bool isConnected = false;

        public MainWindow()
        {
            InitializeComponent();

            string ServerIP_Saved = ConfigurationManager.AppSettings["ServerIP"];
            string ServerPort_Saved = ConfigurationManager.AppSettings["ServerPort"];

            ServerIP.Text = ServerIP_Saved;
            ServerPort.Text = ServerPort_Saved;
        }

        private void Set_Proxy_hdlr(object sender, RoutedEventArgs e)
        {
            Color StartColorHex = (Color)ColorConverter.ConvertFromString("#326CF3");
            Color TerminateColorHex = (Color)ColorConverter.ConvertFromString("#DB3340");
            SolidColorBrush StartColor = new SolidColorBrush(StartColorHex);
            SolidColorBrush TerminateColor = new SolidColorBrush(TerminateColorHex);

            Geometry StartGeometry = (Geometry)FindResource("AudioGeometry");
            Geometry TeminateGeometry = (Geometry)FindResource("AlignBottomGeometry");

            if ((!string.IsNullOrEmpty(ServerIP.Text)) && (!string.IsNullOrEmpty(ServerPort.Text)))
            {
                if (TestConnectivity(ServerIP.Text))
                {
                    string proxyhost = $"http://{ServerIP.Text}:{ServerPort.Text}";

                    Parameters_Saved();

                    try
                    {
                        const string userRoot = "HKEY_CURRENT_USER";
                        const string subkey = @"Software\Microsoft\Windows\CurrentVersion\Internet Settings";
                        const string keyName = userRoot + @"\" + subkey;
                        Registry.SetValue(keyName, "ProxyServer", isConnected ? "":proxyhost);
                        Registry.SetValue(keyName, "ProxyOverride", "<local>");
                        Registry.SetValue(keyName, "ProxyEnable", isConnected ? "0":"1", RegistryValueKind.DWord);
                        InternetSetOption(IntPtr.Zero, INTERNET_OPTION_SETTINGS_CHANGED, IntPtr.Zero, 0);
                        InternetSetOption(IntPtr.Zero, INTERNET_OPTION_REFRESH, IntPtr.Zero, 0);

                        ProxyButton.Background = isConnected ? StartColor : TerminateColor;
                        ProxyButton.ToolTip = isConnected ? "开启服务器代理" : "关闭服务器代理";
                        ProxyButton.SetCurrentValue(IconElement.GeometryProperty, isConnected ? StartGeometry : TeminateGeometry);

                        ServerIP.IsEnabled = isConnected ? true: false;
                        ServerPort.IsEnabled = isConnected ? true : false;

                        //更新连接状态
                        isConnected = !isConnected;
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("proxy_error");
                    }
                }
                
            }
        }

        private bool TestConnectivity(string ipAddress)
        {
            bool serverOn = false;
            try
            {
                Ping ping = new Ping();
                PingReply reply = ping.Send(ipAddress);

                if (reply.Status == IPStatus.Success)
                {
                    Console.WriteLine($"服务器连接成功! Roundtrip time: {reply.RoundtripTime} ms");
                    serverOn = true;
                }
                else
                {
                    Console.WriteLine($"无法连接到服务器: {reply.Status}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"连接服务器失败，Erroe: {ex.Message}");
            }

            return serverOn;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Parameters_Saved();
        }

        private void Parameters_Saved()
        { 
            Configuration cfa = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            cfa.AppSettings.Settings["ServerIP"].Value = ServerIP.Text;
            cfa.AppSettings.Settings["ServerPort"].Value = ServerPort.Text;
            cfa.Save();
        }
    }
}
