using ClassAnalyser.Analysis.RTTI.Syntax;
using ClassAnalyser.Helpers;
using System.Text;

namespace ClassAnalyser.Analysis.RTTI.Types
{
    public class CompleteObjectLocator(IRTTIReader in_reader, nuint in_pThis)
    {
        protected IRTTIReader _reader = in_reader;

        /// <summary>
        /// The location of this complete object locator.
        /// </summary>
        public nuint pThis { get; } = in_pThis;

        /// <summary>
        /// The signature of this class.
        /// </summary>
        public int Signature { get; } = in_reader.Read<int>(in_pThis);

        /// <summary>
        /// The offset of the vftable for this class.
        /// </summary>
        public int VftableOffset { get; } = in_reader.Read<int>(in_pThis + 0x04);

        /// <summary>
        /// The displacement offset of the constructor for this class.
        /// </summary>
        public int CtorDisplacementOffset { get; } = in_reader.Read<int>(in_pThis + 0x08);

        /// <summary>
        /// The location of the type descriptor for this class.
        /// </summary>
        public uint pTypeDescriptor { get; } = in_reader.Read<uint>(in_pThis + 0x0C);

        /// <summary>
        /// The location of the class hierarchy descriptor for this class.
        /// </summary>
        public uint pClassHierarchyDescriptor { get; } = in_reader.Read<uint>(in_pThis + 0x10);

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

        /// <summary>
        /// Exports this class and its base classes into C++ headers.
        /// </summary>
        /// <param name="in_path">The path to export to.</param>
        /// <param name="in_isOverwrite">Determines whether the remote file can be overwritten if it already exists.</param>
        public void ExportHeaders(string in_path, bool in_isExportBaseClasses = true, bool in_isOverwrite = false)
        {
            foreach (var @base in GetClassHierarchyDescriptor().GetBaseClasses())
            {
                var @class = new Class(@base);

                var dir = Path.Combine(in_path, Path.Combine([.. @class.Namespaces]));
                var path = Path.Combine(dir, $"{@class.Name}.h");

                if (!in_isOverwrite && File.Exists(path))
                    throw new IOException($"The destination file already exists: {path}");

                Directory.CreateDirectory(dir);

                File.WriteAllText(path, @class.ToHeader());

                if (!in_isExportBaseClasses)
                    return;
            }
        }

        public Class ToClassSyntax()
        {
            return new(this);
        }

        /// <summary>
        /// Gets formatted information about this class.
        /// </summary>
        public string GetClassInfo()
        {
            var result = new StringBuilder();

            var typeDesc = GetTypeDescriptor();
            var hierarchy = GetClassHierarchyDescriptor();

            result.AppendLine(ToString());
            result.AppendLine($"  {typeDesc}");
            result.AppendLine($"    {hierarchy}");
            result.AppendLine($"      {typeDesc.GetName()}::`RTTI Base Class Array'");
            result.AppendLine($"        {ToClassSyntax().GetHierarchyInfo().Replace("\n", "\n        ").Trim()}");

            return result.ToString();
        }

        /// <summary>
        /// Determines if this RTTI information is valid.
        /// </summary>
        public bool IsValid()
        {
            return !GetTypeDescriptor().GetName().IsNullOrEmptyOrWhiteSpace();
        }

        public override string ToString()
        {
            return $"const {GetTypeDescriptor().GetName()}::`RTTI Complete Object Locator'";
        }
    }
}
