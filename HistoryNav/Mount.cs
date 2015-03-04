using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections;
using Microsoft.Win32.SafeHandles;
using System.Threading;

namespace HistoryNav
{
    public partial class Mount
    {
        public enum EMoveMethod : uint
        {
            Begin = 0,
            Current = 1,
            End = 2
        }
        [Flags]
        public enum FileSystemFeature : uint
        {
            /// <summary>
            /// The file system supports case-sensitive file names.
            /// </summary>
            CaseSensitiveSearch = 1,
            /// <summary>
            /// The file system preserves the case of file names when it places a name on disk.
            /// </summary>
            CasePreservedNames = 2,
            /// <summary>
            /// The file system supports Unicode in file names as they appear on disk.
            /// </summary>
            UnicodeOnDisk = 4,
            /// <summary>
            /// The file system preserves and enforces access control lists (ACL).
            /// </summary>
            PersistentACLS = 8,
            /// <summary>
            /// The file system supports file-based compression.
            /// </summary>
            FileCompression = 0x10,
            /// <summary>
            /// The file system supports disk quotas.
            /// </summary>
            VolumeQuotas = 0x20,
            /// <summary>
            /// The file system supports sparse files.
            /// </summary>
            SupportsSparseFiles = 0x40,
            /// <summary>
            /// The file system supports re-parse points.
            /// </summary>
            SupportsReparsePoints = 0x80,
            /// <summary>
            /// The specified volume is a compressed volume, for example, a DoubleSpace volume.
            /// </summary>
            VolumeIsCompressed = 0x8000,
            /// <summary>
            /// The file system supports object identifiers.
            /// </summary>
            SupportsObjectIDs = 0x10000,
            /// <summary>
            /// The file system supports the Encrypted File System (EFS).
            /// </summary>
            SupportsEncryption = 0x20000,
            /// <summary>
            /// The file system supports named streams.
            /// </summary>
            NamedStreams = 0x40000,
            /// <summary>
            /// The specified volume is read-only.
            /// </summary>
            ReadOnlyVolume = 0x80000,
            /// <summary>
            /// The volume supports a single sequential write.
            /// </summary>
            SequentialWriteOnce = 0x100000,
            /// <summary>
            /// The volume supports transactions.
            /// </summary>
            SupportsTransactions = 0x200000,
        }
        #region imports
        [DllImport("kernel32.dll")]
        static extern uint GetLogicalDrives();
        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        extern static bool GetVolumeInformation(
          string RootPathName,
          StringBuilder VolumeNameBuffer,
          int VolumeNameSize,
          out uint VolumeSerialNumber,
          out uint MaximumComponentLength,
          out FileSystemFeature FileSystemFlags,
          StringBuilder FileSystemNameBuffer,
          int nFileSystemNameSize);
        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        extern static bool GetVolumeInformationByHandleW(
          SafeFileHandle RootPathName,
          StringBuilder VolumeNameBuffer,
          int VolumeNameSize,
          out uint VolumeSerialNumber,
          out uint MaximumComponentLength,
          out FileSystemFeature FileSystemFlags,
          StringBuilder FileSystemNameBuffer,
          int nFileSystemNameSize);

