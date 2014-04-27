using System;
using System.Windows.Forms;
using System.Threading;

namespace laba1
{
    using System.Windows.Forms.DataVisualization.Charting;

    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
            Thread GetOtvet = new Thread(WhoThis.OtvetEvent);
            GetOtvet.IsBackground = true;
            GetOtvet.Start();

        }
        //public static Button p;
        public string s = "";

        private void buttonRun_Click(object sender, EventArgs e)
        {
            if (WhoThis.FindIpList.Count == 0)//есть ли доступные сервера?
            {
                MessageBox.Show("Нет клиентов для вычислений!");

            }
            else
            {
                buttonRun.Enabled = false;

                matrix.Matrix_A_x = Convert.ToUInt32(textBoxAx.Text);
                matrix.Matrix_A_y = Convert.ToUInt32(textBoxAy.Text);
                matrix.Matrix_B_x = Convert.ToUInt32(textBoxBx.Text);
                matrix.Matrix_B_y = Convert.ToUInt32(textBoxBy.Text);

                matrix.Random_min = 0;
                matrix.Random_max = 10;

                //matrix.Potok_Count_Screen = Convert.ToUInt32(textBoxMaxPotok.Text);

                matrix Work = new matrix();
                Work.instal();

                Thread GlobalThread = new Thread(Work.MenegerPotokov);
                GlobalThread.Start();
                timer1.Start();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (Flags.FinalMatrix)
            {
                timer1.Stop();
                Flags.FinalMatrix = false;

                this.Width = 712;
                MyChart.Visible = true;


                Series plot = MyChart.Series[0];
                plot.Points.Clear();

                for (int i = 0; i < matrix.Potok_Count_Real; i++)
                {
                    plot.Points.AddXY(i + 1, matrix.TimePotok[i]);
                }

                buttonRun.Enabled = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //найти список доступных серверов
            WhoThis.SendEvent();

            Thread.Sleep(2500);
            WhoThis.Close();


            lock (s)
            {
                WhoThis.FindIpList.Clear();
                richTextBoxServerList.Text = "";
                if (WhoThis.FullIpList.Count > 0)
                {
                    for (int i = 0; i < WhoThis.FullIpList.Count; i++)
                    {
                        bool flag = false;
                        for (int j = 0; j < WhoThis.FindIpList.Count; j++)
                        {
                            if (WhoThis.FindIpList[j].Ip == WhoThis.FullIpList[i].Ip) { flag = true; }
                        }

                        if (!flag) { WhoThis.FindIpList.Add(WhoThis.FullIpList[i]); }
                    }
                    //WhoThis.FindIpList.AddRange(WhoThis.FullIpList.GroupBy);
                    WhoThis.FullIpList.Clear();
                }


                if (WhoThis.FindIpList.Count > 0)
                {
                    uint CountClient = 0;
                    for (int i = 0; i < WhoThis.FindIpList.Count; i++)
                    {
                        richTextBoxServerList.Text += WhoThis.FindIpList[i].Ip + "\n";
                        CountClient++;
                    }
                    matrix.Potok_Count_Screen = CountClient;
                }
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }


    }
}
