using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCFGSextractor
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Args: path\\to\\mcfg_sw.mbn");
            if (args.Length < 1)
                return;
            string OutDir = Path.GetDirectoryName(args[0]) + Path.DirectorySeparatorChar + "output" + Path.DirectorySeparatorChar;
            using (var file = File.OpenRead(args[0]))

            using (var reader = new BinaryReader(file))
            {
                var pattern = new byte[] { 0x00, 0x00, 0x02, 0x19, 0x00, 0x00, 0x01 };
                var pattern2 = new byte[] { 0x00, 0x00, 0x02, 0x09, 0x00, 0x00, 0x01 };
                // pattern3 has 32bits length
                var pattern3 = new byte[] { 0x02, 0x00, 0x10, 0x19, 0x00, 0x00, 0x01 };

                Queue<byte> queue = new Queue<byte>(pattern.Length);

                byte[] buffer = new byte[1];
                while (1 == reader.Read(buffer, 0, 1))
                {
                    if (queue.Count == pattern.Length)
                    {
                        queue.Dequeue();
                    }

                    queue.Enqueue(buffer[0]);
                    var length_size = 0;
                    if (Matches(queue, pattern) || Matches(queue, pattern2))
                    {
                        length_size = 16;
                    }
                    else if (Matches(queue, pattern3))
                    {
                        length_size = 32;
                    }


                    if (length_size > 0)
                    {
                        // The input is positioned after the last read byte, which
                        // completed the pattern.
                        //Console.WriteLine("Found pattern at " + file.Position);
                        //reader.ReadByte();
                        reader.ReadByte();
                        //var length = reader.ReadByte();
                        //reader.ReadByte();
                        //Console.WriteLine(file.Position);
                        var strLen = reader.ReadUInt16();
                        if (strLen < 1)
                            continue;
                        var filename = System.Text.Encoding.UTF8.GetString(reader.ReadBytes(strLen - 1));
                        Console.WriteLine(filename);
                        //Console.WriteLine(file.Position);
                        reader.ReadByte();
                        reader.ReadByte();
                        reader.ReadByte();

                        var length = 0L;
                        if (length_size == 32)
                        {
                            length = reader.ReadUInt32();
                        }
                        else
                        {
                            length = reader.ReadUInt16();
                        }

                        if (length < 1)
                            continue;
                        reader.ReadByte();
                        var contents = reader.ReadBytes((int)(length - 1));
                        //var contents = reader.ReadString();
                        //Console.WriteLine(length);
                        var dirname = Path.GetDirectoryName(OutDir + filename);
                        if (!Directory.Exists(dirname))
                        {
                            Directory.CreateDirectory(dirname);
                        }
                        File.WriteAllBytes(OutDir + filename, contents);
                    }
                }
            }
            //Console.ReadLine();
        }
        private static bool Matches(Queue<byte> data, byte[] pattern)
        {
            return data.SequenceEqual(pattern);
        }
    }
}
