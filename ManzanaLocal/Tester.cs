using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Manzana;
using System.Runtime.InteropServices;
namespace ManzanaLocal
{
    public partial class Tester : Form
    {
        iPhone phone;
        public Tester()
        {
            InitializeComponent();
        }

        private void Tester_Load(object sender, EventArgs e)
        {

            //afc_dictionary info = new afc_dictionary();
            phone = new iPhone();
            phone.Connect += new ConnectEventHandler(phone_Connect);
        }

        void phone_Connect(object sender, ConnectEventArgs args)
        {
            int a;
            bool b;
            //phone.GetFileInfo("/", out a,out  b);
            IntPtr dict;
            string key;
            string val;
            int ret;

            dict = IntPtr.Zero;

            ret = MobileDevice.AFCDeviceInfoOpen(phone.AFCHandle, ref dict);
            if (ret == 0)
            {
                //MobileDevice.AFCKeyValueRead(dict, out key, out val);
                //while (key != null && val != null)
                //{
                //    System.Diagnostics.Trace.WriteLine(key + ":" + val);
                //    MobileDevice.AFCKeyValueRead(dict, out key, out val);
                //}
                //MobileDevice.AFCKeyValueClose(dict);
            }
        }
    }
}