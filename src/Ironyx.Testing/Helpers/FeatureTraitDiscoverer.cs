using Xunit.Abstractions;
using Xunit.Sdk;

namespace Ironyx.Testing.Helpers
{
    public class FeatureTraitDiscoverer : ITraitDiscoverer
    {
        public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
        {
            string label = "NOT DEFINED";
            var attributeInfo = traitAttribute as ReflectionAttributeInfo;
            var featureAttribute = attributeInfo?.Attribute as FeatureAttribute;
            if (featureAttribute != null)
            {
                label = $"{featureAttribute.Abbreviation} - {featureAttribute.Feature}";
            }

            yield return new KeyValuePair<string, string>("Feature", label);
        }
    }
}
