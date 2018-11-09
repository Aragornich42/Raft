using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Raft
{
    public partial class MsgBox : Form
    {
        public MsgBox()
        {
            InitializeComponent();
        }

        public MsgBox(bool flag)
        {
            InitializeComponent();
            if (flag)
                label1.Text = "Плот сидит на мели";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
