namespace Lithium.Core.Semver;

public interface ISemverSatisfies
{
    bool Satisfies(Semver semver);
}
