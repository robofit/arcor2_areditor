using Unity;
using Base;

public abstract class InteractiveObject : Clickable {

    public abstract string GetName();
    public abstract string GetId();
    public abstract void OpenMenu();
    public abstract bool HasMenu();
    public abstract bool Movable();
    public abstract void StartManipulation();
}
