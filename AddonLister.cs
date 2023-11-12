using System;
using System.Linq;
using System.Text;

namespace RoboBot;

// All of this code belongs to Ors (Github: Riku-S). Ors is cool. Thanks Ors.
public static class AddonLister
{
    private static int _curByte;
    private static byte[] _demoBytes = Array.Empty<byte>();

    // Hackish method to spell out "ðSRB2Replay", required for file verification
    private static readonly byte[] DemoHeaderBytes =
    {
        0xF0, 0x53, 0x52, 0x42, 0x32, 0x52, 0x65, 0x70, 0x6C, 0x61, 0x79, 0x0F
    };

    private static readonly string DemoHeader = Encoding.UTF8.GetString(DemoHeaderBytes);

    private static string ReadByteString(int count)
    {
        string byteString = Encoding.UTF8.GetString(_demoBytes, _curByte, count);
        
        _curByte += count;
        return byteString;
    }
    
    private static byte[] ReadBytes(int count)
    {
        byte[] bytes = _demoBytes[_curByte..(_curByte + count)];
        
        _curByte += count;
        return bytes;
    }

    private static uint ReadUInt8()
    {
        byte uint8 = _demoBytes[_curByte];
        
        _curByte++;
        return uint8;
    }

    private static ushort ReadUInt16()
    {
        ushort uint16 = BitConverter.ToUInt16(_demoBytes, _curByte);

        _curByte += 2;
        return uint16;
    }

    public static (string fileName, string md5)[] GetFilesFromReplay(byte[] bytes)
    {
        _curByte = 0;
        _demoBytes = bytes;

        // read demo header
        if (ReadByteString(12) != DemoHeader)
            return Array.Empty<(string, string)>();

        // Demo Version
        _curByte += 2;
        if (ReadUInt16() < 0x10)
            return Array.Empty<(string, string)>(); // Demo has no file list

        // Demo Checksum
        _curByte += 16;

        // Player or Metal demo
        if (ReadByteString(4) != "PLAY")
            return Array.Empty<(string, string)>();

        // Map Info
        _curByte += 18;

        // Demo Flag
        _curByte += 1;

        UInt16 totalFiles = ReadUInt16();
        
        (string fileNames, string md5)[] files = new (string, string)[totalFiles];

        for (int i = 0; i < totalFiles; ++i)
        {
            string name = "";
            while (true)
            {
                uint b = ReadUInt8();
                if (b == 0)
                    break;
                
                name += (char)b;
            }
            
            // Read the MD5: Read the bytes, transform them into their string hex representation and concatenate them all
            string md5 = string.Concat(ReadBytes(16).Select(x => x.ToString("X")));
            
            files[i] = (name, md5);
        }

        return files;
    }
}