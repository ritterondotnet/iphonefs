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
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Configuration.Provider;
using System.Collections.Specialized;

using NeoGeo.Library.SMB.Provider;

using Manzana;
namespace com.lokkju.iphonefs
{

    /// <summary>
    /// Extension of the basis class FileContext
    /// </summary>
    public class MyFileContext : FileContext
    {
        internal Stream FS = null;

        internal MyFileContext(string Name, bool IsDirectory, Stream _FS)
            : base(Name, IsDirectory)
        {
            FS = _FS;
        }
    }

    public class iPhoneFS : FileSystemProvider 
    {
        private iPhone phone;
        public iPhone Phone
        {
            get
            {
                return phone;
            }
        }

        private string root = "/";

        private string _FileSystemProviderType;

        public override string FileSystemProviderType
        {
            get { return _FileSystemProviderType; }
            set { _FileSystemProviderType = value; }
        }

        /// <summary>
        /// The default constructor, in this case very simple, just set the path of the directory to share
        /// </summary>
        public iPhoneFS()
        {
            phone = new iPhone();
        }

        /// <summary>
        /// Used in initialize the filesystem, you can add implementation specific setup data
        /// </summary>
        /// <param name="name"></param>
        /// <param name="config"></param>
        public override void Initialize(string name, NameValueCollection config)
        {
            // Verify that config isn't null
            if (config == null)
                throw new ArgumentNullException("config");

            // Assign the  provider a default name if it doesn't have one
            if (String.IsNullOrEmpty(name))
                name = "iPhoneFSProvider";

            // Add a default "description" attribute to config if the
            // attribute doesn't exist or is empty
            if (string.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "iPhoneFS provider");
            }

            // Call the  base class's Initialize method
            base.Initialize(name, config);

            // Initialize _FileSystemProviderType
            _FileSystemProviderType = config["filesystemprovidertype"];

            if (string.IsNullOrEmpty(_FileSystemProviderType))
                _FileSystemProviderType = this.GetType().ToString();

            config.Remove("filesystemprovidertype");

            //Initialize _mappedPath;
            string MappedPath = config["mappedpath"];
            if (String.IsNullOrEmpty(MappedPath))
                throw new ProviderException("Empty or missing mappedpath");
            root = MappedPath;
          
            config.Remove("mappedpath");

            // Throw an exception if unrecognized attributes remain
            if (config.Count > 0)
            {
                string attr = config.GetKey(0);
                if (!String.IsNullOrEmpty(attr))
                    throw new ProviderException("Unrecognized attribute: " + attr);
            }
        }

        //Implements the search for listings by means of identification FindFirst and FindNext
        public override NT_STATUS ReadDirectory(UserContext UserContext, FileContext FileObject)
        {
            NT_STATUS error = NT_STATUS.OK;

            MyFileContext HE = (MyFileContext)FileObject;

            if (!HE.IsDirectory)
            {
                Debug.WriteLine("Warning->Handle is not a directory, can not get a listing");
                return NT_STATUS.INVALID_HANDLE;						
            }

            if (!phone.Exists(root + HE.Name) || !phone.IsDirectory(root + HE.Name))
                return NT_STATUS.OBJECT_PATH_NOT_FOUND;   // Directroy not found, should never happen

            HE.Items.Add(new DirectoryContext(".", FileAttributes.Directory));
            HE.Items.Add(new DirectoryContext("..", FileAttributes.Directory));

            DirectoryContext Item = null;

            foreach (string DirName in phone.GetDirectories(root + HE.Name))
            {
                error = GetAttributes(UserContext, root + HE.Name + DirName, out Item); 
                if (error != 0)
                    Trace.WriteLine("Warning->Error: '" + error + "' during listing directories: " + HE.Name + DirName);
                HE.Items.Add(Item);
            }

            foreach (string FileName in phone.GetFiles(root + HE.Name))
            {
                error = GetAttributes(UserContext,root + HE.Name + FileName, out Item); 
                if (error != 0)
                    Trace.WriteLine("Warning->Error: '" + error + "' during listing files: " + FileName);
                else
                    HE.Items.Add(Item);
            }

            return NT_STATUS.OK;
        }

