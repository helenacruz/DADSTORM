using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PuppetMaster
{
    delegate void updateLogsHandler(object sender, UpdateLogsArgs e);
    delegate void blockButtonsHandler(object sender, EventArgs e);
    delegate void enableButtonsHandler(object sender, EventArgs e);

    public partial class PuppetMasterUI : Form
    {
        private PuppetMaster pm;
        private string logs;

        public PuppetMasterUI()
        {
            InitializeComponent();
            pm = new PuppetMaster();
            logs = "";
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
            }
            catch (ParseException ex)
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
        }

        public void blockButtons(object sender, EventArgs e)
        {
            if (this.InvokeRequired == false)
            {
                configButton.Enabled = false;
                scriptButton.Enabled = false;
                RunButton.Enabled = false;
            }
            else
            {
                blockButtonsHandler blButtons = new blockButtonsHandler(blockButtons);
                Invoke(blButtons, new object[] { sender, e });
            }
        }

        public void enableButtons(object sender, EventArgs e)
        {
            if (this.InvokeRequired == false)
            {
                configButton.Enabled = true;
                scriptButton.Enabled = true;
                RunButton.Enabled = true;
            }
            else
            {
                enableButtonsHandler enButtons = new enableButtonsHandler(enableButtons);
                Invoke(enButtons, new object[] { sender, e });
            }
        }

        public void updateLogsUI(object sender, UpdateLogsArgs e)
        {
            if (this.InvokeRequired == false)
            {
                this.Result.AppendText(e.log);
                this.Result.AppendText("\r\n");
            }
            else
            {
                updateLogsHandler updLogs = new updateLogsHandler(updateLogsUI);
                Invoke(updLogs, new object[] { sender, e });
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
            string command = commandTextBox.Text;
            commandTextBox.Text = "";
            await Task.Run(() => pm.enterCommand(command));
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
