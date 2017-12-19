using System;
using System.Configuration;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using Microsoft.Owin.Hosting;
using Microsoft.Win32;
using RemoteService;

namespace WpfApplication
{
    public partial class MainWindow : Window
    {
        const string backgroundArgument = "--background";
        RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        readonly string port = ConfigurationManager.AppSettings["Port"];

        private IDisposable Service { get; set; }

        public MainWindow()
        {
            var startArgument = (Application.Current as App).StartupArgument;
            if (startArgument != null && startArgument == backgroundArgument)
            {
                this.WindowState = WindowState.Minimized;
                this.Hide();
            }

            InitializeComponent();

            this.btnSetup.Content += this.port;
            this.info.Content = $"Version {Assembly.GetEntryAssembly().GetName().Version.ToString().Substring(0, 5)} by styrit.com";
            this.autostart.IsChecked = rkApp.GetValue(Assembly.GetEntryAssembly().GetName().Name) != null;

            this.StartService();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
                this.Hide();

            base.OnStateChanged(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            if (this.Service != null)
                this.Service.Dispose();

            base.OnClosed(e);
        }

        private void StartService()
        {
            this.error.Text = string.Empty;
            this.pannelOK.Visibility = Visibility.Visible;
            this.pannelError.Visibility = Visibility.Collapsed;

            if (string.IsNullOrWhiteSpace(this.port))
            {
                this.error.Text = "ERROR: No 'PORT' defined!";
            }
            else
            {
                try
                {
                    // issue: service is not accessible from remote machines
                    // https://stackoverflow.com/questions/21634333/hosting-webapi-using-owin-in-a-windows-service
                    // https://forums.asp.net/t/1881253.aspx?More+SelfHost+Documentation
                    // https://stackoverflow.com/questions/24976425/running-self-hosted-owin-web-api-under-non-admin-account

                    // To support the MachineName, we have to execute the following command in the cmd with admin rights.
                    // https://technet.microsoft.com/en-us/library/cc725882(v=ws.10).aspx#BKMK_9
                    // netsh http add urlacl url=http://*:7055/ user=Everyone
                    // netsh http delete urlacl url=http://*:7055/

                    // [obsolete] as a workaround, we request admin rights to start this application
                    // autostart is not working if app requests admin rights

                    string baseAddress = $"http://*:{port}/";
                    this.serviceAddress.Text = baseAddress.Replace("*", Environment.MachineName);

                    // Start OWIN host 
                    Service = WebApp.Start<Startup>(baseAddress);
                }
                catch (Exception ex)
                {
                    this.error.Text = $"Service could not start. {(ex.InnerException != null ? ex.InnerException.Message : ex.Message)}";

                    if (ex.InnerException != null)
                    {
                        if (ex.InnerException is System.Net.HttpListenerException innerEx)
                        {
                            if (innerEx.ErrorCode == 5)
                            {
                                // do initial setup
                                this.pannelOK.Visibility = Visibility.Collapsed;
                                this.pannelError.Visibility = Visibility.Visible;
                            }
                            if (innerEx.ErrorCode == 183)
                            {
                                // app already running?
                                this.error.Text = $"Service could not start. There is probably already a running instance of Remote Control or the port '{port}' is already used by another application.";
                            }
                        }
                    }
                }
            }
        }

        private void RunCmd(string command)
        {
            try
            {
                var procStartInfo = new ProcessStartInfo(@"cmd.exe", "/c " + command);
                procStartInfo.UseShellExecute = true;
                procStartInfo.CreateNoWindow = true;
                procStartInfo.Verb = "runas";

                using (var proc = new Process())
                {
                    proc.StartInfo = procStartInfo;
                    proc.Start();
                    proc.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Autostart_Click(object sender, RoutedEventArgs e)
        {
            if (autostart.IsChecked.Value)
            {
                // Add the value in the registry so that the application runs at startup
                rkApp.SetValue(Assembly.GetEntryAssembly().GetName().Name,
                  $"\"{Assembly.GetEntryAssembly().Location}\" {backgroundArgument}");
            }
            else
            {
                // Remove the value from the registry so that the application doesn't start
                rkApp.DeleteValue(Assembly.GetEntryAssembly().GetName().Name, false);
            }
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(this.serviceAddress.Text);
        }

        private void Version_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start("http://styrit.com");
        }

        private void Setup_Click(object sender, RoutedEventArgs e)
        {
            btnSetup.IsEnabled = false;

            this.RunCmd($@"netsh http add urlacl url=http://*:{port}/ user=Everyone");
            this.StartService();

            btnSetup.IsEnabled = true;
        }
    }
}
