using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;

namespace ConsoleServer
{
    class Program
    {
        // порт на котором будет прослушивать сервер на предмет входяшего броткаста от клиента.
        public static int ServerPort_Listen = 65000;
        // порт на котором программа будет взаимодействовать с клиентом.
        public static int ServerPort_WorkWithClient = 7100;
        public static Boolean working = false;

        static void Main(string[] args)
        {
            Thread workThread = new Thread(work);
            workThread.Start();

            Console.WriteLine("Search server...");
            //UdpClient for reading incoming data.
            UdpClient receivingUdpClient = new UdpClient(ServerPort_Listen);
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            Console.WriteLine("Ready! =)");
            while (true)
            {
                if (working) { break; }
                try
                {
                    // Blocks until a message returns on this socket from a remote host.
                    Byte[] receiveBytes = receivingUdpClient.Receive(ref RemoteIpEndPoint);

                    string returnData = Encoding.UTF8.GetString(receiveBytes);

                    byte[] data = Encoding.UTF8.GetBytes(ServerPort_WorkWithClient.ToString());
                    receivingUdpClient.Send(data, data.Length, new IPEndPoint(RemoteIpEndPoint.Address, RemoteIpEndPoint.Port));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            receivingUdpClient.Close();
        }

        public static void work()
        {
            //установка для сокета локальную конечную точку
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, ServerPort_WorkWithClient);

            //создаем сокет tcp/ip 
            Socket MySocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //назначаем сокет локальной конечной точке и слушаем входящии сокеты

            try
            {
                MySocket.Bind(ipEndPoint);
                MySocket.Listen(1);

                //начинаем слушать соеденения
                while (true)
                {
                    Socket handler = MySocket.Accept();
                    byte[] bytes = new byte[1024 * 8 * 8];
                    int ByteRec = 0;
                    String Data = null;

                    ByteRec = handler.Receive(bytes);
                    Data = Encoding.UTF8.GetString(bytes, 0, ByteRec);
                    Matrix work = new Matrix();
                    work.InstalSize(1, Data);
                    handler.Send(Encoding.UTF8.GetBytes("<OK>"));

                    ByteRec = handler.Receive(bytes);
                    Data = Encoding.UTF8.GetString(bytes, 0, ByteRec);
                    work.InstalSize(2, Data);
                    handler.Send(Encoding.UTF8.GetBytes("<OK>"));

                    work.InstalMatrixMamory();

                    ByteRec = handler.Receive(bytes);
                    Data = Encoding.UTF8.GetString(bytes, 0, ByteRec);
                    Console.WriteLine(Data);
                    work.FillMatrix(1, Data);
                    handler.Send(Encoding.UTF8.GetBytes("<OK>"));


                    ByteRec = handler.Receive(bytes);
                    Data = Encoding.UTF8.GetString(bytes, 0, ByteRec);
                    Console.WriteLine(Data);
                    work.FillMatrix(2, Data);
                    handler.Send(Encoding.UTF8.GetBytes("<OK>"));

                    while (true)
                    {
                        ByteRec = handler.Receive(bytes);
                        Data = Encoding.UTF8.GetString(bytes, 0, ByteRec);

                        if (Data.IndexOf("<END>") > -1)//конец счета
                        {
                            break;
                        }
                        else if (Data != "")//счет и ответ
                        {
                            handler.Send(Encoding.UTF8.GetBytes(work.CalculationMatrix(Data)));
                        }
                    }

                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                    handler.Dispose();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            MySocket.Dispose();
            working = false;
        }
    }

    public class Matrix
    {
        public static UInt32 dimension = 0;

        public Int32[,] MatrixData_A;
        public Int32[,] MatrixData_B;

        public void InstalSize(int number, String Data)
        {
            bool flag = true;
            String str = null;

            Char[] arra = Data.ToCharArray();

            if (number == 1)
            {
                for (int i = 0; i < arra.Length; i++)
                {
                    if (arra[i] != ';')
                    {
                        str += arra[i];
                    }
                    else
                    {
                        if (flag)
                        {
                            dimension = Convert.ToUInt32(str);
                            str = null;
                        }
                        else
                        {
                            dimension = Convert.ToUInt32(str);
                            str = null;
                        }
                        flag = false;
                    }
                    if (i == arra.Length - 1)
                    {
                        dimension = Convert.ToUInt32(str);
                        str = null;
                    }
                }
            }
            else
            {
                for (int i = 0; i < arra.Length; i++)
                {
                    if (arra[i] != ';')
                    {
                        str += arra[i];
                    }
                    else
                    {
                        if (flag)
                        {
                            dimension = Convert.ToUInt32(str);
                            str = null;
                        }
                        else
                        {
                            dimension = Convert.ToUInt32(str);
                            str = null;
                        }
                        flag = false;
                    }
                    if (i == arra.Length - 1)
                    {
                        dimension = Convert.ToUInt32(str);
                        str = null;
                    }
                }
            }
        }

        public void InstalMatrixMamory()
        {
            MatrixData_A = new Int32[dimension, dimension];
            MatrixData_B = new Int32[dimension, dimension];
        }

        public void FillMatrix(int number, String Data)
        {
            string buf = null;
            int n = 0;

            if (number == 1)
            {//a
                for (int i = 0; i < dimension; i++)
                {
                    for (int j = 0; j < dimension; j++)
                    {
                        while (true)
                        {
                            if (Data[n] != ';')
                            {
                                buf += Data[n];
                                n++;
                            }
                            else
                            {
                                MatrixData_A[i, j] = Convert.ToInt32(buf);
                                buf = null;
                                n++;
                                break;
                            }

                        }
                    }
                }
            }
            else
            {//b
                for (int i = 0; i < dimension; i++)
                {
                    for (int j = 0; j < dimension; j++)
                    {
                        while (true)
                        {
                            if (Data[n] != ';')
                            {
                                buf += Data[n];
                                n++;
                            }
                            else
                            {
                                MatrixData_B[i, j] = Convert.ToInt32(buf);
                                buf = null;
                                n++;
                                break;
                            }
                        }
                    }
                }
            }
        }

        public String CalculationMatrix(String data)
        {
            int x = 0;
            int y = 0;
            int c = 0;
            String bufer = null;
            bool flag = true;

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] != ';')
                {
                    bufer += data[i];
                }
                else
                {
                    if (flag)
                    {
                        x = Convert.ToInt32(bufer);
                        bufer = null;
                    }
                    else
                    {
                        y = Convert.ToInt32(bufer);
                        bufer = null;
                    }
                    flag = false;
                }
            }

            for (int i = 0; i < dimension; i++)
            {
                c += MatrixData_A[x, i] * MatrixData_B[i, y];
            }

            return Convert.ToString(c);
        }

    }
}
