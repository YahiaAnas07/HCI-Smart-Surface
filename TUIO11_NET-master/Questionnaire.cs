using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Questionnaire
{
    public partial class Questionnaire : Form
    {
        private Client c;
        Bitmap off;
        PointF finger = new PointF();
        System.Windows.Forms.Timer tt = new System.Windows.Forms.Timer();
        int width, height;
        int preR, preC = 0;
        int total_score = 0;
        Brush[,] brs = new Brush[3, 3];
        bool[,] stat = new bool[3, 3];
        int answer_number = 0;
        int row_number = 0;
        int sorter = -1;
        int true_ct = 0;


        int Hand_Gesture, Arm_Gesture, Number_of_Fingers;
        Rectangle[,] boxes;
        Dictionary<(int, int), float> hoverDurations = new Dictionary<(int, int), float>();
        const float requiredHoverTime = 1000;

        public Questionnaire()
        {
            InitializeComponent();
            this.Paint += Questionnaire_Paint;
            this.WindowState = FormWindowState.Maximized;
            this.Load += Questionnaire_Load;
            tt.Tick += Tt_Tick;
            tt.Interval = 100;
            tt.Start();
        }

        private void Questionnaire_Paint(object sender, PaintEventArgs e)
        {
            DrawDubb(e.Graphics);
        }

        private void Questionnaire_Load(object sender, EventArgs e)
        {
            if (off == null)
            {
                off = new Bitmap(this.ClientSize.Width, this.ClientSize.Height);
            }
            width = this.ClientSize.Width;
            height = this.ClientSize.Height;
            Thread clientThread = new Thread(StartClient);
            clientThread.IsBackground = true;
            clientThread.Start();
            create_boxes();
        }

        void DrawDubb(Graphics g)
        {
            Graphics g2 = Graphics.FromImage(off);
            DrawScene(g2);
            g.DrawImage(off, 0, 0);
        }
        private void DrawScene(Graphics g)
        {
            g.Clear(Color.White);


            string[] questions = {
             "On a range from 1 to 3, how oily does your skin feel?",
             "On a range from 1 to 3, how dry does your skin feel?",
             "On a range from 1 to 3, how normal does your skin feel?"
         };

            Font font = new Font("Arial", 12);
            int textMarginY = 30;
            int labelMarginY = 50;


            for (int col = 0; col < 3; col++)
            {
                g.DrawString((col + 1).ToString(), font, Brushes.Black, boxes[0, col].X + boxes[0, col].Width / 2 - 10, boxes[0, col].Y - labelMarginY);
            }


            for (int row = 0; row < 3; row++)
            {
                g.DrawString(questions[row], font, Brushes.Black, boxes[row, 0].X, boxes[row, 0].Y - textMarginY);
            }


            g.FillEllipse(Brushes.Black, (finger.X * width), (finger.Y * height), 15, 15);


            if (boxes != null)
            {
                for (int row = 0; row < 3; row++)
                {
                    for (int col = 0; col < 3; col++)
                    {
                        g.FillRectangle(brs[row, col], boxes[row, col]);
                    }
                }
            }
        }


        //public void pointer()
        //{
        //    if (boxes != null)
        //    {
        //        for (int row = 0; row < 3; row++)
        //        {
        //            for (int col = 0; col < 3; col++)
        //            {
        //                if (finger.X * width >= boxes[row, col].X && finger.X * width <= boxes[row, col].X + boxes[row, col].Width &&
        //                    finger.Y * height >= boxes[row, col].Y && finger.Y * height <= boxes[row, col].Y + boxes[row, col].Height)
        //                {
        //                    if (hoverDurations.ContainsKey((row, col)))
        //                    {
        //                        hoverDurations[(row, col)] += tt.Interval;
        //                    }
        //                    else
        //                    {
        //                        hoverDurations[(row, col)] = tt.Interval;
        //                    }

        //                    if (hoverDurations[(row, col)] >= requiredHoverTime)
        //                    {

        //                        for (int j = 0; j < 3; j++)
        //                        {
        //                            if (j != col)
        //                            {
        //                                if (stat[row, j])
        //                                {
        //                                    brs[row, j] = Brushes.Red;
        //                                    stat[row, j] = false;
        //                                    hoverDurations[(row, j)] = 0;
        //                                }
        //                            }
        //                        }

        //                        if (!stat[row, col])
        //                        {
        //                            brs[row, col] = Brushes.Black;
        //                            stat[row, col] = true;
        //                        }
        //                        else if (stat[row, col] == true && row == preR && col == preC)
        //                        {
        //                            brs[row, col] = Brushes.Red;
        //                            stat[row, col] = false;
        //                        }

        //                        hoverDurations[(row, col)] = 0;


        //                        total_score = CalculateTotalScore();
        //                        this.Text = $"Total Score: {total_score}";
        //                    }
        //                }
        //                else
        //                {
        //                    hoverDurations[(row, col)] = 0;
        //                }
        //            }
        //        }
        //    }
        //}


        private int CalculateTotalScore()
        {
            int score = 0;

            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    if (stat[row, col])
                    {
                        score += (col + 1);
                    }
                }
            }
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    if (stat[row, col])
                    {
                        true_ct++;
                        continue;
                    }
                }
            }

            return score;
        }
        void transformBox()
        {

            for (int j = 0; j < brs.GetLength(1); j++)
            {
                brs[row_number, j] = Brushes.Red;
            }


            brs[row_number, Number_of_Fingers - 1] = Brushes.Black;
            stat[row_number, Number_of_Fingers - 1] = true;
        }



        private void Tt_Tick(object sender, EventArgs e)
        {
            //pointer();
            if (Number_of_Fingers >= 1)
            {
                CalculateTotalScore();
                transformBox();
            }
            DrawDubb(this.CreateGraphics());
        }
        void create_boxes()
        {
            int boxSize = 80;
            int margin = 50;
            int startX = 100;
            int startY = 150;

            boxes = new Rectangle[3, 3];

            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    int x = startX + (col * (boxSize + margin));
                    int y = startY + (row * (boxSize + margin));
                    boxes[row, col] = new Rectangle(x, y, boxSize, boxSize);
                    hoverDurations[(row, col)] = 0;
                    brs[row, col] = Brushes.Red;
                    stat[row, col] = false;
                }
            }
        }


       

        
        private void StartClient()
        {
            c = new Client();
            if (c.connectToSocket("localhost", 5000))
            {
                Stream();
            }
        }
        public void Stream()
        {
            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();
            string msg = "";
            int messageCounter = 0; // Counter to track the number of messages received

            while (true)
            {
                msg = c.receiveMessage();
                Console.WriteLine(msg);
                string[] parts = msg.Split(',');
                //Hand_Gesture = int.Parse(parts[0]);
                //Arm_Gesture = int.Parse(parts[1].Trim());

                messageCounter++;

                //if (Arm_Gesture != 0 && Number_of_Fingers == 0 && messageCounter >= 20)
                //{
                //    int previousRowNumber = row_number;

                //    if (Arm_Gesture == 8)
                //    {
                //        if (row_number == 1)
                //        {
                //            row_number = 2;
                //        }
                //        else if (row_number == 0)
                //        {
                //            row_number = 1;
                //        }
                //    }
                //    else if (Arm_Gesture == 9)
                //    {
                //        row_number--;
                //        if (row_number <= 0)
                //        {
                //            row_number = 0;
                //        }
                //    }


                //    if (row_number != previousRowNumber)
                //    {
                //        Thread.Sleep(2000);
                //        msg = "";
                //    }

                //    messageCounter = 0;
                //}

                //Number_of_Fingers = int.Parse(parts[2]);
                //if (Number_of_Fingers >= 3)
                //{
                //    Number_of_Fingers = 3;
                //}

                //if (Hand_Gesture == 7 && true_ct >= 3)
                //{
                //    MessageBox.Show("Done");
                //}
            }

        }
    }
}

