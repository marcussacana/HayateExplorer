using AdvancedBinary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HayateManager {
    public class FileSystem {
        StructReader BIN;
        StructReader[] LBA;

        int Open = -1;
        public FileSystem(Stream BIN, Stream[] LBA) {
            this.BIN = new StructReader(BIN);
            this.LBA = (from x in LBA select new StructReader(x)).ToArray();
        }

        public string[] ListFiles() {
            if (Open == -1) {
                BIN.BaseStream.Position = 0;
                var tmp = new BINHeader();
                BIN.ReadStruct(ref tmp);
                return (from x in tmp.Offsets select GetFileName(x)).ToArray();
            }

            HEDFormat Header = LoadHeader();
            return (from x in Header.Offsets select GetFileName(x)).ToArray();
        }

        public void OpenPackget(Stream Packget) {
            for (int i = 0; i < LBA.Length && Packget != null; i++)
                if (Packget.GetHashCode() == LBA[i].BaseStream.GetHashCode()) {
                    Open = i;
                    return;
                }

            Open = -1;
        }
        private string GetFileName(uint Offset) {
            return Offset.ToString("X8") + ".bin";
        }

        private HEDFormat LoadHeader() {
            int Bak = Open;
            Open = -1;
            foreach (string FileName in ListFiles()) {
                Stream File = OpenFile(FileName);
                StructReader Temp = new StructReader(File);
                Temp.BaseStream.Position = 0;
                if (Temp.ReadString(StringStyle.CString) != "HED")
                    continue;
                Temp.BaseStream.Position = 0;

                var Header = new HEDFormat();
                Temp.ReadStruct(ref Header);

                if (LBA[Bak].BaseStream.Length == Header.LBALength) {
                    Open = Bak;
                    return Header;
                }
            }
            Open = Bak;
            throw new Exception("Failed to Open LBA Header");
        }
        public Stream OpenFile(string FileName) {
            if (Open == -1) {
                BIN.BaseStream.Position = 0;
                var Header = new BINHeader();
                BIN.ReadStruct(ref Header);

                for (int i = 0; i < Header.Offsets.Length; i++) {
                    string FN = GetFileName(Header.Offsets[i]);
                    if (Path.GetFileNameWithoutExtension(FN).ToLower() != Path.GetFileNameWithoutExtension(FileName).ToLower())
                        continue;

                    uint FileEnd = (i + 1 < Header.Offsets.Length) ? Header.Offsets[i + 1] : (uint)(Header.BinLen + BIN.BaseStream.Position);
                    return new VirtStream(BIN.BaseStream, Header.Offsets[i], FileEnd - Header.Offsets[i]);
                }

                throw new Exception("File Not Found");
            }

            var Info = LoadHeader();
            for (int i = 0; i < Info.Offsets.Length; i++) {
                string FN = GetFileName(Info.Offsets[i]);
                if (Path.GetFileNameWithoutExtension(FN).ToLower() != Path.GetFileNameWithoutExtension(FileName).ToLower())
                    continue;

                uint FileEnd = (i + 1 < Info.Offsets.Length) ? Info.Offsets[i + 1] : (uint)LBA[Open].BaseStream.Length;
                return new VirtStream(LBA[Open].BaseStream, Info.Offsets[i], FileEnd - Info.Offsets[i]);
            }

            throw new Exception("File Not Found");
        }

        internal struct BINHeader {
            [PArray]
            public uint[] Offsets;

            public uint BinLen;//Not Sure
        }

        internal struct HEDFormat {
            [FString(Length = 0x4)]
            public string Signature;

            public uint Unk1;

            [PArray(PrefixType = Const.INT64)]//The correct is uint32, but i'm with lazy to setup.
            public uint[] Offsets;

            public uint LBALength;
        }
    }
}
