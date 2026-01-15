using Lithium.Codecs.Primitives;
using Microsoft.Extensions.DependencyInjection;

namespace Lithium.Codecs.Tests;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void AddLithiumCodecs_Should_RegisterAllPrimitiveCodecs()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLithiumCodecs();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var registry = serviceProvider.GetService<ICodecRegistry>();

        // Assert
        Assert.NotNull(registry);
        
        Assert.IsType<IntegerCodec>(registry.Get<int>());
    }

    [Fact]
    public void TryGet_Finds_RegisteredCodec()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLithiumCodecs();
        var serviceProvider = services.BuildServiceProvider();
        var registry = serviceProvider.GetRequiredService<ICodecRegistry>();

        // Act
        var result = registry.TryGet<int>(out var codec);

        // Assert
        Assert.True(result);
        Assert.NotNull(codec);
        Assert.IsType<IntegerCodec>(codec);
    }

    [Fact]
    public void TryGet_DoesNotFind_UnregisteredCodec()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLithiumCodecs();
        var serviceProvider = services.BuildServiceProvider();
        var registry = serviceProvider.GetRequiredService<ICodecRegistry>();

        // Act
        var result = registry.TryGet<Guid>(out var codec);

        // Assert
        Assert.False(result);
        Assert.Null(codec);
    }
}
