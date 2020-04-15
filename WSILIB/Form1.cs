using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Dicom;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Dicom.Imaging;
using Dicom.Network;


namespace WSILIB
{
    public partial class Form1 : Form
    {
        int start_x;
        int start_y;
        bool PreAreaSelected;
        int mouse_start_x;
        int mouse_start_y;
        int CurrentLevel;
        PathologyView pathologyView;
        public Form1()
        {
            InitializeComponent();
        }

        private async void MYLoad(object sender, EventArgs e)
        {
            start_x = 1;
            start_y = 1;
            CurrentLevel = 1;
            NetWork netWork = new NetWork();
            //netWork.RegisterAllimage("1.2.276.0.7230010.3.1.3.504891108.15516.1555333626.529");
            netWork.RegisterAllimage("1.2.276.0.7230010.3.1.3.296485376.1.1484917366.62819");
            DataManager.InitLevelInImage();
            pathologyView = new PathologyView(start_x, start_y, pictureBox1.Width, pictureBox1.Height, CurrentLevel);
            await Task.Delay(3000);
            pictureBox1.Image = await pathologyView.GetImageAsyn();
            //var DicomDa = DicomFile.Open(@"D:\WSILIB\WSILIB\bin\Debug\DICOM\R2.dcm").Dataset;
            //var image = new DicomImage(DicomDa).RenderImage(3).AsClonedBitmap();
            //pictureBox1.Image = image;
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            PreAreaSelected = true;
            mouse_start_x = e.X;
            mouse_start_y = e.Y;
        }
        /// <summary>
        /// drag the picture in picturebox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (PreAreaSelected)
            {
                SetStart_X(start_x -(e.X - mouse_start_x));
                SetStart_Y(start_y - (e.Y - mouse_start_y));
                pathologyView.ChagePostionState(start_x,start_y);
                Bitmap bmp = await pathologyView.GetImageAsyn();
                if (bmp != null)
                {
                    pictureBox1.Image = bmp;
                }
                mouse_start_x = e.X;
                mouse_start_y = e.Y;
            }
            await Task.Delay(TimeSpan.FromMilliseconds(300));

        }
        /// <summary>
        /// the end of the picture-moving
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            PreAreaSelected = false;
        }

        int flag = 0;
        private async void pictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            if(flag ==0)
            {
                flag++;
            }
            else
            {
                int relative_focus_x = e.X;
                int relative_focus_y = e.Y;
                int absolute_focus_x = start_x + relative_focus_x;
                int absolute_focus_y = start_y + relative_focus_y;
                var Rito = 2;
                if (e.Delta == 120)
                {
                    CurrentLevel++;
                    SetStart_X(absolute_focus_x * Rito - relative_focus_x);
                    SetStart_Y(absolute_focus_y * Rito - relative_focus_y);
                }

                else if (CurrentLevel > 1 && e.Delta == -120)
                {
                    CurrentLevel--;

                    SetStart_X(absolute_focus_x / Rito - relative_focus_x);
                    SetStart_Y(absolute_focus_y / Rito - relative_focus_y);
                }
                else
                {
                    CurrentLevel = 1;
                }

                pathologyView.ChangeLevel(CurrentLevel);
                pathologyView.ChagePostionState(start_x,start_y);
                Bitmap bmp  = await pathologyView.GetImageAsyn();
                pictureBox1.Image = bmp;
                flag--;
            }
        }

        private int GetStart_X()
        {
            return start_x;
        }
        private int GetStart_Y()
        {
            return start_y;
        }
        private void SetStart_X(int value)
        {
            start_x = value > 0 ? value : 1;
        }
        private void SetStart_Y(int value)
        {
            start_y = value > 0 ? value : 1;
        }

    }
}