        public override NT_STATUS DeleteDirectory(UserContext UserContext, string path)
        {
            if (path == "")
                return NT_STATUS.ACCESS_DENIED;                 // Root directory can not be deleted

            if (!phone.Exists(root + path) || !phone.IsDirectory(path))
                return NT_STATUS.OBJECT_PATH_NOT_FOUND;			// Dir not found

            // FIXME: We have no way of checking for directory or file attributes
            //if ((new DirectoryInfo(root + Path).Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
            //    return NT_STATUS.ACCESS_DENIED;

            try
            {
                phone.DeleteDirectory(root + path, false);
            }
            catch (UnauthorizedAccessException ex)
            {
                Trace.WriteLine("Warning->Error error occured when deleting directory '" + path + "' : " + ex.Message);
                return NT_STATUS.ACCESS_DENIED;     // We know it exists, so it must just be denied
            }
            catch (IOException ex)
            {
                Trace.WriteLine("Warning->Error occured when deleting directory '" + path + "' : " + ex.Message);
                return NT_STATUS.DIRECTORY_NOT_EMPTY;   // Nachdem wir oben schon festgestellt haben dass das Listing da ist,
                // und nicht read-only ist, kann es nur noch nicht leer ist. 
            }
            return NT_STATUS.OK;
        }

        public override NT_STATUS CreateDirectory(UserContext UserContext, string Path, FileAttributes Attributes)
        {
            if (Path.IndexOf("\\") == -1)
                return NT_STATUS.OBJECT_PATH_NOT_FOUND;	// Dir not found as no dir is there

            if (phone.Exists(root + Path))
                return NT_STATUS.OBJECT_NAME_COLLISION;  // File/Directory already exists

            phone.CreateDirectory(root + Path);
            // FIXME: We have no way of dealing with directory or file attributes
            //if (Attributes != FileAttributes.Normal)
            //{
            //    DirectoryInfo DI = new DirectoryInfo(root + Path);
            //    DI.Attributes = Attributes;
            //}
            return NT_STATUS.OK;
        }

        public override NT_STATUS FSInfo(UserContext UserContext, out FileSystemAttributes data)
        {
            // Should be implemented very fast, as this method is called quite often
            // Try to implement is without any I/O or cache the I/O results. 

            base.FSInfo(UserContext, out data);

            data.FSName = "iPhoneFS";

            data.SectorUnit = 1;						    // FreeBytes and TotalBytes will me multiplied by this value
            data.Sectors = 1;							    // FreeBytes and TotalBytes will be multiplied by this value

            //FIXME: cache/decache this info?
            phone.RefreshFileSystemInfo();
            data.FreeBytes = phone.FileSystemFreeBytes;
            data.TotalBytes = phone.FileSystemTotalBytes;
            return NT_STATUS.OK;
        }

        public override NT_STATUS DeviceIO(UserContext UserContext, FileContext FileObject, int Command, bool IsFsctl, ref byte[] Input, ref byte[] Output, ref int ValidOutputLength)
        {
            // We implement some of the usaual command on our own
            //http://wiki.ethereal.com/SMB2/Ioctl/Function/
            switch (Command)
            {
                case 0x00090028: // FSCTL_IS_VOLUME_MOUNTED
                    ValidOutputLength = 0;
                    //FIXME: make FSCTL_IS_VOLUME_MOUNTED return based on if the iPhone is connected
                    if (phone.IsConnected) return NT_STATUS.OK;
                    return NT_STATUS.ACCESS_DENIED;           // Return no error as the Filesystem is here
                default:
                    Trace.WriteLine("Warning->IOCTL is implemented, but this method not: 0x" + Command.ToString("X8"));
                    //Debugger.Break();
                    return NT_STATUS.NOT_IMPLEMENTED;
            }
            //return NT_STATUS.NOT_IMPLEMENTED;
        }

