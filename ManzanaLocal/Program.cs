/*--------------------------------------------------------------------*\
 * This source file is subject to the new BSD license that is bundled *
 * with this package in the file LICENSE.txt.                         *
 * It is also available through the world-wide-web at this URL:       *
 * http://www.lokkju.com/license/gnu2                                 *
 * If you did not receive a copy of the license and are unable to     *
 * obtain it through the world-wide-web, please send an email         *
 * to bsd-license@lokkju.com so we can send you a copy immediately.   *
 *                                                                    *
 * @category   iPhone                                                 *
 * @package    iPhone Music Manager                                   *
 * @copyright  Copyright (c) 2007 Lokkju Inc. (http://www.lokkju.com) *
 * @license    http://www.lokkju.com/license/gnu2 GNU v2 License      *
 *                                                                    *
 * $Revision:: 12                      $:  Revision of last commit    *
 * $Author:: lokkju                    $:  Author of last commit      *
 * $Date:: 2007-08-24 11:14:31 -0700 (#$:  Date of last commit        *
 * $Id:: Program.cs 12 2007-08-24 18:14:31Z lokkju                  $ *
\*--------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
namespace ManzanaLocal
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            while(!ensureDllExists("iTunesMobileDevice.dll")){}
            Application.Run(new ManzanaLocal.Tester());
        }
        static bool ensureDllExists(string filename)
        {
            if (File.Exists(filename))
            {
                return true;
            } else {
                DialogResult res = MessageBox.Show("You are missing iTunesMobileDevice.dll!\nDo you want to locate a copy?", "Fatal Error!", MessageBoxButtons.YesNo, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                if (res == DialogResult.No)
                {
                    System.Environment.Exit(0);
                }
                else
                {
                    OpenFileDialog fd = new OpenFileDialog();
                    fd.CheckFileExists = true;
                    fd.FileName = filename;
                    fd.Filter = filename + "|" + filename;
                    fd.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles) + @"\Apple\Mobile Device Support\bin\";
                    fd.Title = "Select a " + filename + " File";
                    res = fd.ShowDialog();
                    if (res == DialogResult.OK)
                    {
                        try
                        {
                            if (Path.GetFileName(fd.FileName) == filename)
                            {
                                File.Copy(fd.FileName,Path.GetDirectoryName(Application.ExecutablePath) + @"\" + filename);
                                return true;
                            }
                        }
                        catch { }
                    }
                }
                return false;
            }
        }
    }
}