using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ThomsonReuters.Languages;
using ThomsonReuters.Languages.TypesLanguage;
using iSynaptic.Commons;
using iSynaptic.Commons.Collections.Generic;
using iSynaptic.Commons.Linq;
using ISymbol = ThomsonReuters.Languages.ISymbol;

namespace ThomsonReuters.CodeGeneration
{
    public abstract class MoleculeCodeGenerator : CodeGenerator
    {
        protected MoleculeCodeGenerator(StringBuilder sb) : base(sb)
        {
        }

        protected MoleculeCodeGenerator(TextWriter writer) : base(writer)
        {
        }

        protected virtual void WriteMolecule(MoleculeSymbol molecule)
        {
            Write("public");
            if(molecule.IsAbstract) Write(" abstract");
            Write(" class {0}", GetMoleculeClassName(molecule, molecule));

            var baseClassName = GetBaseClassName(molecule, molecule);
            
            baseClassName.Run(x => Write(" : {0}", x));

            if (GenerateEquavalence)
            {
                Write(baseClassName.HasValue ? ", " : " : ");
                Write("IEquatable<{0}>", GetMoleculeClassName(molecule, molecule));
            }

            WriteLine();

            using (WithBlock())
            {
                WriteConstructor(molecule);
                WriteEssenceBuilderImplementation(molecule);
                WriteEquavalenceImplementation(molecule);

                foreach (var property in molecule.Properties)
                    WritePropertyImplmementation(property, molecule);
            }

            WriteLine();
        }

        protected virtual void WriteConstructor(MoleculeSymbol molecule)
        {
            Write("{0} {1}(", molecule.IsAbstract ? "protected" : "public", GetMoleculeClassName(molecule, molecule));

            var baseProperties = GetBaseProperties(molecule);
            var allProperties = molecule.Properties
                .Concat(baseProperties);

            Write(allProperties.Delimit(", ", x => string.Format("{0} {1}", GetPublicTypeString(x, molecule), Inflector.Camelize(x.Name))));
            WriteLine(")");

            if (baseProperties.Length > 0)
            {
                using (WithIndentation())
                    WriteLine(": base({0})", baseProperties.Delimit(", ", x => Inflector.Camelize(x.Name)));
            }

            using (WithBlock())
                WritePropertyAssignmentAndValidation(molecule.Properties);
        }

        protected virtual void WriteEquavalenceImplementation(MoleculeSymbol molecule)
        {
            string moleculeClassName = GetMoleculeClassName(molecule, molecule);

            if (!GenerateEquavalence)
                return;

            WriteLine();
            WriteLine("public bool Equals({0} other)", moleculeClassName);
            using (WithBlock())
            {
                WriteLine("if (ReferenceEquals(other, null)) return false;");
                WriteLine("if (GetType() != other.GetType()) return false;");
                WriteLine();

                foreach (var property in molecule.Properties)
                {
                    bool isOptional = property.Cardinality.IsZeroOrMore();
                    bool isMany = property.Cardinality.CanBeMoreThanOne();
                    bool isValueType = property.Type is ExternalType && ((ExternalType)property.Type).Type.IsValueType;

                    if (isOptional && !isMany)
                    {
                        WriteLine(isValueType
                                ? "if ({0}.HasValue != other.{0}.HasValue) return false;"
                                : "if (ReferenceEquals({0}, null) != ReferenceEquals(other.{0}, null)) return false;",
                            property.Name);
                    }

                    if (!isMany)
                    {
                        string prefix = "";
                        if (isOptional)
                        {
                            prefix = string.Format(isValueType
                                    ? "{0}.HasValue && "
                                    : "!ReferenceEquals({0}, null) && ",
                                property.Name);
                        }

                        WriteLine("if({0}!{1}.Equals(other.{1})) return false;", prefix, property.Name);
                    }
                    else
                        WriteLine("if(!{0}.SequenceEqual(other.{0})) return false;", property.Name);

                    WriteLine();
                }

                WriteLine(
                    molecule.Base
                        .Select(x => string.Format("return base.Equals(({0})other);", GetMoleculeClassName(x)))
                        .ValueOrDefault("return true;"));
            }

            WriteLine();

            WriteLine("public override bool Equals(object obj)");
            using (WithBlock())
            {
                WriteLine("var other = obj as {0};", moleculeClassName);
                WriteLine("return !ReferenceEquals(other, null) && Equals(other);");
            }

            WriteLine();

            WriteLine("public override int GetHashCode()");
            using (WithBlock())
            {
                WriteLine(
                    molecule.Base
                        .Select(x => string.Format("int hash = base.GetHashCode();"))
                        .ValueOrDefault("int hash = 1;"));

                foreach (var property in molecule.Properties)
                {
                    bool isOptional = property.Cardinality.IsZeroOrMore();
                    bool isMany = property.Cardinality.CanBeMoreThanOne();
                    bool isValueType = property.Type is ExternalType && ((ExternalType)property.Type).Type.IsValueType;

                    if (!isMany)
                    {
                        if (isOptional)
                        {
                            WriteLine(isValueType
                                          ? "hash = Hashing.Mix(hash + ({0}.HasValue ? {0}.GetHashCode() : 0));"
                                          : "hash = Hashing.Mix(hash + (!ReferenceEquals({0}, null) ? {0}.GetHashCode() : 0));",
                                      property.Name);
                        }
                        else
                            WriteLine("hash = Hashing.Mix(hash + {0}.GetHashCode());", property.Name);
                    }
                    else
                        WriteLine("hash = {0}.Aggregate(hash, (h, item) => Hashing.Mix(h + item.GetHashCode()));", property.Name);
                }

                WriteLine("return hash;");
            }

            WriteLine();

            WriteLine("public static bool operator ==({0} left, {0} right)", moleculeClassName);
            using (WithBlock())
            {
                WriteLine("if (ReferenceEquals(left, null) != ReferenceEquals(right, null)) return false;");
                WriteLine("return ReferenceEquals(left, null) || left.Equals(right);");
            }

            WriteLine();

            WriteLine("public static bool operator !=({0} left, {0} right)", moleculeClassName);
            using (WithBlock())
            {
                WriteLine("return !(left == right);");
            }
        }

