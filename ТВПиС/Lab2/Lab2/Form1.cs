using System;
using System.Windows.Forms;
using System.Threading;

namespace Lab2
{
    using System.Windows.Forms.DataVisualization.Charting;

    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
            Thread GetResponse = new Thread(WhoThis.AnswerEvent);
            GetResponse.IsBackground = true;
            GetResponse.Start();
        }

        public static object locker = new object();

        private void runButton_Click(object sender, EventArgs e)
        {
            if (WhoThis.FindIpList.Count == 0)
            {
                MessageBox.Show("Нет клиентов для вычислений!");
            }
            else
            {
                try
                {
                    Matrix.dimension = Convert.ToUInt32(dimensionTextBox.Text);
                }
                catch (Exception)
                {
                    MessageBox.Show("Некорректная размерность матриц!");
                    return;
                }
                runButton.Enabled = false;

                Matrix workMatrix = new Matrix();
                workMatrix.Generate();

                Thread GlobalThread = new Thread(workMatrix.ThreadManager);
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

                for (int i = 0; i < Matrix.maxThreadCount; i++)
                {
                    plot.Points.AddXY(i + 1, Matrix.timeThread[i]);
                }

                runButton.Enabled = true;
            }
        }

        private void refreshButton_Click(object sender, EventArgs e)
        {
            //найти список доступных клиентов
            WhoThis.SendEvent();

            Thread.Sleep(2500);

            lock (locker)
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
                    Matrix.userThreadCount = CountClient;
                }
            }
        }
    }
}
