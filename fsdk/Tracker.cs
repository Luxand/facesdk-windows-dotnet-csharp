using System;

namespace Luxand
{
    /// <summary>
    /// Represents a face tracker.
    /// </summary>
    public class Tracker : IDisposable
    {
        private int handle = -1;
        private bool disposed = false;

        /// <summary>
        /// Gets the native tracker handle.
        /// </summary>
        public int Handle => handle;

        /// <summary>
        /// Creates a new tracker instance.
        /// </summary>
        public Tracker()
        {
            int res = FSDK.CreateTracker(out handle);
            if (res != FSDK.FSDKE_OK)
                throw new Exception($"FSDK.CreateTracker failed: {res}");
        }

        /// <summary>
        /// Releases tracker resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (handle >= 0)
                {
                    FSDK.FreeTracker(handle);
                    handle = -1;
                }
                disposed = true;
            }
        }
        ~Tracker()
        {
            Dispose(false);
        }

        /// <summary>
        /// Clears all data from the tracker.
        /// </summary>
        public void Clear() => FSDK.ClearTracker(handle);

        /// <summary>
        /// Sets a tracker parameter by name.
        /// </summary>
        public int SetParameter(string name, string value) => FSDK.SetTrackerParameter(handle, name, value);

        /// <summary>
        /// Sets multiple tracker parameters from a string.
        /// </summary>
        public int SetMultipleParameters(string parameters, out int errorPosition) => FSDK.SetTrackerMultipleParameters(handle, parameters, out errorPosition);

        /// <summary>
        /// Gets a tracker parameter value by name.
        /// </summary>
        public string GetParameter(string name, long maxSize = 1024)
        {
            FSDK.CheckForError(FSDK.GetTrackerParameter(handle, name, out var value, maxSize));
            return value;
        }

        /// <summary>
        /// Feeds a frame to the tracker and returns detected face IDs.
        /// </summary>
        public void FeedFrame(long cameraIdx, int image, out long faceCount, out long[] ids, long maxSize = 1024)
        {
            FSDK.CheckForError(FSDK.FeedFrame(handle, cameraIdx, image, out faceCount, out ids, maxSize));
        }

        /// <summary>
        /// Feeds a frame to the tracker and returns detected face IDs.
        /// </summary>
        public void FeedFrame(CImage image, out long[] facesIds)
        {
            FSDK.CheckForError(FSDK.FeedFrame(handle, 0, image.ImageHandle, out var faceCount, out var ids, 1024));
            if (faceCount == 0) {
                facesIds = null;
                return;
            }
            facesIds = ids;
            Array.Resize(ref facesIds, (int)faceCount);
        }

        /// <summary>
        /// Gets the eye coordinates for a tracked face.
        /// </summary>
        public FSDK.TPoint[] GetEyes(long cameraIdx, long id)
        {
            FSDK.CheckForError(FSDK.GetTrackerEyes(handle, cameraIdx, id, out var features));
            return features;
        }

        /// <summary>
        /// Gets the facial features for a tracked face.
        /// </summary>
        public FSDK.TPoint[] GetFacialFeatures(long cameraIdx, long id)
        {
            FSDK.CheckForError(FSDK.GetTrackerFacialFeatures(handle, cameraIdx, id, out var features));
            return features;
        }

        /// <summary>
        /// Gets the face position for a tracked face.
        /// </summary>
        public FSDK.TFacePosition GetFacePosition(long cameraIdx, long id)
        {
            FSDK.CheckForError(FSDK.GetTrackerFacePosition(handle, cameraIdx, id, out var pos));
            return pos;
        }

        /// <summary>
        /// Gets the full face structure for a tracked face.
        /// </summary>
        public FSDK.TFace GetFace(long cameraIdx, long id)
        {
            FSDK.CheckForError(FSDK.GetTrackerFace(handle, cameraIdx, id, out var face));
            return face;
        }

        /// <summary>
        /// Locks a tracked face ID.
        /// </summary>
        public void LockID(long id) => FSDK.CheckForError(FSDK.LockID(handle, id));

        /// <summary>
        /// Unlocks a tracked face ID.
        /// </summary>
        public void UnlockID(long id) => FSDK.CheckForError(FSDK.UnlockID(handle, id));

        /// <summary>
        /// Gets the name assigned to a tracked face ID.
        /// </summary>
        public string GetName(long id, long maxSize = 1024)
        {
            FSDK.CheckForError(FSDK.GetName(handle, id, out var name, maxSize));
            return name;
        }

        /// <summary>
        /// Gets all names assigned to a tracked face ID.
        /// </summary>
        public string GetAllNames(long id, long maxSize = 1024)
        {
            FSDK.CheckForError(FSDK.GetAllNames(handle, id, out var names, maxSize));
            return names;
        }

        /// <summary>
        /// Sets the name for a tracked face ID.
        /// </summary>
        public void SetName(long id, string name) => FSDK.CheckForError(FSDK.SetName(handle, id, name));

        /// <summary>
        /// Removes a tracked face ID from the tracker.
        /// </summary>
        public void PurgeID(long id) => FSDK.CheckForError(FSDK.PurgeID(handle, id));

        /// <summary>
        /// Gets the reassigned ID for a merged or updated face.
        /// </summary>
        public long GetIDReassignment(long id)
        {
            FSDK.CheckForError(FSDK.GetIDReassignment(handle, id, out var reassigned));
            return reassigned;
        }

        /// <summary>
        /// Gets the number of similar IDs for a tracked face.
        /// </summary>
        public long GetSimilarIDCount(long id)
        {
            FSDK.CheckForError(FSDK.GetSimilarIDCount(handle, id, out var count));
            return count;
        }

        /// <summary>
        /// Gets a list of similar IDs for a tracked face.
        /// </summary>
        public long[] GetSimilarIDList(long id, long maxSize = 1024)
        {
            FSDK.CheckForError(FSDK.GetSimilarIDList(handle, id, out var list, maxSize));
            return list;
        }

        /// <summary>
        /// Saves the tracker state to a file.
        /// </summary>
        public void SaveMemoryToFile(string fileName) => FSDK.CheckForError(FSDK.SaveTrackerMemoryToFile(handle, fileName));

        /// <summary>
        /// Loads a tracker from a file.
        /// </summary>
        public static Tracker LoadMemoryFromFile(string fileName)
        {
            FSDK.CheckForError(FSDK.LoadTrackerMemoryFromFile(out var tracker, fileName));
            return new Tracker(tracker);
        }

        private Tracker(int handle)
        {
            this.handle = handle;
        }

        /// <summary>
        /// Gets the size of the memory buffer required to save the tracker.
        /// </summary>
        public long GetMemoryBufferSize()
        {
            FSDK.CheckForError(FSDK.GetTrackerMemoryBufferSize(handle, out var size));
            return size;
        }

        /// <summary>
        /// Saves the tracker state to a memory buffer.
        /// </summary>
        public byte[] SaveMemoryToBuffer()
        {
            FSDK.CheckForError(FSDK.SaveTrackerMemoryToBuffer(handle, out var buffer));
            return buffer;
        }

        /// <summary>
        /// Loads a tracker from a memory buffer.
        /// </summary>
        public static Tracker LoadMemoryFromBuffer(byte[] buffer)
        {
            FSDK.CheckForError(FSDK.LoadTrackerMemoryFromBuffer(out var tracker, buffer));
            return new Tracker(tracker);
        }

        /// <summary>
        /// Gets the value of a facial attribute for a tracked face in the tracker.
        /// </summary>
        public string GetFacialAttribute(long faceId, string attributeName, out string attributeValues)
        {
            FSDK.CheckForError(FSDK.GetTrackerFacialAttribute(handle, 0, faceId, attributeName, out attributeValues, 1024));
            return attributeValues;
        }

        /// <summary>
        /// Gets the number of IDs in the tracker.
        /// </summary>
        public long GetIDsCount()
        {
            long count = 0;
            FSDK.CheckForError(FSDK.GetTrackerIDsCount(handle, out count));
            return count;
        }

        /// <summary>
        /// Gets all IDs in the tracker.
        /// </summary>
        public long[] GetAllIDs(long maxSizeInBytes = 1024)
        {
            FSDK.CheckForError(FSDK.GetTrackerAllIDs(handle, out var idList, maxSizeInBytes));
            return idList;
        }

        /// <summary>
        /// Gets the number of face IDs for a given ID.
        /// </summary>
        public long GetFaceIDsCountForID(long id)
        {
            long count = 0;
            FSDK.CheckForError(FSDK.GetTrackerFaceIDsCountForID(handle, id, out count));
            return count;
        }

        /// <summary>
        /// Gets all face IDs for a given ID.
        /// </summary>
        public long[] GetFaceIDsForID(long id, long maxSizeInBytes = 1024)
        {
            FSDK.CheckForError(FSDK.GetTrackerFaceIDsForID(handle, id, out var faceIdList, maxSizeInBytes));
            return faceIdList;
        }

        /// <summary>
        /// Gets the ID by face ID.
        /// </summary>
        public long GetIDByFaceID(long faceId)
        {
            long id = 0;
            FSDK.CheckForError(FSDK.GetTrackerIDByFaceID(handle, faceId, out id));
            return id;
        }

        /// <summary>
        /// Gets the face template for a face ID.
        /// </summary>
        public byte[] GetFaceTemplate(long faceId)
        {
            FSDK.CheckForError(FSDK.GetTrackerFaceTemplate(handle, faceId, out var faceTemplate));
            return faceTemplate;
        }

        /// <summary>
        /// Creates a new ID from a face template.
        /// </summary>
        public int CreateID(ref byte[] faceTemplate, out long id, out long faceId)
        {
            var res = FSDK.TrackerCreateID(handle, ref faceTemplate, out id, out faceId);
            FSDK.CheckForError(res);
            return res;
        }

        /// <summary>
        /// Adds a face template to an ID.
        /// </summary>
        public int AddFaceTemplate(long id, ref byte[] faceTemplate, out long faceId)
        {
            var res = FSDK.AddTrackerFaceTemplate(handle, id, ref faceTemplate, out faceId);
            FSDK.CheckForError(res);
            return res;
        }

        /// <summary>
        /// Deletes a face by face ID.
        /// </summary>
        public int DeleteFace(long faceId)
        {
            var res = FSDK.DeleteTrackerFace(handle, faceId);
            FSDK.CheckForError(res);
            return res;
        }

        /// <summary>
        /// Gets the image for a face as a CImage.
        /// </summary>
        public CImage GetFaceImage(long faceId)
        {
            int imgHandle = 0;
            var res = FSDK.GetTrackerFaceImage(handle, faceId, out imgHandle);
            FSDK.CheckForError(res);
            return imgHandle > 0 ? new CImage(imgHandle) : null;
        }

        /// <summary>
        /// Sets the image for a face from a CImage.
        /// </summary>
        public int SetFaceImage(long faceId, CImage image)
        {
            var res = FSDK.SetTrackerFaceImage(handle, faceId, image?.ImageHandle ?? 0);
            FSDK.CheckForError(res);
            return res;
        }

        /// <summary>
        /// Deletes the image for a face.
        /// </summary>
        public int DeleteFaceImage(long faceId)
        {
            var res = FSDK.DeleteTrackerFaceImage(handle, faceId);
            FSDK.CheckForError(res);
            return res;
        }

        /// <summary>
        /// Matches a face template against faces in the tracker.
        /// </summary>
        public FSDK.IDSimilarity[] MatchFaces(ref byte[] faceTemplate, float threshold, long maxSizeInBytes = 1024)
        {
            FSDK.CheckForError(FSDK.TrackerMatchFaces(handle, ref faceTemplate, threshold, out var buffer, maxSizeInBytes));
            return buffer;
        }
    }
}
