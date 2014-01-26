using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Xml;

namespace Dota2Mods_Client
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        public string steamPath = "";
        public string dotaPath = "";

        private void button1_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                steamPath = folderBrowserDialog1.SelectedPath;
                dotaPath = steamPath + "\\SteamApps\\common\\dota 2 beta";
                if (Directory.Exists(dotaPath))
                {
                    label4.Text = dotaPath;
                    if (MessageBox.Show("Dota 2 directory found, would you like to save this path as the default one ?", "Dota 2 directory found, you can go to the next step !", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        WriteXML(steamPath, dotaPath);
                    }
                    button3.Enabled = true;
                }
                else
                {
                    MessageBox.Show("Did not find the Dota 2 directory, select it manually.");
                    button2.Enabled = true;
                }
                label2.Text = steamPath;
            }
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                dotaPath = folderBrowserDialog1.SelectedPath;
                button2.Enabled = true;
                label4.Text = dotaPath;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            Form1 f1 = new Form1(steamPath, dotaPath);
            f1.Show();

        }

        private void Form2_Load(object sender, EventArgs e)
        {
            if (File.Exists("config.xml"))
            {
                if (MessageBox.Show("Saved path found, use it ?", "Config file found !", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    ReadXML(ref steamPath, ref dotaPath);
                    this.Visible = false;
                    Form1 f1 = new Form1(steamPath, dotaPath);
                    f1.ShowDialog();
                    this.Visible = false;
                    Application.Exit();
                }
            }
        }


        public static void WriteXML(string steam, string dota)
        {
            XmlTextWriter myXmlTextWriter = new XmlTextWriter("config.xml", null);
            myXmlTextWriter.Formatting = Formatting.Indented;
            myXmlTextWriter.WriteStartDocument(false);
            myXmlTextWriter.WriteComment("This is the configuration file. You can change manually, but it may crash.");


            myXmlTextWriter.WriteStartElement("GlobalPaths");
            myXmlTextWriter.WriteElementString("Steam", steam);
            myXmlTextWriter.WriteElementString("Dota", dota);
            myXmlTextWriter.WriteEndElement();


            myXmlTextWriter.Flush();
            myXmlTextWriter.Close();

        }

        public static void ReadXML(ref string steam, ref string dota)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load("config.xml");
            XmlNodeList xnList = xml.SelectNodes("/GlobalPaths");
            foreach (XmlNode xn in xnList)
            {
                steam = xn["Steam"].InnerText;
                dota = xn["Dota"].InnerText;
            }
        }
    }
}
