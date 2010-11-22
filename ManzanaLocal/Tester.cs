/*--------------------------------------------------------------------*\
 * This source file is subject to the GPLv3 license that is bundled   *
 * with this package in the file COPYING.                             *
 * It is also available through the world-wide-web at this URL:       *
 * http://www.gnu.org/licenses/gpl-3.0.txt                            *
 * If you did not receive a copy of the license and are unable to     *
 * obtain it through the world-wide-web, please send an email         *
 * to bsd-license@lokkju.com so we can send you a copy immediately.   *
 *                                                                    *
 * @category   iPhone                                                 *
 * @package    iPhone File System for Windows                         *
 * @copyright  Copyright (c) 2010 Lokkju Inc. (http://www.lokkju.com) *
 * @license    http://www.gnu.org/licenses/gpl-3.0.txt GNU v3 Licence *
 *                                                                    *
 * $Revision::                            $:  Revision of last commit *
 * $Author::                              $:  Author of last commit   *
 * $Date::                                $:  Date of last commit     *
 * $Id::                                                            $ *
\*--------------------------------------------------------------------*/
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