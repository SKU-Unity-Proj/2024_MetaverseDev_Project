using System;
using UnityEngine;

namespace DiasGames.Abilities
{
    public abstract class AbstractAbility : MonoBehaviour
    {
        // 우선순위를 설정하는 변수, 낮은 숫자가 더 높은 우선순위를 가짐
        [SerializeField] private int abilityPriority = 0;

        // 현재 어빌리티가 실행 중인지 여부를 나타내는 플래그
        public bool IsAbilityRunning { get; private set; }

        // 어빌리티가 중단되거나 시작될 때 발생하는 이벤트
        public event Action<AbstractAbility> abilityStopped = null;
        public event Action<AbstractAbility> abilityStarted = null;

        // 어빌리티 우선순위에 대한 게터
        public int AbilityPriority { get { return abilityPriority; } }

        // 애니메이터 컴포넌트 참조
        protected Animator _animator = null;

        // 어빌리티 시작 및 종료 시간
        public float StartTime { get; private set; } = 0;
        public float StopTime { get; private set; } = 0;

        // 캐릭터 동작을 위한 참조
        protected CharacterActions _action;

        /// <summary>
        /// 캐릭터 동작에 대한 참조를 설정하는 메소드
        /// </summary>
        /// <param name="newAction">새로운 CharacterActions 참조</param>
        public void SetActionReference(ref CharacterActions newAction)
        {
            _action = newAction;
        }

        protected virtual void Start()
        {
            // 애니메이터 컴포넌트를 가져옴
            _animator = GetComponent<Animator>();
        }

        public void StartAbility()
        {
            // 어빌리티를 시작하는 메소드
            IsAbilityRunning = true;
            StartTime = Time.time;
            OnStartAbility();
            abilityStarted?.Invoke(this);
        }
        public void StopAbility()
        {
            // 너무 짧은 시간 내에 어빌리티를 중단하려는 시도를 방지
            if (Time.time - StartTime < 0.1f)
                return;

            // 어빌리티를 중단하는 메소드
            IsAbilityRunning = false;
            StopTime = Time.time;
            OnStopAbility();
            abilityStopped?.Invoke(this);
        }

        // 어빌리티를 실행할 준비가 되었는지 확인하는 추상 메소드
        public abstract bool ReadyToRun();

        // 어빌리티 시작 시 실행되는 추상 메소드
        public abstract void OnStartAbility();

        // 매 프레임마다 어빌리티를 업데이트하는 추상 메소드
        public abstract void UpdateAbility();

        // 어빌리티 중단 시 실행되는 가상 메소드
        public virtual void OnStopAbility() { }

        // 애니메이션 상태를 설정하는 보조 메소드
        protected void SetAnimationState(string stateName, float transitionDuration = 0.1f, int StateLayer = 0)
        {
            if (_animator.HasState(StateLayer, Animator.StringToHash(stateName)))
            {
                _animator.CrossFadeInFixedTime(stateName, transitionDuration, StateLayer);
            }
                
        }

        public void SetLayerPriority(int StateLayer = 1, int Priority = 1) // 애니메이터의 레이어 우선순위 값(무게) 설정
        {
            _animator.SetLayerWeight(StateLayer, Priority);
        }

        /// <summary>
        /// 특정 상태의 애니메이션이 끝났는지 확인하는 메소드
        /// </summary>
        /// <param name="state">확인할 애니메이션 상태의 이름</param>
        /// <returns>애니메이션이 끝났는지 여부</returns>
        protected bool HasFinishedAnimation(string state)
        {
            var stateInfo = _animator.GetCurrentAnimatorStateInfo(0);

            // 애니메이터가 전환 중이면 false 반환
            if (_animator.IsInTransition(0)) return false;

            // 지정된 상태가 현재 상태이고, 정규화된 시간이 거의 끝나면 true 반환
            if (stateInfo.IsName(state))
            {
                float normalizeTime = Mathf.Repeat(stateInfo.normalizedTime, 1);
                if (normalizeTime >= 0.95f) return true;
            }

            return false;
        }
    }
}