        [DllImport("kernel32.dll")]
        static extern uint QueryDosDevice(string lpDeviceName, IntPtr lpTargetPath,
           int ucchMax);

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool SetFilePointerEx(
            [In] SafeFileHandle hFile,
            [In] long lDistanceToMove,
            [Out] out long lpDistanceToMoveHigh,
            [In] EMoveMethod dwMoveMethod);

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto, EntryPoint = "SetFilePointerEx")]
        static extern long SetFilePointerEx2(
            [In] SafeFileHandle hFile,
            [In] long lDistanceToMove,
            [Out] out long lpDistanceToMoveHigh,
            [In] EMoveMethod dwMoveMethod);

        [DllImport("Kernel32.dll", SetLastError = true)]
        internal static extern int GetFileSize(SafeFileHandle hFile, out int highSize);

        [DllImport("kernel32.dll")]
        static extern bool GetFileSizeEx(
            [In] SafeFileHandle hFile,
            [Out] out long lpFileSize);

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern long SetFilePointer(
            [In] SafeFileHandle hFile,
            [In] long lDistanceToMove,
            [Out] out long lpDistanceToMoveHigh,
            [In] EMoveMethod dwMoveMethod);

        [DllImport("ntdll.dll", ExactSpelling = true, SetLastError = true)]
        public static extern int NtCreateFile(out SafeFileHandle handle, FileAccess access,
            OBJECT_ATTRIBUTES objectAttributes, IO_STATUS_BLOCK ioStatus, ref long allocSize,
            uint fileAttributes, FileShare share, uint createDisposition, uint createOptions, IntPtr eaBuffer, uint eaLength);
        #endregion

        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct IO_STATUS_BLOCK
        {
            public uint status;
            public IntPtr information;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct OBJECT_ATTRIBUTES
        {
            public Int32 Length;
            public IntPtr RootDirectory;
            public IntPtr ObjectName;
            public uint Attributes;
            public IntPtr SecurityDescriptor;
            public IntPtr SecurityQualityOfService;

        }

        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct UNICODE_STRING
        {
            public ushort Length;
            public ushort MaximumLength;
            public IntPtr Buffer;

        }
        Hashtable paths = new Hashtable();
        SafeFileHandle h;
        IntPtr hPrime = IntPtr.Zero;
        ulong currentPosition;
        bool isPhysicalDrive = false;
        long diskSize;
        public Database conn;

        public Mount()
#if USA
            :base("MFTParse")
#endif
        {
            conn = new Database("temp");
            conn.main_init();
            //conn.CreateMemory();
            conn.Create("mft", new string[] {"intIndex INTEGER PRIMARY KEY ASC","directory TEXT","filename TEXT",
                            "siaaccessTime TEXT","siacreatedTime TEXT","siawriteTime TEXT","siamftTime TEXT",
                            "fiaaccessTime TEXT","fiacreatedTime TEXT","fiawriteTime TEXT","fiamftTime TEXT",
                            "size TEXT"});
        }
        ~Mount()
        {

        }

        T ByteArrayToStructure<T>(byte[] bytes) where T : struct
        {
            int size = 0;
            if (bytes.Length < Marshal.SizeOf(typeof(T)))
            {
                size = bytes.Length;
            }
            else
            {
                size = Marshal.SizeOf(typeof(T));
            }
            IntPtr dest = Marshal.AllocHGlobal(size);
            Marshal.Copy(bytes, 0, dest, size);
            //GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            T stuff = (T)Marshal.PtrToStructure(dest,
                typeof(T));
            //handle.Free();
            return stuff;
        }
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern UInt32 WaitForSingleObject(IntPtr hHandle, UInt32 dwMilliseconds);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CreateFile(
             [MarshalAs(UnmanagedType.LPTStr)] string filename,
             uint access,
             int share,
             IntPtr securityAttributes, // optional SECURITY_ATTRIBUTES struct or IntPtr.Zero
             int creationDisposition,
             int flagsAndAttributes,
             IntPtr templateFile);

        public Hashtable RawRead(string drive)
        {
            try
            {
                string volume = LogicalDriveToPhysicalDriveN(drive);
                if (!volume.Contains("GLOBALROOT"))
                {
                    volume = @"\\?\GLOBALROOT" + volume;
                }
                hPrime = CreateFile(volume, 0x80000000, 0x00000002 | 0x00000001 | 0x00000004, IntPtr.Zero, 3, 0x80, IntPtr.Zero);
                h = new SafeFileHandle(hPrime, true);
                //int err = Marshal.GetLastWin32Error();
                //h = Program.pFuncCreateFile(volume, FileAccess.Read, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, 0x80, IntPtr.Zero);
                if (!h.IsInvalid)
                {
                    //fs = new FileStream(h, FileAccess.Read);
                    DoWork();
                    return paths;
                    //fs.Close();
                }
                // Read from stream
                else
                {
                    // get error code and throw
                    //fs = new FileStream(diskEx, FileMode.Open);
                    int error = Marshal.GetLastWin32Error();
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        void DoWork()
        {
            bool cont = true;//GetNTFSStart(); <- only call this if using raw disk \\.\PhysicalDrive0

            if (cont)
            {
                //Program.Log("Debug");
                GetNTFSMFT();
                //Program.Log("Debug");
                GetFiles();
            }
            if (!h.IsClosed)
            {
                h.Close();
                h.SetHandleAsInvalid();
            }

        }
        ulong Seek(long position)
        {
            long test = 0;
            bool set = SetFilePointerEx(h, position, out test, EMoveMethod.Begin);
            int error = Marshal.GetLastWin32Error();
            if (error != 0)
            {
                //set = SetFilePointerEx(h, position, out test, EMoveMethod.Current);
            }
            if (set)
            {
                return (ulong)test;
            }
            else
            {
                return 0;
            }
        }

        public uint Read(byte[] buffer, long index, uint count)
        {
            //Program.Log("Debug");
            uint n = 0;
            int error = 0;
            //uint wait = WaitForSingleObject(h.DangerousGetHandle(), 0100);
            //while (0x00000102 == wait)
            {
                //wait = WaitForSingleObject(hPrime, 0100);
            }

            if (!Program.pFuncReadFile(h.DangerousGetHandle(), buffer, count, out n, IntPtr.Zero))
            {
                error = Marshal.GetLastWin32Error();
                //Program.Log("Debug");
                return 0;
            }
            error = Marshal.GetLastWin32Error();
            //Program.Log("Debug");
            return n;
        }
        bool GetNTFSStart()
        {
            //Program.Log("Debug");
            bool worked = false;
            byte[] checkDiskType = new byte[0x200];

            uint isWork = Read(checkDiskType, (long)0, (uint)0x200);
            int error = Marshal.GetLastWin32Error();

            if (checkDiskType[0x1C2] != 0x7)
            {
                MessageBox.Show("Not NTFS Partition, Type: " + checkDiskType[0]);
                worked = false;
                //Program.Log("Debug");
                return worked;
            }
            worked = true;
            NTFSRelativeSector = GetLittleEndianIntegerFromByteArray(new byte[] { checkDiskType[0x1c6], checkDiskType[0x1c7], checkDiskType[0x1c8], checkDiskType[0x1c9] }, 0);
            NTFSStart = 512 * NTFSRelativeSector;
            //Program.Log("Debug");
            return worked;
        }
        void GetNTFSMFT()
        {
            byte[] NTFS = new byte[] { 0x4E, 0x54, 0x46, 0x53 };
            int test = NTFSStart + 3;
            //Program.Log("Debug");
            currentPosition = Seek(NTFSStart);
            //Program.Log("Debug");
            byte[] buffer = new byte[0x400];
            //Program.Log("Debug");
            Read(buffer, 0, 0x400);
            //Program.Log("Debug");
            byte[] NTFSTest = new byte[] { buffer[3], buffer[4], buffer[5], buffer[6] };
            if (ArraysEqual<byte>(NTFS, NTFSTest))
            {
                //Program.Log("Debug");
                byte[] arrBytesPerSector = new byte[] { buffer[0xb], buffer[0xc] };
                //Program.Log("Debug");
                bytesPerSector = GetLittleEndianShortFromByteArray(arrBytesPerSector, 0);
                //Program.Log("Debug");
                bytesPerCluster = bytesPerSector * buffer[0xd];
                byte[] longBuffer = new byte[] { 
                    buffer[0x30], buffer[0x31], buffer[0x32],buffer[0x33],
                    buffer[0x34],buffer[0x35], buffer[0x36],buffer[0x37]
                };
                //Program.Log("Debug");
                mftstart = GetLittleEndianInteger64FromByteArray(longBuffer, 0);
                //Program.Log("Debug");
                mftstart = mftstart * (Int64)bytesPerCluster;
                mftstart = mftstart + (Int64)NTFSStart;
            }
            //Program.Log("Debug");
        }

        void ParseAttributes(byte[] entry)
        {
            //Program.Log("Debug");
            if (entry.Length == 1024)
            {
                //Program.Log("Debug");
                int attrFirstType;
                int firstAttr;
                int attrFirstLength;
                Int64 residentFirstValues;
                Int64 residentSecondValues;
                try
                {
                    //Program.Log("Debug");
                    DB_Entry dbentry = new DB_Entry();
                    int lengthleft = 1024;
                    int fileNameLength = 0;
                    string fullpath = "";
                    string filename = "";
                    string attrname = "";
                    bool hasSia = false, hasFia = false;
                    byte[] arrMft = new byte[0x30];
                    //Program.Log("Debug");
                    Array.Copy(entry, 0, arrMft, 0, 0x30);
                    //Program.Log("Debug");
                    MFT mft = ByteArrayToStructure<MFT>(arrMft);
                    //Program.Log("Debug");
                    firstAttr = BitConverter.ToInt16(new byte[] { entry[0x14], entry[0x15] }, 0);
                    if (firstAttr >= entry.Length || firstAttr < 0)
                    {
                        //return;
                    }
                    attrFirstType = BitConverter.ToInt32(new byte[] { entry[firstAttr], 
                    entry[firstAttr+1], entry[firstAttr+2], entry[firstAttr+3] }, 0);
                    attrFirstLength = BitConverter.ToInt32(new byte[] { entry[firstAttr+4], 
                    entry[firstAttr+5], entry[firstAttr+6], entry[firstAttr+7] }, 0);
                    residentFirstValues = BitConverter.ToInt64(new byte[] { entry[firstAttr+8], 
                    entry[firstAttr+9], entry[firstAttr+10], entry[firstAttr+11], entry[firstAttr+12], 
                    entry[firstAttr+13], entry[firstAttr+14], entry[firstAttr+15]}, 0);
                    residentSecondValues = BitConverter.ToInt64(new byte[] { entry[firstAttr+16], 
                    entry[firstAttr+17], entry[firstAttr+18], entry[firstAttr+19], entry[firstAttr+20], 
                    entry[firstAttr+21], entry[firstAttr+22], entry[firstAttr+23]}, 0);
                    lengthleft -= firstAttr;
                    while (lengthleft > 0)
                    {
                        byte[] buffer = new byte[0x400];
                        try
                        {
                            if (firstAttr + 24 <= entry.Length && attrFirstLength - 24 <= entry.Length && firstAttr > 0 && attrFirstLength - 24 > 0)
                            {
                                Array.Copy(entry, firstAttr + 24, buffer, 0, attrFirstLength - 24);
                            }
                            else
                            {
                                lengthleft = 0;
                                break;
                            }
                            //Program.Log("Debug");

                            //Program.Log("Debug");
                        }
                        catch (Exception ex)
                        {
                            lengthleft = 0;
                        }
                        switch (attrFirstType)
                        {
                            //SIA
                            case 0x10:
                                //Program.Log("Debug");
                                ATTR_SIA sia = ByteArrayToStructure<ATTR_SIA>(buffer);
                                firstAttr += attrFirstLength;
                                attrFirstType = BitConverter.ToInt32(new byte[] { entry[firstAttr], 
                                entry[firstAttr+1], entry[firstAttr+2], entry[firstAttr+3] }, 0);
                                attrFirstLength = BitConverter.ToInt32(new byte[] { entry[firstAttr+4], 
                                entry[firstAttr+5], entry[firstAttr+6], entry[firstAttr+7] }, 0);
                                //Debug.Print(sia.ToString());
                                dbentry.sia = sia;
                                hasSia = true;
                                break;
                            //ATTR LIST
                            case 0x20:
                                //Program.Log("Debug");
                                ATTR_LIST list = ByteArrayToStructure<ATTR_LIST>(buffer);
                                byte[] arrFileName = new byte[list.attrLength];
                                Array.Copy(buffer, 26, arrFileName, 0, arrFileName.Length);
                                attrname = System.Text.UnicodeEncoding.Unicode.GetString(arrFileName);
                                //Debug.Print(attrname);
                                firstAttr += attrFirstLength;
                                attrFirstType = BitConverter.ToInt32(new byte[] { entry[firstAttr], 
                                entry[firstAttr+1], entry[firstAttr+2], entry[firstAttr+3] }, 0);
                                attrFirstLength = BitConverter.ToInt32(new byte[] { entry[firstAttr+4], 
                                entry[firstAttr+5], entry[firstAttr+6], entry[firstAttr+7] }, 0);
                                break;
                            //FIA
                            case 0x30:
                                //Program.Log("Debug");
                                ATTR_FIA fia = ByteArrayToStructure<ATTR_FIA>(buffer);
                                firstAttr += attrFirstLength;
                                attrFirstType = BitConverter.ToInt32(new byte[] { entry[firstAttr], 
                                entry[firstAttr+1], entry[firstAttr+2], entry[firstAttr+3] }, 0);
                                attrFirstLength = BitConverter.ToInt32(new byte[] { entry[firstAttr+4], 
                                entry[firstAttr+5], entry[firstAttr+6], entry[firstAttr+7] }, 0);
                                fileNameLength = fia.filenameLength * 2;
                                byte[] arrFileName2 = new byte[fileNameLength];
                                Array.Copy(buffer, 0x42, arrFileName2, 0, arrFileName2.Length);
                                filename = System.Text.UnicodeEncoding.Unicode.GetString(arrFileName2);
                                //Program.Log("Debug");
                                long temp = (fia.fiaReferenceParentDir << 48);
                                temp = temp >> 48;
                                int mftReferenceToHashTable = (int)temp;

                                if ((string)paths[mftReferenceToHashTable] != null)
                                {
                                    fullpath = (string)paths[mftReferenceToHashTable];
                                }
                                fullpath += "\\" + filename;
                                dbentry.fia = fia;
                                hasFia = true;
                                if ((fia.flags & FLAGS.Directory) == FLAGS.Directory)
                                {
                                    //Program.Log("Debug");
                                    if (!paths.Contains(mft.mftRecordNumber))
                                    {
                                        paths.Add(mft.mftRecordNumber, fullpath);
                                    }
                                    break;
                                }
                                else
                                {
                                    //Program.Log("Debug");
                                    //Debug.Print(fia.ToString());
                                    //Debug.Print("filename: " + fullpath);
                                    //Debug.Print(mft.ToString());
                                    break;
                                }
                            //OBJ ID
                            case 0x40:
                                //Program.Log("Debug");
                                ATTR_OBJID objid = ByteArrayToStructure<ATTR_OBJID>(buffer);
                                firstAttr += attrFirstLength;
                                attrFirstType = BitConverter.ToInt32(new byte[] { entry[firstAttr], 
                                entry[firstAttr+1], entry[firstAttr+2], entry[firstAttr+3] }, 0);
                                attrFirstLength = BitConverter.ToInt32(new byte[] { entry[firstAttr+4], 
                                entry[firstAttr+5], entry[firstAttr+6], entry[firstAttr+7] }, 0);
                                break;
                            //SEC DESC
                            case 0x50:
                                //Program.Log("Debug");
                                byte[] arrSids = new byte[attrFirstLength - 16];
                                ATTR_SECDESC secdesc = ByteArrayToStructure<ATTR_SECDESC>(buffer);
                                Array.Copy(buffer, 16, arrSids, 0, arrSids.Length);
                                firstAttr += attrFirstLength;
                                attrFirstType = BitConverter.ToInt32(new byte[] { entry[firstAttr], 
                                entry[firstAttr+1], entry[firstAttr+2], entry[firstAttr+3] }, 0);
                                attrFirstLength = BitConverter.ToInt32(new byte[] { entry[firstAttr+4], 
                                entry[firstAttr+5], entry[firstAttr+6], entry[firstAttr+7] }, 0);

                                break;
                            //VOL NAME
                            case 0x60:
                                //Program.Log("Debug");
                                System.Text.UnicodeEncoding enc = new UnicodeEncoding();
                                ATTR_VOLNAME volname = new ATTR_VOLNAME(enc.GetString(buffer));
                                firstAttr += attrFirstLength;
                                attrFirstType = BitConverter.ToInt32(new byte[] { entry[firstAttr], 
                                entry[firstAttr+1], entry[firstAttr+2], entry[firstAttr+3] }, 0);
                                attrFirstLength = BitConverter.ToInt32(new byte[] { entry[firstAttr+4], 
                                entry[firstAttr+5], entry[firstAttr+6], entry[firstAttr+7] }, 0);
                                break;
                            //VOL INFO
                            case 0x70:
                                //Program.Log("Debug");
                                ATTR_VOLINFO volinfo = ByteArrayToStructure<ATTR_VOLINFO>(buffer);
                                firstAttr += attrFirstLength;
                                attrFirstType = BitConverter.ToInt32(new byte[] { entry[firstAttr], 
                                entry[firstAttr+1], entry[firstAttr+2], entry[firstAttr+3] }, 0);
                                attrFirstLength = BitConverter.ToInt32(new byte[] { entry[firstAttr+4], 
                                entry[firstAttr+5], entry[firstAttr+6], entry[firstAttr+7] }, 0);
                                break;
                            //DATA
                            case 0x80:
                                //Program.Log("Debug");
                                ATTR_DATA data = new ATTR_DATA();
                                data.data = buffer;
                                firstAttr += attrFirstLength;
                                attrFirstType = BitConverter.ToInt32(new byte[] { entry[firstAttr], 
                                entry[firstAttr+1], entry[firstAttr+2], entry[firstAttr+3] }, 0);
                                attrFirstLength = BitConverter.ToInt32(new byte[] { entry[firstAttr+4], 
                                entry[firstAttr+5], entry[firstAttr+6], entry[firstAttr+7] }, 0);
                                break;
                            //INDX ROOT
                            case 0x90:
                                //Program.Log("Debug");
                                ATTR_INDXROOT indxroot = ByteArrayToStructure<ATTR_INDXROOT>(buffer);
                                firstAttr += attrFirstLength;
                                attrFirstType = BitConverter.ToInt32(new byte[] { entry[firstAttr], 
                                entry[firstAttr+1], entry[firstAttr+2], entry[firstAttr+3] }, 0);
                                attrFirstLength = BitConverter.ToInt32(new byte[] { entry[firstAttr+4], 
                                entry[firstAttr+5], entry[firstAttr+6], entry[firstAttr+7] }, 0);
                                break;
                            //INDX ALLOC
                            case 0xA0:
                                //Program.Log("Debug");
                                ATTR_INDXALLOC indxalloc = new ATTR_INDXALLOC();
                                indxalloc.entries = buffer;
                                firstAttr += attrFirstLength;
                                attrFirstType = BitConverter.ToInt32(new byte[] { entry[firstAttr], 
                                entry[firstAttr+1], entry[firstAttr+2], entry[firstAttr+3] }, 0);
                                attrFirstLength = BitConverter.ToInt32(new byte[] { entry[firstAttr+4], 
                                entry[firstAttr+5], entry[firstAttr+6], entry[firstAttr+7] }, 0);
                                break;
                            //BITMAP
                            case 0xB0:
                                //Program.Log("Debug");
                                ATTR_BITMAP bitmap = ByteArrayToStructure<ATTR_BITMAP>(buffer);
                                firstAttr += attrFirstLength;
                                attrFirstType = BitConverter.ToInt32(new byte[] { entry[firstAttr], 
                                entry[firstAttr+1], entry[firstAttr+2], entry[firstAttr+3] }, 0);
                                attrFirstLength = BitConverter.ToInt32(new byte[] { entry[firstAttr+4], 
                                entry[firstAttr+5], entry[firstAttr+6], entry[firstAttr+7] }, 0);
                                break;
                            //REPARSE
                            case 0xC0:
                                //Program.Log("Debug");
                                ATTR_REPARSE reparse = new ATTR_REPARSE(buffer);
                                firstAttr += attrFirstLength;
                                attrFirstType = BitConverter.ToInt32(new byte[] { entry[firstAttr], 
                                entry[firstAttr+1], entry[firstAttr+2], entry[firstAttr+3] }, 0);
                                attrFirstLength = BitConverter.ToInt32(new byte[] { entry[firstAttr+4], 
                                entry[firstAttr+5], entry[firstAttr+6], entry[firstAttr+7] }, 0);
                                break;
                            //EA INFO
                            case 0xD0:
                                //Program.Log("Debug");
                                ATTR_EAINFO eainfo = ByteArrayToStructure<ATTR_EAINFO>(buffer);
                                firstAttr += attrFirstLength;
                                attrFirstType = BitConverter.ToInt32(new byte[] { entry[firstAttr], 
                                entry[firstAttr+1], entry[firstAttr+2], entry[firstAttr+3] }, 0);
                                attrFirstLength = BitConverter.ToInt32(new byte[] { entry[firstAttr+4], 
                                entry[firstAttr+5], entry[firstAttr+6], entry[firstAttr+7] }, 0);
                                break;
                            //EA
                            case 0xE0:
                                //Program.Log("Debug");
                                ATTR_EA ea = new ATTR_EA(buffer);
                                firstAttr += attrFirstLength;
                                attrFirstType = BitConverter.ToInt32(new byte[] { entry[firstAttr], 
                                entry[firstAttr+1], entry[firstAttr+2], entry[firstAttr+3] }, 0);
                                attrFirstLength = BitConverter.ToInt32(new byte[] { entry[firstAttr+4], 
                                entry[firstAttr+5], entry[firstAttr+6], entry[firstAttr+7] }, 0);
                                break;
                            default:
                                //Program.Log("Debug");
                                lengthleft = 0;
                                break;
                        }
                        lengthleft -= firstAttr;
                    }
                    if (hasFia != false && hasSia != false)
                    {

                        //Program.Log("Debug");
                        //dbEntries.Add(new Tuple<string, string[]>("nav", new string[] {"NULL", fullpath, filename, DateTime.FromFileTime(dbentry.sia.siaCreate).ToString(),
                        //DateTime.FromFileTime(dbentry.sia.siaAccess).ToString(),DateTime.FromFileTime(dbentry.sia.siaAlter).ToString(),
                        //DateTime.FromFileTime(dbentry.sia.siaMod).ToString(),DateTime.FromFileTime(dbentry.fia.fiaCreate).ToString(),
                        //DateTime.FromFileTime(dbentry.fia.fiaMod).ToString(),DateTime.FromFileTime(dbentry.fia.fiaAccess).ToString(),
                        //DateTime.FromFileTime(dbentry.fia.fiaAlter).ToString(),dbentry.fia.fiaAllocSize.ToString()}));

                        conn.Insert("mft", new string[] {"NULL", fullpath, filename, DateTime.FromFileTime(dbentry.sia.siaCreate).ToString(),
                        DateTime.FromFileTime(dbentry.sia.siaAccess).ToString(),DateTime.FromFileTime(dbentry.sia.siaAlter).ToString(),
                        DateTime.FromFileTime(dbentry.sia.siaMod).ToString(),DateTime.FromFileTime(dbentry.fia.fiaCreate).ToString(),
                        DateTime.FromFileTime(dbentry.fia.fiaMod).ToString(),DateTime.FromFileTime(dbentry.fia.fiaAccess).ToString(),
                        DateTime.FromFileTime(dbentry.fia.fiaAlter).ToString(),dbentry.fia.fiaAllocSize.ToString()});
                    }
                }
                catch (Exception ex)
                {
                    //Program.Log("Debug");
                }
            }
            else
            {
                //Program.Log("Debug");
                return;
                //throw new ArgumentOutOfRangeException();
            }
        }
        /// <summary>
        /// Takes drive letter and translates to physical drive for fixed disks only
        /// </summary>
        /// <param name="disk">drive letter or full path</param>
        /// <returns>\\.\PhysicalDriveN or full path</returns>
        string LogicalDriveToPhysicalDriveN(string disk)
        {
            //Program.Log("Debug");
            string retval = disk;
            try
            {
                //Program.Log("Debug");
                DriveInfo[] dr = DriveInfo.GetDrives();
                for (int i = 0, j = 0; i < dr.Length; i++)
                {
                    //Program.Log("Debug");
                    if (dr[i].RootDirectory.ToString().Contains(disk))
                    {
                        string root = dr[i].RootDirectory.ToString();
                        root = root.Remove(root.Length - 1, 1);
                        string[] vals = QueryDosDevice(root);
                        if (vals.Length == 1)
                        {
                            retval = vals[0];
                        }
                        else
                        {
                            retval = "\\\\.\\PhysicalDrive" + j;
                        }
                        isPhysicalDrive = true;
                        diskSize = dr[i].TotalSize;

                    }
                    if (dr[i].DriveType == DriveType.Fixed)
                    {
                        j++;
                    }
                }
            }
            catch (Exception ex)
            {
                //Program.Log("Debug");
            }
            //Program.Log("Debug");
            return retval;
        }
        void GetFiles()
        {
            //Program.Log("Debug");
            //fs.Seek((long)mftstart, SeekOrigin.Begin);
            currentPosition = Seek(mftstart);
            byte[] mfttest = new byte[5];
            byte[] mftSpace = new byte[5];
            byte[] mftSpace2 = new byte[5];
            byte[] buffer = new byte[1024];
            byte[] bigBuffer = new byte[0x40000];
            //Program.Log("Debug");
            uint place = Read(buffer, (long)0, (uint)1024);

            //Program.Log("Debug");
            int numberOfMftsProcessed = 0;
            ulong offsetToContent = 0;
            Array.Copy(buffer, 0, mfttest, 0, mfttest.Length);

            if (ArraysEqual<byte>(mfttest, mftSig))
            {
                offsetToContent = (ulong)BitConverter.ToInt32(buffer, 0x4c);
                offsetToContent *= 1024;
                currentPosition += offsetToContent;
                Seek((long)currentPosition);
                Read(bigBuffer, 0, 0x40000);
                while (ArraysEqual<byte>(bigBuffer, mftSig, 5))
                {
                    //Program.Log("Debug");


                    //Program.Log("Debug");
                    try
                    {
                        //Program.Log("Debug");
                        //fs.Read(buffer, 0, buffer.Length);
                        ////Debug.Print("Entry# " + entryNumber.ToString("X"));
                        for (int i = 0; i < bigBuffer.Length; i += 0x400)
                        {
                            Array.Copy(bigBuffer, i, buffer, 0, buffer.Length);
                            if (ArraysEqual<byte>(buffer, mftSig, 5))
                            {
                                ParseAttributes(buffer);
                            }
                            else
                            {
                                Debug.Print("uh oh");
                            }
                        }
                        //Program.Log("Debug");
                        currentPosition += Read(bigBuffer, 0, 0x40000);
                        //ulong works = Seek((long)currentPosition);
                        numberOfMftsProcessed += 0x100;
                        //Array.Copy(buffer, 0, mfttest, 0, mfttest.Length);
                    }
                    catch (OverflowException ex)
                    {
                        ////Debug.Print(String.Format("Position is: {0:X}", fs.Position));
                    }
                }
            }
        }

        private static string[] QueryDosDevice(string root = "")
        {
            // Allocate some memory to get a list of all system devices.
            // Start with a small size and dynamically give more space until we have enough room.
            uint returnSize = 0;
            int maxSize = 260;
            string allDevices = null;
            IntPtr mem;
            string[] retval = null;
            //Program.Log("Debug");
            if (root != "")
            {
                mem = Marshal.AllocHGlobal(maxSize);
                uint s = QueryDosDevice(root, mem, maxSize);
                string strDrive = Marshal.PtrToStringAnsi(mem, (int)s);
                retval = new string[1] { strDrive.Replace("\0", "") };
                Marshal.FreeHGlobal(mem);
                return retval;
            }
            else
            {
                while (returnSize == 0)
                {
                    //Program.Log("Debug");
                    mem = Marshal.AllocHGlobal(maxSize);
                    if (mem != IntPtr.Zero)
                    {
                        // mem points to memory that needs freeing
                        try
                        {
                            //Program.Log("Debug");
                            returnSize = QueryDosDevice(null, mem, maxSize);
                            if (returnSize != 0)
                            {
                                //Program.Log("Debug");
                                allDevices = Marshal.PtrToStringAnsi(mem, (int)returnSize);
                                retval = allDevices.Split('\0');
                                break;    // not really needed, but makes it more clear...
                            }
                            else if (Marshal.GetLastWin32Error() == 0x7a)
                            //maybe better
                            //else if( Marshal.GetLastWin32Error() == ERROR_INSUFFICIENT_BUFFER)
                            //ERROR_INSUFFICIENT_BUFFER = 122;
                            {
                                maxSize *= 10;
                            }
                            else
                            {
                                //Program.Log("Debug");
                                Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());
                            }
                        }
                        finally
                        {
                            //Program.Log("Debug");

                            Marshal.FreeHGlobal(mem);
                        }
                    }
                    else
                    {
                        //Program.Log("Debug");
                        throw new OutOfMemoryException();
                    }
                }
            }
            return retval;
        }
        #region Helper Functions

        int GetBigEndianIntegerFromByteArray(byte[] data, int startIndex)
        {
            return ((data[startIndex] & 0xFF) << 24)
                 | ((data[startIndex + 1] & 0xFF) << 16)
                 | ((data[startIndex + 2] & 0xFF) << 8)
                 | data[startIndex + 3];
        }

        int GetLittleEndianIntegerFromByteArray(byte[] data, int startIndex)
        {
            return ((data[startIndex + 3] & 0xFF) << 24)
                 | ((data[startIndex + 2] & 0xFF) << 16)
                 | ((data[startIndex + 1] & 0xFF) << 8)
                 | data[startIndex];
        }
        int GetLittleEndianShortFromByteArray(byte[] data, int startIndex)
        {
            return ((data[startIndex + 1] & 0xFF) << 8)
                 | data[startIndex];
        }
        Int64 GetBigEndianInteger64FromByteArray(byte[] data, int startIndex)
        {
            return (Int64)((data[startIndex] & 0xFF) << 56)
                 | ((data[startIndex + 1] & 0xFF) << 48)
                 | ((data[startIndex + 2] & 0xFF) << 40)
                 | ((data[startIndex + 3] & 0xFF) << 32)
                 | ((data[startIndex + 4] & 0xFF) << 24)
                 | ((data[startIndex + 5] & 0xFF) << 16)
                 | ((data[startIndex + 6] & 0xFF) << 8)
                 | data[startIndex + 7];
        }

        Int64 GetLittleEndianInteger64FromByteArray(byte[] arr, int startIndex)
        {
            Int64 retVal = 0;
            retVal = BitConverter.ToInt64(arr, 0);
            retVal = ((byte)(arr[7] & 0xFF)) << 56;
            retVal |= (arr[6] & 0xFF) << 48;
            retVal |= (arr[5] & 0xFF) << 40;
            retVal |= (arr[4] & 0xFF) << 32;
            retVal |= (arr[3] & 0xFF) << 24;
            retVal |= (arr[2] & 0xFF) << 16;
            retVal |= (arr[1] & 0xFF) << 8;
            retVal |= arr[0] & 0xFF;
            return retVal;
        }
        /// <summary>
        /// Match up to N bytes of two arrays
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="a1"></param>
        /// <param name="a2"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static bool ArraysEqual<T>(T[] a1, T[] a2, int length = 0)
        {
            int checkLength = 0;
            if (ReferenceEquals(a1, a2))
                return true;

            if (a1 == null || a2 == null)
                return false;

            if (a1.Length != a2.Length && length == 0)
                return false;
            //length is greater than smallest array
            if (length > 0 && (a1.Length < length || a2.Length < length))
            {
                throw new ArgumentOutOfRangeException("length", "longer than smallest array");
            }
            else
            {
                if (length > 0)
                {
                    checkLength = length;
                }
                else
                {
                    checkLength = a1.Length;
                }
            }
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < checkLength; i++)
            {
                if (!comparer.Equals(a1[i], a2[i])) return false;
            }
            return true;
        }
        #endregion
    }
}