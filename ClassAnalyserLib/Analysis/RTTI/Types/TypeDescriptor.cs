using ClassAnalyser.Analysis.MSVC;
using ClassAnalyser.Analysis.RTTI.Syntax;

namespace ClassAnalyser.Analysis.RTTI.Types
{
    public class TypeDescriptor(IRTTIReader in_reader, nuint in_pThis)
    {
        protected IRTTIReader _reader = in_reader;

        /// <summary>
        /// The location of this type descriptor.
        /// </summary>
        public nuint pThis { get; } = in_pThis;

        /// <summary>
        /// The location of the vftable for RTTI.
        /// </summary>
        public nuint pTypeInfoVftable { get; } = in_reader.ReadPointer(in_pThis);

        public nuint pRuntimeRef { get; } = in_reader.ReadPointer(in_pThis + in_reader.GetPointerSize());

        /// <summary>
        /// Gets the declared name of this class.
        /// </summary>
        /// <param name="in_isNameOnly">Determines whether the namespaces should be omitted.</param>
        /// <param name="in_isDemangled">Determines whether the output should be demangled.</param>
        /// <param name="in_demanglerFlags">The flags for the demangler.</param>
        public string GetName(bool in_isNameOnly = false, bool in_isDemangled = true, IEnumerable<EDemanglerFlags> in_demanglerFlags = null)
        {
            var mangledName = _reader.ReadStringNullTerminated(pThis + (_reader.GetPointerSize() * 2));

            if (string.IsNullOrEmpty(mangledName))
                return string.Empty;

            var result = string.Empty;

            if (in_isDemangled)
            {
                result = Demangler.GetUndecoratedName(mangledName, in_demanglerFlags ?? [ EDemanglerFlags.NameOnly ]);
            }
            else
            {
                return mangledName;
            }

            if (!in_isNameOnly)
                return result;

            return new Class(result).Name;
        }

        /// <summary>
        /// Gets all namespaces from this class' name.
        /// </summary>
        public string[] GetNamespaces()
        {
            return [.. new Class(GetName()).Namespaces];
        }

        public override string ToString()
        {
            return $"class {GetName()} `RTTI Type Descriptor'";
        }
    }
}
