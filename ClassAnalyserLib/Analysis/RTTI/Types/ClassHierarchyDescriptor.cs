namespace ClassAnalyser.Analysis.RTTI.Types
{
    public class ClassHierarchyDescriptor(IRTTIReader in_reader, nuint in_pThis)
    {
        protected IRTTIReader _reader = in_reader;

        /// <summary>
        /// The location of this class hierarchy descriptor.
        /// </summary>
        public nuint pThis { get; } = in_pThis;

        /// <summary>
        /// The signature of this class.
        /// </summary>
        public uint Signature { get; } = in_reader.Read<uint>(in_pThis);

        /// <summary>
        /// The attributes of this class.
        /// </summary>
        public uint Attributes { get; } = in_reader.Read<uint>(in_pThis + 0x04);

        /// <summary>
        /// The amount of base classes this class derives from.
        /// </summary>
        public uint BaseClassCount { get; } = in_reader.Read<uint>(in_pThis + 0x08);

        /// <summary>
        /// The location of the base class descriptors.
        /// </summary>
        public uint pBaseClasses { get; } = in_reader.Read<uint>(in_pThis + 0x0C);

        /// <summary>
        /// Gets a base class by index.
        /// </summary>
        /// <param name="in_index">The index of the base class.</param>
        public BaseClassDescriptor GetBaseClass(int in_index)
        {
            if (in_index > BaseClassCount)
                return null;

            var baseAddr = _reader.GetBaseAddress();

            return new BaseClassDescriptor(_reader,
                baseAddr + _reader.Read<uint>((baseAddr + pBaseClasses) + ((uint)in_index * 4)));
        }

        /// <summary>
        /// Gets all base classes.
        /// </summary>
        public BaseClassDescriptor[] GetBaseClasses()
        {
            var result = new List<BaseClassDescriptor>();

            for (int i = 0; i < BaseClassCount; i++)
                result.Add(GetBaseClass(i));

            return [.. result];
        }

        public override string ToString()
        {
            return $"{GetBaseClass(0).GetTypeDescriptor().GetName()}::`RTTI Class Hierarchy Descriptor'";
        }
    }
}
