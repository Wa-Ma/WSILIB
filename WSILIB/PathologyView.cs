using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dicom;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Dicom.Imaging;
using System.Drawing;

namespace WSILIB
{
    /// <summary>
    /// Show the Whole Slide Image
    /// </summary>
    class PathologyView
    {
        List<Frame> RequestList = new List<Frame>();
        int PreAreaWidth;
        int PreAreaHeight;
        int start_x;
        int start_y;
        int SingleFrameHeight;
        int SingleFrameWidth;
        int MaxFramesHeightCount;
        int MaxFramesWidthCount;
        int CurrentLevel;
        int Rito = 2;

        internal PathologyView(int start_x, int start_y, int PreAreaWidth, int PreAreaHeight, int CurrentLevel)
        {
            this.start_x = start_x;
            this.start_y = start_y;
            this.PreAreaHeight = PreAreaHeight;
            this.PreAreaWidth = PreAreaWidth;
            this.CurrentLevel = CurrentLevel;
            String tmp_sopinstanceuid = DataManager.GetSOPInstanceUIDsInLevel(CurrentLevel)[0];
            SingleFrameHeight = DataManager.GetSpecialImageInformationBySOPINstancUID<int>(ImageInformation.Rows, tmp_sopinstanceuid);
            SingleFrameWidth = DataManager.GetSpecialImageInformationBySOPINstancUID<int>(ImageInformation.Columns, tmp_sopinstanceuid);
            MaxFramesHeightCount = PreAreaHeight / SingleFrameHeight + 1;
            MaxFramesWidthCount = PreAreaWidth / SingleFrameWidth + 1;
        }
        /// <summary>
        /// Get an Seted Image
        /// </summary>
        /// <returns></returns>
        internal async Task<Bitmap> GetImageAsyn()
        {
            SetRequestList();
            if(RequestList.Count==0)
            {
                return null;
            }
            var x_edge = RequestList[0].ColumnPositionInTotalImagePixelMatrix;
            var y_edge = RequestList[0].RowPositionInTotalImagePixelMatrix;
            Mat panorama = await ExtractDownSampleAsyn();
            if (panorama == null) return null;
            else
            {
                int relative_x = (int)(start_x  - x_edge);
                int relative_y = start_y  - y_edge;
                int w = PreAreaWidth < panorama.Width - relative_x ? PreAreaWidth : panorama.Width - relative_x;
                int h = PreAreaHeight < panorama.Height - relative_y ? PreAreaHeight : panorama.Height - relative_y;
                if (w < 1 || h < 1) return null;
                Mat ImageROI = new Mat();
                try
                {
                    Rect roi = new Rect(relative_x, relative_y, w, h);
                    ImageROI = new Mat(panorama, roi);
                }
                
                catch
                {
                    return null;
                }
                panorama.Dispose();
                var result = BitmapConverter.ToBitmap(ImageROI);
                ImageROI.Dispose();
                return result;
            }
        }
        /// <summary>
        /// Change the image Position
        /// </summary>
        /// <param name="start_x">the changed X Position</param>
        /// <param name="start_y">the changed Y Position</param>
        internal void ChagePostionState(int start_x, int start_y)
        {
            this.start_x = start_x;
            this.start_y = start_y;
        }

        /// <summary>
        /// Get Extraace Down Sample
        /// </summary>
        /// <returns></returns>
        private async Task<Mat> ExtractDownSampleAsyn()
        {
            if (RequestList.Count==0) return null;
            await EnsureAllCaches();
            //Mat tmp_m = Cache.GetMat(RequestList[2]);
            //return tmp_m;
            var x_buff = RequestList[0].ColumnPositionInTotalImagePixelMatrix;
            var y_buff = RequestList[0].RowPositionInTotalImagePixelMatrix;

            List<Mat> HMatList = new List<Mat>();
            List<Mat> VMatList = new List<Mat>();
            for (int i = 0; i < RequestList.Count; i++)
            {
                int y = RequestList[i].RowPositionInTotalImagePixelMatrix;
                Mat tmp_mat = Cache.GetMat(RequestList[i]);

                if (y == y_buff)
                {
                    if (tmp_mat != null)
                        HMatList.Add(tmp_mat);
                }
                else
                {
                    if(HMatList.Count!=0)
                    {
                        Mat ttt = new Mat();
                        Cv2.HConcat(HMatList.ToArray(), ttt);
                        HMatList.Clear();
                        HMatList.Add(tmp_mat);
                        VMatList.Add(ttt);
                    }
                   
                }
                y_buff = y;
            }
            Mat vmat = new Mat();
            if (HMatList.Count!=0)
            {
                Cv2.HConcat(HMatList.ToArray(), vmat);
                HMatList.Clear();
                VMatList.Add(vmat);
            }
            if (VMatList.Count != 0)
            {
                Mat Pano = new Mat();
                Cv2.VConcat(VMatList.ToArray(), Pano);
                return Pano;
            }
            else return null;
        }
        /// <summary>
        /// Ensure all Cache in Disk
        /// </summary>
        /// <returns></returns>
        private async Task  EnsureAllCaches()
        {
            int c = 0;
            while (true)
            {
                if (c == RequestList.Count) break;
                for(int i = 0;i<RequestList.Count;i++)
                {
                    if(Cache.ContainsKey(RequestList[i]))++c;
                    else
                    {
                        var x = RequestList[i].ColumnPositionInTotalImagePixelMatrix;
                        var y = RequestList[i].RowPositionInTotalImagePixelMatrix;
                        var SOPInstanceUID = RequestList[i].SOPInstanceUID;
                        var FrameIndex = RequestList[i].FrameIndex;
                        IImage iimage = await DataManager.GetLocalImageAsyn(SOPInstanceUID, FrameIndex);
                        if(iimage!=null)
                        {
                            var bmp = iimage.AsClonedBitmap();
                            var tmp_mat = BitmapConverter.ToMat(bmp);
                            var frame = new Frame(SOPInstanceUID, y, x, FrameIndex);
                            Cache.AddCache(frame, tmp_mat);
                            ++c;
                        }
                    }
                }
               await Task.Delay(TimeSpan.FromMilliseconds(100));
            }
        }
        /// <summary>
        /// Chang the WSI Level
        /// </summary>
        /// <param name="currentLevel"></param>
        internal void ChangeLevel(int currentLevel)
        {
            CurrentLevel = currentLevel;
        }


        /// <summary>
        /// Set Request List Which the Level and Location is ensured.
        /// </summary>
        private void SetRequestList()
        {
            RequestList.Clear();
            int relative_x = start_x;
            int relative_y = start_y;
            for (int i = 0; i < MaxFramesHeightCount; i++)
            {
                int x_buff = relative_x;
                for (int j = 0; j < MaxFramesWidthCount; j++)
                {
                    Frame frame = DataManager.SelectFrameByOffset(relative_x, relative_y, SingleFrameWidth, SingleFrameHeight, CurrentLevel);
                    if (frame == null) break;
                    RequestList.Add(frame);
                    relative_x += SingleFrameWidth;
                }
                relative_x = x_buff;
                relative_y += SingleFrameHeight;
            }
        }
    }
}
