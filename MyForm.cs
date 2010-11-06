using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;

namespace XYModel
{
    public partial class MyForm : Form
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MyForm(256, 256));
        }

        ManagerForm managerForm;
        public MyForm(int X, int Y)
        {
            InitializeComponent();
            ManagerFormInitialize();
            this.managerForm.g_textbox.Text = g_beta.ToString();
            this.managerForm.h_textBox.Text = h.ToString();
            this.managerForm.Theta0_textBox.Text = Theta_0.ToString();
            this.managerForm.Speed_textBox.Text = Speed.ToString();
            Initialize(X, Y);
            this.advance_timer.Enabled = true;
            this.average_calc_timer.Enabled = true;
            //this.OldLocation = this.Location;
        }


        void Initialize(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
            this.managerForm.Width_textBox.Text = X.ToString();
            this.managerForm.Height_textBox.Text = Y.ToString();
            var canvas = this.canvas = new Bitmap(X, Y);
            this.pictureBox1.Image = canvas;
            var Theta = this.Theta = new double[X, Y];
            for (int y = 0; y < Y; y++)
                for (int x = 0; x < X; x++)
                {
                    var degree = random.NextDouble() * 2 * PI;
                    this.Theta[x, y] = degree;
                    byte r, g, b;
                    HSLToRGB((int)(degree * 180 / PI), 255, 128, out r, out g, out b);
                    canvas.SetPixel(x, y, Color.FromArgb(r, g, b));
                }
        }


        static readonly Random random = new Random();
        const double PI = Math.PI;
        Bitmap canvas;
        int X, Y;

        double g_beta = 1, h = 0;
        double Theta_0 = 0;
        double[,] Theta;

        void Advance(int x, int y)
        {
            double NewTheta = random.NextDouble() * 2 * PI;
            double Theta = this.Theta[x, y];
            double ThetaAvg = (Theta + NewTheta) / 2.0;
            double H = 0;
            if (x != 0)
                H += Sin(ThetaAvg - this.Theta[x - 1, y]);
            else
                H += Sin(ThetaAvg - this.Theta[X - 1, y]);

            if (x != X - 1)
                H += Sin(ThetaAvg - this.Theta[x + 1, y]);
            else
                H += Sin(ThetaAvg - this.Theta[0, y]);

            if (y != 0)
                H += Sin(ThetaAvg - this.Theta[x, y - 1]);
            else
                H += Sin(ThetaAvg - this.Theta[x, Y - 1]);

            if (y < Y - 1)
                H += Sin(ThetaAvg - this.Theta[x, y + 1]);
            else
                H += Sin(ThetaAvg - this.Theta[x, 0]);

            H *= -2 * g_beta * Sin((NewTheta - Theta) / 2.0);
            H -= 2 * h * Sin(ThetaAvg - Theta_0) * Sin((NewTheta - Theta) / 2.0);
            if (H > 0 || Math.Exp(H) > random.NextDouble())
            {
                this.Theta[x, y] = NewTheta;
                byte r, g, b;
                HSLToRGB((int)(NewTheta * 180 / PI), 255, 128, out r, out g, out b);
                canvas.SetPixel(x, y, Color.FromArgb(r, g, b));
            }

        }

        double Sin(double degree)
        {
            return Math.Sin(degree);
        }
        double Cos(double degree)
        {
            return Math.Cos(degree);
        }
        static void HSLToRGB(int h, int s, int l,
                                                     out byte rr, out byte gg, out byte bb)
        {
            int hh = h;
            double ss = s / 255d;
            double ll = l / 255d;

            double r, g, b, maxc, minc;

            if (s == 0)
            {
                r = g = b = ll;
            }
            else
            {
                if (ll <= 0.5) maxc = ll * (1 + ss); else maxc = ll * (1 - ss) + ss;
                minc = 2 * ll - maxc;

                int hhh = hh + 120;
                if (hhh >= 360) hhh = hhh - 360;
                if (hhh < 60) r = minc + (maxc - minc) * hhh / 60;
                else if (hhh < 180) r = maxc;
                else if (hhh < 240) r = minc + (maxc - minc) * (240 - hhh) / 60;
                else r = minc;

                hhh = hh;
                if (hhh < 60) g = minc + (maxc - minc) * hhh / 60;
                else if (hhh < 180) g = maxc;
                else if (hhh < 240) g = minc + (maxc - minc) * (240 - hhh) / 60;
                else g = minc;

                hhh = hh - 120;
                if (hhh < 0) hhh = hhh + 360;
                if (hhh < 60) b = minc + (maxc - minc) * hhh / 60;
                else if (hhh < 180) b = maxc;
                else if (hhh < 240) b = minc + (maxc - minc) * (240 - hhh) / 60;
                else b = minc;
            }
            rr = (byte)(r * 255);
            gg = (byte)(g * 255);
            bb = (byte)(b * 255);
        }





        void ManagerFormInitialize()
        {
            var form = this.managerForm = new ManagerForm();
            form.Show();
            form.Set_button.Click += new EventHandler(Set_button_Click);
            form.Reset_button.Click += new EventHandler(Reset_button_Click);
            form.NewToolStripButton.Click += new EventHandler(NewToolStripButton_Click);
            form.OpenToolStripButton.Click += new EventHandler(OpenToolStripButton_Click);
            form.SaveToolStripButton.Click += new EventHandler(SaveToolStripButton_Click);
            form.g_textbox.KeyDown += new KeyEventHandler(g_textbox_KeyDown);

            this.pictureBox1.MouseHover += new EventHandler(pictureBox1_MouseHover);
        }

        void pictureBox1_MouseHover(object sender, EventArgs e)
        {
            this.managerForm.Visible = true;
            this.managerForm.Focus();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            this.managerForm.Dispose();
            base.OnClosing(e);
        }

        void Reset_button_Click(object sender, EventArgs e)
        {
            var form = this.managerForm;

            {
                form.g_textbox.Text = g_beta.ToString();
                form.Width_textBox.Text = this.X.ToString();
                form.Height_textBox.Text = this.Y.ToString();
                form.Speed_textBox.Text = Speed.ToString();
                form.h_textBox.Text = h.ToString();
                form.Theta0_textBox.Text = Theta_0.ToString();
            }


        }

        void Set_button_Click(object sender, EventArgs e)
        {
            var mes = "";
            try
            { this.g_beta = double.Parse(this.managerForm.g_textbox.Text); }
            catch { mes += "g Invalid!\n"; }

            var flag = true;
            int X = this.X, Y = this.Y;
            try
            {
                X = int.Parse(this.managerForm.Width_textBox.Text);
            }
            catch { mes += "X Invalid!\n"; flag = false; }
            try
            {
                Y = int.Parse(this.managerForm.Height_textBox.Text);
            }
            catch { mes += "Y Invalid!\n"; flag = false; }
            if (flag && X != this.X || Y != this.Y)
                this.ResizeTheta(X, Y);

            try
            { this.Speed = int.Parse(this.managerForm.Speed_textBox.Text); }
            catch { mes += "Speed Invalid!\n"; }

            try
            { this.h = double.Parse(this.managerForm.h_textBox.Text); }
            catch { mes += "h Invalid!\n"; }

            try
            { this.Theta_0 = double.Parse(this.managerForm.Theta0_textBox.Text); }
            catch { mes += "Theta_0 Invalid!\n"; }


            if (mes != "")
            {
                MessageBox.Show(mes, "Exceptions!");
            }

        }
        const string ImageFilter =
            "Image File|*.bmp;*.jpg;*.jpeg;*.png;*.gif";
        //"Bitmap(*.bmp)|*.bmp|JPEG File(*.jpg;*.jpeg)|*.jpg;*.jpeg|PNG File(*.png)|*.png|GIF File(*.gif)|*.gif";
        static SaveFileDialog sfd = new SaveFileDialog()
        {
            Filter = ImageFilter
        };
        static OpenFileDialog ofd = new OpenFileDialog()
        {
            Filter = ImageFilter
        };
        void SaveToolStripButton_Click(object sender, EventArgs e)
        {
            if (sfd.ShowDialog(this.managerForm) == DialogResult.OK)
            {
                var filename = sfd.FileName;

                if (filename.ToLower().EndsWith(".png"))
                    canvas.Save(filename, ImageFormat.Png);
                else if (filename.ToLower().EndsWith(".gif"))
                    canvas.Save(filename, ImageFormat.Gif);
                else if (filename.ToLower().EndsWith(".jpg") || filename.ToLower().EndsWith(".jpeg"))
                    canvas.Save(filename, ImageFormat.Jpeg);
                else
                    canvas.Save(filename, ImageFormat.Bmp);

            }
        }

        void LoadBitmap(Bitmap bitmap)
        {
            this.pictureBox1.Image = this.canvas = bitmap;

            this.X = this.canvas.Width;
            this.Y = this.canvas.Height;
            this.Theta = new double[X, Y];
            for (int y = 0; y < Y; y++)
                for (int x = 0; x < X; x++)
                {
                    var degree = canvas.GetPixel(x, y).GetHue() * PI / 180;
                    this.Theta[x, y] = degree;
                }
            this.managerForm.Width_textBox.Text = X.ToString();
            this.managerForm.Height_textBox.Text = Y.ToString();
        }

        void OpenToolStripButton_Click(object sender, EventArgs e)
        {
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                using (var stream = ofd.OpenFile())
                    LoadBitmap(Image.FromStream(stream) as Bitmap);
            }
        }

        void NewToolStripButton_Click(object sender, EventArgs e)
        {
            this.Initialize(X, Y);
        }



        private void g_textbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                try
                {
                    this.g_beta = double.Parse(this.managerForm.g_textbox.Text);
                    e.Handled = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            else if (e.KeyCode == Keys.Escape)
            {
                this.managerForm.g_textbox.Text = this.g_beta.ToString();
                e.Handled = true;
            }
        }

        void ResizeTheta(int X, int Y)
        {
            Bitmap resized = new Bitmap(X, Y);
            var g = Graphics.FromImage(resized);
            g.DrawImage(this.canvas, new Rectangle(0, 0, X, Y));
            g.Dispose();
            LoadBitmap(resized);
        }
        /*
        void _ResizeTheta(int X, int Y)
        {
            var Theta_ = this.Theta;
            var X_ = this.X;
            var Y_ = this.Y;

            var Theta = new double[X, Y];
            this.Theta = Theta;
            this.X = X;
            this.Y = Y;
            var canvas = this.canvas = new Bitmap(X, Y);
            this.pictureBox1.Image = canvas;
            for (int y = 0; y < Y; y++)
                for (int x = 0; x < X; x++)
                {
                    int x_, rx, y_, ry;
                    x_ = Math.DivRem(x * X_, X, out rx);
                    y_ = Math.DivRem(y * Y_, Y, out ry);

                    var degree = Theta_[x_, y_];


                    this.Theta[x, y] = degree;
                    byte r, g, b;
                    HSLToRGB((int)(degree * 180 / PI), 255, 128, out r, out g, out b);
                    canvas.SetPixel(x, y, Color.FromArgb(r, g, b));
                }
            this.managerForm.Width_textBox.Text = X.ToString();
            this.managerForm.Height_textBox.Text = Y.ToString();
        }
        */

        private void average_calc_timer_Tick(object sender, EventArgs e)
        {
            double Cos = 0, Sin = 0;
            foreach (var theta in this.Theta)
            {
                Cos += this.Cos(theta);
                Sin += this.Sin(theta);
            }
            Cos /= X * Y;
            Sin /= X * Y;
            this.Text = string.Format("XY Model ({0},{1})", Cos.ToString("0.00"), Sin.ToString("0.00"));
        }



        int Speed = 12;
        private void advance_timer_Tick(object sender, EventArgs e)
        {
            var Now = DateTime.Now;

            int N = 1 << Speed;

            for (int n = 0; n < N; n++)
            {
                int x = random.Next(0, X);
                int y = random.Next(0, Y);
                this.Advance(x, y);
            }

            this.pictureBox1.Invalidate();

            var Elapsed = DateTime.Now - Now;
            this.advance_timer.Interval = (int)(Elapsed.TotalMilliseconds + 1);
        }

        /*
        Point OldLocation;
        private void MyForm_Move(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Normal)
            {
                var Location = this.Location;
                var dx = Location.X - this.OldLocation.X;
                var dy = Location.Y - this.OldLocation.Y;
                var ManagerLocation = this.managerForm.Location;
                this.managerForm.Location = new Point(ManagerLocation.X + dx, ManagerLocation.Y + dy);
                this.OldLocation = Location;
            }
        }
        */
    }
}
