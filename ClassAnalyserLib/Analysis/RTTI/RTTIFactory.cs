using ClassAnalyser.Analysis.MSVC;
using ClassAnalyser.Analysis.RTTI.Types;

namespace ClassAnalyser.Analysis.RTTI
{
    public class RTTIFactory(IRTTIReader in_reader)
    {
        protected IRTTIReader _reader = in_reader;

        /// <summary>
        /// Gets RTTI from the input vftable pointer.
        /// </summary>
        /// <param name="in_pVftable">The pointer to the vftable with RTTI.</param>
        public CompleteObjectLocator GetRuntimeInfoFromVftable(nuint in_pVftable)
        {
            if (in_pVftable == 0)
                return null;

            var addr = _reader.ReadPointer(in_pVftable - _reader.GetPointerSize());

            if (!_reader.IsMemoryAccessible(addr))
                return null;

            var result = new CompleteObjectLocator(_reader, addr);

            if (!result.IsValid())
                return null;

            return result;
        }

        /// <summary>
        /// Gets RTTI from the input class pointer.
        /// </summary>
        /// <param name="in_pClass">The pointer to the class where the first member is a pointer back to the vftable which has RTTI.</param>
        public CompleteObjectLocator GetRuntimeInfoFromClass(nuint in_pClass)
        {
            return GetRuntimeInfoFromVftable(GetVftableFromClass(in_pClass));
        }

        /// <summary>
        /// Gets a pointer to the vftable of a class.
        /// </summary>
        /// <param name="in_pClass">The class to get the vftable pointer from.</param>
        public nuint GetVftableFromClass(nuint in_pClass)
        {
            return _reader.ReadPointer(in_pClass);
        }

        /// <summary>
        /// Gets the name of a class from its pointer.
        /// </summary>
        /// <param name="in_pClass">The pointer to the class where the first member is a pointer back to the vftable which has RTTI.</param>
        /// <param name="in_isDemangled">Determines whether to demangle the MSVC name of the class.</param>
        /// <param name="in_demanglerFlags">The flags for the demangler.</param>
        public string GetClassName(nuint in_pClass, bool in_isDemangled = true, IEnumerable<EDemanglerFlags> in_demanglerFlags = null)
        {
            var pRuntimeInfo = GetRuntimeInfoFromClass(in_pClass);

            if (pRuntimeInfo == null)
                return string.Empty;

            var pTypeDescriptor = pRuntimeInfo.GetTypeDescriptor();

            if (pTypeDescriptor == null)
                return string.Empty;

            return pTypeDescriptor.GetName(false, in_isDemangled, in_demanglerFlags);
        }

        /// <summary>
        /// Gets the namespaces of a class from its pointer.
        /// </summary>
        /// <param name="in_pClass">The pointer to the class where the first member is a pointer back to the vftable which has RTTI.</param>
        public string[] GetClassNamespaces(nuint in_pClass)
        {
            return GetClassName(in_pClass).Split(new[] { "::" }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
