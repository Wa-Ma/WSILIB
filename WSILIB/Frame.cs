using System;

namespace WSILIB
{
    internal class Frame
    {
        public String SOPInstanceUID
        {
            private set;
            get;
        }
        public int RowPositionInTotalImagePixelMatrix
        {
            private set;
            get;
        }
        public int ColumnPositionInTotalImagePixelMatrix
        {
            private set;
            get;
        }
        public int FrameIndex
        {
            private set;
            get;
        }
        public Frame(string SOPInstanceUID, int RowPositionInTotalImagePixelMatrix, int ColumnPositionInTotalImagePixelMatrix, int FrameIndex)
        {
            this.FrameIndex = FrameIndex;
            this.SOPInstanceUID = SOPInstanceUID;
            this.RowPositionInTotalImagePixelMatrix = RowPositionInTotalImagePixelMatrix;
            this.ColumnPositionInTotalImagePixelMatrix = ColumnPositionInTotalImagePixelMatrix;
        }
        public override bool Equals(object obj)
        {
            var tmp = obj as Frame;
            return tmp.FrameIndex == FrameIndex && tmp.SOPInstanceUID == SOPInstanceUID;
        }

        public override int GetHashCode()
        {
            int FrameIndex_hashcode = FrameIndex.GetHashCode();
            int SOPInstanceUID_hashcode = SOPInstanceUID.GetHashCode();
            return FrameIndex_hashcode + SOPInstanceUID_hashcode;
        }
    }
    public static class intExtenSions
    {
        public static uint[] ToUInt32Array(this int i)
        {
            uint[] re = new uint[1];
            re[0] = (uint)i;
            return re;
        }
    }
}