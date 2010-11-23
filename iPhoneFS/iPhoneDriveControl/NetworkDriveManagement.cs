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

/*--------------------------------------------------------------------*\
 * Some code Copyright 2007 Richard.Heinrich@palissimo.de             *
\*--------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
namespace iPhoneDriveControl
{
    class NetworkDriveManagement
    {
        private const int RESOURCETYPE_ANY = 0x1;
        private const int CONNECT_INTERACTIVE = 0x00000008;
        private const int CONNECT_PROMPT = 0x00000010;
        [DllImport("mpr.dll", EntryPoint = "WNetAddConnection2W", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        private static extern int WNetAddConnection2(ref NETRESOURCE lpNetResource, string lpPassword, string lpUsername, Int32 dwFlags);
        [DllImport("mpr.dll", EntryPoint = "WNetCancelConnection2W", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        private static extern int WNetCancelConnection2(string lpName, Int32 dwFlags,bool fForce);
        [DllImport("shell32.dll", EntryPoint = "ShellExecuteW", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        private static extern int ShellExecuteW(long hwnd, string lpOperation, string lpFile, string lpParameters, string lpDirectory, long nShowCmd);
    
        [StructLayout(LayoutKind.Sequential)]
        internal struct NETRESOURCE {
            public int dwScope;
            public int dwType;
            public int dwDisplayType;
            public int dwUsage;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpLocalName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpRemoteName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpComment;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpProvider;
        }
        public static string NextDrive()
        {
            Dictionary<string,string> drives = new Dictionary<string,string>();
            DriveInfo[] di = System.IO.DriveInfo.GetDrives();
            foreach (DriveInfo d in di)
            {
                drives.Add(d.Name.ToUpper().TrimEnd("\\/".ToCharArray()), d.Name.ToUpper().TrimEnd("\\/".ToCharArray()));
            }
            for(int i = 67;i<90;i++)
            {
                string drive = ((char)i) + ":";
                if (!drives.ContainsKey(drive.ToUpper()))
                {
                    return drive.ToUpper();
                }
            }
            throw new SystemException("No drives available!");
        }
        public static int MapDrive(string driveletter, string unc)
        {
            NETRESOURCE ConnInf = new NETRESOURCE();
            ConnInf.dwScope = 0;
            ConnInf.dwType = RESOURCETYPE_ANY;
            ConnInf.dwDisplayType = 0;
            ConnInf.dwUsage = 0;
            ConnInf.lpLocalName = driveletter;
            ConnInf.lpRemoteName = unc;
            ConnInf.lpComment = null;
            ConnInf.lpProvider = null;
            int ret = WNetAddConnection2(ref ConnInf, null, null,0);
            if (ret != 0)
                throw new IOException("An error occured mapping the resource.  The error code is: " + ret);
            return ret;
        }
        public static int UnmapDrive(string driveletter)
        {
            int dwFlags = 0;
            int ret = WNetCancelConnection2(driveletter, dwFlags,true);
            if (ret != 0)
                throw new IOException("An error occured unmapping the resource.  The error code is: " + ret);
            return ret;
        }
    }
}
