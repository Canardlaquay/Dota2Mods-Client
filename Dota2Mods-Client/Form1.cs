using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlAgilityPack;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Net;
using System.IO.Compression;

namespace Dota2Mods_Client
{
    public partial class Form1 : Form
    {
        string steamPath = ""; //do I really need to re-explain ? :) 
        string dotaPath = ""; // (seriously, it's in Form2)
        string frotaPath = ""; //frota folder in addons folder
        List<string> rawInfo = new List<string>();
        SInfo[] serverInfo = new SInfo[50]; // struct array
       
        int updateTime = 1000;
        


        public Form1(string steamPath, string dotaPath)
        {
            InitializeComponent();
            this.steamPath = steamPath; //get 
            this.dotaPath = dotaPath;
            frotaPath = dotaPath + "\\dota\\addons\\Frota";
        }

        

        public struct SInfo //used to parse data for the listview
        {
            public string name;
            public int players;
            public int maxplayers;
            public string totalplayers;
            public string ip;
            public string map;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
            textBox1.Text = steamPath; 
            textBox2.Text = dotaPath;
            backgroundWorker1.RunWorkerAsync(); //worker refreshing server list every "updatetime" (in seconds)
            try
            {
                label4.Text = GetLocalFrotaVersion(frotaPath); 
            }
            catch (Exception)
            {
                
            }
            try
            {
                label6.Text = GetActualFrotaVersion();
            }
            catch (Exception)
            {
                
                
            }
            
        }

        public static string GetLocalFrotaVersion(string frotaPath) //reads data from the version.txt (Frota/scripts/version.txt)
        {
            string str = File.ReadAllText(frotaPath + "\\scripts\\version.txt").Replace("\"VersionControl\" {\n    \"version\"   \"", "").Replace("\"\n}", "");
            if (str == "")
            {
                str = "Not installed";
            }
            return str;
        }

        public static string GetActualFrotaVersion() //reads data from the version.txt from github
        {
            WebClient wc = new WebClient();
            string str = wc.DownloadString("https://raw2.github.com/ash47/Frota/master/scripts/version.txt");
            return str.Replace("\"VersionControl\" {\n    \"version\"   \"", "").Replace("\"\n}", ""); //parses the string
        }


        public static SInfo[] ParseRawInfo(List<string> list)
        {
            SInfo[] s = new SInfo[50];
            for (int i = 0; i < list.Count / 8; i++) //each server has 8 lines of info, so Count/8 gets the number of servers
            {
                s[i].name = list[2 + (8 * i)];
                s[i].totalplayers = list[3 + (8 * i)];
                string[] players = list[3 + (8 * i)].Split('/');
                s[i].players = Convert.ToInt32(players[0]);
                s[i].maxplayers = Convert.ToInt32(players[1]);
                s[i].ip = list[6 + (8 * i)];
                s[i].map = list[7 + (8 * i)];
                    
            }

            return s;
        }

        public static List<string> GetServersFromMap(List<string> list, string map) //query gametracker by map
        {
            HtmlWeb htmlWeb = new HtmlWeb();

            // Creates an HtmlDocument object from an URL
            HtmlAgilityPack.HtmlDocument document = htmlWeb.Load("http://www.gametracker.com/search/dota2/?search_by=map&query="+map.Trim()+"&searchipp=50");

            var query = from table in document.DocumentNode.SelectNodes("//table").Cast<HtmlNode>()
                        from row in table.SelectNodes("tr").Cast<HtmlNode>()
                        from cell in row.SelectNodes("td").Cast<HtmlNode>()
                        select new { Table = table.Id, CellText = cell.InnerText, CellClass = cell.Attributes };
            string rep = "";
            bool started = false;
            bool stopped = true;
            foreach (var cell in query)
            {

                if (cell.CellText.Contains("Rank&darr"))
                {
                    stopped = !stopped;
                    started = false;
                }
                if (started && !stopped)
                {
                    list.Add(cell.CellText.Trim());
                }

                if (cell.CellText.Contains("Server Map&nbsp;"))
                {
                    started = true;
                }

            }
            return list;
            
        }

        private void button1_Click(object sender, EventArgs e) //connect button
        {
            if (label4.Text == "Not installed")
            {
                if (MessageBox.Show("Frota is not installed, would you like to install it now ?", "Frota directory not found", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    backgroundWorker2.RunWorkerAsync(); //run the Frota installation asynchronously
                }
                
            }
            else
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    if (isDotaRunning())
                    {
                        string cmd = "steam://connect/" + listView1.SelectedItems[0].SubItems[2].Text; //used to launch dota, and directly connecting to the server
                        System.Diagnostics.Process.Start(cmd);
                    }
                    else
                    {
                        MessageBox.Show("Dota is not currently running !");
                    }


                }
            }
            
        }

