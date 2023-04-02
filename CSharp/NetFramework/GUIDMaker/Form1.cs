using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GUIDMaker
{
    public partial class Form1 : Form
    {
        public string Header => textBox1.Text;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 10; )
            {
                Guid guid = Guid.NewGuid();
                string header = Header;
                string array = new string(guid.ToString().Where(x => x != '-').ToArray());
                if (header.Length == 0 ||
                    header.Select((c, idx) => (c, idx)).All(x => array[x.idx] == x.c))
                {
                    sb.AppendLine("// " + guid.ToString());
                    i++;
                }
            }
            textBox2.Text = sb.ToString();
        }
    }
}
