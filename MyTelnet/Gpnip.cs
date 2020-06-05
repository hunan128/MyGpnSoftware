using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyGpnSoftware
{
    [Serializable]
    class Gpnip
    {
        
        private string gpnIP;
        public string GpnIP
        {
            get { return gpnIP; }
            set { gpnIP = value; }
        }
    }
}
