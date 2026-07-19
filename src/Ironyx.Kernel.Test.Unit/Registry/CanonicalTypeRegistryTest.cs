using Ironyx.Kernel.Registry;
using Ironyx.Kernel.Test.Features;

namespace Ironyx.Kernel.Test.Unit.Registry
{
    public class CanonicalTypeRegistryTest
    {
        private CanonicalTypeRegistry CreateSUT()
        {
            return new CanonicalTypeRegistry();
        }

        [Fact(DisplayName = "[UNIT][CTR-001]: Registrate Command")]
        [GrpcEndpointFeature]
        public void CanonicalTypeRegistry_Add_RegistrateCommand()
        {
            // Arrange
            var sut = CreateSUT();

            // Act
            sut.Add(typeof(TestCommand));

            // Assert
            Assert.Single(sut.Registrations, r => typeof(TestCommand).FullName.Equals(r.Key.Type)
                                                && r.Key.Version.Equals("v1")
                                                && r.Value.Equals(typeof(TestCommand)));
        }

        [Fact(DisplayName = "[UNIT][CTR-002]: Version is Not Defined")]
        [GrpcEndpointFeature]
        public void CanonicalTypeRegistry_Add_VersionIsNotDefined()
        {
            // Arrange
            var sut = CreateSUT();

            // Act
            // Assert
            Assert.Throws<ArgumentException>(() => sut.Add(typeof(TestCommandWithoutVersion)));
        }

        [Fact(DisplayName = "[UNIT][CTR-003]: Add Type with Different Version")]
        [GrpcEndpointFeature]
        public void CanonicalTypeRegistry_Add_AddTypeWithDifferentVersion()
        {
            // Arrange
            var sut = CreateSUT();

            sut.Add(typeof(TestCommand));

            // Act
            sut.Add(typeof(V2.TestCommand));

            // Assert
            Assert.Collection(sut.Registrations, r => r.Validate<TestCommand>("v1"), r => r.Validate<V2.TestCommand>("v2"));
        }

        [Fact(DisplayName = "[UNIT][CTR-004]: Add Same Type")]
        [GrpcEndpointFeature]
        public void CanonicalTypeRegistry_Add_AddSameType()
        {
            // Arrange
            var sut = CreateSUT();

            sut.Add(typeof(TestCommand));

            // Act
            // Assert
            Assert.Throws<InvalidOperationException>(() => sut.Add(typeof(TestCommand)));
        }

        [Fact(DisplayName = "[UNIT][CTR-005]: Resolve Runtime Type")]
        [GrpcEndpointFeature]
        public void CanonicalTypeRegistry_Resolve_ResolveRunetimeType()
        {
            // Arrange
            var sut = CreateSUT();

            sut.Add(typeof(TestCommand));

            // Act
            var type = sut[typeof(TestCommand).FullName!, "v1"];

            // Assert
            Assert.Equal(typeof(TestCommand), type);
        }

        [Fact(DisplayName = "[UNIT][CTR-006]: Attempt to Resolve Unknown Type")]
        [GrpcEndpointFeature]
        public void CanonicalTypeRegistry_Resolve_AttemptToResolveUnkownType()
        {
            // Arrange
            var sut = CreateSUT();

            // Act
            // Assert
            Assert.Throws<NotSupportedException>(() => sut[typeof(TestCommand).FullName!, "v1"]);
        }
    }

    [RequestVersion("v1")]
    file record TestCommand : Command { }

    file record TestCommandWithoutVersion : Command { }

    file static class CanonicalTypeRegistryTestExtensions
    {
        public static void Validate<TCommand>(this KeyValuePair<CanonicalTypeDescription, Type> registration, string version)
            where TCommand : Command
        {
            Assert.Equal(typeof(TCommand).FullName, registration.Key.Type);
            Assert.Equal(version, registration.Key.Version);
            Assert.Equal(typeof(TCommand), registration.Value);
        }
    }
}

namespace Ironyx.Kernel.Test.Unit.Registry.V2
{
    [RequestVersion("v2")]
    file record TestCommand : Command { }
}