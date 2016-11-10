using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PuppetMaster
{
    public partial class PuppetMasterUI : Form
    {
        private PuppetMaster pm;

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
            await Task.Run(() => pm.readScriptFile());
            this.Result.Text = pm.getLogs();
            this.Result.SelectionStart = this.Result.Text.Length;
            this.Result.ScrollToCaret();
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            await Task.Run(() => pm.processOneMoreStep());
            this.Result.Text = pm.getLogs();
            this.Result.SelectionStart = this.Result.Text.Length;
            this.Result.ScrollToCaret();
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
