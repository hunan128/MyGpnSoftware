using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MyGpnSoftware
{
    public partial class NewPassword : Form
    {
        public NewPassword()
        {
            InitializeComponent();
        }
        public NewPassword(string OldLoginPassword,string OldEnablePassword) : this() {
            textBoxEnablePasswordOld.Text = OldEnablePassword;
            textBoxLoginPasswordOld.Text = OldLoginPassword;
        }
        public string _NewLoginPassword
        {
            get { return textBoxLoginPassword.Text; }
        }
        public string _NewEnablePassword
        {
            get { return textBoxEnablePassword.Text; }
        }
    }
}
