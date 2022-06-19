using Discord;
using Discord.Commands;
using Discord.Gateway;
using Discord.Media;
using Discord.WebSockets;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TwoCaptcha;
using TwoCaptcha.Captcha;
using TwoCaptcha.Exceptions;
using DiscordRPC;
using Microsoft.VisualBasic;
using WebSocketSharp;
using Newtonsoft.Json;

public partial class MainForm : MetroSuite.MetroForm
{
    public string inviteCode = "";
    public bool dmSpammer, serverSpammer, typingSpammer, webhookSpammer, guildRaidMode, friendSpamMode, downloadingMembers, downloadingRoles;
    public List<string> users = new List<string>(), roles = new List<string>(), blackListedTokens = new List<string>();
    public int tagAllIndex, rolesTagIndex;
    public List<Tuple<string, DiscordSocketClient>> sessions = new List<Tuple<string, DiscordSocketClient>>();
    public string theGuildId = "", theChannelId = "", theXCP64 = "", theChannelType = "";
    public string theGuildId1 = "", theChannelId1 = "", theXCP641 = "", theChannelType1 = "";
    private string[] formats = new string[] { "jpg", "png", "bmp", "jpeg", "jfif", "jpe", "rle", "dib", "svg", "svgz" };
    public string voiceSessionId = "";
    public List<Tuple<string, string>> languages = new List<Tuple<string, string>>();
    public DiscordRpcClient rpc;
    public int multipleMessageIndex = 0, multipleMessageDMIndex = 0;
    public bool downloadingUsers;

    private WebSocket ws;
    private List<string> ids;
    private List<string> idQueue;

    public MainForm()
    {
        if (System.Reflection.Assembly.GetExecutingAssembly() != System.Reflection.Assembly.GetCallingAssembly())
        {
            Process.GetCurrentProcess().Kill();
            return;
        }

        InitializeComponent();

        if (System.Reflection.Assembly.GetExecutingAssembly() != System.Reflection.Assembly.GetCallingAssembly())
        {
            Process.GetCurrentProcess().Kill();
            return;
        }

        CheckForIllegalCrossThreadCalls = false;
        Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;

        foreach (TabPage tabPage in firefoxMainTabControl1.TabPages)
        {
            tabPage.ForeColor = System.Drawing.Color.FromArgb(153, 153, 153);
            tabPage.BackColor = System.Drawing.Color.FromArgb(40, 40, 40);
        }

        if (!System.IO.File.Exists("tokens.txt"))
        {
            System.IO.File.WriteAllText("tokens.txt", "");
        }
        else
        {
            metroTextbox1.Text = System.IO.File.ReadAllText("tokens.txt");
        }

        if (!System.IO.File.Exists("proxies.txt"))
        {
            System.IO.File.WriteAllText("proxies.txt", "");
        }
        else
        {
            metroTextbox2.Text = System.IO.File.ReadAllText("proxies.txt");
        }

        if (System.Reflection.Assembly.GetExecutingAssembly() != System.Reflection.Assembly.GetCallingAssembly())
        {
            Process.GetCurrentProcess().Kill();
            return;
        }

        new LiveLogsForm().Show();

        metroComboBox1.SelectedIndex = 0;
        metroComboBox2.SelectedIndex = 0;

        string totalString = "All files (*.*)|*.*";

        foreach (string format in formats)
        {
            totalString += "|" + format.ToUpper() + " Image (*." + format + ")|*." + format;
        }

        openFileDialog2.Filter = totalString;
        openFileDialog2.DefaultExt = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        totalString = null;
        formats = null;

        pictureBox24.Location = new System.Drawing.Point(909, 4);
        pictureBox23.Location = new System.Drawing.Point(879, 4);
        pictureBox22.Location = new System.Drawing.Point(849, 4);

        pictureBox24.Size = new System.Drawing.Size(24, 24);
        pictureBox23.Size = new System.Drawing.Size(24, 24);
        pictureBox22.Size = new System.Drawing.Size(24, 24);

        if (System.Reflection.Assembly.GetExecutingAssembly() != System.Reflection.Assembly.GetCallingAssembly())
        {
            Process.GetCurrentProcess().Kill();
            return;
        }

        ids = new List<string>();
        idQueue = new List<string>();
    }

    public bool AreIDsValid(string ids)
    {
        try
        {
            ids = ids.Replace(" ", "").Replace('\t'.ToString(), "").Trim();

            if (!ids.Contains(","))
            {
                if (!IsIDValid(ids))
                {
                    return false;
                }
            }
            else
            {
                string[] splitted = Microsoft.VisualBasic.Strings.Split(ids, ",");

                foreach (string id in splitted)
                {
                    if (!IsIDValid(id))
                    {
                        return false;
                    }
                }

                splitted = null;
            }

            ids = null;

            return true;
        }
        catch
        {
            return false;
        }
    }

    public List<string> GetIDs(string ids)
    {
        List<string> idList = new List<string>();
        
        try
        {
            ids = ids.Replace(" ", "").Replace('\t'.ToString(), "").Trim();

            if (ids.Contains(","))
            {
                string[] splitted = Microsoft.VisualBasic.Strings.Split(ids, ",");

                foreach (string id in splitted)
                {
                    idList.Add(id);
                }

                splitted = null;
            }
            else
            {
                idList.Add(ids);
            }

            ids = null;
        }
        catch
        {

        }

        return idList;
    }

    public bool AreFriendsValid(string ids)
    {
        ids = ids.Replace(" ", "").Replace('\t'.ToString(), "").Trim();

        try
        {
            if (!ids.Contains(","))
            {
                if (!IsIDValid(ids) && !IsTagValid(ids))
                {
                    return false;
                }
            }
            else
            {
                string[] splitted = Microsoft.VisualBasic.Strings.Split(ids, ",");

                foreach (string id in splitted)
                {
                    if (!IsIDValid(id) && !IsTagValid(id))
                    {
                        return false;
                    }
                }

                splitted = null;
            }

            ids = null;

            return true;
        }
        catch
        {
            return false;
        }
    }

    public List<string> GetFriends(string ids)
    {
        List<string> idList = new List<string>();

        try
        {
            ids = ids.Replace(" ", "").Replace('\t'.ToString(), "").Trim();

            if (ids.Contains(","))
            {
                string[] splitted = Microsoft.VisualBasic.Strings.Split(ids, ",");

                foreach (string id in splitted)
                {
                    idList.Add(id);
                }

                splitted = null;
            }
            else
            {
                idList.Add(ids);
            }

            ids = null;
        }
        catch
        {

        }

        return idList;
    }

