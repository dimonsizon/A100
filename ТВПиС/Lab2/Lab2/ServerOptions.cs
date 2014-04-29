using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

using System.Windows.Forms;

namespace Lab2
{
    public class ClientIpAddress
    {
        public ClientIpAddress(String I, String P)
        {
            Ip = I;
            Port = P;
        }
        public String Ip = "";
        public String Port = "";
    }

    public class WhoThis
    {
        public static IPAddress ip = IPAddress.Broadcast;
        public static int toPort = 65000;//порт  кому
        public static int fromPort = 65001;//порт от кого/клиент
        public static IPEndPoint ep = new IPEndPoint(ip, toPort);
        public static UdpClient udp = new UdpClient(fromPort);
        public static string txtText = "Test connection...";
        public static byte[] data = Encoding.UTF8.GetBytes(txtText);
        public static IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

        public static List<ClientIpAddress> FullIpList = new List<ClientIpAddress>();
        /// <summary>
        /// список ip доступных клиентов
        /// </summary>
        public static List<ClientIpAddress> FindIpList = new List<ClientIpAddress>();

        public static void SendEvent()
        {
            try
            {
                udp.Send(data, data.Length, ep);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public static void AnswerEvent()
        {
            try
            {
                while (true)
                {
                    Byte[] receiveBytes = udp.Receive(ref RemoteIpEndPoint);
                    string returnData = Encoding.UTF8.GetString(receiveBytes);
                    FullIpList.Add(new ClientIpAddress(RemoteIpEndPoint.Address.ToString(), returnData.ToString()));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }

    /// <summary>
    /// класс для методов общения с клиентом
    /// </summary>
    public class MyConnectionToServer
    {

        public static void NewConnection()
        {
            //буфер для входных данных
            byte[] bytes = new byte[1024 * 8 * 8];
            int ByteRec = 0;
            String Data = null;            //начало

            Matrix.startEvent.WaitOne();

            try
            {
                Flags.GetNewDataForServer.WaitOne();

                int id_serv = NextServer();

                Flags.GetNewDataForServer.ReleaseMutex();

                //удаленая конечная точка для сокета
                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(WhoThis.FindIpList[id_serv].Ip), Convert.ToInt32(WhoThis.FindIpList[id_serv].Port));
                Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                //соеденяемся
                sender.Connect(ipEndPoint);

                //послать размер матрицы A и В
                sender.Send(Encoding.UTF8.GetBytes(Matrix.dimension.ToString() + ";" + Matrix.dimension.ToString()));
                ByteRec = sender.Receive(bytes);
                Data = Encoding.UTF8.GetString(bytes, 0, ByteRec);
                sender.Send(Encoding.UTF8.GetBytes(Matrix.dimension.ToString() + ";" + Matrix.dimension.ToString()));
                ByteRec = sender.Receive(bytes);
                Data = Encoding.UTF8.GetString(bytes, 0, ByteRec);
                sender.Send(Encoding.UTF8.GetBytes(MatrixString.MatrixToString(1)));
                ByteRec = sender.Receive(bytes);
                Data = Encoding.UTF8.GetString(bytes, 0, ByteRec);
                sender.Send(Encoding.UTF8.GetBytes(MatrixString.MatrixToString(2)));
                ByteRec = sender.Receive(bytes);
                Data = Encoding.UTF8.GetString(bytes, 0, ByteRec);

                int[] xy = new int[3];

                while (true)
                {
                    Flags.GetNewData.WaitOne(); //блокировка мьютекса

                    xy = Matrix.NextMatrixData();

                    Flags.GetNewData.ReleaseMutex(); //освобождение мьютекса

                    if (xy[2] == 1)
                    {
                        if (Matrix.threadCount == Matrix.stopThread)
                        {
                            Matrix.stopEvent.Set();
                            Matrix.NextMatrixData_null();
                        }
                        else
                        {
                            Matrix.stopThread++;
                        }
                        sender.Send(Encoding.UTF8.GetBytes("<END>"));

                        break;
                    }
                    else
                    {
                        //послать номер ячейки:
                        sender.Send(Encoding.UTF8.GetBytes(Convert.ToString(xy[0]) + ";" + Convert.ToString(xy[1]) + ";"));

                        //ожидать результат:
                        ByteRec = sender.Receive(bytes);
                        Data = Encoding.UTF8.GetString(bytes, 0, ByteRec);
                        Matrix.MatrixData_C[xy[0], xy[1]] = Convert.ToInt32(Data);
                    }
                }

                sender.Shutdown(SocketShutdown.Both);
                sender.Close();
                sender.Dispose();
            }
            catch (Exception e)
            {
                MessageBox.Show("Исключение " + e.ToString());
            }

            Matrix.stopEvent.Set();
        }

        public static int returnServerID = 0;
        public static int MaxNow = 1;
        public static int NextServer()
        {
            returnServerID++;
            return returnServerID - 1;
        }

    }
    public class MatrixString
    {
        public static string MatrixToString(int number)
        {
            String bufer = null;

            if (number == 1)
            {
                for (int i = 0; i < Matrix.dimension; i++)
                {
                    for (int j = 0; j < Matrix.dimension; j++)
                    {
                        bufer += Matrix.MatrixData_A[i, j].ToString() + ";";
                    }
                }

                return bufer;
            }
            else
            {
                for (int i = 0; i < Matrix.dimension; i++)
                {
                    for (int j = 0; j < Matrix.dimension; j++)
                    {
                        bufer += Matrix.MatrixData_B[i, j].ToString() + ";";
                    }
                }
                return bufer;
            }
        }
    }
}
