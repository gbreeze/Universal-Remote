using System;
using System.Configuration;
using System.Windows;
using RemoteService;
using Microsoft.Owin.Hosting;
using Microsoft.Win32;
using System.Reflection;

namespace WpfApplication
{
    public partial class MainWindow : Window
    {
        const string backgroundArgument = "--background";
        RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

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

        private void StartService()
        {
            string port = ConfigurationManager.AppSettings["Port"];
            if (string.IsNullOrWhiteSpace(port))
            {
                this.error.Text = "ERROR: No 'PORT' defined!";
            }
            else
            {
                try
                {
                    // issue: not accesable from remote machine
                    // https://stackoverflow.com/questions/21634333/hosting-webapi-using-owin-in-a-windows-service
                    // https://forums.asp.net/t/1881253.aspx?More+SelfHost+Documentation
                    // https://stackoverflow.com/questions/24976425/running-self-hosted-owin-web-api-under-non-admin-account

                    // To support the MachineName, we have to execute the following command in the cmd with admin rights.
                    // netsh http add urlacl url=http://*:7055/ user=Everyone

                    // as a workaround, we request admin rights to start this application

                    string baseAddress = $"http://*:{port}/";
                    this.serviceAddress.Text = baseAddress.Replace("*", Environment.MachineName);

                    // Start OWIN host 
                    Service = WebApp.Start<Startup>(baseAddress);
                }
                catch (Exception ex)
                {
                    this.error.Text = $"Service could not start. {(ex.InnerException != null ? ex.InnerException.Message : ex.Message)}";
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            this.Service.Dispose();
            base.OnClosed(e);
        }

        private void autostart_Click(object sender, RoutedEventArgs e)
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

        private void open_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(this.serviceAddress.Text);
        }

        private void version_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("http://styrit.com");
        }
    }
}
