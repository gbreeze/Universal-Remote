using System;
using System.Windows;

namespace WpfApplication
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        System.Windows.Forms.NotifyIcon icon = new System.Windows.Forms.NotifyIcon();
        public string StartupArgument { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            if (e.Args.Length > 0)
                StartupArgument = e.Args[0];

            icon.Click += new EventHandler(icon_Click);
            icon.DoubleClick += new EventHandler(icon_Click);
            icon.Icon = new System.Drawing.Icon(typeof(App), "TrayIcon.ico");
            icon.Visible = true;

            base.OnStartup(e);
        }

        private void icon_Click(object sender, EventArgs e)
        {
            if (MainWindow != null)
            {
                MainWindow.Show();
                MainWindow.WindowState = WindowState.Normal;
                MainWindow.Activate();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            icon.Visible = false;
            base.OnExit(e);
        }
    }
}
