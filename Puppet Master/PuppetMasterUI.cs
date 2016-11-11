using System;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace PuppetMaster
{
    public partial class PuppetMasterUI : Form
    {
        private PuppetMaster pm;
        private string logs="";

        public PuppetMasterUI()
        {
            InitializeComponent();
            pm = new PuppetMaster();
            pm.start();
            this.Result.Text = pm.getLogs();
            this.Result.SelectionStart = this.Result.Text.Length;
            this.Result.ScrollToCaret();
            FormClosing += PuppetMasterUI_FormClosing;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            try
            {
                configButton.Enabled = false;
                scriptButton.Enabled = false;

                await Task.Run(() => pm.readScriptFile());

                System.Windows.Forms.Timer timer1 = new System.Windows.Forms.Timer();
                timer1.Tick += new EventHandler(refreshLogs);
                timer1.Interval = 1000; // in miliseconds
                timer1.Start();
            }
            catch(ParseException ex)
            {
                this.Result.Text = ex.Msg;
                this.Result.SelectionStart = this.Result.Text.Length;
                this.Result.ScrollToCaret();
            }
            
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            configButton.Enabled = false;

            await Task.Run(() => pm.processOneMoreStep());
            if (pm.finishedparsingScript())
                scriptButton.Enabled = false;

            System.Windows.Forms.Timer timer1 = new System.Windows.Forms.Timer();
            timer1.Tick += new EventHandler(refreshLogs);
            timer1.Interval = 1000; // in miliseconds
            timer1.Start();
        }

        private void refreshLogs(object sender, EventArgs e)
        {
            string refreshedLogs = pm.getLogs();
            if (!refreshedLogs.Equals(logs))
            {
                logs = refreshedLogs;
                this.Result.Text = pm.getLogs();
                this.Result.SelectionStart = this.Result.Text.Length;
                this.Result.ScrollToCaret();
            }
        }

        private void Result_TextChanged(object sender, EventArgs e)
        {

        }

        private void PuppetMasterUI_Load(object sender, EventArgs e)
        {

        }

        private async void RunButton_Click(object sender, EventArgs e)
        {
            await Task.Run(() => pm.enterCommand(commandTextBox.Text));
            commandTextBox.Text = "";
            this.Result.Text = pm.getLogs();
            this.Result.SelectionStart = this.Result.Text.Length;
            this.Result.ScrollToCaret();
        }

        private void PuppetMasterUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            pm.shutDownAll();
        }

        private void PuppetMasterUI_Load_1(object sender, EventArgs e)
        {

        }
    }
}
