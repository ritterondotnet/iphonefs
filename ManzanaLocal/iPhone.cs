// Software License Agreement (BSD License)
// 
// Copyright (c) 2007, Peter Dennis Bartok <PeterDennisBartok@gmail.com>
// All rights reserved.
// 
// Redistribution and use of this software in source and binary forms, with or without modification, are
// permitted provided that the following conditions are met:
// 
// * Redistributions of source code must retain the above
//   copyright notice, this list of conditions and the
//   following disclaimer.
// 
// * Redistributions in binary form must reproduce the above
//   copyright notice, this list of conditions and the
//   following disclaimer in the documentation and/or other
//   materials provided with the distribution.
// 
// * Neither the name of Peter Dennis Bartok nor the names of its
//   contributors may be used to endorse or promote products
//   derived from this software without specific prior
//   written permission of Yahoo! Inc.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED
// WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A
// PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
// ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR
// TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
// ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// 

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Manzana
{
	/// <summary>
	/// Exposes access to the Apple iPhone
	/// </summary>
	public class iPhone {
		#region Locals
		private DeviceNotificationCallback			dnc;
		private DeviceRestoreNotificationCallback	drn1;
		private DeviceRestoreNotificationCallback	drn2;
		private DeviceRestoreNotificationCallback	drn3;
		private DeviceRestoreNotificationCallback	drn4;

		internal AMDevice	iPhoneHandle;
		internal IntPtr		hAFC;
		private bool		connected;
		private string		current_directory;
		#endregion	// Locals

		#region Constructors
		/// <summary>
		/// Creates a new iPhone object. If an iPhone is connected to the computer, a connection will automatically be opened.
		/// </summary>
		public iPhone () {
			AMDeviceNotification notification;
			int ret = 0;

			dnc = new DeviceNotificationCallback(NotifyCallback);
			drn1 = new DeviceRestoreNotificationCallback(DfuConnectCallback);
			drn2 = new DeviceRestoreNotificationCallback(RecoveryConnectCallback);
			drn3 = new DeviceRestoreNotificationCallback(DfuDisconnectCallback);
			drn4 = new DeviceRestoreNotificationCallback(RecoveryDisconnectCallback);

			notification = new AMDeviceNotification();
			ret = MobileDevice.AMDeviceNotificationSubscribe(dnc, 0, 0, 0, ref notification);
			if (ret != 0) {
				throw new Exception("AMDeviceNotificationSubscribe failed with error " + ret);
			}

			ret = MobileDevice.AMRestoreRegisterForDeviceNotifications(drn1, drn2, drn3, drn4, 0, IntPtr.Zero);
			if (ret != 0) {
				throw new Exception("AMRestoreRegisterForDeviceNotifications failed with error " + ret);
			}
			current_directory = "/";
		}
		#endregion	// Constructors

		#region Properties
		/// <summary>
		/// Gets the current activation state of the phone
		/// </summary>
		public string ActivationState {
			get {
				return MobileDevice.AMDeviceCopyValue(ref iPhoneHandle, 0, "ActivationState");
			}
		}

		/// <summary>
		/// Returns true if an iPhone is connected to the computer
		/// </summary>
		public bool IsConnected {
			get {
				return connected;
			}
		}

		/// <summary>
		/// Returns the Device information about the connected iPhone
		/// </summary>
		public AMDevice Device {
			get {
				return this.iPhoneHandle;
			}
		}

		/// <summary>
		/// Returns the handle to the iPhone com.apple.afc service
		/// </summary>
		public IntPtr AFCHandle {
			get {
				return this.hAFC;
			}
		}

		/// <summary>
		/// Gets/Sets the current working directory, used by all file and directory methods
		/// </summary>
		public string CurrentDirectory {
			get {
				return current_directory;
			}

			set {
				current_directory = value;
			}
		}
		#endregion	// Properties

		#region Events
		/// <summary>
		/// The <c>Connect</c> event is triggered when a iPhone is connected to the computer
		/// </summary>
		public event ConnectEventHandler Connect;

		/// <summary>
		/// Raises the <see>Connect</see> event.
		/// </summary>
		/// <param name="args">A <see cref="ConnectEventArgs"/> that contains the event data.</param>
		protected void OnConnect(ConnectEventArgs args) {
			ConnectEventHandler handler = Connect;

			if (handler != null) {
				handler(this, args);
			}
		}

		/// <summary>
		/// The <c>Disconnect</c> event is triggered when the iPhone is disconnected from the computer
		/// </summary>
		public event ConnectEventHandler Disconnect;

		/// <summary>
		/// Raises the <see>Disconnect</see> event.
		/// </summary>
		/// <param name="args">A <see cref="ConnectEventArgs"/> that contains the event data.</param>
		protected void OnDisconnect(ConnectEventArgs args) {
			ConnectEventHandler handler = Disconnect;

			if (handler != null) {
				handler(this, args);
			}
		}

		/// <summary>
		/// Write Me
		/// </summary>
		public event EventHandler DfuConnect;

		/// <summary>
		/// Raises the <see>DfuConnect</see> event.
		/// </summary>
		/// <param name="args">A <see cref="DeviceNotificationEventArgs"/> that contains the event data.</param>
		protected void OnDfuConnect(DeviceNotificationEventArgs args) {
			EventHandler handler = DfuConnect;

			if (handler != null) {
				handler(this, args);
			}
		}

		/// <summary>
		/// Write Me
		/// </summary>
		public event EventHandler DfuDisconnect;

		/// <summary>
		/// Raises the <see>DfiDisconnect</see> event.
		/// </summary>
		/// <param name="args">A <see cref="DeviceNotificationEventArgs"/> that contains the event data.</param>
		protected void OnDfuDisconnect(DeviceNotificationEventArgs args) {
			EventHandler handler = DfuDisconnect;

			if (handler != null) {
				handler(this, args);
			}
		}

		/// <summary>
		/// The RecoveryModeEnter event is triggered when the attached iPhone enters Recovery Mode
		/// </summary>
		public event EventHandler RecoveryModeEnter;

		/// <summary>
		/// Raises the <see>RecoveryModeEnter</see> event.
		/// </summary>
		/// <param name="args">A <see cref="DeviceNotificationEventArgs"/> that contains the event data.</param>
		protected void OnRecoveryModeEnter(DeviceNotificationEventArgs args) {
			EventHandler handler = RecoveryModeEnter;

			if (handler != null) {
				handler(this, args);
			}
		}

		/// <summary>
		/// The RecoveryModeLeave event is triggered when the attached iPhone leaves Recovery Mode
		/// </summary>
		public event EventHandler RecoveryModeLeave;

		/// <summary>
		/// Raises the <see>RecoveryModeLeave</see> event.
		/// </summary>
		/// <param name="args">A <see cref="DeviceNotificationEventArgs"/> that contains the event data.</param>
		protected void OnRecoveryModeLeave(DeviceNotificationEventArgs args) {
			EventHandler handler = RecoveryModeLeave;

			if (handler != null) {
				handler(this, args);
			}
		}

		#endregion	// Events

		#region Filesystem
		/// <summary>
		/// Returns the names of files in a specified directory
		/// </summary>
		/// <param name="path">The directory from which to retrieve the files.</param>
		/// <returns>A <c>String</c> array of file names in the specified directory. Names are relative to the provided directory</returns>
		public string[] GetFiles(string path) {
            CleanPath(ref path);
			IntPtr			hAFCDir;
			string			buffer;
			ArrayList		paths;
			string			full_path;

			if (!connected) {
				throw new Exception("Not connected to phone");
			}

			hAFCDir = new IntPtr();
			full_path = FullPath(current_directory, path);

			if (MobileDevice.AFCDirectoryOpen(hAFC, full_path, ref hAFCDir) != 0) {
				throw new Exception("Path does not exist");
			}

			buffer = null;
			paths = new ArrayList();
			MobileDevice.AFCDirectoryRead(hAFC, hAFCDir, ref buffer);

			while (buffer!=null) {
				if (!IsDirectory(FullPath(full_path, buffer))) {
					paths.Add(buffer);
				}
				MobileDevice.AFCDirectoryRead(hAFC, hAFCDir, ref buffer);
			}
			MobileDevice.AFCDirectoryClose(hAFC, hAFCDir);
			return (string[])paths.ToArray(typeof(string));
		}

        public struct AMCFileInfo
        {
            public int size;
            public int blocks;
            public bool isDirectory;
            public string ifmt;
        }
		/// <summary>
		/// Returns the size and type of the specified file or directory.
		/// </summary>
		/// <param name="path">The file or directory for which to retrieve information.</param>
		/// <param name="size">Returns the size of the specified file or directory</param>
		/// <param name="directory">Returns <c>true</c> if the given path describes a directory, false if it is a file.</param>
		public AMCFileInfo GetFileInfo(string path) {
            CleanPath(ref path);
            IntPtr key;
            IntPtr val;
            IntPtr dict = IntPtr.Zero;
            string skey;
            string sval;
            int ret;
            AMCFileInfo fi = new AMCFileInfo();
            ret = MobileDevice.AFCFileInfoOpen(hAFC, path, out dict);
            if (ret == 0)
            {
                try  {
                    while ((MobileDevice.AFCKeyValueRead(dict,out key , out val) == 0) && (skey = Marshal.PtrToStringAnsi(key)) != null && (sval = Marshal.PtrToStringAnsi(val)) != null) {
                        switch (skey)
                        {
                            case "st_size":
                                fi.size = int.Parse(sval);
                                break;
                            case "st_blocks":
                                fi.blocks = int.Parse(sval);
                                break;
                            case "st_ifmt":
                                fi.ifmt = sval;
                                if(sval == "S_IFDIR") fi.isDirectory = true;
                                break;
                        }
                    }
                }
                catch (AccessViolationException ave) { }
                finally
                {
                    MobileDevice.AFCKeyValueClose(dict);
                }
            }
            return fi;
		}

		/// <summary>
		/// Returns the size of the specified file or directory.
		/// </summary>
		/// <param name="path">The file or directory for which to obtain the size.</param>
		/// <returns></returns>
		public int FileSize(string path) {
            CleanPath(ref path);
            AMCFileInfo fi = GetFileInfo(path);
            return fi.size;
		}

		/// <summary>
		/// Creates the directory specified in path
		/// </summary>
		/// <param name="path">The directory path to create</param>
		/// <returns>true if directory was created</returns>
		public bool CreateDirectory(string path) {
            CleanPath(ref path);
			string full_path;

			full_path = FullPath(current_directory, path);
			if (MobileDevice.AFCDirectoryCreate(hAFC, full_path) != 0) {
				return false;
			}

			return true;
		}

		/// <summary>
		/// Gets the names of subdirectories in a specified directory.
		/// </summary>
		/// <param name="path">The path for which an array of subdirectory names is returned.</param>
		/// <returns>An array of type <c>String</c> containing the names of subdirectories in <c>path</c>.</returns>
		public string[] GetDirectories(string path) {
            CleanPath(ref path);
			IntPtr hAFCDir;
			string buffer;
			ArrayList paths;
			string full_path;

			if (!connected) {
				throw new Exception("Not connected to phone");
			}

			hAFCDir = new IntPtr();
			full_path = FullPath(current_directory, path);

			if (MobileDevice.AFCDirectoryOpen(hAFC, full_path, ref hAFCDir) != 0) {
				throw new Exception("Path does not exist");
			}

			buffer = null;
			paths = new ArrayList();
			MobileDevice.AFCDirectoryRead(hAFC, hAFCDir, ref buffer);

			while (buffer!=null) {
				if ((buffer != ".") && (buffer != "..") && IsDirectory(FullPath(full_path, buffer))) {
					paths.Add(buffer);
				}
				MobileDevice.AFCDirectoryRead(hAFC, hAFCDir, ref buffer);
			}
			MobileDevice.AFCDirectoryClose(hAFC, hAFCDir);
			return (string[])paths.ToArray(typeof(string));
		}

		/// <summary>
		/// Moves a file or a directory and its contents to a new location or renames a file or directory if the old and new parent path matches.
		/// </summary>
		/// <param name="sourceName">The path of the file or directory to move or rename.</param>
		/// <param name="destName">The path to the new location for <c>sourceName</c>.</param>
		///	<remarks>Files cannot be removed across filesystem boundaries.</remarks>
		public void Rename(string sourceName, string destName) {
            CleanPath(ref sourceName);
            CleanPath(ref destName);
			MobileDevice.AFCRenamePath(hAFC, FullPath(current_directory, sourceName), FullPath(current_directory, destName));
		}

		/// <summary>
		/// FIXME
		/// </summary>
		/// <param name="sourceName"></param>
		/// <param name="destName"></param>
		public void Copy(string sourceName, string destName) {
			
		}
        public void Move(string sourceName, string destName)
        {
            CleanPath(ref sourceName);
            CleanPath(ref destName);
            MobileDevice.AFCRenamePath(hAFC, sourceName, destName);
        }
		/// <summary>
		/// Returns the root information for the specified path. 
		/// </summary>
		/// <param name="path">The path of a file or directory.</param>
		/// <returns>A string containing the root information for the specified path. </returns>
		public string GetDirectoryRoot(string path) {
			return "/";
		}

		/// <summary>
		/// Determines whether the given path refers to an existing file or directory on the phone. 
		/// </summary>
		/// <param name="path">The path to test.</param>
		/// <returns><c>true</c> if path refers to an existing file or directory, otherwise <c>false</c>.</returns>
		public bool Exists(string path) {
            CleanPath(ref path);
			IntPtr	data;

			data = IntPtr.Zero;
            //FIXME: memory errors!
            try
            {
                int ret = MobileDevice.AFCFileInfoOpen(hAFC, path, out data);
                if (ret != 0)
                {
                    return false;
                }
            }
            catch { }

			return true;
		}

		/// <summary>
		/// Determines whether the given path refers to an existing directory on the phone. 
		/// </summary>
		/// <param name="path">The path to test.</param>
		/// <returns><c>true</c> if path refers to an existing directory, otherwise <c>false</c>.</returns>
		public bool IsDirectory(string path) {
            CleanPath(ref path);
            AMCFileInfo fi = GetFileInfo(path);
			return fi.isDirectory;
		}

		/// <summary>
		/// Deletes an empty directory from a specified path.
		/// </summary>
		/// <param name="path">The name of the empty directory to remove. This directory must be writable and empty.</param>
		public void DeleteDirectory(string path) {
            CleanPath(ref path);
			string full_path;

			full_path = FullPath(current_directory, path);
			if (IsDirectory(full_path)) {
				MobileDevice.AFCRemovePath(hAFC, full_path);
			}
		}

		/// <summary>
		/// Deletes the specified directory and, if indicated, any subdirectories in the directory.
		/// </summary>
		/// <param name="path">The name of the directory to remove.</param>
		/// <param name="recursive"><c>true</c> to remove directories, subdirectories, and files in path; otherwise, <c>false</c>. </param>
		public void DeleteDirectory(string path, bool recursive) {
            CleanPath(ref path);
			string full_path;

			if (!recursive) {
				DeleteDirectory(path);
				return;
			}

			full_path = FullPath(current_directory, path);
			if (IsDirectory(full_path)) {
				InternalDeleteDirectory(path);
			}
				
		}

		/// <summary>
		/// Deletes the specified file.
		/// </summary>
		/// <param name="path">The name of the file to remove.</param>
		public void DeleteFile(string path) {
            CleanPath(ref path);
			string full_path;

			full_path = FullPath(current_directory, path);
			if (Exists(full_path)) {
				MobileDevice.AFCRemovePath(hAFC, full_path);
			}
		}

		/// <summary>
		/// Gets the current working directory of the object. 
		/// </summary>
		/// <returns>A <c>string</c> containing the path of the current working directory. </returns>
		public string GetCurrentDirectory() {
			return current_directory;
		}
        public struct AFCDeviceInfo
        {
            public string Model;
            public long FileSystemTotalBytes;
            public long FileSystemFreeBytes;
            public int FileSystemBlockSize;
        }
        public AFCDeviceInfo GetDeviceInfo()
        {
            IntPtr key;
            IntPtr val;
            IntPtr dict;
            string skey;
            string sval;
            long lval;
            int ival;
            dict = IntPtr.Zero;
            AFCDeviceInfo di = new AFCDeviceInfo();
                
            int ret = MobileDevice.AFCDeviceInfoOpen(hAFC, ref dict);
            if (ret == 0)
            {
                try
                {
                    while ((MobileDevice.AFCKeyValueRead(dict, out key, out val) == 0) && (skey = Marshal.PtrToStringAnsi(key)) != null && (sval = Marshal.PtrToStringAnsi(val)) != null)
                    {
                        switch (skey)
                        {
                            case "Model":
                                di.Model = sval;
                                break;
                            case "FSFreeBytes":
                                long.TryParse(sval,out lval);
                                di.FileSystemFreeBytes = lval;
                                break;
                            case "FSTotalBytes":
                                long.TryParse(sval, out lval);
                                di.FileSystemTotalBytes = lval;
                                break;
                            case "FSBlockSize":
                                Int32.TryParse(sval, out ival);
                                di.FileSystemBlockSize = ival;
                                break;
                            default:
                                System.Diagnostics.Trace.WriteLine(skey + ":" + sval);
                                break;
                        }
                    }
                }
                catch (AccessViolationException ave) { }
                finally
                {
                    MobileDevice.AFCKeyValueClose(dict);
                }
            } 
            return di;
        }
		/// <summary>
		/// Sets the application's current working directory to the specified directory.
		/// </summary>
		/// <param name="path">The path to which the current working directory should be set.</param>
		public void SetCurrentDirectory(string path) {
            CleanPath(ref path);
			string new_path;

			new_path = FullPath(current_directory, path);
			if (!IsDirectory(new_path)) {
				throw new Exception("Invalid directory specified");
			}
			current_directory = new_path;
		}
		#endregion	// Filesystem

		#region Private Methods
		private bool ConnectToPhone() {
			if (MobileDevice.AMDeviceConnect(ref iPhoneHandle) == 1) {
				//int connid;

				throw new Exception("Phone in recovery mode, support not yet implemented");
				//connid = MobileDevice.AMDeviceGetConnectionID(ref iPhoneHandle);
				//MobileDevice.AMRestoreModeDeviceCreate(0, connid, 0);
				//return false;
			}
			if (MobileDevice.AMDeviceIsPaired(ref iPhoneHandle) == 0) {
				return false;
			}

			if (MobileDevice.AMDeviceValidatePairing(ref iPhoneHandle) != 0) {
				return false;
			}

			if (MobileDevice.AMDeviceStartSession(ref iPhoneHandle) == 1) {
				return false;
			}
            //FIXME:AFC2 service won't work?
			if (MobileDevice.AMDeviceStartService(ref iPhoneHandle, MobileDevice.StringToCFString("com.apple.afc2"), ref hAFC, IntPtr.Zero) != 0) {
				if (MobileDevice.AMDeviceStartService(ref iPhoneHandle, MobileDevice.StringToCFString("com.apple.afc"), ref hAFC, IntPtr.Zero) != 0) {
					return false;
				}
			}

			if (MobileDevice.AFCConnectionOpen(hAFC, 0, ref hAFC) != 0) {
				return false;
			}

			connected = true;
			return true;
		}

		private void NotifyCallback(ref AMDeviceNotificationCallbackInfo callback) {
			if (callback.msg == NotificationMessage.Connected) {
				iPhoneHandle = callback.dev;
				if (ConnectToPhone()) {
					OnConnect(new ConnectEventArgs(callback));
				}
			} else if (callback.msg == NotificationMessage.Disconnected) {
				connected = false;
				OnDisconnect(new ConnectEventArgs(callback));
			}
		}

		private void DfuConnectCallback(ref AMRecoveryDevice callback) {
			OnDfuConnect(new DeviceNotificationEventArgs(callback));
		}

		private void DfuDisconnectCallback(ref AMRecoveryDevice callback) {
			OnDfuDisconnect(new DeviceNotificationEventArgs(callback));
		}

		private void RecoveryConnectCallback(ref AMRecoveryDevice callback) {
			OnRecoveryModeEnter(new DeviceNotificationEventArgs(callback));
		}

		private void RecoveryDisconnectCallback(ref AMRecoveryDevice callback) {
			OnRecoveryModeLeave(new DeviceNotificationEventArgs(callback));
		}

		private void InternalDeleteDirectory(string path) {
            CleanPath(ref path);
			string full_path;
			string[] contents;

			full_path = FullPath(current_directory, path);
			contents = GetFiles(path);
			for (int i = 0; i < contents.Length; i++) {
				DeleteFile(full_path + "/" + contents[i]);
			}

			contents = GetDirectories(path);
			for (int i = 0; i < contents.Length; i++) {
				InternalDeleteDirectory(full_path + "/" + contents[i]);
			}

			DeleteDirectory(path);
		}

		static char[] path_separators = { '/' };
        internal void CleanPath(ref string path)
        {
            path = path.Replace(@"\", "/");
            while (path.Contains("//")) { path = path.Replace("//", "/"); };
            if ((path == null) || (path == String.Empty)) {
				path = "/";
			}
        }
		internal string FullPath(string path1, string path2) {
			string[]		path_parts;
			string[]		result_parts;
			int				target_index;

			if ((path1 == null) || (path1 == String.Empty)) {
				path1 = "/";
			}

			if ((path2 == null) || (path2 == String.Empty)) {
				path2 = "/";
			}

			if (path2[0] == '/') {
				path_parts = path2.Split(path_separators);
			} else if (path1[0] == '/') {
				path_parts = (path1 + "/" + path2).Split(path_separators);
			} else {
				path_parts = ("/" + path1 + "/" + path2).Split(path_separators);
			}
			result_parts = new string[path_parts.Length];
			target_index = 0;

			for (int i = 0; i < path_parts.Length; i++) {
				if (path_parts[i] == "..") {
					if (target_index > 0) {
						target_index--;
					}
				} else if ((path_parts[i] == ".") || (path_parts[i] == "")) {
					// Do nothing
				} else {
					result_parts[target_index++] = path_parts[i];
				}
			}

			return "/" + String.Join("/", result_parts, 0, target_index);
		}
		#endregion	// Private Methods
	}
}
