using System;

namespace Luxand
{
    /// <summary>
    /// Represents an image loaded or created with FaceSDK. Provides methods for face detection, manipulation, and template extraction.
    /// </summary>
    public class CImage : IDisposable
    {
        private int hImage = -1, width = 0, height = 0;
        private bool disposed = false;

        /// <summary>
        /// Updates the width and height fields from the native image handle.
        /// </summary>
        private void PopulateHeightAndWidth()
        {
            FSDK.CheckForError(FSDK.GetImageHeight(hImage, out height));
            FSDK.CheckForError(FSDK.GetImageWidth(hImage, out width));
        }

        /// <summary>
        /// Gets the native image handle.
        /// </summary>
        public int ImageHandle => hImage;

        /// <summary>
        /// Gets the width of the image in pixels. This property is lazily evaluated and cached after the first call.
        /// </summary>
        public int Width => GetWidth();
        /// <summary>
        /// Gets the height of the image in pixels. This property is lazily evaluated and cached after the first call.
        /// </summary>
        public int Height => GetHeight();

        /// <summary>
        /// Creates an empty image.
        /// </summary>
        public CImage()
        {
            FSDK.CheckForError(FSDK.CreateEmptyImage(out hImage));
        }

        /// <summary>
        /// Wraps an existing FaceSDK image handle.
        /// </summary>
        public CImage(int ImageHandle) // constructor for making CImage from image already loaded to FaceSDK
        {
            hImage = ImageHandle;
            PopulateHeightAndWidth();
        }

        /// <summary>
        /// Loads an image from a file.
        /// </summary>
        public CImage(string FileName)
        {
            FSDK.CheckForError(FSDK.LoadImageFromFile(out hImage, FileName));
            PopulateHeightAndWidth();
        }

        /// <summary>
        /// Loads an image from a Windows HBITMAP handle.
        /// </summary>
        public CImage(IntPtr BitmapHandle)
        {
            FSDK.CheckForError(FSDK.LoadImageFromHBitmap(out hImage, BitmapHandle));
            PopulateHeightAndWidth();
        }

#if USE_SYSTEM_DRAWING
        /// <summary>
        /// Loads an image from a System.Drawing.Image object.
        /// </summary>
        /// <param name="ImageObject">The System.Drawing.Image to load.</param>
        public CImage(System.Drawing.Image ImageObject)
        {
            FSDK.CheckForError(FSDK.LoadImageFromCLRImage(out hImage, ImageObject));
            PopulateHeightAndWidth();
        }

        /// <summary>
        /// Converts the FaceSDK image to a System.Drawing.Image object.
        /// </summary>
        /// <returns>The converted System.Drawing.Image.</returns>
        public System.Drawing.Image ToCLRImage()
        {
            FSDK.CheckForError(FSDK.SaveImageToCLRImage(hImage, out var img));
            return img;
        }
#endif

        /// <summary>
        /// Returns the height of the image in pixels. Throws if the image handle is invalid.
        /// </summary>
        /// <returns>The height of the image in pixels.</returns>
        public int GetHeight()
        {
            if (hImage < 0)
                throw new InvalidOperationException("Image handle is invalid.");
            if (height == 0)
                FSDK.CheckForError(FSDK.GetImageHeight(hImage, out height));
            return height;
        }

        /// <summary>
        /// Returns the width of the image in pixels. Throws if the image handle is invalid.
        /// </summary>
        /// <returns>The width of the image in pixels.</returns>
        public int GetWidth()
        {
            if (hImage < 0)
                throw new InvalidOperationException("Image handle is invalid.");
            if (width == 0)
                FSDK.CheckForError(FSDK.GetImageWidth(hImage, out width));
            return width;
        }

        /// <summary>
        /// Releases the resources used by the CImage instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the resources used by the CImage instance.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing) { /* dispose managed components */ }

