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
                throw new IOException("An error occured mapping the resource.  The error code is: " + ret);
            return ret;
        }
    }
}
