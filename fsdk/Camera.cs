using System;
using System.Runtime.InteropServices;

namespace Luxand
{
    /// <summary>
    /// Provides access to video cameras using FaceSDK. Supports camera enumeration, opening, frame grabbing, and resource management.
    /// </summary>
    public class Camera : IDisposable
    {
        private int camHandle = -1;
        private bool disposed = false;

        /// <summary>
        /// Supported video compression types.
        /// </summary>
        public enum VideoCompressionType
        {
            MJPEG = 0
        }

        // --- Static methods for camera enumeration и configuration ---
        /// <summary>
        /// Initializes camera capturing system-wide. Call before using any camera functions.
        /// </summary>
        public static void InitializeCapturing() => FSDK.CheckForError(FSDK.InitializeCapturing());

        /// <summary>
        /// Finalizes camera capturing system-wide. Call after all camera operations are done.
        /// </summary>
        public static void FinalizeCapturing() => FSDK.CheckForError(FSDK.FinalizeCapturing());

        /// <summary>
        /// Gets the list of available camera names.
        /// </summary>
        public static string[] GetCameraList() => FSDK.GetCameraList();

        /// <summary>
        /// Sets the retrieval format for the GetCameraList function. 
        /// Depending on the value of the argument, either web camera names (by default) or their unique IDs (Device Path) are returned. 
        /// Device Path may be necessary if the system has several web cameras from the same manufacturer that have the same name. 
        /// This function does not support IP cameras.
        /// </summary>
        public static void SetCameraNaming(bool UseDevicePathAsName) => FSDK.CheckForError(FSDK.SetCameraNaming(UseDevicePathAsName));

        /// <summary>
        /// Gets the list of supported video formats for the specified camera.
        /// </summary>
        /// <param name="CameraName">The name of the camera.</param>
        /// <returns>An array of <see cref="VideoFormatInfo"/> structures describing supported formats.</returns>
        public static FSDK.VideoFormatInfo[] GetVideoFormatList(string CameraName)
        {
            return FSDK.GetVideoFormatList(CameraName);
        }   

        /// <summary>
        /// Sets the video format for the specified camera.
        /// </summary>
        /// <param name="cameraName">The name of the camera.</param>
        /// <param name="videoFormat">The video format to set.</param>
        /// <returns>FSDKE_OK on success or an error code on failure.</returns>
        public static int SetVideoFormat(string cameraName, FSDK.VideoFormatInfo videoFormat)
        {
            int res = FSDK.SetVideoFormat(cameraName, videoFormat);
            FSDK.CheckForError(res);
            return res;
        }

        // --- Instance methods for camera operation ---

        /// <summary>
        /// Opens a camera by name.
        /// </summary>
        public Camera(string cameraName)
        {
            FSDK.CheckForError(FSDK.OpenVideoCamera(cameraName, out camHandle));
        }
        /// <summary>
        /// Opens a camera by name and sets the video format.
        /// </summary>
        public Camera(string cameraName, FSDK.VideoFormatInfo videoFormat)
        {
            FSDK.CheckForError(FSDK.SetVideoFormat(cameraName, videoFormat));
            FSDK.CheckForError(FSDK.OpenVideoCamera(cameraName, out camHandle));
        }
        /// <summary>
        /// Opens an IP camera with the specified parameters.
        /// </summary>
        public Camera(VideoCompressionType compressionType, string url, string username, string password, int timeoutSeconds)
        {
            FSDK.CheckForError(FSDK.OpenIPVideoCamera(compressionType, url, username, password, timeoutSeconds, out camHandle));
        }

        /// <summary>
        /// Grabs a frame from the camera.
        /// </summary>
        public CImage GrabFrame()
        {
            FSDK.CheckForError(FSDK.GrabFrame(camHandle, out var himage));
            return new CImage(himage);
        }

        /// <summary>
        /// Closes the camera and releases resources.
        /// </summary>
        public void Close()
        {             
            if (camHandle >= 0)
            {
                FSDK.CheckForError(FSDK.CloseVideoCamera(camHandle));
                camHandle = -1;
            }
        }

        /// <summary>
        /// Releases the camera resource.
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
                Close();
                disposed = true;
            }
        }
        ~Camera()
        {
            Dispose(false);
        }
    }
}
