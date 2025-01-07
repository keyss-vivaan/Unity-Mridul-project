using System;

public abstract class ARState
{
    protected StateManager arManager;

    public ARState(StateManager manager)
    {
        arManager = manager;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update() { }

    public virtual void OnUndo() { }
    public virtual void OnFinishFloor() { }
    public virtual void OnFinishWindows() { }
    public virtual void OnSetHeight() { }

    public virtual void Visualize() { }


}
