using System.IO;
using System.Linq;
using System.Text;
using ThomsonReuters.Languages;
using ThomsonReuters.Languages.TypesLanguage;
using iSynaptic.Commons;
using iSynaptic.Commons.Linq;
using ISymbol = ThomsonReuters.Languages.ISymbol;

namespace ThomsonReuters.CodeGeneration
{
    public class EntitiesCodeGenerator : MoleculeCodeGenerator
    {
        public EntitiesCodeGenerator(StringBuilder sb)
            : base(sb)
        {
        }

        public EntitiesCodeGenerator(TextWriter writer)
            : base(writer)
        {
        }

        protected override bool NotInterestedIn(object subject)
        {
            var interestedIn = new[]
            {
                typeof (Compilation),
                typeof (NamespaceSymbol),
                typeof (EntitySymbol),
                typeof (EventSymbol)
            };

            return !(interestedIn.Contains(subject.GetType()))
                   || base.NotInterestedIn(subject);
        }

        public override void VisitEntity(EntitySymbol entity)
        {
            WriteLine("public partial class {0} : {1}", entity.Name,
                      entity.Base.Select(x => x.Name.ToString()).ValueOrDefault(
                          () => string.Format("Entity<{0}>", GetPublicTypeString(entity.IdentityType.Value, entity))));

            using (WithBlock())
                base.VisitEntity(entity);
        }

        public override void VisitEvent(EventSymbol @event)
        {
            WriteMolecule(@event);
        }

        protected override string GetMoleculeClassName(MoleculeSymbol molecule, ISymbol relativeTo = null)
        {
            return string.Format("{0}Event", molecule.Name);
        }

        protected override Maybe<string> GetBaseClassName(MoleculeSymbol molecule, ISymbol relativeTo = null)
        {
            return base.GetBaseClassName(molecule, relativeTo)
                .Or("Event");
        }

        protected override AtomSymbol[] GetBaseProperties(MoleculeSymbol molecule)
        {
            var farthestBase = (molecule.Parent as EntitySymbol).Recurse(x => x.Base).Last();

            var additionalProperty = molecule.Annotations.ContainsKey("StartEvent")
                ? new AtomSymbol(Enumerable.Empty<Annotation>(), new TypeLookup(farthestBase.IdentityType.Value.Type), new Cardinality(1, 1), "entityId")
                : new AtomSymbol(Enumerable.Empty<Annotation>(), new TypeLookup(new OpaqueType(molecule.FullName + "Event")), new Cardinality(1, 1), "parent");

            return base.GetBaseProperties(molecule)
                .Concat(new[] { additionalProperty })
                .ToArray();
        }

        protected override bool GenerateEquavalence {  get { return false; } }
        protected override bool GenerateEssence { get { return false; } }

        protected class OpaqueType : IType
        {
            public OpaqueType(QualifiedIdentifier fullName)
            {
                FullName = Guard.NotNull(fullName, "fullName");
            }

            public Identifier Name { get { return FullName.Last(); } }
            public QualifiedIdentifier FullName { get; set; }
        }

    }
}
