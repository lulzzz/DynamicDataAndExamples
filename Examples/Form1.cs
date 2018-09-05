using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DynamicData;

namespace Examples
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        DynamicNetwork dn = null;

        private void button1_Click(object sender, EventArgs e)
        {
            DynamicBuild build = new DynamicBuild();
            build.MinNodeConnections = 1;
            build.MaxNodeConnections = 8;
            build.MinTotalNodes = 50;
            build.MaxTotalNodes = 100;
            build.TakeRandom = true;
            build.ShuffleData = true;

            List<PairBit> traindata = PairBit.LoadFromFile("EqualColorsShuffled.TrainData"); //EqualColorsShuffled Blue  _FULL

           traindata.ShuffleList();

            for (int x = 0; x < traindata.Count; x++)
            {
                traindata[x].CreateDoubleInputs(false);
            }

         /*   List<PairBit> traindataTwo = PairBit.LoadFromFile("DAY_2_AVG_EQ_NML.TrainData"); //DAY_2_AVG_EQ_NML

            for (int x = 0; x < traindataTwo.Count; x++)
            {
                traindataTwo[x].CreateDoubleInputs(true);
            }*/

            dn = new DynamicNetwork(traindata, build);

            Random r = new Random(DateTime.Now.Millisecond);

            double Top = 0.1;

            NetworkResponse stp = new NetworkResponse();

            SaveState state = new SaveState();

            for (int v = 0; v < 25; v++)
            {
                dn = new DynamicNetwork(traindata, build);

                for (int i = 0; i < 500; i++)
                {
                    stp = dn.Train(Top);

                    double per = stp.Percent;

                    if (per > Top)
                    {
                        Top = per;

                        state = dn.GetSaveState();
                        pictureBox1.Image = Draw(state.Nodes);
                        pictureBox1.Refresh();
                    }
                    else
                    {
                        dn.ReRollNetwork();
                    }
                }

                Console.WriteLine(Top + " :TOP: " + v);
            }

            dn.Nodes = state.Nodes;
            dn.Outputs = state.Outputs;
            stp = dn.Train(0);

            Console.WriteLine("final:" + (stp.Correct / (stp.Wrong + stp.Correct)));

            /*dn.SaveNetwork("test.DDNet", state);

            DynamicNetwork loadtest = new DynamicNetwork("test.DDNet");
            loadtest.SetTrain(traindataTwo);
            stp = dn.Train(0);
            
            Console.WriteLine("Load:" + (stp.Correct / (stp.Wrong + stp.Correct)));*/

            pictureBox1.Image = Draw(state.Nodes);
        }

        public Bitmap Draw(List<Node> nodes)
        {
            int ext = 50;
            int ex = 12;
            int w = (int)(Math.Sqrt(nodes.Count));

            int plus = 30;

            ext = 900 / w;
            ex = ext / 2;
            plus = ex / 2;
            Pen line = new Pen(Color.FromArgb(255, Color.Teal));
            Pen input = new Pen(Color.FromArgb(255, Color.Gold), 2f);
            Pen line2 = new Pen(Color.FromArgb(255, Color.Wheat), 2f);
            Bitmap btm = new Bitmap(1200, 1200);
            Rectangle r = new Rectangle(0, 0, ex, ex);

            using (Graphics g = Graphics.FromImage(btm))
            {
                g.Clear(Color.Black);

                for (int i = 0; i < nodes.Count; i++)
                {
                    int x = i % w;
                    int y = i / w;

                    if (i == 0)
                    {
                        x = 0;
                        y = 0;
                    }

                    r.X = (x * ext) - (ex / 2) + plus;
                    r.Y = (y * ext) - (ex / 2) + plus;

                    // line = new Pen(Color.FromArgb(-(i + 100000) *(x * x) * (y*y) ), 1f);

                    if (nodes[i].isInput)
                    {
                        g.DrawRectangle(input, r);
                    }
                    else
                    {
                        g.DrawRectangle(line2, r);

                        if (nodes[i].Value > nodes[i].Threshold)
                        {
                            line = new Pen(Color.FromArgb(255, (byte)((0.5 + (nodes[i].Value / 2)) * 255), 100, 255));

                            int[] dx = nodes[i].Connections;

                            if (dx != null)
                            {
                                for (int v = 0; v < dx.Length; v++)
                                {
                                    int x2 = (dx[v] % w);
                                    int y2 = (dx[v] / w);

                                    if (dx[v] == 0)
                                    {
                                        x2 = 0;
                                        y2 = 0;
                                    }

                                    g.DrawLine(line, (x * ext) + plus, (y * ext) + plus, (x2 * ext) + (plus), (y2 * ext) + plus);
                                }
                            }
                        }
                    }
                }
            }

            return btm;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();

            cd.ShowDialog();

            PairBit bt = new PairBit();
            bt.Inputs = new byte[] { cd.Color.R, cd.Color.G, cd.Color.B };
            bt.CreateDoubleInputs(false);



            string text = dn.RunNetwork(bt).Answer;

            MessageBox.Show(text);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
