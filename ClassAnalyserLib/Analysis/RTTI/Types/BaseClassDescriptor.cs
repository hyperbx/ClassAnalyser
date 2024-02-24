namespace ClassAnalyser.Analysis.RTTI.Types
{
    public class BaseClassDescriptor(IRTTIReader in_reader, nuint in_pThis)
    {
        protected IRTTIReader _reader = in_reader;

        /// <summary>
        /// The location of this base class descriptor.
        /// </summary>
        public nuint pThis { get; } = in_pThis;

        /// <summary>
        /// The location of the type descriptor for this class.
        /// </summary>
        public uint pTypeDescriptor { get; } = in_reader.Read<uint>(in_pThis);

        /// <summary>
        /// The number of sub-elements for this class in the base class array (used for multiple inheritence).
        /// </summary>
        public int SubElementCount { get; } = in_reader.Read<int>(in_pThis + 0x04);

        public int MemberDisplacement { get; } = in_reader.Read<int>(in_pThis + 0x08);
        public int VftableDisplacement { get; } = in_reader.Read<int>(in_pThis + 0x0C);
        public int DisplacementWithinVftable { get; } = in_reader.Read<int>(in_pThis + 0x10);
        public int BaseClassAttributes { get; } = in_reader.Read<int>(in_pThis + 0x14);

        /// <summary>
        /// The location of the class hierarchy descriptor for this class.
        /// </summary>
        public uint pClassHierarchyDescriptor { get; } = in_reader.Read<uint>(in_pThis + 0x18);

        /// <summary>
        /// Gets the type descriptor.
        /// </summary>
        public TypeDescriptor GetTypeDescriptor()
        {
            return new TypeDescriptor(_reader, _reader.GetBaseAddress() + pTypeDescriptor);
        }

        /// <summary>
        /// Gets the class hierarchy descriptor.
        /// </summary>
        public ClassHierarchyDescriptor GetClassHierarchyDescriptor()
        {
            return new ClassHierarchyDescriptor(_reader, _reader.GetBaseAddress() + pClassHierarchyDescriptor);
        }

        public override string ToString()
        {
            return $"{GetTypeDescriptor().GetName()}::`RTTI Base Class Descriptor at ({MemberDisplacement}, {VftableDisplacement}, {DisplacementWithinVftable}, {BaseClassAttributes})'";
        }
    }
}
