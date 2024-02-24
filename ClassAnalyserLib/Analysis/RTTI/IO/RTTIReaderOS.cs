using System.Text;

namespace ClassAnalyser.Analysis.RTTI.IO
{
    public class RTTIReaderOS(int in_processID) : IRTTIReader
    {
        public virtual int ProcessID { get; set; } = in_processID;

        public virtual nuint GetBaseAddress()
        {
            throw new NotImplementedException();
        }

        public virtual nuint GetPointerSize()
        {
            throw new NotImplementedException();
        }

        public virtual bool IsMemoryAccessible(nuint in_address)
        {
            throw new NotImplementedException();
        }

        public virtual T Read<T>(nuint in_address) where T : unmanaged
        {
            throw new NotImplementedException();
        }

        public virtual nuint ReadPointer(nuint in_address)
        {
            throw new NotImplementedException();
        }

        public virtual string ReadStringNullTerminated(nuint in_address, Encoding in_encoding = null)
        {
            throw new NotImplementedException();
        }
    }
}
