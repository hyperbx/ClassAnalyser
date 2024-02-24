using System.Runtime.InteropServices;

namespace ClassAnalyser.Helpers
{
    public class MemoryHelper
    {
        /// <summary>
        /// Transforms a byte array to the specified unmanaged type.
        /// </summary>
        /// <typeparam name="T">The type to transform the byte array to.</typeparam>
        /// <param name="in_data">The data to transform.</param>
        /// <param name="in_isBigEndian">Determines whether the data is in big-endian format.</param>
        public static T ByteArrayToUnmanagedType<T>(byte[] in_data, bool in_isBigEndian = false) where T : unmanaged
        {
            if (in_data == null || in_data.Length <= 0)
                return default;

            if (in_isBigEndian)
                in_data = in_data.Reverse().ToArray();

            var handle = GCHandle.Alloc(in_data, GCHandleType.Pinned);

            try
            {
                return (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            }
            finally
            {
                handle.Free();
            }
        }

        /// <summary>
        /// Transforms an unmanaged object to a byte array.
        /// </summary>
        /// <typeparam name="T">The type to transform.</typeparam>
        /// <param name="in_structure">The data to transform.</param>
        /// <param name="in_isBigEndian">Determines whether the data is in big-endian format.</param>
        public static byte[] UnmanagedTypeToByteArray<T>(T in_structure, bool in_isBigEndian = true) where T : unmanaged
        {
            byte[] data = new byte[Marshal.SizeOf(typeof(T))];

            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);

            try
            {
                Marshal.StructureToPtr(in_structure, handle.AddrOfPinnedObject(), false);
            }
            finally
            {
                handle.Free();
            }

            return in_isBigEndian ? data.Reverse().ToArray() : data;
        }
    }
}
