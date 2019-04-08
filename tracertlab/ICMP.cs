using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tracertlab
{
    class ICMP
    {
        const int typeRequest = 8, codeRequest = 0, sizeHeader = 8;
        public byte type;
        public byte code;
        public UInt16 checksum;
        public int mesLength;
        public byte[] Message = new byte[1024];

        public ICMP() { }
        public ICMP(int length, byte[] message)
        {
            type = message[20];
            code = message[21];
            checksum = BitConverter.ToUInt16(message, 22);
            mesLength = length - 24;
            Buffer.BlockCopy(message, 24, Message, 0, mesLength);
        }

        public byte[] getBytes()
        {
            byte[] msg = new byte[mesLength + 9];
            Buffer.BlockCopy(BitConverter.GetBytes(type), 0, msg, 0, 1);
            Buffer.BlockCopy(BitConverter.GetBytes(code), 0, msg, 1, 1);
            Buffer.BlockCopy(BitConverter.GetBytes(checksum), 0, msg, 2, 2);
            Buffer.BlockCopy(Message, 0, msg, 4, mesLength);
            return msg;
        }

        public UInt16 CountedChecksum()
        {
            UInt32 currChecksum = 0;
            byte[] msg = getBytes();
            int packetsize = mesLength + sizeHeader;
            int index = 0;

            while (index < packetsize)
            {
                currChecksum += Convert.ToUInt32(BitConverter.ToUInt16(msg, index));
                index += 2;
            }
            currChecksum = (currChecksum >> 16) + (currChecksum & 0xffff);
            currChecksum += (currChecksum >> 16);
            return (UInt16)(~currChecksum);
        }

        public int CreateICMPrequest(byte[] msg, short number)
        {
            //эхо-запрос

            type = typeRequest;
            code = codeRequest;
            checksum = 0;
            Buffer.BlockCopy(new byte[] { 0, 1 }, 0, Message, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(number), 0, Message, 2, 2);
            Buffer.BlockCopy(msg, 0, Message, 4, msg.Length);
            mesLength = msg.Length + 4;
            checksum = CountedChecksum();
            return msg.Length + sizeHeader;
        }

        public void changeIdNumber(short number)
        {
            checksum = 0;
            Buffer.BlockCopy(BitConverter.GetBytes(number), 0, Message, 2, 2);
            checksum = CountedChecksum();
        }
    }
}
