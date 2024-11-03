/*
	TUIO C# Demo - part of the reacTIVision project
	Copyright (c) 2005-2016 Martin Kaltenbrunner <martin@tuio.org>

	This program is free software; you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation; either version 2 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with this program; if not, write to the Free Software
	Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*/
using System.Reflection;
using System.Threading;
using System.Linq;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using TUIO;
using System.IO;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using System.Data;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
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

public class Circle
{
	public float X { get; set; }
	public float Y { get; set; }
	public int Width { get; set; }
	public int Height { get; set; }
	public Color Color { get; set; }

	public bool Show { get; set; }
	public string Text { get; set; }

	public Circle(float x, float y, int width, int height, Color color, string text = "", bool show = false)
	{
		X = x;
		Y = y;
		Width = width;
		Height = height;
		Color = color;
		Show = show;
		Text = text;
	}
}
// Define the RectangleShape class
public class RectangleShape
{
	public float X { get; set; }
	public float Y { get; set; }
	public int Width { get; set; }
	public int Height { get; set; }
	public Color Color { get; set; }

	public bool Show { get; set; }
	public string Text { get; set; }

	public RectangleShape(float x, float y, int width, int height, Color color, string text = "", bool show = false)
	{
		X = x;
		Y = y;
		Width = width;
		Height = height;
		Color = color;
		Show = show;
		Text = text;
	}
}

public class TuioDemo : Form, TuioListener
{
	private TuioClient client;
	private Dictionary<long, TuioObject> objectList;
	private Dictionary<long, TuioCursor> cursorList;
	private Dictionary<long, TuioBlob> blobList;

	public int currentScreen = 1;
	public int currentProduct = 5;
	float lastAngle = 0;


	public bool openMenu = false;
	public static int width, height;
	private int window_width = 640;
	private int window_height = 480;
	private int window_left = 0;
	private int window_top = 0;
	int Number_of_Fingers=0;
	int true_ct=0;
    int Hand_Gesture = 0;
    int Arm_Gesture =0;
    int row_number=0;
	Rectangle[,] boxes;
	Brush[,] brs = new Brush[3, 3];
	bool[,] stat = new bool[3, 3];
	private int screen_width = Screen.PrimaryScreen.Bounds.Width;
	private int screen_height = Screen.PrimaryScreen.Bounds.Height;
	List<Circle> circles = new List<Circle>();
	List<RectangleShape> rectangles = new List<RectangleShape>();

	private Client c;
	Bitmap off;
	PointF finger = new PointF();

	private bool fullscreen;
	private bool verbose;
	System.Windows.Forms.Timer tt = new System.Windows.Forms.Timer();
	Font font = new Font("Arial", 10.0f);
	SolidBrush fntBrush = new SolidBrush(Color.White);
	SolidBrush bgrBrush = new SolidBrush(Color.FromArgb(0, 0, 64));
	SolidBrush curBrush = new SolidBrush(Color.FromArgb(192, 0, 192));
	SolidBrush objBrush = new SolidBrush(Color.FromArgb(64, 0, 0));
	SolidBrush blbBrush = new SolidBrush(Color.FromArgb(64, 64, 64));
	Pen curPen = new Pen(new SolidBrush(Color.Blue), 1);
	int Gender = -1;
	int age = -1;
	int skin_type = -1;
	float yaxis, Xaxis;
	string genderText = "";
	string ageText = "";
	string skinTypeText = "";
	Image robot;

	public TuioDemo(int port)
	{

		verbose = false;
		fullscreen = false;
		width = window_width;
		height = window_height;

		this.ClientSize = new System.Drawing.Size(width, height);
		this.Name = "TuioDemo";
		this.Text = "TuioDemo";
		this.Load += TuioDemo_Load;
		this.Closing += new CancelEventHandler(Form_Closing);
		this.KeyDown += new KeyEventHandler(Form_KeyDown);
		this.WindowState = FormWindowState.Maximized;
        tt.Tick += Tt_Tick;
		tt.Interval = 100;
		tt.Start();
		this.SetStyle(ControlStyles.AllPaintingInWmPaint |
						ControlStyles.UserPaint |
						ControlStyles.DoubleBuffer, true);

		objectList = new Dictionary<long, TuioObject>(128);
		cursorList = new Dictionary<long, TuioCursor>(128);
		blobList = new Dictionary<long, TuioBlob>(128);

		client = new TuioClient(port);
		client.addTuioListener(this);

		client.connect();
	}

	public void checkThumbsUp()
    {
		if(Hand_Gesture==7)
        {

			if(currentScreen==0)
            {
				currentScreen = 1;

			}
			else if(currentScreen == 1)
            {
				currentScreen = 2;
			}
			else if (currentScreen == 2)
			{
				currentScreen = 3;
			}

		}
    }
    private void Tt_Tick(object sender, EventArgs e)
    {
		checkThumbsUp();
		if (Number_of_Fingers >= 1 && currentScreen==4)
		{
			CalculateTotalScore();
			transformBox();
		}

		this.Invalidate();
		
    }