        protected virtual void WriteEssenceBuilderImplementation(MoleculeSymbol molecule)
        {
            if (!GenerateEssence)
                return;

            var baseValues = molecule.Base
                .Select(x => x.Recurse(y => y.Base))
                .Squash()
                .ToArray();

            var baseProperties = baseValues
                .SelectMany(x => x.Properties)
                .ToArray();

            var properties = molecule.Properties.Concat(baseProperties).ToArray();

            if (!molecule.Properties.Any() && molecule.IsAbstract)
                return;

            Func<MoleculeSymbol, bool> needsEssence = x => x.Properties.Any();

            var closestBase = baseValues.TryFirst(needsEssence);
            var furthestBase = baseValues.TryLast(needsEssence);

            WriteLine();
            WriteLine("public {0}{1}class Essence{2}", 
                closestBase.HasValue ? "new " : "", 
                molecule.IsAbstract ? "abstract " : "", 
                closestBase.Select(x => string.Format(" : {0}.Essence", GetMoleculeClassName(x, molecule))).ValueOrDefault(""));

            using (WithBlock())
            {
                foreach (var property in molecule.Properties)
                {
                    string publicType = GetPublicTypeString(property, molecule);
                    WriteLine("public {0} {1} {{ protected get; set; }}", publicType, property.Name);
                }

                WriteLine();

                WriteLine("public {0}{1} Create()", closestBase.HasValue ? "new " : "", GetMoleculeClassName(molecule, molecule));
                using (WithBlock())
                {
                    WriteLine("return ({0})CreateValue();", GetMoleculeClassName(molecule, molecule));
                }

                WriteLine();
                
                if (!molecule.IsAbstract)
                {
                    WriteLine("protected {0} {1} CreateValue()", furthestBase.HasValue ? "override" : "virtual", furthestBase.Or(molecule).Select(x => GetMoleculeClassName(x, molecule).ToString()).Value);
                    using (WithBlock())
                    {
                        WriteLine("return new {0}({1});", GetMoleculeClassName(molecule, molecule), properties.Delimit(", ", x => x.Name));
                    }
                }
                else if(!furthestBase.HasValue)
                    WriteLine("protected abstract {0} CreateValue();", GetMoleculeClassName(molecule, molecule));
            }

            WriteLine();
            WriteLine("public {0}Essence ToEssence()", closestBase.HasValue ? "new " : "");
            using (WithBlock())
                WriteLine("return (Essence)CreateEssence();");

            WriteLine();

            if (!molecule.IsAbstract)
            {
                WriteLine("protected {0} {1}.Essence CreateEssence()", furthestBase.HasValue ? "override" : "virtual",
                          furthestBase.Or(molecule).Select(x => GetMoleculeClassName(x, molecule).ToString()).Value);
                using (WithBlock())
                {
                    WriteLine("return new Essence");
                    using (WithBlock(withStatementEnd: true))
                    {
                        WriteLine(properties.Delimit(",\r\n", x => string.Format("{0} = {0}", x.Name)));
                    }
                }
            }
            else if(!furthestBase.HasValue)
                WriteLine("protected abstract {0}.Essence CreateEssence();", GetMoleculeClassName(molecule, molecule));
        }

