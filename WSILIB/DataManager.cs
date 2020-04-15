using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dicom.Imaging;
using Dicom;


namespace WSILIB
{
    static class  DataManager
    {
       internal static Frame SelectFrameByOffset(int x, int y,int w, int h,int c)
        {
            var tmp = Sqlhelper.Exesql($"SELECT * From Frame,Image WHERE " +
                $"ColumnPositionInTotalImagePixelMatrix<={x} and ColumnPositionInTotalImagePixelMatrix>{x - w}" +
                $"and RowPositionInTotalImagePixelMatrix<={y} and RowPositionInTotalImagePixelMatrix>{y - h}" +
                $"and Frame.SOPInstanceUID = (SELECT Image.SOPInstanceUID WHERE Image.Level = {c}) ");
            if(tmp.Rows.Count!=0)
            {
                var SOPInstanceUID = tmp.Rows[0]["SOPInstanceUID"].ToString();
                var ColumnPositionInTotalImagePixelMatrix = int.Parse(tmp.Rows[0]["ColumnPositionInTotalImagePixelMatrix"].ToString());
                var RowPositionInTotalImagePixelMatrix = int.Parse(tmp.Rows[0]["RowPositionInTotalImagePixelMatrix"].ToString());
                var FrameIndex =int.Parse(tmp.Rows[0]["FrameIndex"].ToString());
                Frame frame = new Frame(SOPInstanceUID, RowPositionInTotalImagePixelMatrix, ColumnPositionInTotalImagePixelMatrix, FrameIndex);
                return frame;
            }
            return null;
        }

        internal static String[] GetSOPInstanceUIDsInLevel(int level)
        {
            
            var table = Sqlhelper.Exesql($"SELECT SOPInstanceUID FROM Image WHERE Level = {level}");
            String[] ans = new String[table.Rows.Count];
            for (int i = 0; i<table.Rows.Count;i++)
            {
                ans[i] = table.Rows[i][0].ToString();
            }
            return ans;
        }

        internal static T GetSpecialImageInformationBySOPINstancUID<T>(ImageInformation intype, string SOPInstanceUID) where T:struct
        {
            if(intype == ImageInformation.SOPInstanceUID)
            {
                if(typeof(T)==typeof(String))
                {
                    return (T)Convert.ChangeType(SOPInstanceUID, typeof(T));
                }
                throw new NotSupportedException();
            }
            else
            {
                if(typeof(T) == typeof(int))
                {
                  int ans = int.Parse( Sqlhelper.Exesql($"SELECT {intype.ToString("G")} FROM Image WHERE SOPInstanceUID = \'{SOPInstanceUID}\'").Rows[0][0].ToString());
                  return (T)Convert.ChangeType(ans, typeof(T));
                }
                throw new NotSupportedException();
            }
        }

        internal static async Task<IImage> GetLocalImageAsyn(string sOPInstanceUID, int frameIndex)
        {
            var tmp = Sqlhelper.Exesql($"Select * FROM Local_Frame WHERE SOPInstanceUID=\'{sOPInstanceUID}\' and FrameIndex = {frameIndex}");
            if (tmp.Rows.Count!=0)
            {
                String FilePath = tmp.Rows[0]["LocalFilePath"].ToString();
                //var df = DicomFile.Open(@"D:\ProjectHub\WSILIB\WSILIB\bin\Debug\1.dcm");
                int LocalFrameIndex = int.Parse(tmp.Rows[0]["LocalFrameIndex"].ToString());
                var dcmFile = DicomFile.Open(FilePath);
                var ans = new DicomImage(dcmFile.Dataset);
                var my = ans.RenderImage(LocalFrameIndex);
                return my;
            }
            else
            {
                NetWork netWork = new NetWork();
                await netWork.SaveTargetFramesAsyn(sOPInstanceUID, frameIndex);
                return null;
            }
        }


        internal static void AddFrame(string SOPInstanceUID, int RowPositionInTotalImagePixelMatrix, int ColumnPositionInTotalImagePixelMatrix, int i)
        {
            Sqlhelper.ExecuteNonQuery($"INSERT INTO Frame(SOPInstanceUID,RowPositionInTotalImagePixelMatrix,ColumnPositionInTotalImagePixelMatrix,FrameIndex) VALUES(\'{SOPInstanceUID}\',{RowPositionInTotalImagePixelMatrix},{ColumnPositionInTotalImagePixelMatrix},{i})");

        }

