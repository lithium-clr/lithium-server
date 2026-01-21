namespace Lithium.Server.Core.Semver;

public interface ISemverSatisfies
{
    bool Satisfies(Semver semver);
}