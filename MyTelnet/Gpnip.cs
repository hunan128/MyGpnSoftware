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
        private int gpnPRY;

        public int GpnPRY
        {
            get { return gpnPRY; }
            set { gpnPRY = value; }
        }
        private bool gpnZX;

        public bool GpnZX
        {
            get { return gpnZX; }
            set { gpnZX = value; }
        }

    }
}
