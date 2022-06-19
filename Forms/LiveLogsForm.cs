using System;
using System.Diagnostics;
using System.Windows.Forms;

public partial class LiveLogsForm : MetroSuite.MetroForm
{
    public LiveLogsForm()
    {
        try
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;

            new System.Threading.Thread(updateAll).Start();

            pictureBox1.Location = new System.Drawing.Point(713, 4);
            pictureBox2.Location = new System.Drawing.Point(683, 4);
            pictureBox3.Location = new System.Drawing.Point(653, 4);
        }
        catch
        {

        }
    }

    public void updateAll()
    {
        while (true)
        {
            try
            {
                System.Threading.Thread.Sleep(10);

                if (!Utils.hideLiveLogs)
                {
                    try
                    {
                        if (richTextBox1.Text.Length >= 2147483000)
                        {
                            richTextBox1.Text = "";
                        }

                        richTextBox1.Text += Utils.queue[0];

                        Utils.queue.RemoveAt(0);

                        if (richTextBox1.Text.Length >= 2147483000)
                        {
                            richTextBox1.Text = "";
                        }
                    }
                    catch
                    {

                    }
                }

                if (Utils.hideLiveLogs)
                {
                    this.Visible = false;
                }
                else
                {
                    this.Visible = true;
                }
            }
            catch
            {

            }
        }
    }

    private void LiveLogsForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        try
        {
            Process.GetCurrentProcess().Kill();
        }
        catch
        {

        }
    }

    private void pictureBox1_Click(object sender, System.EventArgs e)
    {
        try
        {
            Process.GetCurrentProcess().Kill();
        }
        catch
        {

        }
    }

    private void pictureBox2_Click(object sender, System.EventArgs e)
    {
        try
        {
            if (WindowState == FormWindowState.Maximized)
            {
                WindowState = FormWindowState.Normal;
            }
            else
            {
                WindowState = FormWindowState.Maximized;
            }
        }
        catch
        {

        }
    }

    private void pictureBox3_Click(object sender, System.EventArgs e)
    {
        try
        {
            WindowState = FormWindowState.Minimized;
        }
        catch
        {

        }
    }

    private void pictureBox1_MouseEnter(object sender, EventArgs e)
    {
        pictureBox1.Size = new System.Drawing.Size(22, 22);
    }

    private void pictureBox1_MouseLeave(object sender, EventArgs e)
    {
        pictureBox1.Size = new System.Drawing.Size(24, 24);
    }

    private void pictureBox2_MouseEnter(object sender, EventArgs e)
    {
        pictureBox2.Size = new System.Drawing.Size(22, 22);
    }

    private void pictureBox2_MouseLeave(object sender, EventArgs e)
    {
        pictureBox2.Size = new System.Drawing.Size(24, 24);
    }

    private void pictureBox3_MouseEnter(object sender, EventArgs e)
    {
        pictureBox3.Size = new System.Drawing.Size(22, 22);
    }

    private void pictureBox3_MouseLeave(object sender, EventArgs e)
    {
        pictureBox3.Size = new System.Drawing.Size(24, 24);
    }
}