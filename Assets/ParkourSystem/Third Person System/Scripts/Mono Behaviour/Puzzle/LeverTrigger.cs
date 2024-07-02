using UnityEngine;

namespace DiasGames.Puzzle
{
    public class LeverTrigger : MonoBehaviour, IPushable
    {
        [SerializeField] private Transform targetCharacterTransform = null;
        [SerializeField] private Transform rightIK = null;
        [SerializeField] private Transform leftIK = null;
        [Space]
        [SerializeField] private Collider characterRefCollider;
        public Transform playerTarget;

        private PushableObject _block = null;

        private void Awake()
        {
            _block = GetComponentInParent<PushableObject>();
            characterRefCollider.enabled = false;
        }

        public void StartPush()
        {
            _block.EnablePhysics();
            //characterRefCollider.enabled = true;
        }

        public void StopPush()
        {
            _block.DisablePhysics();
            //characterRefCollider.enabled = false;
        }

        public void SetPushState(bool pushing)
        {
            if (pushing)
                _block.EnablePhysics();
            else
                _block.DisablePhysics();
        }


        #region IHandIk & ICharacterTargetPos

        public Transform GetLeftHandTarget()
        {
            return leftIK;
        }

        public Transform GetRightHandTarget()
        {
            return rightIK;
        }

        public Transform GetTarget()
        {
            playerTarget.position = targetCharacterTransform.position;
            return targetCharacterTransform;
        }

        #endregion
    }
}