using UnityEngine;

public interface IPushable : IHandIKTarget, ICharacterTargetPos
{
    void StartPush();
    void StopPush();

}