    private void TuioDemo_Load(object sender, EventArgs e)
	{

		if (off == null)
		{
			off = new Bitmap(this.ClientSize.Width, this.ClientSize.Height);
		}
		width = this.ClientSize.Width;
		height = this.ClientSize.Height;
		robot = Image.FromFile("robot.png");
		////Section 1: Products
		//int circleDiameter = 200;
		//int circleY = 100;
		//circles.Add(new Circle((width / 2) - (circleDiameter / 2) - 200, circleY + 50, circleDiameter - 50, circleDiameter - 50, Color.Gray));
		//circles.Add(new Circle((width / 2) - (circleDiameter / 2), circleY, circleDiameter, circleDiameter, Color.LightGoldenrodYellow));
		//circles.Add(new Circle((width / 2) - (circleDiameter / 2) + 250, circleY + 50, circleDiameter - 50, circleDiameter - 50, Color.Gray));

		////Section 1 END

		//circles.Add(new Circle((width / 2) - (300 / 2), height / 2 - 300 / 2, 300, 300, Color.Teal));


		//circles.Add(new Circle(width - 230, 800, 100, 100, Color.Teal));



		///Products
		//int x = this.ClientSize.Width / 2 - 250;
		//int y = this.ClientSize.Height / 2 - 250;
		//int radius = 500;
		//circles.Add(new Circle(x, y, radius, radius, Color.Transparent, "IMAGE OF PRODUCT", true));
		//x = this.ClientSize.Width / 2 - 200;
		//y = this.ClientSize.Height / 2 - 200;
		//radius = 400;
		//circles.Add(new Circle(x, y, radius, radius, Color.Transparent, "IMAGE OF PRODUCT", true));


		///


		//int rectWidth = 50;
		//int rectHeight = 250;
		//int rectX = 100;
		//int rectY = 250;
		//rectangles.Add(new RectangleShape(rectX, rectY, rectWidth, rectHeight, Color.Gray));
		//rectY = 500;
		//rectangles.Add(new RectangleShape(rectX, rectY, rectWidth, rectHeight, Color.Gray));


		//int borderX = rectX;
		//int borderY = 250;
		//int borderWidth = rectWidth;
		//int borderHeight = rectHeight * 2;
		//rectangles.Add(new RectangleShape(borderX, borderY, borderWidth, borderHeight, Color.Transparent));


		//rectWidth = 250;
		//rectHeight = 50;
		//rectX = width / 2 - 375;
		//rectY = 850;
		//rectangles.Add(new RectangleShape(rectX, rectY, rectWidth, rectHeight, Color.Gray));
		//rectX = rectX + 250;
		//rectangles.Add(new RectangleShape(rectX, rectY, rectWidth, rectHeight, Color.Gray));

		//rectX = rectX + 250;
		//rectangles.Add(new RectangleShape(rectX, rectY, rectWidth, rectHeight, Color.Gray));


		//borderX = width / 2 - 375;
		//borderY = rectY;
		//borderWidth = rectWidth * 3;
		//borderHeight = rectHeight;
		//rectangles.Add(new RectangleShape(borderX, borderY, borderWidth, borderHeight, Color.Transparent));


		//rectWidth = 50;
		//rectHeight = 250;
		//rectX = width - 200;
		//rectY = 250;
		//rectangles.Add(new RectangleShape(rectX, rectY, rectWidth, rectHeight, Color.Gray));
		//rectY = 500;
		//rectangles.Add(new RectangleShape(rectX, rectY, rectWidth, rectHeight, Color.Gray));

		//borderX = rectX;
		//borderY = 250;
		//borderWidth = rectWidth;
		//borderHeight = rectHeight * 2;
		//rectangles.Add(new RectangleShape(borderX, borderY, borderWidth, borderHeight, Color.Transparent));




        Thread clientThread = new Thread(StartClient);
		create_boxes();
        clientThread.IsBackground = true;
        clientThread.Start();
    }
    private void StartClient()
    {
        c = new Client();
        if (c.connectToSocket("localhost", 5000))
        {
            Stream();
        }
    }


    //public void Stream()
    //{
    //    string msg = "";
    //    while (true)
    //    {
    //        msg = c.receiveMessage();
    //        string[] coords = msg.Split(',');
    //        finger.X = float.Parse(coords[0]);
    //        finger.Y = float.Parse(coords[1]);
    //        Console.WriteLine(coords[0]);
    //    }
    //}
    public void Stream()
    {
		
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			string msg = "";
			int messageCounter = 0; // Counter to track the number of messages received

			while (true)
			{
				msg = c.receiveMessage();
				Console.WriteLine(msg);
				string[] parts = msg.Split(',');
				Hand_Gesture = int.Parse(parts[0]);
				Arm_Gesture = int.Parse(parts[1].Trim());

				messageCounter++;

				if (Arm_Gesture != 0 && Number_of_Fingers == 0 && messageCounter >= 20)
				{
					int previousRowNumber = row_number;

					if (Arm_Gesture == 8)
					{
						if (row_number == 1)
						{
							row_number = 2;
						}
						else if (row_number == 0)
						{
							row_number = 1;
						}
					}
					else if (Arm_Gesture == 9)
					{
						row_number--;
						if (row_number <= 0)
						{
							row_number = 0;
						}
					}


					if (row_number != previousRowNumber)
					{
						Thread.Sleep(2000);
						msg = "";
					}

					messageCounter = 0;
				}

				Number_of_Fingers = int.Parse(parts[2]);
				if (Number_of_Fingers >= 3)
				{
					Number_of_Fingers = 3;
				}

				if (Hand_Gesture == 7 && true_ct == 3)
				{
					MessageBox.Show("Done");
				}
			}
		

    }


    private void Form_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
	{

		if (e.KeyData == Keys.F1)
		{
			if (fullscreen == false)
			{

				width = screen_width;
				height = screen_height;

				window_left = this.Left;
				window_top = this.Top;

				this.FormBorderStyle = FormBorderStyle.None;
				this.Left = 0;
				this.Top = 0;
				this.Width = screen_width;
				this.Height = screen_height;

				fullscreen = true;
			}
			else
			{

				width = window_width;
				height = window_height;

				this.FormBorderStyle = FormBorderStyle.Sizable;
				this.Left = window_left;
				this.Top = window_top;
				this.Width = window_width;
				this.Height = window_height;

				fullscreen = false;
			}
		}
		else if (e.KeyData == Keys.Escape)
		{
			this.Close();

		}
		else if (e.KeyData == Keys.Up)
		{
			currentScreen++;
		}
		else if (e.KeyData == Keys.Down)
		{
			currentScreen--;
		}
		else if (e.KeyData == Keys.V)
		{
			verbose = !verbose;
		}
		else if (e.KeyData == Keys.Q)
		{
			
			currentScreen = 4;
			
		}

	}

