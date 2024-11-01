using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace questionnaire
{
    public partial class Form1 : Form
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
        Rectangle[,] boxes;
        Dictionary<(int, int), float> hoverDurations = new Dictionary<(int, int), float>();
        const float requiredHoverTime = 1000;

        public Form1()
        {
            InitializeComponent();
            this.Paint += Form1_Paint;
            this.WindowState = FormWindowState.Maximized;
            tt.Tick += Tt_Tick;
            tt.Interval = 100;
            tt.Start();
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


        public void pointer()
        {
            if (boxes != null)
            {
                for (int row = 0; row < 3; row++)
                {
                    for (int col = 0; col < 3; col++)
                    {
                        if (finger.X * width >= boxes[row, col].X && finger.X * width <= boxes[row, col].X + boxes[row, col].Width &&
                            finger.Y * height >= boxes[row, col].Y && finger.Y * height <= boxes[row, col].Y + boxes[row, col].Height)
                        {
                            if (hoverDurations.ContainsKey((row, col)))
                            {
                                hoverDurations[(row, col)] += tt.Interval;
                            }
                            else
                            {
                                hoverDurations[(row, col)] = tt.Interval;
                            }

                            if (hoverDurations[(row, col)] >= requiredHoverTime)
                            {

                                for (int j = 0; j < 3; j++)
                                {
                                    if (j != col)
                                    {
                                        if (stat[row, j])
                                        {
                                            brs[row, j] = Brushes.Red;
                                            stat[row, j] = false;
                                            hoverDurations[(row, j)] = 0;
                                        }
                                    }
                                }

                                if (!stat[row, col])
                                {
                                    brs[row, col] = Brushes.Black;
                                    stat[row, col] = true;
                                }
                                else if (stat[row, col] == true && row == preR && col == preC)
                                {
                                    brs[row, col] = Brushes.Red;
                                    stat[row, col] = false;
                                }

                                hoverDurations[(row, col)] = 0;


                                total_score = CalculateTotalScore();
                                this.Text = $"Total Score: {total_score}";
                            }
                        }
                        else
                        {
                            hoverDurations[(row, col)] = 0;
                        }
                    }
                }
            }
        }


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

            return score;
        }


        private void Tt_Tick(object sender, EventArgs e)
        {
            pointer();
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


        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            DrawDubb(e.Graphics);
        }

        private void Form1_Load(object sender, EventArgs e)
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
            string msg = "";
            while (true)
            {
                msg = c.receiveMessage();
                string[] coords = msg.Split(',');
                finger.X = float.Parse(coords[0]);
                finger.Y = float.Parse(coords[1]);

            }
        }
    }
    class Client
    {
        public NetworkStream stream;
        public TcpClient client;

        public bool connectToSocket(string host, int portNumber)
        {
            try
            {
                client = new TcpClient(host, portNumber);
                stream = client.GetStream();
                Console.WriteLine("Connection made! with " + host);
                return true;
            }
            catch (SocketException e)
            {
                Console.WriteLine("Connection Failed: " + e.Message);
                return false;
            }
        }

        public string receiveMessage()
        {
            try
            {
                byte[] receiveBuffer = new byte[1024];
                int bytesReceived = stream.Read(receiveBuffer, 0, 1024);
                string data = Encoding.UTF8.GetString(receiveBuffer, 0, bytesReceived);
                return data;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error receiving message: " + e.Message);
            }

            return null;
        }
    }
}