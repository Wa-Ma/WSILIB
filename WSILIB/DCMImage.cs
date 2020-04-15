/// <summary>
/// 储存图像结构
/// </summary>
namespace WSILIB
{
    /// <summary>
    /// DCMImage is not due to the Read Local File,It's just a Litlte Class For Request.
    /// </summary>
    internal class DCMImage
    {

        public string SOPInstanceUID {
            private set;
            get;
            }
        
        public int TotalPixelMatrixColumns
        {
        private set;
        get;
        }

        public int TotalPixelMatrixRows
        {     
            private set;
            get;
        } 
        public int Level
        {
            private set;
            get;
        }
        public int Rows
        {
            private set;
            get;
        }
        public int Columns
        {
            private set;
            get;
        }
        public DCMImage(string SOPInstanceUID, int TotalPixelMatrixColumns,int TotalPixelMatrixRows,int level)
        {
            this.SOPInstanceUID = SOPInstanceUID;
            this.TotalPixelMatrixColumns = TotalPixelMatrixColumns;
            this.TotalPixelMatrixRows = TotalPixelMatrixRows;
            Level = level;
        }
        
    }
    enum ImageInformation
    {
        SOPInstanceUID,
        TotalPixelMatrixColumns,
        TotalPixelMatrixRows,
        Rows,
        Columns,
        Level
    }
}