                if (hImage >= 0)
                {
                    FSDK.FreeImage(hImage);
                    hImage = -1;
                }
            }
            disposed = true;
        }
        ~CImage()
        {
            Dispose(false);
        }

        /// <summary>
        /// Detects the most prominent face in the image.
        /// </summary>
        /// <returns>A <see cref="FSDK.TFace"/> structure describing the detected face.</returns>
        public FSDK.TFace DetectFace()
        {
            FSDK.CheckForError(FSDK.DetectFace(hImage, out FSDK.TFace face));
            return face;
        }

        /// <summary>
        /// Detects multiple faces in the image, sorted by detection score in descending order.
        /// </summary>
        /// <param name="maxCount">The maximum number of faces to detect.</param>
        /// <returns>An array of <see cref="FSDK.TFace"/> structures describing the detected faces. Empty if no face was found.</returns>
        public FSDK.TFace[] DetectMultipleFaces(int maxCount = 256)
        {
            var res = FSDK.DetectMultipleFaces(hImage, out FSDK.TFace[] FaceArray, maxCount);

            if (FSDK.FSDKE_FACE_NOT_FOUND != res)
                FSDK.CheckForError(res);

            return FaceArray;
        }

        /// <summary>
        /// Detects facial features in the image.
        /// </summary>
        /// <returns>An array of TPointF structures representing the detected facial features.</returns>
        public FSDK.TPointF[] DetectFacialFeatures()
        {
            FSDK.CheckForError(FSDK.DetectFacialFeatures(hImage, out FSDK.TPointF[] feats));
            return feats;
        }

        /// <summary>
        /// Detects facial features in the image within a previously detected face.
        /// </summary>
        /// <param name="Face">The face to look inside.</param>
        /// <returns>An array of TPointF structures representing the detected facial features.</returns>
        public FSDK.TPointF[] DetectFacialFeaturesInRegion(in FSDK.TFace Face)
        {
            FSDK.CheckForError(FSDK.DetectFacialFeaturesInRegion(hImage, Face, out FSDK.TPointF[] feats));
            return feats;
        }

        /// <summary>
        /// Detects facial attributes using the detected facial features.
        /// </summary>
        /// <returns> Facial attribute as a string. </returns>
        public string DetectFacialAttributeUsingFeatures(FSDK.TPointF[] FacialFeatures, string AttributeName)
        {
            FSDK.CheckForError(FSDK.DetectFacialAttributeUsingFeatures(hImage, FacialFeatures, AttributeName, out string attr, 1024));
            return attr;
        }

        /// <summary>
        /// Detects facial attributes for a previously detected face, without detecting the facial features first.
        /// </summary>
        /// <param name="Face">The face to describe.</param>
        /// <param name="AttributeName">The name of the attribute, for example "Gender" or "Expression".</param>
        /// <returns> Facial attribute as a string. </returns>
        public string DetectFacialAttributeUsingFace(in FSDK.TFace Face, string AttributeName)
        {
            FSDK.CheckForError(FSDK.DetectFacialAttributeUsingFace(hImage, Face, AttributeName, out string attr));
            return attr;
        }

        /// <summary>
        /// Mirrors the image vertically.
        /// </summary>
        /// <returns>The mirrored image.</returns>
        public CImage MirrorVertical()
        {
            FSDK.CheckForError(FSDK.MirrorImage(hImage, false));
            return this;
        }

        /// <summary>
        /// Mirrors the image horizontally.
        /// </summary>
        /// <returns>The mirrored image.</returns>
        public CImage MirrorHorizontal()
        {
            FSDK.CheckForError(FSDK.MirrorImage(hImage, true));
            return this;
        }

        /// <summary>
        /// Resizes the image by a specified ratio.
        /// </summary>
        /// <param name="Ratio">The ratio to resize the image by.</param>
        /// <returns>The resized image.</returns>
        public CImage Resize(double Ratio)
        {
            FSDK.CheckForError(FSDK.CreateEmptyImage(out int NewImage));
            FSDK.CheckForError(FSDK.ResizeImage(hImage, Ratio, NewImage));
            return new CImage(NewImage);
        }

        /// <summary>
        /// Rotates the image by a specified angle.
        /// </summary>
        /// <param name="Angle">The angle to rotate the image by.</param>
        /// <returns>The rotated image.</returns>
        public CImage Rotate(double Angle)
        {
            FSDK.CheckForError(FSDK.CreateEmptyImage(out int NewImage));
            FSDK.CheckForError(FSDK.RotateImage(hImage, Angle, NewImage));
            return new CImage(NewImage);
        }

        /// <summary>
        /// Rotates the image around a specified center point.
        /// </summary>
        /// <param name="Angle">The angle to rotate the image by.</param>
        /// <param name="XCenter">The x-coordinate of the center point.</param>
        /// <param name="YCenter">The y-coordinate of the center point.</param>
        public CImage RotateImageCenter(double Angle, double XCenter, double YCenter)
        {
            FSDK.CheckForError(FSDK.CreateEmptyImage(out int NewImage));
            FSDK.CheckForError(FSDK.RotateImageCenter(hImage, Angle, XCenter, YCenter, NewImage));
            return new CImage(NewImage);
        }

        /// <summary>
        /// Rotates the image by 90 degrees in the specified direction.
        /// </summary>
        /// <param name="Multiplier">The number of times to rotate 90 degrees.</param>
        /// <returns>The rotated image.</returns>
        public CImage Rotate90(int Multiplier)
        {
            FSDK.CheckForError(FSDK.CreateEmptyImage(out int NewImage));
            FSDK.CheckForError(FSDK.RotateImage90(hImage, Multiplier, NewImage));
            return new CImage(NewImage);
        }

        /// <summary>
        /// Creates a copy of the image.
        /// </summary>
        /// <returns>The copied image.</returns>
        public CImage Copy()
        {
            FSDK.CheckForError(FSDK.CreateEmptyImage(out int NewImage));
            FSDK.CheckForError(FSDK.CopyImage(hImage, NewImage));
            return new CImage(NewImage);
        }

        /// <summary>
        /// Copies a rectangular region of the image.
        /// </summary>
        /// <param name="x1">The x-coordinate of the top-left corner of the region.</param>
        /// <param name="y1">The y-coordinate of the top-left corner of the region.</param>
        /// <param name="x2">The x-coordinate of the bottom-right corner of the region.</param>
        /// <param name="y2">The y-coordinate of the bottom-right corner of the region.</param>
        /// <returns>The image containing the copied region.</returns>
        public CImage CopyRect(int x1, int y1, int x2, int y2)
        {
            FSDK.CheckForError(FSDK.CreateEmptyImage(out int NewImage));
            FSDK.CheckForError(FSDK.CopyRect(hImage, x1, y1, x2, y2, NewImage));
            return new CImage(NewImage);
        }

        /// <summary>
        /// Copies a rectangular region of the image and replicates the border.
        /// </summary>
        /// <param name="x1">The x-coordinate of the top-left corner of the region.</param>
        /// <param name="y1">The y-coordinate of the top-left corner of the region.</param>
        /// <param name="x2">The x-coordinate of the bottom-right corner of the region.</param>
        /// <param name="y2">The y-coordinate of the bottom-right corner of the region.</param>
        /// <returns>The image containing the copied region with replicated border.</returns>
        public CImage CopyRectReplicateBorder(int x1, int y1, int x2, int y2)
        {
            FSDK.CheckForError(FSDK.CreateEmptyImage(out int NewImage));
            FSDK.CheckForError(FSDK.CopyRectReplicateBorder(hImage, x1, y1, x2, y2, NewImage));
            return new CImage(NewImage);
        }

        /// <summary>
        /// Saves the image to a file.
        /// </summary>
        /// <param name="FileName">The name of the file to save the image to.</param>
        public void Save(string FileName)
        {
            FSDK.CheckForError(FSDK.SaveImageToFile(hImage, FileName));
        }

        /// <summary>
        /// Gets the Windows HBITMAP handle of the image.
        /// </summary>
        /// <returns>The HBITMAP handle.</returns>
        public IntPtr GetHbitmap()
        {
            FSDK.CheckForError(FSDK.SaveImageToHBitmap(hImage, out IntPtr bmh));
            return bmh;
        }

        /// <summary>
        /// Extracts the face template from the image.
        /// </summary>
        /// <returns>The face template bytes.</returns>
        public byte[] GetFaceTemplate()
        {
            FSDK.CheckForError(FSDK.GetFaceTemplate(hImage, out byte[] tmpl));
            return tmpl;
        }

        /// <summary>
        /// Extracts the face template for a previously detected face.
        /// </summary>
        /// <param name="Face">The face to extract the template for.</param>
        /// <returns>The face template bytes.</returns>
        public byte[] GetFaceTemplateInRegion(in FSDK.TFace Face)
        {
            FSDK.CheckForError(FSDK.GetFaceTemplateInRegion(hImage, Face, out byte[] tmpl));
            return tmpl;
        }

        /// <summary>
        /// Gets the required buffer size in bytes for saving the image in the specified pixel format.
        /// </summary>
        /// <param name="ImageMode">The pixel format for which to calculate the buffer size.</param>
        /// <returns>The required buffer size in bytes.</returns>
        public int GetImageBufferSize(FSDK.FSDK_IMAGEMODE ImageMode)
        {
            FSDK.CheckForError(FSDK.GetImageBufferSize(hImage, out var Size, ImageMode));
            return Size;
        }

        /// <summary>
        /// Saves the image to a byte buffer in the specified pixel format.
        /// </summary>
        /// <param name="ImageMode">The pixel format to use for the output buffer.</param>
        /// <returns>A byte array containing the image data.</returns>
        public byte[] SaveImageToBuffer(FSDK.FSDK_IMAGEMODE ImageMode = FSDK.FSDK_IMAGEMODE.FSDK_IMAGE_COLOR_24BIT)
        {
            FSDK.CheckForError(FSDK.SaveImageToBuffer(hImage, out var buffer, ImageMode));
            return buffer;
        }

        /// <summary>
        /// Saves the image to a file.
        /// </summary>
        /// <param name="FileName">The name of the file to save the image to.</param>
        public int SaveImageToFile(string FileName)
        {
            FSDK.CheckForError(FSDK.SaveImageToFile(hImage, FileName));
            return 0;
        }

        /// <summary>
        /// Saves the image to a file.
        /// </summary>
        /// <param name="FileName">The name of the file to save the image to.</param>
        public int SaveImageToFileW(string FileName)
        {
            FSDK.CheckForError(FSDK.SaveImageToFileW(hImage, FileName));
            return 0;
        }

        /// <summary>
        /// Loads an image from a raw pixel buffer.
        /// </summary>
        /// <param name="Buffer">The byte array containing the image pixel data.</param>
        /// <param name="Width">The width of the image in pixels.</param>
        /// <param name="Height">The height of the image in pixels.</param>
        /// <param name="ScanLine">The number of bytes per image row (stride).</param>
        /// <param name="ImageMode">The pixel format of the image buffer.</param>
        /// <returns>A new <see cref="CImage"/> instance containing the loaded image.</returns>
        public static CImage LoadImageFromBuffer(byte[] Buffer, int Width, int Height, int ScanLine, FSDK.FSDK_IMAGEMODE ImageMode)
        {
            FSDK.CheckForError(FSDK.LoadImageFromBuffer(out int hImageBuffer, Buffer, Width, Height, ScanLine, ImageMode));
            return new CImage(hImageBuffer);
        }

        /// <summary>
        /// Loads an image from a file, preserving its alpha channel. The image is loaded in 32-bit color mode.
        /// </summary>
        /// <param name="FileName">The name of the file to load.</param>
        /// <returns>A new <see cref="CImage"/> instance containing the loaded image.</returns>
        public static CImage LoadImageFromFileWithAlpha(string FileName)
        {
            FSDK.CheckForError(FSDK.LoadImageFromFileWithAlpha(out int hImageBuffer, FileName));
            return new CImage(hImageBuffer);
        }

        /// <summary>
        /// Loads an image from a buffer holding an encoded JPEG file.
        /// </summary>
        /// <param name="Buffer">The bytes of the JPEG file.</param>
        /// <returns>A new <see cref="CImage"/> instance containing the loaded image.</returns>
        public static CImage LoadImageFromJpegBuffer(byte[] Buffer)
        {
            FSDK.CheckForError(FSDK.LoadImageFromJpegBuffer(out int hImageBuffer, Buffer));
            return new CImage(hImageBuffer);
        }

        /// <summary>
        /// Loads an image from a buffer holding an encoded PNG file.
        /// </summary>
        /// <param name="Buffer">The bytes of the PNG file.</param>
        /// <param name="WithAlpha">True to preserve the alpha channel of the PNG, false to discard it.</param>
        /// <returns>A new <see cref="CImage"/> instance containing the loaded image.</returns>
        public static CImage LoadImageFromPngBuffer(byte[] Buffer, bool WithAlpha = false)
        {
            int hImageBuffer;
            FSDK.CheckForError(WithAlpha
                ? FSDK.LoadImageFromPngBufferWithAlpha(out hImageBuffer, Buffer)
                : FSDK.LoadImageFromPngBuffer(out hImageBuffer, Buffer));
            return new CImage(hImageBuffer);
        }
    }
}
