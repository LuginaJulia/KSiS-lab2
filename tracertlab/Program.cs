using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace tracertlab
{
    class Program
    {
        const int TimeToLiveExc = 11, TypeReply = 0, maxTtl = 30;

        static int Main(string[] args)
        {
            short number = 1;
            const string teststring = "1234";
            const int mesSize = 1024;                               
            string IPAddr = args[0];
            bool getDomen = false;
            if ((args.Length==2)&&(args[1]=="-d"))
            {
                getDomen = true;
            }
            IPAddress IP;
            IPEndPoint IPEndPnt;
            bool RightIP = IPAddress.TryParse(IPAddr, out IP);
            if (RightIP)
            {
                IPEndPnt = new IPEndPoint(IPAddress.Parse(IPAddr), 0);
            }
            else
            {
                try
                {
                    IPHostEntry iphe = Dns.GetHostEntry(IPAddr);
                    IPEndPnt = new IPEndPoint(iphe.AddressList[0], 0);
                }
                catch 
                {
                    Console.WriteLine("error! Please check entered data and try again");
                    return 1;
                    throw;           
                }
            }
            byte[] message = new byte[mesSize];
            message = Encoding.ASCII.GetBytes(teststring);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);
            EndPoint EndPnt = IPEndPnt;
            ICMP packet = new ICMP();
            int ICMPsize;
            int recv;
            const int timeout = 3000;
            ICMPsize = packet.CreateICMPrequest(message, number); //создаем ICMP-пакет для запроса
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, timeout);
            Console.WriteLine("Tracing route to {0}", IPEndPnt.ToString());
            short ttl = 1;
            int start, end;
            bool epIsReached = false;
            do 
            {
                Console.Write("{0}", ttl);
                for ( int i=0; i<3; i++) 
                {
                    packet.changeIdNumber(number);
                    socket.Ttl = ttl;
                    start = Environment.TickCount;
                    
                    socket.SendTo(packet.getBytes(), ICMPsize, SocketFlags.None, IPEndPnt);                   
                    try
                    {
                        message = new byte[mesSize];
                        recv = socket.ReceiveFrom(message, ref EndPnt);
                        ICMP reply = new ICMP(recv, message);
                        end = Environment.TickCount;                       
                        if (reply.type == TimeToLiveExc)
                        {
                            Console.Write(" {0}ms ", end - start);
                            if (i == 2) {
                                Console.Write(" {0}  ", EndPnt.ToString());
                                if (getDomen)
                                {
                                    GetDomainName(EndPnt);
                                }
                                
                            }
                        }
                        else
                        {
                            if (reply.type == TypeReply)
                            {
                                epIsReached = true;
                                Console.Write(" {0}ms ", end - start);
                                if (i == 2) {
                                    Console.Write(" {0} ", EndPnt.ToString());
                                    if (getDomen)
                                    {
                                        GetDomainName(EndPnt);
                                    }

                                }
                                                               
                            }
                        }

                    }
                    catch (SocketException)
                    {
                        Console.Write("* ");
                    }
                    number++;
                }
                Console.WriteLine();
                ttl++;
            }while ((ttl< maxTtl) && (!epIsReached));

            if (epIsReached)
            {
                Console.WriteLine("Trace complete");
            }

            socket.Close();
            return 0;
        }
        public static void GetDomainName(EndPoint EndPnt)
        {
                IPHostEntry domen;
                try
                {
                    IPEndPoint ip = (IPEndPoint)EndPnt;
                    IPAddress ip1 = ip.Address;
                    domen = Dns.GetHostEntry(ip1.ToString());
                    Console.Write("[ {0} ]", domen.HostName);
                }
                catch
                {
                    Console.Write("-");
                }
        }
    }
}