	private void Form_Closing(object sender, System.ComponentModel.CancelEventArgs e)
	{
		client.removeTuioListener(this);

		client.disconnect();
		System.Environment.Exit(0);
	}

	public void addTuioObject(TuioObject o)
	{
		lock (objectList)
		{
			objectList.Add(o.SessionID, o);
		}
		if (verbose) Console.WriteLine("add obj " + o.SymbolID + " (" + o.SessionID + ") " + o.X + " " + o.Y + " " + o.Angle);
	}

	public void updateTuioObject(TuioObject o)
	{

		if (verbose) Console.WriteLine("set obj " + o.SymbolID + " " + o.SessionID + " " + o.X + " " + o.Y + " " + o.Angle + " " + o.MotionSpeed + " " + o.RotationSpeed + " " + o.MotionAccel + " " + o.RotationAccel);
	}

	public void removeTuioObject(TuioObject o)
	{
		lock (objectList)
		{
			objectList.Remove(o.SessionID);
		}
		if (verbose) Console.WriteLine("del obj " + o.SymbolID + " (" + o.SessionID + ")");
	}

	public void addTuioCursor(TuioCursor c)
	{
		lock (cursorList)
		{
			cursorList.Add(c.SessionID, c);
		}
		if (verbose) Console.WriteLine("add cur " + c.CursorID + " (" + c.SessionID + ") " + c.X + " " + c.Y);
	}

	public void updateTuioCursor(TuioCursor c)
	{
		if (verbose) Console.WriteLine("set cur " + c.CursorID + " (" + c.SessionID + ") " + c.X + " " + c.Y + " " + c.MotionSpeed + " " + c.MotionAccel);
	}

	public void removeTuioCursor(TuioCursor c)
	{
		lock (cursorList)
		{
			cursorList.Remove(c.SessionID);
		}
		if (verbose) Console.WriteLine("del cur " + c.CursorID + " (" + c.SessionID + ")");
	}

	public void addTuioBlob(TuioBlob b)
	{
		lock (blobList)
		{
			blobList.Add(b.SessionID, b);
		}
		if (verbose) Console.WriteLine("add blb " + b.BlobID + " (" + b.SessionID + ") " + b.X + " " + b.Y + " " + b.Angle + " " + b.Width + " " + b.Height + " " + b.Area);
	}

	public void updateTuioBlob(TuioBlob b)
	{

		if (verbose) Console.WriteLine("set blb " + b.BlobID + " (" + b.SessionID + ") " + b.X + " " + b.Y + " " + b.Angle + " " + b.Width + " " + b.Height + " " + b.Area + " " + b.MotionSpeed + " " + b.RotationSpeed + " " + b.MotionAccel + " " + b.RotationAccel);
	}

	public void removeTuioBlob(TuioBlob b)
	{
		lock (blobList)
		{
			blobList.Remove(b.SessionID);
		}
		if (verbose) Console.WriteLine("del blb " + b.BlobID + " (" + b.SessionID + ")");
	}

