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
        /// <summary>
        /// порт на котором будет прослушивать сервер на предмет входяшего броткаста от клиента.
        /// </summary>
        public static int ServerLocalPort_Slushaem = 65000;
        /// <summary>
        /// порт на котором программа будет взаимодействовать с клиентом.
        /// </summary>
        public static int ServerLocalPort_Work = 7100;
        public static Boolean working = false;
     

        static void Main(string[] args)
        {
            Thread workThread = new Thread(work);
            workThread.Start();

            Console.WriteLine("Поиск сервера....");
            //Creates a UdpClient for reading incoming data.
            UdpClient receivingUdpClient = new UdpClient(ServerLocalPort_Slushaem);
            //Creates an IPEndPoint to record the IP Address and port number of the sender. 
            // The IPEndPoint will allow you to read datagrams sent from any source.
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            Console.WriteLine("Готово!");
            while (true)
            {
                if (working) { break; }
                try
                {
                    // Blocks until a message returns on this socket from a remote host.
                    Byte[] receiveBytes = receivingUdpClient.Receive(ref RemoteIpEndPoint);

                    string returnData = Encoding.UTF8.GetString(receiveBytes);

                    byte[] data = Encoding.UTF8.GetBytes(ServerLocalPort_Work.ToString());
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
                //IPHostEntry ipHost = Dns.Resolve("localhost");
                //IPAddress ipAdrr = ipHost.AddressList[0];
                //IPEndPoint ipEndPoint = new IPEndPoint(ipAdrr, ServerLocalPort_Work);
                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, ServerLocalPort_Work);
            
                //создаем сокет tcp/ip 
                Socket MySocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //назначаем сокет локальной конечной точке и
                // слушаем входящии сокеты
            
                try
                {
                    MySocket.Bind(ipEndPoint);
                    MySocket.Listen(1);
                    
                    //начинаем слушать соеденения
                    while (true)
                    {
                      //  Console.WriteLine("Срервер ожидает поключения на " + ipEndPoint);
                        //Console.WriteLine("Сервер ожидает соеденения. Готов работать");
                        
                        Socket handler = MySocket.Accept();
                        byte[] bytes = new byte[1024*8*8];
                        int ByteRec = 0;
                        String Data = null;
      
                        ByteRec = handler.Receive(bytes);
                        Data = Encoding.UTF8.GetString(bytes, 0, ByteRec);
                        Matrix work = new Matrix();
                        work.InstalRazmer(1, Data);
                        handler.Send(Encoding.UTF8.GetBytes("<OK>"));

                        ByteRec = handler.Receive(bytes);
                        Data = Encoding.UTF8.GetString(bytes, 0, ByteRec);
                        work.InstalRazmer(2, Data);
                        handler.Send(Encoding.UTF8.GetBytes("<OK>"));
              
                        work.InstalMatrixMamory();
        
                        ByteRec = handler.Receive(bytes);
                        Data = Encoding.UTF8.GetString(bytes, 0, ByteRec);
                        Console.WriteLine(Data);
                        work.ZapolnitMatrix(1,Data);
                        handler.Send(Encoding.UTF8.GetBytes("<OK>"));

             
                        ByteRec = handler.Receive(bytes);
                        Data = Encoding.UTF8.GetString(bytes, 0, ByteRec);
                        Console.WriteLine(Data);
                        work.ZapolnitMatrix(2, Data);
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
                                handler.Send(Encoding.UTF8.GetBytes(work.Schet(Data)));
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

                //отработали....в топку
                MySocket.Dispose();
                working = false;
        }
    }

    public class Matrix
    {
        //размерность матриц
        public static UInt32 Matrix_A_x = 0; //строка
        public static UInt32 Matrix_A_y = 0; //столбец
        public static UInt32 Matrix_B_x = 0;
        public static UInt32 Matrix_B_y = 0;

        public Int32[,] MatrixData_A;
        public Int32[,] MatrixData_B;



        public void InstalRazmer(int nomer, String Data)
        {
            bool flag = true;
            String str = null;
            
            Char[] arra = Data.ToCharArray();

                if (nomer == 1)
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
                                Matrix_A_x = Convert.ToUInt32(str);
                                str = null;
                            }
                            else
                            {
                                Matrix_A_y = Convert.ToUInt32(str);
                                str = null;
                            }
                            flag = false;
                        }
                        if (i == arra.Length - 1)
                        {
                            Matrix_A_y = Convert.ToUInt32(str);
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
                                Matrix_B_x = Convert.ToUInt32(str);
                                str = null;
                            }
                            else
                            {
                                Matrix_B_y = Convert.ToUInt32(str);
                                str = null;
                            }
                            flag = false;
                        }
                        if (i == arra.Length - 1)
                        {
                            Matrix_B_y = Convert.ToUInt32(str);
                            str = null;
                        }
                    }

                }

            }

        public void InstalMatrixMamory()
        {
            MatrixData_A = new Int32[Matrix_A_x, Matrix_A_y];
            MatrixData_B = new Int32[Matrix_B_x, Matrix_B_y];
        }

        public void ZapolnitMatrix(int nomer, String Data)
        {
            string buf = null;
            int n = 0;

            if (nomer == 1)
            {//a
                for (int i = 0; i < Matrix_A_x; i++)//строка
                {
                    for (int j = 0; j < Matrix_A_y; j++)//столбцы
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
                //тут написать функцию, преобразование строки в матрицу АААА
            }
            else
            {//b
                for (int i = 0; i < Matrix_B_x; i++)//строка
                {
                    for (int j = 0; j < Matrix_B_y; j++)//столбцы
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
                //тут написать функцию, преобразование строки в матрицу ББББ
            }

        }

        public String Schet(String data)
        {
            int x = 0;
            int y = 0;
            int c = 0;
            String bufer = null;
            bool flag = true;

            for (int i = 0; i < data.Length; i++ )
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

            for (int i = 0; i < Matrix_A_y; i++)
            {
                c += MatrixData_A[x, i] * MatrixData_B[i, y];
            }

            return Convert.ToString(c);
        }
    
    }
}
