using System;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Luxand
{
    /// <summary>
    /// Static class providing P/Invoke wrappers and constants for the native FaceSDK library.
    /// </summary>
    public unsafe partial class FSDK
    {
#if IOS
        private const string Dll = "__Internal";
#else
        private const string Dll = "facesdk";
#endif

        // TYPES AND CONSTANTS
        /// <summary>
        /// Size of the face template (version 1).
        /// </summary>
        public const int TemplateSize = 1040;
        /// <summary>
        /// Size of the face template (version 2).
        /// </summary>
        public const int TemplateSize2 = 2068;
        /// <summary>
        /// Size of the TFacePosition struct in bytes.
        /// </summary>
        public const int sizeofTFacePosition = 24;
        /// <summary>
        /// Size of the TFace struct in bytes.
        /// </summary>
        public const int sizeofTFace = 56;

        // Error codes
        /// <summary>Success.</summary>
        public const int FSDKE_OK = 0;
        /// <summary>General failure.</summary>
        public const int FSDKE_FAILED = -1;
        /// <summary>Library not activated.</summary>
        public const int FSDKE_NOT_ACTIVATED = -2;
        /// <summary>Out of memory.</summary>
        public const int FSDKE_OUT_OF_MEMORY = -3;
        /// <summary>Invalid argument.</summary>
        public const int FSDKE_INVALID_ARGUMENT = -4;
        /// <summary>I/O error.</summary>
        public const int FSDKE_IO_ERROR = -5;
        /// <summary>Image too small.</summary>
        public const int FSDKE_IMAGE_TOO_SMALL = -6;
        /// <summary>Face not found.</summary>
        public const int FSDKE_FACE_NOT_FOUND = -7;
        /// <summary>Insufficient buffer size.</summary>
        public const int FSDKE_INSUFFICIENT_BUFFER_SIZE = -8;
        /// <summary>Unsupported image extension.</summary>
        public const int FSDKE_UNSUPPORTED_IMAGE_EXTENSION = -9;
        /// <summary>Cannot open file.</summary>
        public const int FSDKE_CANNOT_OPEN_FILE = -10;
        /// <summary>Cannot create file.</summary>
        public const int FSDKE_CANNOT_CREATE_FILE = -11;
        /// <summary>Bad file format.</summary>
        public const int FSDKE_BAD_FILE_FORMAT = -12;
        /// <summary>File not found.</summary>
        public const int FSDKE_FILE_NOT_FOUND = -13;
        /// <summary>Connection closed.</summary>
        public const int FSDKE_CONNECTION_CLOSED = -14;
        /// <summary>Connection failed.</summary>
        public const int FSDKE_CONNECTION_FAILED = -15;
        /// <summary>IP camera initialization failed.</summary>
        public const int FSDKE_IP_INIT_FAILED = -16;
        /// <summary>Server activation required.</summary>
        public const int FSDKE_NEED_SERVER_ACTIVATION = -17;
        /// <summary>ID not found.</summary>
        public const int FSDKE_ID_NOT_FOUND = -18;
        /// <summary>Attribute not detected.</summary>
        public const int FSDKE_ATTRIBUTE_NOT_DETECTED = -19;
        /// <summary>Insufficient tracker memory limit.</summary>
        public const int FSDKE_INSUFFICIENT_TRACKER_MEMORY_LIMIT = -20;
        /// <summary>Unknown attribute.</summary>
        public const int FSDKE_UNKNOWN_ATTRIBUTE = -21;
        /// <summary>Unsupported file version.</summary>
        public const int FSDKE_UNSUPPORTED_FILE_VERSION = -22;
        /// <summary>Syntax error.</summary>
        public const int FSDKE_SYNTAX_ERROR = -23;
        /// <summary>Parameter not found.</summary>
        public const int FSDKE_PARAMETER_NOT_FOUND = -24;
        /// <summary>Invalid template.</summary>
        public const int FSDKE_INVALID_TEMPLATE = -25;
        /// <summary>Unsupported template version.</summary>
        public const int FSDKE_UNSUPPORTED_TEMPLATE_VERSION = -26;
        /// <summary>Camera index does not exist.</summary>
        public const int FSDKE_CAMERA_INDEX_DOES_NOT_EXIST = -27;
        /// <summary>Platform not licensed.</summary>
        public const int FSDKE_PLATFORM_NOT_LICENSED = -28;
        /// <summary> TensorFlow runtime is not initialized.</summary>
        public const int FSDKE_TENSORFLOW_NOT_INITIALIZED = -29;
        /// <summary> The required plugin is not loaded.</summary>
        public const int FSDKE_PLUGIN_NOT_LOADED = -30;
        /// <summary> No permission to use the plugin.</summary>
        public const int FSDKE_PLUGIN_NO_PERMISSION = -31;
        /// <summary> Face ID not found.</summary>
        public const int FSDKE_FACEID_NOT_FOUND = -32;
        /// <summary> Face image not found.</summary>
        public const int FSDKE_FACEIMAGE_NOT_FOUND = -33;

        /// <summary>iBeta initialization failed.</summary>
        public const int FSDKE_IBETA_INITIALIZATION_FAILED = -200;

        /// <summary>
        /// The number of facial feature points detected by the FaceSDK engine.
        /// </summary>
        public const int FSDK_FACIAL_FEATURE_COUNT = 70;

        /// <summary>
        /// Specifies the supported image pixel formats for FaceSDK image operations.
        /// </summary>
        public enum FSDK_IMAGEMODE
        {
            /// <summary>
            /// 8-bit grayscale image format.
            /// </summary>
            FSDK_IMAGE_GRAYSCALE_8BIT,
            /// <summary>
            /// 24-bit color image format (RGB).
            /// </summary>
            FSDK_IMAGE_COLOR_24BIT,
            /// <summary>
            /// 32-bit color image format (ARGB).
            /// </summary>
            FSDK_IMAGE_COLOR_32BIT
        };

        /// <summary>
        /// Represents a point (x, y) in image coordinates.
        /// </summary>
        public struct TPoint
        {
            public int x, y;
        }

        /// <summary>
        /// Represents the position and rotation of a detected face in the image.
        /// </summary>
        public struct TFacePosition
        {
            public int xc, yc, w;
            public int padding;
            public double angle;
        }

        /// <summary>
        /// Represents a detected face, including bounding box and facial features.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct TFace
        {
            /// <summary>
            /// Bounding box of the face.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct BBox
            {
                public TPoint p0, p1;
            }

            /// <summary>
            /// Face bounding box
            /// </summary>
            public BBox bbox;

            /// <summary>
            /// Array of facial feature points (typically 5 points).
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public TPoint[] features;
            
            /// <summary>
            /// The center of the face
            /// </summary>
            public TPoint center()
            {
                return new TPoint
                {
                    x = (bbox.p0.x + bbox.p1.x) / 2,
                    y = (bbox.p0.y + bbox.p1.y) / 2
                };
            }

            /// <summary>
            /// Face width
            /// </summary>
            public int width()
            {
                return bbox.p1.x - bbox.p0.x;
            }
            /// <summary>
            /// Face height
            /// </summary>
            public int height()
            {
                return bbox.p1.y - bbox.p0.y;
            }

            /// <summary>
            /// Left edge of the face bounding box.
            /// </summary>
            public int left()
            {
                return bbox.p0.x;
            }

            /// <summary>
            /// Right edge of the face bounding box.
            /// </summary>
            public int right()
            {
                return bbox.p1.x;
            }
            /// <summary>
            /// Top edge of the face bounding box.
            /// </summary>
            public int top()
            {
                return bbox.p0.y;
            }

            /// <summary>
            /// Bottom edge of the face bounding box.
            /// </summary>
            public int bottom()
            {
                return bbox.p1.y;
            }

            /// <summary>
            /// Check if the face bounding box is empty (either width or height is zero).
            /// </summary>
            public bool empty()
            {
                return height() == 0 || width() == 0;
            }
        }

        /// <summary>
        /// Enumerates all supported facial feature points for landmark detection.
        /// </summary>
        public enum FacialFeatures {
            FSDKP_LEFT_EYE = 0,
            FSDKP_RIGHT_EYE = 1,
            FSDKP_LEFT_EYE_INNER_CORNER = 24,
            FSDKP_LEFT_EYE_OUTER_CORNER = 23,
            FSDKP_LEFT_EYE_LOWER_LINE1 = 38,
            FSDKP_LEFT_EYE_LOWER_LINE2 = 27,
            FSDKP_LEFT_EYE_LOWER_LINE3 = 37,
            FSDKP_LEFT_EYE_UPPER_LINE1 = 35,
            FSDKP_LEFT_EYE_UPPER_LINE2 = 28,
            FSDKP_LEFT_EYE_UPPER_LINE3 = 36,
            FSDKP_LEFT_EYE_LEFT_IRIS_CORNER = 29,
            FSDKP_LEFT_EYE_RIGHT_IRIS_CORNER = 30,
            FSDKP_RIGHT_EYE_INNER_CORNER = 25,
            FSDKP_RIGHT_EYE_OUTER_CORNER = 26,
            FSDKP_RIGHT_EYE_LOWER_LINE1 = 41,
            FSDKP_RIGHT_EYE_LOWER_LINE2 = 31,
            FSDKP_RIGHT_EYE_LOWER_LINE3 = 42,
            FSDKP_RIGHT_EYE_UPPER_LINE1 = 40,
            FSDKP_RIGHT_EYE_UPPER_LINE2 = 32,
            FSDKP_RIGHT_EYE_UPPER_LINE3 = 39,
            FSDKP_RIGHT_EYE_LEFT_IRIS_CORNER = 33,
            FSDKP_RIGHT_EYE_RIGHT_IRIS_CORNER = 34,
            FSDKP_LEFT_EYEBROW_INNER_CORNER = 13,
            FSDKP_LEFT_EYEBROW_MIDDLE = 16,
            FSDKP_LEFT_EYEBROW_MIDDLE_LEFT = 18,
            FSDKP_LEFT_EYEBROW_MIDDLE_RIGHT = 19,
            FSDKP_LEFT_EYEBROW_OUTER_CORNER = 12,
            FSDKP_RIGHT_EYEBROW_INNER_CORNER = 14,
            FSDKP_RIGHT_EYEBROW_MIDDLE = 17,
            FSDKP_RIGHT_EYEBROW_MIDDLE_LEFT = 20,
            FSDKP_RIGHT_EYEBROW_MIDDLE_RIGHT = 21,
            FSDKP_RIGHT_EYEBROW_OUTER_CORNER = 15,
            FSDKP_NOSE_TIP = 2,
            FSDKP_NOSE_BOTTOM = 49,
            FSDKP_NOSE_BRIDGE = 22,
            FSDKP_NOSE_LEFT_WING = 43,
            FSDKP_NOSE_LEFT_WING_OUTER = 45,
            FSDKP_NOSE_LEFT_WING_LOWER = 47,
            FSDKP_NOSE_RIGHT_WING = 44,
            FSDKP_NOSE_RIGHT_WING_OUTER = 46,
            FSDKP_NOSE_RIGHT_WING_LOWER = 48,
            FSDKP_MOUTH_RIGHT_CORNER = 3,
            FSDKP_MOUTH_LEFT_CORNER = 4,
            FSDKP_MOUTH_TOP = 54,
            FSDKP_MOUTH_TOP_INNER = 61,
            FSDKP_MOUTH_BOTTOM = 55,
            FSDKP_MOUTH_BOTTOM_INNER = 64,
            FSDKP_MOUTH_LEFT_TOP = 56,
            FSDKP_MOUTH_LEFT_TOP_INNER = 60,
            FSDKP_MOUTH_RIGHT_TOP = 57,
            FSDKP_MOUTH_RIGHT_TOP_INNER = 62,
            FSDKP_MOUTH_LEFT_BOTTOM = 58,
            FSDKP_MOUTH_LEFT_BOTTOM_INNER = 63,
            FSDKP_MOUTH_RIGHT_BOTTOM = 59,
            FSDKP_MOUTH_RIGHT_BOTTOM_INNER = 65,
            FSDKP_NASOLABIAL_FOLD_LEFT_UPPER = 50,
            FSDKP_NASOLABIAL_FOLD_LEFT_LOWER = 52,
            FSDKP_NASOLABIAL_FOLD_RIGHT_UPPER = 51,
            FSDKP_NASOLABIAL_FOLD_RIGHT_LOWER = 53,
            FSDKP_CHIN_BOTTOM = 11,
            FSDKP_CHIN_LEFT = 9,
            FSDKP_CHIN_RIGHT = 10,
            FSDKP_FACE_CONTOUR1 = 7,
            FSDKP_FACE_CONTOUR2 = 5,
            FSDKP_FACE_CONTOUR12 = 6,
            FSDKP_FACE_CONTOUR13 = 8,
            FSDKP_FACE_CONTOUR14 = 66,
            FSDKP_FACE_CONTOUR15 = 67,
            FSDKP_FACE_CONTOUR16 = 68,
            FSDKP_FACE_CONTOUR17 = 69
        }
        //}

        /// <summary>
        /// Describes a video format supported by the camera.
        /// </summary>
        public struct VideoFormatInfo
        {
            public int Width;
            public int Height;
            public int BPP;
        }

        /// <summary>
        /// Represents a similarity score between a face ID and a template.
        /// </summary>
        public struct IDSimilarity
        {
            public long ID;
            public float Similarity;
        }

        /// <summary>
        /// Throws an exception if the FaceSDK function returns an error code.
        /// </summary>
        /// <param name="hr">The error code returned by a FaceSDK function.</param>
        public static void CheckForError(int hr)
        {
            if (hr != FSDKE_OK)
                throw new Exception("Luxand FaceSDK Error Number " + hr.ToString());
        }


        //INITIALIZATION FUNCTIONS{
        [DllImport(Dll, EntryPoint = "FSDK_ActivateLibrary", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int ActivateLibraryInternal(string LicenseKey);
        public static int ActivateLibrary(string LicenseKey)
        {
            return ActivateLibraryInternal(LicenseKey);
        }
        
        [DllImport(Dll, EntryPoint = "FSDK_GetHardware_ID", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetHardware_IDInternal([OutAttribute] StringBuilder HardwareID);
        public static int GetHardware_ID(out string HardwareID)
        {
            var tmps = new StringBuilder(1024);
            int res = GetHardware_IDInternal(tmps);
            HardwareID = tmps.ToString();
            return res;
        }
        
        [DllImport(Dll, EntryPoint = "FSDK_GetLicenseInfo", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetLicenseInfoInternal([OutAttribute] StringBuilder LicenseInfo);
        public static int GetLicenseInfo(out string LicenseInfo)
        {
            StringBuilder tmps = new StringBuilder(1024);
            int res = GetLicenseInfoInternal(tmps);
            LicenseInfo = tmps.ToString();
            return res;
        }
        
        [DllImport(Dll, EntryPoint = "FSDK_Initialize", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int InitializeInternal(string DataFilesPath);
        public static int InitializeLibrary(){
            SetParameter("environment", ".NET");
            return InitializeInternal("");
        }

        [DllImport(Dll, EntryPoint = "FSDK_Finalize", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int FinalizeLibraryInternal();
        public static int FinalizeLibrary()
        {
            return FinalizeLibraryInternal();
        }

        [DllImport(Dll, EntryPoint = "FSDK_SetHTTPProxy", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SetHTTPProxyInternal(string ServerNameOrIPAddress, ushort Port, string UserName, string Password);
        public static int SetHTTPProxy(string ServerNameOrIPAddress, ushort Port, string UserName, string Password)
        {
            return SetHTTPProxyInternal(ServerNameOrIPAddress, Port, UserName, Password);
        }


        [DllImport(Dll, EntryPoint = "FSDK_SetNumThreads", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SetNumThreadsInternal(int Num);
        public static int SetNumThreads(int Num)
        {
            return SetNumThreadsInternal(Num);
        }

        [DllImport(Dll, EntryPoint = "FSDK_GetNumThreads", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetNumThreadsInternal(out int Num);
        public static int GetNumThreads(out int Num)
        {
            return GetNumThreadsInternal(out Num);
        }

        //}

        //FACE DETECTION FUNCTIONS{
        [DllImport(Dll, EntryPoint = "FSDK_DetectEyes", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int DetectEyesInternal(int Image, [Out, MarshalAs(UnmanagedType.LPArray, SizeConst = FSDK_FACIAL_FEATURE_COUNT)] TPoint[] FacialFeatures);
        public static int DetectEyes(int Image, out TPoint[] FacialFeatures)
        {
            FacialFeatures = new TPoint[FSDK_FACIAL_FEATURE_COUNT];
            return DetectEyesInternal(Image, FacialFeatures);
        }
        
        [DllImport(Dll, EntryPoint = "FSDK_DetectEyesInRegion", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int DetectEyesInRegionInternal(int Image, in TFacePosition FacePosition, [Out, MarshalAs(UnmanagedType.LPArray, SizeConst = FSDK_FACIAL_FEATURE_COUNT)] TPoint[] FacialFeatures);
        public static int DetectEyesInRegion(int Image, in TFacePosition FacePosition, out TPoint[] FacialFeatures){
            FacialFeatures = new TPoint[FSDK_FACIAL_FEATURE_COUNT];
            return DetectEyesInRegionInternal(Image, FacePosition, FacialFeatures);
        }
        
        [DllImport(Dll, EntryPoint = "FSDK_DetectFace", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int DetectFaceInternal(int Image, out TFacePosition FacePosition);
        public static int DetectFace(int Image, out TFacePosition facePosition)
        {
            return DetectFaceInternal(Image, out facePosition);
        }

        [DllImport(Dll, EntryPoint = "FSDK_DetectFace2", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int DetectFace2Internal(int Image, out TFace FacePosition);
        public static int DetectFace2(int Image, out TFace FacePosition)
        {
            return DetectFace2Internal(Image, out FacePosition);
        }

        [DllImport(Dll, EntryPoint = "FSDK_DetectMultipleFaces", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int DetectMultipleFacesInternal(int Image, out int DetectedCount, [Out, MarshalAs(UnmanagedType.LPArray)] TFacePosition[] FaceArray, int MaxSizeInBytes);
        public static int DetectMultipleFaces(int Image, out TFacePosition[] FaceArray, int MaxSizeInBytes)
        {
            var faceArray = new TFacePosition[MaxSizeInBytes / Marshal.SizeOf(typeof(TFacePosition))];
            var res = DetectMultipleFacesInternal(Image, out var DetectedCount, faceArray, MaxSizeInBytes);

            if (res != FSDKE_OK)
            {
                FaceArray = new TFacePosition[0];
                return res;
            }

            FaceArray = new TFacePosition[DetectedCount];
            Array.Copy(faceArray, FaceArray, DetectedCount);
            return res;
        }

        public static int DetectMultipleFaces(int Image, out int DetectedCount, out TFacePosition[] FaceArray, int MaxSizeInBytes)
        {
            var res = DetectMultipleFaces(Image, out FaceArray, MaxSizeInBytes);
            DetectedCount = FaceArray.Length;
            return res;
        }

        [DllImport(Dll, EntryPoint = "FSDK_DetectMultipleFaces2", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int DetectMultipleFaces2Internal(int Image, out int DetectedCount, [Out, MarshalAs(UnmanagedType.LPArray)] TFace[] FaceArray, int MaxSizeInBytes);
        public static int DetectMultipleFaces2(int Image, out TFace[] Faces, int MaxSize)
        {
            var faces = new TFace[MaxSize];
            var result = DetectMultipleFaces2Internal(Image, out var detectedCount, faces, MaxSize * Marshal.SizeOf(typeof(TFace)));

            if (result != FSDKE_OK)
            {
                Faces = new TFace[0];
                return result;
            }

            Faces = new TFace[detectedCount];
            Array.Copy(faces, Faces, detectedCount);
            return result;
        }

        public static int DetectMultipleFaces2(int Image, out int DetectedCount, out TFace[] Faces, int MaxSize)
        {
            var result = DetectMultipleFaces2(Image, out Faces, MaxSize);
            DetectedCount = Faces.Length;
            return result;
        }

        [DllImport(Dll, EntryPoint = "FSDK_DetectFacialFeatures", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int DetectFacialFeaturesInternal(int Image, [Out, MarshalAs(UnmanagedType.LPArray, SizeConst = FSDK_FACIAL_FEATURE_COUNT)] TPoint[] FacialFeatures);
        public static int DetectFacialFeatures(int Image, out TPoint[] FacialFeatures)
        {
            FacialFeatures = new TPoint[FSDK_FACIAL_FEATURE_COUNT];
            return DetectFacialFeaturesInternal(Image, FacialFeatures);
        }

        [DllImport(Dll, EntryPoint = "FSDK_DetectFacialFeaturesInRegion", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int DetectFacialFeaturesInRegionInternal(int Image, in TFacePosition FacePosition, [Out, MarshalAs(UnmanagedType.LPArray, SizeConst = FSDK_FACIAL_FEATURE_COUNT)] TPoint[] FacialFeatures);
        public static int DetectFacialFeaturesInRegion(int Image, in TFacePosition FacePosition, out TPoint[] FacialFeatures)
        {
            FacialFeatures = new TPoint[FSDK_FACIAL_FEATURE_COUNT];
            return DetectFacialFeaturesInRegionInternal(Image, FacePosition, FacialFeatures);
        }

        [DllImport(Dll, EntryPoint = "FSDK_SetFaceDetectionParameters", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SetFaceDetectionParametersInternal(bool HandleArbitraryRotations, bool DetermineFaceRotationAngle, int InternalResizeWidth);
        public static int SetFaceDetectionParameters(bool HandleArbitraryRotations, bool DetermineFaceRotationAngle, int InternalResizeWidth)
        {
            return SetFaceDetectionParametersInternal(HandleArbitraryRotations, DetermineFaceRotationAngle, InternalResizeWidth);
        }

        [DllImport(Dll, EntryPoint = "FSDK_SetFaceDetectionThreshold", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SetFaceDetectionThresholdInternal(int Threshold);
        private static int SetFaceDetectionThreshold(int Threshold)
        {
            return SetFaceDetectionThresholdInternal(Threshold);
        }
        //}

        //IMAGE MANIPULATION FUNCTIONS{
        [DllImport(Dll, EntryPoint = "FSDK_CreateEmptyImage", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int CreateEmptyImageInternal(out int Image);
        public static int CreateEmptyImage(out int Image)
        {
            return CreateEmptyImageInternal(out Image);
        }

        [DllImport(Dll, EntryPoint = "FSDK_LoadImageFromBuffer", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int LoadImageFromBufferInternal(out int Image, [In] byte[] Buffer, int Width, int Height, int ScanLine, FSDK_IMAGEMODE ImageMode);
        public static int LoadImageFromBuffer(out int Image, byte[] Buffer, int Width, int Height, int ScanLine, FSDK_IMAGEMODE ImageMode)
        {
            return LoadImageFromBufferInternal(out Image, Buffer, Width, Height, ScanLine, ImageMode);
        }


        [DllImport(Dll, EntryPoint = "FSDK_GetImageBufferSize", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetImageBufferSizeInternal(int Image, out int BufSize, FSDK_IMAGEMODE ImageMode);
        public static int GetImageBufferSize(int Image, out int BufSize, FSDK_IMAGEMODE ImageMode)
        {
            return GetImageBufferSizeInternal(Image, out BufSize, ImageMode);
        }

        [DllImport(Dll, EntryPoint = "FSDK_SaveImageToBuffer", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SaveImageToBufferInternal(int Image, [Out, MarshalAs(UnmanagedType.LPArray)] byte[] Buffer, FSDK_IMAGEMODE ImageMode);
        public static int SaveImageToBuffer(int Image, out byte[] Buffer, FSDK_IMAGEMODE ImageMode)
        {
            int res = GetImageBufferSize(Image, out var MaxSizeInBytes, ImageMode);
            if (res != FSDKE_OK){
                Buffer = null;
                return res;
            }

            Buffer = new byte[MaxSizeInBytes];
            return SaveImageToBufferInternal(Image, Buffer, ImageMode);
        }

        [DllImport(Dll, EntryPoint = "FSDK_LoadImageFromFile", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int LoadImageFromFileInternal(out int Image, string FileName);
        public static int LoadImageFromFile(out int Image, string FileName)
        {
            return LoadImageFromFileInternal(out Image, FileName);
        }

        [DllImport(Dll, EntryPoint = "FSDK_LoadImageFromFileW", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int LoadImageFromFileWInternal(out int Image, [In, MarshalAs(UnmanagedType.BStr)] string FileName);
        public static int LoadImageFromFileW(out int Image, string FileName) {
            return LoadImageFromFileWInternal(out Image, FileName); 
        }

        [DllImport(Dll, EntryPoint = "FSDK_FreeImage", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int FreeImageInternal(int Image);
        public static int FreeImage(int Image)
        {
            return FreeImageInternal(Image);
        }

        [DllImport(Dll, EntryPoint = "FSDK_SaveImageToFile", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SaveImageToFileInternal(int Image, string FileName);
        public static int SaveImageToFile(int Image, string FileName)
        {
            return SaveImageToFileInternal(Image, FileName);
        }

        [DllImport(Dll, EntryPoint = "FSDK_SaveImageToFileW", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SaveImageToFileWInternal(int Image, [In, MarshalAs(UnmanagedType.BStr)] string FileName);
        public static int SaveImageToFileW(int Image, string FileName)
        {
            return SaveImageToFileWInternal(Image, FileName);
        }

        [DllImport(Dll, EntryPoint = "FSDK_LoadImageFromHBitmap", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int LoadImageFromHBitmapInternal(out int Image, IntPtr BitmapHandle);
        public static int LoadImageFromHBitmap(out int Image, IntPtr BitmapHandle)
        {
            return LoadImageFromHBitmapInternal(out Image, BitmapHandle);
        }

        [DllImport(Dll, EntryPoint = "FSDK_SaveImageToHBitmap", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SaveImageToHBitmapInternal(int Image, out IntPtr BitmapHandle);
        public static int SaveImageToHBitmap(int Image, out IntPtr BitmapHandle)
        {
            return SaveImageToHBitmapInternal(Image, out BitmapHandle);
        }

        [DllImport("gdi32.dll")]
        static extern bool DeleteObject(IntPtr hObject);

#if USE_SYSTEM_DRAWING
        /// <summary>
        /// Loads an image from a System.Drawing.Image object into the FaceSDK image format.
        /// </summary>
        /// <param name="Image">Receives the handle to the created FaceSDK image.</param>
        /// <param name="ImageObject">The System.Drawing.Image object to load from.</param>
        /// <returns>FSDKE_OK on success or an error code on failure.</returns>
        public static int LoadImageFromCLRImage(out int Image, System.Drawing.Image ImageObject)
        {
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(ImageObject);
            IntPtr hbm = bmp.GetHbitmap();
            int res = LoadImageFromHBitmap(out Image, hbm);
            DeleteObject(hbm);
            bmp.Dispose();
            return res;
        }

        /// <summary>
        /// Saves a FaceSDK image to a System.Drawing.Image object.
        /// </summary>
        /// <param name="Image">The handle to the FaceSDK image to save.</param>
        /// <param name="ImageObject">Receives the resulting System.Drawing.Image object.</param>
        /// <returns>FSDKE_OK on success or an error code on failure.</returns>
        public static int SaveImageToCLRImage(int Image, out System.Drawing.Image ImageObject)
        {
            int res = SaveImageToHBitmap(Image, out var hbm);
            ImageObject = System.Drawing.Image.FromHbitmap(hbm);
            DeleteObject(hbm);
            return res;
        }
#endif

        [DllImport(Dll, EntryPoint = "FSDK_SetJpegCompressionQuality", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SetJpegCompressionQualityInternal(int Quality);
        public static int SetJpegCompressionQuality(int Quality)
        {
            return SetJpegCompressionQualityInternal(Quality);
        }

        [DllImport(Dll, EntryPoint = "FSDK_CopyImage", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int CopyImageInternal(int SourceImage, int DestImage);
        public static int CopyImage(int SourceImage, int DestImage)
        {
            return CopyImageInternal(SourceImage, DestImage);
        }

        [DllImport(Dll, EntryPoint = "FSDK_ResizeImage", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int ResizeImageInternal(int SourceImage, double ratio, int DestImage);
        public static int ResizeImage(int SourceImage, double ratio, int DestImage)
        {
            return ResizeImageInternal(SourceImage, ratio, DestImage);
        }

        [DllImport(Dll, EntryPoint = "FSDK_MirrorImage", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int MirrorImageInternal(int Image, bool UseVerticalMirroringInsteadOfHorizontal);
        public static int MirrorImage(int Image, bool UseVerticalMirroringInsteadOfHorizontal)
        {
            return MirrorImageInternal(Image, UseVerticalMirroringInsteadOfHorizontal);
        }

        [DllImport(Dll, EntryPoint = "FSDK_RotateImage", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int RotateImageInternal(int SourceImage, double angle, int DestImage);
        public static int RotateImage(int SourceImage, double angle, int DestImage)
        {
            return RotateImageInternal(SourceImage, angle, DestImage);
        }

        [DllImport(Dll, EntryPoint = "FSDK_RotateImageCenter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int RotateImageCenterInternal(int SourceImage, double angle, double xCenter, double yCenter, int DestImage);
        public static int RotateImageCenter(int SourceImage, double angle, double xCenter, double yCenter, int DestImage)
        {
            return RotateImageCenterInternal(SourceImage, angle, xCenter, yCenter, DestImage);
        }

        [DllImport(Dll, EntryPoint = "FSDK_RotateImage90", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int RotateImage90Internal(int SourceImage, int Multiplier, int DestImage);
        public static int RotateImage90(int SourceImage, int Multiplier, int DestImage)
        {
            return RotateImage90Internal(SourceImage, Multiplier, DestImage);
        }

        [DllImport(Dll, EntryPoint = "FSDK_CopyRect", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int CopyRectInternal(int SourceImage, int x1, int y1, int x2, int y2, int DestImage);
        public static int CopyRect(int SourceImage, int x1, int y1, int x2, int y2, int DestImage)
        {
            return CopyRectInternal(SourceImage, x1, y1, x2, y2, DestImage);
        }

        [DllImport(Dll, EntryPoint = "FSDK_CopyRectReplicateBorder", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int CopyRectReplicateBorderInternal(int SourceImage, int x1, int y1, int x2, int y2, int DestImage);
        public static int CopyRectReplicateBorder(int SourceImage, int x1, int y1, int x2, int y2, int DestImage)
        {
            return CopyRectReplicateBorderInternal(SourceImage, x1, y1, x2, y2, DestImage);
        }

        [DllImport(Dll, EntryPoint = "FSDK_GetImageWidth", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetImageWidthInternal(int SourceImage, out int Width);
        public static int GetImageWidth(int SourceImage, out int Width)
        {
            return GetImageWidthInternal(SourceImage, out Width);
        }

        [DllImport(Dll, EntryPoint = "FSDK_GetImageHeight", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetImageHeightInternal(int SourceImage, out int Height);
        public static int GetImageHeight(int SourceImage, out int Height)
        {
            return GetImageHeightInternal(SourceImage, out Height);
        }

        [DllImport(Dll, EntryPoint = "FSDK_ExtractFaceImage", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int ExtractFaceImageInternal(int Image, [In, MarshalAs(UnmanagedType.LPArray, SizeConst = FSDK_FACIAL_FEATURE_COUNT)] TPoint[] FacialFeatures, int Width, int Height, out int ExtractedFaceImage, [Out, MarshalAs(UnmanagedType.LPArray, SizeConst = FSDK_FACIAL_FEATURE_COUNT)] TPoint[] ResizedFeatures);
        public static int ExtractFaceImage(int Image, TPoint[] FacialFeatures, int Width, int Height, out int ExtractedFaceImage, out TPoint[] ResizedFeatures){
            ResizedFeatures = new TPoint[FSDK_FACIAL_FEATURE_COUNT];
            return ExtractFaceImageInternal(Image, FacialFeatures, Width, Height, out ExtractedFaceImage, ResizedFeatures);
        }
        //}

        //MATCHING{
        [DllImport(Dll, EntryPoint = "FSDK_GetFaceTemplate", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetFaceTemplateInternal(int Image, [In, Out, MarshalAs(UnmanagedType.LPArray, SizeConst = TemplateSize)] byte[] FaceTemplate);
        public static int GetFaceTemplate(int Image, out byte[] FaceTemplate)
        {
            FaceTemplate = new byte[TemplateSize];
            return GetFaceTemplateInternal(Image, FaceTemplate);
        }

        [DllImport(Dll, EntryPoint = "FSDK_GetFaceTemplateInRegion", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetFaceTemplateInRegionInteral(int Image, in TFacePosition FacePosition, [Out, MarshalAs(UnmanagedType.LPArray, SizeConst = TemplateSize)] byte[] FaceTemplate);
        public static int GetFaceTemplateInRegion(int Image, TFacePosition FacePosition, out byte[] FaceTemplate)
        {
            FaceTemplate = new byte[TemplateSize];
            return GetFaceTemplateInRegionInteral(Image, FacePosition, FaceTemplate);
        }

        [DllImport(Dll, EntryPoint = "FSDK_GetFaceTemplate2", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetFaceTemplate2Internal(int Image, [Out, MarshalAs(UnmanagedType.LPArray, SizeConst = TemplateSize2)] byte[] FaceTemplate);

        public static int GetFaceTemplate2(int Image, out byte[] FaceTemplate)
        {
            FaceTemplate = new byte[TemplateSize2];
            return GetFaceTemplate2Internal(Image, FaceTemplate);
        }

        [DllImport(Dll, EntryPoint = "FSDK_GetFaceTemplateInRegion2", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetFaceTemplateInRegion2Internal(int Image, in TFace Face, [Out, MarshalAs(UnmanagedType.LPArray, SizeConst = TemplateSize2)] byte[] FaceTemplate);
        public static int GetFaceTemplateInRegion2(int Image, TFace Face, out byte[] FaceTemplate)
        {
            FaceTemplate = new byte[TemplateSize2];
            return GetFaceTemplateInRegion2Internal(Image, Face, FaceTemplate);
        }

        [DllImport(Dll, EntryPoint = "FSDK_GetFaceTemplateUsingEyes", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetFaceTemplateUsingEyesInternal(int Image, [In, MarshalAs(UnmanagedType.LPArray)] TPoint[] eyeCoords, [Out, MarshalAs(UnmanagedType.LPArray, SizeConst = TemplateSize)] byte[] FaceTemplate);
        public static int GetFaceTemplateUsingEyes(int Image, TPoint[] eyeCoords, out byte[] FaceTemplate){
            FaceTemplate = new byte[TemplateSize];
            return GetFaceTemplateUsingEyesInternal(Image, eyeCoords, FaceTemplate);
        }
        
        [DllImport(Dll, EntryPoint = "FSDK_GetFaceTemplateUsingFeatures", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetFaceTemplateUsingFeaturesInternal(int Image, [In, MarshalAs(UnmanagedType.LPArray, SizeConst = FSDK_FACIAL_FEATURE_COUNT)] TPoint[] FacialFeatures, [Out, MarshalAs(UnmanagedType.LPArray, SizeConst = TemplateSize)] byte[] FaceTemplate);
        public static int GetFaceTemplateUsingFeatures(int Image, TPoint[] FacialFeatures, out byte[] FaceTemplate){
            FaceTemplate = new byte[TemplateSize];
            return GetFaceTemplateUsingFeaturesInternal(Image, FacialFeatures, FaceTemplate);
        }
        
        [DllImport(Dll, EntryPoint = "FSDK_MatchFaces", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int MatchFacesInternal([In, MarshalAs(UnmanagedType.LPArray)] byte[] FaceTemplate1, [In, MarshalAs(UnmanagedType.LPArray)] byte[] FaceTemplate2, out float Similarity);
        public static int MatchFaces(byte[] FaceTemplate1, byte[] FaceTemplate2, out float Similarity){
            return MatchFacesInternal(FaceTemplate1, FaceTemplate2, out Similarity);
        }
        
        [DllImport(Dll, EntryPoint = "FSDK_GetMatchingThresholdAtFAR", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetMatchingThresholdAtFARInternal(float FARValue, out float Threshold);
        public static int GetMatchingThresholdAtFAR(float FARValue, out float Threshold)
        {
            return GetMatchingThresholdAtFARInternal(FARValue, out Threshold);
        }

        [DllImport(Dll, EntryPoint = "FSDK_GetMatchingThresholdAtFRR", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetMatchingThresholdAtFRRInternal(float FRRValue, out float Threshold);
        public static int GetMatchingThresholdAtFRR(float FRRValue, out float Threshold)
        {
            return GetMatchingThresholdAtFRRInternal(FRRValue, out Threshold);      
        }
        //}

        //TRACKER{
        [DllImport(Dll, EntryPoint = "FSDK_CreateTracker", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int CreateTrackerInternal(out int Tracker);
        public static int CreateTracker(out int Tracker)
        {
            return CreateTrackerInternal(out Tracker);
        }

        [DllImport(Dll, EntryPoint = "FSDK_FreeTracker", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int FreeTrackerInternal(int Tracker);
        public static int FreeTracker(int Tracker)
        {
            return FreeTrackerInternal(Tracker);
        }

        [DllImport(Dll, EntryPoint = "FSDK_ClearTracker", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int ClearTrackerInternal(int Tracker);
        public static int ClearTracker(int Tracker)
        {
            return ClearTrackerInternal(Tracker);
        }

        [DllImport(Dll, EntryPoint = "FSDK_SetTrackerParameter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SetTrackerParameterInternal(int Tracker, string ParameterName, string ParameterValue);
        public static int SetTrackerParameter(int Tracker, string ParameterName, string ParameterValue)
        {
            return SetTrackerParameterInternal(Tracker, ParameterName, ParameterValue);
        }

        [DllImport(Dll, EntryPoint = "FSDK_SetTrackerMultipleParameters", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SetTrackerMultipleParametersInternal(int Tracker, string Parameters, out int ErrorPosition);
        public static int SetTrackerMultipleParameters(int Tracker, string Parameters, out int ErrorPosition)
        {
            return SetTrackerMultipleParametersInternal(Tracker, Parameters, out ErrorPosition);
        }

        [DllImport(Dll, EntryPoint = "FSDK_GetTrackerParameter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetTrackerParameterInternal(int Tracker, string ParameterName, [Out] StringBuilder ParameterValue, long MaxSizeInBytes);
        public static int GetTrackerParameter(int Tracker, string ParameterName, out string ParameterValue, long MaxSizeInBytes)
        {
            StringBuilder tmps = new StringBuilder((int)MaxSizeInBytes);
            int res = GetTrackerParameterInternal(Tracker, ParameterName, tmps, MaxSizeInBytes);
            ParameterValue = tmps.ToString();
            return res;
        }

        [DllImport(Dll, EntryPoint = "FSDK_FeedFrame", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int FeedFrameInternal(int Tracker, long CameraIdx, int Image, out long FaceCount, [Out, MarshalAs(UnmanagedType.LPArray)] long[] IDs, long MaxSizeInBytes);
        public static int FeedFrame(int Tracker, long CameraIdx, int Image, out long FaceCount, out long[] IDs, long MaxSizeInBytes)
        {
            IDs = new long[MaxSizeInBytes / sizeof(long)];
            return FeedFrameInternal(Tracker, CameraIdx, Image, out FaceCount, IDs, MaxSizeInBytes);
        }

        [DllImport(Dll, EntryPoint = "FSDK_GetTrackerEyes", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetTrackerEyesInternal(int Tracker, long CameraIdx, long ID, [Out, MarshalAs(UnmanagedType.LPArray, SizeConst = FSDK_FACIAL_FEATURE_COUNT)] TPoint[] FacialFeatures);
        public static int GetTrackerEyes(int Tracker, long CameraIdx, long ID, out TPoint[] FacialFeatures)
        {
            FacialFeatures = new TPoint[FSDK_FACIAL_FEATURE_COUNT];
            return GetTrackerEyesInternal(Tracker, CameraIdx, ID, FacialFeatures);
        }

        [DllImport(Dll, EntryPoint = "FSDK_GetTrackerFacialFeatures", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetTrackerFacialFeaturesInternal(int Tracker, long CameraIdx, long ID, [Out, MarshalAs(UnmanagedType.LPArray, SizeConst = FSDK_FACIAL_FEATURE_COUNT)] TPoint[] FacialFeatures);
        public static int GetTrackerFacialFeatures(int Tracker, long CameraIdx, long ID, out TPoint[] FacialFeatures)
        {
            FacialFeatures = new TPoint[FSDK_FACIAL_FEATURE_COUNT];
            return GetTrackerFacialFeaturesInternal(Tracker, CameraIdx, ID, FacialFeatures);
        }
        
        [DllImport(Dll, EntryPoint = "FSDK_GetTrackerFacePosition", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetTrackerFacePositionInternal(int Tracker, long CameraIdx, long ID, out TFacePosition FacePosition);
        public static int GetTrackerFacePosition(int Tracker, long CameraIdx, long ID, out TFacePosition FacePosition)
        {
            return GetTrackerFacePositionInternal(Tracker, CameraIdx, ID, out FacePosition);
        }

        [DllImport(Dll, EntryPoint = "FSDK_GetTrackerFace", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetTrackerFaceInternal(int Tracker, long CameraIdx, long ID, out TFace FacePosition);
        public static int GetTrackerFace(int Tracker, long CameraIdx, long ID, out TFace Face)
        {
            return GetTrackerFaceInternal(Tracker, CameraIdx, ID, out Face);
        }

        [DllImport(Dll, EntryPoint = "FSDK_LockID", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int LockIDInternal(int Tracker, long ID);
        public static int LockID(int Tracker, long ID)
        {
            return LockIDInternal(Tracker, ID);
        }

        [DllImport(Dll, EntryPoint = "FSDK_UnlockID", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int UnlockIDInternal(int Tracker, long ID);
        public static int UnlockID(int Tracker, long ID)
        {
            return UnlockIDInternal(Tracker, ID);
        }

        [DllImport(Dll, EntryPoint = "FSDK_GetName", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetNameInternal(int Tracker, long ID, [Out] StringBuilder Name, long MaxSizeInBytes);
        public static int GetName(int Tracker, long ID, out string Name, long MaxSizeInBytes)
        {
            StringBuilder tmps = new StringBuilder((int)MaxSizeInBytes);
            int res = GetNameInternal(Tracker, ID, tmps, MaxSizeInBytes);
            Name = tmps.ToString();
            return res;
        }

        [DllImport(Dll, EntryPoint = "FSDK_GetAllNames", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetAllNamesInternal(int Tracker, long ID, [Out] StringBuilder Names, long MaxSizeInBytes);
        public static int GetAllNames(int Tracker, long ID, out string Names, long MaxSizeInBytes)
        {
            StringBuilder tmps = new StringBuilder((int)MaxSizeInBytes);
            int res = GetAllNamesInternal(Tracker, ID, tmps, MaxSizeInBytes);
            Names = tmps.ToString();
            return res;
        }

        [DllImport(Dll, EntryPoint = "FSDK_SetName", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SetNameInternal(int Tracker, long ID, string Name);
        public static int SetName(int Tracker, long ID, string Name)
        {
            return SetNameInternal(Tracker, ID, Name);
        }

        [DllImport(Dll, EntryPoint = "FSDK_PurgeID", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int PurgeIDInternal(int Tracker, long ID);
        public static int PurgeID(int Tracker, long ID)
        {
            return PurgeIDInternal(Tracker, ID);
        }

        [DllImport(Dll, EntryPoint = "FSDK_GetIDReassignment", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetIDReassignmentInternal(int Tracker, long ID, out long ReassignedID);
        public static int GetIDReassignment(int Tracker, long ID, out long ReassignedID)
        {
            return GetIDReassignmentInternal(Tracker, ID, out ReassignedID);
        }

        [DllImport(Dll, EntryPoint = "FSDK_GetSimilarIDCount", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetSimilarIDCountInternal(int Tracker, long ID, out long Count);
        public static int GetSimilarIDCount(int Tracker, long ID, out long Count)
        {
            return GetSimilarIDCountInternal(Tracker, ID, out Count);
        }

        [DllImport(Dll, EntryPoint = "FSDK_GetSimilarIDList", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetSimilarIDListInternal(int Tracker, long ID, [Out, MarshalAs(UnmanagedType.LPArray)] long[] SimilarIDList, long MaxSizeInBytes);
        public static int GetSimilarIDList(int Tracker, long ID, out long[] SimilarIDList, long MaxSizeInBytes)
        {
            SimilarIDList = new long[MaxSizeInBytes / sizeof(long)];
            return GetSimilarIDListInternal(Tracker, ID, SimilarIDList, MaxSizeInBytes);
        }

        [DllImport(Dll, EntryPoint = "FSDK_SaveTrackerMemoryToFile", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SaveTrackerMemoryToFileInternal(int Tracker, string FileName);
        public static int SaveTrackerMemoryToFile(int Tracker, string FileName)
        {
            return SaveTrackerMemoryToFileInternal(Tracker, FileName);
        }

        [DllImport(Dll, EntryPoint = "FSDK_LoadTrackerMemoryFromFile", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int LoadTrackerMemoryFromFileInternal(out int Tracker, string FileName);
        public static int LoadTrackerMemoryFromFile(out int Tracker, string FileName)
        {
            return LoadTrackerMemoryFromFileInternal(out Tracker, FileName);
        }

        [DllImport(Dll, EntryPoint = "FSDK_GetTrackerMemoryBufferSize", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetTrackerMemoryBufferSizeInternal(int Tracker, out long BufSize);
        public static int GetTrackerMemoryBufferSize(int Tracker, out long BufSize)
        {
            return GetTrackerMemoryBufferSizeInternal(Tracker, out BufSize);
        }

        [DllImport(Dll, EntryPoint = "FSDK_SaveTrackerMemoryToBuffer", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SaveTrackerMemoryToBufferInternal(int Tracker, [Out, MarshalAs(UnmanagedType.LPArray)] byte[] Buffer, long MaxSizeInBytes);
        public static int SaveTrackerMemoryToBuffer(int Tracker, out byte[] Buffer)
        {
            var res = GetTrackerMemoryBufferSize(Tracker, out var size);
            if (res != FSDKE_OK)
            {
                Buffer = null;
                return res;
            }
            Buffer = new byte[size];
            return SaveTrackerMemoryToBufferInternal(Tracker, Buffer, size);
        }

        [DllImport(Dll, EntryPoint = "FSDK_LoadTrackerMemoryFromBuffer", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int LoadTrackerMemoryFromBufferInternal(out int Tracker, byte[] Buffer);
        public static int LoadTrackerMemoryFromBuffer(out int Tracker, byte[] Buffer)
        {
            return LoadTrackerMemoryFromBufferInternal(out Tracker, Buffer);
        }
        //}

        //FACIAL_ATTRIBUTES{
        [DllImport(Dll, EntryPoint = "FSDK_GetTrackerFacialAttribute", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetTrackerFacialAttributeInternal(int Tracker, long CameraIdx, long ID, string AttributeName, [Out] StringBuilder AttributeValues, long MaxSizeInBytes);
        public static int GetTrackerFacialAttribute(int Tracker, long CameraIdx, long ID, string AttributeName, out string AttributeValues, long MaxSizeInBytes)
        {
            StringBuilder tmps = new StringBuilder((int)MaxSizeInBytes);
            int res = GetTrackerFacialAttributeInternal(Tracker, CameraIdx, ID, AttributeName, tmps, MaxSizeInBytes);
            AttributeValues = tmps.ToString();
            return res;
        }

        [DllImport(Dll, EntryPoint = "FSDK_GetTrackerIDsCount", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetTrackerIDsCount(int Tracker, out long Count);

        [DllImport(Dll, EntryPoint = "FSDK_GetTrackerAllIDs", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetTrackerAllIDsInternal(int Tracker, [Out, MarshalAs(UnmanagedType.LPArray)] long[] IDList, long MaxSizeInBytes);
        public static int GetTrackerAllIDs(int Tracker, out long[] IDList, long MaxSizeInBytes)
        {
            IDList = new long[MaxSizeInBytes / 8];
            return GetTrackerAllIDsInternal(Tracker, IDList, MaxSizeInBytes);
        }

        [DllImport(Dll, EntryPoint = "FSDK_GetTrackerFaceIDsCountForID", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetTrackerFaceIDsCountForID(int Tracker, long ID, out long Count);

        [DllImport(Dll, EntryPoint = "FSDK_GetTrackerFaceIDsForID", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetTrackerFaceIDsForIDInternal(int Tracker, long ID, [Out, MarshalAs(UnmanagedType.LPArray)] long[] FaceIDList, long MaxSizeInBytes);
        public static int GetTrackerFaceIDsForID(int Tracker, long ID, out long[] FaceIDList, long MaxSizeInBytes)
        {
            FaceIDList = new long[MaxSizeInBytes / 8];
            return GetTrackerFaceIDsForIDInternal(Tracker, ID, FaceIDList, MaxSizeInBytes);
        }

        [DllImport(Dll, EntryPoint = "FSDK_GetTrackerIDByFaceID", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetTrackerIDByFaceID(int Tracker, long FaceID, out long ID);

        [DllImport(Dll, EntryPoint = "FSDK_GetTrackerFaceTemplate", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int FSDK_GetTrackerFaceTemplateInternal(int Tracker, long FaceID, [In, Out, MarshalAs(UnmanagedType.LPArray)] byte[] FaceTemplate);
        public static int GetTrackerFaceTemplate(int Tracker, long FaceID, out byte[] FaceTemplate)
        {
            FaceTemplate = new byte[FSDK.TemplateSize];
            return FSDK_GetTrackerFaceTemplateInternal(Tracker, FaceID, FaceTemplate);
        }

        [DllImport(Dll, EntryPoint = "FSDK_TrackerCreateID", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int FSDK_TrackerCreateIDInternal(int Tracker, [In, Out, MarshalAs(UnmanagedType.LPArray)] byte[] FaceTemplate, out long ID, out long FaceID);
        public static int TrackerCreateID(int Tracker, ref byte[] FaceTemplate, out long ID, out long FaceID)
        {
            return FSDK_TrackerCreateIDInternal(Tracker, FaceTemplate, out ID, out FaceID);
        }

        [DllImport(Dll, EntryPoint = "FSDK_AddTrackerFaceTemplate", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int FSDK_AddTrackerFaceTemplateInternal(int Tracker, long ID, [In, Out, MarshalAs(UnmanagedType.LPArray)] byte[] FaceTemplate, out long FaceID);
        public static int AddTrackerFaceTemplate(int Tracker, long ID, ref byte[] FaceTemplate, out long FaceID)
        {
            return FSDK_AddTrackerFaceTemplateInternal(Tracker, ID, FaceTemplate, out FaceID);
        }

        [DllImport(Dll, EntryPoint = "FSDK_DeleteTrackerFace", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern int DeleteTrackerFace(int Tracker, long FaceID);

        [DllImport(Dll, EntryPoint = "FSDK_GetTrackerFaceImage", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetTrackerFaceImage(int Tracker, long FaceID, out int Image);

        [DllImport(Dll, EntryPoint = "FSDK_SetTrackerFaceImage", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetTrackerFaceImage(int Tracker, long FaceID, int Image);

        [DllImport(Dll, EntryPoint = "FSDK_DeleteTrackerFaceImage", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern int DeleteTrackerFaceImage(int Tracker, long FaceID);

        [DllImport(Dll, EntryPoint = "FSDK_TrackerMatchFaces", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern int FSDK_TrackerMatchFacesInternal(int Tracker, [In, Out, MarshalAs(UnmanagedType.LPArray)] byte[] FaceTemplate, float Threshold, [Out, MarshalAs(UnmanagedType.LPArray)] IDSimilarity[] Buffer, out long Count, long MaxSizeInBytes);
        public static int TrackerMatchFaces(int Tracker, ref byte[] FaceTemplate, float Threshold, out IDSimilarity[] Buffer, long MaxSizeInBytes)
        {
            Buffer = new IDSimilarity[MaxSizeInBytes / sizeof(IDSimilarity)];
            long Count = 0;
            int res = FSDK_TrackerMatchFacesInternal(Tracker, FaceTemplate, Threshold, Buffer, out Count, MaxSizeInBytes);
            Array.Resize(ref Buffer, (int)Count);
            return res;
        }

        [DllImport(Dll, EntryPoint = "FSDK_DetectFacialAttributeUsingFeatures", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int DetectFacialAttributeUsingFeaturesInternal(int Image, [In, MarshalAs(UnmanagedType.LPArray, SizeConst = FSDK_FACIAL_FEATURE_COUNT)] TPoint[] FacialFeatures, string AttributeName, [Out] StringBuilder AttributeValues, long MaxSizeInBytes);
        public static int DetectFacialAttributeUsingFeatures(int Image, TPoint[] FacialFeatures, string AttributeName, out string AttributeValues, long MaxSizeInBytes)
        {
            StringBuilder tmps = new StringBuilder((int)MaxSizeInBytes);
            int res = DetectFacialAttributeUsingFeaturesInternal(Image, FacialFeatures, AttributeName, tmps, MaxSizeInBytes);
            AttributeValues = tmps.ToString();
            return res;
        }

        [DllImport(Dll, EntryPoint = "FSDK_GetValueConfidence", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetValueConfidenceInternal(string AttributeValues, string Value, out float Confidence);
        public static int GetValueConfidence(string AttributeValues, string Value, out float Confidence)
        {
            return GetValueConfidenceInternal(AttributeValues, Value, out Confidence);
        }


        [DllImport(Dll, EntryPoint = "FSDK_SetParameter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SetParameterInternal(string ParameterName, string ParameterValue);
        public static int SetParameter(string ParameterName, string ParameterValue)
        {
            return SetParameterInternal(ParameterName, ParameterValue);
        }


        [DllImport(Dll, EntryPoint = "FSDK_SetParameters", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SetParametersInternal(string Parameters, out int ErrorPos);
        public static int SetParameters(string Parameters, out int ErrorPos)
        {
            return SetParametersInternal(Parameters, out ErrorPos);
        }
        //}

        /// <summary>
        /// Initializes camera capturing system-wide. Call before using any camera functions.
        /// </summary>
        [DllImport(Dll, EntryPoint = "FSDK_InitializeCapturing", CallingConvention = CallingConvention.Cdecl)]
        private static extern int InitializeCapturingInternal();
        public static int InitializeCapturing() => InitializeCapturingInternal();

        /// <summary>
        /// Finalizes camera capturing system-wide. Call after all camera operations are done.
        /// </summary>
        [DllImport(Dll, EntryPoint = "FSDK_FinalizeCapturing", CallingConvention = CallingConvention.Cdecl)]
        private static extern int FinalizeCapturingInternal();
        public static int FinalizeCapturing() => FinalizeCapturingInternal();

        /// <summary>
        /// Sets the camera naming mode. If true, uses the device path as the camera name; otherwise, uses the default name.
        /// </summary>
        /// <param name="UseDevicePathAsName">If true, use device path as camera name; otherwise, use default name.</param>
        /// <returns>FSDKE_OK on success or an error code on failure.</returns>
        [DllImport(Dll, EntryPoint = "FSDK_SetCameraNaming", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetCameraNaming(bool UseDevicePathAsName);

        /// <summary>
        /// Frees a camera list previously allocated by the SDK.
        /// </summary>
        /// <param name="CameraList">Pointer to the camera list array.</param>
        /// <param name="CameraCount">Number of cameras in the list.</param>
        /// <returns>FSDKE_OK on success or an error code on failure.</returns>
        [DllImport(Dll, EntryPoint = "FSDK_FreeCameraList", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int FreeCameraList(byte** CameraList, int CameraCount);

        /// <summary>
        /// Gets the list of available camera names.
        /// </summary>
        [DllImport(Dll, EntryPoint = "FSDK_GetCameraList", CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetCameraListInternal(byte*** CameraList, out int CameraCount);
        public static string[] GetCameraList()
        {
            byte** pCameraList;
            int res = GetCameraListInternal(&pCameraList, out var CameraCount);
            string[] CameraList = Luxand.Helpers.GetStrings(pCameraList, CameraCount);
            FreeCameraList(pCameraList, CameraCount);
            return CameraList;
        }

        [DllImport(Dll, EntryPoint = "FSDK_GetCameraListEx", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetCameraListExInternal(byte*** CameraNameList, byte*** CameraDevicePathList, out int CameraCount);
        public static int GetCameraListEx(out string[] CameraNameList, out string[] CameraDevicePathList, out int CameraCount)
        {
            byte** pCameraNMList;
            byte** pCameraDPList;
            var res = GetCameraListExInternal(&pCameraNMList, &pCameraDPList, out CameraCount);
            CameraNameList = Helpers.GetStrings(pCameraNMList, CameraCount);
            CameraDevicePathList = Helpers.GetStrings(pCameraDPList, CameraCount);
            FreeCameraList(pCameraNMList, CameraCount);
            FreeCameraList(pCameraDPList, CameraCount);
            return res;
        }

        [DllImport(Dll, EntryPoint = "FSDK_FreeVideoFormatList", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int FreeVideoFormatList(void* VideoFormatList);
        
        [DllImport(Dll, EntryPoint = "FSDK_GetVideoFormatList", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int FSDK_GetVideoFormatListInternal([In]byte[] CameraName, out void* VideoFormatList, out int VideoFormatCount);
        public static int GetVideoFormatList(ref string CameraName, out VideoFormatInfo[] VideoFormatList, out int VideoFormatCount){
            void * pVideoFormatList;
            int res = FSDK_GetVideoFormatListInternal(Helpers.EncodeString(CameraName), out pVideoFormatList, out VideoFormatCount);
            VideoFormatList = new VideoFormatInfo[VideoFormatCount];
            for (int i = 0; i < VideoFormatCount; ++i)
            {
                VideoFormatList[i] = ((VideoFormatInfo*)pVideoFormatList)[i];
            }
            FreeVideoFormatList(pVideoFormatList);
            return res;
        }
        public static VideoFormatInfo[] GetVideoFormatList(string CameraName)
        {
            if (string.IsNullOrEmpty(CameraName))
            {
                throw new ArgumentException("Camera name cannot be null or empty.", nameof(CameraName));
            }
            GetVideoFormatList(ref CameraName, out var VideoFormatList, out var VideoFormatCount);
            return VideoFormatList;
        }

        /// <summary>
        /// Opens a video camera by name and returns a handle.
        /// </summary>
        [DllImport(Dll, EntryPoint = "FSDK_OpenVideoCamera", CallingConvention = CallingConvention.Cdecl)]
        private static extern int OpenVideoCameraInternal([In] byte[] cameraName, out int cameraHandle);
        public static int OpenVideoCamera(string cameraName, out int cameraHandle) => OpenVideoCameraInternal(Helpers.EncodeString(cameraName), out cameraHandle);

        /// <summary>
        /// Sets the video format for the specified camera.
        /// </summary>
        [DllImport(Dll, EntryPoint = "FSDK_SetVideoFormat", CallingConvention = CallingConvention.Cdecl)]
        private static extern int SetVideoFormatInternal([In] byte[] cameraName, VideoFormatInfo videoFormat);
        public static int SetVideoFormat(string cameraName, VideoFormatInfo videoFormat) => SetVideoFormatInternal(Helpers.EncodeString(cameraName), videoFormat);

        /// <summary>
        /// Opens an IP video camera with the specified parameters and returns a handle.
        /// </summary>
        [DllImport(Dll, EntryPoint = "FSDK_OpenIPVideoCamera", CallingConvention = CallingConvention.Cdecl)]
        private static extern int OpenIPVideoCameraInternal(Camera.VideoCompressionType compressionType, string url, string username, string password, int timeoutSeconds, out int cameraHandle);
        public static int OpenIPVideoCamera(Camera.VideoCompressionType compressionType, string url, string username, string password, int timeoutSeconds, out int cameraHandle)
            => OpenIPVideoCameraInternal(compressionType, url, username, password, timeoutSeconds, out cameraHandle);

        /// <summary>
        /// Closes the specified video camera handle.
        /// </summary>
        [DllImport(Dll, EntryPoint = "FSDK_CloseVideoCamera", CallingConvention = CallingConvention.Cdecl)]
        private static extern int CloseVideoCameraInternal(int cameraHandle);
        public static int CloseVideoCamera(int cameraHandle) => CloseVideoCameraInternal(cameraHandle);

        /// <summary>
        /// Grabs a frame from the specified camera handle and returns an image handle.
        /// </summary>
        [DllImport(Dll, EntryPoint = "FSDK_GrabFrame", CallingConvention = CallingConvention.Cdecl)]
        private static extern int GrabFrameInternal(int cameraHandle, out int imageHandle);
        public static int GrabFrame(int cameraHandle, out int imageHandle) => GrabFrameInternal(cameraHandle, out imageHandle);
    }
}
