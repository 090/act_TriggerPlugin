using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TriggerTrigger
{
    public partial class Log : Form
    {
        bool _on;
        public Log()
        {
            InitializeComponent();
            _on = false;
        }

        private void Log_FormClosing(object sender, FormClosingEventArgs e)
        {
            _on = false;
            e.Cancel = true;
            this.Visible = false;
            richTextBoxLog.Clear();
        }

        public void Add(string str)
        {
            if ( !_on)
                return;
            richTextBoxLog.AppendText(str.Trim() + Environment.NewLine);

#if DEBUG
            System.Diagnostics.Debug.WriteLine(str.Trim());
#endif

        }
        public void On()
        {
            _on = true;
        }
        public void Off()
        {
            _on = false;
        }
        private void Log_VisibleChanged(object sender, EventArgs e)
        {
            _on = Visible;
        }

    }
}
