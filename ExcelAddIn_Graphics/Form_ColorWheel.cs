﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Runtime.InteropServices;


using Excel = Microsoft.Office.Interop.Excel;
using Office = Microsoft.Office.Core;
using Microsoft.Office.Tools.Excel;

using Microsoft.Office.Tools.Ribbon;

using range = Microsoft.Office.Interop.Excel.Range;
using worksheet = Microsoft.Office.Tools.Excel.Worksheet;


namespace ExcelAddIn_Graphics
{
    public partial class Form_ColorWheel : Form
    {
        public Point formLoad, formLeft, formRight;
        Timer tm;
        public int flag;
        public int[] SelectRGB;
        public Form_ColorWheel()
        {
            InitializeComponent();
            Rectangle rect = Screen.PrimaryScreen.WorkingArea;
            formLeft = new Point(0, rect.Height - Height);
            formRight = new Point(rect.Width - Width, rect.Height - Height);
        }

        [DllImport("gdi32")]
        private static extern IntPtr CreateDC(
       string lpszDriver, // 驱动名称
       string lpszDevice, // 设备名称
       string lpszOutput, // 无用，可以设定位"NULL"
       IntPtr lpInitData // 任意的打印机数据
      );
        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(
        IntPtr hdcDest, // 目标设备的句柄
        int nXDest, // 目标对象的左上角的X坐标
        int nYDest, // 目标对象的左上角的X坐标
        int nWidth, // 目标对象的矩形的宽度
        int nHeight, // 目标对象的矩形的长度
        IntPtr hdcSrc, // 源设备的句柄
        int nXSrc, // 源对象的左上角的X坐标
        int nYSrc, // 源对象的左上角的X坐标
        int dwRop // 光栅的操作值
        );

        private void tabControl_ColorWheel_Selected(Object sender, TabControlEventArgs e)
        {
            if (tabControl_ColorWheel.SelectedIndex==2)
            {
                Close();

                string item = "Color_wheel.pdf";
                string path = System.AppDomain.CurrentDomain.BaseDirectory + "/" + item ;
                System.Diagnostics.Process.Start(path);        
            }
            
        }

        void elementChart_MouseDown(int Button, int Shift, int x, int y)
        {
            if (flag == 1)
            {
                Excel.Chart chart = Globals.ThisAddIn.Application.ActiveChart;

                //Int32 ;
                Int32 elementID = 0;
                Int32 arg1 = 0;
                Int32 arg2 = 0;

                chart.GetChartElement(x, y, ref elementID, ref arg1, ref arg2);

                //

                string element = ((Excel.XlChartItem)elementID).ToString();
                if (element == "xlSeries")
                {
                    Excel.SeriesCollection series = (Excel.SeriesCollection)chart.SeriesCollection();
                    Excel.Series Sseries = series.Item(arg1);
                    Sseries.Format.Fill.BackColor.RGB = System.Drawing.Color.FromArgb(SelectRGB[2], SelectRGB[1], SelectRGB[0]).ToArgb();
                    Sseries.Format.Fill.ForeColor.RGB = System.Drawing.Color.FromArgb(SelectRGB[2], SelectRGB[1], SelectRGB[0]).ToArgb();
                    flag = 0;
                }
            }

            // MessageBox.Show("Chart element is: " + ((Excel.XlChartItem)elementID).ToString()
            //    + "\n arg1 is: " + arg1.ToString() + "\n arg2 is: " + arg2.ToString());

        }

        private void btnGetColor_Click(object sender, EventArgs e)
        {
            flag = 1;
            //formLoad = this.Location;
            //this.Location = formLeft;
            //this.TopMost = true;

            tm = new Timer();
            tm.Interval = 1;
            tm.Tick += new EventHandler(tm_Tick);
            tm.Enabled = true;

            Form_ColorWheel_GetColor getColors = new Form_ColorWheel_GetColor();
            getColors.Tag = this;
            getColors.ShowDialog();

            tm.Enabled = false;
            //this.Location = formLoad;
            //this.TopMost = false;

            txtARGB.SelectAll();

            Excel.Chart chart = Globals.ThisAddIn.Application.ActiveChart;
            chart.MouseDown += new Excel.ChartEvents_MouseDownEventHandler(elementChart_MouseDown);

        }

