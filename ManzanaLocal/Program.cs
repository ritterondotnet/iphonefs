/*---------------------------------------------------------------------------*\
* Copyright (C) 2007-2011 Lokkju, Inc <lokkju@lokkju.com>                     *
*                                                                             *
* This program is free software; you can redistribute it and/or modify it     *
* under the terms of the GNU General Public License as published by the Free  *
* Software Foundation; either version 3 of the License, or (at your option)   *
* any later version.                                                          *
*                                                                             *
* This program is distributed in the hope that it will be useful, but WITHOUT *
* ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or       *
* FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for    *
* more details.                                                               *
* You should have received a copy of the GNU General Public License along     *
* with this program; if not, see <http://www.gnu.org/licenses>.               *
*                                                                             *
* Additional permission under GNU GPL version 3 section 7:                    *
* If you modify this Program, or any covered work, by linking or combining it *
* with the NeoGeo SMB library, or a modified version of that library,         *
* the licensors of this Program grant you additional permission to convey the *
* resulting work as long as the library is distributed without fee.           *
*-----------------------------------------------------------------------------*
* @category   iPhone                                                          *
* @package    iPhone File System for Windows                                  *
* @copyright  Copyright (c) 2010 Lokkju Inc. (http://www.lokkju.com)          *
* @license    http://www.gnu.org/licenses/gpl-3.0.txt GNU v3 Licence          *
*                                                                             *
* $Revision::                                     $:  Revision of last commit *
* $Author::                                         $:  Author of last commit *
* $Date::                                             $:  Date of last commit *
* $Id::                                                                     $ *
\*---------------------------------------------------------------------------*/

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