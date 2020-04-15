
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dicom;
using Dicom.Network;
using System.IO;

namespace WSILIB
{
    class NetWork
    {
        private string StoragePath = @".\DICOM";
        private string QRServerHost = "127.0.0.1"; // "www.dicomserver.co.uk";
        private int QRServerPort = 26104; // 104;
        private string QRServerAET = "WSIServer"; // "STORESCP";
        private string AET = "FODICOMSCU";
        DicomClient client;
        DicomCFindRequest request;

        internal NetWork()
        {
            client = new DicomClient();
            client.NegotiateAsyncOps();
        }
        /// <summary>
        /// Get WSI Headers From NetWork,Send a CFind
        /// </summary>
        /// <param name="serieUid">SeriesUID ,also a WSI Classify</param>
        internal void RegisterAllimage(string serieUid)
        {
            request = CreateImageRequestBySeriesUID(serieUid);
            request.OnResponseReceived += (req, response) =>
            {
                if (response.Status == DicomStatus.Pending)
                {
                    var SOPInstanceUID = response.Dataset?.GetSingleValue<string>(DicomTag.SOPInstanceUID);
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
                if (response.Status == DicomStatus.Success)
                {
                    DataManager.InitLevelInImage();
                }

            };

            client.AddRequest(request);
            client.SendAsync(QRServerHost, QRServerPort, false, AET, QRServerAET).Wait();

        }
        /// <summary>
        /// Send CGet to Get Target Frame
        /// </summary>
        /// <param name="SOPInstanceUID">the SOPInstanceUID Where Frame in</param>
        /// <param name="FrameIndex"> the FrameIndex Where Frame in</param>
        /// <returns></returns>
        internal async Task SaveTargetFramesAsyn(String SOPInstanceUID, int FrameIndex)
        {
            client = new DicomClient();
            var cGetRequest = CreateCGetRquest_FramesByList(SOPInstanceUID,FrameIndex);

            cGetRequest.Dataset.AddOrUpdate(new DicomTag(0x8, 0x5), "ISO_IR 100");
            cGetRequest.Dataset.AddOrUpdate(new DicomSequence(DicomTag.PerFrameFunctionalGroupsSequence));
            var FrameExtractionSequence = new DicomSequence(DicomTag.FrameExtractionSequence);
            cGetRequest.Dataset.AddOrUpdate(DicomTag.SOPInstanceUID, SOPInstanceUID);

            cGetRequest.Dataset.AddOrUpdate(DicomTag.QueryRetrieveLevel, DicomQueryRetrieveLevel.NotApplicable);

            var pcs = DicomPresentationContext.GetScpRolePresentationContextsFromStorageUids(
                        DicomStorageCategory.Image,
                        DicomTransferSyntax.ExplicitVRLittleEndian,
                        DicomTransferSyntax.ImplicitVRLittleEndian,
                        DicomTransferSyntax.ImplicitVRBigEndian,
                        DicomTransferSyntax.JPEGLSLossless,
                        DicomTransferSyntax.JPEG2000Lossless,
                        DicomTransferSyntax.JPEGProcess14SV1,
                        DicomTransferSyntax.JPEGProcess14,
                        DicomTransferSyntax.RLELossless,
                        DicomTransferSyntax.JPEGLSNearLossless,
                        DicomTransferSyntax.JPEG2000Lossy,
                        DicomTransferSyntax.JPEGProcess1,
                        DicomTransferSyntax.JPEGProcess2_4
                    );
            client.AdditionalPresentationContexts.AddRange(pcs);
            client.OnCStoreRequest += (DicomCStoreRequest req) =>
            {
                SaveImage(req.Dataset);

                return new DicomCStoreResponse(req, DicomStatus.Success);
            };
            client.AddRequest(cGetRequest);
            client.Send(QRServerHost, QRServerPort, false, AET, QRServerAET);

        }


        internal DicomCGetRequest CreateCGetRquest_FramesByList(String SOPInstanceUID, int FrameIndex)
        {
            DicomDataset command = new DicomDataset();
            command.AddOrUpdate(DicomTag.CommandField, (ushort)DicomCommandField.CGetRequest);
            command.Add(DicomTag.AffectedSOPClassUID, DicomUID.CompositeInstanceRootRetrieveGET);
            command.AddOrUpdate(DicomTag.Priority, (ushort)DicomPriority.Medium);
            command.AddOrUpdate(DicomTag.MessageID, (ushort)1);
            command.AddOrUpdate(DicomTag.CommandDataSetType, (ushort)0x0202);
            command.AddOrUpdate(DicomTag.SimpleFrameList, FrameIndex.ToUInt32Array());
            command.AddOrUpdate(DicomTag.SOPInstanceUID, SOPInstanceUID);
            var cGetRequest = new DicomCGetRequest(command);
            cGetRequest.Dataset = new DicomDataset();
            cGetRequest.Dataset.Add(DicomTag.AffectedSOPClassUID, DicomUID.CompositeInstanceRootRetrieveGET);
            return cGetRequest;
        }
        internal void SaveImage(DicomDataset dataset)
        {
            //var studyUid = dataset.GetSingleValue<string>(DicomTag.StudyInstanceUID);
            var instUid = dataset.GetSingleValue<string>(DicomTag.SOPInstanceUID);

            var path = Path.GetFullPath(StoragePath);
            //path = Path.Combine(path, studyUid);

            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            path = Path.Combine(path, instUid) + ".dcm";

            new DicomFile(dataset).Save(path);
            var FrameExtractionSequence = dataset.GetSequence(DicomTag.FrameExtractionSequence);
            var items = FrameExtractionSequence.GetEnumerator();
            while (items.MoveNext())
            {
                var oldSOPInstancUID = items.Current.GetSingleValue<String>(DicomTag.SOPInstanceUID);
                var Framelist = items.Current.GetValues<uint>(DicomTag.SimpleFrameList);
                for (int i = 0; i < Framelist.Length; i++)
                {
                    DataManager.SetLocalFile(oldSOPInstancUID, (int)Framelist[i], path, i);
                }
            }

        }

        internal DicomCFindRequest CreateImageRequestBySeriesUID(string seriesUID)
        {
            var request = new DicomCFindRequest(DicomQueryRetrieveLevel.Image);

            request.Dataset.AddOrUpdate(new DicomTag(0x8, 0x5), "ISO_IR 100");

            // add the dicom tags with empty values that should be included in the result
            request.Dataset.AddOrUpdate(DicomTag.SOPInstanceUID, "");
            request.Dataset.AddOrUpdate(new DicomSequence(DicomTag.PerFrameFunctionalGroupsSequence));

            request.Dataset.AddOrUpdate(DicomTag.TotalPixelMatrixColumns, "");
            request.Dataset.AddOrUpdate(DicomTag.TotalPixelMatrixRows, "");
            request.Dataset.AddOrUpdate(DicomTag.Rows, "");

            request.Dataset.AddOrUpdate(DicomTag.Columns, "");
            request.Dataset.AddOrUpdate(DicomTag.NumberOfFrames, "");

            // add the dicom tags that contain the filter criterias
            request.Dataset.AddOrUpdate(DicomTag.SeriesInstanceUID, seriesUID);
            //request.Command.AddOrUpdate(DicomTag.AffectedSOPClassUID,DicomUID.CompositeInstanceRetrieveWithoutBulkDataGET);

            return request;
        }
    }
}