        public override NT_STATUS Close(FileContext FileObject, DateTime LastWriteTime)
        {
            // If you use a write cache, be sure to call flush or any similar method to write the data through. 
            // The close should be done within 20 secounds and after all data is at the final media stored
            MyFileContext hinfo = (MyFileContext)FileObject;

            if (hinfo.FS != null)
                hinfo.FS.Close();
            //FIXME: should we try to set the last write time on files and directories?
            /*
            try
            {
                if (hinfo.IsDirectory)
                    Directory.SetLastWriteTime(root + hinfo.Name, LastWriteTime);
                else
                    FileEx.SetLastWriteTime(root + hinfo.Name, LastWriteTime);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Warning->Beim Setzen der LastWriteTime für '" + hinfo.Name + "' ist in Close eine Exeption aufgetreten: " + ex.Message);
            }
            */
            return NT_STATUS.OK;
        }

        public override NT_STATUS Close(FileContext FileObject)
        {
            MyFileContext hinfo = (MyFileContext)FileObject;

            if (hinfo.FS != null)
                hinfo.FS.Close();

            return NT_STATUS.OK;
        }

        public override NT_STATUS GetService(out string Service, out string NativeFileSystem, out string Comment)
        {
            base.GetService(out Service, out NativeFileSystem, out Comment);

            //FIXME: add definitions for NativeFileSystem and Service
            //Service = "iPhoneDrive";
            NativeFileSystem = "iPhoneFS";
            Comment = "iPhone Drive Mountable Filesystem";
            return NT_STATUS.OK;
        }