        internal static void AddImage(string SOPInstanceUID, int Rows, int Columns, int TotalPixelMatrixColumns, int TotalPixelMatrixRows,int Level)
        {
            Sqlhelper.ExecuteNonQuery($"INSERT INTO Image(SOPInstanceUID,Rows,Columns,TotalPixelMatrixColumns,TotalPixelMatrixRows,Level) Values(\'{SOPInstanceUID}\',{Rows},{Columns},{TotalPixelMatrixColumns},{TotalPixelMatrixRows},{Level})");            
        }

        internal static void InitLevelInImage()
        {
            int MinSize = int.Parse(Sqlhelper.Exesql("SELECT MIN(TotalPixelMatrixRows)FROM Image").Rows[0][0].ToString());
            int MaxSize = int.Parse(Sqlhelper.Exesql("SELECT MAX(TotalPixelMatrixRows) FROM  Image").Rows[0][0].ToString());
            int c = int.Parse(Sqlhelper.Exesql("SELECT COUNT(DISTINCT TotalPixelMatrixRows)FROM Image").Rows[0][0].ToString());
            while ((MaxSize+1)>=MinSize)
            {                
                Sqlhelper.ExecuteNonQuery($"UPDATE Image SET Level = {c--} WHERE TotalPixelMatrixRows = {MaxSize+1}");
                MaxSize = MaxSize/2;
            }
        }

        internal static void SetLocalFile(string oldSOPInstancUID, int v, string path, int i)
        {
            Sqlhelper.ExecuteNonQuery($"INSERT INTO Local_Frame(SOPInstanceUID,FrameIndex,LocalFilePath,LocalFrameIndex) Values(\'{oldSOPInstancUID}\',{v},\'{path}\',{i})");
        }
        internal static void RegisterAllimage(String Path)
        {
            var response = DicomFile.Open(Path);
            var SOPInstanceUID = response.Dataset.GetSingleValue<String>(DicomTag.SOPInstanceUID);
                    int RowPositionInTotalImagePixelMatrix;
                    int ColumnPositionInTotalImagePixelMatrix;
                    int TotalPixelMatrixRows;
                    int TotalPixelMatrixColumns;
                    int Rows;
                    int Columns;
                    TotalPixelMatrixRows = response.Dataset.GetSingleValue<int>(DicomTag.TotalPixelMatrixRows);
                    TotalPixelMatrixColumns = response.Dataset.GetSingleValue<int>(DicomTag.TotalPixelMatrixColumns);
                    Rows = response.Dataset.GetSingleValue<int>(DicomTag.Rows);
                    Columns = response.Dataset.GetSingleValue<int>(DicomTag.Columns);
                    var Level = 0;
                    DicomSequence PerFrameFunctionalGroupsSequence = response.Dataset.GetSequence(DicomTag.PerFrameFunctionalGroupsSequence);
                    if (PerFrameFunctionalGroupsSequence != null)
                    {
                        var PerFrameFunctionalGroupsItems = PerFrameFunctionalGroupsSequence.GetEnumerator();
                        var FrameIndex = 0;
                        while (PerFrameFunctionalGroupsItems.MoveNext())
                        {
                            var PlanePositionSlideSequence = PerFrameFunctionalGroupsItems.Current.GetSequence(DicomTag.PlanePositionSlideSequence);

                            ColumnPositionInTotalImagePixelMatrix = PlanePositionSlideSequence.Items[0].GetSingleValue<int>(DicomTag.ColumnPositionInTotalImagePixelMatrix);
                            RowPositionInTotalImagePixelMatrix = PlanePositionSlideSequence.Items[0].GetSingleValue<int>(DicomTag.RowPositionInTotalImagePixelMatrix);
                            DataManager.AddFrame(SOPInstanceUID, RowPositionInTotalImagePixelMatrix, ColumnPositionInTotalImagePixelMatrix, FrameIndex++);

                        }
                    }
                    DataManager.AddImage(SOPInstanceUID, Rows, Columns, TotalPixelMatrixColumns, TotalPixelMatrixRows, Level);
                }


        
    }
}
