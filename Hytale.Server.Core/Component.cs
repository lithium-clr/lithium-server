namespace Hytale.Server.Core;

public interface IComponent
{
    void OnLoad();
    void OnUnload();
}

public abstract partial class Component : IComponent
{
    public virtual void OnLoad()
    {
    }

    public virtual void OnUnload()
    {
    }
}