        public override NT_STATUS Create(UserContext UserContext, string Name, SearchFlag Flags, FileMode Mode, FileAccess Access, FileShare Share, FileAttributes Attributes, out FileContext FileObject)
        {
            FileObject = null;

            string PathName = root + Name;

            try
            {
                switch (Mode)
                {
                    case FileMode.Open:			// Both work only if the file exists
                    case FileMode.Truncate:
                        switch (Flags)
                        {
                            case SearchFlag.FileAndDir:
                                if (phone.Exists(PathName))
                                {
                                    if (!phone.IsDirectory(PathName))
                                    {
                                        //FIXME: Implement FileMode and FileShare
                                        FileObject = new MyFileContext(Name, false, iPhoneFile.Open(phone, PathName, Access));
                                        return NT_STATUS.OK;
                                    }
                                    else
                                    {
                                        FileObject = new MyFileContext(Name, true, null);
                                        return NT_STATUS.OK;
                                    }
                                }
                                return NT_STATUS.NO_SUCH_FILE;
                            case SearchFlag.File:
                                if (phone.Exists(PathName) && !phone.IsDirectory(PathName))
                                {
                                    //FIXME: Implement FileMode and FileShare
                                    FileObject = new MyFileContext(Name, false, iPhoneFile.Open(phone, PathName, Access));
                                    return NT_STATUS.OK;
                                }
                                return NT_STATUS.NO_SUCH_FILE; ;
                            case SearchFlag.Dir:
                                if (phone.Exists(PathName) && phone.IsDirectory(PathName))
                                {
                                    FileObject = new MyFileContext(Name, true, null);
                                    return NT_STATUS.OK;
                                }
                                return NT_STATUS.OBJECT_PATH_NOT_FOUND;
                            default:
                                return NT_STATUS.INVALID_PARAMETER;
                        }

                    case FileMode.CreateNew:
                        // Works only if the file does not exists
                        if (phone.Exists(PathName))
                            return NT_STATUS.OBJECT_NAME_COLLISION;	// Access denied as it is already there

                        if (Access == FileAccess.Read)              // Office 2003 makes the stupid call of: "CreateNew with Read access", C# refuse to execute it!
                            Access = FileAccess.ReadWrite;
                        //FIXME: Implement FileMode and FileShare
                        FileObject = new MyFileContext(Name, false, iPhoneFile.Open(phone,PathName, Access));
                        return NT_STATUS.OK;

                    case FileMode.Create:
                    case FileMode.OpenOrCreate:
                        // Use existing file if possible otherwise create new
                        FileObject = new MyFileContext(Name, false, iPhoneFile.Open(phone,PathName, Access));
                        return NT_STATUS.OK;
                    default:
                        return NT_STATUS.INVALID_PARAMETER;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Warning->Exception when opening filestream: " + ex.Message);
                return NT_STATUS.NO_SUCH_FILE;
            }

        }

        public override NT_STATUS Rename(UserContext UserContext, string OldName, string NewName)
        {
            NT_STATUS error = NT_STATUS.OK;

            if (phone.Exists(root + OldName) && phone.IsDirectory(root + OldName))
            {
                // We are in Directory case
                if (phone.Exists(root + NewName))
                    return NT_STATUS.OBJECT_NAME_COLLISION;
                try
                {
                    //FIXME: We don't have a way to move directories
                    //DirectoryBroker.Move(phone,root + OldName, root + NewName);
                    phone.Rename(root + OldName, root + NewName);
                    return NT_STATUS.OBJECT_NAME_COLLISION;
                }
                catch (Exception e)
                {
                    Trace.WriteLine("Warning->Exception when renaming directory: " + e.Message);
                    error = (NT_STATUS)Marshal.GetHRForException(e);
                    //error = 3;					// ListingError
                }
            }
            else
            {	// We are in File case
                if (!phone.Exists(root + OldName))
                    return NT_STATUS.OBJECT_NAME_NOT_FOUND;		// Orginalname nicht da
                if (phone.Exists(root + NewName))
                    return NT_STATUS.OBJECT_NAME_COLLISION;
                try
                {
                    phone.Rename(root + OldName, root + NewName);
                }
                catch (Exception e)
                {
                    Trace.WriteLine("Warning->Exception when renaming file: " + e.Message);
                    error = (NT_STATUS)Marshal.GetHRForException(e);
                }
            }
            return error;
        }

        public override NT_STATUS Delete(UserContext UserContext, string FileName)
        {
            NT_STATUS error = NT_STATUS.OK;

            string OriginalName = root + FileName;

            Debug.WriteLine("Info->File '" + FileName + "' with full path '" + OriginalName + "' is being deleted.");

            try
            {
                if (!phone.Exists(OriginalName) || phone.IsDirectory(OriginalName))
                    return NT_STATUS.OBJECT_NAME_NOT_FOUND;
                phone.DeleteFile(OriginalName);
            }
            catch (Exception e)
            {
                Trace.WriteLine("Warning->Exception in Delete:" + e.Message);
                error = (NT_STATUS)Marshal.GetHRForException(e);					// ERROR_READ_FAULT
            }

            return error;
        }

        public override NT_STATUS Flush(UserContext UserContext, FileContext FileObject)
        {
            // Will not be called very often, but make sure that the call returns after the data is writen through the final media

            MyFileContext HE = (MyFileContext)FileObject;

            if (HE.IsDirectory || HE.FS == null)
                return NT_STATUS.OK;						// or ERROR_INVALID_HANDLE

            try
            {
                HE.FS.Flush();
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Warning->Exception in Flush: " + ex.Message);
                return (NT_STATUS)Marshal.GetHRForException(ex);
                //return 29;					// ERROR_WRITE_FAULT
            }

            return NT_STATUS.OK;
        }

        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources here.
                }

                // Dispose unmanaged resources here.

