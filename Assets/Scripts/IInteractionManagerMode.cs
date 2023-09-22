using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractionManagerMode
{
    public void Activate();
    public void Deactivate();
    public void TouchInteraction(Touch[] touches);
}
