using System;
using System.IO;
using System.Text;

namespace MutalkLib
{
    public class Crc32
    {
        private readonly uint[] ChecksumTable;
        private readonly uint Polynomial = 0xEDB88320;

        public Crc32()
        {
            ChecksumTable = new uint[0x100];

            for (uint index = 0; index < 0x100; ++index)
            {
                uint item = index;
                for (int bit = 0; bit < 8; ++bit)
                    item = ((item & 1) != 0) ? (Polynomial ^ (item >> 1)) : (item >> 1);
                ChecksumTable[index] = item;
            }
        }

        public int ComputeHashFromString(string str)
        {
            using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(str));
            var hash = ComputeHash(stream);

            if (BitConverter.IsLittleEndian) Array.Reverse(hash);

            return BitConverter.ToInt32(hash, 0);
        }

        public byte[] ComputeHash(Stream stream)
        {
            uint result = 0xFFFFFFFF;

            int current;
            while ((current = stream.ReadByte()) != -1)
                result = ChecksumTable[(result & 0xFF) ^ (byte)current] ^ (result >> 8);

            byte[] hash = BitConverter.GetBytes(~result);
            Array.Reverse(hash);
            return hash;
        }

        public byte[] ComputeHash(byte[] data)
        {
            return ComputeHash(new MemoryStream(data));
        }
    }
}
