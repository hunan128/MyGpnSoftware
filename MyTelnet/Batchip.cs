using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyGpnSoftware
{
    [Serializable]
    class Batchip
    {



        private string batchip;
        public string BatchIP
        {
            get { return batchip; }
            set { batchip = value; }
        }
        private string pry;
        public string Pry
        {
            get { return pry; }
            set { pry = value; }
        }

    }
}
