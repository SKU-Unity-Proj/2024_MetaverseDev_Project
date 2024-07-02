using UnityEngine;

public interface IHDraggable : IHandIKTarget, ICharacterTargetPos
{
    void HStartDrag();
    void HStopDrag();

    bool HMove(Vector3 velocity);
}