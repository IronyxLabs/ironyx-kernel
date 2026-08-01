using Xunit.Sdk;

namespace Ironyx.Testing
{
    [TraitDiscoverer("Ironyx.Testing.Helpers.FeatureTraitDiscoverer", "Ironyx.Testing")]
    [AttributeUsage(AttributeTargets.Method)]
    public class FeatureAttribute : Attribute, ITraitAttribute
    {
        public string Abbreviation { get; }
        public string Feature { get; }

        public FeatureAttribute(string abbreviation, string feature)
        {
            Abbreviation = abbreviation;
            Feature = feature;
        }
    }
}