	public void refresh(TuioTime frameTime)
	{
		Invalidate();
	}
	private GraphicsPath GetRoundedRectanglePath(Rectangle rect, int radius)
	{
		GraphicsPath path = new GraphicsPath();

		int diameter = radius * 2;
		Size size = new Size(diameter, diameter);
		Rectangle arc = new Rectangle(rect.Location, size);

		// Top-left corner
		path.AddArc(arc, 180, 90);

		// Top-right corner
		arc.X = rect.Right - diameter;
		path.AddArc(arc, 270, 90);

		// Bottom-right corner
		arc.Y = rect.Bottom - diameter;
		path.AddArc(arc, 0, 90);

		// Bottom-left corner
		arc.X = rect.Left;
		path.AddArc(arc, 90, 90);

		path.CloseFigure();
		return path;
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
				
				brs[row, col] = Brushes.Red;
				stat[row, col] = false;
			}
		}
	}
	protected override void OnPaintBackground(PaintEventArgs pevent)
	{
		Rectangle rect;
		// Getting the graphics object
		Graphics g = pevent.Graphics;
		g.FillRectangle(bgrBrush, new Rectangle(0, 0, width, height));
		Text = $"{genderText} {ageText} {skinTypeText}";


		// Use anti-aliasing to smooth graphics
		g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

		g.Clear(Color.WhiteSmoke);
		rect = new Rectangle(screen_width / 2 - 600, 300, 500, 500);
		rectangles.Add(new RectangleShape(screen_width / 2 - 600, 300, 500, 500, Color.Gray));
		rect = new Rectangle(screen_width / 2 + 100, 300, 500, 500);
        rectangles.Add(new RectangleShape(screen_width / 2 + 100, 300, 500, 500, Color.Gray));
		rect = new Rectangle(screen_width / 2 - 850, 300, 500, 500);
		rectangles.Add(new RectangleShape(screen_width / 2 - 850, 300, 500, 500, Color.Gray));
		rect = new Rectangle(screen_width / 2 - 250, 300, 500, 500);
		rectangles.Add(new RectangleShape(screen_width / 2 - 250, 300, 500, 500, Color.Gray));
		rect = new Rectangle(screen_width / 2 + 350, 300, 500, 500);
		rectangles.Add(new RectangleShape(screen_width / 2 + 350, 300, 500, 500, Color.Gray));
		rect = new Rectangle(screen_width / 2 - 850, 300, 500, 500);
		rectangles.Add(new RectangleShape(screen_width / 2 - 850, 300, 500, 500, Color.Gray));
		rect = new Rectangle(screen_width / 2 - 250, 300, 500, 500);
		rectangles.Add(new RectangleShape(screen_width / 2 - 250, 300, 500, 500, Color.Gray));
		rect = new Rectangle(screen_width / 2 + 350, 300, 500, 500);
		rectangles.Add(new RectangleShape(screen_width / 2 + 350, 300, 500, 500, Color.Gray));
		if (currentScreen == 0)
		{

			g.DrawImage(robot, 550, 200, 700, 700);
			Rectangle rect2 = new Rectangle(screen_width - 700, screen_height - 380, 600, 200);
			int radius2 = 20;
			Font font2 = new Font("Tahoma", 36, FontStyle.Bold);
			using (GraphicsPath path2 = GetRoundedRectanglePath(rect2, radius2))
			using (SolidBrush fillBrush2 = new SolidBrush(Color.FromArgb(128, Color.White)))
			using (Pen borderPen2 = new Pen(Color.Black, 2))
			{
				g.FillPath(fillBrush2, path2);

				// Draw the border around the rounded rectangle
				g.DrawPath(borderPen2, path2);

			}
			rect = new Rectangle(screen_width - 600, screen_height - 300, 400, 100);
			int radius = 20;
			string text = "Start!";
			Font font1 = new Font("Tahoma", 36, FontStyle.Bold);

			// Create the rounded rectangle path
			using (GraphicsPath path = GetRoundedRectanglePath(rect, radius))
			using (SolidBrush fillBrush = new SolidBrush(Color.FromArgb(200, Color.Green)))
			using (Pen borderPen = new Pen(Color.Black, 2))
			{
				// Fill the rounded rectangle
				g.FillPath(fillBrush, path);

				// Draw the border around the rounded rectangle
				g.DrawPath(borderPen, path);

				// Draw the text in the center of the rounded rectangle
				SizeF textSize = g.MeasureString(text, font1);
				PointF textPosition = new PointF(
					rect.X + (rect.Width - textSize.Width) / 2,
					rect.Y + (rect.Height - textSize.Height) / 2
				);
				g.DrawString(text, font1, Brushes.White, textPosition);
			}

			SolidBrush b = new SolidBrush(Color.Green);

			using (Font font = new Font("Tahoma", 48, FontStyle.Bold))
			{
				g.DrawString("AI Skin Care Assistant", font, Brushes.Black, new RectangleF(100, 100, 800, 300));
			}
			using (Font font = new Font("Tahoma", 16, FontStyle.Italic))
			{
				g.DrawString("Your way to a clean and clear skin.", font, Brushes.Black, new RectangleF(100, 200, 800, 600));
				g.DrawString("To start please hover over the 'Start!' button with your hand", font, Brushes.Black, new RectangleF(screen_width - 690, screen_height - 350, 800, 500));
			}





		}
		
		else if (currentScreen == 1)
		{

			rect = new Rectangle(screen_width / 2 - 600, 300, 500, 500);
			int radius = 20;
			Image image = Image.FromFile("male-white.png");

			// Calculate the position to center the image within the rectangle
			int imageX = rect.X + (rect.Width - image.Width) / 2;
			int imageY = rect.Y + (rect.Height - image.Height) / 2;

			using (GraphicsPath path = GetRoundedRectanglePath(rect, radius))
			using (SolidBrush fillBrush = new SolidBrush(Color.FromArgb(200, Color.Turquoise)))
			{
				// Fill the rounded rectangle
				g.FillPath(fillBrush, path);

				// Draw the image at the calculated position
				g.DrawImage(image, imageX, imageY, image.Width, image.Height);

			}

			rect = new Rectangle(screen_width / 2 + 100, 300, 500, 500);
			radius = 20;
			image = Image.FromFile("female-white.png");

			// Calculate the position to center the image within the rectangle
			imageX = rect.X + (rect.Width - image.Width) / 2;
			imageY = rect.Y + (rect.Height - image.Height) / 2;

			using (GraphicsPath path = GetRoundedRectanglePath(rect, radius))
			using (SolidBrush fillBrush = new SolidBrush(Color.FromArgb(200, Color.Pink)))
			{
				// Fill the rounded rectangle
				g.FillPath(fillBrush, path);

				// Draw the image at the calculated position
				g.DrawImage(image, imageX, imageY, image.Width, image.Height);
			}

			using (Font font = new Font("Tahoma", 36, FontStyle.Bold))
			{
				g.DrawString("Choose your gender", font, Brushes.Black, new RectangleF(screen_width / 2 - 275, 100, 550, 300));
			}
			image = Image.FromFile("right-arrow.png");

			// Calculate the position to center the image within the rectangle
			imageX = screen_width - 150;
			imageY = screen_height - 200;
			g.DrawImage(image, imageX, imageY, image.Width, image.Height);


		}
		
		else if (currentScreen == 2)
		{
			rect = new Rectangle(screen_width / 2 - 850, 300, 500, 500);
			int radius = 20;
			Image image = Image.FromFile("adult.png");

			// Calculate the position to center the image within the rectangle
			int imageX = rect.X + (rect.Width - 220) / 2;
			int imageY = rect.Y + (rect.Height - 220) / 2;

			using (GraphicsPath path = GetRoundedRectanglePath(rect, radius))
			using (SolidBrush fillBrush = new SolidBrush(Color.FromArgb(200, Color.LightGreen)))
			{
				// Fill the rounded rectangle
				g.FillPath(fillBrush, path);

				// Draw the image at the calculated position
				g.DrawImage(image, imageX, imageY, 220, 220);
			}

			
			image = Image.FromFile("middle-age.png");
			rect = new Rectangle(screen_width / 2 - 250, 300, 500, 500);
			// Calculate the position to center the image within the rectangle
			imageX = rect.X + (rect.Width - 220) / 2;
			imageY = rect.Y + (rect.Height - 220) / 2;

			using (GraphicsPath path = GetRoundedRectanglePath(rect, radius))
			using (SolidBrush fillBrush = new SolidBrush(Color.FromArgb(200, Color.Green)))
			{
				// Fill the rounded rectangle
				g.FillPath(fillBrush, path);

				// Draw the image at the calculated position
				g.DrawImage(image, imageX, imageY, 220, 220);
			}

			
			rect = new Rectangle(screen_width / 2 + 350, 300, 500, 500);
			radius = 20;
			image = Image.FromFile("old.png");

			// Calculate the position to center the image within the rectangle
			imageX = rect.X + (rect.Width - 220) / 2;
			imageY = rect.Y + (rect.Height - 220) / 2;

			using (GraphicsPath path = GetRoundedRectanglePath(rect, radius))
			using (SolidBrush fillBrush = new SolidBrush(Color.FromArgb(200, Color.DarkGreen)))
			{
				// Fill the rounded rectangle
				g.FillPath(fillBrush, path);

				// Draw the image at the calculated position
				g.DrawImage(image, imageX, imageY, 220, 220);
			}

			using (Font font = new Font("Tahoma", 36, FontStyle.Bold))
			{
				g.DrawString("Choose your age range", font, Brushes.Black, new RectangleF(screen_width / 2 - 300, 100, 600, 300));
			}
			image = Image.FromFile("right-arrow.png");

			// Calculate the position to center the image within the rectangle
			imageX = screen_width - 150;
			imageY = screen_height - 200;
			g.DrawImage(image, imageX, imageY, image.Width, image.Height);
		}
		
		else if (currentScreen == 3)
		{
			rect = new Rectangle(screen_width / 2 - 850, 300, 500, 500);
			int radius = 20;
			Image image = Image.FromFile("clear.png");

			// Calculate the position to center the image within the rectangle
			int imageX = rect.X + (rect.Width - 220) / 2;
			int imageY = rect.Y + (rect.Height - 220) / 2;

			using (GraphicsPath path = GetRoundedRectanglePath(rect, radius))
			using (SolidBrush fillBrush = new SolidBrush(Color.FromArgb(200, Color.Wheat)))
			{
				// Fill the rounded rectangle
				g.FillPath(fillBrush, path);

				// Draw the image at the calculated position
				g.DrawImage(image, imageX, imageY, 220, 220);
			}

			rect = new Rectangle(screen_width / 2 - 250, 300, 500, 500);
			radius = 20;
			image = Image.FromFile("dry.png");

			// Calculate the position to center the image within the rectangle
			imageX = rect.X + (rect.Width - 220) / 2;
			imageY = rect.Y + (rect.Height - 220) / 2;

			using (GraphicsPath path = GetRoundedRectanglePath(rect, radius))
			using (SolidBrush fillBrush = new SolidBrush(Color.FromArgb(200, Color.LightSalmon)))
			{
				// Fill the rounded rectangle
				g.FillPath(fillBrush, path);

				// Draw the image at the calculated position
				g.DrawImage(image, imageX, imageY, 220, 220);
			}

			rect = new Rectangle(screen_width / 2 + 350, 300, 500, 500);
			radius = 20;
			image = Image.FromFile("oily.png");

			// Calculate the position to center the image within the rectangle
			imageX = rect.X + (rect.Width - 220) / 2;
			imageY = rect.Y + (rect.Height - 220) / 2;

			using (GraphicsPath path = GetRoundedRectanglePath(rect, radius))
			using (SolidBrush fillBrush = new SolidBrush(Color.FromArgb(200, Color.DarkKhaki)))
			{
				// Fill the rounded rectangle
				g.FillPath(fillBrush, path);

				// Draw the image at the calculated position
				g.DrawImage(image, imageX, imageY, 220, 220);
			}

			using (Font font = new Font("Tahoma", 36, FontStyle.Bold))
			{
				g.DrawString("Choose your skin type", font, Brushes.Black, new RectangleF(screen_width / 2 - 300, 100, 600, 300));
			}
			image = Image.FromFile("right-arrow.png");

			// Calculate the position to center the image within the rectangle
			imageX = screen_width - 150;
			imageY = screen_height - 200;
			g.DrawImage(image, imageX, imageY, image.Width, image.Height);
		}
		else if (currentScreen == 4)
		{
			foreach (Circle circle in circles)
			{
				if (circle.Show)
				{

					if (circle.Color != Color.Transparent)
					{

						using (Brush gradientBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
							new RectangleF(circle.X, circle.Y, circle.Width, circle.Height),
							circle.Color, ControlPaint.Dark(circle.Color), 45f))
						{
							g.FillEllipse(gradientBrush, circle.X, circle.Y, circle.Width, circle.Height);
						}
					}


					if (circle.Color == Color.Transparent)
					{
						using (Pen blackPen = new Pen(Color.Black, 3))
						{
							g.DrawEllipse(blackPen, circle.X, circle.Y, circle.Width, circle.Height);

						}

					}
				}
			}
			///////
			int segments = 6;
			float angleStep = 360f / segments;
			int centerX = this.ClientSize.Width / 2;
			int centerY = this.ClientSize.Height / 2;


			for (int i = 0; i < segments; i++)
			{
				float startAngle = i * angleStep;
				float sweepAngle = angleStep;



				using (GraphicsPath path = new GraphicsPath())
				{
					Rectangle outerRect = new Rectangle(
						centerX - 250,
						centerY - 250,
						250 * 2,
						250 * 2);
					path.AddArc(outerRect, startAngle, sweepAngle);
					Rectangle innerRect = new Rectangle(
						centerX - 200,
						centerY - 200,
						200 * 2,
						200 * 2);
					path.AddArc(innerRect, startAngle + sweepAngle, -sweepAngle);
					path.CloseFigure();


					if (i == currentProduct)
					{
						g.FillPath(Brushes.LightBlue, path);
					}
					else
					{
						g.FillPath(Brushes.LightPink, path);

					}


					double radians = startAngle * Math.PI / 180;
					int outerX = centerX + (int)(250 * Math.Cos(radians));
					int outerY = centerY + (int)(250 * Math.Sin(radians));

					int innerX = centerX + (int)(200 * Math.Cos(radians));
					int innerY = centerY + (int)(200 * Math.Sin(radians));

					g.DrawLine(Pens.Black, innerX, innerY, outerX, outerY);
					// Add rotated text to the center of each arc
					float textAngle = startAngle + sweepAngle / 2;
					double textRadians = textAngle * Math.PI / 180;
					int textX = centerX + (int)(225 * Math.Cos(textRadians));
					int textY = centerY + (int)(225 * Math.Sin(textRadians));

					string text = $"Segment {i + 1}";
					Font font = new Font("Arial", 12, FontStyle.Bold);
					SizeF textSize = g.MeasureString(text, font);


					g.TranslateTransform(textX, textY);
					if (i == 0 || i == 1 || i == 2)
					{
						g.RotateTransform(textAngle + 270);
					}
					else
					{
						g.RotateTransform(textAngle + 90);

					}

					// Draw text centered at the rotated position
					g.DrawString(text, font, Brushes.Black, -textSize.Width / 2, -textSize.Height / 2);


					// Reset transformation
					g.ResetTransform();
				}
				using (Font font = new Font("Tahoma", 36, FontStyle.Bold))
				{
					g.DrawString("Recommended Products", font, Brushes.Black, new RectangleF(screen_width / 2 - 300, 100, 700, 300));
				}
			}
		}
		else if (currentScreen == 5)
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


			//////

			foreach (RectangleShape rectangle in rectangles)
				{
					if (rectangle.Show)
					{
						if (rectangle.Color != Color.Transparent)
						{

							using (Brush gradientBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
								new RectangleF(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height),
								rectangle.Color, ControlPaint.Dark(rectangle.Color), 45f))
							{
								g.FillRectangle(gradientBrush, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
							}
						}


						if (rectangle.Color == Color.Transparent)
						{
							using (Pen blackPen = new Pen(Color.Black, 3))
							{
								g.DrawRectangle(blackPen, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
							}
						}
					}
				}


				using (Font font = new Font("Segoe UI", 16, FontStyle.Bold))
					//using (StringFormat format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
					//{
					//	g.DrawString("Questionnaire", font, Brushes.Black, new RectangleF(circles[4].X -10, circles[4].Y + circles[4].Height+10 , circles[4].Width +120, 80));
					//	g.DrawString("Rotate", font, Brushes.Black, new RectangleF(circles[3].X, circles[3].Y + circles[3].Height + 10, circles[3].Width, 30), format);
					//	g.DrawString("Adult", font, Brushes.Black, rectangles[0].X + rectangles[0].Width + 10, rectangles[0].Y + rectangles[0].Height / 2 - 10);
					//	g.DrawString("Teen", font, Brushes.Black, rectangles[1].X + rectangles[1].Width + 10, rectangles[1].Y + rectangles[1].Height / 2 - 10);
					//	g.DrawString("Dry", font, Brushes.Black, new RectangleF(rectangles[3].X, rectangles[3].Y + rectangles[3].Height + 10, rectangles[3].Width, 30), format);
					//	g.DrawString("Normal", font, Brushes.Black, new RectangleF(rectangles[4].X, rectangles[4].Y + rectangles[4].Height + 10, rectangles[4].Width, 30), format);
					//	g.DrawString("Oily", font, Brushes.Black, new RectangleF(rectangles[5].X, rectangles[5].Y + rectangles[5].Height + 10, rectangles[5].Width, 30), format);
					//	g.DrawString("Male", font, Brushes.Black, rectangles[7].X + rectangles[7].Width + 10, rectangles[7].Y + rectangles[7].Height / 2 - 10);
					//	g.DrawString("Female", font, Brushes.Black, rectangles[8].X + rectangles[8].Width + 10, rectangles[8].Y + rectangles[8].Height / 2 - 10 );
					//}

					// draw the cursor path
					if (cursorList.Count > 0)
					{
						lock (cursorList)
						{
							foreach (TuioCursor tcur in cursorList.Values)
							{
								List<TuioPoint> path = tcur.Path;
								TuioPoint current_point = path[0];

								for (int i = 0; i < path.Count; i++)
								{
									TuioPoint next_point = path[i];
									g.DrawLine(curPen, current_point.getScreenX(width), current_point.getScreenY(height), next_point.getScreenX(width), next_point.getScreenY(height));
									current_point = next_point;
								}
								g.FillEllipse(curBrush, current_point.getScreenX(width) - height / 100, current_point.getScreenY(height) - height / 100, height / 50, height / 50);
								g.DrawString(tcur.CursorID + "", font, fntBrush, new PointF(tcur.getScreenX(width) - 10, tcur.getScreenY(height) - 10));
							}
						}
					}


				// draw the objects
				if (objectList.Count > 0)
				{
					lock (objectList)
					{
						foreach (TuioObject tobj in objectList.Values)
						{
							string objectImagePath = "";
							string backgroundImagePath = "";

							int ox = tobj.getScreenX(width);
							int oy = tobj.getScreenY(height);
							int size = height / 10;

							g.TranslateTransform(ox, oy);
							g.RotateTransform((float)(tobj.Angle / Math.PI * 180.0f));
							g.TranslateTransform(-ox, -oy);

							g.FillRectangle(objBrush, new Rectangle(ox - size / 2, oy - size / 2, size, size));

							g.TranslateTransform(ox, oy);
							g.RotateTransform(-1 * (float)(tobj.Angle / Math.PI * 180.0f));
							g.TranslateTransform(-ox, -oy);
							switch (tobj.SymbolID)
							{


							case 9:
							yaxis = tobj.Y * ClientSize.Height;
							Xaxis = tobj.X * ClientSize.Width;
							if (currentScreen == 1)
							{
							
								if (yaxis >= rectangles[0].Y && yaxis <= rectangles[0].Y + rectangles[0].Height && Xaxis >= rectangles[0].X && Xaxis <= rectangles[0].X + rectangles[0].Width)
								{
									Gender = 1;
									rectangles[7].Color = Color.Cyan;
									rectangles[8].Color = Color.Gray;

								}
								else
								{
									if (yaxis >= rectangles[1].Y && yaxis <= rectangles[1].Y + rectangles[1].Height && Xaxis >= rectangles[1].X && Xaxis <= rectangles[1].X + rectangles[1].Width)
									{
										Gender = 0;
										rectangles[7].Color = Color.Gray;
										rectangles[8].Color = Color.Pink;
									}

								}
								genderText = Gender == 0 ? "Female" : "Male";
								this.Text = genderText;
							}
							else if(currentScreen == 2)
                            {
								yaxis = tobj.Y * ClientSize.Height;
								Xaxis = tobj.X * ClientSize.Width;
								if (yaxis >= rectangles[2].Y && yaxis <= rectangles[2].Y + rectangles[2].Height && Xaxis >= rectangles[2].X && Xaxis <= rectangles[2].X + rectangles[2].Width)
								{
									age = 0;
								
								}
								else
								{
									if (yaxis >= rectangles[3].Y && yaxis <= rectangles[3].Y + rectangles[3].Height && Xaxis >= rectangles[3].X && Xaxis <= rectangles[3].X + rectangles[3].Width)
									{
										age = 1;
										
									}
									else
									{
										if (yaxis >= rectangles[4].Y && yaxis <= rectangles[4].Y + rectangles[4].Height && Xaxis >= rectangles[4].X && Xaxis <= rectangles[4].X + rectangles[4].Width)
										{
											age = 2;
										}
									}
								}
								
								
								this.Text = age.ToString();
							}
							else if(currentScreen==3)
                            {
								yaxis = tobj.Y * ClientSize.Height;
								Xaxis = tobj.X * ClientSize.Width;
								if (yaxis >= rectangles[5].Y && yaxis <= rectangles[5].Y + rectangles[5].Height && Xaxis >= rectangles[5].X && Xaxis <= rectangles[5].X + rectangles[5].Width)
								{
									skinTypeText = "Normal skin";

									



								}
								else if (yaxis >= rectangles[6].Y && yaxis <= rectangles[6].Y + rectangles[6].Height && Xaxis >= rectangles[6].X && Xaxis <= rectangles[6].X + rectangles[6].Width)
								{
									skinTypeText = "Dry skin";
									
									


								}
								else if (yaxis >= rectangles[7].Y && yaxis <= rectangles[7].Y + rectangles[7].Height && Xaxis >= rectangles[7].X && Xaxis <= rectangles[7].X + rectangles[7].Width)
								{

									skinTypeText = "Oily skin";

								}
							
								this.Text = skinTypeText;
							}
							else if(currentScreen==4)
                            {

								yaxis = tobj.Y * ClientSize.Height;
								Xaxis = tobj.X * ClientSize.Width;
								float currentAngle = tobj.Angle / (float)Math.PI * 180.0f % 360;
								float angleDifference = currentAngle - lastAngle;

								// Adjust angle difference to be within the range [-180, 180]
								if (angleDifference > 180)
								{
									angleDifference -= 360;
								}
								else if (angleDifference < -180)
								{
									angleDifference += 360;
								}

								// Check if the angle difference (in either direction) meets the threshold for a 45-degree turn
								if (Math.Abs(angleDifference) >= 45)
								{
									if (angleDifference > 0)
									{
										currentProduct = (currentProduct + 1) % 6; // Clockwise
									}
									else
									{
										currentProduct = (currentProduct - 1 + 6) % 6; // Anti-clockwise
									}

									lastAngle = currentAngle;
									objectImagePath = Path.Combine(Environment.CurrentDirectory, $"{currentProduct + 1}.png");
									backgroundImagePath = Path.Combine(Environment.CurrentDirectory, $"{(currentProduct + 2) % 6 + 1}.png");
									Text = $"Product {currentProduct + 1}";
									Console.WriteLine($"Angle: {currentAngle}, Current Product: {currentProduct + 1}");
								}
								this.Text = Text;



					}
							break;

								
								
								case 3:
									yaxis = tobj.Y * ClientSize.Height;
									Xaxis = tobj.X * ClientSize.Width;
									if (yaxis >= circles[3].Y && yaxis <= circles[3].Y + circles[3].Height && Xaxis >= circles[3].X && Xaxis <= circles[3].X + circles[3].Width)
									{
										if (tobj.Angle / Math.PI * 180.0f > 0 && tobj.Angle / Math.PI * 180.0f < 90)
										{
											objectImagePath = Path.Combine(Environment.CurrentDirectory, "1.png");
											backgroundImagePath = Path.Combine(Environment.CurrentDirectory, "2.png");
											Text = "product 1";
											circles[0].Color = Color.PaleVioletRed;
											circles[1].Color = Color.Gray;
											circles[2].Color = Color.Gray;
											Console.WriteLine(tobj.Angle / Math.PI * 180.0f);

										}

										if (tobj.Angle / Math.PI * 180.0f > 120 && tobj.Angle / Math.PI * 180.0f < 180)
										{
											objectImagePath = Path.Combine(Environment.CurrentDirectory, "1.png");
											backgroundImagePath = Path.Combine(Environment.CurrentDirectory, "3.png");
											Text = "product 2";
											circles[0].Color = Color.Gray;
											circles[1].Color = Color.PaleVioletRed;
											circles[2].Color = Color.Gray;
											Console.WriteLine(tobj.Angle / Math.PI * 180.0f);
										}
										if (tobj.Angle / Math.PI * 180.0f > 240 && tobj.Angle / Math.PI * 180.0f < 270)
										{
											objectImagePath = Path.Combine(Environment.CurrentDirectory, "1.png");
											backgroundImagePath = Path.Combine(Environment.CurrentDirectory, "4.png");
											Text = "product 3";
											circles[0].Color = Color.Gray;
											circles[1].Color = Color.Gray;
											circles[2].Color = Color.PaleVioletRed;
											Console.WriteLine(tobj.Angle / Math.PI * 180.0f);
										}
										if (tobj.Angle / Math.PI * 180.0f > 270 && tobj.Angle / Math.PI * 180.0f < 360)
										{
											circles[0].Color = Color.Gray;
											circles[1].Color = Color.Gray;
											circles[2].Color = Color.Gray;
										}
									}

									this.Text = Text;
									break;
								///Open Menu
								case 4:
									yaxis = tobj.Y * ClientSize.Height;
									Xaxis = tobj.X * ClientSize.Width;
									if (yaxis >= circles[5].Y && yaxis <= circles[5].Y + circles[5].Height && Xaxis >= circles[5].X && Xaxis <= circles[5].X + circles[5].Width)
									{
										openMenu = true;
									}
									break;
								///close Menu
								case 5:
									yaxis = tobj.Y * ClientSize.Height;
									Xaxis = tobj.X * ClientSize.Width;
									if (yaxis >= circles[5].Y && yaxis <= circles[5].Y + circles[5].Height && Xaxis >= circles[5].X && Xaxis <= circles[5].X + circles[5].Width)
									{
										openMenu = false;
									}
									break;

								default:

									g.FillRectangle(objBrush, new Rectangle(ox - size / 2, oy - size / 2, size, size));
									g.DrawString(tobj.SymbolID + "", font, fntBrush, new PointF(ox - 10, oy - 10));
									break;
							}


							try
							{
								// Draw background image without rotation
								if (File.Exists(backgroundImagePath))
								{
									using (Image bgImage = Image.FromFile(backgroundImagePath))
									{
										g.DrawImage(bgImage, new Rectangle(new Point(0, 0), new Size(width, height)));
									}
								}
								else
								{
									Console.WriteLine($"Background image not found: {backgroundImagePath}");
								}

								// Draw object image with rotation
								if (File.Exists(objectImagePath))
								{
									using (Image objectImage = Image.FromFile(objectImagePath))
									{
										// Save the current state of the graphics object
										GraphicsState state = g.Save();

										// Apply transformations for rotation
										g.TranslateTransform(ox, oy);
										g.RotateTransform((float)(tobj.Angle / Math.PI * 180.0f));
										g.TranslateTransform(-ox, -oy);

										// Draw the rotated object
										g.DrawImage(objectImage, new Rectangle(ox - size / 2, oy - size / 2, size, size));

										// Restore the graphics state
										g.Restore(state);
									}
								}
								else
								{
									Console.WriteLine($"Object image not found: {objectImagePath}");
									// Fall back to drawing a rectangle
									g.FillRectangle(objBrush, new Rectangle(ox - size / 2, oy - size / 2, size, size));
								}
							}
							catch
							{

							}

							g.DrawString(tobj.SymbolID + "", font, fntBrush, new PointF(ox - 10, oy - 10));
						}
					}
				}

				// draw the blobs
				if (blobList.Count > 0)
				{

					lock (blobList)
					{
						foreach (TuioBlob tblb in blobList.Values)
						{
							int bx = tblb.getScreenX(width);
							int by = tblb.getScreenY(height);
							float bw = tblb.Width * width;
							float bh = tblb.Height * height;

							g.TranslateTransform(bx, by);
							g.RotateTransform((float)(tblb.Angle / Math.PI * 180.0f));
							g.TranslateTransform(-bx, -by);

							g.FillEllipse(blbBrush, bx - bw / 2, by - bh / 2, bw, bh);

							g.TranslateTransform(bx, by);
							g.RotateTransform(-1 * (float)(tblb.Angle / Math.PI * 180.0f));
							g.TranslateTransform(-bx, -by);

							g.DrawString(tblb.BlobID + "", font, fntBrush, new PointF(bx, by));
						}
					}
				}
			}
		
	
		public static void Main(String[] argv)
		{
			int port = 0;
			switch (argv.Length)
			{
				case 1:
					port = int.Parse(argv[0], null);
					if (port == 0) goto default;
					break;
				case 0:
					port = 3333;
					break;
				default:
					Console.WriteLine("usage: mono TuioDemo [port]");
					System.Environment.Exit(0);
					break;
			}

			TuioDemo app = new TuioDemo(port);
			Application.Run(app);
		}
		//void LaunchQuestionnaire()
		//{
		//    try
		//    {
		//        // Assuming Code B is compiled as an executable
		//        Process.Start("C:\\Users\\Pierre\\source\\repos\\HCI-PROJECT\\questi\\bin\\Debug\\questionnaire.exe");
		//    }
		//    catch (Exception e)
		//    {
		//        Console.WriteLine("Failed to launch questionnaire: " + e.Message);
		//    }
		//}

	}


