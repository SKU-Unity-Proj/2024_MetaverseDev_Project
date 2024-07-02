using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiasGames.Components
{
    // Ragdoll 컴포넌트: 캐릭터가 사망 시 ragdoll 물리 효과를 적용
    public class Ragdoll : MonoBehaviour
    {
        private Health _health;          // Health 컴포넌트 참조. 캐릭터 사망 여부 확인용
        private Animator _animator;      // Animator 컴포넌트 참조. Ragdoll이 작동하려면 비활성화되어야 함

        // Ragdoll에 사용되는 Rigidbody들
        private List<Rigidbody> _ragdollRigidbodies = new List<Rigidbody>();
        // Ragdoll에 사용되는 Collider들
        private List<Collider> _ragdollColliders = new List<Collider>();

        private void Awake()
        {
            // Health 및 Animator 컴포넌트 찾기
            _health = GetComponent<Health>();
            _animator = GetComponent<Animator>();

            // Ragdoll에 필요한 Rigidbody와 Collider 참조 얻기
            GetRagdollReferences();
        }

        private void OnEnable()
        {
            // Health 컴포넌트가 있을 경우, 캐릭터 사망 시 Ragdoll 활성화 이벤트 연결
            if (_health)
                _health.OnDead += ActivateRagdoll;
        }

        private void OnDisable()
        {
            // Health 컴포넌트가 있을 경우, 이벤트 연결 해제
            if (_health)
                _health.OnDead -= ActivateRagdoll;
        }

        private void GetRagdollReferences()
        {
            // Animator 컴포넌트가 없으면 종료
            if (_animator == null) return;

            // Animator에 설정된 모든 뼈대(Bones)를 순회하면서 Rigidbody와 Collider 찾기
            for (int i = 0; i < 18; i++)
            {
                var bone = _animator.GetBoneTransform((HumanBodyBones)i);

                // Rigidbody 컴포넌트가 있으면 비활성화하고 리스트에 추가
                if (bone.TryGetComponent(out Rigidbody rb))
                {
                    rb.isKinematic = true; // 물리 효과 비활성화
                    _ragdollRigidbodies.Add(rb);
                }

                // Collider 컴포넌트가 있으면 비활성화하고 리스트에 추가
                if (bone.TryGetComponent(out Collider coll))
                {
                    coll.enabled = false;
                    _ragdollColliders.Add(coll);
                }
            }
        }

        private void ActivateRagdoll()
        {
            // Animator 컴포넌트가 없으면 종료
            if (_animator == null) return;

            // Animator 비활성화
            _animator.enabled = false;

            // 모든 Rigidbody를 활성화하여 물리 효과 적용
            _ragdollRigidbodies.ForEach(r => {
                r.isKinematic = false;
                r.useGravity = true;
            });

            // 모든 Collider를 활성화
            _ragdollColliders.ForEach(c => c.enabled = true);
        }
    }
}
