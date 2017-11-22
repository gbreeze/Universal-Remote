using System;
using System.Runtime.InteropServices;

namespace RemoteService
{
    public class MediaService
    {
        // http://www.blackwasp.co.uk/basicvolumecontrol.aspx
        const int WM_APPCOMMAND = 0x319;
        const int APPCOMMAND_VOLUME_MUTE = 0x80000;
        const int APPCOMMAND_VOLUME_DOWN = 0x90000;
        const int APPCOMMAND_VOLUME_UP = 0xA0000;

        public void Send(string cmd)
        {
            if (cmd == "play-pause")
                keybd_event(179, 0, 0, 0);
            if (cmd == "forward")
                keybd_event(176, 0, 0, 0);
            if (cmd == "backward")
                keybd_event(177, 0, 0, 0);
            if (cmd == "volume-off")
                SendMessage(FindWindow(null, null), WM_APPCOMMAND, 0, APPCOMMAND_VOLUME_MUTE);
            if (cmd.StartsWith("volume-up"))
            {
                int num;
                try
                {
                    num = Convert.ToInt32(cmd.Replace("volume-up", ""));
                }
                catch
                {
                    num = 1;
                }
                for (int index = 0; index < num; ++index)
                    SendMessage(FindWindow(null, null), WM_APPCOMMAND, 0, APPCOMMAND_VOLUME_UP);
            }
            if (cmd.StartsWith("volume-down"))
            {
                int num;
                try
                {
                    num = Convert.ToInt32(cmd.Replace("volume-down", ""));
                }
                catch
                {
                    num = 1;
                }
                for (int index = 0; index < num; ++index)
                    SendMessage(FindWindow(null, null), WM_APPCOMMAND, 0, APPCOMMAND_VOLUME_DOWN);
            }
        }

        [DllImport("User32.dll")]
        private static extern int SendMessage(int hWnd, int Msg, int wParam, int lParam);

        [DllImport("User32.dll")]
        private static extern int FindWindow(string lpClassName, string lpWindowName);

        [DllImport("User32.dll")]
        private static extern int keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

    }
}
