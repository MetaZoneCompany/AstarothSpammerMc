using Microsoft.Win32.SafeHandles;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

static class Program
{
    [STAThread]
    static void Main()
    {
        try
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
        catch
        {
            Process.GetCurrentProcess().Kill();
            return;
        }
    }
}