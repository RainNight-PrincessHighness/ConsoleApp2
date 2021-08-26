/*
 本程序遵循开源协议 原创者为GeminiYeyu
 请勿随意二次开发 此源码仅供学习参考 
 */
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ConsoleApp2
{
    class Program
    {
        static void Main(string[] args)
        {
            
        }

        public class NAT
        {
            int localProt { get; set; }
            string localIp { get; set; }
            int TargetPort { get; set; }
            string TargetIp { get; set; }
            public NAT(string localIp, int localProt, string TargetIp, int TargetPort)
            {
                this.localIp = localIp;
                this.localProt = localProt;
                this.TargetIp = TargetIp;
                this.TargetPort = TargetPort;
            }

            public NAT()
            {
                this.localIp = "0.0.0.0";
                this.localProt = 8080;
                this.TargetIp = "0.0.0.0";
                this.TargetPort = 80;
            }

            public void Run()
            {
                  
                IPAddress ip = IPAddress.Parse(localIp);
                Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); ;
                serverSocket.Bind(new IPEndPoint(ip, localProt));
                serverSocket.Listen(10000);
                Console.WriteLine("启动监听{0}成功", serverSocket.LocalEndPoint.ToString());
                Thread myThread = new Thread(Listen);
                myThread.Start(serverSocket);
            }

            
            private void Listen(object obj)
            {
                Socket serverSocket = (Socket)obj;
                IPAddress ip = IPAddress.Parse(TargetIp);
                while (true)
                {
                    Socket tcp1 = serverSocket.Accept();
                    Socket tcp2 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    tcp2.Connect(new IPEndPoint(ip, TargetPort));
                    
                    ThreadPool.QueueUserWorkItem(new WaitCallback(SwapMsg), new thSock
                    {
                        tcp1 = tcp2,
                        tcp2 = tcp1
                    });
                    
                    ThreadPool.QueueUserWorkItem(new WaitCallback(SwapMsg), new thSock
                    {
                        tcp1 = tcp1,
                        tcp2 = tcp2
                    });
                }
            }   
            public void SwapMsg(object obj)
            {
                thSock mSocket = (thSock)obj;
                while (true)
                {
                    try
                    {
                        byte[] result = new byte[1024];
                        int num = mSocket.tcp2.Receive(result, result.Length, SocketFlags.None);
                        if (num == 0) 
                        {
                            if (mSocket.tcp1.Connected)
                            {
                                mSocket.tcp1.Close();
                            }
                            if (mSocket.tcp2.Connected)
                            {
                                mSocket.tcp2.Close();
                            }
                            break;
                        }
                        mSocket.tcp1.Send(result, num, SocketFlags.None);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        if (mSocket.tcp1.Connected)
                        {
                            mSocket.tcp1.Close();
                        }
                        if (mSocket.tcp2.Connected)
                        {
                            mSocket.tcp2.Close();
                        }
                        break;
                    }
                }
            }

        }

        public class thSock
        {
            public Socket tcp1 { get; set; }
            public Socket tcp2 { get; set; }
        }
    }
}
