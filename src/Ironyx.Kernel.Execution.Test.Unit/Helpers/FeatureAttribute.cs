using Xunit.Sdk;

namespace Ironyx.Kernel.Execution.Test.Unit.Helpers
{
    [TraitDiscoverer("Ironyx.Kernel.Execution.Test.Unit.Helpers.FeatureTraitDiscoverer", "Ironyx.Kernel.Execution.Test.Unit")]
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
