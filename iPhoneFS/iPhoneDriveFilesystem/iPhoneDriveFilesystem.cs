using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;
//using Microsoft.Win32;
using Suchwerk.Interfaces;
using Manzana;
namespace lokkju.iPx.iPhoneDrive
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

    public class iPhoneFS : BaseFileSystem, Suchwerk.Interfaces.IFilesystem
    {
        private iPhone phone;
        public iPhone Phone
        {
            get
            {
                return phone;
            }
        }
        private static string root = "/";
        /// <summary>
        /// The default constructor, in this case very simple, just set the path of the directory to share
        /// </summary>
        public iPhoneFS()
        {
            phone = new iPhone();
        }

        //Implements the search for listings by means of identification FindFirst and FindNext
        public NT_STATUS ReadDirectory(object UserContext, FileContext FileObject)
        {
            NT_STATUS error = NT_STATUS.OK;

            MyFileContext HE = (MyFileContext)FileObject;

            if (!HE.IsDirectory)
            {
                Debug.WriteLine("Warning->Handle is not a directory, can not get a listing");
                return NT_STATUS.INVALID_HANDLE;						// ERROR_INVALID_HANDLE
            }

            if (!phone.Exists(root + HE.Name) || !phone.IsDirectory(root + HE.Name))
                return NT_STATUS.OBJECT_PATH_NOT_FOUND;   // Directroy not found, sollte nie passieren

            if (HE.Items == null)
                HE.Items = new List<DirData>();
            else
            {
                Debug.WriteLine("Warning->Listing was already filled, we should access the cache.");
                HE.Items.Clear();   // Should never occur
            }
            DateTime dd = new DateTime(2007, 1, 1);
            DirData r = new DirData(".", FileAttributes.Directory);
            r.CreationTime = dd;
            r.FileSize = 0;
            r.LastAccessTime = dd;
            r.LastWriteTime = dd;
            DirData rr = new DirData("..", FileAttributes.Directory);
            rr.CreationTime = dd;
            rr.FileSize = 0;
            rr.LastAccessTime = dd;
            rr.LastWriteTime = dd;
            
            HE.Items.Add(r);
            HE.Items.Add(rr);

            DirData Item = null;
            foreach (string DirName in phone.GetDirectories(root + HE.Name))
            {
                error = GetAttributes(UserContext, root + HE.Name + DirName, out Item); //, SearchFlag.Dir);
                if (error != 0)
                    Trace.WriteLine("Warning->Error: '" + error + "' during listing directories: " + HE.Name + DirName);
                HE.Items.Add(Item);
            }

            foreach (string FileName in phone.GetFiles(root + HE.Name))
            {
                error = GetAttributes(UserContext,root + HE.Name + FileName, out Item); //, SearchFlag.File);
                if (error != 0)
                    Trace.WriteLine("Warning->Error: '" + error + "' during listing files: " + FileName);
                else
                    HE.Items.Add(Item);
            }
            return NT_STATUS.OK;
        }

        public NT_STATUS DeleteDirectory(object UserContext, string path)
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

        public NT_STATUS CreateDirectory(object UserContext, string Path, FileAttributes Attributes)
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

        public NT_STATUS FSInfo(object UserContext, out FileSystemAttributes data)
        {
            // Should be implemented very fast, as this method is called quite often
            // Try to implement is without any I/O or cache the I/O results. 

            base.FSInfo(out data);

            data.FSName = "iPhoneFS";

            data.SectorUnit = 1;						    // FreeBytes and TotalBytes will me multiplied by this value
            data.Sectors = 1;							    // FreeBytes and TotalBytes will be multiplied by this value

            //FIXME: cache/decache this info?
            iPhone.AFCDeviceInfo di = phone.GetDeviceInfo();
            data.FreeBytes = di.FileSystemFreeBytes;
            data.TotalBytes = di.FileSystemTotalBytes;
            return NT_STATUS.OK;
        }

        public NT_STATUS DeviceIO(object UserContext, FileContext FileObject, int Command, bool IsFsctl, ref byte[] Input, ref byte[] Output, ref int ValidOutputLength)
        {
            // We implement some of the usaual command on our own
            //http://wiki.ethereal.com/SMB2/Ioctl/Function/
            switch (Command)
            {
                case 0x00090028: // FSCTL_IS_VOLUME_MOUNTED
                    ValidOutputLength = 0;
                    //FIXME: make FSCTL_IS_VOLUME_MOUNTED return based on if the iPhone is connected
                    return 0;           // Return no error as the Filesystem is here
                default:
                    Trace.WriteLine("Warning->IOCTL is implemented, but this method not: 0x" + Command.ToString("X8"));
                    //Debugger.Break();
                    return NT_STATUS.NOT_IMPLEMENTED;
            }
            //return NT_STATUS.NOT_IMPLEMENTED;
        }

        public NT_STATUS Close(object UserContext, FileContext FileObject, DateTime LastWriteTime)
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

        public NT_STATUS Close(object UserContext, FileContext FileObject)
        {
            MyFileContext hinfo = (MyFileContext)FileObject;

            if (hinfo.FS != null)
                hinfo.FS.Close();

            return NT_STATUS.OK;
        }

        public new NT_STATUS GetService(out string Service, out string NativeFileSystem, out string Comment)
        {
            base.GetService(out Service, out NativeFileSystem, out Comment);
            //FIXME: add definitions for NativeFileSystem and Service
            //Service = "iPhoneDrive";
            NativeFileSystem = "iPhoneFS";
            Comment = "iPhone Drive Mountable Filesystem";
            return NT_STATUS.OK;
        }

        public NT_STATUS Create(object UserContext, string Name, SearchFlag Flags, FileMode Mode, FileAccess Access, FileShare Share, out FileContext FileObject)
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

        public NT_STATUS Rename(object UserContext, string OldName, string NewName)
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
                    phone.Move(root + OldName, root + NewName);
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
                    phone.Move(root + OldName, root + NewName);
                }
                catch (Exception e)
                {
                    Trace.WriteLine("Warning->Exception when renaming file: " + e.Message);
                    error = (NT_STATUS)Marshal.GetHRForException(e);
                }
            }
            return error;
        }

        public NT_STATUS Delete(object UserContext, string FileName)
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

        public NT_STATUS Flush(object UserContext, FileContext FileObject)
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

        public NT_STATUS Write(object UserContext, FileContext FileObject, long Offset, ref int Count, ref byte[] Buffer, int Start)
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

        public NT_STATUS Lock(object UserContext, FileContext FileObject, long Offset, long Count)
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

        public NT_STATUS Unlock(object UserContext, FileContext FileObject, long Offset, long Count)
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

        public NT_STATUS Read(object UserContext, FileContext FileObject, long Offset, ref int Count, ref byte[] Buffer, int Start)
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

        public NT_STATUS SetAttributes(object UserContext, FileContext FileObject, DirData data)
        {
            // We aware of the delete flag! Is often used to delete files. 
            NT_STATUS error = NT_STATUS.OK;
            //FIXME: attributes?  what attributes?
            return error;
        }

        public NT_STATUS GetAttributes(object UserContext, FileContext FileObject, out DirData data)
        {
            //FIXME: attributes?  what attributes?
            data = new DirData();
            string FileName = root + FileObject.Name;
            DateTime dd = new DateTime(2007, 1, 1);
            if (phone.Exists(FileName) && !phone.IsDirectory(FileName))
            {
                data.Attrib = FileAttributes.Normal;
                data.CreationTime = dd;
                data.LastAccessTime = dd;
                data.LastWriteTime = dd;
                data.FileSize = phone.FileSize(FileName);                      // data.AllocationSize willbe set to the same value
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
                data.FileSize = phone.FileSize(FileName);
                data.Name = FileObject.Name;
                data.ShortName = "";//GetShortName(FileName);
                return NT_STATUS.OK;
            }

            // Now we know that the object is not there, let's see if the path (=UpperDir) is valid
            if (!Directory.Exists(root + Path.GetDirectoryName(FileObject.Name)))
                return NT_STATUS.OBJECT_PATH_NOT_FOUND;
            return NT_STATUS.OBJECT_NAME_NOT_FOUND;  
        }

        public NT_STATUS GetAttributes(object UserContext, string PathName, out DirData data) //, SearchFlag SF)
        {
            //FIXME: attributes?  what attributes?
            data = new DirData();
            string FileName = root + PathName;
            DateTime dd = new DateTime(2007, 1, 1);
            if (phone.Exists(FileName) && !phone.IsDirectory(FileName))
            {
                data.Attrib = FileAttributes.Normal;
                data.CreationTime = dd;
                data.LastAccessTime = dd;
                data.LastWriteTime = dd;
                data.FileSize = phone.FileSize(FileName);                      // data.AllocationSize willbe set to the same value
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
                data.FileSize = phone.FileSize(FileName);
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

        public NT_STATUS GetStreamInfo(object UserContext, string Name, out List<DirData> StreamInfo)
        {
            //FIXME: streams? we don't want no streams!
            StreamInfo = new List<DirData>();
            return NT_STATUS.OK;
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

