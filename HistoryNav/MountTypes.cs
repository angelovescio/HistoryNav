using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace HistoryNav
{
    public partial class Mount
    {
        //http://eddandforensics.blogspot.com/2010/08/mft-parsing-enscript.html
        //http://pauldotcom.com/2011/03/tim-mugherini-presents-ntfs-mf.html
        //MFTParser.EnScript is in the downloads

        //NTFS information
        int NTFSStart;
        int NTFSRelativeSector;
        int bytesPerSector;
        int bytesPerCluster;
        Int64 mftstart;
        byte[] mftSig = new byte[] { 0x46, 0x49, 0x4C, 0x45, 0x30 };

        public struct MFT
        {
            public int magic;
            public short offsetUpdateSequence;
            public short numFixups;
            public Int64 logFileSeqNum;
            public short seqNum;
            public short hardLinkCount;
            public short attrFirstOffset;
            public short flags;
            public int mftSize;
            public int mftAlloc;
            public Int64 fileRefRecord;
            public short nextAttrId;
            public short align;
            public int mftRecordNumber;
            public override string ToString()
            {
                string retval = "==MFT==\n";
                retval += "magic: " + magic + "\n";
                retval += "offsetUpdateSequence: " + offsetUpdateSequence.ToString("X") + "\n";
                retval += "numFixups: " + numFixups + "\n";
                retval += "logFileSeqNum: " + logFileSeqNum.ToString("X") + "\n";
                retval += "seqNum: " + seqNum + "\n";
                retval += "hardLinkCount: " + hardLinkCount.ToString("X") + "\n";
                retval += "attrFirstOffset: " + attrFirstOffset.ToString("X") + "\n";
                retval += "flags: " + flags + "\n";
                retval += "mftSize: " + mftSize.ToString("X") + "\n";
                retval += "mftAlloc: " + mftAlloc.ToString("X") + "\n";
                retval += "fileRefRecord: " + fileRefRecord.ToString("X") + "\n";
                retval += "nextAttrId: " + nextAttrId + "\n";
                retval += "align: " + align + "\n";
                retval += "mftRecordNumber: " + mftRecordNumber.ToString("X") + "\n";
                return retval;
            }
        }
        public struct DB_Entry
        {
            public ATTR_SIA sia;
            public ATTR_FIA fia;
        }
        public enum DOSPERMISSIONS : int
        {
            ReadOnly = 0x0001,
            Hidden = 0x0002,
            System = 0x0004,
            Archive = 0x0020,
            Device = 0x0040,
            Normal = 0x0080,
            Temporary = 0x0100,
            SparseFile = 0x0200,
            ReparsePoint = 0x0400
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ATTR_SIA//0x10
        {
            public Int64 siaCreate;
            public Int64 siaAlter;
            public Int64 siaAccess;
            public Int64 siaMod;
            public DOSPERMISSIONS dosPermissions;
            public int maxNumVersions;
            public int numVersions;
            public int classId;
            public int ownerId;
            public int securityId;
            public Int64 quotaCharged;
            public Int64 updateSequenceNumber;
            public override string ToString()
            {
                string retval = "==ATTR_SIA==\n";
                retval += "siaCreate: " + DateTime.FromFileTime(siaCreate).ToString() + "\n";
                retval += "siaAlter: " + DateTime.FromFileTime(siaAlter).ToString() + "\n";
                retval += "siaAccess: " + DateTime.FromFileTime(siaAccess).ToString() + "\n";
                retval += "siaMod: " + DateTime.FromFileTime(siaMod).ToString() + "\n";
                retval += "dosPermissions: " + dosPermissions + "\n";
                retval += "maxNumVersions: " + maxNumVersions + "\n";
                retval += "numVersions: " + numVersions + "\n";
                retval += "classId: " + classId + "\n";
                retval += "ownerId: " + ownerId + "\n";
                retval += "securityId: " + securityId + "\n";
                retval += "quotaCharged: " + quotaCharged + "\n";
                retval += "updateSequenceNumber: " + updateSequenceNumber + "\n";
                return retval;
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ATTR_LIST//0x20
        {
            public int attrType;
            public short attrLength;
            public byte lengthOfName;
            public byte offsetToName;
            public Int64 startingVCN;
            public Int64 baseFileReference;
            public short attrId;
            public override string ToString()
            {
                string retval = "==ATTR_LIST==\n";
                retval += "attrType: " + attrType + "\n";
                retval += "attrLength: " + attrLength + "\n";
                retval += "lengthOfName: " + lengthOfName + "\n";
                retval += "offsetToName: " + offsetToName + "\n";
                retval += "startingVCN: " + startingVCN + "\n";
                retval += "baseFileReference: " + baseFileReference + "\n";
                retval += "attrId: " + attrId + "\n";
                return retval;
            }
            //public string name;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ATTR_FIA//0x30
        {
            public Int64 fiaReferenceParentDir;
            public Int64 fiaCreate;
            public Int64 fiaAlter;
            public Int64 fiaMod;
            public Int64 fiaAccess;
            public Int64 fiaAllocSize;
            public Int64 fiaRealSize;
            public FLAGS flags;
            public int secId;
            public byte filenameLength;
            public byte filenameSpace;
            public override string ToString()
            {
                string retval = "==ATTR_FIA==\n";
                retval += "fiaReferenceParentDir: " + fiaReferenceParentDir.ToString("X") + "\n";
                retval += "fiaCreate: " + DateTime.FromFileTime(fiaCreate).ToString() + "\n";
                retval += "fiaAlter: " + DateTime.FromFileTime(fiaAlter).ToString() + "\n";
                retval += "fiaMod: " + DateTime.FromFileTime(fiaMod).ToString() + "\n";
                retval += "fiaAccess: " + DateTime.FromFileTime(fiaAccess).ToString() + "\n";
                retval += "fiaAllocSize: " + fiaAllocSize + "\n";
                retval += "fiaRealSize: " + fiaRealSize + "\n";
                retval += "flags: \n";
                foreach (FLAGS flag in Enum.GetValues(typeof(FLAGS)))
                {
                    if ((flags & flag) == flag)
                    {
                        retval += "\t" + flag + "\n";
                    }
                }
                retval += "secId: " + secId + "\n";
                retval += "filenameLength: " + filenameLength + "\n";
                retval += "filenameSpace: " + filenameSpace + "\n";
                return retval;
            }
            //public string filename;
        }
        public enum FLAGS : int
        {
            ReadOnly = 0x0001,
            Hidden = 0x0002,
            System = 0x0004,
            Archive = 0x0020,
            Device = 0x0040,
            Normal = 0x0080,
            Temporary = 0x0100,
            SparseFile = 0x0200,
            ReparsePoint = 0x0400,
            Compressed = 0x0800,
            Offline = 0x1000,
            NotContentIndexed = 0x2000,
            Encrypted = 0x4000,
            Directory = 0x10000000,
            Index = 0x20000000
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ATTR_OBJID//0x40
        {
            public Int64 GUIDObjectId;
            public Int64 GUIDBirthVolId;
            public Int64 GUIDBirthObjId;
            public Int64 GUIDDomainId;
            public override string ToString()
            {
                string retval = "==ATTR_OBJID==\n";
                retval += "GUIDObjectId: " + GUIDObjectId + "\n";
                retval += "GUIDBirthVolId: " + GUIDBirthVolId + "\n";
                retval += "GUIDBirthObjId: " + GUIDBirthObjId + "\n";
                retval += "GUIDDomainId: " + GUIDDomainId + "\n";
                return retval;
            }
        }
        //TODO:needs work
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ATTR_SECDESC//0x50
        {
            public SECID_HEADER header;
            public ACEAUDITFLAGS audit;
            public ACEFLAGS permissions;
            public override string ToString()
            {
                string retval = "==ATTR_SECDESC==\n";
                retval += "header: " + header + "\n";
                retval += "audit: " + audit + "\n";
                retval += "permissions: " + permissions + "\n";
                return retval;
            }
            //public string SIDUSER;
            //public string SIDGROUP;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ATTR_VOLNAME//0x60
        {
            public string filename;
            public override string ToString()
            {
                string retval = "==ATTR_VOLNAME==\n";
                return retval + filename;
            }
            public ATTR_VOLNAME(string str)
            {
                filename = str;
            }
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ATTR_VOLINFO//0x70
        {
            public Int64 zeros;
            public byte major;
            public byte minor;
            public VOLFLAGS flags;
            public int zeroints;
            public override string ToString()
            {
                string retval = "==ATTR_VOLINFO==\n";
                retval += "zeros: " + zeros + "\n";
                retval += "major: " + major + "\n";
                retval += "minor: " + minor + "\n";
                retval += "flags: " + flags + "\n";
                retval += "zeroints: " + zeroints + "\n";
                return retval;
            }
        }
        //VOL info items
        public enum VOLFLAGS : ushort
        {
            Dirty = 0x0001,
            ResizeLogFile = 0x0002,
            UpgradeonMount = 0x0004,
            MountedonNT4 = 0x0008,
            DeleteUSNunderway = 0x0010,
            RepairObjectIds = 0x0020,
            Modifiedbychkdsk = 0x8000
        }
        //end
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ATTR_DATA//0x80
        {
            public byte[] data;
            public override string ToString()
            {
                string retval = "==ATTR_DATA==\n";
                return retval + BitConverter.ToString(data).Replace("-", string.Empty);
            }
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ATTR_INDXROOT//0x90
        {
            public Int64 GUIDObjectId;
            public Int64 GUIDBirthVolId;
            public Int64 GUIDBirthObjId;
            public Int64 GUIDDomainId;
            public override string ToString()
            {
                string retval = "==ATTR_INDXROOT==\n";
                retval += "GUIDObjectId: " + GUIDObjectId + "\n";
                retval += "GUIDBirthVolId: " + GUIDBirthVolId + "\n";
                retval += "GUIDBirthObjId: " + GUIDBirthObjId + "\n";
                retval += "GUIDDomainId: " + GUIDDomainId + "\n";
                return retval;
            }

        }
        //indexroot
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct INDXROOT//0xB0
        {
            public int type;
            public int collation;
            public int indxAlloc;
            public byte clustersPerIndx;
            //pad to nearest 8 bytes
            public byte[] padding;
            public override string ToString()
            {
                string retval = "==INDXROOT==\n";
                retval += "type: " + type + "\n";
                retval += "collation: " + collation + "\n";
                retval += "indxAlloc: " + indxAlloc + "\n";
                retval += "clustersPerIndx: " + clustersPerIndx + "\n";
                retval += "padding: " + BitConverter.ToString(padding).Replace("-", string.Empty) + "\n";
                return retval;
            }
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct INDXHDR//0xC0
        {
            public int offset;
            public int size;
            public int allocSize;
            public INDXFLAGS flags;
            //pad to nearest 8 bytes
            public byte[] padding;
            public override string ToString()
            {
                string retval = "==INDXHDR==\n";
                retval += "offset: " + offset + "\n";
                retval += "size: " + size + "\n";
                retval += "allocSize: " + allocSize + "\n";
                retval += "flags: " + flags + "\n";
                retval += "padding: " + BitConverter.ToString(padding).Replace("-", string.Empty) + "\n";
                return retval;
            }
        }

        public enum INDXFLAGS : byte//
        {
            small = 0,
            large = 1
        }
        //end
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ATTR_INDXALLOC//0xA0
        {
            //public INDXALLOCENTRY[] entries;
            public byte[] entries;
            public override string ToString()
            {
                string retval = "==ATTR_INDXALLOC==\n";
                retval += "padding: " + BitConverter.ToString(entries).Replace("-", string.Empty) + "\n";
                return retval;
            }
        }
        //alloc items
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct INDXALLOCENTRY
        {
            public Int64 reference;
            public short lengthEntry;
            public short lengthStream;
            public INDXENTRYFLAGS flags;
            //The next field is only present when the last entry flag is not set
            public byte[] stream;
            //The next field is only present when the sub-node flag is set
            public Int64 subnode;
            public override string ToString()
            {
                string retval = "==INDXALLOCENTRY==\n";
                retval += "reference: " + reference + "\n";
                retval += "lengthEntry: " + lengthEntry + "\n";
                retval += "lengthStream: " + lengthStream + "\n";
                retval += "flags: " + flags + "\n";
                retval += "stream: " + BitConverter.ToString(stream).Replace("-", string.Empty) + "\n";
                retval += "subnode: " + subnode + "\n";
                return retval;
            }
        }
        public enum INDXENTRYFLAGS : byte
        {
            subnode = 1,
            lastnode = 2
        }
        //end
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ATTR_BITMAP//0xB0
        {
            public int bitfield;
            public override string ToString()
            {
                string retval = "==ATTR_BITMAP==\n";
                retval += "bitfield: " + bitfield + "\n";
                return retval;
            }
        }
        static int GetPaddingCount(int length)
        {
            int retval = 0;
            while ((length + retval) % 8 != 0)
            {
                retval++;
            }
            return retval;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ATTR_REPARSE//0xC0
        {
            public int type;
            public int length;
            public int paddingCount;
            public string data;
            public override string ToString()
            {
                string retval = "==ATTR_REPARSE==\n";
                retval += "type: " + type + "\n";
                retval += "length: " + length + "\n";
                retval += "paddingLength: " + paddingCount.ToString() + "\n";
                retval += "data: " + data + "\n";
                return retval;
            }
            public ATTR_REPARSE(byte[] arr, int offset = 0)
            {
                type = BitConverter.ToInt32(arr, offset);
                length = BitConverter.ToInt16(arr, 4 + offset);
                paddingCount = GetPaddingCount(length + 10);
                System.Text.UnicodeEncoding enc = new UnicodeEncoding();
                data = enc.GetString(arr, offset + paddingCount, length);
            }
        }
        //reparse items
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct REPARSE_DATA
        {
            public short suboffset;
            public short sublength;
            public short offset;
            public short length;
            public byte[] data;
            public override string ToString()
            {
                string retval = "==REPARSE_DATA==\n";
                retval += "suboffset: " + suboffset + "\n";
                retval += "sublength: " + sublength + "\n";
                retval += "offset: " + offset + "\n";
                retval += "length: " + length + "\n";
                retval += "data: " + BitConverter.ToString(data).Replace("-", string.Empty) + "\n";
                return retval;
            }
        }
        public enum REPARSEFLAGS : uint
        {
            alias = 0x20000000,
            latency = 0x40000000,
            microsoft = 0x80000000,
            nss = 0x68000005,
            nssrecover = 0x68000006,
            sis = 0x68000007,
            dfs = 0x68000008,
            mount = 0x88000003,
            hsm = 0xA8000004,
            link = 0xE8000000
        }
        //end
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ATTR_EAINFO//0xD0
        {
            public short packedSize;
            public short attrCount;
            public int unpackedSize;
            public override string ToString()
            {
                string retval = "==ATTR_EAINFO==\n";
                retval += "packedSize: " + packedSize + "\n";
                retval += "attrCount: " + attrCount + "\n";
                retval += "unpackedSize: " + unpackedSize + "\n";
                return retval;
            }
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ATTR_EA//0xE0
        {
            public int off;
            public byte flags;
            public byte nameLength;
            public short valueLength;
            public string name;
            public string value;
            public override string ToString()
            {
                string retval = "==ATTR_EA==\n";
                retval += "offset: " + off + "\n";
                retval += "flags: " + flags + "\n";
                retval += "nameLength: " + nameLength + "\n";
                retval += "valueLength: " + valueLength + "\n";
                retval += "name: " + name + "\n";
                retval += "value: " + value + "\n";
                return retval;
            }
            public ATTR_EA(byte[] arr, int offset = 0)
            {
                System.Text.ASCIIEncoding enc = new ASCIIEncoding();
                off = BitConverter.ToInt32(arr, offset);
                flags = arr[4 + offset];
                nameLength = arr[5 + offset];
                valueLength = BitConverter.ToInt16(arr, 6 + offset);
                name = enc.GetString(arr, 8 + offset, nameLength);
                value = "";
            }
        }
        //ea items
        public enum EAFLAGS : byte
        {
            needEa = 0x80

        }
        //end
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ATTR_LOGGEDSTREAM//0x100
        {
            public byte[] data;
            public override string ToString()
            {
                return BitConverter.ToString(data).Replace("-", string.Empty);
            }
        }
        //ATTR_SECDESC structs
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SECID_HEADER
        {
            public byte revision;
            public byte padding;
            public short flags;
            public int offsetUserSid;
            public int offsetGroupSid;
            public int offsetSACL;
            public int offsetDACL;
            public override string ToString()
            {
                string retval = "==SECID_HEADER==\n";
                retval += "revision: " + revision + "\n";
                retval += "padding: " + padding + "\n";
                retval += "flags: " + flags + "\n";
                retval += "offsetUserSid: " + offsetUserSid + "\n";
                retval += "offsetGroupSid: " + offsetGroupSid + "\n";
                retval += "offsetSACL: " + offsetSACL + "\n";
                retval += "offsetDACL: " + offsetDACL + "\n";
                return retval;
            }
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SECID_ACL
        {
            public byte revision;
            public byte padding;
            public short size;
            public short acecount;
            public short padding1;
            public override string ToString()
            {
                string retval = "==SECID_ACL==\n";
                retval += "revision: " + revision + "\n";
                retval += "padding: " + padding + "\n";
                retval += "size: " + size + "\n";
                retval += "acecount: " + acecount + "\n";
                retval += "padding1: " + padding1 + "\n";
                return retval;
            }
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SECID_ACE
        {
            public ACETYPES type;
            public ACEFLAGS flags;
            public short size;
            public int mask;
            public byte[] sid;
            public override string ToString()
            {
                string retval = "==SECID_ACE==\n";
                retval += "type: " + type + "\n";
                retval += "flags: " + flags + "\n";
                retval += "size: " + size + "\n";
                retval += "mask: " + mask + "\n";
                retval += "sid: " + BitConverter.ToString(sid).Replace("-", string.Empty) + "\n";
                return retval;
            }
        }
        public enum ACEFLAGS
        {
            objInherit = 1,
            conInherit = 0x2,
            noPropagate = 0x4,
            inheritOnly = 0x8,
        }
        public enum SECDESCCTRLFLAGS : ushort
        {
            owner = 1,
            group = 0x2,
            daclPresent = 0x4,
            daclDefaulted = 0x8,
            saclPresent = 0x10,
            saclDefaulted = 0x20,
            daclAutoInheritReq = 0x100,
            saclAutoInheritReq = 0x200,
            daclAutoInherit = 0x400,
            saclAutoInherit = 0x800,
            daclProtected = 0x1000,
            saclProtected = 0x2000,
            rmControl = 0x4000,
            selfRelative = 0x8000
        }
        public enum ACETYPES
        {
            allowed = 0x0,
            denied = 0x1,
            audit = 0x2
        }
        public enum ACEAUDITFLAGS
        {
            success = 0x40,
            fail = 0x80
        }
        public enum SECDESCFLAGS
        {
            OwnerDefaulted = 0x0001,
            GroupDefaulted = 0x0002,
            DACLPresent = 0x0004,
            DACLDefaulted = 0x0008,
            SACLPresent = 0x0010,
            SACLDefaulted = 0x0020,
            DACLAutoInheritReq = 0x0100,
            SACLAutoInheritReq = 0x0200,
            DACLAutoInherited = 0x0400,
            SACLAutoInherited = 0x0800,
            DACLProtected = 0x1000,
            SACLProtected = 0x2000,
            RMControlValid = 0x4000,
            SelfRelative = 0x8000
        }
    }
}
