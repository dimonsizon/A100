using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace laba1
{
    public class Flags
    {
        /// <summary>
        /// уже умножили?
        /// </summary>
        public static Boolean FinalMatrix = false;

        /// <summary>
        /// уже можно считать?
        /// </summary>
        public static Mutex GetNewData = new Mutex();

        /// <summary>
        /// уже можно брать новую ячейку для отправки?
        /// </summary>
        public static Mutex GetNewDataForServer = new Mutex();
        
    }
    public class IdServ
    {
        public IdServ(int aaa)
        {
            id = aaa;
        }
        public int id = 0;
    }

    public class matrix
    {
        //Великий рандом / min max числа для заполнения
        public static Int32 Random_min = 0;
        public static Int32 Random_max = 3;

        //размерность матриц
        public static UInt32 Matrix_A_x = 2; //строка
        public static UInt32 Matrix_A_y = 2; //столбец
        public static UInt32 Matrix_B_x = 2;
        public static UInt32 Matrix_B_y = 2;
        public static UInt32 Matrix_MaxXY = 2; // по сути сколько нужно ильтерауций что бы получить матрицу =)

        //число потоков заданое с экрана
        public static UInt32 Potok_Count_Screen = 2;
        //число потоков максимум необходимое
        public static UInt32 Potok_Count_Real = 2;
        //число потоков текущее (режим работы)
        public UInt32 Potok_Count_Now = 1;
        public static Double[] TimePotok;
        //числа матрицы
        public static Int32[,] MatrixData_A;
        public static Int32[,] MatrixData_B;
        public static Int32[,] MatrixData_C;

        /// <summary>
        /// сгенерить матрицы и т п.
        /// </summary>
        public void instal()
        {
            //выбрать максимальное кол-во потоков
            //if (Matrix_A_x > Matrix_B_y) { Potok_Count_Real = Matrix_A_x; } else { Potok_Count_Real = Matrix_B_y; }
            Potok_Count_Real = Matrix_A_x * Matrix_B_y;
            Matrix_MaxXY = Potok_Count_Real;
            if (Potok_Count_Real > Potok_Count_Screen) Potok_Count_Real = Potok_Count_Screen;
            if (Potok_Count_Real > WhoThis.FindIpList.Count) Potok_Count_Real = Convert.ToUInt32( WhoThis.FindIpList.Count );
            TimePotok = new Double[Potok_Count_Real];
            //выделим память матрицам 
            MatrixData_A = new Int32[Matrix_A_x, Matrix_A_y];
            MatrixData_B = new Int32[Matrix_B_x,Matrix_B_y];
            MatrixData_C = new Int32[Matrix_A_x, Matrix_B_y];
            
            //заполнить
            Random Rnd = new Random();
            for (int x = 0; x < Matrix_A_x; x++ )
            {
                for (int y = 0; y < Matrix_A_y; y++)
                {
                    MatrixData_A[x, y] = Rnd.Next(Random_min, Random_max);
                   
                }
            }
            for (int x = 0; x < Matrix_B_x; x++)
            {
                for (int y = 0; y < Matrix_B_y; y++)
                {
                   
                    MatrixData_B[x, y] = Rnd.Next(Random_min, Random_max);
                }
            }
        }

        /// <summary>
        /// вывод матрицы
        /// </summary>
        /// <returns></returns>
        public String PrintMatrix(int nomer)
        {
            String TempStr = "";
            if (nomer == 1)
            {// A
                for (int x = 0; x < Matrix_A_x; x++)
                {
                    for (int y = 0; y < Matrix_A_y; y++)
                    {
                        TempStr += MatrixData_A[x, y] + " ";

                    }
                    TempStr += "\n";
                }
            }
            if (nomer == 2)
            {// B
                for (int x = 0; x < Matrix_B_x; x++)
                {
                    for (int y = 0; y < Matrix_B_y; y++)
                    {
                        TempStr += MatrixData_B[x, y] + " ";

                    }
                    TempStr += "\n";
                }
            }
            if (nomer == 3)
            {// C
                for (int x = 0; x < Matrix_A_x; x++)
                {
                    for (int y = 0; y < Matrix_B_y; y++)
                    {
                        TempStr += MatrixData_C[x, y] + " ";

                    }
                    TempStr += "\n";
                }
            }

            return TempStr;
        }

        public String UmnozhenieOnePotok()
        {
            //добавить подчет занятого времени на подсчет
            System.Diagnostics.Stopwatch WTF = new System.Diagnostics.Stopwatch();
            //строка на столбец. x * y
            WTF.Start();
            for (int x = 0; x < Matrix_A_x; x++)
            {
                for (int y = 0; y < Matrix_B_y; y++)
                {
                    for (int i = 0; i < Matrix_A_y; i++ )
                    {
                        MatrixData_C[x, y] += MatrixData_A[x, i] * MatrixData_B[i, y];
                    }
                }
            }
            WTF.Stop();
            return WTF.Elapsed.TotalMilliseconds.ToString();
        }




        /// <summary>
        /// события для запуска всех потоков
        /// </summary>
        public static EventWaitHandle wait;
        /// <summary>
        /// события сработает по оканчанию всех потоков
        /// </summary>
        public static EventWaitHandle waitAllPotok;
        public static int VsegoPotok = 0;
        public static int StopPotok = 0;
        /// <summary>
        /// уравление рабочими потоками
        /// </summary>
        public void MenegerPotokov()
        {
           
            for (int n = 1; n <= Potok_Count_Real; n++)
            {
                Thread.Sleep(1500); //пауза между подходами

                //начало великого кода для 2 ЛАБЫ
                
                wait = new ManualResetEvent(false);
                waitAllPotok = new ManualResetEvent(false);
                System.Diagnostics.Stopwatch WTF = new System.Diagnostics.Stopwatch();
                
                StopPotok = 1;
                VsegoPotok = n;
                
                //создать неообходимое кол-во потоков под клиента
                Thread[] Potoki = new Thread[n];

                MyConnectionToServer.MaxNow = n;
                MyConnectionToServer.returnServerID = 0;
                for (int i = 0; i < n; i++)
                {
                    Potoki[i] = new Thread(MyConnectionToServer.NewConnection);
                    Potoki[i].Start();
                    
                }
                
                Thread.Sleep(2500);
                WTF.Start();
                wait.Set();
                
                waitAllPotok.WaitOne();
                
                WTF.Stop();
                TimePotok[n - 1] = WTF.Elapsed.TotalMilliseconds;

                
                //конец великого кода для 2 ЛАБЫ


              /*  wait = new ManualResetEvent(false);
                waitAllPotok = new ManualResetEvent(false);
                StopPotok = 1;
                VsegoPotok = n;
           
            
                System.Diagnostics.Stopwatch WTF = new System.Diagnostics.Stopwatch();

               //создать неообходимое кол-во потоков
                Thread[] Potoki = new Thread[n];
                
                for (int i = 0; i < n; i++)
                {
                    Potoki[i] = new Thread(ParalelPotok);
                    Potoki[i].Start();
                }
                      WTF.Start();
                      wait.Set();
                      waitAllPotok.WaitOne();
               // for (int i = 0; i < n; i++)
               // {
               //     Potoki[i].Join();
               // }
                     
                      WTF.Stop();
                      TimePotok[n-1] = WTF.Elapsed.TotalMilliseconds;
                   // ParalelPotok();
              */ 
             }
            Flags.FinalMatrix = true;
        }


        /// <summary>
        /// функция для паралельного подсчета
        /// </summary>
        /// <param name="x">номер строки первой матрици</param>
        /// <param name="y">номер столбца второй матрици</param>
        /// <param name="Dlina">размерность матрици(сколько в строке столбцов и наоборот)</param>
        /// <returns>ответ</returns>
        public void ParalelPotok()
        {
            int[] xy = new int[3];
            //сохранить результа
            wait.WaitOne();
            //попросить новые данные, если 0 выйти.
            while (true)
            {
                Flags.GetNewData.WaitOne(); // ставим блок мьютиксом
                
                xy = NextMatrixData();

                Flags.GetNewData.ReleaseMutex(); //освобождаем мьютикс

                if (xy[2] == 1) 
                {
                    if (VsegoPotok == StopPotok)
                    {
                        waitAllPotok.Set();
                    }
                    else
                    {
                        StopPotok++;
                    }
                    
                    break; 
                }
                else
                {
                    //считаем ячейку
                    MatrixData_C[xy[0],xy[1]] = 0;
                    for (int i = 0; i < Matrix_A_y; i++)
                    {
                        MatrixData_C[xy[0], xy[1]] += MatrixData_A[xy[0], i] * MatrixData_B[i, xy[1]];
                    }
                }


            }
        }

        static int NextXY = 1;
        static int NextX = 0;
        static int NextY = 0;
        public static int[] NextMatrixData()
        {
            
            int[] t = new int[3];

            //возвратить координату, для умножения. если нет то послать 0.
            if (NextXY > Matrix_MaxXY)
            {
                t[2] = 1; //x
                 //y  //t[2] = 0; //
            }
            else
            {
                t[0] = NextX;
                t[1] = NextY;
                if (NextX == Matrix_A_x-1)
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
