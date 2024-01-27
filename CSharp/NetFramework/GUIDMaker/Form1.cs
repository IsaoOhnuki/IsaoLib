using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
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
            textBox2.Text += sb.ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox2.Text.Length > 0)
            {
                Clipboard.SetText(textBox2.Text);
            }
        }

        [Flags]
        enum MSIINSTALLCONTEXT
        {
            MSIINSTALLCONTEXT_NONE = 0,
            MSIINSTALLCONTEXT_USERMANAGED = 1,
            MSIINSTALLCONTEXT_USERUNMANAGED = 2,
            MSIINSTALLCONTEXT_MACHINE = 4,
            MSIINSTALLCONTEXT_ALL = MSIINSTALLCONTEXT_USERMANAGED | MSIINSTALLCONTEXT_USERUNMANAGED | MSIINSTALLCONTEXT_MACHINE,
        }

        enum MSIGETERROR
        {
            ERROR_SUCCESS = 0,
            ERROR_ACCESS_DENIED = 5,
            ERROR_INVALID_PARAMETER = 87,
            ERROR_MORE_DATA = 234,
            ERROR_NO_MORE_ITEMS = 259,
            ERROR_BAD_CONFIGURATION = 1610,
            ERROR_FUNCTION_FAILED = 1627,

            INSTALLSTATE_NOTUSED,
            INSTALLSTATE_BADCONFIG,
            INSTALLSTATE_ABSENT,
            INSTALLSTATE_INVALIDARG,
            INSTALLSTATE_LOCAL,
            INSTALLSTATE_SOURCE,
            INSTALLSTATE_SOURCEABSENT,
            INSTALLSTATE_UNKNOWN,
            INSTALLSTATE_BROKEN,
        }

        static string Everyone = "s-1-1-0";

        [DllImport("Msi.dll")]
        static extern uint MsiEnumClientsExW(
            string szComponent,
            string szUserSid,
            MSIINSTALLCONTEXT dwContext,
            uint dwProductIndex,
            out string szInstalledComponentCode,
            out MSIINSTALLCONTEXT pdwInstalledContext,
            out string szSid,
            ref uint pcchSid);

        [DllImport("Msi.dll")]
        static extern uint MsiEnumComponentsExW(
            string szUserSid,
            MSIINSTALLCONTEXT dwContext,
            uint dwIndex,
            out string szInstalledComponentCode,
            out MSIINSTALLCONTEXT pdwInstalledContext,
            out string szSid,
            ref UIntPtr pcchSid);

        [DllImport("Msi.dll")]
        static extern uint MsiGetComponentPathExW(string szProductCode, string szComponentCode, string szUserSid,
            MSIINSTALLCONTEXT dwContext,
            out string lpOutPathBuffer,
            ref string pcchOutPathBuffer);

        private void button3_Click(object sender, EventArgs e)
        {
            UIntPtr pcchSid = UIntPtr.Zero;
            for (uint i = 0; i < uint.MaxValue; i++)
            {
                uint ret = MsiEnumComponentsExW(
                    Everyone,
                    MSIINSTALLCONTEXT.MSIINSTALLCONTEXT_ALL,
                    i,
                    out string szInstalledComponentCode,
                    out MSIINSTALLCONTEXT pdwInstalledContext,
                    out string szSid,
                    ref pcchSid);
                if (ret != (uint)MSIGETERROR.ERROR_SUCCESS)
                {
                    break;
                }
            }
        }
    }
}
