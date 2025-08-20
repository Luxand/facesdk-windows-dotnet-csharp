using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace Luxand
{
    /// <summary>
    /// Helper methods for working with strings and byte arrays.
    /// </summary>
    public unsafe class Helpers 
    {
        /// <summary>
        /// Returns the length in bytes of a null-terminated char* string.
        /// </summary>
        /// <param name="data">Pointer to a null-terminated char array.</param>
        /// <returns>Length in bytes (not characters) of the string, excluding the null terminator.</returns>
        public static int GetLength(char* data)
        {
            var res = 0;
            while (data[res] != '\0')
            {
                ++res;
            }
            return sizeof(char) * res;
        }

        /// <summary>
        /// Returns the length in bytes of a null-terminated byte* string.
        /// </summary>
        /// <param name="data">Pointer to a null-terminated byte array.</param>
        /// <returns>Length in bytes of the string, excluding the null terminator.</returns>
        public static int GetLength(byte* data)
        {
            var res = 0;
            while (data[res] != 0)
            {
                ++res;
            }
            return sizeof(byte) * res;
        }

        /// <summary>
        /// Determines if the current platform uses UTF-16 encoding for strings (Windows) or UTF-8 (Unix/Mac).
        /// </summary>
        /// <returns>True if UTF-16 (Windows), false if UTF-8 (Unix/Mac).</returns>
        public static bool IsUTF16()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.WinCE:
                case PlatformID.Win32S:
                case PlatformID.Win32NT:
                case PlatformID.Win32Windows:
                    return true;
                case PlatformID.MacOSX:
                case PlatformID.Unix:
                    return false;
                case PlatformID.Xbox:
                    throw new InvalidEnumArgumentException();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Returns a managed byte array containing the bytes of a native null-terminated string.
        /// </summary>
        /// <param name="data">Pointer to the native string (byte* or char*).</param>
        /// <returns>Byte array with the string data (without null terminator).</returns>
        public static byte[] GetByteArray(byte* data)
        {
            return new byte[IsUTF16() ? GetLength((char*)data) : GetLength(data)];
        }

        /// <summary>
        /// Converts an array of native string pointers to a managed string array, using the appropriate encoding for the platform.
        /// </summary>
        /// <param name="data">Pointer to an array of string pointers.</param>
        /// <param name="count">Number of strings.</param>
        /// <returns>Array of managed strings.</returns>
        public static string[] GetStrings(byte** data, int count)
        {
            var encoding = IsUTF16() ? Encoding.Unicode : Encoding.UTF8;
            var result = new string[count];
            for (var i = 0; i < count; ++i)
            {
                var bytes = GetByteArray(data[i]);
                Marshal.Copy((IntPtr)data[i], bytes, 0, bytes.Length);
                result[i] = encoding.GetString(bytes);
            }

            return result;
        }

        /// <summary>
        /// Encodes a managed string to a byte array using the platform's native encoding (UTF-16 or UTF-8).
        /// </summary>
        /// <param name="value">The managed string to encode.</param>
        /// <returns>Byte array with the encoded string.</returns>
        public static byte[] EncodeString(string value)
        {
            return IsUTF16() ? Encoding.Unicode.GetBytes(value) : Encoding.UTF8.GetBytes(value);
        }
    }
}