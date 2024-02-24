using System.Text;

namespace ClassAnalyser.Analysis.RTTI
{
    public interface IRTTIReader
    {
        public nuint GetBaseAddress();
        public nuint GetPointerSize();
        public bool IsMemoryAccessible(nuint in_address);
        public T Read<T>(nuint in_address) where T : unmanaged;
        public nuint ReadPointer(nuint in_address);
        public string ReadStringNullTerminated(nuint in_address, Encoding in_encoding = null);
    }
}
