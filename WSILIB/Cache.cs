using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace WSILIB
{
    class Cache
    {
        private static Dictionary<Frame, Mat> CachList = new Dictionary<Frame, Mat>();
        Frame FrameInstance;
        Mat mat;
        internal static void AddCache(Frame key, Mat mat)
        {
            if (CachList.ContainsKey(key)) return;
            CachList.Add(key, mat);
        }
        internal static bool ContainsKey(Frame key)
        {

            if (CachList.ContainsKey(key)) return true;
            return false;
        }
        internal static Mat GetMat(Frame key)
        {
            if (Cache.ContainsKey(key)) return CachList[key];
            return null;
        }
        internal static void DeleteTheLevelCache()
        {
            CachList.Clear();
        }
        internal static void Clear()
        {
            CachList.Clear();
        }
    }


}