    public Tuple<bool, string, string> GetGroupInviteInformations(string inviteCode)
    {
        try
        {
            inviteCode = GetInviteCodeByInviteLink(inviteCode);

            if (IsIDValid(inviteCode))
            {
                return new Tuple<bool, string, string>(false, inviteCode, "");
            }

            Leaf.xNet.HttpRequest httpRequest = Utils.CreateCleanRequest();
            string[] proxy = null;

            try
            {
                if (metroChecker13.Checked)
                {
                    proxy = Microsoft.VisualBasic.Strings.Split(GetRandomProxy(), ":");
                    httpRequest.Proxy = new Leaf.xNet.HttpProxyClient(proxy[0], int.Parse(proxy[1]));
                }
                else
                {
                    httpRequest.Proxy = null;
                }
            }
            catch
            {

            }

            httpRequest.AddHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            httpRequest.AddHeader("Accept-Encoding", "gzip, deflate, br");
            httpRequest.AddHeader("Accept-Language", "it-IT,it;q=0.8,en-US;q=0.5,en;q=0.3");
            httpRequest.AddHeader("Connection", "keep-alive");
            httpRequest.AddHeader("Cookie", Utils.GetRandomCookie());
            httpRequest.AddHeader("DNT", "1");
            httpRequest.AddHeader("Host", "discord.com");
            httpRequest.AddHeader("TE", "Trailers");
            httpRequest.AddHeader("Upgrade-Insecure-Requests", "1");
            httpRequest.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:88.0) Gecko/20100101 Firefox/88.0");

            httpRequest.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;

            string response = Utils.DecompressResponse(httpRequest.Get("https://discord.com/api/v9/invites/" + inviteCode + "?with_counts=true"));
            dynamic jss = JObject.Parse(response);

            string guildId = "", channelId = "", channelType = "", statusCode = (string)jss.code;
            bool status = true;

            if (statusCode == "10006" || statusCode == "0" || statusCode != inviteCode)
            {
                status = false;
            }

            if (status)
            {
                channelId = (string)jss.channel.id;
                channelType = (string)jss.channel.type;
            }

            httpRequest = null;
            proxy = null;
            inviteCode = null;
            response = null;
            jss = null;
            statusCode = null;
            guildId = null;

            return new Tuple<bool, string, string>(status, channelId, channelType);
        }
        catch
        {
            return new Tuple<bool, string, string>(false, "", "");
        }
    }

    public string getTokenLanguage(string token)
    {
        try
        {
            if (blackListedTokens.Contains(token))
            {
                return "";
            }

            foreach (Tuple<string, string> theTuple in languages)
            {
                try
                {
                    if (theTuple.Item1 == token)
                    {
                        return theTuple.Item2;
                    }
                }
                catch
                {

                }
            }

            Leaf.xNet.HttpRequest httpRequest = Utils.CreateCleanRequest();
            string[] proxy = null;

            try
            {
                if (metroChecker13.Checked)
                {
                    proxy = Microsoft.VisualBasic.Strings.Split(GetRandomProxy(), ":");
                    httpRequest.Proxy = new Leaf.xNet.HttpProxyClient(proxy[0], int.Parse(proxy[1]));
                }
                else
                {
                    httpRequest.Proxy = null;
                }
            }
            catch
            {

            }

            httpRequest.AddHeader("Accept", "*");
            httpRequest.AddHeader("Accept-Encoding", "gzip, deflate, br");
            httpRequest.AddHeader("Accept-Language", "it");
            httpRequest.AddHeader("Authorization", token);
            httpRequest.AddHeader("Connection", "keep-alive");
            httpRequest.AddHeader("Cookie", Utils.GetRandomCookie());
            httpRequest.AddHeader("DNT", "1");
            httpRequest.AddHeader("Host", "discord.com");
            httpRequest.AddHeader("Origin", "https://discord.com");
            httpRequest.AddHeader("Referer", "https://discord.com/channels/@me");
            httpRequest.AddHeader("TE", "Trailers");
            httpRequest.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:88.0) Gecko/20100101 Firefox/88.0");
            httpRequest.AddHeader("X-Super-Properties", "eyJvcyI6IldpbmRvd3MiLCJicm93c2VyIjoiRmlyZWZveCIsImRldmljZSI6IiIsImJyb3dzZXJfdXNlcl9hZ2VudCI6Ik1vemlsbGEvNS4wIChXaW5kb3dzIE5UIDEwLjA7IFdpbjY0OyB4NjQ7IHJ2Ojg3LjApIEdlY2tvLzIwMTAwMTAxIEZpcmVmb3gvODcuMCIsImJyb3dzZXJfdmVyc2lvbiI6Ijg3LjAiLCJvc192ZXJzaW9uIjoiMTAiLCJyZWZlcnJlciI6IiIsInJlZmVycmluZ19kb21haW4iOiIiLCJyZWZlcnJlcl9jdXJyZW50IjoiIiwicmVmZXJyaW5nX2RvbWFpbl9jdXJyZW50IjoiIiwicmVsZWFzZV9jaGFubmVsIjoic3RhYmxlIiwiY2xpZW50X2J1aWxkX251bWJlciI6ODE2NDgsImNsaWVudF9ldmVudF9zb3VyY2UiOm51bGx9");

            httpRequest.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;

            string response = Utils.DecompressResponse(httpRequest.Get("https://discord.com/api/v9/users/@me/settings"));
            dynamic jss = JObject.Parse(response); 
            string locale = (string)jss.locale;

            languages.Add(new Tuple<string, string>(token, locale));

            token = null;
            response = null;
            jss = null;
            httpRequest = null;
            proxy = null;

            return locale;
        }
        catch (Exception ex)
        {
            return "";
        }
    }

    public Tuple<bool, string, string, string> GetInviteInformations(string inviteCode)
    {
        try
        {
            inviteCode = GetInviteCodeByInviteLink(inviteCode);

            if (IsIDValid(inviteCode))
            {
                return new Tuple<bool, string, string, string>(false, inviteCode, "", "");
            }

            Leaf.xNet.HttpRequest httpRequest = Utils.CreateCleanRequest();
            string[] proxy = null;

            try
            {
                if (metroChecker13.Checked)
                {
                    proxy = Microsoft.VisualBasic.Strings.Split(GetRandomProxy(), ":");
                    httpRequest.Proxy = new Leaf.xNet.HttpProxyClient(proxy[0], int.Parse(proxy[1]));
                }
                else
                {
                    httpRequest.Proxy = null;
                }
            }
            catch
            {

            }

            httpRequest.AddHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            httpRequest.AddHeader("Accept-Encoding", "gzip, deflate, br");
            httpRequest.AddHeader("Accept-Language", "it-IT,it;q=0.8,en-US;q=0.5,en;q=0.3");
            httpRequest.AddHeader("Connection", "keep-alive");
            httpRequest.AddHeader("Cookie", Utils.GetRandomCookie());
            httpRequest.AddHeader("DNT", "1");
            httpRequest.AddHeader("Host", "discord.com");
            httpRequest.AddHeader("TE", "Trailers");
            httpRequest.AddHeader("Upgrade-Insecure-Requests", "1");
            httpRequest.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:88.0) Gecko/20100101 Firefox/88.0");

            httpRequest.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;

            string response = Utils.DecompressResponse(httpRequest.Get("https://discord.com/api/v9/invites/" + inviteCode + "?inputValue=https://discord.gg/" + inviteCode + "&with_counts=true&with_expiration=true"));
            dynamic jss = JObject.Parse(response);

            string guildId = "", channelId = "", channelType = "", statusCode = (string)jss.code;
            bool status = true;

            if (statusCode == "10006" || statusCode == "0" || statusCode != inviteCode)
            {
                status = false;
            }

            if (status)
            {
                guildId = (string)jss.guild.id;
                channelId = (string)jss.channel.id;
                channelType = (string)jss.channel.type;
            }

            inviteCode = null;
            jss = null;
            response = null;
            proxy = null;
            httpRequest = null;

            return new Tuple<bool, string, string, string>(status, guildId, channelId, channelType);
        }
        catch
        {
            return new Tuple<bool, string, string, string>(false, "", "", "");
        }
    }

    public bool IsIDValid(string id)
    {
        try
        {
            if (id.Length != 18)
            {
                return false;
            }

            if (!Microsoft.VisualBasic.Information.IsNumeric(id))
            {
                return false;
            }
        }
        catch
        {

        }

        id = null;

        return true;
    }

    public void addLiveLog(string log)
    {
        try
        {
            if (metroChecker29.Checked)
            {
                Utils.queue.Add(Environment.NewLine + "[" + DateTime.Now.ToLongTimeString() + "] " + log);
            }

            log = null;
        }
        catch
        {

        }
    }

    public string getTagAllUser()
    {
        try
        {
            if (tagAllIndex > users.Count - 1)
            {
                tagAllIndex = users.Count - 1;
            }

            if (tagAllIndex == users.Count - 1)
            {
                tagAllIndex = 0;

                return users[users.Count - 1];
            }
            else if (users.Count == 1)
            {
                tagAllIndex = 0;

                return users[0];
            }
            else if (users.Count == 0)
            {
                return "";
            }
            else
            {
                int temp = tagAllIndex;

                tagAllIndex++;

                return users[temp];
            }
        }
        catch
        {

        }

        return "";
    }

    public string getRoleTag()
    {
        try
        {
            if (rolesTagIndex > roles.Count - 1)
            {
                rolesTagIndex = roles.Count - 1;
            }

            if (rolesTagIndex == roles.Count - 1)
            {
                rolesTagIndex = 0;

                return roles[roles.Count - 1];
            }
            else if (roles.Count == 1)
            {
                rolesTagIndex = 0;

                return roles[0];
            }
            else if (roles.Count == 0)
            {
                return "";
            }
            else
            {
                int temp = rolesTagIndex;

                rolesTagIndex++;

                return roles[temp];
            }
        }
        catch
        {

        }

        return "";
    }

    private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        try
        {
            Process.GetCurrentProcess().Kill();
        }
        catch
        {

        }
    }

    public static byte[] Post(string uri, NameValueCollection pairs)
    {
        try
        {
            using (WebClient webClient = new WebClient())
            {
                return webClient.UploadValues(uri, pairs);
            }
        }
        catch
        {
            return new byte[] { };
        }
    }

    private void metroButton1_Click(object sender, EventArgs e)
    {
        try
        {
            openFileDialog1.Title = "Open your tokens list...";

            if (openFileDialog1.ShowDialog().Equals(DialogResult.OK))
            {
                metroTextbox1.Text = System.IO.File.ReadAllText(openFileDialog1.FileName);
                openFileDialog1.FileName = "";
            }
        }
        catch
        {

        }
    }

    private void metroButton2_Click(object sender, EventArgs e)
    {
        try
        {
            openFileDialog1.Title = "Open your proxies list...";

            if (openFileDialog1.ShowDialog().Equals(DialogResult.OK))
            {
                metroTextbox2.Text = System.IO.File.ReadAllText(openFileDialog1.FileName);
                openFileDialog1.FileName = "";
            }
        }
        catch
        {

        }
    }

    private void metroTextbox1_TextChanged(object sender, EventArgs e)
    {
        try
        {
            System.IO.File.WriteAllText("tokens.txt", metroTextbox1.Text);
        }
        catch
        {

        }
    }

    private void metroTextbox2_TextChanged(object sender, EventArgs e)
    {
        try
        {
            System.IO.File.WriteAllText("proxies.txt", metroTextbox2.Text);
        }
        catch
        {

        }
    }

    private void metroTrackbar1_Scroll(object sender, MetroSuite.MetroTrackbar.TrackbarEventArgs e)
    {
        try
        {
            metroChecker3.Text = "Delay: " + metroTrackbar1.Value + "ms";
        }
        catch
        {

        }
    }

    private void metroTrackbar2_Scroll(object sender, MetroSuite.MetroTrackbar.TrackbarEventArgs e)
    {
        try
        {
            metroChecker8.Text = "Delay: " + metroTrackbar2.Value + "ms";
        }
        catch
        {

        }
    }

    private void metroTrackbar3_Scroll(object sender, MetroSuite.MetroTrackbar.TrackbarEventArgs e)
    {
        try
        {
            metroChecker9.Text = "Delay: " + metroTrackbar3.Value + "ms";
        }
        catch
        {

        }
    }

    private void metroTrackbar4_Scroll(object sender, MetroSuite.MetroTrackbar.TrackbarEventArgs e)
    {
        try
        {
            metroChecker10.Text = "Delay: " + metroTrackbar4.Value + "ms";
        }
        catch
        {

        }
    }

    private void metroTrackbar6_Scroll(object sender, MetroSuite.MetroTrackbar.TrackbarEventArgs e)
    {
        try
        {
            metroChecker12.Text = "Delay: " + metroTrackbar6.Value + "ms";
        }
        catch
        {

        }
    }

    private void metroButton6_Click(object sender, EventArgs e)
    {
        try
        {
            addLiveLog("Succesfully started Server Spammer for channel ID " + metroTextbox4.Text + ".");
            metroButton6.Enabled = false;

            serverSpammer = true;

            new Thread(doServerSpammer).Start();

            if (metroChecker25.Checked)
            {
                new Thread(doServerSpammer).Start();
            }

            metroButton5.Enabled = true;
        }
        catch
        {
            try
            {
                metroButton6.Enabled = true;
                metroButton5.Enabled = false;
                serverSpammer = false;
            }
            catch
            {

            }
        }
    }

    private void metroButton5_Click(object sender, EventArgs e)
    {
        try
        {
            addLiveLog("Succesfully stopped Server Spammer for channel ID " + metroTextbox4.Text + ".");
            metroButton5.Enabled = false;

            serverSpammer = false;

            metroButton6.Enabled = true;
        }
        catch
        {

        }
    }

    public void doServerSpammer()
    {
        try
        {
            if (metroChecker73.Checked)
            {
                if (!AreIDsValid(metroTextbox4.Text))
                {
                    metroButton5.Enabled = false;
                    metroButton6.Enabled = true;

                    MessageBox.Show("The ids of the channels are not valid!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }
            }
            else
            {
                if (!IsIDValid(metroTextbox4.Text))
                {
                    metroButton5.Enabled = false;
                    metroButton6.Enabled = true;

                    MessageBox.Show("The id of channel is not valid!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }
            }

            int i = 0, j = 0;

            if (metroChecker42.Checked)
            {
                try
                {
                    i = int.Parse(metroTextbox31.Text.Replace(" ", "").Replace('\t'.ToString(), ""));

                    if (i < 0)
                    {
                        i = 0;
                    }
                }
                catch
                {

                }
            }

            foreach (string token in SplitToLines(metroTextbox1.Text))
            {
                try
                {
                    if (blackListedTokens.Contains(token))
                    {
                        continue;
                    }

                    if (metroChecker42.Checked)
                    {
                        if (j == i)
                        {
                            break;
                        }

                        j++;
                    }

                    if (metroChecker8.Checked)
                    {
                        Thread.Sleep(metroTrackbar2.Value);
                    }

                    if (metroChecker63.Checked)
                    {
                        for (int z = 0; z <= 10; z++)
                        {
                            new Thread(() => spamServer(token)).Start();
                        }
                    }
                    else
                    {
                        new Thread(() => spamServer(token)).Start();
                    }
                }
                catch
                {

                }
            }
        }
        catch
        {

        }
    }

    public string ReplaceFirst(string text, string search, string replace)
    {
        try
        {
            int pos = text.IndexOf(search);

            if (pos < 0)
            {
                return text;
            }

            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }
        catch
        {

        }

        return text;
    }

    public void spamServer(string token)
    {
        try
        {
            string msg = getServerSpamMessage();

            string[] proxy = null;

            try
            {
                if (metroChecker13.Checked)
                {
                    proxy = Microsoft.VisualBasic.Strings.Split(GetRandomProxy(), ":");
                }
            }
            catch
            {

            }

            /*bool needToDelay = false;
            int delayMs = 0;*/

            while (true)
            {
                try
                {
                    if (serverSpammer)
                    {
                        if (blackListedTokens.Contains(token))
                        {
                            return;
                        }

                        /*if (needToDelay)
                        {
                            Thread.Sleep(delayMs);

                            delayMs = 0;
                            needToDelay = false;
                        }

                        if (metroChecker8.Checked)
                        {
                            Thread.Sleep(metroTrackbar2.Value);
                        }

                        if (needToDelay)
                        {
                            Thread.Sleep(delayMs);

                            delayMs = 0;
                            needToDelay = false;
                        }*/

                        /*if (metroChecker52.Checked)
                        {
                            if (metroChecker73.Checked)
                            {
                                try
                                {
                                    foreach (string id in GetIDs(metroTextbox4.Text))
                                    {
                                        new Thread(() => processMessage(token, ref proxy, ref msg, ref needToDelay, ref delayMs, id)).Start();
                                    }
                                }
                                catch
                                {

                                }
                            }
                            else
                            {
                                try
                                {
                                    new Thread(() => processMessage(token, ref proxy, ref msg, ref needToDelay, ref delayMs, metroTextbox4.Text)).Start();
                                }
                                catch
                                {

                                }
                            }
                        }
                        else
                        {
                            if (metroChecker73.Checked)
                            {
                                try
                                {
                                    foreach (string id in GetIDs(metroTextbox4.Text))
                                    {
                                        try
                                        {
                                            processMessage(token, ref proxy, ref msg, ref needToDelay, ref delayMs, id);
                                        }
                                        catch
                                        {

                                        }
                                    }
                                }
                                catch
                                {

                                }
                            }
                            else
                            {
                                try
                                {
                                    processMessage(token, ref proxy, ref msg, ref needToDelay, ref delayMs, metroTextbox4.Text);
                                }
                                catch
                                {

                                }
                            }
                        }*/

                        /*if (needToDelay)
                        {
                            Thread.Sleep(delayMs);

                            delayMs = 0;
                            needToDelay = false;
                        }*/

                        if (metroChecker52.Checked)
                        {
                            if (metroChecker73.Checked)
                            {
                                try
                                {
                                    foreach (string id in GetIDs(metroTextbox4.Text))
                                    {
                                        new Thread(() => processMessage(token, ref proxy, ref msg, id)).Start();
                                    }
                                }
                                catch
                                {

                                }
                            }
                            else
                            {
                                try
                                {
                                    new Thread(() => processMessage(token, ref proxy, ref msg, metroTextbox4.Text)).Start();
                                }
                                catch
                                {

                                }
                            }
                        }
                        else
                        {
                            if (metroChecker73.Checked)
                            {
                                try
                                {
                                    foreach (string id in GetIDs(metroTextbox4.Text))
                                    {
                                        try
                                        {
                                            processMessage(token, ref proxy, ref msg, id);
                                        }
                                        catch
                                        {

                                        }
                                    }
                                }
                                catch
                                {

                                }
                            }
                            else
                            {
                                try
                                {
                                    processMessage(token, ref proxy, ref msg, metroTextbox4.Text);
                                }
                                catch
                                {

                                }
                            }
                        }
                    }
                    else
                    {
                        proxy = null;
                        token = null;
                        msg = null;

                        return;
                    }
                }
                catch
                {

                }
            }
        }
        catch
        {

        }
    }

    public void processMessage(string token, ref string[] proxy, ref string msg, /*ref bool needToDelay, ref int delayMs,*/ string channelId)
    {
        /*if (needToDelay || delayMs > 0)
        {
            return;
        }*/

        try
        {
            string newMsg = msg;

            if (metroChecker1.Checked)
            {
                while (newMsg.Contains("[mtag]"))
                {
                    string tag = getTagAllUser();

                    if (tag != "")
                    {
                        newMsg = ReplaceFirst(newMsg, "[mtag]", " <@" + getTagAllUser() + "> ");
                    }              
                }
            }

            if (metroChecker32.Checked)
            {
                string tagAllUsers = "";

                int i = 0, limit = 0;

                if (metroChecker33.Checked)
                {
                    try
                    {
                        limit = int.Parse(metroTextbox23.Text);
                    }
                    catch
                    {

                    }
                }

                foreach (string user in roles)
                {
                    if (metroChecker33.Checked)
                    {
                        if (i == limit)
                        {
                            break;
                        }
                    }

                    i++;

                    if (tagAllUsers == "")
                    {
                        tagAllUsers = "<@&" + user + ">";
                    }
                    else
                    {
                        tagAllUsers += " <@&" + user + ">";
                    }
                }

                if (tagAllUsers != "" && tagAllUsers != "<@&>" && tagAllUsers != "<@&>")
                {
                    while (newMsg.Contains("[rall]"))
                    {
                        newMsg = ReplaceFirst(newMsg, "[rall]", tagAllUsers.Replace("<@&>", ""));
                    }
                }
            }

            if (metroChecker31.Checked)
            {
                while (newMsg.Contains("[rtag]"))
                {
                    string roleTag = getRoleTag();

                    if (roleTag != "")
                    {
                        newMsg = ReplaceFirst(newMsg, "[rtag]", " <@&" + roleTag + "> ");
                    }

                    roleTag = null;
                }
            }

            if (metroChecker78.Checked)
            {
                while (newMsg.Contains("[crashgif]"))
                {
                    newMsg = ReplaceFirst(newMsg, "[crashgif]", " https://gfycat.com/dazzlingsarcasticamurminnow ");
                }
            }

            if (metroChecker34.Checked)
            {
                while (newMsg.Contains("[lag]"))
                {
                    newMsg = ReplaceFirst(newMsg, "[lag]", Utils.GetLagMessage());
                }
            }

            if (metroChecker2.Checked)
            {
                string tagAllUsers = "";

                int i = 0, limit = 0;

                if (metroChecker21.Checked)
                {
                    try
                    {
                        limit = int.Parse(metroTextbox18.Text);
                    }
                    catch
                    {

                    }
                }

                foreach (string user in users)
                {
                    if (metroChecker21.Checked)
                    {
                        if (i == limit)
                        {
                            break;
                        }
                    }

                    i++;

                    if (tagAllUsers == "")
                    {
                        tagAllUsers = "<@" + user + ">";
                    }
                    else
                    {
                        tagAllUsers += " <@" + user + ">";
                    }
                }

                while (newMsg.Contains("[all]"))
                {
                    newMsg = ReplaceFirst(newMsg, "[all]", tagAllUsers);
                }
            }

            if (metroChecker19.Checked)
            {
                while (newMsg.Contains("[rndnum]"))
                {
                    newMsg = ReplaceFirst(newMsg, "[rndnum]", Utils.GetRandomNumber(1000, 9999).ToString());
                }

                while (newMsg.Contains("[rndstr]"))
                {
                    newMsg = ReplaceFirst(newMsg, "[rndstr]", Utils.RandomNormalString(16));
                }
            }

            string messageJson = "{\"content\":\"" + newMsg + "\", \"tts\": " + metroChecker6.Checked.ToString().ToLower() + ", \"nonce\": " + Utils.RandomNonce().ToString() + (IsIDValid(metroTextbox38.Text) ? ",\"message_reference\":{\"channel_id\":\"" + metroTextbox4.Text + "\",\"message_id\":\"" + metroTextbox38.Text + "\"}}" : "}");

            if (metroChecker60.Checked)
            {
                messageJson = "{\"embed\": { \"title\":\"Raid by AstarothSpammer\", \"description\":\"" + newMsg + "\" }, \"tts\": " + metroChecker6.Checked.ToString().ToLower() + ", \"nonce\": " + Utils.RandomNonce().ToString() + (IsIDValid(metroTextbox38.Text) ? ",\"message_reference\":{\"channel_id\":\"" + metroTextbox4.Text + "\",\"message_id\":\"" + metroTextbox38.Text + "\"}}" : "}");
            }

            Leaf.xNet.HttpRequest httpRequest = Utils.CreateCleanRequest();

            try
            {
                if (metroChecker13.Checked)
                {
                    if (proxy != null)
                    {
                        httpRequest.Proxy = new Leaf.xNet.HttpProxyClient(proxy[0], int.Parse(proxy[1]));
                    }
                    else
                    {
                        httpRequest.Proxy = null;
                    }
                }
            }
            catch
            {

            }

            httpRequest.AddHeader("Accept", "*/*");
            httpRequest.AddHeader("Accept-Encoding", "gzip, deflate, br");
            httpRequest.AddHeader("Accept-Language", getTokenLanguage(token));
            httpRequest.AddHeader("Authorization", token);
            httpRequest.AddHeader("Connection", "keep-alive");
            httpRequest.AddHeader("Content-Length", messageJson.Length.ToString());
            httpRequest.AddHeader("Content-Type", "application/json");
            httpRequest.AddHeader("Cookie", Utils.GetRandomCookie(getTokenLanguage(token)));
            httpRequest.AddHeader("DNT", "1");
            httpRequest.AddHeader("Host", "discord.com");
            httpRequest.AddHeader("Origin", "https://discord.com");
            httpRequest.AddHeader("TE", "Trailers");
            httpRequest.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:88.0) Gecko/20100101 Firefox/88.0");
            httpRequest.AddHeader("X-Super-Properties", "eyJvcyI6IldpbmRvd3MiLCJicm93c2VyIjoiRmlyZWZveCIsImRldmljZSI6IiIsImJyb3dzZXJfdXNlcl9hZ2VudCI6Ik1vemlsbGEvNS4wIChXaW5kb3dzIE5UIDEwLjA7IFdpbjY0OyB4NjQ7IHJ2Ojg3LjApIEdlY2tvLzIwMTAwMTAxIEZpcmVmb3gvODcuMCIsImJyb3dzZXJfdmVyc2lvbiI6Ijg3LjAiLCJvc192ZXJzaW9uIjoiMTAiLCJyZWZlcnJlciI6IiIsInJlZmVycmluZ19kb21haW4iOiIiLCJyZWZlcnJlcl9jdXJyZW50IjoiIiwicmVmZXJyaW5nX2RvbWFpbl9jdXJyZW50IjoiIiwicmVsZWFzZV9jaGFubmVsIjoic3RhYmxlIiwiY2xpZW50X2J1aWxkX251bWJlciI6ODE0NTIsImNsaWVudF9ldmVudF9zb3VyY2UiOm51bGx9");

            httpRequest.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;

            if (blackListedTokens.Contains(token))
            {
                return;
            }

            string result = Utils.DecompressResponse(httpRequest.Post("https://discord.com/api/v9/channels/" + channelId + "/messages", messageJson, "application/json"));
            addLiveLog(proxy == null ? "Token (" + token + ") sent a message in channel ID " + channelId + " -> " + result : "Token (" + token + ") with proxy (" + proxy[0] + ":" + proxy[1] + ") sent a message in channel ID " + channelId + " -> " + result);

            if (result.ToLower().Contains("verify your account") || result.ToLower().Contains("you are being blocked from accessing our"))
            {
                blackListedTokens.Add(token);
                addLiveLog("Token (" + token + ") has been blacklisted because it is unverified or blocked by Discord -> " + result);

                return;
            }
            /*else if (result.Contains("retry_after"))
            {
                dynamic jss = JObject.Parse(result);
                string retry_after = jss.retry_after;

                if (retry_after.Contains("."))
                {
                    retry_after = Microsoft.VisualBasic.Strings.Split(retry_after, ".")[0];
                }

                retry_after = retry_after.Trim().Replace(" ", "").Replace('\t'.ToString(), "");

                addLiveLog(proxy == null ? "Token (" + token + ") detected slow mode/rate limit in channel ID " + metroTextbox4.Text + ", sleeping for " + Convert.ToString(int.Parse(retry_after) + 1) + " seconds" : "Token (" + token + ") with proxy (" + proxy[0] + ":" + proxy[1] + ") detected slow mode/rate limit in channel ID " + channelId + ", sleeping for " + Convert.ToString(int.Parse(retry_after) + 1) + " seconds");

                needToDelay = true;
                delayMs = (int.Parse(retry_after) + 1) * 1000;
            }*/
            else if (metroChecker75.Checked)
            {
                dynamic jss = JObject.Parse(result);
                string id = (string)jss.id;
                string[] theProxy = proxy;

                new Thread(() => deleteMessage(token, channelId, id, theProxy)).Start();
            }
        }
        catch
        {

        }

        try
        {
            if (metroChecker4.Checked || metroChecker74.Checked)
            {
                msg = getServerSpamMessage();
            }
        }
        catch
        {

        }
    }

    public void deleteMessage(string token, string channelId, string messageId, string[] proxy)
    {
        try
        {
            Leaf.xNet.HttpRequest httpRequest = Utils.CreateCleanRequest();

            try
            {
                if (metroChecker13.Checked)
                {
                    if (proxy != null)
                    {
                        httpRequest.Proxy = new Leaf.xNet.HttpProxyClient(proxy[0], int.Parse(proxy[1]));
                    }
                    else
                    {
                        httpRequest.Proxy = null;
                    }
                }
            }
            catch
            {

            }

            httpRequest.AddHeader("Accept", "*/*");
            httpRequest.AddHeader("Accept-Encoding", "gzip, deflate, br");
            httpRequest.AddHeader("Accept-Language", getTokenLanguage(token));
            httpRequest.AddHeader("Authorization", token);
            httpRequest.AddHeader("Connection", "keep-alive");
            httpRequest.AddHeader("Cookie", Utils.GetRandomCookie(getTokenLanguage(token)));
            httpRequest.AddHeader("DNT", "1");
            httpRequest.AddHeader("Host", "discord.com");
            httpRequest.AddHeader("Origin", "https://discord.com");
            httpRequest.AddHeader("Referer", "https://discord.com/channels/@me");
            httpRequest.AddHeader("TE", "Trailers");
            httpRequest.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:88.0) Gecko/20100101 Firefox/88.0");
            httpRequest.AddHeader("X-Super-Properties", "eyJvcyI6IldpbmRvd3MiLCJicm93c2VyIjoiRmlyZWZveCIsImRldmljZSI6IiIsInN5c3RlbV9sb2NhbGUiOiJpdC1JVCIsImJyb3dzZXJfdXNlcl9hZ2VudCI6Ik1vemlsbGEvNS4wIChXaW5kb3dzIE5UIDEwLjA7IFdpbjY0OyB4NjQ7IHJ2Ojg4LjApIEdlY2tvLzIwMTAwMTAxIEZpcmVmb3gvODguMCIsImJyb3dzZXJfdmVyc2lvbiI6Ijg4LjAiLCJvc192ZXJzaW9uIjoiMTAiLCJyZWZlcnJlciI6IiIsInJlZmVycmluZ19kb21haW4iOiIiLCJyZWZlcnJlcl9jdXJyZW50IjoiIiwicmVmZXJyaW5nX2RvbWFpbl9jdXJyZW50IjoiIiwicmVsZWFzZV9jaGFubmVsIjoic3RhYmxlIiwiY2xpZW50X2J1aWxkX251bWJlciI6ODMzNjQsImNsaWVudF9ldmVudF9zb3VyY2UiOm51bGx9");

            string result = Utils.DecompressResponse(httpRequest.Delete("https://discord.com/api/v9/channels/" + channelId + "/messages/" + messageId));

            if (result.ToLower().Contains("verify your account") || result.ToLower().Contains("you are being blocked from accessing our"))
            {
                blackListedTokens.Add(token);
                addLiveLog("Token (" + token + ") has been blacklisted because it is unverified or blocked by Discord -> " + result);

                return;
            }

            if (result.Contains("retry_after"))
            {
                dynamic jss = JObject.Parse(result);
                string retry_after = jss.retry_after;

                if (retry_after.Contains("."))
                {
                    retry_after = Microsoft.VisualBasic.Strings.Split(retry_after, ".")[0];
                }

                retry_after = retry_after.Trim().Replace(" ", "").Replace('\t'.ToString(), "");

                Thread.Sleep((int.Parse(retry_after) + 1) * 1000);
                deleteMessage(token, channelId, messageId, proxy);
            }
        }
        catch
        {
            
        }
    }

    private void metroButton12_Click(object sender, EventArgs e)
    {
        try
        {
            addLiveLog("Succesfully started Typing Spammer in channel ID " + metroTextbox10.Text);
            metroButton12.Enabled = false;

            typingSpammer = true;
            new Thread(doTypingSpammer).Start();

            metroButton11.Enabled = true;
        }
        catch
        {

        }
    }

    private void metroButton11_Click(object sender, EventArgs e)
    {
        try
        {
            addLiveLog("Succesfully stopped Typing Spammer in channel ID " + metroTextbox10.Text);
            metroButton11.Enabled = false;

            typingSpammer = false;

            metroButton12.Enabled = true;
        }
        catch
        {

        }
    }

    public void doTypingSpammer()
    {
        try
        {
            if (metroChecker77.Checked)
            {
                if (!AreIDsValid(metroTextbox10.Text))
                {
                    metroButton11.Enabled = false;
                    metroButton12.Enabled = true;

                    MessageBox.Show("The ids of the channels are not valid!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }
            }
            else
            {
                if (!IsIDValid(metroTextbox10.Text))
                {
                    metroButton11.Enabled = false;
                    metroButton12.Enabled = true;

                    MessageBox.Show("The id of channel is not valid!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }
            }

            int i = 0, j = 0;

            if (metroChecker39.Checked)
            {
                try
                {
                    i = int.Parse(metroTextbox29.Text.Replace(" ", "").Replace('\t'.ToString(), ""));

                    if (i < 0)
                    {
                        i = 0;
                    }
                }
                catch
                {

                }
            }

            foreach (string token in SplitToLines(metroTextbox1.Text))
            {
                try
                {
                    if (blackListedTokens.Contains(token))
                    {
                        continue;
                    }

                    if (metroChecker39.Checked)
                    {
                        if (j == i)
                        {
                            break;
                        }

                        j++;
                    }

                    if (metroChecker77.Checked)
                    {
                        foreach (string id in GetIDs(metroTextbox10.Text))
                        {
                            new Thread(() => typingSpam(token, id)).Start();
                        }
                    }
                    else
                    {
                        new Thread(() => typingSpam(token, metroTextbox10.Text)).Start();
                    }
                }
                catch
                {

                }
            }
        }
        catch
        {

        }
    }

    public void typingSpam(string token, string channelId)
    {
        while (true)
        {
            try
            {
                if (typingSpammer)
                {
                    if (blackListedTokens.Contains(token))
                    {
                        return;
                    }

                    try
                    {
                        Leaf.xNet.HttpRequest httpRequest = Utils.CreateCleanRequest();
                        string[] proxy = null;

                        try
                        {
                            if (metroChecker13.Checked)
                            {
                                proxy = Microsoft.VisualBasic.Strings.Split(GetRandomProxy(), ":");
                                httpRequest.Proxy = new Leaf.xNet.HttpProxyClient(proxy[0], int.Parse(proxy[1]));
                            }
                            else
                            {
                                httpRequest.Proxy = null;
                            }
                        }
                        catch
                        {

                        }

                        httpRequest.AddHeader("Accept", "*/*");
                        httpRequest.AddHeader("Accept-Encoding", "gzip, deflate, br");
                        httpRequest.AddHeader("Accept-Language", getTokenLanguage(token));
                        httpRequest.AddHeader("Authorization", token);
                        httpRequest.AddHeader("Connection", "keep-alive");
                        httpRequest.AddHeader("Content-Length", "0");
                        httpRequest.AddHeader("Cookie", Utils.GetRandomCookie(getTokenLanguage(token)));
                        httpRequest.AddHeader("DNT", "1");
                        httpRequest.AddHeader("Host", "discord.com");
                        httpRequest.AddHeader("Origin", "https://discord.com");
                        httpRequest.AddHeader("TE", "Trailers");
                        httpRequest.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:88.0) Gecko/20100101 Firefox/88.0");
                        httpRequest.AddHeader("X-Super-Properties", "eyJvcyI6IldpbmRvd3MiLCJicm93c2VyIjoiRmlyZWZveCIsImRldmljZSI6IiIsImJyb3dzZXJfdXNlcl9hZ2VudCI6Ik1vemlsbGEvNS4wIChXaW5kb3dzIE5UIDEwLjA7IFdpbjY0OyB4NjQ7IHJ2Ojg3LjApIEdlY2tvLzIwMTAwMTAxIEZpcmVmb3gvODcuMCIsImJyb3dzZXJfdmVyc2lvbiI6Ijg3LjAiLCJvc192ZXJzaW9uIjoiMTAiLCJyZWZlcnJlciI6IiIsInJlZmVycmluZ19kb21haW4iOiIiLCJyZWZlcnJlcl9jdXJyZW50IjoiIiwicmVmZXJyaW5nX2RvbWFpbl9jdXJyZW50IjoiIiwicmVsZWFzZV9jaGFubmVsIjoic3RhYmxlIiwiY2xpZW50X2J1aWxkX251bWJlciI6ODE0NTIsImNsaWVudF9ldmVudF9zb3VyY2UiOm51bGx9");

                        httpRequest.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                        string result = Utils.DecompressResponse(httpRequest.Post("https://discord.com/api/v9/channels/" + channelId + "/typing"));

                        addLiveLog(proxy == null ? "Token (" + token + ") is now typing in channel ID " + channelId + " -> " + result : "Token (" + token + ") with proxy (" + proxy[0] + ":" + proxy[1] + ") is now typing channel ID " + channelId + " -> " + result);

                        if (result.ToLower().Contains("verify your account") || result.ToLower().Contains("you are being blocked from accessing our"))
                        {
                            blackListedTokens.Add(token);
                            addLiveLog("Token (" + token + ") has been blacklisted because it is unverified or blocked by Discord -> " + result);

                            return;
                        }

                        Thread.Sleep(8000);
                    }
                    catch
                    {

                    }
                }
                else
                {
                    return;
                }
            }
            catch
            {

            }
        }
    }

    private void metroButton14_Click(object sender, EventArgs e)
    {
        try
        {
            if (!CheckWebhook(metroTextbox11.Text))
            {
                MessageBox.Show("The webhook is not valid!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }

            addLiveLog("Succesfully started Webhook Spammer for the webhook '" + metroTextbox11.Text + "'.");

            metroButton14.Enabled = false;

            webhookSpammer = true;
            new Thread(doWebhookSpammer).Start();

            metroButton13.Enabled = true;
        }
        catch
        {

        }
    }

    public static bool CheckWebhook(string WebhookUrl, string ProxyIp = "", string ProxyPort = "", string UserAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_6) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.105 Safari/537.36")
    {
        try
        {
            if (!WebhookUrl.StartsWith("https://discord.com/api/webhooks/"))
            {
                return false;
            }

            if (WebhookUrl.Length != 120)
            {
                return false;
            }

            var Req = (HttpWebRequest)WebRequest.Create(WebhookUrl);

            if (ProxyIp != "")
            {
                if (ProxyPort != "")
                {
                    Req.Proxy = new WebProxy(ProxyIp, Convert.ToInt32(ProxyPort));
                }
            }

            Req.Method = "GET";
            Req.UserAgent = UserAgent;

            return new StreamReader(((HttpWebResponse)Req.GetResponse()).GetResponseStream()).ReadToEnd().Contains("id");
        }
        catch
        {
            return false;
        }
    }

    private void metroButton13_Click(object sender, EventArgs e)
    {
        try
        {
            addLiveLog("Succesfully stopped Webhook Spammer for the webhook '" + metroTextbox11.Text + "'.");
            metroButton13.Enabled = false;

            webhookSpammer = false;

            metroButton14.Enabled = true;
        }
        catch
        {

        }
    }

    public void doWebhookSpammer()
    {
        string msg = metroTextbox14.Text;
        bool added = false;

        if (metroChecker11.Checked)
        {
            msg = "```\n" + msg;
        }

        if (metroChecker7.Checked)
        {
            msg = ">>> " + msg;
        }

        if (metroChecker11.Checked)
        {
            msg = msg + "\n```";
        }

        while (true)
        {
            try
            {
                if (webhookSpammer)
                {
                    if (metroChecker12.Checked)
                    {
                        Thread.Sleep(metroTrackbar6.Value);
                    }

                    if (metroChecker20.Checked)
                    {
                        msg = metroTextbox14.Text;

                        if (metroChecker11.Checked)
                        {
                            msg = "```\n" + msg;
                        }

                        if (metroChecker7.Checked)
                        {
                            msg = ">>> " + msg;
                        }

                        if (metroChecker11.Checked)
                        {
                            msg = msg + "\n```";
                        }

                        if (metroChecker15.Checked)
                        {
                            msg += " " + Utils.GetRandomNumber(1000, 9999);
                        }
                    }
                    else
                    {
                        if (metroChecker15.Checked)
                        {
                            if (added)
                            {
                                msg = msg.Substring(0, msg.Length - 5);
                            }

                            msg += " " + Utils.GetRandomNumber(1000, 9999);
                            added = true;
                        }
                        else if (added)
                        {
                            msg = msg.Substring(0, msg.Length - 5);
                            added = false;
                        }
                    }

                    if (metroChecker43.Checked)
                    {
                        msg = Utils.GetLagMessage();
                    }

                    try
                    {
                        Post(metroTextbox11.Text, new NameValueCollection()
            {
                { "username", metroTextbox12.Text },
                { "avatar_url", metroTextbox13.Text },
                { "content", msg }
            });
                    }
                    catch
                    {

                    }
                }
                else
                {
                    return;
                }
            }
            catch
            {

            }
        }
    }

    private void metroButton16_Click(object sender, EventArgs e)
    {
        try
        {
            addLiveLog("Succesfully started DM Spammer for the user ID " + metroTextbox15.Text + ".");
            metroButton16.Enabled = false;

            dmSpammer = true;
            new Thread(doDmSpammer).Start();

            if (metroChecker58.Checked)
            {
                new Thread(doDmSpammer).Start();
            }

            metroButton15.Enabled = true;
        }
        catch
        {

        }
    }

    private void metroButton15_Click(object sender, EventArgs e)
    {
        try
        {
            addLiveLog("Succesfully stopped DM Spammer for the user ID " + metroTextbox15.Text + ".");
            metroButton15.Enabled = false;

            dmSpammer = false;

            metroButton16.Enabled = true;
        }
        catch
        {

        }
    }

    public void doDmSpammer()
    {
        try
        {
            if (metroChecker83.Checked)
            {
                if (!AreIDsValid(metroTextbox15.Text))
                {
                    metroButton15.Enabled = false;
                    metroButton16.Enabled = true;

                    MessageBox.Show("The ids of the users are not valid!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }
            }
            else
            {
                if (!IsIDValid(metroTextbox15.Text))
                {
                    metroButton15.Enabled = false;
                    metroButton16.Enabled = true;

                    MessageBox.Show("The id of the user is not valid!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }
            }

            int i = 0, j = 0;

            if (metroChecker36.Checked)
            {
                try
                {
                    i = int.Parse(metroTextbox26.Text.Replace(" ", "").Replace('\t'.ToString(), ""));

                    if (i < 0)
                    {
                        i = 0;
                    }
                }
                catch
                {

                }
            }

            foreach (string token in SplitToLines(metroTextbox1.Text))
            {
                try
                {
                    if (blackListedTokens.Contains(token))
                    {
                        continue;
                    }

                    if (metroChecker36.Checked)
                    {
                        if (j == i)
                        {
                            break;
                        }

                        j++;
                    }

                    if (metroChecker16.Checked)
                    {
                        Thread.Sleep(metroTrackbar7.Value);
                    }

                    if (metroChecker64.Checked)
                    {
                        if (metroChecker83.Checked)
                        {
                            for (int z = 0; z <= 10; z++)
                            {
                                foreach (string id in GetIDs(metroTextbox15.Text))
                                {
                                    new Thread(() => dmSpam(token, metroTextbox15.Text)).Start();
                                }
                            }
                        }
                        else
                        {
                            for (int z = 0; z <= 10; z++)
                            {
                                new Thread(() => dmSpam(token, metroTextbox15.Text)).Start();
                            }
                        }
                    }
                    else
                    {
                        if (metroChecker83.Checked)
                        {
                            foreach (string id in GetIDs(metroTextbox15.Text))
                            {
                                new Thread(() => dmSpam(token, id)).Start();
                            }
                        }
                        else
                        {
                            new Thread(() => dmSpam(token, metroTextbox15.Text)).Start();
                        }
                    }
                }
                catch
                {

                }
            }
        }
        catch
        {

        }
    }

    public string getDMSpamMessage()
    {
        string msg = "";

        try
        {
            if (metroChecker14.Checked)
            {
                msg = ">>> ";
            }

            if (!metroChecker84.Checked)
            {
                List<string> lines = new List<string>();

                foreach (string line in SplitToLines(metroTextbox16.Text))
                {
                    lines.Add(line);
                }

                if (lines.Count != 1)
                {
                    foreach (string line in lines)
                    {
                        msg = msg + " \\u000d" + line;
                    }
                }
                else
                {
                    msg += metroTextbox16.Text;
                }
            }
            else
            {
                if (multipleMessageDMIndex < 0)
                {
                    multipleMessageDMIndex = 0;
                }

                int count = 0;

                foreach (char c in metroTextbox16.Text.ToCharArray())
                {
                    if (c == '|')
                    {
                        count++;
                    }
                }

                if (multipleMessageDMIndex > count)
                {
                    multipleMessageDMIndex = 0;
                }

                if (count == 0)
                {
                    List<string> lines = new List<string>();

                    foreach (string line in SplitToLines(metroTextbox16.Text))
                    {
                        lines.Add(line);
                    }

                    if (lines.Count != 1)
                    {
                        foreach (string line in lines)
                        {
                            msg = msg + " \\u000d" + line;
                        }
                    }
                    else
                    {
                        msg = metroTextbox16.Text;
                    }

                    multipleMessageDMIndex++;
                }
                else if (count == 1 && Microsoft.VisualBasic.Strings.Split(metroTextbox16.Text, "|")[1].Replace(" ", "").Replace('\t'.ToString(), "").Trim() == "")
                {
                    string[] splitted = Microsoft.VisualBasic.Strings.Split(metroTextbox16.Text, "|");
                    string definitive = splitted[0];
                    List<string> lines = new List<string>();

                    foreach (string line in SplitToLines(definitive))
                    {
                        lines.Add(line);
                    }

                    if (lines.Count != 1)
                    {
                        foreach (string line in lines)
                        {
                            msg = msg + " \\u000d" + line;
                        }
                    }
                    else
                    {
                        msg = definitive;
                    }

                    multipleMessageDMIndex++;
                }
                else
                {
                    string[] splitted = Microsoft.VisualBasic.Strings.Split(metroTextbox16.Text, "|");
                    string definitive = splitted[multipleMessageDMIndex];
                    List<string> lines = new List<string>();

                    foreach (string line in SplitToLines(definitive))
                    {
                        lines.Add(line);
                    }

                    if (lines.Count != 1)
                    {
                        foreach (string line in lines)
                        {
                            msg = msg + " \\u000d" + line;
                        }
                    }
                    else
                    {
                        msg = definitive;
                    }

                    if (multipleMessageDMIndex == count)
                    {
                        multipleMessageDMIndex = 0;
                    }
                    else
                    {
                        multipleMessageDMIndex++;
                    }
                }
            }
        }
        catch
        {

        }

        return msg;
    }

    public string getServerSpamMessage()
    {
        string msg = "";

        try
        {
            if (metroChecker5.Checked)
            {
                msg = ">>> ";
            }

            if (!metroChecker74.Checked)
            {
                List<string> lines = new List<string>();

                foreach (string line in SplitToLines(metroTextbox5.Text))
                {
                    lines.Add(line);
                }

                if (lines.Count != 1)
                {
                    foreach (string line in lines)
                    {
                        if (metroChecker53.Checked)
                        {
                            msg = msg + " \\u000d" + Utils.BypassAntiSpamFilter(line);
                        }
                        else
                        {
                            msg = msg + " \\u000d" + line;
                        }
                    }
                }
                else
                {
                    if (metroChecker53.Checked)
                    {
                        msg += Utils.BypassAntiSpamFilter(metroTextbox5.Text);
                    }
                    else
                    {
                        msg = metroTextbox5.Text;
                    }
                }
            }
            else
            {
                if (multipleMessageIndex < 0)
                {
                    multipleMessageIndex = 0;
                }

                int count = 0;

                foreach (char c in metroTextbox5.Text.ToCharArray())
                {
                    if (c == '|')
                    {
                        count++;
                    }
                }

                if (multipleMessageIndex > count)
                {
                    multipleMessageIndex = 0;
                }

                if (count == 0)
                {
                    List<string> lines = new List<string>();

                    foreach (string line in SplitToLines(metroTextbox5.Text))
                    {
                        lines.Add(line);
                    }

                    if (lines.Count != 1)
                    {
                        foreach (string line in lines)
                        {
                            if (metroChecker53.Checked)
                            {
                                msg = msg + " \\u000d" + Utils.BypassAntiSpamFilter(line);
                            }
                            else
                            {
                                msg = msg + " \\u000d" + line;
                            }
                        }
                    }
                    else
                    {
                        if (metroChecker53.Checked)
                        {
                            msg += Utils.BypassAntiSpamFilter(metroTextbox5.Text);
                        }
                        else
                        {
                            msg = metroTextbox5.Text;
                        }
                    }

                    multipleMessageIndex++;
                }
                else if (count == 1 && Microsoft.VisualBasic.Strings.Split(metroTextbox5.Text, "|")[1].Replace(" ", "").Replace('\t'.ToString(), "").Trim() == "")
                {
                    string[] splitted = Microsoft.VisualBasic.Strings.Split(metroTextbox5.Text, "|");
                    string definitive = splitted[0];
                    List<string> lines = new List<string>();

                    foreach (string line in SplitToLines(definitive))
                    {
                        lines.Add(line);
                    }

                    if (lines.Count != 1)
                    {
                        foreach (string line in lines)
                        {
                            if (metroChecker53.Checked)
                            {
                                msg = msg + " \\u000d" + Utils.BypassAntiSpamFilter(line);
                            }
                            else
                            {
                                msg = msg + " \\u000d" + line;
                            }
                        }
                    }
                    else
                    {
                        if (metroChecker53.Checked)
                        {
                            msg += Utils.BypassAntiSpamFilter(definitive);
                        }
                        else
                        {
                            msg = definitive;
                        }
                    }

                    multipleMessageIndex++;
                }
                else
                {
                    string[] splitted = Microsoft.VisualBasic.Strings.Split(metroTextbox5.Text, "|");
                    string definitive = splitted[multipleMessageIndex];
                    List<string> lines = new List<string>();

                    foreach (string line in SplitToLines(definitive))
                    {
                        lines.Add(line);
                    }

                    if (lines.Count != 1)
                    {
                        foreach (string line in lines)
                        {
                            if (metroChecker53.Checked)
                            {
                                msg = msg + " \\u000d" + Utils.BypassAntiSpamFilter(line);
                            }
                            else
                            {
                                msg = msg + " \\u000d" + line;
                            }
                        }
                    }
                    else
                    {
                        if (metroChecker53.Checked)
                        {
                            msg += Utils.BypassAntiSpamFilter(definitive);
                        }
                        else
                        {
                            msg = definitive;
                        }
                    }

                    if (multipleMessageIndex == count)
                    {
                        multipleMessageIndex = 0;
                    }
                    else
                    {
                        multipleMessageIndex++;
                    }
                }
            }
        }
        catch
        {

        }

        return msg;
    }

    public void dmSpam(string token, string id)
    {
        try
        {
            string msg = getDMSpamMessage();
            bool added = false;
            /*bool needToDelay = false;
            int delayMs = 0;*/

            string[] proxy = null;

            try
            {
                if (metroChecker13.Checked)
                {
                    proxy = Microsoft.VisualBasic.Strings.Split(GetRandomProxy(), ":");
                }
            }
            catch
            {

            }

            string recipient = "";

            try
            {
                recipient = createDM(token, id, 5000, proxy).Result;
            }
            catch
            {

            }

            if (recipient != null || recipient != "")
            {
                while (true)
                {
                    if (blackListedTokens.Contains(token))
                    {
                        continue;
                    }

                    try
                    {
                        if (dmSpammer)
                        {
                            /*if (needToDelay)
                            {
                                Thread.Sleep(delayMs);

                                needToDelay = false;
                                delayMs = 0;
                            }

                            if (metroChecker16.Checked)
                            {
                                Thread.Sleep(metroTrackbar7.Value);
                            }

                            if (needToDelay)
                            {
                                Thread.Sleep(delayMs);

                                needToDelay = false;
                                delayMs = 0;
                            }*/

                            if (metroChecker59.Checked)
                            {
                                new Thread(() => processDmSpamMessage(token, ref msg, recipient, proxy/*, ref needToDelay, ref delayMs*/)).Start();
                            }
                            else
                            {
                                processDmSpamMessage(token, ref msg, recipient, proxy/*, ref needToDelay, ref delayMs*/);
                            }

                            /*if (needToDelay)
                            {
                                Thread.Sleep(delayMs);

                                needToDelay = false;
                                delayMs = 0;
                            }*/
                        }
                        else
                        {
                            return;
                        }
                    }
                    catch
                    {

                    }
                }
            }
        }
        catch
        {

        }
    }

    public void processDmSpamMessage(string token, ref string msg, string recipient, string[] proxy/*, ref bool needToDelay, ref int delayMs*/)
    {
        /*if (needToDelay || delayMs > 0)
        {
            return;
        }*/

        try
        {
            Leaf.xNet.HttpRequest httpRequest = Utils.CreateCleanRequest();

            try
            {
                if (metroChecker13.Checked)
                {
                    if (proxy != null)
                    {
                        httpRequest.Proxy = new Leaf.xNet.HttpProxyClient(proxy[0], int.Parse(proxy[1]));
                    }
                    else
                    {
                        httpRequest.Proxy = null;
                    }
                }
            }
            catch
            {

            }

            string newMsg = msg;

            if (metroChecker18.Checked)
            {
                while (newMsg.Contains("[rndnum]"))
                {
                    newMsg = ReplaceFirst(newMsg, "[rndnum]", Utils.GetRandomNumber(1000, 9999).ToString());
                }

                while (newMsg.Contains("[rndstr]"))
                {
                    newMsg = ReplaceFirst(newMsg, "[rndstr]", Utils.RandomNormalString(16));
                }
            }

            if (metroChecker85.Checked)
            {
                while (newMsg.Contains("[crashgif]"))
                {
                    newMsg = ReplaceFirst(newMsg, "[crashgif]", " https://gfycat.com/dazzlingsarcasticamurminnow ");
                }
            }

            if (metroChecker41.Checked)
            {
                while (newMsg.Contains("[lag]"))
                {
                    newMsg = ReplaceFirst(newMsg, "[lag]", Utils.GetLagMessage());
                }
            }

            string messageJson = "{\"content\":\"" + newMsg + "\",\"nonce\":" + Utils.RandomNonce().ToString() + ",\"tts\":false}";

            if (blackListedTokens.Contains(token))
            {
                return;
            }

            httpRequest.AddHeader("Accept", "*/*");
            httpRequest.AddHeader("Accept-Encoding", "gzip, deflate, br");
            httpRequest.AddHeader("Accept-Language", getTokenLanguage(token));
            httpRequest.AddHeader("Authorization", token);
            httpRequest.AddHeader("Connection", "keep-alive");
            httpRequest.AddHeader("Content-Length", messageJson.Length.ToString());
            httpRequest.AddHeader("Content-Type", "application/json");
            httpRequest.AddHeader("Cookie", Utils.GetRandomCookie(getTokenLanguage(token)));
            httpRequest.AddHeader("DNT", "1");
            httpRequest.AddHeader("Host", "discord.com");
            httpRequest.AddHeader("Origin", "https://discord.com");
            httpRequest.AddHeader("Referer", "https://discord.com/channels/@me/" + recipient);
            httpRequest.AddHeader("TE", "Trailers");
            httpRequest.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:88.0) Gecko/20100101 Firefox/88.0");
            httpRequest.AddHeader("X-Super-Properties", "eyJvcyI6IldpbmRvd3MiLCJicm93c2VyIjoiRmlyZWZveCIsImRldmljZSI6IiIsImJyb3dzZXJfdXNlcl9hZ2VudCI6Ik1vemlsbGEvNS4wIChXaW5kb3dzIE5UIDEwLjA7IFdpbjY0OyB4NjQ7IHJ2Ojg3LjApIEdlY2tvLzIwMTAwMTAxIEZpcmVmb3gvODcuMCIsImJyb3dzZXJfdmVyc2lvbiI6Ijg3LjAiLCJvc192ZXJzaW9uIjoiMTAiLCJyZWZlcnJlciI6IiIsInJlZmVycmluZ19kb21haW4iOiIiLCJyZWZlcnJlcl9jdXJyZW50IjoiIiwicmVmZXJyaW5nX2RvbWFpbl9jdXJyZW50IjoiIiwicmVsZWFzZV9jaGFubmVsIjoic3RhYmxlIiwiY2xpZW50X2J1aWxkX251bWJlciI6ODE0NTIsImNsaWVudF9ldmVudF9zb3VyY2UiOm51bGx9");

            string result = Utils.DecompressResponse(httpRequest.Post("https://discord.com/api/v9/channels/" + recipient + "/messages", messageJson, "application/json"));
            addLiveLog(proxy == null ? "Token (" + token + ") sent a message to the user ID " + metroTextbox15.Text + " with channel ID " + recipient + " -> " + result : "Token (" + token + ") with proxy (" + proxy[0] + ":" + proxy[1] + ") sent a message to the user ID " + metroTextbox15.Text + " with channel ID " + recipient + " -> " + result);

            if (result.ToLower().Contains("verify your account") || result.ToLower().Contains("you are being blocked from accessing our"))
            {
                blackListedTokens.Add(token);
                addLiveLog("Token (" + token + ") has been blacklisted because it is unverified or blocked by Discord -> " + result);

                return;
            }

            /*if (result.Contains("retry_after"))
            {
                dynamic jss = JObject.Parse(result);
                string retry_after = jss.retry_after;

                if (retry_after.Contains("."))
                {
                    retry_after = Microsoft.VisualBasic.Strings.Split(retry_after, ".")[0];
                }

                retry_after = retry_after.Trim().Replace(" ", "").Replace('\t'.ToString(), "");

                addLiveLog(proxy == null ? "Token (" + token + ") detected slow mode/rate limit in sending message to user ID " + metroTextbox15.Text + ", in channel ID " + recipient + ", sleeping for " + Convert.ToString(int.Parse(retry_after) + 1) + " seconds" : "Token (" + token + ") with proxy (" + proxy[0] + ":" + proxy[1] + ") detected slow mode/rate limit in sending message to user ID " + metroTextbox15.Text + ", in channel ID " + recipient + ", sleeping for " + Convert.ToString(int.Parse(retry_after) + 1) + " seconds");

                needToDelay = true;
                delayMs = (int.Parse(retry_after) + 1) * 1000;
            }*/
        }
        catch
        {

        }

        if (metroChecker17.Checked || metroChecker84.Checked)
        {
            msg = getDMSpamMessage();
        }
    }

    public async Task<String> createDM(string token, string targetUserId, int timeout = 5000, string[] proxy = null)
    {
        try
        {
            Leaf.xNet.HttpRequest httpRequest = Utils.CreateCleanRequest();

            try
            {
                if (metroChecker13.Checked)
                {
                    if (proxy != null)
                    {
                        httpRequest.Proxy = new Leaf.xNet.HttpProxyClient(proxy[0], int.Parse(proxy[1]));
                    }
                    else
                    {
                        httpRequest.Proxy = null;
                    }
                }
            }
            catch
            {

            }

            string content = "{\"recipients\":[\"" + targetUserId + "\"]}";

            httpRequest.AddHeader("Accept", "*/*");
            httpRequest.AddHeader("Accept-Encoding", "gzip, deflate, br");
            httpRequest.AddHeader("Accept-Language", getTokenLanguage(token));
            httpRequest.AddHeader("Authorization", token);
            httpRequest.AddHeader("Connection", "keep-alive");
            httpRequest.AddHeader("Content-Length", content.Length.ToString());
            httpRequest.AddHeader("Content-Type", "application/json");
            httpRequest.AddHeader("Cookie", Utils.GetRandomCookie(getTokenLanguage(token)));
            httpRequest.AddHeader("DNT", "1");
            httpRequest.AddHeader("Host", "discord.com");
            httpRequest.AddHeader("Origin", "https://discord.com");
            httpRequest.AddHeader("Referer", "https://discord.com/channels/@me");
            httpRequest.AddHeader("TE", "Trailers");
            httpRequest.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:88.0) Gecko/20100101 Firefox/88.0");
            httpRequest.AddHeader("X-Context-Properties", "e30=");
            httpRequest.AddHeader("X-Super-Properties", "eyJvcyI6IldpbmRvd3MiLCJicm93c2VyIjoiRmlyZWZveCIsImRldmljZSI6IiIsInN5c3RlbV9sb2NhbGUiOiJpdC1JVCIsImJyb3dzZXJfdXNlcl9hZ2VudCI6Ik1vemlsbGEvNS4wIChXaW5kb3dzIE5UIDEwLjA7IFdpbjY0OyB4NjQ7IHJ2Ojg4LjApIEdlY2tvLzIwMTAwMTAxIEZpcmVmb3gvODguMCIsImJyb3dzZXJfdmVyc2lvbiI6Ijg4LjAiLCJvc192ZXJzaW9uIjoiMTAiLCJyZWZlcnJlciI6IiIsInJlZmVycmluZ19kb21haW4iOiIiLCJyZWZlcnJlcl9jdXJyZW50IjoiIiwicmVmZXJyaW5nX2RvbWFpbl9jdXJyZW50IjoiIiwicmVsZWFzZV9jaGFubmVsIjoic3RhYmxlIiwiY2xpZW50X2J1aWxkX251bWJlciI6ODMzNjQsImNsaWVudF9ldmVudF9zb3VyY2UiOm51bGx9");

            httpRequest.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;

            string responseStr = Utils.DecompressResponse(httpRequest.Post("https://discord.com/api/v9/users/@me/channels", content, "application/json"));

            dynamic jss = JObject.Parse(responseStr);
            var channelId = jss.id;
            return channelId;
        }
        catch
        {
            return "";
        }
    }

    private void metroTrackbar7_Scroll(object sender, MetroSuite.MetroTrackbar.TrackbarEventArgs e)
    {
        metroChecker16.Text = "Delay: " + metroTrackbar7.Value + "ms";
    }

    private void metroButton3_Click(object sender, EventArgs e)
    {
        try
        {
            if (metroChecker76.Checked)
            {
                metroButton3.Enabled = false;
                metroButton4.Enabled = true;
            }

            if (metroChecker72.Checked)
            {
                new Thread(startGroupJoiner).Start();
            }
            else
            {
                new Thread(startGuildJoiner).Start();
            }
        }
        catch
        {

        }
    }

    public void startGroupJoiner()
    {
        try
        {
            Tuple<bool, string, string> info = GetGroupInviteInformations(metroTextbox3.Text);

            if (info.Item1)
            {
                theChannelType = "";
                theXCP64 = "";
                theChannelId = "";
                theChannelId = info.Item2;
                theChannelType = info.Item3;
                theXCP64 = Utils.GetGroupXCP(theChannelId, theChannelType);

                if (metroChecker30.Checked)
                {
                    metroTextbox4.Text = theChannelId;
                }

                addLiveLog("Succesfully started Group Joiner for group invite link/code '" + metroTextbox3.Text + "'. Converting to group invite code...");
                inviteCode = GetInviteCodeByInviteLink(metroTextbox3.Text);
                addLiveLog("Succesfully converted the group invite link to a group invite code: '" + inviteCode + "'. Got the Group ID: " + theGuildId);

                new Thread(doGroupJoiner).Start();
            }
            else
            {
                MessageBox.Show("The invite link / code is not valid! Check if your tokens are all valid.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch
        {

        }
    }

    public void doGroupJoiner()
    {
        int i = 0, j = 0;

        if (metroChecker35.Checked)
        {
            try
            {
                i = int.Parse(metroTextbox25.Text.Replace(" ", "").Replace('\t'.ToString(), ""));

                if (i < 0)
                {
                    i = 0;
                }
            }
            catch
            {

            }
        }

        foreach (string token in SplitToLines(metroTextbox1.Text))
        {
            if (blackListedTokens.Contains(token))
            {
                continue;
            }

            if (metroChecker35.Checked)
            {
                if (j == i)
                {
                    break;
                }

                j++;
            }

            if (metroChecker3.Checked)
            {
                Thread.Sleep(metroTrackbar1.Value);
            }

            new Thread(() => joinGroup(token)).Start();
        }
    }

    public void joinGroup(string token)
    {
        try
        {
            if (blackListedTokens.Contains(token))
            {
                return;
            }

            Leaf.xNet.HttpRequest httpRequest = Utils.CreateCleanRequest();
            string[] proxy = null;

            try
            {
                if (metroChecker13.Checked)
                {
                    proxy = Microsoft.VisualBasic.Strings.Split(GetRandomProxy(), ":");
                    httpRequest.Proxy = new Leaf.xNet.HttpProxyClient(proxy[0], int.Parse(proxy[1]));
                }
                else
                {
                    httpRequest.Proxy = null;
                }
            }
            catch
            {

            }

            httpRequest.AddHeader("Accept", "*/*");
            httpRequest.AddHeader("Accept-Encoding", "gzip, deflate, br");
            httpRequest.AddHeader("Accept-Language", getTokenLanguage(token));
            httpRequest.AddHeader("Authorization", token);
            httpRequest.AddHeader("Connection", "keep-alive");
            httpRequest.AddHeader("Content-Length", "0");
            httpRequest.AddHeader("Cookie", Utils.GetRandomCookie(getTokenLanguage(token)));
            httpRequest.AddHeader("DNT", "1");
            httpRequest.AddHeader("Host", "discord.com");
            httpRequest.AddHeader("Origin", "https://discord.com");
            httpRequest.AddHeader("TE", "Trailers");
            httpRequest.AddHeader("Referer", "https://discord.com/channels/@me");
            httpRequest.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:88.0) Gecko/20100101 Firefox/88.0");
            httpRequest.AddHeader("X-Context-Properties", theXCP64);
            httpRequest.AddHeader("X-Super-Properties", "eyJvcyI6IldpbmRvd3MiLCJicm93c2VyIjoiRmlyZWZveCIsImRldmljZSI6IiIsInN5c3RlbV9sb2NhbGUiOiJpdC1JVCIsImJyb3dzZXJfdXNlcl9hZ2VudCI6Ik1vemlsbGEvNS4wIChXaW5kb3dzIE5UIDEwLjA7IFdpbjY0OyB4NjQ7IHJ2Ojg4LjApIEdlY2tvLzIwMTAwMTAxIEZpcmVmb3gvODguMCIsImJyb3dzZXJfdmVyc2lvbiI6Ijg4LjAiLCJvc192ZXJzaW9uIjoiMTAiLCJyZWZlcnJlciI6IiIsInJlZmVycmluZ19kb21haW4iOiIiLCJyZWZlcnJlcl9jdXJyZW50IjoiIiwicmVmZXJyaW5nX2RvbWFpbl9jdXJyZW50IjoiIiwicmVsZWFzZV9jaGFubmVsIjoic3RhYmxlIiwiY2xpZW50X2J1aWxkX251bWJlciI6ODMwNDAsImNsaWVudF9ldmVudF9zb3VyY2UiOm51bGx9");
            httpRequest.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;

            string response = Utils.DecompressResponse(httpRequest.Post("https://discord.com/api/v9/invites/" + GetInviteCodeByInviteLink(inviteCode)));

            addLiveLog(proxy == null ? "Token (" + token + ") tried to join the group with invite code '" + inviteCode + "' and ID '" + theChannelId + "' -> " + response : "Token (" + token + ") with proxy (" + proxy[0] + ":" + proxy[1] + ") tried to join the group with invite code '" + inviteCode + "' and ID '" + theChannelId + "' -> " + response);

            if (response.ToLower().Contains("verify your account") || response.ToLower().Contains("you are being blocked from accessing our"))
            {
                blackListedTokens.Add(token);
                addLiveLog("Token (" + token + ") has been blacklisted because it is unverified or blocked by Discord -> " + response);

                return;
            }

            if (InJoinRaidMode())
            {
                if (metroChecker3.Checked)
                {
                    Thread.Sleep(metroTrackbar1.Value);
                }

                leaveGroup(token);
            }
        }
        catch
        {

        }
    }

    public void startGuildJoiner()
    {
        try
        {
            if (metroChecker48.Checked || metroChecker65.Checked)
            {
                if (!IsIDValid(metroTextbox39.Text))
                {
                    MessageBox.Show("The ID of the Captcha Bot is not valid!", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            if (metroChecker65.Checked)
            {
                if (!IsIDValid(metroTextbox40.Text))
                {
                    MessageBox.Show("The ID of the channel where to bypass the Captcha Bot is not valid!", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            Tuple<bool, string, string, string> info = GetInviteInformations(metroTextbox3.Text);

            if (info.Item1)
            {
                theGuildId = "";
                theChannelType = "";
                theXCP64 = "";
                theChannelId = "";
                theGuildId = info.Item2;
                theChannelId = info.Item3;
                theChannelType = info.Item4;
                theXCP64 = Utils.GetXCP(theGuildId, theChannelId, theChannelType);

                if (theGuildId != "404")
                {
                    if (metroChecker30.Checked)
                    {
                        metroTextbox4.Text = theChannelId;
                    }

                    if (metroChecker49.Checked)
                    {
                        metroTextbox17.Text = theGuildId;
                    }

                    addLiveLog("Succesfully started Guild Joiner for guild invite link/code '" + metroTextbox3.Text + "'. Converting to guild invite code...");
                    inviteCode = GetInviteCodeByInviteLink(metroTextbox3.Text);
                    addLiveLog("Succesfully converted the guild invite link to a guild invite code: '" + inviteCode + "'. Got the Guild ID: " + theGuildId);

                    new Thread(doGuildJoiner).Start();
                }
            }
            else
            {
                MessageBox.Show("The invite link / code is not valid! Check if your tokens are all valid.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch
        {
            MessageBox.Show("The invite link / code is not valid! Check if your tokens are all valid.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public void doGuildJoiner()
    {
        int i = 0, j = 0;

        if (metroChecker35.Checked)
        {
            try
            {
                i = int.Parse(metroTextbox25.Text.Replace(" ", "").Replace('\t'.ToString(), ""));

                if (i < 0)
                {
                    i = 0;
                }
            }
            catch
            {

            }
        }

        foreach (string token in SplitToLines(metroTextbox1.Text))
        {
            if (blackListedTokens.Contains(token))
            {
                continue;
            }

            if (metroChecker35.Checked)
            {
                if (j == i)
                {
                    break;
                }

                j++;
            }

            if (metroChecker3.Checked)
            {
                Thread.Sleep(metroTrackbar1.Value);
            }

            new Thread(() => joinGuild(token)).Start();
        }
    }

    public bool InJoinRaidMode()
    {
        return guildRaidMode && metroChecker76.Checked && metroButton4.Enabled;
    }

    public void joinGuild(string token)
    {
        try
        {
            if (blackListedTokens.Contains(token))
            {
                return;
            }

            Leaf.xNet.HttpRequest httpRequest = Utils.CreateCleanRequest();
            string[] proxy = null;

            try
            {
                if (metroChecker13.Checked)
                {
                    proxy = Microsoft.VisualBasic.Strings.Split(GetRandomProxy(), ":");
                    httpRequest.Proxy = new Leaf.xNet.HttpProxyClient(proxy[0], int.Parse(proxy[1]));
                }
                else
                {
                    httpRequest.Proxy = null;
                }
            }
            catch
            {

            }

            httpRequest.AddHeader("Accept", "*/*");
            httpRequest.AddHeader("Accept-Encoding", "gzip, deflate, br");
            httpRequest.AddHeader("Accept-Language", getTokenLanguage(token));
            httpRequest.AddHeader("Authorization", token);
            httpRequest.AddHeader("Connection", "keep-alive");
            httpRequest.AddHeader("Content-Length", "0");
            httpRequest.AddHeader("Cookie", Utils.GetRandomCookie(getTokenLanguage(token)));
            httpRequest.AddHeader("DNT", "1");
            httpRequest.AddHeader("Host", "discord.com");
            httpRequest.AddHeader("Origin", "https://discord.com");
            httpRequest.AddHeader("TE", "Trailers");
            httpRequest.AddHeader("Referer", "https://discord.com/channels/@me");
            httpRequest.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:88.0) Gecko/20100101 Firefox/88.0");
            httpRequest.AddHeader("X-Context-Properties", theXCP64);
            httpRequest.AddHeader("X-Super-Properties", "eyJvcyI6IldpbmRvd3MiLCJicm93c2VyIjoiRmlyZWZveCIsImRldmljZSI6IiIsInN5c3RlbV9sb2NhbGUiOiJpdC1JVCIsImJyb3dzZXJfdXNlcl9hZ2VudCI6Ik1vemlsbGEvNS4wIChXaW5kb3dzIE5UIDEwLjA7IFdpbjY0OyB4NjQ7IHJ2Ojg4LjApIEdlY2tvLzIwMTAwMTAxIEZpcmVmb3gvODguMCIsImJyb3dzZXJfdmVyc2lvbiI6Ijg4LjAiLCJvc192ZXJzaW9uIjoiMTAiLCJyZWZlcnJlciI6IiIsInJlZmVycmluZ19kb21haW4iOiIiLCJyZWZlcnJlcl9jdXJyZW50IjoiIiwicmVmZXJyaW5nX2RvbWFpbl9jdXJyZW50IjoiIiwicmVsZWFzZV9jaGFubmVsIjoic3RhYmxlIiwiY2xpZW50X2J1aWxkX251bWJlciI6ODMwNDAsImNsaWVudF9ldmVudF9zb3VyY2UiOm51bGx9");
            httpRequest.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;

            string response = Utils.DecompressResponse(httpRequest.Post("https://discord.com/api/v9/invites/" + GetInviteCodeByInviteLink(inviteCode)));

            addLiveLog(proxy == null ? "Token (" + token + ") tried to join the guild with invite code '" + inviteCode + "' and ID '" + theGuildId + "' -> " + response : "Token (" + token + ") with proxy (" + proxy[0] + ":" + proxy[1] + ") tried to join the guild with invite code '" + inviteCode + "' and ID '" + theGuildId + "' -> " + response);

            if (response.ToLower().Contains("verify your account") || response.ToLower().Contains("you are being blocked from accessing our"))
            {
                blackListedTokens.Add(token);
                addLiveLog("Token (" + token + ") has been blacklisted because it is unverified or blocked by Discord -> " + response);

                return;
            }

            if (InJoinRaidMode())
            {
                if (metroChecker23.Checked)
                {
                    Thread.Sleep(500);

                    bypassRules1(token);
                }
                else
                {
                    if (metroChecker3.Checked)
                    {
                        Thread.Sleep(metroTrackbar1.Value);
                    }

                    leaveGuild(token);
                }
            }
            else
            {
                if (metroChecker23.Checked)
                {
                    Thread.Sleep(500);

                    bypassRules1(token);
                }

                if (metroChecker48.Checked || metroChecker65.Checked)
                {
                    bypassCaptcha(token);
                }

                if (metroChecker50.Checked)
                {
                    bypassReaction(token);
                }
            }
        }
        catch
        {

        }
    }

    public void bypassReaction(string token)
    {
        try
        {
            if (blackListedTokens.Contains(token))
            {
                return;
            }

            bool exist = false;

            foreach (Tuple<string, DiscordSocketClient> tuple in sessions)
            {
                if (tuple.Item1 == token)
                {
                    exist = true;

                    break;
                }
            }

            if (!exist)
            {
                if (blackListedTokens.Contains(token))
                {
                    return;
                }

                DiscordSocketConfig config = new DiscordSocketConfig();

                if (metroChecker13.Checked)
                {
                    config.Proxy = AnarchyProxy.Parse(AnarchyProxyType.HTTP, GetRandomProxy());
                }

                config.ApiVersion = 9;
                DiscordSocketClient client = new DiscordSocketClient(config);
                client.Login(token);
                Thread.Sleep(500);
                sessions.Add(new Tuple<string, DiscordSocketClient>(token, client));
                Tuple<bool, string, string, string> info = GetInviteInformations(metroTextbox3.Text);

                try
                {
                    foreach (DiscordChannel channel in client.GetGuild(ulong.Parse(info.Item2)).GetChannels())
                    {
                        try
                        {
                            foreach (DiscordMessage message in client.GetChannelMessages(channel))
                            {
                                try
                                {
                                    foreach (MessageReaction reaction in message.Reactions)
                                    {
                                        try
                                        {
                                            client.AddMessageReaction(channel.Id, message.Id, reaction.Emoji.Name, reaction.Emoji.Id);
                                        }
                                        catch
                                        {

                                        }
                                    }
                                }
                                catch
                                {

                                }
                            }
                        }
                        catch
                        {

                        }
                    }
                }
                catch
                {

                }
            }
            else
            {
                Tuple<bool, string, string, string> info = GetInviteInformations(metroTextbox3.Text);

                foreach (Tuple<string, DiscordSocketClient> tuple in sessions)
                {
                    try
                    {
                        try
                        {
                            foreach (DiscordChannel channel in tuple.Item2.GetGuild(ulong.Parse(info.Item2)).GetChannels())
                            {
                                try
                                {
                                    foreach (DiscordMessage message in tuple.Item2.GetChannelMessages(channel))
                                    {
                                        try
                                        {
                                            foreach (MessageReaction reaction in message.Reactions)
                                            {
                                                try
                                                {
                                                    tuple.Item2.AddMessageReaction(channel.Id, message.Id, reaction.Emoji.Name, reaction.Emoji.Id);
                                                }
                                                catch
                                                {

                                                }
                                            }
                                        }
                                        catch
                                        {

                                        }
                                    }
                                }
                                catch
                                {

                                }
                            }
                        }
                        catch
                        {

                        }
                    }
                    catch
                    {

                    }
                }
            }
        }
        catch
        {

        }
    }

    public void bypassRules1(string token)
    {
        try
        {
            if (blackListedTokens.Contains(token))
            {
                return;
            }

            Leaf.xNet.HttpRequest httpRequest = Utils.CreateCleanRequest();
            string[] proxy = null;

            try
            {
                if (metroChecker13.Checked)
                {
                    proxy = Microsoft.VisualBasic.Strings.Split(GetRandomProxy(), ":");
                    httpRequest.Proxy = new Leaf.xNet.HttpProxyClient(proxy[0], int.Parse(proxy[1]));
                }
                else
                {
                    httpRequest.Proxy = null;
                }
            }
            catch
            {

            }

            httpRequest.AddHeader("Accept", "*/*");
            httpRequest.AddHeader("Accept-Encoding", "gzip, deflate, br");
            httpRequest.AddHeader("Accept-Language", getTokenLanguage(token));
            httpRequest.AddHeader("Authorization", token);
            httpRequest.AddHeader("Connection", "keep-alive");
            httpRequest.AddHeader("Cookie", Utils.GetRandomCookie(getTokenLanguage(token)));
            httpRequest.AddHeader("DNT", "1");
            httpRequest.AddHeader("Host", "discord.com");
            httpRequest.AddHeader("Referer", "https://discord.com/channels/" + theGuildId + "/" + theChannelId);
            httpRequest.AddHeader("TE", "Trailers");
            httpRequest.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:88.0) Gecko/20100101 Firefox/88.0");
            httpRequest.AddHeader("X-Super-Properties", "eyJvcyI6IldpbmRvd3MiLCJicm93c2VyIjoiRmlyZWZveCIsImRldmljZSI6IiIsImJyb3dzZXJfdXNlcl9hZ2VudCI6Ik1vemlsbGEvNS4wIChXaW5kb3dzIE5UIDEwLjA7IFdpbjY0OyB4NjQ7IHJ2Ojg3LjApIEdlY2tvLzIwMTAwMTAxIEZpcmVmb3gvODcuMCIsImJyb3dzZXJfdmVyc2lvbiI6Ijg3LjAiLCJvc192ZXJzaW9uIjoiMTAiLCJyZWZlcnJlciI6IiIsInJlZmVycmluZ19kb21haW4iOiIiLCJyZWZlcnJlcl9jdXJyZW50IjoiIiwicmVmZXJyaW5nX2RvbWFpbl9jdXJyZW50IjoiIiwicmVsZWFzZV9jaGFubmVsIjoic3RhYmxlIiwiY2xpZW50X2J1aWxkX251bWJlciI6ODE0NTIsImNsaWVudF9ldmVudF9zb3VyY2UiOm51bGx9");

            httpRequest.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;

            string result = Utils.DecompressResponse(httpRequest.Get("https://discord.com/api/v9/guilds/" + theGuildId + "/member-verification?with_guild=false&invite_code=" + inviteCode));
            addLiveLog(proxy == null ? "Token (" + token + ") joined and now is getting the field forms of the Discord rules verification to bypass nextly in guild of invite code '" + inviteCode + "' and ID '" + theGuildId + "' -> " + result : "Token (" + token + ") with proxy (" + proxy[0] + ":" + proxy[1] + ") joined and now is getting the field forms of the Discord rules verification to bypass nextly in guild of invite code '" + inviteCode + "' and ID '" + theGuildId + "' -> " + result);

            if (result.ToLower().Contains("verify your account") || result.ToLower().Contains("you are being blocked from accessing our"))
            {
                blackListedTokens.Add(token);
                addLiveLog("Token (" + token + ") has been blacklisted because it is unverified or blocked by Discord -> " + result);

                return;
            }

            new Thread(() => bypassRules2(token, result)).Start();
        }
        catch
        {

        }
    }

    public void bypassRules2(string token, string data)
    {
        try
        {
            if (blackListedTokens.Contains(token))
            {
                return;
            }

            Leaf.xNet.HttpRequest httpRequest = Utils.CreateCleanRequest();
            string[] proxy = null;

            try
            {
                if (metroChecker13.Checked)
                {
                    proxy = Microsoft.VisualBasic.Strings.Split(GetRandomProxy(), ":");
                    httpRequest.Proxy = new Leaf.xNet.HttpProxyClient(proxy[0], int.Parse(proxy[1]));
                }
                else
                {
                    httpRequest.Proxy = null;
                }
            }
            catch
            {

            }

            string toSend = "";

            if (data.Contains("\"form_fields\": []") || data.Contains("\"form_fields\":[]"))
            {
                string lol1 = Microsoft.VisualBasic.Strings.Split(data, "{\"version\": \"")[1];
                string lol2 = Microsoft.VisualBasic.Strings.Split(lol1, "\"")[0];

                toSend = "{\"version\": \"" + lol2 + "\",\"form_fields\": []}";
            }
            else
            {
                string lol1 = Microsoft.VisualBasic.Strings.Split(data, "}], \"description\":")[0];

                toSend = lol1 + ",\"response\":true}]}";
            }

            httpRequest.AddHeader("Accept", "*/*");
            httpRequest.AddHeader("Accept-Encoding", "gzip, deflate, br");
            httpRequest.AddHeader("Accept-Language", getTokenLanguage(token));
            httpRequest.AddHeader("Authorization", token);
            httpRequest.AddHeader("Connection", "keep-alive");
            httpRequest.AddHeader("Content-Length", toSend.Length.ToString());
            httpRequest.AddHeader("Content-Type", "application/json");
            httpRequest.AddHeader("Cookie", Utils.GetRandomCookie(getTokenLanguage(token)));
            httpRequest.AddHeader("DNT", "1");
            httpRequest.AddHeader("Host", "discord.com");
            httpRequest.AddHeader("Origin", "https://discord.com");
            httpRequest.AddHeader("Referer", "https://discord.com/channels/" + theGuildId + "/" + theChannelId);
            httpRequest.AddHeader("TE", "Trailers");
            httpRequest.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:88.0) Gecko/20100101 Firefox/88.0");
            httpRequest.AddHeader("X-Super-Properties", "eyJvcyI6IldpbmRvd3MiLCJicm93c2VyIjoiRmlyZWZveCIsImRldmljZSI6IiIsImJyb3dzZXJfdXNlcl9hZ2VudCI6Ik1vemlsbGEvNS4wIChXaW5kb3dzIE5UIDEwLjA7IFdpbjY0OyB4NjQ7IHJ2Ojg3LjApIEdlY2tvLzIwMTAwMTAxIEZpcmVmb3gvODcuMCIsImJyb3dzZXJfdmVyc2lvbiI6Ijg3LjAiLCJvc192ZXJzaW9uIjoiMTAiLCJyZWZlcnJlciI6IiIsInJlZmVycmluZ19kb21haW4iOiIiLCJyZWZlcnJlcl9jdXJyZW50IjoiIiwicmVmZXJyaW5nX2RvbWFpbl9jdXJyZW50IjoiIiwicmVsZWFzZV9jaGFubmVsIjoic3RhYmxlIiwiY2xpZW50X2J1aWxkX251bWJlciI6ODE0NTIsImNsaWVudF9ldmVudF9zb3VyY2UiOm51bGx9");

            string response = Utils.DecompressResponse(httpRequest.Put("https://discord.com/api/v9/guilds/" + theGuildId + "/requests/@me", toSend, "application/json"));
            addLiveLog(proxy == null ? "Token (" + token + ") has bypassed the Discord rules verification system in guild of invite code '" + inviteCode + "' and ID '" + theGuildId + "' -> " + response : "Token (" + token + ") with proxy (" + proxy[0] + ":" + proxy[1] + ") has bypassed the Discord rules verification system in guild of invite code '" + inviteCode + "' and ID '" + theGuildId + "' -> " + response);

            if (response.ToLower().Contains("verify your account") || response.ToLower().Contains("you are being blocked from accessing our"))
            {
                blackListedTokens.Add(token);
                addLiveLog("Token (" + token + ") has been blacklisted because it is unverified or blocked by Discord -> " + response);

                return;
            }

            if (InJoinRaidMode())
            {
                if (metroChecker3.Checked)
                {
                    Thread.Sleep(metroTrackbar1.Value);
                }

                leaveGuild(token);
            }
        }
        catch
        {

        }
    }

    public async void bypassCaptcha(string token)
    {
        try
        {
            if (blackListedTokens.Contains(token))
            {
                return;
            }

            bool exist = false;

            foreach (Tuple<string, DiscordSocketClient> tuple in sessions)
            {
                try
                {
                    if (tuple.Item1 == token)
                    {
                        exist = true;

                        break;
                    }
                }
                catch
                {

                }
            }

            if (!exist)
            {
                if (blackListedTokens.Contains(token))
                {
                    return;
                }

                DiscordSocketConfig config = new DiscordSocketConfig();
                string proxy = GetRandomProxy();

                if (metroChecker13.Checked)
                {
                    config.Proxy = AnarchyProxy.Parse(AnarchyProxyType.HTTP, proxy);
                }

                config.ApiVersion = 9;
                DiscordSocketClient client = new DiscordSocketClient(config);
                client.Login(token);
                Thread.Sleep(500);
                sessions.Add(new Tuple<string, DiscordSocketClient>(token, client));

                try
                {
                    string recipient = metroTextbox40.Text;

                    if (metroChecker48.Checked)
                    {
                        if (metroChecker13.Checked)
                        {
                            recipient = createDM(token, metroTextbox39.Text, 5000, Microsoft.VisualBasic.Strings.Split(proxy, ":")).Result;
                        }
                        else
                        {
                            recipient = createDM(token, metroTextbox39.Text).Result;
                        }                       
                    }

                    if (recipient != "404" && recipient != "" && recipient != null)
                    {
                        foreach (DiscordMessage captchaMessage in client.GetChannelMessages(ulong.Parse(recipient)))
                        {
                            string captchaUrl = captchaMessage.Embed.Image.Url;
                            string captchaBase64 = Convert.ToBase64String(new WebClient().DownloadData(captchaUrl));

                            TwoCaptcha.TwoCaptcha solver = new TwoCaptcha.TwoCaptcha(metroTextbox32.Text);

                            Normal captcha = new Normal();
                            captcha.SetBase64(captchaBase64);
                            captcha.SetCaseSensitive(true);

                            await solver.Solve(captcha);
                            string solved = captcha.Code;
                            bool ciao = false;
                            int a = 0;

                            if (metroChecker48.Checked)
                            {
                                if (metroChecker13.Checked)
                                {
                                    //processDmSpamMessage(token, ref solved, recipient, Microsoft.VisualBasic.Strings.Split(proxy, ":"), ref ciao, ref a);
                                    processDmSpamMessage(token, ref solved, recipient, Microsoft.VisualBasic.Strings.Split(proxy, ":"));
                                }
                                else
                                {
                                    //processDmSpamMessage(token, ref solved, recipient, null, ref ciao, ref a);
                                    processDmSpamMessage(token, ref solved, recipient, null);
                                }
                            }
                            else
                            {
                                if (metroChecker13.Checked)
                                {
                                    string[] proxyn = Microsoft.VisualBasic.Strings.Split(proxy, ":");
                                    processMessage(token, ref proxyn, ref solved, recipient);
                                }
                                else
                                {
                                    string[] proxyn = null;
                                    processMessage(token, ref proxyn, ref solved, recipient);
                                }
                            }

                            break;
                        }
                    }
                }
                catch
                {

                }
            }
            else
            {
                foreach (Tuple<string, DiscordSocketClient> tuple in sessions)
                {
                    try
                    {
                        if (tuple.Item1 == token)
                        {
                            string recipient = metroTextbox40.Text;

                            if (metroChecker48.Checked)
                            {
                                recipient = createDM(token, metroTextbox39.Text).Result;
                            }

                            if (recipient != "404" && recipient != "" && recipient != null)
                            {
                                Thread.Sleep(1235);

                                foreach (DiscordMessage captchaMessage in tuple.Item2.GetChannelMessages(ulong.Parse(recipient)))
                                {
                                    string captchaUrl = captchaMessage.Embed.Image.Url;
                                    string captchaBase64 = Convert.ToBase64String(new WebClient().DownloadData(captchaUrl));

                                    TwoCaptcha.TwoCaptcha solver = new TwoCaptcha.TwoCaptcha(metroTextbox32.Text);

                                    Normal captcha = new Normal();
                                    captcha.SetBase64(captchaBase64);
                                    captcha.SetCaseSensitive(true);

                                    await solver.Solve(captcha);
                                    string solved = captcha.Code;
                                    bool ciao = false;
                                    int a = 0;

                                    if (metroChecker48.Checked)
                                    {
                                        if (metroChecker13.Checked)
                                        {
                                            //processDmSpamMessage(token, ref solved, recipient, Microsoft.VisualBasic.Strings.Split(GetRandomProxy(), ":"), ref ciao, ref a);
                                            processDmSpamMessage(token, ref solved, recipient, Microsoft.VisualBasic.Strings.Split(GetRandomProxy(), ":"));
                                        }
                                        else
                                        {
                                            //processDmSpamMessage(token, ref solved, recipient, null, ref ciao, ref a);
                                            processDmSpamMessage(token, ref solved, recipient, null);
                                        }
                                    }
                                    else
                                    {
                                        if (metroChecker13.Checked)
                                        {
                                            string[] proxyn = Microsoft.VisualBasic.Strings.Split(GetRandomProxy(), ":");
                                            processMessage(token, ref proxyn, ref solved, recipient);
                                        }
                                        else
                                        {
                                            string[] proxyn = null;
                                            processMessage(token, ref proxyn, ref solved, recipient);
                                        }
                                    }

                                    break;
                                }
                            }

                            break;
                        }
                    }
                    catch
                    {

                    }
                }
            }
        }
        catch
        {

        }
    }

    private void metroButton4_Click(object sender, EventArgs e)
    {
        if (metroChecker76.Checked)
        {
            metroButton4.Enabled = false;
            metroButton3.Enabled = true;
        }

        if (metroChecker72.Checked)
        {
            new Thread(startGroupLeaver).Start();
        }
        else
        {
            new Thread(startGuildLeaver).Start();
        }
    }

    public void startGroupLeaver()
    {
        string token = GetRandomValidToken();
        Tuple<bool, string, string> info = GetGroupInviteInformations(metroTextbox3.Text);

        if (IsIDValid(metroTextbox3.Text) || IsIDValid(info.Item2))
        {
            theChannelId = "";
            theChannelId = info.Item2;

            if (theChannelId != "404")
            {
                MessageBox.Show("aaa");
                addLiveLog("Succesfully started Guild Leaver for the channel ID '" + theChannelId + "'.");
                new Thread(doGroupLeaver).Start();
            }
        }
        else
        {
            MessageBox.Show("The group ID / invite link / invite code is not valid!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public void doGroupLeaver()
    {
        int i = 0, j = 0;

        if (metroChecker35.Checked)
        {
            try
            {
                i = int.Parse(metroTextbox25.Text.Replace(" ", "").Replace('\t'.ToString(), ""));

                if (i < 0)
                {
                    i = 0;
                }
            }
            catch
            {

            }
        }

        foreach (string token in SplitToLines(metroTextbox1.Text))
        {
            if (blackListedTokens.Contains(token))
            {
                continue;
            }

            if (metroChecker35.Checked)
            {
                if (j == i)
                {
                    break;
                }

                j++;
            }

            if (metroChecker3.Checked)
            {
                Thread.Sleep(metroTrackbar1.Value);
            }

            new Thread(() => leaveGroup(token)).Start();
        }
    }

    public void leaveGroup(string token)
    {
        try
        {
            if (blackListedTokens.Contains(token))
            {
                return;
            }

            Leaf.xNet.HttpRequest httpRequest = Utils.CreateCleanRequest();
            string[] proxy = null;

            try
            {
                if (metroChecker13.Checked)
                {
                    proxy = Microsoft.VisualBasic.Strings.Split(GetRandomProxy(), ":");
                    httpRequest.Proxy = new Leaf.xNet.HttpProxyClient(proxy[0], int.Parse(proxy[1]));
                }
                else
                {
                    httpRequest.Proxy = null;
                }
            }
            catch
            {

            }

            httpRequest.AddHeader("Accept", "*/*");
            httpRequest.AddHeader("Accept-Encoding", "gzip, deflate, br");
            httpRequest.AddHeader("Accept-Language", getTokenLanguage(token));
            httpRequest.AddHeader("Authorization", token);
            httpRequest.AddHeader("Connection", "keep-alive");
            httpRequest.AddHeader("Cookie", Utils.GetRandomCookie(getTokenLanguage(token)));
            httpRequest.AddHeader("DNT", "1");
            httpRequest.AddHeader("Host", "discord.com");
            httpRequest.AddHeader("Origin", "https://discord.com");
            httpRequest.AddHeader("Referer", "https://discord.com/channels/@me");
            httpRequest.AddHeader("TE", "Trailers");
            httpRequest.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:88.0) Gecko/20100101 Firefox/88.0");
            httpRequest.AddHeader("X-Super-Properties", "eyJvcyI6IldpbmRvd3MiLCJicm93c2VyIjoiRmlyZWZveCIsImRldmljZSI6IiIsImJyb3dzZXJfdXNlcl9hZ2VudCI6Ik1vemlsbGEvNS4wIChXaW5kb3dzIE5UIDEwLjA7IFdpbjY0OyB4NjQ7IHJ2Ojg3LjApIEdlY2tvLzIwMTAwMTAxIEZpcmVmb3gvODcuMCIsImJyb3dzZXJfdmVyc2lvbiI6Ijg3LjAiLCJvc192ZXJzaW9uIjoiMTAiLCJyZWZlcnJlciI6IiIsInJlZmVycmluZ19kb21haW4iOiIiLCJyZWZlcnJlcl9jdXJyZW50IjoiIiwicmVmZXJyaW5nX2RvbWFpbl9jdXJyZW50IjoiIiwicmVsZWFzZV9jaGFubmVsIjoic3RhYmxlIiwiY2xpZW50X2J1aWxkX251bWJlciI6ODE0NTIsImNsaWVudF9ldmVudF9zb3VyY2UiOm51bGx9");

            httpRequest.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;

            string response = Utils.DecompressResponse(httpRequest.Delete("https://discord.com/api/v9/channels/" + theChannelId));
            addLiveLog(proxy == null ? "Token (" + token + ") left the channel with ID " + theChannelId + " -> " + response : "Token (" + token + ") with proxy (" + proxy[0] + ":" + proxy[1] + ") left the group with ID " + theChannelId + " -> " + response);

            if (response.ToLower().Contains("verify your account") || response.ToLower().Contains("you are being blocked from accessing our"))
            {
                blackListedTokens.Add(token);
                addLiveLog("Token (" + token + ") has been blacklisted because it is unverified or blocked by Discord -> " + response);

                return;
            }

            if (InJoinRaidMode())
            {
                if (metroChecker3.Checked)
                {
                    Thread.Sleep(metroTrackbar1.Value);
                }

                joinGroup(token);
            }
        }
        catch
        {

        }
    }

    public void startGuildLeaver()
    {
        string token = GetRandomValidToken();
        Tuple<bool, string, string, string> info = GetInviteInformations(metroTextbox3.Text);

        if (IsIDValid(metroTextbox3.Text) || IsIDValid(info.Item2))
        {
            theGuildId = "";
            theGuildId = info.Item2;

            if (theGuildId != "404")
            {
                addLiveLog("Succesfully started Guild Leaver for the guild ID '" + theGuildId + "'.");
                new Thread(doGuildLeaver).Start();
            }
        }
        else
        {
            MessageBox.Show("The guild ID / invite link / invite code is not valid!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public void doGuildLeaver()
    {
        int i = 0, j = 0;

        if (metroChecker35.Checked)
        {
            try
            {
                i = int.Parse(metroTextbox25.Text.Replace(" ", "").Replace('\t'.ToString(), ""));

                if (i < 0)
                {
                    i = 0;
                }
            }
            catch
            {

            }
        }

        foreach (string token in SplitToLines(metroTextbox1.Text))
        {
            if (blackListedTokens.Contains(token))
            {
                continue;
            }

            if (metroChecker35.Checked)
            {
                if (j == i)
                {
                    break;
                }

                j++;
            }

            if (metroChecker3.Checked)
            {
                Thread.Sleep(metroTrackbar1.Value);
            }

            new Thread(() => leaveGuild(token)).Start();
        }
    }

    public void leaveGuild(string token)
    {
        try
        {
            if (blackListedTokens.Contains(token))
            {
                return;
            }

            Leaf.xNet.HttpRequest httpRequest = Utils.CreateCleanRequest();
            string[] proxy = null;

            try
            {
                if (metroChecker13.Checked)
                {
                    proxy = Microsoft.VisualBasic.Strings.Split(GetRandomProxy(), ":");
                    httpRequest.Proxy = new Leaf.xNet.HttpProxyClient(proxy[0], int.Parse(proxy[1]));
                }
                else
                {
                    httpRequest.Proxy = null;
                }
            }
            catch
            {

            }

            httpRequest.AddHeader("Accept", "*/*");
            httpRequest.AddHeader("Accept-Encoding", "gzip, deflate, br");
            httpRequest.AddHeader("Accept-Language", getTokenLanguage(token));
            httpRequest.AddHeader("Authorization", token);
            httpRequest.AddHeader("Connection", "keep-alive");
            httpRequest.AddHeader("Cookie", Utils.GetRandomCookie(getTokenLanguage(token)));
            httpRequest.AddHeader("DNT", "1");
            httpRequest.AddHeader("Host", "discord.com");
            httpRequest.AddHeader("Origin", "https://discord.com");
            httpRequest.AddHeader("Referer", "https://discord.com/channels/@me");
            httpRequest.AddHeader("TE", "Trailers");
            httpRequest.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:88.0) Gecko/20100101 Firefox/88.0");
            httpRequest.AddHeader("X-Super-Properties", "eyJvcyI6IldpbmRvd3MiLCJicm93c2VyIjoiRmlyZWZveCIsImRldmljZSI6IiIsImJyb3dzZXJfdXNlcl9hZ2VudCI6Ik1vemlsbGEvNS4wIChXaW5kb3dzIE5UIDEwLjA7IFdpbjY0OyB4NjQ7IHJ2Ojg3LjApIEdlY2tvLzIwMTAwMTAxIEZpcmVmb3gvODcuMCIsImJyb3dzZXJfdmVyc2lvbiI6Ijg3LjAiLCJvc192ZXJzaW9uIjoiMTAiLCJyZWZlcnJlciI6IiIsInJlZmVycmluZ19kb21haW4iOiIiLCJyZWZlcnJlcl9jdXJyZW50IjoiIiwicmVmZXJyaW5nX2RvbWFpbl9jdXJyZW50IjoiIiwicmVsZWFzZV9jaGFubmVsIjoic3RhYmxlIiwiY2xpZW50X2J1aWxkX251bWJlciI6ODE0NTIsImNsaWVudF9ldmVudF9zb3VyY2UiOm51bGx9");

            httpRequest.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;

            string response = Utils.DecompressResponse(httpRequest.Delete("https://discord.com/api/v9/users/@me/guilds/" + theGuildId));
            addLiveLog(proxy == null ? "Token (" + token + ") left the guild with ID " + theGuildId + " -> " + response : "Token (" + token + ") with proxy (" + proxy[0] + ":" + proxy[1] + ") left the guild with ID " + theGuildId + " -> " + response);

            if (response.ToLower().Contains("verify your account") || response.ToLower().Contains("you are being blocked from accessing our"))
            {
                blackListedTokens.Add(token);
                addLiveLog("Token (" + token + ") has been blacklisted because it is unverified or blocked by Discord -> " + response);

                return;
            }

            if (InJoinRaidMode())
            {
                if (metroChecker3.Checked)
                {
                    Thread.Sleep(metroTrackbar1.Value);
                }

                joinGuild(token);
            }
        }
        catch
        {

        }
    }

    private void metroButton8_Click(object sender, EventArgs e)
    {
        addLiveLog("Succesfully started Reaction adder for the message ID " + metroTextbox8.Text + " in channel ID " + metroTextbox7.Text + " with emoji '" + metroTextbox6.Text + "'.");
        new Thread(doReactionAdder).Start();
    }

    public void doReactionAdder()
    {
        if (!IsIDValid(metroTextbox7.Text))
        {
            MessageBox.Show("The id of channel is not valid!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);

            return;
        }

        if (!IsIDValid(metroTextbox8.Text))
        {
            MessageBox.Show("The id of message is not valid!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);

            return;
        }

        if (metroChecker62.Checked)
        {
            if (!metroTextbox6.Text.Contains(":") && !metroTextbox6.Text.Contains("%3A"))
            {
                MessageBox.Show("The emote is not valid!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }
            else
            {
                string[] splitter = Microsoft.VisualBasic.Strings.Split(metroTextbox6.Text, "%3A");

                if (metroTextbox6.Text.Contains(":"))
                {
                    splitter = Microsoft.VisualBasic.Strings.Split(metroTextbox6.Text, ":");
                }

                if (!IsIDValid(splitter[1]))
                {
                    MessageBox.Show("The emote id is not valid!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }

                if (splitter[0].Replace(" ", "").Replace('\t'.ToString(), "") == "")
                {
                    MessageBox.Show("The emote name is not valid!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }
            }
        }
        else
        {
            if (metroTextbox6.Text.Length > 3 || metroTextbox6.Text.Replace(" ", "").Trim().Replace('\t'.ToString(), "").Length > 3)
            {
                MessageBox.Show("The reaction is not valid!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }
        }

        int i = 0, j = 0;

        if (metroChecker37.Checked)
        {
            try
            {
                i = int.Parse(metroTextbox27.Text.Replace(" ", "").Replace('\t'.ToString(), ""));

                if (i < 0)
                {
                    i = 0;
                }
            }
            catch
            {

            }
        }

        foreach (string token in SplitToLines(metroTextbox1.Text))
        {
            if (blackListedTokens.Contains(token))
            {
                continue;
            }

            if (metroChecker37.Checked)
            {
                if (j == i)
                {
                    break;
                }

                j++;
            }

            if (metroChecker9.Checked)
            {
                Thread.Sleep(metroTrackbar3.Value);
            }

            new Thread(() => addReaction(token)).Start();
        }
    }

    public void addReaction(string token)
    {
        try
        {
            if (blackListedTokens.Contains(token))
            {
                return;
            }

            Leaf.xNet.HttpRequest httpRequest = Utils.CreateCleanRequest();
            string[] proxy = null;

            try
            {
                if (metroChecker13.Checked)
                {
                    proxy = Microsoft.VisualBasic.Strings.Split(GetRandomProxy(), ":");
                    httpRequest.Proxy = new Leaf.xNet.HttpProxyClient(proxy[0], int.Parse(proxy[1]));
                }
                else
                {
                    httpRequest.Proxy = null;
                }
            }
            catch
            {

            }

            httpRequest.AddHeader("Accept", "*/*");
            httpRequest.AddHeader("Accept-Encoding", "gzip, deflate, br");
            httpRequest.AddHeader("Accept-Language", getTokenLanguage(token));
            httpRequest.AddHeader("Authorization", token);
            httpRequest.AddHeader("Connection", "keep-alive");
            httpRequest.AddHeader("Content-Length", "0");
            httpRequest.AddHeader("Cookie", Utils.GetRandomCookie(getTokenLanguage(token)));
            httpRequest.AddHeader("DNT", "1");
            httpRequest.AddHeader("Host", "discord.com");
            httpRequest.AddHeader("Origin", "https://discord.com");
            httpRequest.AddHeader("TE", "Trailers");
            httpRequest.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:88.0) Gecko/20100101 Firefox/88.0");
            httpRequest.AddHeader("X-Super-Properties", "eyJvcyI6IldpbmRvd3MiLCJicm93c2VyIjoiRmlyZWZveCIsImRldmljZSI6IiIsImJyb3dzZXJfdXNlcl9hZ2VudCI6Ik1vemlsbGEvNS4wIChXaW5kb3dzIE5UIDEwLjA7IFdpbjY0OyB4NjQ7IHJ2Ojg3LjApIEdlY2tvLzIwMTAwMTAxIEZpcmVmb3gvODcuMCIsImJyb3dzZXJfdmVyc2lvbiI6Ijg3LjAiLCJvc192ZXJzaW9uIjoiMTAiLCJyZWZlcnJlciI6IiIsInJlZmVycmluZ19kb21haW4iOiIiLCJyZWZlcnJlcl9jdXJyZW50IjoiIiwicmVmZXJyaW5nX2RvbWFpbl9jdXJyZW50IjoiIiwicmVsZWFzZV9jaGFubmVsIjoic3RhYmxlIiwiY2xpZW50X2J1aWxkX251bWJlciI6ODE0NTIsImNsaWVudF9ldmVudF9zb3VyY2UiOm51bGx9");

            httpRequest.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;

            string response = Utils.DecompressResponse(httpRequest.Put("https://discord.com/api/v9/channels/" + metroTextbox7.Text + "/messages/" + metroTextbox8.Text + "/reactions/" + metroTextbox6.Text.Replace("%3A", ":") + "/@me"));
            addLiveLog(proxy == null ? "Token (" + token + ") added reaction to message ID " + metroTextbox8.Text + " in channel ID " + metroTextbox7.Text + " with the emoji '" + metroTextbox6.Text + "' -> " + response : "Token (" + token + ") with proxy (" + proxy[0] + ":" + proxy[1] + " added reaction to message ID " + metroTextbox8.Text + " in channel ID " + metroTextbox7.Text + " with the emoji '" + metroTextbox6.Text + "' -> " + response);

            if (response.ToLower().Contains("verify your account") || response.ToLower().Contains("you are being blocked from accessing our"))
            {
                blackListedTokens.Add(token);
                addLiveLog("Token (" + token + ") has been blacklisted because it is unverified or blocked by Discord -> " + response);
            }
        }
        catch
        {

        }
    }

    private void metroButton7_Click(object sender, EventArgs e)
    {
        addLiveLog("Succesfully started Reaction remover for the message ID " + metroTextbox8.Text + " in channel ID " + metroTextbox7.Text + " with emoji '" + metroTextbox6.Text + "'.");
        new Thread(doReactionRemover).Start();
    }

    public void doReactionRemover()
    {
        if (!IsIDValid(metroTextbox7.Text))
        {
            MessageBox.Show("The id of channel is not valid!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);

            return;
        }

        if (!IsIDValid(metroTextbox8.Text))
        {
            MessageBox.Show("The id of message is not valid!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);

            return;
        }

        if (metroChecker62.Checked)
        {
            if (!metroTextbox6.Text.Contains(":") && !metroTextbox6.Text.Contains("%3A"))
            {
                MessageBox.Show("The emote is not valid!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }
            else
            {
                string[] splitter = Microsoft.VisualBasic.Strings.Split(metroTextbox6.Text, "%3A");

                if (metroTextbox6.Text.Contains(":"))
                {
                    splitter = Microsoft.VisualBasic.Strings.Split(metroTextbox6.Text, ":");
                }

                if (!IsIDValid(splitter[1]))
                {
                    MessageBox.Show("The emote id is not valid!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }

                if (splitter[0].Replace(" ", "").Replace('\t'.ToString(), "") == "")
                {
                    MessageBox.Show("The emote name is not valid!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }
            }
        }
        else
        {
            if (metroTextbox6.Text.Length > 3 || metroTextbox6.Text.Replace(" ", "").Trim().Replace('\t'.ToString(), "").Length > 3)
            {
                MessageBox.Show("The reaction is not valid!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }
        }

        int i = 0, j = 0;

        if (metroChecker37.Checked)
        {
            try
            {
                i = int.Parse(metroTextbox27.Text.Replace(" ", "").Replace('\t'.ToString(), ""));

                if (i < 0)
                {
                    i = 0;
                }
            }
            catch
            {

            }
        }

        foreach (string token in SplitToLines(metroTextbox1.Text))
        {
            if (blackListedTokens.Contains(token))
            {
                continue;
            }

            if (metroChecker37.Checked)
            {
                if (j == i)
                {
                    break;
                }

                j++;
            }

            if (metroChecker9.Checked)
            {
                Thread.Sleep(metroTrackbar3.Value);
            }

            new Thread(() => removeReaction(token)).Start();
        }
    }

    public void removeReaction(string token)
    {
        try
        {
            if (blackListedTokens.Contains(token))
            {
                return;
            }

            Leaf.xNet.HttpRequest httpRequest = Utils.CreateCleanRequest();
            string[] proxy = null;

            try
            {
                if (metroChecker13.Checked)
                {
                    proxy = Microsoft.VisualBasic.Strings.Split(GetRandomProxy(), ":");
                    httpRequest.Proxy = new Leaf.xNet.HttpProxyClient(proxy[0], int.Parse(proxy[1]));
                }
                else
                {
                    httpRequest.Proxy = null;
                }
            }
            catch
            {

            }

            httpRequest.AddHeader("Accept", "*/*");
            httpRequest.AddHeader("Accept-Encoding", "gzip, deflate, br");
            httpRequest.AddHeader("Accept-Language", getTokenLanguage(token));
            httpRequest.AddHeader("Authorization", token);
            httpRequest.AddHeader("Connection", "keep-alive");
            httpRequest.AddHeader("Cookie", Utils.GetRandomCookie(getTokenLanguage(token)));
            httpRequest.AddHeader("DNT", "1");
            httpRequest.AddHeader("Host", "discord.com");
            httpRequest.AddHeader("Origin", "https://discord.com");
            httpRequest.AddHeader("TE", "Trailers");
            httpRequest.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:88.0) Gecko/20100101 Firefox/88.0");
            httpRequest.AddHeader("X-Super-Properties", "eyJvcyI6IldpbmRvd3MiLCJicm93c2VyIjoiRmlyZWZveCIsImRldmljZSI6IiIsImJyb3dzZXJfdXNlcl9hZ2VudCI6Ik1vemlsbGEvNS4wIChXaW5kb3dzIE5UIDEwLjA7IFdpbjY0OyB4NjQ7IHJ2Ojg3LjApIEdlY2tvLzIwMTAwMTAxIEZpcmVmb3gvODcuMCIsImJyb3dzZXJfdmVyc2lvbiI6Ijg3LjAiLCJvc192ZXJzaW9uIjoiMTAiLCJyZWZlcnJlciI6IiIsInJlZmVycmluZ19kb21haW4iOiIiLCJyZWZlcnJlcl9jdXJyZW50IjoiIiwicmVmZXJyaW5nX2RvbWFpbl9jdXJyZW50IjoiIiwicmVsZWFzZV9jaGFubmVsIjoic3RhYmxlIiwiY2xpZW50X2J1aWxkX251bWJlciI6ODE0NTIsImNsaWVudF9ldmVudF9zb3VyY2UiOm51bGx9");

            httpRequest.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;

            string response = Utils.DecompressResponse(httpRequest.Delete("https://discord.com/api/v9/channels/" + metroTextbox7.Text + "/messages/" + metroTextbox8.Text + "/reactions/" + metroTextbox6.Text.Replace("%3A", ":") + "/@me"));
            addLiveLog(proxy == null ? "Token (" + token + ") removed reaction from message ID " + metroTextbox8.Text + " in channel ID " + metroTextbox7.Text + " with the emoji '" + metroTextbox6.Text + "' -> " + response : "Token (" + token + ") with proxy (" + proxy[0] + ":" + proxy[1] + " removed reaction from message ID " + metroTextbox8.Text + " in channel ID " + metroTextbox7.Text + " with the emoji '" + metroTextbox6.Text + "' -> " + response);

            if (response.ToLower().Contains("verify your account") || response.ToLower().Contains("you are being blocked from accessing our"))
            {
                blackListedTokens.Add(token);
                addLiveLog("Token (" + token + ") has been blacklisted because it is unverified or blocked by Discord -> " + response);
            }
        }
        catch
        {

        }
    }

    private void metroButton10_Click(object sender, EventArgs e)
    {
        if (metroChecker80.Checked)
        {
            metroButton10.Enabled = false;
            metroButton9.Enabled = true;
        }

        addLiveLog("Succesfully started Friend adder for the user ID " + metroTextbox9.Text + ".");
        new Thread(doFriendAdder).Start();
    }

    private void metroButton9_Click(object sender, EventArgs e)
    {
        if (metroChecker80.Checked)
        {
            metroButton9.Enabled = false;
            metroButton10.Enabled = true;
        }

        addLiveLog("Succesfully started Friend remover for the user ID " + metroTextbox9.Text + ".");
        new Thread(doFriendRemover).Start();
    }

    public bool IsTagValid(string tag)
    {
        if (tag.Length > 37)
        {
            return false;
        }

        if (!tag.Contains("#"))
        {
            return false;
        }

        string[] splitted = Microsoft.VisualBasic.Strings.Split(tag, "#");

        if (splitted[0].Replace(" ", "").Trim().Replace('\t'.ToString(), "") == "" || splitted[1].Replace(" ", "").Trim().Replace('\t'.ToString(), "") == "")
        {
            return false;
        }

        if (splitted[1].Replace(" ", "").Trim().Replace('\t'.ToString(), "").Length != 4)
        {
            return false;
        }

        if (!Microsoft.VisualBasic.Information.IsNumeric(splitted[1].Replace(" ", "").Trim().Replace('\t'.ToString(), "")))
        {
            return false;
        }

        return true;
    }

    public void doFriendAdder()
    {
        if (metroChecker80.Checked)
        {
            if (metroChecker79.Checked)
            {
                if (!AreIDsValid(metroTextbox9.Text))
                {
                    MessageBox.Show("The ids of the users are not valid!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }
            }
            else
            {
                if (!IsIDValid(metroTextbox9.Text))
                {
                    MessageBox.Show("The id of the users is not valid!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }
            }
        }
        else
        {
            if (metroChecker79.Checked)
            {
                if (!AreFriendsValid(metroTextbox9.Text))
                {
                    MessageBox.Show("The ids & tags of the users are not valid!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }
            }
            else
            {
                if (!IsIDValid(metroTextbox9.Text) && !IsTagValid(metroTextbox9.Text))
                {
                    MessageBox.Show("The id or tag of user is not valid!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }
            }
        }

        int i = 0, j = 0;

        if (metroChecker38.Checked)
        {
            try
            {
                i = int.Parse(metroTextbox28.Text.Replace(" ", "").Replace('\t'.ToString(), ""));

                if (i < 0)
                {
                    i = 0;
                }
            }
            catch
            {

            }
        }

        foreach (string token in SplitToLines(metroTextbox1.Text))
        {
            if (blackListedTokens.Contains(token))
            {
                continue;
            }

            if (metroChecker38.Checked)
            {
                if (j == i)
                {
                    break;
                }

                j++;
            }

            if (metroChecker10.Checked)
            {
                Thread.Sleep(metroTrackbar4.Value);
            }

            if (metroChecker79.Checked)
            {
                foreach (string friend in GetFriends(metroTextbox9.Text))
                {
                    new Thread(() => addFriend(token, friend)).Start();
                }
            }
            else
            {
                new Thread(() => addFriend(token, metroTextbox9.Text)).Start();
            }
        }
    }

    public void addFriend(string token, string friend)
    {
        if (blackListedTokens.Contains(token))
        {
            return;
        }

        Leaf.xNet.HttpRequest httpRequest = Utils.CreateCleanRequest();
        string[] proxy = null;

        try
        {
            if (metroChecker13.Checked)
            {
                proxy = Microsoft.VisualBasic.Strings.Split(GetRandomProxy(), ":");
                httpRequest.Proxy = new Leaf.xNet.HttpProxyClient(proxy[0], int.Parse(proxy[1]));
            }
            else
            {
                httpRequest.Proxy = null;
            }
        }
        catch
        {

        }

        string response = "";

        try
        {
            var request = new HttpRequestMessage();

            if (friend.Contains("#"))
            {
                string[] splitted = Microsoft.VisualBasic.Strings.Split(friend, "#");
                string messageJson = "{\"username\":\"" + splitted[0] + "\",\"discriminator\":" + splitted[1].Replace(" ", "").Trim().Replace('\t'.ToString(), "") + "}";

                httpRequest.AddHeader("Accept", "*/*");
                httpRequest.AddHeader("Accept-Encoding", "gzip, deflate, br");
                httpRequest.AddHeader("Accept-Language", getTokenLanguage(token));
                httpRequest.AddHeader("Authorization", token);
                httpRequest.AddHeader("Connection", "keep-alive");
                httpRequest.AddHeader("Content-Length", messageJson.Length.ToString());
                httpRequest.AddHeader("Content-Type", "application/json");
                httpRequest.AddHeader("Cookie", Utils.GetRandomCookie(getTokenLanguage(token)));
                httpRequest.AddHeader("DNT", "1");
                httpRequest.AddHeader("Host", "discord.com");
                httpRequest.AddHeader("Origin", "https://discord.com");
                httpRequest.AddHeader("Referer", "https://discord.com/channels/@me");
                httpRequest.AddHeader("TE", "Trailers");
                httpRequest.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:88.0) Gecko/20100101 Firefox/88.0");
                httpRequest.AddHeader("X-Context-Properties", "eyJsb2NhdGlvbiI6IkFkZCBGcmllbmQifQ==");
                httpRequest.AddHeader("X-Super-Properties", "eyJvcyI6IldpbmRvd3MiLCJicm93c2VyIjoiRmlyZWZveCIsImRldmljZSI6IiIsImJyb3dzZXJfdXNlcl9hZ2VudCI6Ik1vemlsbGEvNS4wIChXaW5kb3dzIE5UIDEwLjA7IFdpbjY0OyB4NjQ7IHJ2Ojg3LjApIEdlY2tvLzIwMTAwMTAxIEZpcmVmb3gvODcuMCIsImJyb3dzZXJfdmVyc2lvbiI6Ijg3LjAiLCJvc192ZXJzaW9uIjoiMTAiLCJyZWZlcnJlciI6IiIsInJlZmVycmluZ19kb21haW4iOiIiLCJyZWZlcnJlcl9jdXJyZW50IjoiIiwicmVmZXJyaW5nX2RvbWFpbl9jdXJyZW50IjoiIiwicmVsZWFzZV9jaGFubmVsIjoic3RhYmxlIiwiY2xpZW50X2J1aWxkX251bWJlciI6ODE0NTIsImNsaWVudF9ldmVudF9zb3VyY2UiOm51bGx9");

                response = Utils.DecompressResponse(httpRequest.Post("https://discord.com/api/v9/users/@me/relationships", messageJson, "application/json"));
                addLiveLog(proxy == null ? "Token (" + token + ") added as friend the user " + friend + " -> " + response : "Token (" + token + ") with proxy (" + proxy[0] + ":" + proxy[1] + " added as friend the user " + friend + " -> " + response);
            }
            else
            {
                string messageJson = "{}";

                httpRequest.AddHeader("Accept", "*/*");
                httpRequest.AddHeader("Accept-Encoding", "gzip, deflate, br");
                httpRequest.AddHeader("Accept-Language", getTokenLanguage(token));
                httpRequest.AddHeader("Authorization", token);
                httpRequest.AddHeader("Connection", "keep-alive");
                httpRequest.AddHeader("Content-Length", messageJson.Length.ToString());
                httpRequest.AddHeader("Content-Type", "application/json");
                httpRequest.AddHeader("Cookie", Utils.GetRandomCookie(getTokenLanguage(token)));
                httpRequest.AddHeader("DNT", "1");
                httpRequest.AddHeader("Host", "discord.com");
                httpRequest.AddHeader("Origin", "https://discord.com");
                httpRequest.AddHeader("Referer", "https://discord.com/channels/@me");
                httpRequest.AddHeader("TE", "Trailers");
                httpRequest.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:88.0) Gecko/20100101 Firefox/88.0");
                httpRequest.AddHeader("X-Context-Properties", "eyJsb2NhdGlvbiI6IkNvbnRleHRNZW51In0=");
                httpRequest.AddHeader("X-Super-Properties", "eyJvcyI6IldpbmRvd3MiLCJicm93c2VyIjoiRmlyZWZveCIsImRldmljZSI6IiIsImJyb3dzZXJfdXNlcl9hZ2VudCI6Ik1vemlsbGEvNS4wIChXaW5kb3dzIE5UIDEwLjA7IFdpbjY0OyB4NjQ7IHJ2Ojg3LjApIEdlY2tvLzIwMTAwMTAxIEZpcmVmb3gvODcuMCIsImJyb3dzZXJfdmVyc2lvbiI6Ijg3LjAiLCJvc192ZXJzaW9uIjoiMTAiLCJyZWZlcnJlciI6IiIsInJlZmVycmluZ19kb21haW4iOiIiLCJyZWZlcnJlcl9jdXJyZW50IjoiIiwicmVmZXJyaW5nX2RvbWFpbl9jdXJyZW50IjoiIiwicmVsZWFzZV9jaGFubmVsIjoic3RhYmxlIiwiY2xpZW50X2J1aWxkX251bWJlciI6ODE0NTIsImNsaWVudF9ldmVudF9zb3VyY2UiOm51bGx9");

                response = Utils.DecompressResponse(httpRequest.Put("https://discord.com/api/v9/users/@me/relationships/" + friend, messageJson, "application/json"));
                addLiveLog(proxy == null ? "Token (" + token + ") added as friend the user ID " + friend + " -> " + response : "Token (" + token + ") with proxy (" + proxy[0] + ":" + proxy[1] + " added as friend the user ID " + friend + " -> " + response);
            }

            if (response.ToLower().Contains("verify your account") || response.ToLower().Contains("you are being blocked from accessing our"))
            {
                blackListedTokens.Add(token);
                addLiveLog("Token (" + token + ") has been blacklisted because it is unverified or blocked by Discord -> " + response);
            }
        }
        catch
        {

        }

        if (metroChecker80.Checked && metroButton9.Enabled)
        {
            if (metroChecker10.Checked)
            {
                Thread.Sleep(metroTrackbar8.Value);
            }

            removeFriend(token, friend);
        }
    }

    public void doFriendRemover()
    {
        if (metroChecker79.Checked)
        {
            if (!AreIDsValid(metroTextbox9.Text))
            {
                MessageBox.Show("The ids of the users are not valid!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }
        }
        else
        {
            if (!IsIDValid(metroTextbox9.Text))
            {
                MessageBox.Show("The id of user is not valid!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }
        }

        int i = 0, j = 0;

        if (metroChecker35.Checked)
        {
            try
            {
                i = int.Parse(metroTextbox28.Text.Replace(" ", "").Replace('\t'.ToString(), ""));

                if (i < 0)
                {
                    i = 0;
                }
            }
            catch
            {

            }
        }

        foreach (string token in SplitToLines(metroTextbox1.Text))
        {
            if (blackListedTokens.Contains(token))
            {
                continue;
            }

            if (metroChecker38.Checked)
            {
                if (j == i)
                {
                    break;
                }

                j++;
            }

            if (metroChecker10.Checked)
            {
                Thread.Sleep(metroTrackbar4.Value);
            }

            if (metroChecker79.Checked)
            {
                foreach (string id in GetIDs(metroTextbox9.Text))
                {
                    new Thread(() => removeFriend(token, id)).Start();
                }
            }
            else
            {
                new Thread(() => removeFriend(token, metroTextbox9.Text)).Start();
            }
        }
    }

    public void removeFriend(string token, string id)
    {
        try
        {
            if (blackListedTokens.Contains(token))
            {
                return;
            }

            Leaf.xNet.HttpRequest httpRequest = Utils.CreateCleanRequest();
            string[] proxy = null;

            try
            {
                if (metroChecker13.Checked)
                {
                    proxy = Microsoft.VisualBasic.Strings.Split(GetRandomProxy(), ":");
                    httpRequest.Proxy = new Leaf.xNet.HttpProxyClient(proxy[0], int.Parse(proxy[1]));
                }
                else
                {
                    httpRequest.Proxy = null;
                }
            }
            catch
            {

            }

            httpRequest.AddHeader("Accept", "*/*");
            httpRequest.AddHeader("Accept-Encoding", "gzip, deflate, br");
            httpRequest.AddHeader("Accept-Language", getTokenLanguage(token));
            httpRequest.AddHeader("Authorization", token);
            httpRequest.AddHeader("Connection", "keep-alive");
            httpRequest.AddHeader("Cookie", Utils.GetRandomCookie(getTokenLanguage(token)));
            httpRequest.AddHeader("DNT", "1");
            httpRequest.AddHeader("Host", "discord.com");
            httpRequest.AddHeader("Origin", "https://discord.com");
            httpRequest.AddHeader("Referer", "https://discord.com/channels/@me");
            httpRequest.AddHeader("TE", "Trailers");
            httpRequest.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:88.0) Gecko/20100101 Firefox/88.0");
            httpRequest.AddHeader("X-Context-Properties", "eyJsb2NhdGlvbiI6IkZyaWVuZHMifQ==");
            httpRequest.AddHeader("X-Super-Properties", "eyJvcyI6IldpbmRvd3MiLCJicm93c2VyIjoiRmlyZWZveCIsImRldmljZSI6IiIsImJyb3dzZXJfdXNlcl9hZ2VudCI6Ik1vemlsbGEvNS4wIChXaW5kb3dzIE5UIDEwLjA7IFdpbjY0OyB4NjQ7IHJ2Ojg3LjApIEdlY2tvLzIwMTAwMTAxIEZpcmVmb3gvODcuMCIsImJyb3dzZXJfdmVyc2lvbiI6Ijg3LjAiLCJvc192ZXJzaW9uIjoiMTAiLCJyZWZlcnJlciI6IiIsInJlZmVycmluZ19kb21haW4iOiIiLCJyZWZlcnJlcl9jdXJyZW50IjoiIiwicmVmZXJyaW5nX2RvbWFpbl9jdXJyZW50IjoiIiwicmVsZWFzZV9jaGFubmVsIjoic3RhYmxlIiwiY2xpZW50X2J1aWxkX251bWJlciI6ODE0NTIsImNsaWVudF9ldmVudF9zb3VyY2UiOm51bGx9");

            httpRequest.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;

            string response = Utils.DecompressResponse(httpRequest.Delete("https://discord.com/api/v9/users/@me/relationships/" + id));
            addLiveLog(proxy == null ? "Token (" + token + ") remove from friends the user ID " + id + " -> " + response : "Token (" + token + ") with proxy (" + proxy[0] + ":" + proxy[1] + " removed from friends the user ID " + id + " -> " + response);

            if (response.ToLower().Contains("verify your account") || response.ToLower().Contains("you are being blocked from accessing our"))
            {
                blackListedTokens.Add(token);
                addLiveLog("Token (" + token + ") has been blacklisted because it is unverified or blocked by Discord -> " + response);
            }
        }
        catch
        {

        }

        if (metroChecker80.Checked && metroButton9.Enabled)
        {
            if (metroChecker10.Checked)
            {
                Thread.Sleep(metroTrackbar8.Value);
            }

            addFriend(token, id);
        }
    }

    public IEnumerable<string> SplitToLines(string input)
    {
        if (input == null)
        {
            yield break;
        }

        using (System.IO.StringReader reader = new System.IO.StringReader(input))
        {
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                yield return line;
            }
        }
    }

    private void metroButton18_Click(object sender, EventArgs e)
    {
        try
        {
            if (!IsIDValid(metroTextbox17.Text))
            {
                MessageBox.Show("The ID of the guild is not valid!", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!IsIDValid(metroTextbox41.Text))
            {
                MessageBox.Show("The ID of the channel is not valid! Please, specify a channel ID on the Settings section where you can see the most part of the members of the guild.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (metroButton18.Text == "Start downloading users")
            {
                metroButton18.Text = "Stop downloading users";
                downloadingUsers = true;
                users.Clear();
                metroLabel13.Text = users.Count.ToString() + " users have been downloaded.";
                new Thread(downloadAllUsers).Start();
            }
            else if (metroButton18.Text == "Stop downloading users")
            {
                downloadingUsers = false;
                metroButton18.Text = "Start downloading users";
                metroLabel13.Text = users.Count.ToString() + " users have been downloaded.";
            }
        }
        catch
        {
            MessageBox.Show("Failed to download all users!", "AstarothSpammer", MessageBoxButtons.OK, MessageBoxIcon.Error);

            metroLabel13.Text = "0 users have been downloaded.";
            users.Clear();
        }
    }

    public void downloadAllUsers()
    {
        try
        {
            metroLabel13.Text = "0 users have been downloaded.";
            users.Clear();
            string token = GetRandomValidToken();
            startDownloading(token);
        }
        catch
        {
            MessageBox.Show("Failed to download all users!", "AstarothSpammer", MessageBoxButtons.OK, MessageBoxIcon.Error);

            metroLabel13.Text = "0 users have been downloaded.";
            users.Clear();
        }
    }

    public void startDownloading(string token)
    {
        try
        {
            users.Clear();
            metroLabel13.Text = users.Count.ToString() + " users have been downloaded.";

            ws = new WebSocket("wss://gateway.discord.gg/?encoding=json&v=9");

            ws.CustomHeaders = new Dictionary<string, string>
            {
            { "Accept", "*/*" },
            { "Accept-Encoding", "gzip, deflate, br" },
            { "Accept-Language", "it-IT,it;q=0.8,en-US;q=0.5,en;q=0.3" },
            { "Cache-Control", "no-cache" },
            { "Connection", "keep-alive, Upgrade" },
            { "DNT", "1" },
            { "Host", "gateway.discord.gg" },
            { "Origin", "https://discord.com" },
            { "Pragma", "no-cache" },
            { "Sec-WebSocket-Extensions", "permessage-deflate" },
            { "Sec-WebSocket-Version", "13" },
            { "Upgrade", "websocket" },
            { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:88.0) Gecko/20100101 Firefox/88.0" }
            };

            new Thread(fetchQueue).Start();
            ws.OnMessage += Ws_OnMessage;
            ws.Connect();
            ws.Send("{\"op\":2,\"d\":{\"token\":\"" + token + "\",\"capabilities\":61,\"properties\":{\"os\":\"Windows\",\"browser\":\"Firefox\",\"device\":\"\",\"system_locale\":\"it-IT\",\"browser_user_agent\":\"Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:88.0) Gecko/20100101 Firefox/88.0\",\"browser_version\":\"88.0\",\"os_version\":\"10\",\"referrer\":\"\",\"referring_domain\":\"\",\"referrer_current\":\"https://discord.com/\",\"referring_domain_current\":\"discord.com\",\"release_channel\":\"stable\",\"client_build_number\":84632,\"client_event_source\":null},\"presence\":{\"status\":\"online\",\"since\":0,\"activities\":[],\"afk\":false},\"compress\":false,\"client_state\":{\"guild_hashes\":{},\"highest_last_message_id\":\"0\",\"read_state_version\":0,\"user_guild_settings_version\":-1}}}");
            ws.Send("{\"op\":14,\"d\":{\"guild_id\":\"" + metroTextbox17.Text + "\",\"typing\":true,\"activities\":true,\"threads\":true,\"channels\":{\"" + metroTextbox41.Text + "\":[[0,99]]}}}");
        }
        catch
        {
            MessageBox.Show("Failed to download all users!", "AstarothSpammer", MessageBoxButtons.OK, MessageBoxIcon.Error);

            metroLabel13.Text = "0 users have been downloaded.";
            users.Clear();
        }
    }

    private void Ws_OnMessage(object sender, WebSocketSharp.MessageEventArgs e)
    {
        try
        {
            string data = System.Text.Encoding.UTF8.GetString(e.RawData);
            dynamic jss = JObject.Parse(data);

            if (jss.op == 10)
            {
                int heartbeat_interval = jss.d.heartbeat_interval;
                new Thread(() => doHeartbeat(heartbeat_interval)).Start();
            }

            if (jss.t == "GUILD_MEMBER_LIST_UPDATE")
            {
                idQueue.Add(data);
            }
        }
        catch
        {

        }
    }

    public void doHeartbeat(int heartbeat)
    {
        try
        {
            while (downloadingUsers)
            {
                try
                {
                    Thread.Sleep(heartbeat);
                    ws.Send("{\"op\":1,\"d\":null}");
                }
                catch
                {

                }
            }
        }
        catch
        {

        }
    }

    public void fetchQueue()
    {
        while (downloadingUsers)
        {
            Thread.Sleep(250);

            try
            {
                string data = idQueue[0];
                idQueue.RemoveAt(0);

                string[] splitted = Strings.Split(data, "\"id\":\"");

                for (int i = 1; i < splitted.Length; i++)
                {
                    try
                    {
                        string another = splitted[i];

                        string[] anotherSplit = Strings.Split(another, "\"");
                        string finalId = anotherSplit[0];

                        if (Information.IsNumeric(finalId) && finalId.Length == 18)
                        {
                            ids.Add(finalId);
                        }
                    }
                    catch
                    {

                    }
                }

                for (int i = 0; i < ids.Count; i++)
                {
                    try
                    {
                        for (int j = 0; j < ids.Count; j++)
                        {
                            try
                            {
                                if (i != j)
                                {
                                    if (ids[i] == ids[j])
                                    {
                                        ids.RemoveAt(i);
                                    }
                                }
                            }
                            catch
                            {

                            }
                        }
                    }
                    catch
                    {

                    }
                }

                foreach (string id in ids)
                {
                    users.Add(id);
                }

                for (int i = 0; i < users.Count; i++)
                {
                    try
                    {
                        for (int j = 0; j < users.Count; j++)
                        {
                            try
                            {
                                if (i != j)
                                {
                                    if (users[i] == users[j])
                                    {
                                        users.RemoveAt(j);
                                    }
                                }
                            }
                            catch
                            {

                            }
                        }
                    }
                    catch
                    {

                    }
                }

                metroLabel13.Text = users.Count.ToString() + " users have been downloaded.";
            }
            catch
            {

            }
        }
    }

    public string GetRandomValidToken()
    {
        string token = "";

        try
        {
            List<string> tokens = new List<string>();

            foreach (string theToken in SplitToLines(metroTextbox1.Text))
            {
                if (blackListedTokens.Contains(theToken))
                {
                    continue;
                }

                try
                {
                    tokens.Add(theToken);
                }
                catch
                {

                }
            }

            while (token == "")
            {
                string temp = tokens[Utils.GetRandomNumber1(tokens.Count)];

                if (blackListedTokens.Contains(temp))
                {
                    continue;
                }

                token = temp;
            }
        }
        catch
        {

        }

        return token;
    }

    private void metroButton19_Click(object sender, EventArgs e)
    {
        if (saveFileDialog1.ShowDialog().Equals(DialogResult.OK))
        {
            string allUsers = "";

            foreach (string user in users)
            {
                if (allUsers == "")
                {
                    allUsers = user;
                }
                else
                {
                    allUsers += "\r\n" + user;
                }
            }

            System.IO.File.WriteAllText(saveFileDialog1.FileName, allUsers);
        }
    }

    private void metroButton20_Click(object sender, EventArgs e)
    {
        openFileDialog1.Title = "Open your downloaded users list...";

        if (openFileDialog1.ShowDialog().Equals(DialogResult.OK))
        {
            users.Clear();

            foreach (string user in SplitToLines(System.IO.File.ReadAllText(openFileDialog1.FileName)))
            {
                try
                {
                    if (user.Trim().Replace(" ", "").Replace('\t'.ToString(), "") != "" && user.Trim().Replace(" ", "").Replace('\t'.ToString(), "").Length == 18)
                    {
                        if (Microsoft.VisualBasic.Information.IsNumeric(user.Trim().Replace(" ", "").Replace('\t'.ToString(), "")))
                        {
                            users.Add(user.Trim().Replace(" ", "").Replace('\t'.ToString(), ""));
                        }
                    }
                }
                catch
                {

                }
            }

            metroLabel13.Text = users.Count.ToString() + " users have been downloaded.";
        }
    }

    private void metroButton21_Click(object sender, EventArgs e)
    {
        new Thread(doTokenChecker).Start();
    }

    public void doTokenChecker()
    {
        metroLabel15.Text = "Removing dead tokens...";
        string newLines = "";

        foreach (string line in SplitToLines(metroTextbox1.Text))
        {
            if (CheckToken(line))
            {
                if (newLines == "")
                {
                    newLines = line.Trim().Replace(" ", "").Replace('\t'.ToString(), "").Replace('\n'.ToString(), "").Replace('\r'.ToString(), "").Replace(Environment.NewLine, "");
                }
                else
                {
                    newLines += "\r\n" + line.Trim().Replace(" ", "").Replace('\t'.ToString(), "").Replace('\n'.ToString(), "").Replace('\r'.ToString(), "").Replace(Environment.NewLine, "");
                }
            }
        }

        metroTextbox1.Text = newLines;
        metroLabel15.Text = "Succesfully removed dead tokens.";
    }

    public static bool CheckToken(string token)
    {
        try
        {
            token = token.Trim().Replace(" ", "").Replace('\t'.ToString(), "").Replace('\n'.ToString(), "").Replace('\r'.ToString(), "").Replace(Environment.NewLine, "");

            if (token.Length != 88 && token.Length != 59)
            {
                return false;
            }

            var http = new HttpClient();

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri("https://discord.com/api/v9/users/@me/library"),
                Method = HttpMethod.Get,
                Headers = { { HttpRequestHeader.Authorization.ToString(), token }, { HttpRequestHeader.ContentType.ToString(), "multipart/mixed" }, },
            };

            if (http.SendAsync(request).Result.StatusCode.Equals(HttpStatusCode.OK))
            {
                return true;
            }

            return false;
        }
        catch
        {

        }

        return false;
    }

    public void switchToLightMode()
    {
        this.Style = MetroSuite.Design.Style.Light;

        foreach (TabPage tabPage in firefoxMainTabControl1.TabPages)
        {
            tabPage.BackColor = System.Drawing.Color.White;
        }
    }

    public void switchToDarkMode()
    {
        this.Style = MetroSuite.Design.Style.Dark;

        foreach (TabPage tabPage in firefoxMainTabControl1.TabPages)
        {
            tabPage.BackColor = System.Drawing.Color.FromArgb(40, 40, 40);
        }
    }

    private void metroTrackbar5_Scroll(object sender, MetroSuite.MetroTrackbar.TrackbarEventArgs e)
    {
        metroChecker22.Text = "Delay: " + metroTrackbar5.Value + "ms";
    }

    private void metroChecker35_CheckedChanged(object sender, bool isChecked)
    {
        metroTextbox25.Enabled = metroChecker35.Checked;
    }

    private void metroChecker36_CheckedChanged(object sender, bool isChecked)
    {
        metroTextbox26.Enabled = metroChecker36.Checked;
    }

    private void metroChecker42_CheckedChanged(object sender, bool isChecked)
    {
        metroTextbox31.Enabled = metroChecker42.Checked;
    }

    private void metroChecker37_CheckedChanged(object sender, bool isChecked)
    {
        metroTextbox27.Enabled = metroChecker37.Checked;
    }

    private void metroChecker38_CheckedChanged(object sender, bool isChecked)
    {
        metroTextbox28.Enabled = metroChecker38.Checked;
    }

    private void metroChecker39_CheckedChanged(object sender, bool isChecked)
    {
        metroTextbox29.Enabled = metroChecker39.Checked;
    }

    private void metroChecker40_CheckedChanged(object sender, bool isChecked)
    {
        metroTextbox30.Enabled = metroChecker40.Checked;
    }

    private void metroButton17_Click(object sender, EventArgs e)
    {
        new Thread(doVoiceJoiner).Start();
    }

    public void doVoiceJoiner()
    {
        if (!IsIDValid(metroTextbox20.Text))
        {
            MessageBox.Show("The id of channel is not valid!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);

            return;
        }

        if (!IsIDValid(metroTextbox21.Text))
        {
            MessageBox.Show("The id of guild is not valid!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);

            return;
        }

        int i = 0, j = 0;

        if (metroChecker40.Checked)
        {
            try
            {
                i = int.Parse(metroTextbox30.Text.Replace(" ", "").Replace('\t'.ToString(), ""));

                if (i < 0)
                {
                    i = 0;
                }
            }
            catch
            {

            }
        }

        voiceSessionId = Utils.RandomNormalString(32);

        foreach (string token in SplitToLines(metroTextbox1.Text))
        {
            if (blackListedTokens.Contains(token))
            {
                return;
            }

            if (metroChecker40.Checked)
            {
                if (j == i)
                {
                    break;
                }

                j++;
            }

            if (metroChecker22.Checked)
            {
                Thread.Sleep(metroTrackbar5.Value);
            }

            new Thread(() => joinVoice(token)).Start();
        }
    }

    public void joinVoice(string token)
    {
        try
        {
            if (blackListedTokens.Contains(token))
            {
                return;
            }

            bool exist = false;

            foreach (Tuple<string, DiscordSocketClient> tuple in sessions)
            {
                try
                {
                    if (tuple.Item1 == token)
                    {
                        exist = true;

                        break;
                    }
                }
                catch
                {

                }
            }

            if (!exist)
            {
                if (blackListedTokens.Contains(token))
                {
                    return;
                }

                DiscordSocketConfig config = new DiscordSocketConfig();

                if (metroChecker13.Checked)
                {
                    config.Proxy = AnarchyProxy.Parse(AnarchyProxyType.HTTP, GetRandomProxy());
                }

                config.ApiVersion = 9;
                DiscordSocketClient client = new DiscordSocketClient(config);
                client.OnLeftGuild += Client_OnLeftGuild;
                client.Login(token);
                Thread.Sleep(500);
                sessions.Add(new Tuple<string, DiscordSocketClient>(token, client));

                try
                {
                    if (metroChecker24.Checked)
                    {
                        new Thread(() => autoLeave(client)).Start();
                    }
                }
                catch
                {

                }

                try
                {
                    new Thread(() => screenshareThings(client)).Start();
                    DiscordVoiceSession session = client.JoinVoiceChannel(new VoiceStateProperties() { ChannelId = ulong.Parse(metroTextbox21.Text), GuildId = ulong.Parse(metroTextbox20.Text), Muted = metroChecker44.Checked, Deafened = metroChecker45.Checked, Video = metroChecker46.Checked });
                    session.ReceivePackets = false;
                    session.OnConnected += Session_OnConnected;
                    session.OnDisconnected += Session_OnDisconnected;
                    session.Connect();
                }
                catch
                {

                }
            }
            else
            {
                foreach (Tuple<string, DiscordSocketClient> tuple in sessions)
                {
                    try
                    {
                        if (tuple.Item1 == token)
                        {
                            try
                            {
                                if (metroChecker24.Checked)
                                {
                                    new Thread(() => autoLeave(tuple.Item2)).Start();
                                }
                            }
                            catch
                            {

                            }

                            new Thread(() => screenshareThings(tuple.Item2)).Start();
                            DiscordVoiceSession session = tuple.Item2.JoinVoiceChannel(new VoiceStateProperties() { ChannelId = ulong.Parse(metroTextbox21.Text), GuildId = ulong.Parse(metroTextbox20.Text), Muted = metroChecker44.Checked, Deafened = metroChecker45.Checked, Video = metroChecker46.Checked });
                            session.ReceivePackets = false;
                            session.OnConnected += Session_OnConnected;
                            session.OnDisconnected += Session_OnDisconnected;
                            session.Connect();

                            break;
                        }
                    }
                    catch
                    {

                    }
                }
            }
        }
        catch
        {

        }
    }

    public void screenshareThings(DiscordSocketClient client)
    {
        try
        {
            Thread.Sleep(1750);

            try
            {
                if (metroChecker57.Checked)
                {
                    var http = new HttpClient();
                    string timestamp = "";

                    string year = "", month = "", day = "", hour = "", minute = "", second = "";

                    year = DateTime.Now.Year.ToString();
                    month = DateTime.Now.Month.ToString();

                    if (month.Length == 1)
                    {
                        month = "0" + month;
                    }

                    day = DateTime.Now.Day.ToString();

                    if (day.Length == 1)
                    {
                        day = "0" + day;
                    }

                    hour = DateTime.Now.Hour.ToString();

                    if (hour.Length == 1)
                    {
                        hour = "0" + hour;
                    }

                    minute = DateTime.Now.Minute.ToString();

                    if (minute.Length == 1)
                    {
                        minute = "0" + minute;
                    }

                    second = DateTime.Now.Minute.ToString();

                    if (second.Length == 1)
                    {
                        second = "0" + second;
                    }

                    timestamp = year + "-" + month + "-" + day + "T" + hour + ":" + minute + ":" + second + "." + DateTime.Now.Millisecond.ToString() + "Z";

                    string messageJson = "{\"request_to_speak_timestamp\":\"" + timestamp + "\",\"channel_id\":\"" + metroTextbox21.Text + "\"}";

                    Leaf.xNet.HttpRequest httpRequest = Utils.CreateCleanRequest();
                    string[] proxy = null;

                    try
                    {
                        if (metroChecker13.Checked)
                        {
                            proxy = Microsoft.VisualBasic.Strings.Split(GetRandomProxy(), ":");
                            httpRequest.Proxy = new Leaf.xNet.HttpProxyClient(proxy[0], int.Parse(proxy[1]));
                        }
                        else
                        {
                            httpRequest.Proxy = null;
                        }
                    }
                    catch
                    {

                    }

                    httpRequest.AddHeader("Accept", "*/*");
                    httpRequest.AddHeader("Accept-Encoding", "gzip, deflate, br");
                    httpRequest.AddHeader("Accept-Language", getTokenLanguage(client.Token));
                    httpRequest.AddHeader("Authorization", client.Token);
                    httpRequest.AddHeader("Connection", "keep-alive");
                    httpRequest.AddHeader("Content-Length", messageJson.Length.ToString());
                    httpRequest.AddHeader("Content-Type", "application/json");
                    httpRequest.AddHeader("Cookie", Utils.GetRandomCookie(getTokenLanguage(client.Token)));
                    httpRequest.AddHeader("DNT", "1");
                    httpRequest.AddHeader("Host", "discord.com");
                    httpRequest.AddHeader("Origin", "https://discord.com");
                    httpRequest.AddHeader("Referer", "https://discord.com/channels/" + metroTextbox20.Text + "/" + metroTextbox21.Text);
                    httpRequest.AddHeader("TE", "Trailers");
                    httpRequest.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:88.0) Gecko/20100101 Firefox/88.0");
                    httpRequest.AddHeader("X-Super-Properties", "eyJvcyI6IldpbmRvd3MiLCJicm93c2VyIjoiRmlyZWZveCIsImRldmljZSI6IiIsImJyb3dzZXJfdXNlcl9hZ2VudCI6Ik1vemlsbGEvNS4wIChXaW5kb3dzIE5UIDEwLjA7IFdpbjY0OyB4NjQ7IHJ2Ojg3LjApIEdlY2tvLzIwMTAwMTAxIEZpcmVmb3gvODcuMCIsImJyb3dzZXJfdmVyc2lvbiI6Ijg3LjAiLCJvc192ZXJzaW9uIjoiMTAiLCJyZWZlcnJlciI6IiIsInJlZmVycmluZ19kb21haW4iOiIiLCJyZWZlcnJlcl9jdXJyZW50IjoiIiwicmVmZXJyaW5nX2RvbWFpbl9jdXJyZW50IjoiIiwicmVsZWFzZV9jaGFubmVsIjoic3RhYmxlIiwiY2xpZW50X2J1aWxkX251bWJlciI6ODE0NTIsImNsaWVudF9ldmVudF9zb3VyY2UiOm51bGx9");

                    httpRequest.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                    httpRequest.Patch("https://discord.com/api/v9/guilds/" + metroTextbox20.Text + "/voice-states/@me", messageJson, "application/json");
                }
            }
            catch
            {

            }

            if (metroChecker47.Checked)
            {
                client.newGoLive(ulong.Parse(metroTextbox20.Text), ulong.Parse(metroTextbox21.Text), client.User.Id);
            }

            if (metroChecker51.Checked)
            {
                client.newWatchGoLive(ulong.Parse(metroTextbox20.Text), ulong.Parse(metroTextbox21.Text), ulong.Parse(metroTextbox33.Text));
            }
        }
        catch
        {

        }
    }

    private void Session_OnDisconnected(DiscordVoiceSession session, DiscordMediaCloseEventArgs args)
    {
        new Thread(() => processDisconnected(session)).Start();
    }

    public void processDisconnected(DiscordVoiceSession session)
    {
        try
        {
            if (metroChecker54.Checked)
            {
                DiscordVoiceSession newSession = session.Client.JoinVoiceChannel(new VoiceStateProperties() { ChannelId = session.Channel.Id, GuildId = session.Guild.Id, Muted = metroChecker44.Checked, Deafened = metroChecker45.Checked, Video = metroChecker46.Checked });
                session.ReceivePackets = false;
                session.OnConnected += Session_OnConnected;
                session.OnDisconnected += Session_OnDisconnected;
                session.Connect();
            }
        }
        catch
        {

        }
    }

    private void Session_OnConnected(DiscordVoiceSession session, EventArgs e)
    {
        new Thread(() => processConnected(session)).Start();
    }

    public void processConnected(DiscordVoiceSession session)
    {
        try
        {
            string actualVoiceSessionId = voiceSessionId;

            try
            {
                if (metroChecker57.Checked)
                {
                    var http = new HttpClient();
                    string timestamp = "";

                    string year = "", month = "", day = "", hour = "", minute = "", second = "";

                    year = DateTime.Now.Year.ToString();
                    month = DateTime.Now.Month.ToString();

                    if (month.Length == 1)
                    {
                        month = "0" + month;
                    }

                    day = DateTime.Now.Day.ToString();

                    if (day.Length == 1)
                    {
                        day = "0" + day;
                    }

                    hour = DateTime.Now.Hour.ToString();

                    if (hour.Length == 1)
                    {
                        hour = "0" + hour;
                    }

                    minute = DateTime.Now.Minute.ToString();

                    if (minute.Length == 1)
                    {
                        minute = "0" + minute;
                    }

                    second = DateTime.Now.Minute.ToString();

                    if (second.Length == 1)
                    {
                        second = "0" + second;
                    }

                    timestamp = year + "-" + month + "-" + day + "T" + hour + ":" + minute + ":" + second + "." + DateTime.Now.Millisecond.ToString() + "Z";

                    string messageJson = "{\"request_to_speak_timestamp\":\"" + timestamp + "\",\"channel_id\":\"" + metroTextbox21.Text + "\"}";

                    Leaf.xNet.HttpRequest httpRequest = Utils.CreateCleanRequest();
                    string[] proxy = null;

                    try
                    {
                        if (metroChecker13.Checked)
                        {
                            proxy = Microsoft.VisualBasic.Strings.Split(GetRandomProxy(), ":");
                            httpRequest.Proxy = new Leaf.xNet.HttpProxyClient(proxy[0], int.Parse(proxy[1]));
                        }
                        else
                        {
                            httpRequest.Proxy = null;
                        }
                    }
                    catch
                    {

                    }

                    httpRequest.AddHeader("Accept", "*/*");
                    httpRequest.AddHeader("Accept-Encoding", "gzip, deflate, br");
                    httpRequest.AddHeader("Accept-Language", getTokenLanguage(session.Client.Token));
                    httpRequest.AddHeader("Authorization", session.Client.Token);
                    httpRequest.AddHeader("Connection", "keep-alive");
                    httpRequest.AddHeader("Content-Length", messageJson.Length.ToString());
                    httpRequest.AddHeader("Content-Type", "application/json");
                    httpRequest.AddHeader("Cookie", Utils.GetRandomCookie(getTokenLanguage(session.Client.Token)));
                    httpRequest.AddHeader("DNT", "1");
                    httpRequest.AddHeader("Host", "discord.com");
                    httpRequest.AddHeader("Origin", "https://discord.com");
                    httpRequest.AddHeader("Referer", "https://discord.com/channels/" + session.Guild.Id.ToString() + "/" + session.Channel.Id.ToString());
                    httpRequest.AddHeader("TE", "Trailers");
                    httpRequest.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:88.0) Gecko/20100101 Firefox/88.0");
                    httpRequest.AddHeader("X-Super-Properties", "eyJvcyI6IldpbmRvd3MiLCJicm93c2VyIjoiRmlyZWZveCIsImRldmljZSI6IiIsImJyb3dzZXJfdXNlcl9hZ2VudCI6Ik1vemlsbGEvNS4wIChXaW5kb3dzIE5UIDEwLjA7IFdpbjY0OyB4NjQ7IHJ2Ojg3LjApIEdlY2tvLzIwMTAwMTAxIEZpcmVmb3gvODcuMCIsImJyb3dzZXJfdmVyc2lvbiI6Ijg3LjAiLCJvc192ZXJzaW9uIjoiMTAiLCJyZWZlcnJlciI6IiIsInJlZmVycmluZ19kb21haW4iOiIiLCJyZWZlcnJlcl9jdXJyZW50IjoiIiwicmVmZXJyaW5nX2RvbWFpbl9jdXJyZW50IjoiIiwicmVsZWFzZV9jaGFubmVsIjoic3RhYmxlIiwiY2xpZW50X2J1aWxkX251bWJlciI6ODE0NTIsImNsaWVudF9ldmVudF9zb3VyY2UiOm51bGx9");

                    httpRequest.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                    httpRequest.Patch("https://discord.com/api/v9/guilds/" + metroTextbox20.Text + "/voice-states/@me", messageJson, "application/json");
                }
            }
            catch
            {

            }

            try
            {
                if (metroChecker47.Checked)
                {
                    session.Client.newGoLive(ulong.Parse(metroTextbox20.Text), ulong.Parse(metroTextbox21.Text), session.Client.User.Id);
                }
            }
            catch
            {

            }

            try
            {
                if (metroChecker51.Checked)
                {
                    session.Client.newWatchGoLive(ulong.Parse(metroTextbox20.Text), ulong.Parse(metroTextbox21.Text), ulong.Parse(metroTextbox33.Text));
                }
            }
            catch
            {

            }

            try
            {
                if (metroChecker56.Checked)
                {
                    if (System.IO.File.Exists(metroTextbox34.Text))
                    {
                        if (System.IO.Path.GetExtension(metroTextbox34.Text).ToLower().Equals(".mp3"))
                        {
                            DiscordVoiceStream stream = session.CreateStream(96000U, AudioApplication.Mixed);
                            session.SetSpeakingState(DiscordSpeakingFlags.Soundshare);

                            if (!metroSwitch1.Checked)
                            {
                                for (; ; )
                                {
                                    try
                                    {
                                        if (actualVoiceSessionId == voiceSessionId)
                                        {
                                            stream.CopyFrom(DiscordVoiceUtils.GetAudioStream(metroTextbox34.Text));
                                        }
                                        else
                                        {
                                            return;
                                        }
                                    }
                                    catch
                                    {

                                    }
                                }
                            }
                            else
                            {
                                int times = 1;

                                try
                                {
                                    times = int.Parse(metroTextbox35.Text);
                                }
                                catch
                                {

                                }

                                for (int i = 0; i < times; i++)
                                {
                                    try
                                    {
                                        if (actualVoiceSessionId == voiceSessionId)
                                        {
                                            stream.CopyFrom(DiscordVoiceUtils.GetAudioStream(metroTextbox34.Text));
                                        }
                                        else
                                        {
                                            return;
                                        }
                                    }
                                    catch
                                    {

                                    }
                                }

                                session.SetSpeakingState(DiscordSpeakingFlags.NotSpeaking);
                            }
                        }
                    }
                }
            }
            catch
            {

            }
        }
        catch
        {

        }
    }

    public void autoLeave(DiscordSocketClient client)
    {
        try
        {
            Thread.Sleep(10 + metroTrackbar8.Value);
            client.ChangeVoiceState(new VoiceStateProperties());
        }
        catch
        {

        }
    }

    private void metroButton23_Click(object sender, EventArgs e)
    {
        try
        {
            voiceSessionId = Utils.RandomNormalString(32);
            new Thread(doVoiceLeaver).Start();
        }
        catch
        {

        }
    }

    public void doVoiceLeaver()
    {
        if (!IsIDValid(metroTextbox20.Text))
        {
            MessageBox.Show("The id of channel is not valid!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);

            return;
        }

        if (!IsIDValid(metroTextbox21.Text))
        {
            MessageBox.Show("The id of guild is not valid!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);

            return;
        }

        int i = 0, j = 0;

        if (metroChecker40.Checked)
        {
            try
            {
                i = int.Parse(metroTextbox30.Text.Replace(" ", "").Replace('\t'.ToString(), ""));

                if (i < 0)
                {
                    i = 0;
                }
            }
            catch
            {

            }
        }

        foreach (string token in SplitToLines(metroTextbox1.Text))
        {
            if (blackListedTokens.Contains(token))
            {
                continue;
            }

            if (metroChecker40.Checked)
            {
                if (j == i)
                {
                    break;
                }

                j++;
            }

            if (metroChecker22.Checked)
            {
                Thread.Sleep(metroTrackbar5.Value);
            }

            new Thread(() => leaveVoice(token)).Start();
        }
    }

    public void leaveVoice(string token)
    {
        try
        {
            if (blackListedTokens.Contains(token))
            {
                return;
            }

            bool exist = false;

            foreach (Tuple<string, DiscordSocketClient> tuple in sessions)
            {
                try
                {
                    if (tuple.Item1 == token)
                    {
                        exist = true;

                        break;
                    }
                }
                catch
                {

                }
            }

            if (!exist)
            {
                if (blackListedTokens.Contains(token))
                {
                    return;
                }

                DiscordSocketConfig config = new DiscordSocketConfig();

                if (metroChecker13.Checked)
                {
                    config.Proxy = AnarchyProxy.Parse(AnarchyProxyType.HTTP, GetRandomProxy());
                }

                config.ApiVersion = 9;
                DiscordSocketClient client = new DiscordSocketClient(config);
                client.OnLeftGuild += Client_OnLeftGuild;
                client.Login(token);
                Thread.Sleep(500);
                sessions.Add(new Tuple<string, DiscordSocketClient>(token, client));

                client.ChangeVoiceState(new VoiceStateProperties());
            }
            else
            {
                foreach (Tuple<string, DiscordSocketClient> tuple in sessions)
                {
                    try
                    {
                        if (tuple.Item1 == token)
                        {
                            tuple.Item2.ChangeVoiceState(new VoiceStateProperties());

                            break;
                        }
                    }
                    catch

                    {

                    }
                }
            }
        }
        catch
        {

        }
    }

    private void metroTrackbar8_Scroll(object sender, MetroSuite.MetroTrackbar.TrackbarEventArgs e)
    {
        metroChecker24.Text = "Auto leave from voice channel: " + metroTrackbar8.Value + "ms";
    }

    private void metroTrackbar9_Scroll(object sender, MetroSuite.MetroTrackbar.TrackbarEventArgs e)
    {
        metroChecker26.Text = "Delay: " + metroTrackbar9.Value + "ms";
    }

    private void metroTrackbar10_Scroll(object sender, MetroSuite.MetroTrackbar.TrackbarEventArgs e)
    {
        metroChecker27.Text = "Auto end call: " + metroTrackbar10.Value + "ms";
    }

    private void metroButton25_Click(object sender, EventArgs e)
    {
        new Thread(doCallStarter).Start();
    }

    private void metroButton24_Click(object sender, EventArgs e)
    {
        new Thread(doCallEnder).Start();
    }

    public static string GetInviteCodeByInviteLink(string inviteLink)
    {
        try
        {
            if (inviteLink.EndsWith("/"))
            {
                inviteLink = inviteLink.Substring(0, inviteLink.Length - 1);
            }

            if (inviteLink.Contains("discord") && inviteLink.Contains("/") && inviteLink.Contains("http"))
            {
                string[] splitter = Microsoft.VisualBasic.Strings.Split(inviteLink, "/");

                return splitter[splitter.Length - 1];
            }
        }
        catch
        {

        }

        return inviteLink;
    }

    private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        Process.Start(linkLabel1.Text);
    }

    public string GetRandomProxy()
    {
        try
        {
            List<string> lines = new List<string>();

            foreach (string line in SplitToLines(metroTextbox2.Text))
            {
                lines.Add(line);
            }

            if (lines.Count > 1)
            {
                return lines[Utils.GetRandomNumber1(lines.Count)].Trim().Replace(" ", "").Replace('\t'.ToString(), "");
            }
            else
            {
                return metroTextbox2.Text.Trim().Replace(" ", "").Replace('\t'.ToString(), "");
            }
        }
        catch
        {

        }

        return "";
    }

    public void doCallStarter()
    {
        if (!IsIDValid(metroTextbox19.Text))
        {
            MessageBox.Show("The id of user is not valid!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);

            return;
        }

        int i = 0, j = 0;

        if (metroChecker28.Checked)
        {
            try
            {
                i = int.Parse(metroTextbox22.Text.Replace(" ", "").Replace('\t'.ToString(), ""));

                if (i < 0)
                {
                    i = 0;
                }
            }
            catch
            {

            }
        }

        foreach (string token in SplitToLines(metroTextbox1.Text))
        {
            if (blackListedTokens.Contains(token))
            {
                return;
            }

            if (metroChecker28.Checked)
            {
                if (j == i)
                {
                    break;
                }

                j++;
            }

            if (metroChecker26.Checked)
            {
                Thread.Sleep(metroTrackbar9.Value);
            }

            new Thread(() => startCall(token)).Start();
        }
    }

    public void startCall(string token)
    {
        try
        {
            if (blackListedTokens.Contains(token))
            {
                return;
            }

            bool exist = false;

            foreach (Tuple<string, DiscordSocketClient> tuple in sessions)
            {
                try
                {
                    if (tuple.Item1 == token)
                    {
                        exist = true;

                        break;
                    }
                }
                catch
                {

                }
            }

            if (!exist)
            {
                if (blackListedTokens.Contains(token))
                {
                    return;
                }

                DiscordSocketConfig config = new DiscordSocketConfig();

                if (metroChecker13.Checked)
                {
                    config.Proxy = AnarchyProxy.Parse(AnarchyProxyType.HTTP, GetRandomProxy());
                }

                config.ApiVersion = 9;
                DiscordSocketClient client = new DiscordSocketClient(config);
                client.OnLeftGuild += Client_OnLeftGuild;
                client.Login(token);
                Thread.Sleep(500);
                sessions.Add(new Tuple<string, DiscordSocketClient>(token, client));

                try
                {
                    if (metroChecker27.Checked)
                    {
                        new Thread(() => autoEndCall(client)).Start();
                    }
                }
                catch
                {

                }

                try
                {
                    string channelId = createDM(token, metroTextbox19.Text).Result;
                    client.JoinVoiceChannel(new VoiceStateProperties() { ChannelId = ulong.Parse(channelId) });
                    //client.newStartCall(ulong.Parse(channelId));
                }
                catch
                {

                }
            }
            else
            {
                foreach (Tuple<string, DiscordSocketClient> tuple in sessions)
                {
                    try
                    {
                        if (tuple.Item1 == token)
                        {
                            try
                            {
                                if (metroChecker27.Checked)
                                {
                                    new Thread(() => autoEndCall(tuple.Item2)).Start();
                                }
                            }
                            catch
                            {

                            }

                            string channelId = createDM(token, metroTextbox19.Text).Result;
                            tuple.Item2.JoinVoiceChannel(new VoiceStateProperties() { ChannelId = ulong.Parse(channelId) });
                            //tuple.Item2.newStartCall(ulong.Parse(channelId));

                            break;
                        }
                    }
                    catch

                    {

                    }
                }
            }
        }
        catch
        {

        }
    }

    private void metroButton26_Click(object sender, EventArgs e)
    {
        if (!IsIDValid(metroTextbox17.Text))
        {
            MessageBox.Show("The ID of the guild is not valid!", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        roles.Clear();
        metroLabel20.Text = "0 roles have been downloaded.";
        new Thread(downloadAllRoles).Start();
    }

    public void downloadAllRoles()
    {
        try
        {
            DiscordSocketConfig config = new DiscordSocketConfig();

            if (metroChecker13.Checked)
            {
                config.Proxy = AnarchyProxy.Parse(AnarchyProxyType.HTTP, GetRandomProxy());
            }

            config.ApiVersion = 9;

            DiscordSocketClient client = new DiscordSocketClient(config);
            client.OnLoggedIn += Client_OnLoggedIn1;

            List<string> tokens = new List<string>();

            foreach (string token in SplitToLines(metroTextbox1.Text))
            {
                if (blackListedTokens.Contains(token))
                {
                    continue;
                }

                try
                {
                    tokens.Add(token);
                }
                catch
                {

                }
            }

            client.Login(tokens[Utils.GetRandomNumber1(tokens.Count)]);
        }
        catch
        {
            MessageBox.Show("Failed to download all roles!", "AstarothSpammer", MessageBoxButtons.OK, MessageBoxIcon.Error);

            metroLabel20.Text = "0 roles have been downloaded.";
            roles.Clear();
        }
    }

    private void metroChecker43_CheckedChanged(object sender, bool isChecked)
    {
        metroTextbox14.Enabled = !metroChecker43.Checked;
    }

    private void metroChecker51_CheckedChanged(object sender, bool isChecked)
    {
        metroTextbox33.Enabled = metroChecker51.Checked;
    }

    private void metroButton29_Click(object sender, EventArgs e)
    {
        new Thread(doNickNameSet).Start();
    }

    private void metroButton30_Click(object sender, EventArgs e)
    {
        new Thread(doGameSet).Start();
    }

    public void doNickNameSet()
    {
        foreach (string token in SplitToLines(metroTextbox1.Text))
        {
            if (blackListedTokens.Contains(token))
            {
                continue;
            }

            new Thread(() => setNickName(token)).Start();
        }
    }

    public void doGameSet()
    {
        foreach (string token in SplitToLines(metroTextbox1.Text))
        {
            if (blackListedTokens.Contains(token))
            {
                continue;
            }

            new Thread(() => setGame(token)).Start();
        }
    }

    public void setNickName(string token)
    {
        try
        {
            if (blackListedTokens.Contains(token))
            {
                return;
            }

            bool exist = false;

            foreach (Tuple<string, DiscordSocketClient> tuple in sessions)
            {
                try
                {
                    if (tuple.Item1 == token)
                    {
                        exist = true;

                        break;
                    }
                }
                catch
                {

                }
            }

            if (!exist)
            {
                if (blackListedTokens.Contains(token))
                {
                    return;
                }

                DiscordSocketConfig config = new DiscordSocketConfig();

                if (metroChecker13.Checked)
                {
                    config.Proxy = AnarchyProxy.Parse(AnarchyProxyType.HTTP, GetRandomProxy());
                }

                config.ApiVersion = 9;
                DiscordSocketClient client = new DiscordSocketClient(config);
                client.OnLeftGuild += Client_OnLeftGuild;
                client.Login(token);
                Thread.Sleep(500);
                sessions.Add(new Tuple<string, DiscordSocketClient>(token, client));

                client.SetClientNickname(ulong.Parse(metroTextbox17.Text), metroTextbox24.Text);
            }
            else
            {
                foreach (Tuple<string, DiscordSocketClient> tuple in sessions)
                {
                    try
                    {
                        if (tuple.Item1 == token)
                        {
                            tuple.Item2.SetClientNickname(ulong.Parse(metroTextbox17.Text), metroTextbox24.Text);

                            break;
                        }
                    }
                    catch
                    {

                    }
                }
            }
        }
        catch
        {

        }
    }

    public void setGame(string token)
    {
        try
        {
            if (blackListedTokens.Contains(token))
            {
                return;
            }

            bool exist = false;

            foreach (Tuple<string, DiscordSocketClient> tuple in sessions)
            {
                try
                {
                    if (tuple.Item1 == token)
                    {
                        exist = true;

                        break;
                    }
                }
                catch
                {

                }
            }

            if (!exist)
            {
                if (blackListedTokens.Contains(token))
                {
                    return;
                }

                DiscordSocketConfig config = new DiscordSocketConfig();

                if (metroChecker13.Checked)
                {
                    config.Proxy = AnarchyProxy.Parse(AnarchyProxyType.HTTP, GetRandomProxy());
                }

                config.ApiVersion = 9;
                DiscordSocketClient client = new DiscordSocketClient(config);
                client.OnLeftGuild += Client_OnLeftGuild;
                client.Login(token);
                Thread.Sleep(500);
                sessions.Add(new Tuple<string, DiscordSocketClient>(token, client));

                ActivityProperties activityProperties = new ActivityProperties();

                activityProperties.Type = ActivityType.Game;
                activityProperties.Name = metroTextbox24.Text;

                client.SetActivity(activityProperties);
            }
            else
            {
                foreach (Tuple<string, DiscordSocketClient> tuple in sessions)
                {
                    try
                    {
                        if (tuple.Item1 == token)
                        {
                            ActivityProperties activityProperties = new ActivityProperties();

                            activityProperties.Type = ActivityType.Game;
                            activityProperties.Name = metroTextbox24.Text;

                            tuple.Item2.SetActivity(activityProperties);

                            break;
                        }
                    }
                    catch
                    {

                    }
                }
            }
        }
        catch
        {

        }
    }

    private void metroButton31_Click(object sender, EventArgs e)
    {
        foreach (string token in SplitToLines(metroTextbox1.Text))
        {
            if (blackListedTokens.Contains(token))
            {
                continue;
            }

            new Thread(() => setOnlineStatus(token)).Start();
        }
    }

    public void setOnlineStatus(string token)
    {
        try
        {
            if (blackListedTokens.Contains(token))
            {
                return;
            }

            bool exist = false;

            foreach (Tuple<string, DiscordSocketClient> tuple in sessions)
            {
                try
                {
                    if (tuple.Item1 == token)
                    {
                        exist = true;

                        break;
                    }
                }
                catch
                {

                }
            }

            if (!exist)
            {
                if (blackListedTokens.Contains(token))
                {
                    return;
                }

                DiscordSocketConfig config = new DiscordSocketConfig();

                if (metroChecker13.Checked)
                {
                    config.Proxy = AnarchyProxy.Parse(AnarchyProxyType.HTTP, GetRandomProxy());
                }

                config.ApiVersion = 9;
                DiscordSocketClient client = new DiscordSocketClient(config);
                client.OnLeftGuild += Client_OnLeftGuild;
                client.Login(token);
                Thread.Sleep(500);
                sessions.Add(new Tuple<string, DiscordSocketClient>(token, client));

                if (metroComboBox1.SelectedIndex == 0)
                {
                    client.SetStatus(UserStatus.Online);
                }
                else if (metroComboBox1.SelectedIndex == 1)
                {
                    client.SetStatus(UserStatus.Idle);
                }
                else if (metroComboBox1.SelectedIndex == 2)
                {
                    client.SetStatus(UserStatus.DoNotDisturb);
                }
                else
                {
                    client.SetStatus(UserStatus.Invisible);
                }
            }
            else
            {
                foreach (Tuple<string, DiscordSocketClient> tuple in sessions)
                {
                    try
                    {
                        if (tuple.Item1 == token)
                        {
                            if (metroComboBox1.SelectedIndex == 0)
                            {
                                tuple.Item2.SetStatus(UserStatus.Online);
                            }
                            else if (metroComboBox1.SelectedIndex == 1)
                            {
                                tuple.Item2.SetStatus(UserStatus.Idle);
                            }
                            else if (metroComboBox1.SelectedIndex == 2)
                            {
                                tuple.Item2.SetStatus(UserStatus.DoNotDisturb);
                            }
                            else
                            {
                                tuple.Item2.SetStatus(UserStatus.Invisible);
                            }

                            break;
                        }
                    }
                    catch
                    {

                    }
                }
            }
        }
        catch
        {

        }
    }

    private void metroButton32_Click(object sender, EventArgs e)
    {
        if (MessageBox.Show("Are you sure you want to Phone Lock all of your loaded tokens?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation).Equals(DialogResult.Yes))
        {
            new Thread(doPhoneLocker).Start();
        }
    }

    public void doPhoneLocker()
    {
        foreach (string token in SplitToLines(metroTextbox1.Text))
        {
            new Thread(() => phoneLock(token)).Start();
        }
    }

    public void phoneLock(string token)
    {
        try
        {
            new HttpClient().SendAsync(new HttpRequestMessage
            {
                RequestUri = new Uri($"https://discord.com/api/v9/invites/mjGFnGy4vj"),
                Method = HttpMethod.Post,

                Headers =
                {
                    {
                        HttpRequestHeader.Authorization.ToString(), token
                    },

                    {
                        HttpRequestHeader.ContentType.ToString(), "multipart/mixed"
                    },
                },
            });
        }
        catch
        {

        }
    }

    private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        Process.Start(linkLabel2.Text);
    }

    private void metroButton33_Click(object sender, EventArgs e)
    {
        new Thread(doHypeSquadSetter).Start();
    }

    public void doHypeSquadSetter()
    {
        foreach (string token in SplitToLines(metroTextbox1.Text))
        {
            if (blackListedTokens.Contains(token))
            {
                continue;
            }

            new Thread(() => setHypeSquad(token)).Start();
        }
    }

    public void setHypeSquad(string token)
    {
        try
        {
            if (blackListedTokens.Contains(token))
            {
                return;
            }

            bool exist = false;

            foreach (Tuple<string, DiscordSocketClient> tuple in sessions)
            {
                try
                {
                    if (tuple.Item1 == token)
                    {
                        exist = true;

                        break;
                    }
                }
                catch
                {

                }
            }

            if (!exist)
            {
                if (blackListedTokens.Contains(token))
                {
                    return;
                }

                DiscordSocketConfig config = new DiscordSocketConfig();

                if (metroChecker13.Checked)
                {
                    config.Proxy = AnarchyProxy.Parse(AnarchyProxyType.HTTP, GetRandomProxy());
                }

                config.ApiVersion = 9;
                DiscordSocketClient client = new DiscordSocketClient(config);
                client.OnLeftGuild += Client_OnLeftGuild;
                client.Login(token);
                Thread.Sleep(500);
                sessions.Add(new Tuple<string, DiscordSocketClient>(token, client));

                if (metroComboBox2.SelectedIndex == 0)
                {
                    client.GetClientUser().SetHypesquad(Hypesquad.Balance);
                }
                else if (metroComboBox2.SelectedIndex == 1)
                {
                    client.GetClientUser().SetHypesquad(Hypesquad.Bravery);
                }
                else if (metroComboBox2.SelectedIndex == 2)
                {
                    client.GetClientUser().SetHypesquad(Hypesquad.Brilliance);
                }
                else
                {
                    client.GetClientUser().SetHypesquad(Hypesquad.None);
                }
            }
            else
            {
                foreach (Tuple<string, DiscordSocketClient> tuple in sessions)
                {
                    try
                    {
                        if (tuple.Item1 == token)
                        {
                            if (metroComboBox2.SelectedIndex == 0)
                            {
                                tuple.Item2.GetClientUser().SetHypesquad(Hypesquad.Balance);
                            }
                            else if (metroComboBox2.SelectedIndex == 1)
                            {
                                tuple.Item2.GetClientUser().SetHypesquad(Hypesquad.Bravery);
                            }
                            else if (metroComboBox2.SelectedIndex == 2)
                            {
                                tuple.Item2.GetClientUser().SetHypesquad(Hypesquad.Brilliance);
                            }
                            else
                            {
                                tuple.Item2.GetClientUser().SetHypesquad(Hypesquad.None);
                            }

                            break;
                        }
                    }
                    catch
                    {

                    }
                }
            }
        }
        catch
        {

        }
    }

    private void metroChecker55_CheckedChanged(object sender, bool isChecked)
    {
        Utils.hideLiveLogs = metroChecker55.Checked;
    }

    private void metroButton37_Click(object sender, EventArgs e)
    {
        addLiveLog("Succesfully started the Anti Kick.");

        metroButton37.Enabled = false;

        metroTextbox36.Enabled = false;
        metroTextbox37.Enabled = false;

        new Thread(startAntiKick).Start();

        metroButton36.Enabled = true;
    }

    private void metroButton36_Click(object sender, EventArgs e)
    {
        addLiveLog("Succesfully stopped the Anti Kick.");

        metroButton36.Enabled = false;
        metroButton37.Enabled = true;

        metroTextbox36.Enabled = true;
        metroTextbox37.Enabled = true;
    }

    private void pictureBox24_Click(object sender, EventArgs e)
    {
        try
        {
            Process.GetCurrentProcess().Kill();
        }
        catch
        {

        }
    }

    private void pictureBox23_Click(object sender, EventArgs e)
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

    private void pictureBox22_Click(object sender, EventArgs e)
    {
        try
        {
            WindowState = FormWindowState.Minimized;
        }
        catch
        {

        }
    }

    private void pictureBox24_MouseEnter(object sender, EventArgs e)
    {
        pictureBox24.Size = new System.Drawing.Size(22, 22);
    }

    private void pictureBox24_MouseLeave(object sender, EventArgs e)
    {
        pictureBox24.Size = new System.Drawing.Size(24, 24);
    }

    private void pictureBox23_MouseEnter(object sender, EventArgs e)
    {
        pictureBox23.Size = new System.Drawing.Size(22, 22);
    }

    private void pictureBox23_MouseLeave(object sender, EventArgs e)
    {
        pictureBox23.Size = new System.Drawing.Size(24, 24);
    }

    private void pictureBox22_MouseEnter(object sender, EventArgs e)
    {
        pictureBox22.Size = new System.Drawing.Size(22, 22);
    }

    private void pictureBox22_MouseLeave(object sender, EventArgs e)
    {
        pictureBox22.Size = new System.Drawing.Size(24, 24);
    }

    private void metroChecker61_Click(object sender, EventArgs e)
    {
        metroChecker62.Checked = false;
    }

    private void metroChecker62_Click(object sender, EventArgs e)
    {
        metroChecker61.Checked = false;
    }

    private void metroButton38_Click(object sender, EventArgs e)
    {
        new Thread(fetchEmote).Start();
    }

    public void fetchEmote()
    {
        try
        {
            string token = GetRandomValidToken();

            var httpRequest = new Leaf.xNet.HttpRequest();
            string[] proxy = null;

            try
            {
                if (metroChecker13.Checked)
                {
                    proxy = Microsoft.VisualBasic.Strings.Split(GetRandomProxy(), ":");
                    httpRequest.Proxy = new Leaf.xNet.HttpProxyClient(proxy[0], int.Parse(proxy[1]));
                }
                else
                {
                    httpRequest.Proxy = null;
                }
            }
            catch
            {

            }

            httpRequest.KeepTemporaryHeadersOnRedirect = false;
            httpRequest.EnableMiddleHeaders = false;
            httpRequest.ClearAllHeaders();
            httpRequest.AllowEmptyHeaderValues = false;

            httpRequest.AddHeader("Accept", "*/*");
            httpRequest.AddHeader("Accept-Encoding", "gzip, deflate, br");
            httpRequest.AddHeader("Accept-Language", getTokenLanguage(token));
            httpRequest.AddHeader("Authorization", token);
            httpRequest.AddHeader("Connection", "keep-alive");
            httpRequest.AddHeader("Cookie", Utils.GetRandomCookie(getTokenLanguage(token)));
            httpRequest.AddHeader("DNT", "1");
            httpRequest.AddHeader("Host", "discord.com");
            httpRequest.AddHeader("Referer", "https://discord.com/channels/@me");
            httpRequest.AddHeader("TE", "Trailers");
            httpRequest.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:88.0) Gecko/20100101 Firefox/88.0");
            httpRequest.AddHeader("X-Super-Properties", "eyJvcyI6IldpbmRvd3MiLCJicm93c2VyIjoiRmlyZWZveCIsImRldmljZSI6IiIsInN5c3RlbV9sb2NhbGUiOiJpdC1JVCIsImJyb3dzZXJfdXNlcl9hZ2VudCI6Ik1vemlsbGEvNS4wIChXaW5kb3dzIE5UIDEwLjA7IFdpbjY0OyB4NjQ7IHJ2Ojg4LjApIEdlY2tvLzIwMTAwMTAxIEZpcmVmb3gvODguMCIsImJyb3dzZXJfdmVyc2lvbiI6Ijg4LjAiLCJvc192ZXJzaW9uIjoiMTAiLCJyZWZlcnJlciI6IiIsInJlZmVycmluZ19kb21haW4iOiIiLCJyZWZlcnJlcl9jdXJyZW50IjoiIiwicmVmZXJyaW5nX2RvbWFpbl9jdXJyZW50IjoiIiwicmVsZWFzZV9jaGFubmVsIjoic3RhYmxlIiwiY2xpZW50X2J1aWxkX251bWJlciI6ODMwNDAsImNsaWVudF9ldmVudF9zb3VyY2UiOm51bGx9");

            httpRequest.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;

            string response = Utils.DecompressResponse(httpRequest.Get("https://discord.com/api/v9/channels/" + metroTextbox7.Text + "/messages?limit=50"));
            dynamic dynJson = JsonConvert.DeserializeObject(response);

            foreach (var item in dynJson)
            {
                if ((string) item.id == metroTextbox8.Text)
                {
                    foreach (var item1 in item.reactions)
                    {
                        string reaction = "", id = "";
                        id = item1.emoji.id;
                        reaction = item1.emoji.name;

                        if (id != null && id != "")
                        {
                            reaction += ":" + id;
                        }

                        metroTextbox6.Text = reaction;

                        break;
                    }

                    break;
                }
            }
        }
        catch
        {
            try
            {
                MessageBox.Show("Failed to fetch your emote!", "AstarothSpammer", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch
            {

            }
        }
    }

    private void metroChecker48_MouseClick(object sender, MouseEventArgs e)
    {
        metroChecker65.Checked = false;
    }

    private void metroChecker65_MouseClick(object sender, MouseEventArgs e)
    {
        metroChecker48.Checked = false;
    }

    private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        Process.Start(linkLabel3.Text);
    }

    private void metroButton34_Click_1(object sender, EventArgs e)
    {
        if (!Microsoft.VisualBasic.Information.IsNumeric(metroTextbox42.Text))
        {
            MessageBox.Show("Please, insert a valid number of placeholders!", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        metroTextbox43.Text = "";
        string total = "";

        for (int i = 0; i < int.Parse(metroTextbox42.Text); i++)
        {
            if (metroChecker66.Checked)
            {
                total += "[rndnum] ";
            }

            if (metroChecker67.Checked)
            {
                total += "[rndstr] ";
            }

            if (metroChecker68.Checked)
            {
                total += "[mtag] ";
            }

            if (metroChecker69.Checked)
            {
                total += "[rtag] ";
            }

            if (metroChecker70.Checked)
            {
                total += "[all] ";
            }

            if (metroChecker71.Checked)
            {
                total += "[rall] ";
            }
        }

        try
        {
            metroTextbox43.Text = total.Substring(0, total.Length - 1);
        }
        catch
        {
            metroTextbox43.Text = total;
        }
    }

    private void metroChecker73_CheckedChanged(object sender, bool isChecked)
    {
        if (metroChecker73.Checked)
        {
            metroTextbox4.MaxLength = 2147483647;
        }
        else
        {
            metroTextbox4.MaxLength = 18;
        }
    }

    private void metroChecker83_CheckedChanged(object sender, bool isChecked)
    {
        if (metroChecker83.Checked)
        {
            metroTextbox15.MaxLength = 2147483647;
        }
        else
        {
            metroTextbox15.MaxLength = 18;
        }
    }

    private void metroChecker83_CheckedChanged_1(object sender, bool isChecked)
    {
        if (metroChecker83.Checked)
        {
            metroTextbox15.MaxLength = 2147483647;
        }
        else
        {
            metroTextbox15.MaxLength = 18;
        }
    }

    private void metroChecker74_CheckedChanged(object sender, bool isChecked)
    {
        if (metroChecker74.Checked)
        {
            metroTextbox5.MaxLength = 2147483647;
        }    
        else
        {
            metroTextbox5.MaxLength = 2000;
        }
    }

    private void metroChecker84_CheckedChanged(object sender, bool isChecked)
    {
        if (metroChecker84.Checked)
        {
            metroTextbox16.MaxLength = 2147483647;
        }
        else
        {
            metroTextbox16.MaxLength = 2000;
        }
    }

    private void metroChecker81_CheckedChanged(object sender, bool isChecked)
    {

    }

    private void metroChecker77_CheckedChanged(object sender, bool isChecked)
    {
        if (metroChecker77.Checked)
        {
            metroTextbox10.MaxLength = 2147483647;
        }
        else
        {
            metroTextbox10.MaxLength = 18;
        }
    }

    private void metroChecker80_CheckedChanged(object sender, bool isChecked)
    {
        try
        {
            if (metroChecker80.Checked)
            {
                metroButton10.Text = "Start Spamming";
                metroButton9.Text = "Stop Spamming";

                pictureBox13.BackgroundImage = pictureBox21.BackgroundImage;
                pictureBox14.BackgroundImage = pictureBox20.BackgroundImage;

                metroButton10.Enabled = true;
                metroButton9.Enabled = false;

                friendSpamMode = true;
            }
            else
            {
                metroButton10.Text = "Add friend";
                metroButton9.Text = "Remove friend";

                pictureBox13.BackgroundImage = pictureBox11.BackgroundImage;
                pictureBox14.BackgroundImage = pictureBox12.BackgroundImage;

                metroButton10.Enabled = true;
                metroButton9.Enabled = true;

                friendSpamMode = false;
            }

            this.Refresh();

            metroButton10.Refresh();
            metroButton9.Refresh();

            pictureBox13.Refresh();
            pictureBox14.Refresh();
        }
        catch
        {

        }
    }

    private void metroChecker79_CheckedChanged(object sender, bool isChecked)
    {
        try
        {
            if (metroChecker79.Checked)
            {
                metroTextbox9.MaxLength = 2147483647;
            }
            else
            {
                metroTextbox9.MaxLength = 18;
            }
        }
        catch
        {

        }
    }

    private void metroChecker76_CheckedChanged(object sender, bool isChecked)
    {
        try
        {
            if (metroChecker76.Checked)
            {
                metroButton3.Text = "Start Spamming";
                metroButton4.Text = "Stop Spamming";

                pictureBox1.BackgroundImage = pictureBox21.BackgroundImage;
                pictureBox2.BackgroundImage = pictureBox20.BackgroundImage;

                metroButton3.Enabled = true;
                metroButton4.Enabled = false;

                guildRaidMode = true;
            }
            else
            {
                metroButton3.Text = "Join this guild";
                metroButton4.Text = "Leave this guild";

                pictureBox1.BackgroundImage = pictureBox11.BackgroundImage;
                pictureBox2.BackgroundImage = pictureBox12.BackgroundImage;

                metroButton3.Enabled = true;
                metroButton4.Enabled = true;

                guildRaidMode = false;
            }

            this.Refresh();

            metroButton3.Refresh();
            metroButton4.Refresh();

            pictureBox1.Refresh();
            pictureBox2.Refresh();
        }
        catch
        {

        }
    }

    public void startAntiKick()
    {
        try
        {
            if (!IsIDValid(metroTextbox36.Text))
            {
                metroButton36.Enabled = false;
                metroButton37.Enabled = true;

                metroTextbox36.Enabled = true;
                metroTextbox37.Enabled = true;

                MessageBox.Show("The id of the guild is not valid!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }

            string theToken = GetRandomValidToken();
            Tuple<bool, string, string, string> info = GetInviteInformations(metroTextbox37.Text);

            if (!info.Item1)
            {
                metroButton36.Enabled = false;
                metroButton37.Enabled = true;

                metroTextbox36.Enabled = true;
                metroTextbox37.Enabled = true;

                MessageBox.Show("The invite link / code is not valid! Please, check if your tokens are valid.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }

            theGuildId1 = "";
            theChannelType1 = "";
            theXCP641 = "";
            theChannelId1 = "";

            theGuildId1 = info.Item2;
            theChannelId1 = info.Item3;
            theChannelType1 = info.Item4;

            theXCP641 = Utils.GetXCP(theGuildId1, theChannelId1, theChannelType1);

            addLiveLog("Got all data of the guild to start the Anti Kick process.");

            foreach (string token in SplitToLines(metroTextbox1.Text))
            {
                try
                {
                    if (blackListedTokens.Contains(token))
                    {
                        continue;
                    }

                    new Thread(() => antiKick(token)).Start();
                }
                catch
                {

                }
            }
        }
        catch
        {

        }
    }

    public void antiKick(string token)
    {
        try
        {
            if (blackListedTokens.Contains(token))
            {
                return;
            }

            bool exist = false;

            foreach (Tuple<string, DiscordSocketClient> tuple in sessions)
            {
                try
                {
                    if (tuple.Item1 == token)
                    {
                        exist = true;

                        break;
                    }
                }
                catch
                {

                }
            }

            if (!exist)
            {
                if (blackListedTokens.Contains(token))
                {
                    return;
                }

                DiscordSocketConfig config = new DiscordSocketConfig();

                if (metroChecker13.Checked)
                {
                    config.Proxy = AnarchyProxy.Parse(AnarchyProxyType.HTTP, GetRandomProxy());
                }

                config.ApiVersion = 9;
                DiscordSocketClient client = new DiscordSocketClient(config);
                client.OnLeftGuild += Client_OnLeftGuild;
                client.Login(token);
                Thread.Sleep(500);
                sessions.Add(new Tuple<string, DiscordSocketClient>(token, client));
            }
            else
            {
                foreach (Tuple<string, DiscordSocketClient> tuple in sessions)
                {
                    try
                    {
                        if (tuple.Item1 == token)
                        {
                            tuple.Item2.OnLeftGuild += Client_OnLeftGuild;

                            break;
                        }
                    }
                    catch
                    {

                    }
                }
            }
        }
        catch
        {

        }
    }

    private void Client_OnLeftGuild(DiscordSocketClient client, GuildUnavailableEventArgs args)
    {
        try
        {
            if (metroButton37.Enabled)
            {
                return;
            }

            if (blackListedTokens.Contains(client.Token))
            {
                return;
            }

            if (args.Guild.Id.ToString().Equals(metroTextbox36.Text))
            {
                Leaf.xNet.HttpRequest httpRequest = Utils.CreateCleanRequest();
                string[] proxy = null;

                try
                {
                    if (metroChecker13.Checked)
                    {
                        proxy = Microsoft.VisualBasic.Strings.Split(GetRandomProxy(), ":");
                        httpRequest.Proxy = new Leaf.xNet.HttpProxyClient(proxy[0], int.Parse(proxy[1]));
                    }
                    else
                    {
                        httpRequest.Proxy = null;
                    }
                }
                catch
                {

                }

                httpRequest.AddHeader("Accept", "*/*");
                httpRequest.AddHeader("Accept-Encoding", "gzip, deflate, br");
                httpRequest.AddHeader("Accept-Language", getTokenLanguage(client.Token));
                httpRequest.AddHeader("Authorization", client.Token);
                httpRequest.AddHeader("Connection", "keep-alive");
                httpRequest.AddHeader("Content-Length", "0");
                httpRequest.AddHeader("Cookie", Utils.GetRandomCookie(getTokenLanguage(client.Token)));
                httpRequest.AddHeader("DNT", "1");
                httpRequest.AddHeader("Host", "discord.com");
                httpRequest.AddHeader("Origin", "https://discord.com");
                httpRequest.AddHeader("TE", "Trailers");
                httpRequest.AddHeader("Referer", "https://discord.com/channels/@me");
                httpRequest.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:88.0) Gecko/20100101 Firefox/88.0");
                httpRequest.AddHeader("X-Context-Properties", theXCP641);
                httpRequest.AddHeader("X-Super-Properties", "eyJvcyI6IldpbmRvd3MiLCJicm93c2VyIjoiRmlyZWZveCIsImRldmljZSI6IiIsInN5c3RlbV9sb2NhbGUiOiJpdC1JVCIsImJyb3dzZXJfdXNlcl9hZ2VudCI6Ik1vemlsbGEvNS4wIChXaW5kb3dzIE5UIDEwLjA7IFdpbjY0OyB4NjQ7IHJ2Ojg4LjApIEdlY2tvLzIwMTAwMTAxIEZpcmVmb3gvODguMCIsImJyb3dzZXJfdmVyc2lvbiI6Ijg4LjAiLCJvc192ZXJzaW9uIjoiMTAiLCJyZWZlcnJlciI6IiIsInJlZmVycmluZ19kb21haW4iOiIiLCJyZWZlcnJlcl9jdXJyZW50IjoiIiwicmVmZXJyaW5nX2RvbWFpbl9jdXJyZW50IjoiIiwicmVsZWFzZV9jaGFubmVsIjoic3RhYmxlIiwiY2xpZW50X2J1aWxkX251bWJlciI6ODMwNDAsImNsaWVudF9ldmVudF9zb3VyY2UiOm51bGx9");

                httpRequest.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;

                string response = Utils.DecompressResponse(httpRequest.Post("https://discord.com/api/v9/invites/" + GetInviteCodeByInviteLink(GetInviteCodeByInviteLink(metroTextbox37.Text))));

                addLiveLog(proxy == null ? "Token (" + client.Token + ") tried to join the guild with invite code '" + GetInviteCodeByInviteLink(metroTextbox37.Text) + "' and ID '" + theGuildId1 + "' after being kicked from the guild. Anti Kick process succesfully started -> " + response : "Token (" + client.Token + ") with proxy (" + proxy[0] + ":" + proxy[1] + ") tried to join the guild with invite code '" + GetInviteCodeByInviteLink(metroTextbox37.Text) + "' and ID '" + theGuildId1 + "' after being kicked from the guild. Anti Kick process succesfully started -> " + response);

                if (response.ToLower().Contains("verify your account") || response.ToLower().Contains("you are being blocked from accessing our"))
                {
                    blackListedTokens.Add(client.Token);
                    addLiveLog("Token (" + client.Token + ") has been blacklisted because it is unverified or blocked by Discord -> " + response);

                    return;
                }

                if (metroChecker23.Checked)
                {
                    Thread.Sleep(2500);

                    bypassRules1(client.Token);
                }

                if (metroChecker48.Checked)
                {
                    Thread.Sleep(2500);

                    bypassCaptcha(client.Token);
                }

                if (metroChecker50.Checked)
                {
                    Thread.Sleep(2500);

                    bypassReaction(client.Token);
                }
            }
        }
        catch
        {

        }
    }

    private void metroButton22_Click(object sender, EventArgs e)
    {
        try
        {
            if (openFileDialog3.ShowDialog() == DialogResult.OK)
            {
                metroTextbox34.Text = openFileDialog3.FileName;
            }
        }
        catch
        {

        }
    }

    private void metroChecker56_CheckedChanged(object sender, bool isChecked)
    {
        try
        {
            metroTextbox34.Enabled = metroChecker56.Checked;
            metroButton22.Enabled = metroChecker56.Checked;
            metroSwitch1.Enabled = metroChecker56.Checked;
        }
        catch
        {

        }
    }

    private void metroSwitch1_CheckedChanged(object sender, bool isChecked)
    {
        try
        {
            metroTextbox35.Enabled = metroSwitch1.Checked;
        }
        catch
        {

        }
    }

    private async void Client_OnLoggedIn1(DiscordSocketClient client, LoginEventArgs args)
    {
        try
        {
            roles.Clear();
            metroLabel20.Text = roles.Count.ToString() + " roles have been downloaded.";
            ulong guildId = ulong.Parse(metroTextbox17.Text);

            var list = client.GetGuildRolesAsync(guildId).Result;

            foreach (DiscordRole role in list)
            {
                if (!role.Name.ToLower().Equals("@everyone"))
                {
                    try
                    {
                        roles.Add(role.Id.ToString());
                        metroLabel20.Text = roles.Count.ToString() + " roles have been downloaded.";
                    }
                    catch
                    {

                    }
                }
            }

            metroLabel20.Text = roles.Count.ToString() + " roles have been downloaded.";
            MessageBox.Show("Succesfully downloaded all roles!", "AstarothSpammer", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch
        {
            MessageBox.Show("Failed to download all roles!", "AstarothSpammer", MessageBoxButtons.OK, MessageBoxIcon.Error);

            metroLabel20.Text = "0 roles have been downloaded.";
            roles.Clear();
        }
    }

    private void metroButton27_Click(object sender, EventArgs e)
    {
        try
        {
            if (saveFileDialog2.ShowDialog().Equals(DialogResult.OK))
            {
                string allUsers = "";

                foreach (string user in roles)
                {
                    if (allUsers == "")
                    {
                        allUsers = user;
                    }
                    else
                    {
                        allUsers += "\r\n" + user;
                    }
                }

                System.IO.File.WriteAllText(saveFileDialog2.FileName, allUsers);
            }
        }
        catch
        {

        }
    }

    private void metroButton28_Click(object sender, EventArgs e)
    {
        try
        {
            openFileDialog1.Title = "Open your downloaded roles list...";

            if (openFileDialog1.ShowDialog().Equals(DialogResult.OK))
            {
                roles.Clear();

                foreach (string user in SplitToLines(System.IO.File.ReadAllText(openFileDialog1.FileName)))
                {
                    try
                    {
                        if (user.Trim().Replace(" ", "").Replace('\t'.ToString(), "") != "" && user.Trim().Replace(" ", "").Replace('\t'.ToString(), "").Length == 18)
                        {
                            if (Microsoft.VisualBasic.Information.IsNumeric(user.Trim().Replace(" ", "").Replace('\t'.ToString(), "")))
                            {
                                roles.Add(user.Trim().Replace(" ", "").Replace('\t'.ToString(), ""));
                            }
                        }
                    }
                    catch
                    {

                    }
                }

                metroLabel20.Text = roles.Count.ToString() + " roles have been downloaded.";
            }
        }
        catch
        {

        }
    }

    public void autoEndCall(DiscordSocketClient client)
    {
        try
        {
            Thread.Sleep(10 + metroTrackbar10.Value);
            client.ChangeVoiceState(new VoiceStateProperties());
        }
        catch
        {

        }
    }

    public void doCallEnder()
    {
        try
        {
            if (!IsIDValid(metroTextbox19.Text))
            {
                MessageBox.Show("The id of user is not valid!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }

            int i = 0, j = 0;

            if (metroChecker28.Checked)
            {
                try
                {
                    i = int.Parse(metroTextbox22.Text.Replace(" ", "").Replace('\t'.ToString(), ""));

                    if (i < 0)
                    {
                        i = 0;
                    }
                }
                catch
                {

                }
            }

            foreach (string token in SplitToLines(metroTextbox1.Text))
            {
                try
                {
                    if (blackListedTokens.Contains(token))
                    {
                        return;
                    }

                    if (metroChecker28.Checked)
                    {
                        if (j == i)
                        {
                            break;
                        }

                        j++;
                    }

                    if (metroChecker26.Checked)
                    {
                        Thread.Sleep(metroTrackbar9.Value);
                    }

                    new Thread(() => endCall(token)).Start();
                }
                catch
                {

                }
            }
        }
        catch
        {

        }
    }

    public void endCall(string token)
    {
        try
        {
            if (blackListedTokens.Contains(token))
            {
                return;
            }

            try
            {
                bool exist = false;

                foreach (Tuple<string, DiscordSocketClient> tuple in sessions)
                {
                    try
                    {
                        if (tuple.Item1 == token)
                        {
                            exist = true;

                            break;
                        }
                    }
                    catch
                    {

                    }
                }

                if (!exist)
                {
                    if (blackListedTokens.Contains(token))
                    {
                        return;
                    }

                    DiscordSocketConfig config = new DiscordSocketConfig();

                    if (metroChecker13.Checked)
                    {
                        config.Proxy = AnarchyProxy.Parse(AnarchyProxyType.HTTP, GetRandomProxy());
                    }

                    config.ApiVersion = 9;
                    DiscordSocketClient client = new DiscordSocketClient(config);
                    client.Login(token);
                    Thread.Sleep(500);
                    sessions.Add(new Tuple<string, DiscordSocketClient>(token, client));

                    client.ChangeVoiceState(new VoiceStateProperties());
                }
                else
                {
                    foreach (Tuple<string, DiscordSocketClient> tuple in sessions)
                    {
                        try
                        {
                            if (tuple.Item1 == token)
                            {
                                tuple.Item2.ChangeVoiceState(new VoiceStateProperties());

                                break;
                            }
                        }
                        catch

                        {

                        }
                    }
                }
            }
            catch
            {

            }
        }
        catch
        {

        }
    }
}