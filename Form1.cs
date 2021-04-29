using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace aramboost31
{
    public partial class Form1 : Form
    {
        private LCUListener listener = new LCUListener();

        public Form1()
        {
            InitializeComponent();
            listener.StartListening();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.FormClosing += Form1_FormClosing;
            
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if (this.listener.GetGatheredLCUs().Count == 0)
            {
                MessageBox.Show("Please run League of Legends first");
            }
            else
            {

                foreach (LCUClient LCU in listener.GetGatheredLCUs())
                {
                    await LCU.HttpPostForm("/lol-login/v1/session/invoke", new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("destination", "lcdsServiceProxy"),
                new KeyValuePair<string, string>("method", "call"),
                new KeyValuePair<string, string>("args", "[\"\",\"teambuilder-draft\",\"activateBattleBoostV1\",\"\"]")
            });
                }

            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.listener.StopListening();
        }
    }
}
