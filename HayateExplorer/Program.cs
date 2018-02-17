using HayateManager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HayateExplorer {
    class Program {
        static void Main(string[] args) {
            Console.Title = "Hayate Explorer";

            if (args == null || args.Length < 1) {
                Console.WriteLine("Drag&Drop the .bin file inside the original game directory");
                Console.ReadKey();
                return;
            }
            foreach (string Packget in args) {
                Stream Main = new StreamReader(Packget).BaseStream;
                string Dir = Path.GetDirectoryName(Packget);
                if (Dir.Trim('\\') == string.Empty)
                    Dir = ".\\";

                string[] LBAPath = Directory.GetFiles(Dir, "*.lba");
                Stream[] LBA = (from x in LBAPath select new StreamReader(x).BaseStream).ToArray();

                FileSystem FS = new FileSystem(Main, LBA);

                Console.WriteLine("Extracting: {0}", Path.GetFileName(Packget));
                bool Continue = Extract(FS, Packget + "~\\");

                if (Continue && LBA.Length == 0) {
                    Console.WriteLine("ERROR - FAILED TO OPEN LBA PACKGET");
                    continue;
                }

                if (Continue) {
                    for (int i = 0; i < LBA.Length; i++) {
                        Console.WriteLine("Extracting: {0}", Path.GetFileName(LBAPath[i]));
                        FS.OpenPackget(LBA[i]);
                        Extract(FS, LBAPath[i] + "~\\");
                    }
                }
            }

            Console.Title = "Hayate Explorer";
            Console.WriteLine("Sucess, Press a Key To Exit");
            Console.ReadKey();
        }

        private static bool Extract(FileSystem FileSystem, string Path) {
            if (Directory.Exists(Path))
                Directory.Delete(Path, true);

            Directory.CreateDirectory(Path);
            string[] Files = FileSystem.ListFiles();

            bool ContainsHed = false;
            foreach (string File in Files) {
                Console.Title = "Processing: " + File;
                Stream Data = FileSystem.OpenFile(File);
                string FN = System.IO.Path.GetFileNameWithoutExtension(File) + GetExtension(Data);
                if (FN.EndsWith(".hed"))
                    ContainsHed = true;
                Stream Output = new StreamWriter(Path + FN).BaseStream;
                Data.CopyTo(Output);
                Output.Close();
            }

            return ContainsHed;
        }

        private static string GetExtension(Stream File) {
            Format[] Formats = new Format[] {
                new Format() {
                    Signature = 0x46464952u,
                    Extension = ".wav"
                }
            };
            long Pos = File.Position;
            byte[] Arr = new byte[4];
            if (File.Read(Arr, 0, Arr.Length) != Arr.Length) {
                File.Position = Pos;
                return ".bin";
            }

            File.Position = Pos;
            uint Signature = BitConverter.ToUInt32(Arr, 0);

            foreach (Format Format in Formats) {
                if (Signature == Format.Signature)
                    return Format.Extension;
            }

            bool OK = true;
            for (int i = 0; i < Arr.Length;  i++) {
                if (!IsASCII(Arr[i]) && Arr[i] != 0x00)
                    OK = false;
            }

            if (OK) {
                string Ext =  "." + Encoding.ASCII.GetString(Arr).Trim(' ', '\x0', '.').ToLower();
                if (Ext != ".")
                    return Ext;
            }

            return ".bin";
        }


        internal static bool IsASCII(byte Byte) {
            return (Byte >= 0x61 && Byte <= 0x7A) || (Byte >= 0x41 && Byte <= 0x5A);
        }

        private struct Format {
            public uint Signature;
            public string Extension;
        }
    }
}
