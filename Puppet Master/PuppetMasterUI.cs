using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
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
            Console.WriteLine("Logs: " + pm.getLogs());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            pm.readScriptFile();
            this.Result.Text = pm.getLogs();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            pm.processOneMoreStep();
            this.Result.Text = pm.getLogs();
        }

        private void Result_TextChanged(object sender, EventArgs e)
        {

        }

        private void PuppetMasterUI_Load(object sender, EventArgs e)
        {

        }

        private void RunButton_Click(object sender, EventArgs e)
        {
            pm.enterCommand(commandTextBox.Text);
            commandTextBox.Text = "";
            this.Result.Text = pm.getLogs();
        }

        private void PuppetMasterUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            pm.shutDownAll();
        }

    }
}