        public static bool isDotaRunning()
        {
            Process[] proc = Process.GetProcessesByName("dota");
            bool found = false;
            if (proc.Length > 0)
            {
                if (proc[0].ProcessName == "dota")
                {
                    found = true;
                }
            }
            


            return found;
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    //this part refreshes the image at the bottom right, with the server info and such
                    webBrowser1.Stop();
                    webBrowser1.DocumentText = "";
                    Uri Url = new Uri("http://cache.www.gametracker.com/components/html0/?host=" + listView1.SelectedItems[0].SubItems[2].Text + "&bgColor=1F2642&fontColor=8790AE&titleBgColor=11172D&titleColor=FFFFFF&borderColor=333333&linkColor=FF9900&borderLinkColor=999999&showMap=0&showCurrPlayers=0&showTopPlayers=0&showBlogs=0&width=260");
                    webBrowser1.Url = null;
                    webBrowser1.Navigate(Url);
                    
                }
            }
            catch (Exception)
            {

            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            
            
        }

        
        



        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                rawInfo.Clear();
                rawInfo = GetServersFromMap(rawInfo, "runehill");
                rawInfo = GetServersFromMap(rawInfo, "riverofsouls"); //gets data from gametracker using map name
                serverInfo = ParseRawInfo(rawInfo); //parses it in a structured array
            }
            catch (Exception)
            {

            }

            System.Threading.Thread.Sleep(updateTime); //don't do it so fast, sleep a bit
            
        }


        private void backgroundWorker1_RunWorkerCompleted_1(object sender, RunWorkerCompletedEventArgs e)
        {
            //when the background worker finishes (its work, yes), refreshes the Form (can't do it directly from the async worker)
            richTextBox1.Text += "Refreshed server list.\n";
            int selectedIndex = 0; 
            if (listView1.SelectedIndices.Count > 0) //yes, up here (and =0 if none)
            {
                selectedIndex = listView1.SelectedIndices[0];
            }
            
            listView1.Items.Clear();
            foreach (SInfo s in serverInfo)
            {
                if (s.name != null)
                {
                    ListViewItem item = new ListViewItem(s.name);
                    item.SubItems.Add(s.totalplayers);
                    item.SubItems.Add(s.ip);
                    item.SubItems.Add(s.map);
                    listView1.Items.Add(item);
                }
            }
            listView1.Focus();
            listView1.Items[selectedIndex].Selected = true; //reselects last selected index defined up here
            
            backgroundWorker1.RunWorkerAsync(); //loop again
            
        }

        private void button4_Click(object sender, EventArgs e)
        {
            backgroundWorker2.RunWorkerAsync(); //run the Frota installation asynchronously
        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            backgroundWorker2.WorkerReportsProgress = true; //reports progress, another way to update the Form in the backgroundworker
            if (File.Exists("master.zip"))
            {
                File.Delete("master.zip");
                backgroundWorker2.ReportProgress(10, "Deleted old master.zip (Frota archive).\n"); //report(percent, richtextbox text)
            }
            if (Directory.Exists(Path.GetDirectoryName(Application.ExecutablePath) + "\\Frota-master"))
            {
                Directory.Delete(Path.GetDirectoryName(Application.ExecutablePath) + "\\Frota-master", true);
                backgroundWorker2.ReportProgress(20, "Deleted old Frota unzipped folder.\n");
            }
            WebClient webClient = new WebClient();
            
            backgroundWorker2.ReportProgress(30, "Downloading Frota archive...\n");
            webClient.DownloadFile("https://github.com/ash47/Frota/archive/master.zip", "master.zip"); //downloads the archive from github
            backgroundWorker2.ReportProgress(60, "Finished downloading, now extracting.\n");
            ZipFile.ExtractToDirectory("master.zip", Path.GetDirectoryName(Application.ExecutablePath)); //extracts it
            backgroundWorker2.ReportProgress(70, "Finished extraction.\n");
            if (Directory.Exists(frotaPath)) //DELETE EVERYTHING
            {
                Directory.Delete(frotaPath, true);
                backgroundWorker2.ReportProgress(80, "Old Frota folder deleted.\n");
            }
            Directory.Move(Path.GetDirectoryName(Application.ExecutablePath) + "\\Frota-master", frotaPath);
            backgroundWorker2.ReportProgress(100, "Frota is now installed !\n"); //cool
        }

        private void backgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            label4.Text = GetLocalFrotaVersion(frotaPath);//update printed Local version (up-left)
            MessageBox.Show("Frota is now installed with the last version !");//popup
        }

        private void backgroundWorker2_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            richTextBox1.Text += e.UserState as string;
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            richTextBox1.ScrollToCaret();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            updateTime = (int)numericUpDown1.Value * 1000;
            richTextBox1.Text += "Changed refresh time to " + updateTime.ToString() + ".\n"; //server browser refreshtime
        }

        private void button2_Click(object sender, EventArgs e) //change steampath from this form, read comments in Form1 to understand, it's basically the same
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                steamPath = folderBrowserDialog1.SelectedPath;
                dotaPath = steamPath + "\\SteamApps\\common\\dota 2 beta";
                if (Directory.Exists(dotaPath))
                {
                    textBox2.Text = dotaPath;
                    if (MessageBox.Show("Dota 2 directory found, would you like to save this path as the default one ?", "Dota 2 directory found, you can go to the next step !", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        Form2.WriteXML(steamPath, dotaPath);
                    }
                    
                }
                else
                {
                    MessageBox.Show("Did not find the Dota 2 directory, select it manually.");
                    
                }
                textBox1.Text = steamPath;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            
            Application.Exit();//not sure if I really need this code, or if I really need to comment this one
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string cmd = "steam://rungameid/570";
            System.Diagnostics.Process.Start(cmd);
        }

        
        



    }
}
