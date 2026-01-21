using System.Numerics;
using FluentAssertions;
using Lithium.Core.Extensions;

namespace Lithium.Core.Tests.Extensions;

public class VectorExtensionsTests
{
    [Fact]
    public void RotateY_ShouldRotateVectorCorrectly()
    {
        var v = new Vector3(1, 0, 0);
        var rotated = v.RotateY(90);
        
        // Rotating (1,0,0) 90 degrees clockwise around Y should result in (0,0,1) or (0,0,-1) depending on coordinate system.
        // Our implementation:
        // Clockwise: x' = x*cos - z*sin, z' = x*sin + z*cos
        // cos(90) = 0, sin(90) = 1
        // x' = 1*0 - 0*1 = 0
        // z' = 1*1 + 0*0 = 1
        // Result (0, 0, 1)
        
        rotated.X.Should().BeApproximately(0f, 0.0001f);
        rotated.Z.Should().BeApproximately(1f, 0.0001f);
    }

    [Fact]
    public void DistanceHorizontal_ShouldIgnoreY()
    {
        var a = new Vector3(0, 0, 0);
        var b = new Vector3(3, 100, 4);
        
        a.DistanceHorizontal(b).Should().Be(5f); // Sqrt(3^2 + 4^2) = 5
    }
}
