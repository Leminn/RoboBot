using System;
using System.Linq;
using System.Text;

namespace RoboBot
{
    internal class JobInfo
    {
        private class BadJobException<T> : Exception
        {
            public override string Message { get; }
            public T Property { get; }

            public BadJobException(string message, T property) : base(message)
            {
                Message = message;
                Property = property;
            }
        }

        public const int AddonNameSize = 40;
        public const int MaxAddons = 5;
        public bool HasAddons { get; }
        public byte AddonsCount { get; } // ignored if (!hasAddons)
        public byte[][] AddonsFileNames { get; } // ignored if (!hasAddons)

        public static JobInfo NoAddons = new JobInfo();

        public static JobInfo CreateFromStrings(byte addonsCount, string[] addonsFileNames)
        {
            byte[][] addonsNamesConverted = new byte[addonsCount][];
            for (int i = 0; i < addonsCount; i++)
            {
                addonsNamesConverted[i] = Encoding.ASCII.GetBytes(addonsFileNames[i]);
            }

            return new JobInfo(addonsCount, addonsNamesConverted);
        }

        public JobInfo(byte addonsCount, byte[][] addonsFileNames)
        {
            HasAddons = true;
            AddonsCount = addonsCount != 0 ? addonsCount : throw new BadJobException<byte>("AddonsCount can't be 0 if it contains addons", addonsCount);

            if (addonsFileNames.Any(addon => addon.Length > AddonNameSize))
                throw new BadJobException<byte[][]>("An addon file name can't be greater than 40 bytes", addonsFileNames);

            AddonsFileNames = addonsFileNames;
        }

        public JobInfo()
        {
            HasAddons = false;
        }

        public byte[] ToBytes()
        {
            byte[] bytes = new byte[202];

            if (HasAddons)
            {
                bytes[0] = 1;
                bytes[1] = AddonsCount;
                int curByte = 2;
                byte[] blankBytes = new byte[40];
                for (int i = 0; i < MaxAddons; i++)
                {
                    if (i >= AddonsFileNames.Length)
                        blankBytes.CopyTo(bytes, curByte);
                    else
                        AddonsFileNames[i].CopyTo(bytes, curByte);

                    curByte += AddonNameSize;
                }
                return bytes;
            }

            bytes[0] = 0;
            return bytes;
        }

        public static JobInfo FromBytes(ref byte[] bytes)
        {
            if (bytes[0] == 1)
            {
                byte addonsCount = bytes[1];
                byte[][] addonFileNames = new byte[addonsCount][];

                int curByte = 2;
                for (int i = 0; i < addonsCount; i++)
                {
                    addonFileNames[i] = bytes.Skip(curByte).Take(AddonNameSize).ToArray();
                    curByte += AddonNameSize;
                }

                return new JobInfo(addonsCount, addonFileNames);
            }

            return new JobInfo();
        }
    }
}