        protected virtual void WritePropertyAssignmentAndValidation(IEnumerable<AtomSymbol> arguments)
        {
            foreach (var argument in arguments)
            {
                string argName = Inflector.Camelize(argument.Name);
                string fieldName = "_" + Inflector.Camelize(argument.Name);

                bool isOptional = argument.Cardinality.Minimum == 0;
                bool isMany = argument.Cardinality.CanBeMoreThanOne();
                bool isValueType = (argument.Type is ExternalType && ((ExternalType)argument.Type).Type.IsValueType) ||
                                   argument.Type is ExternalEnumSymbol;

                if (isMany)
                {
                    WriteLine("if({0} == null) throw new ArgumentNullException(\"{0}\");", argName);
                    WriteLine("{0} = {1}.ToArray();", fieldName, argName);

                    if (!isValueType)
                    {
                        WriteLine("if({0}.Any(_ => _ == null))", fieldName, argument.Cardinality.Minimum);
                        using (WithIndentation())
                            WriteLine("throw new ArgumentException(\"{0} cannot contain null values.\", \"{1}\");", Inflector.Humanize(Inflector.Titleize(argument.Name)), argName);
                    }

                    if (argument.Cardinality.Maximum.HasValue)
                    {
                        WriteLine("if({0}.Length > {1})", fieldName, argument.Cardinality.Maximum.Value);
                        using (WithIndentation())
                            WriteLine("throw new ArgumentException(\"You cannot provide more than {0} {1}.\", \"{2}\");", argument.Cardinality.Maximum.Value, (Inflector.Titleize(argument.Cardinality.Maximum.Value == 1 ? Inflector.Singularize(argument.Name) : argument.Name.ToString())).ToLower(), argName);
                    }
                }
                else
                    WriteLine("{0} = {1};", fieldName, argName);

                if (!isOptional)
                {
                    if (isMany)
                    {
                        WriteLine("if({0}.Length < {1})", fieldName, argument.Cardinality.Minimum);
                        using (WithIndentation())
                            WriteLine("throw new ArgumentException(\"You must provide at least {0} {1}.\", \"{2}\");", argument.Cardinality.Minimum, (Inflector.Titleize(argument.Cardinality.Minimum == 1 ? Inflector.Singularize(argument.Name) : argument.Name.ToString())).ToLower(), argName);

                    }
                    else if (!isValueType)
                    {
                        WriteLine("if({0} == null)", fieldName);
                        using (WithIndentation())
                            WriteLine("throw new ArgumentNullException(\"{1}\");", argument.Cardinality.Minimum, argName);
                    }
                }

                WriteLine();
            }
        }

        protected virtual void WritePropertyImplmementation(AtomSymbol property, MoleculeSymbol parent)
        {
            string privateType = GetPrivateTypeString(property, parent);
            string publicType = GetPublicTypeString(property, parent);
            string fieldName = Inflector.Camelize(property.Name);
            bool isMany = property.Cardinality.CanBeMoreThanOne();
            Maybe<string> description = GetPropertyDescription(property);

            WriteLine();
            WriteLine("private {0} _{1};", privateType, fieldName);
            if (description.HasValue)
            {
                WriteLine("/// <summary>");
                WriteLine("/// " + description.Value);
                WriteLine("/// </summary>");
            }
            WriteLine("public {0} {1} {{ get {{ return _{2}{3}; }} private set {{ _{2} = value{4}; }} }}", publicType, property.Name, fieldName, isMany ? ".AsEnumerable()" : "", isMany ? ".ToArray()" : "");
        }

        private Maybe<string> GetPropertyDescription(AtomSymbol property)
        {
            return property.Annotations
                .TryGetValue("Description")
                .Select(x => x.SingleOrDefault())
                .SelectMaybe(x => x.Pairs.TrySingle(y => y.Key == "Summary"))
                .Select(x => x.Value.Value);
        }

        protected virtual string GetMoleculeClassName(MoleculeSymbol molecule, ISymbol relativeTo = null)
        {
            return GetRelativeName(molecule.FullName, relativeTo != null ? relativeTo.FullName : null);
        }

        protected virtual AtomSymbol[] GetBaseProperties(MoleculeSymbol molecule)
        {
            return molecule.Base
                .Select(x => x.Recurse(y => y.Base))
                .ValueOrDefault(Enumerable.Empty<ComplexValueSymbol>())
                .SelectMany(x => x.Properties)
                .ToArray();
        }

        protected virtual Maybe<string> GetBaseClassName(MoleculeSymbol molecule, ISymbol relativeTo = null)
        {
            return molecule.Base.Select(x => GetMoleculeClassName(x, relativeTo));
        }

        protected virtual bool GenerateEquavalence { get { return true; } }
        protected virtual bool GenerateEssence { get { return true; } }
    }
}