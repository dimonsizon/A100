using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

using System.Windows.Forms;


namespace laba1
{
    public class ServerIpAdress
    {
        public ServerIpAdress(String I, String P)
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
        public static int remport = 65000;//порт кому
        public static int locport = 65001;//порт от кого/клиент
        public static IPEndPoint ep = new IPEndPoint(ip, remport);
        public static UdpClient udp = new UdpClient(locport);
        public static string txtText = "Есть кто?";
        public static byte[] data = Encoding.UTF8.GetBytes(txtText);
        public static IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
        
        public static List<ServerIpAdress> FullIpList = new List<ServerIpAdress>();
        /// <summary>
        /// список ip доступных серверов
        /// </summary>
        public static List<ServerIpAdress> FindIpList = new List<ServerIpAdress>();

        ~WhoThis()
        {
            udp.Close();
        }
        public static void Close()
        {
           // udp.Close();
        }
        /// <summary>
        /// послать сообщение в эфир
        /// </summary>
        public static void SendEvent()
        {
            try
            {
                //пошлем 3 раза ;)
                udp.Send(data, data.Length, ep);
                udp.Send(data, data.Length, ep);
                udp.Send(data, data.Length, ep);
                //richTextBoxLog.Text += "найден клиент:" + RemoteIpEndPoint.Address.ToString() + ":" + returnData.ToString() + "\n";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// ожидать ответ
        /// </summary>
        public static void OtvetEvent()
        {
            try
            {
                for (; ; )
                {
                   
                    Byte[] receiveBytes = udp.Receive(ref RemoteIpEndPoint);

                    string returnData = Encoding.UTF8.GetString(receiveBytes);
                    FullIpList.Add(new ServerIpAdress(RemoteIpEndPoint.Address.ToString(), returnData.ToString() ) );
                    //MessageBox.Show("найден клиент:" + RemoteIpEndPoint.Address.ToString() + ":" + returnData.ToString() + "\n");
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
            byte[] bytes = new byte[1024*8*8];
            int ByteRec = 0;
            String Data = null;            //начало

            matrix.wait.WaitOne();

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
                //MessageBox.Show("клиент соединился " + sender.RemoteEndPoint.ToString());

               // string theMessage = "0";


                //послать размер матрицы A и В
                sender.Send(Encoding.UTF8.GetBytes(matrix.Matrix_A_x.ToString()+";"+matrix.Matrix_A_y.ToString()));
                ByteRec = sender.Receive(bytes);
                Data = Encoding.UTF8.GetString(bytes, 0, ByteRec);
                //Thread.Sleep(100);
                sender.Send(Encoding.UTF8.GetBytes(matrix.Matrix_B_x.ToString() + ";" + matrix.Matrix_B_y.ToString()));
                ByteRec = sender.Receive(bytes);
                Data = Encoding.UTF8.GetString(bytes, 0, ByteRec);
                sender.Send(Encoding.UTF8.GetBytes( matrixString.MatrixToString(1) ) );
                ByteRec = sender.Receive(bytes);
                Data = Encoding.UTF8.GetString(bytes, 0, ByteRec);
                //Thread.Sleep(100);
                sender.Send(Encoding.UTF8.GetBytes(matrixString.MatrixToString(2)));
                ByteRec = sender.Receive(bytes);
                Data = Encoding.UTF8.GetString(bytes, 0, ByteRec);
                
                
                
                //Ура, вродь работает, теперь передаем x,y ячейки - и ждем результат. пока не закончатся ячейки
                int[] xy = new int[3];


                for (; ; )
                {
                    Flags.GetNewData.WaitOne(); // ставим блок мьютиксом

                    xy = matrix.NextMatrixData();

                    Flags.GetNewData.ReleaseMutex(); //освобождаем мьютикс

                    if (xy[2] == 1)
                    {
                        if (matrix.VsegoPotok == matrix.StopPotok)
                        {
                            matrix.waitAllPotok.Set();
                            matrix.NextMatrixData_null();
                           
                        }
                        else
                        {
                            matrix.StopPotok++;
                            
                        }
                        //послать <END>
                        sender.Send(Encoding.UTF8.GetBytes( "<END>" ));

                        break;
                    }
                    else
                    {
                        //послать номер ячейки:
                        sender.Send(Encoding.UTF8.GetBytes(Convert.ToString(xy[0]) + ";" + Convert.ToString(xy[1]) + ";"));

                        //ожидать результат:
                        ByteRec = sender.Receive(bytes);
                        Data = Encoding.UTF8.GetString(bytes, 0, ByteRec);
                        matrix.MatrixData_C[xy[0], xy[1]] = Convert.ToInt32(Data);
                    }

                }


                    //byte[] msgByte = Encoding.UTF8.GetBytes(theMessage);

                    //sender.Send(msgByte);

                    //ответ
                    //int byteOtvet = sender.Receive(bytes);

                    //Encoding.UTF8.GetString(bytes, 0, byteOtvet);

                    //закрытие
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();
                sender.Dispose();
            }
            catch(Exception e)
            {
                //Console.WriteLine("Исключение " + e.ToString());
                MessageBox.Show("Исключение " + e.ToString());
            }

            matrix.waitAllPotok.Set();
        }

        public static int returnServerID = 0;
        public static int MaxNow = 1;
        public static int NextServer()
        {
            returnServerID++;
            return returnServerID-1;
        }

    }
    public class matrixString
    {

        public static string MatrixToString(int nomer)
        {
            String bufer = null;
            
            if (nomer == 1)
            {
                for (int i = 0; i < matrix.Matrix_A_x; i++)
                {
                    for (int j = 0; j < matrix.Matrix_A_y; j++)
                    {
                        bufer += matrix.MatrixData_A[i,j].ToString() + ";";
                    }
                }

                return bufer;
            }
            else
            {
                for (int i = 0; i < matrix.Matrix_B_x; i++)
                {
                    for (int j = 0; j < matrix.Matrix_B_y; j++)
                    {
                        bufer += matrix.MatrixData_B[i, j].ToString() + ";";
                    }
                }
                return bufer;
            }

        }

    }

    
}
