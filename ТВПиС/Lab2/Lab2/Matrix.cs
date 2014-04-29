using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Lab2
{
    public class Flags
    {
        // статус завершения
        public static Boolean FinalMatrix = false;

        // уже можно считать?
        public static Mutex GetNewData = new Mutex();

        // уже можно брать новую ячейку для отправки?
        public static Mutex GetNewDataForServer = new Mutex();

    }
    public class IdServ
    {
        public IdServ(int _id)
        {
            id = _id;
        }
        public int id = 0;
    }

    public class Matrix
    {
        public static Int32 Random_min = 0;
        public static Int32 Random_max = 10;

        //размерность матриц
        public static UInt32 dimension = 2; 
        public static UInt32 Matrix_MaxXY = 2; // кол-во итераций для получения матрицы

        public static UInt32 userThreadCount = 2;
        public static UInt32 maxThreadCount = 2;

        public static Double[] timeThread;
        //матрицы
        public static Int32[,] MatrixData_A;
        public static Int32[,] MatrixData_B;
        public static Int32[,] MatrixData_C;

        /// <summary>
        /// сгенерить матрицы и т п.
        /// </summary>
        public void Generate()
        {
            //выбрать максимальное кол-во потоков
            maxThreadCount = dimension * dimension;
            Matrix_MaxXY = maxThreadCount;
            if (maxThreadCount > userThreadCount) maxThreadCount = userThreadCount;
            if (maxThreadCount > WhoThis.FindIpList.Count) maxThreadCount = Convert.ToUInt32(WhoThis.FindIpList.Count);
            timeThread = new Double[maxThreadCount];

            MatrixData_A = new Int32[dimension, dimension];
            MatrixData_B = new Int32[dimension, dimension];
            MatrixData_C = new Int32[dimension, dimension];

            Random Rnd = new Random();
            for (int x = 0; x < dimension; x++)
            {
                for (int y = 0; y < dimension; y++)
                {
                    MatrixData_A[x, y] = Rnd.Next(Random_min, Random_max);
                }
            }
            for (int x = 0; x < dimension; x++)
            {
                for (int y = 0; y < dimension; y++)
                {
                    MatrixData_B[x, y] = Rnd.Next(Random_min, Random_max);
                }
            }
        }

        // событие для запуска всех потоков
        public static EventWaitHandle startEvent;
        // событие сработает по оканчанию всех потоков
        public static EventWaitHandle stopEvent;
        public static int threadCount = 0;
        public static int stopThread = 0;

        public void ThreadManager()
        {
            for (int n = 1; n <= maxThreadCount; n++)
            {
                Thread.Sleep(1500); //пауза между подходами
                startEvent = new ManualResetEvent(false);
                stopEvent = new ManualResetEvent(false);
                System.Diagnostics.Stopwatch tick = new System.Diagnostics.Stopwatch();

                stopThread = 1;
                threadCount = n;

                Thread[] threads = new Thread[n];

                MyConnectionToServer.MaxNow = n;
                MyConnectionToServer.returnServerID = 0;
                for (int i = 0; i < n; i++)
                {
                    threads[i] = new Thread(MyConnectionToServer.NewConnection);
                    threads[i].Start();
                }

                Thread.Sleep(2500);
                tick.Start();
                startEvent.Set();

                stopEvent.WaitOne();

                tick.Stop();
                timeThread[n - 1] = tick.Elapsed.TotalMilliseconds;
            }
            Flags.FinalMatrix = true;
        }

        static int NextXY = 1;
        static int NextX = 0;
        static int NextY = 0;
        public static int[] NextMatrixData()
        {

            int[] t = new int[3];

            //возвратить координату, для умножения, если нет то послать 0.
            if (NextXY > Matrix_MaxXY)
            {
                t[2] = 1; //x
            }
            else
            {
                t[0] = NextX;
                t[1] = NextY;
                if (NextX == dimension - 1)
                {
                    NextX = 0;
                    NextY++;
                }
                else
                {
                    NextX++;
                }
            }
            NextXY++;

            return t;
        }

        public static void NextMatrixData_null()
        {
            NextXY = 1;
            NextX = 0;
            NextY = 0;
        }
    }
}