                // Nothing to do in this case. If you have to do here something, make it fast!
            }
            disposed = true;
        }

        ~iPhoneFS()
        {
            Dispose(false);
        }

        public override NT_STATUS Write(UserContext UserContext, FileContext FileObject, long Offset, ref int Count, ref byte[] Buffer, int Start)
        {
            // All locking issies are handled in the calling class, expect if other application access the files from outside 
            // WinFUSE

            // It's possible to write all data to a cache and write it through to the final media after a flush or close. But this 
            // write through should not last longer than 20 secounds
            NT_STATUS error = NT_STATUS.OK;

            MyFileContext HE = (MyFileContext)FileObject;

            if (HE.IsDirectory || HE.FS == null)
            {
                Debug.WriteLine("Warning->Cannot write to Directory.");
                Count = 0;
                return NT_STATUS.INVALID_HANDLE;						// ERROR_INVALID_HANDLE
            }

            if (!HE.FS.CanWrite && !HE.FS.CanSeek)
            {
                Debug.WriteLine("Warning->The file can not be written.");
                Count = 0;
                return NT_STATUS.INVALID_PARAMETER;						// ERROR_INVALID_PARAMETER;
            }

            if (Count > 0x0FFFFFFF)
            {
                Debug.WriteLine("Warning->Number of bytes to write is too large.");
                Count = 0;
                return NT_STATUS.INVALID_PARAMETER;						//ERROR_INVALID_PARAMETER
            }

            long NewOffset;
            try
            {
                NewOffset = HE.FS.Seek(Offset, System.IO.SeekOrigin.Begin);
                if (NewOffset != Offset)
                {
                    Debug.WriteLine("Warning->The indicated position can not be written.");
                    Count = 0;
                    return NT_STATUS.INVALID_PARAMETER;                 //132 ERROR_SEEK_ON_DEVICE
                }

                BinaryWriter Writer = new BinaryWriter(HE.FS);

                Writer.Write(Buffer, Start, Count);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Warning->Exception in Write: " + ex.Message);
                Count = 0;
                //error = 29;					// ERROR_WRITE_FAULT
                error = (NT_STATUS)Marshal.GetHRForException(ex);
            }
            return error;
        }

        public override NT_STATUS Lock(UserContext UserContext, FileContext FileObject, long Offset, long Count)
        {
            return NT_STATUS.OK;

            // This code is only necessary if there are other applications accessing the same directories and files
            // All locks issued by the WinFUSE clients are handle in the caller code already
            /*
            int error = NoError;

            MyFileContext HE = (MyFileContext)FileObject;

            if (HE.FS == null)
            {
                Debug.WriteLine("Warning->FileStream is not valid.");
                return 6;						// ERROR_INVALID_HANDLE
            }

            if (Count < 1)
            {
                Debug.WriteLine("Warning->Anzahl der zu lesenden Bytes ist zu groß.");
                return 87;						//ERROR_INVALID_PARAMETER
            }

            if (Offset > 0x7FFFFFFFFFFFFFFF || Offset < 0)
            {
                Debug.WriteLine("Warning->Anzahl der zu lesenden Bytes ist zu groß.");
                return 87;						//ERROR_INVALID_PARAMETER
            }

            // The C# lock functions only allow lock inside the file length, SMB locks can be outside and some 
            // application like WinWord and Excel need them!
            if (Offset + Count > HE.FS.Length)
                return 0x20001;                 // Not implemented, will be treaded as no error by the caller

            try
            {
                HE.FS.Lock(Offset, Count);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Warning->Exception bei Lock/Unlock: " + ex.Message);
                return 33;						//ERROR_LOCK
            }
            return error;
            */
        }

        public override NT_STATUS Unlock(UserContext UserContext, FileContext FileObject, long Offset, long Count)
        {
            return NT_STATUS.OK;

            // This code is only necessary if there are other applications accessing the same directories and files
            // All locks issued by the WinFUSE clients are handle in the caller code already
            /*
            MyFileContext HE = (MyFileContext)FileObject;

            if (HE.FS == null)
            {
                Debug.WriteLine("Warning->FileStream is not valid.");
                return 6;						// ERROR_INVALID_HANDLE
            }

            if (Count < 1)
            {
                Debug.WriteLine("Warning->Anzahl der zu lesenden Bytes ist zu groß.");
                return 87;						//ERROR_INVALID_PARAMETER
            }

            if (Offset > 0x7FFFFFFFFFFFFFFF || Offset < 0)
            {
                Debug.WriteLine("Warning->Anzahl der zu lesenden Bytes ist zu groß.");
                return 87;						//ERROR_INVALID_PARAMETER
            }

            // The C# Unlock functions only allow locks/unlocks inside the file length, SMB locks can be outside and some 
            // application like WinWord and Excel need them!
            if (Offset + Count > HE.FS.Length)
                return 0x20001;                 // Not implemented, will be treaded as no error by the caller

            try
            {
                HE.FS.Unlock(Offset, Count);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Warning->Exception bei Lock/Unlock: " + ex.Message);
                return 33;						//ERROR_LOCK
            }
            return NoError;
            */
        }

        public override NT_STATUS Read(UserContext UserContext, FileContext FileObject, long Offset, ref int Count, ref byte[] Buffer, int Start)
        {
            // All locking issues are handled in the calling class, the only read collision that can occure are when other
            // application access the same file 
            NT_STATUS error = NT_STATUS.OK;

            MyFileContext HE = (MyFileContext)FileObject;

            if (HE.IsDirectory || HE.FS == null)
            {
                Debug.WriteLine("Warning->Can not read from a Directory.");
                Count = 0;
                return NT_STATUS.INVALID_HANDLE;						// ERROR_INVALID_HANDLE
            }

            if (!HE.FS.CanRead && !HE.FS.CanSeek)
            {
                Debug.WriteLine("Warning->Can not Read or Seek the file.");
                Count = 0;
                return NT_STATUS.INVALID_PARAMETER;						// ERROR_INVALID_PARAMETER;
            }

            if (Count > 0x0FFFFFFF)
            {
                Debug.WriteLine("Warning->Number of bytes to read is too large.");
                Count = 0;
                return NT_STATUS.INVALID_PARAMETER;						//ERROR_INVALID_PARAMETER
            }

            long NewOffset;
            try
            {
                NewOffset = HE.FS.Seek(Offset, System.IO.SeekOrigin.Begin);
                if (NewOffset != Offset)
                {
                    Debug.WriteLine("Warning->The indicated position can not be read");
                    Count = 0;
                    return NT_STATUS.INVALID_PARAMETER;                 // 132 = ERROR_SEEK_ON_DEVICE
                }

                BinaryReader Reader = new BinaryReader(HE.FS);

                Count = Reader.Read(Buffer, Start, Count);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Warning->Exception in Read: " + ex.Message);
                Count = 0;
                //error = 30;					// ERROR_READ_FAULT
                error = (NT_STATUS)Marshal.GetHRForException(ex);
            }
            return error;
        }

        public override NT_STATUS SetAttributes(UserContext UserContext, FileContext FileObject, DirectoryContext data)
        {
           NT_STATUS error = NT_STATUS.OK;
            //FIXME: attributes?  what attributes? The attributes of the file, e.g. archive, hidden ....
            return error;
        }

        public override NT_STATUS GetAttributes(UserContext UserContext, FileContext FileObject, out DirectoryContext data)
        {
            //FIXME: attributes?  what attributes?

            //Should be implemented very fast, is called quit often
            data = new DirectoryContext();

            string FileName = root + FileObject.Name;
            DateTime dd = new DateTime(2007, 1, 1);
            if (phone.Exists(FileName) && !phone.IsDirectory(FileName))
            {
                data.Attrib = FileAttributes.Normal;
                data.CreationTime = dd;
                data.LastAccessTime = dd;
                data.LastWriteTime = dd;
                data.FileSize = (long)phone.FileSize(FileName);                      // data.AllocationSize willbe set to the same value
                data.Name = FileObject.Name;
                data.ShortName = ""; //GetShortName(FileObject.Name);
                return NT_STATUS.OK;
            }
            if (phone.Exists(FileName) && phone.IsDirectory(FileName))
            {
                data.Attrib = FileAttributes.Directory;
                data.CreationTime = dd;
                data.LastAccessTime = dd;
                data.LastWriteTime = dd;
                data.FileSize = (long)phone.FileSize(FileName);
                data.Name = FileObject.Name;
                data.ShortName = "";//GetShortName(FileName);
                return NT_STATUS.OK;
            }

            // Now we know that the object is not there, let's see if the path (=UpperDir) is valid
            if (!Directory.Exists(root + Path.GetDirectoryName(FileObject.Name)))
                return NT_STATUS.OBJECT_PATH_NOT_FOUND;
            return NT_STATUS.OBJECT_NAME_NOT_FOUND;  
        }

        public override NT_STATUS GetAttributes(UserContext UserContext, string PathName, out DirectoryContext data) //, SearchFlag SF)
        {
            //FIXME: attributes?  what attributes?
            data = new DirectoryContext();
            string FileName = root + PathName;
            DateTime dd = new DateTime(2007, 1, 1);
            if (phone.Exists(FileName) && !phone.IsDirectory(FileName))
            {
                data.Attrib = FileAttributes.Normal;
                data.CreationTime = dd;
                data.LastAccessTime = dd;
                data.LastWriteTime = dd;
                data.FileSize = (long)phone.FileSize(FileName);                      // data.AllocationSize willbe set to the same value
                data.Name = Path.GetFileName(FileName);
                data.ShortName = "";//GetShortName(PathName);
                return NT_STATUS.OK;
            }
            if (phone.Exists(FileName) && phone.IsDirectory(FileName))
            {
                data.Attrib = FileAttributes.Directory;
                data.CreationTime = dd;
                data.LastAccessTime = dd;
                data.LastWriteTime = dd;
                data.FileSize = (long)phone.FileSize(FileName);
                string[] DirName = PathName.Split("/\\".ToCharArray());
                data.Name = (DirName.Length > 0)?DirName[DirName.Length-1]:"/";
                data.ShortName = "";//GetShortName(FileName);
                return NT_STATUS.OK;
            }

            // Now we know that the object is not there, let's see if the path (=UpperDir) is valid
            if (!Directory.Exists(root + Path.GetDirectoryName(PathName)))
                return NT_STATUS.OBJECT_PATH_NOT_FOUND;
            return NT_STATUS.OBJECT_NAME_NOT_FOUND;   
        }

        public override NT_STATUS GetStreamInfo(UserContext UserContext, string Name, out List<DirectoryContext> StreamInfo)
        {
            //RH: I guess the iPhone do not support named streams
            StreamInfo = null;
            return NT_STATUS.NOT_IMPLEMENTED;
        }

        
        private static string GetShortName(string LongName)
        {
            System.Text.StringBuilder ShortName = new System.Text.StringBuilder(260);
            int result = GetShortPathName(LongName, ShortName, ShortName.Capacity);

            string Name = ShortName.ToString();
            if (result != 0)
                if (Name.IndexOf("\\") == -1)
                    return Name;
                else
                    return Name.Substring(Name.LastIndexOf("\\") + 1);

            int error = Marshal.GetLastWin32Error();
            return "Error.8_3";
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetShortPathName([MarshalAs(UnmanagedType.LPTStr)]string lpszLongPath, [MarshalAs(UnmanagedType.LPTStr)] System.Text.StringBuilder lpszShortPath, int cchBuffer);

    }	// iPhoneFS
}	//Namespace

