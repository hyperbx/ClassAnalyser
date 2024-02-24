using ClassAnalyser.Analysis.RTTI.Types;
using System.Text;
using System.Text.RegularExpressions;

namespace ClassAnalyser.Analysis.RTTI.Syntax
{
    public class Class
    {
        /// <summary>
        /// The declared name of this class.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The declared namespaces of this class (omitted from <see cref="Name"/>).
        /// </summary>
        public List<string> Namespaces { get; } = [];

        /// <summary>
        /// The template variables for this class.
        /// </summary>
        public List<Class> Templates { get; } = [];

        /// <summary>
        /// The base classes this class derives.
        /// </summary>
        public List<Class> BaseClasses { get; } = [];

        /// <summary>
        /// The descriptor of this base class (if it is one).
        /// </summary>
        public BaseClassDescriptor BaseClassDescriptor { get; private set; } = null;

        /// <summary>
        /// Determines whether this class has namespaces.
        /// </summary>
        public bool HasNamespaces => Namespaces.Count > 0;

        /// <summary>
        /// Determines whether this class has template variables.
        /// </summary>
        public bool HasTemplates => Templates.Count > 0;

        /// <summary>
        /// Determines whether this class has base classes.
        /// </summary>
        public bool HasBaseClasses => BaseClasses.Count > 0;

        /// <summary>
        /// Determines whether this class is a template.
        /// </summary>
        public bool IsTemplate { get; private set; }

        /// <summary>
        /// Determines whether this class is a base class.
        /// </summary>
        public bool IsBaseClass => BaseClassDescriptor != null;

        public Class() { }

        /// <summary>
        /// Parses class information from a declaration string.
        /// </summary>
        /// <param name="in_fullName">The full declared name of the class (including namespaces and templates).</param>
        public Class(string in_fullName)
        {
            var namespaces = ParseNamespaces(in_fullName);

            for (int i = 0; i < namespaces.Length; i++)
            {
                var @namespace = namespaces[i];

                if (@namespace.Contains('<') && @namespace.EndsWith('>'))
                {
                    var templatesStart = @namespace.LastIndexOf('<');
                    var templates = @namespace[templatesStart..].Split(',');

                    foreach (var template in templates)
                        Templates.Add(new Class(template.Trim().TrimEnd('>')) { IsTemplate = true });

                    @namespace = @namespace[..templatesStart];
                }

                Namespaces.Add(@namespace);
            }

            Name = Namespaces.LastOrDefault();

            if (!HasNamespaces)
                return;

            // Remove last namespace since it'll be the declared name.
            Namespaces.RemoveAt(Namespaces.Count - 1);
        }

        public Class(BaseClassDescriptor in_baseClassDescriptor) : this(in_baseClassDescriptor.GetTypeDescriptor().GetName())
        {
            BaseClassDescriptor = in_baseClassDescriptor;

            for (int i = 1; i <= in_baseClassDescriptor.SubElementCount; i++)
            {
                var @base = in_baseClassDescriptor.GetClassHierarchyDescriptor().GetBaseClass(i);

                if (@base.SubElementCount > 0)
                {
                    var @class = new Class(@base);

                    i += @class.GetTotalBaseClasses();

                    BaseClasses.Add(@class);

                    continue;
                }

                BaseClasses.Add(new Class(@base));
            }
        }

        public Class(CompleteObjectLocator in_completeObjectLocator)
            : this(in_completeObjectLocator.GetClassHierarchyDescriptor().GetBaseClass(0)) { }

        public int GetTotalBaseClasses()
        {
            var result = BaseClasses.Count;

            foreach (var @base in BaseClasses)
                result += @base.GetTotalBaseClasses();

            return result;
        }

        private static string[] ParseNamespaces(string in_str)
        {
            var pattern = @"(?<part>[^<:]+(<[^>]+>)?)(?:::|$)";
            var matches = Regex.Matches(in_str, pattern);
            var results = new string[matches.Count];

            for (int i = 0; i < matches.Count; i++)
                results[i] = matches[i].Value.Trim(':');

            return results;
        }

        public string ToHeader()
        {
            var result = string.Empty;
            var namespaces = string.Join("::", Namespaces);

            if (HasNamespaces)
                result += $"namespace {namespaces}\n{{\n";

            var tab = HasNamespaces ? "    " : "";

            if (HasTemplates)
            {
                var templateDecl = $"{tab}template<";

                if (Templates.Count == 1)
                {
                    templateDecl += "typename T";
                }
                else
                {
                    for (int i = 0; i < Templates.Count; i++)
                    {
                        templateDecl += $"typename T{i}";

                        if (i != Templates.Count - 1)
                            templateDecl += ", ";
                    }
                }

                templateDecl += '>';

                result += $"{templateDecl}\n";
            }

            result += $"{tab}class {Name}";

            if (HasBaseClasses)
            {
                result += " : ";

                for (int i = 0; i < BaseClasses.Count; i++)
                {
                    var baseClassName = BaseClasses[i].Name;

                    // Remove namespaces from class name if we're in the same namespace.
                    if (baseClassName.StartsWith(namespaces))
                        baseClassName = baseClassName[namespaces.Length..].Trim(':');

                    result += $"public {baseClassName}";

                    if (i < BaseClasses.Count - 1)
                        result += ", ";
                }
            }

            result += $"\n{tab}{{\n\n{tab}}};\n";

            if (HasNamespaces)
                result += "}\n";

            if (HasBaseClasses)
            {
                var sb = new StringBuilder();

                foreach (var @base in BaseClasses)
                    sb.AppendLine($"#include <{string.Join('\\', @base.Namespaces)}\\{@base.Name}.h>");

                result = $"{sb}\n{result}";
            }

            return "#pragma once\n\n" + result;
        }

        public override string ToString()
        {
            var result = $"{string.Join("::", Namespaces)}::{Name}";

            if (IsBaseClass)
                return BaseClassDescriptor.ToString();

            if (IsTemplate)
                return result;

            if (HasTemplates)
                result += $"<{string.Join(", ", Templates)}>";

            return result;
        }
    }
}
