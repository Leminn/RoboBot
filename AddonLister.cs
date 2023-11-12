using System;
using System.IO;
using System.Text;


namespace RoboBot;
// All of this code belongs to Ors (Github: Riku-S). Ors is cool. Thanks Ors.
public class AddonLister
{
    int curByte = 0;
    public string? fileName = "";

    static byte[] DEMOHEADERBYTES =
    {
        0xF0, 0x53, 0x52, 0x42, 0x32, 0x52, 0x65, 0x70, 0x6C, 0x61, 0x79, 0x0F
    }; // hackish method to spell out "ðSRB2Replay", required for file verification

    string DEMOHEADER = Encoding.UTF8.GetString(DEMOHEADERBYTES);

    string ReadByteString(int start, int length, byte[] demoBytes)
    {
        byte[] bytes = new byte[length];
        Stream stream = new MemoryStream(demoBytes);
        using (BinaryReader reader = new BinaryReader(stream))
        {
            reader.BaseStream.Seek(start, SeekOrigin.Begin);
            reader.Read(bytes, 0, length);
        }

        curByte += length;
        return Encoding.UTF8.GetString(bytes);
    }

    uint ReadUInt8(int start, byte[] demoBytes)
    {
        byte[] bytes = new byte[1];
        Stream stream = new MemoryStream(demoBytes);
        using (BinaryReader reader = new BinaryReader(stream))
        {
            reader.BaseStream.Seek(start, SeekOrigin.Begin);
            reader.Read(bytes, 0, 1);
        }

        curByte++;
        return bytes[0];
    }

    Int16 ReadSInt16(int start, byte[] demoBytes)
    {
        byte[] bytes = new byte[2];
        Stream stream = new MemoryStream(demoBytes);
        using (BinaryReader reader = new BinaryReader(stream))
        {
            reader.BaseStream.Seek(start, SeekOrigin.Begin);
            reader.Read(bytes, 0, 2);
        }

        curByte += 2;
        return BitConverter.ToInt16(bytes, 0);
    }

    int ReadSInt8(int start, byte[] demoBytes)
    {
        byte[] bytes = new byte[1];
        Stream stream = new MemoryStream(demoBytes);
        using (BinaryReader reader = new BinaryReader(stream))
        {
            reader.BaseStream.Seek(start, SeekOrigin.Begin);
            reader.Read(bytes, 0, 1);
        }

        curByte++;
        return bytes[0];
    }

    UInt16 ReadUInt16(int start, byte[] demoBytes)
    {
        byte[] bytes = new byte[2];
        Stream stream = new MemoryStream(demoBytes);
        using (BinaryReader reader = new BinaryReader(stream))
        {
            reader.BaseStream.Seek(start, SeekOrigin.Begin);
            reader.Read(bytes, 0, 2);
        }

        curByte += 2;
        return BitConverter.ToUInt16(bytes, 0);
    }

    public string[] GetFilesFromReplay(byte[] bytes)
    {
        uint version, subVersion, demoVersion, mapNumber, demoFlag, demoRings, charAbility, charAbility2, pflags;
        string playMetal, playerName, charName, colorName;

        curByte = 0;

        // read demo header
        string demoHeader = ReadByteString(curByte, 12, bytes);
        if (demoHeader != DEMOHEADER)
        {
            Console.WriteLine(fileName + " is not an SRB2 replay file.");
            return new string[0];
        }

        // Demo Version
        curByte += 2;
        //version = ReadUInt8(curByte, bytes);
        //subVersion = ReadUInt8(curByte, bytes);
        demoVersion = ReadUInt16(curByte, bytes);

        // Demo Checksum
        curByte += 16;

        // Player or Metal demo
        playMetal = ReadByteString(curByte, 4, bytes);

        if (playMetal != "PLAY")
        {
            Console.WriteLine(fileName + " is not a valid replay file.");
            return new string[0];
        }

        // Map Info
        curByte += 18;

        // Demo Flag
        curByte += 1;

        if (demoVersion < 0x10)
        {
            // demo has no file list
            return new string[0];
        }

        UInt16 totalFiles = ReadUInt16(curByte, bytes);
        ;
        string[] fileNames = new string[totalFiles];

        for (int i = 0; i < totalFiles; ++i)
        {
            string name = "";
            while (true)
            {
                uint b = ReadUInt8(curByte, bytes);
                if (b == 0) break;
                else
                    name += (char)b;
            }

            curByte += 16; // md5
            fileNames[i] = name;
        }

        return fileNames;
    }



    //
    // while (string.IsNullOrEmpty(fileName))
    // {
    //     fileName = Console.ReadLine();
    // }
    //
        // byte[] file = File.ReadAllBytes(fileName);
    //
    //
    // string[] files = GetFilesFromReplay(file);
    //     foreach (string sFile in files)
    // {
    //     Console.WriteLine(sFile);
    // }
}