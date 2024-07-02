using UnityEngine;

namespace DiasGames.Puzzle
{
    public class PushableObject : MonoBehaviour
    {
        private Rigidbody _rigidbody = null;

        private void Awake()
        {
            //_rigidbody = GetComponent<Rigidbody>();
        }


        public void EnablePhysics()
        {
            //_rigidbody.isKinematic = false;
            //_rigidbody.velocity = Vector3.zero;
        }

        public virtual void DisablePhysics()
        {
           // _rigidbody.isKinematic = true;
            //_rigidbody.velocity = Vector3.zero;
        }
    }
}