        //private void Form_ColorWheel_MouseMove(object sender, MouseEventArgs e)
        //{
        //    Form_ColorWheel Mymainform = (Form_ColorWheel)this.Tag;

        //    if (e.X > Mymainform.Left && e.X < Mymainform.Left + Mymainform.Width && e.Y > Mymainform.Top && e.Y < Mymainform.Top + Mymainform.Width)
        //    {
        //        Mymainform.Location = Mymainform.Location == Mymainform.formLeft ? Mymainform.formRight : Mymainform.formLeft;
        //    }
        //}

        void tm_Tick(object sender, EventArgs e)
        {

            //Screen screen = Screen.PrimaryScreen;
            //Rectangle rc = screen.Bounds;
            //int iWidth = rc.Width;
            //int iHeight = rc.Height;
            //Image myImage = new Bitmap(iWidth, iHeight);
            ////从一个继承自Image类的对象中创建Graphics对象
            //Graphics g = Graphics.FromImage(myImage);
            //g.CopyFromScreen(new Point(0, 0), new Point(0, 0), new Size(iWidth, iHeight));

            IntPtr hdlDisplay = CreateDC("display", null, null, IntPtr.Zero);

            System.Drawing.Graphics g = System.Drawing.Graphics.FromHdc(hdlDisplay);

            Bitmap bmp = new Bitmap(1, 1, g);

            System.Drawing.Graphics gimg = System.Drawing.Graphics.FromImage(bmp);

            IntPtr hdlScreen = g.GetHdc();
            IntPtr hdlBmp = gimg.GetHdc();

            BitBlt(hdlBmp, 0, 0, 1, 1, hdlScreen, MousePosition.X, MousePosition.Y, 13369376);

            g.ReleaseHdc(hdlScreen);
            gimg.ReleaseHdc(hdlBmp);

            picColor.BackColor = bmp.GetPixel(0, 0);

            SelectRGB = new int[3];
            SelectRGB[0] = picColor.BackColor.R;
            SelectRGB[1] = picColor.BackColor.G;
            SelectRGB[2] = picColor.BackColor.B;
            //label9.Text = Convert.ToString(MousePosition.X);
            //label7.Text = Convert.ToString(MousePosition.Y);
            txtARGB.Text = "0x" + picColor.BackColor.ToArgb().ToString("x").ToUpper();
            txtRGB.Text = picColor.BackColor.R.ToString().ToLower() + "," + picColor.BackColor.G.ToString().ToLower() + "," + picColor.BackColor.B.ToString().ToLower();
            //txtL.Text = "#" + picColor.BackColor.R.ToString("x").PadLeft(2, '0') + picColor.BackColor.G.ToString("x").PadLeft(2, '0') + picColor.BackColor.B.ToString("x").PadLeft(2, '0');

            g.Dispose();
            gimg.Dispose();

            bmp.Dispose(); // 释放 bmp 所使用的资源

            /*属于第一个版本的程序代码
            Point p = new Point(MousePosition.X, MousePosition.Y);
            lblX.Text = p.X.ToString();
            lblY.Text = p.Y.ToString();

            IntPtr h = GetDC(new IntPtr(0)); //取设备场景，0代表着全屏

            uint color = GetPixel(h, p);          //去颜色
            uint red = (color & 0xFF);            //转换R
            uint green = (color & 0xFF00) / 256;    //转换G
            uint blue = (color & 0xFF0000) / 65536;//转换B

            txtRGB.Text = string.Format("{0},{1},{2}", red, green, blue);
            txtT.Text = color.ToString();
            txtL.Text = "#" + red.ToString("x").PadLeft(2, '0') + green.ToString("x").PadLeft(2, '0') + blue.ToString("x").PadLeft(2, '0');

            picColor.BackColor = Color.FromArgb((int)red,(int)green,(int)blue);
       */
        }
    }
}
