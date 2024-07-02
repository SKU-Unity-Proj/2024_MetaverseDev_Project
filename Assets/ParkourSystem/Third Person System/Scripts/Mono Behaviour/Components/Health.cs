using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace DiasGames.Components
{
    // 'Health' 컴포넌트 정의: 캐릭터의 건강 상태 관리
    public class Health : MonoBehaviour, IDamage
    {
        // 캐릭터의 최대 체력 포인트
        [SerializeField] private int MaxHealthPoints = 100;

        // 캐릭터 사망 시 발생할 이벤트
        [Space]
        [SerializeField] private UnityEvent OnCharacterDeath;

        // 내부 변수
        private int _currentHP = 100; // 현재 체력
        private float _lastDamageTime; // 마지막으로 데미지를 받은 시간
        private Coroutine _restoreRoutine; // 체력 회복 코루틴
        private const float RestoreDelay = 3f; // 체력 회복 지연 시간 (3초)
        private const int RestoreAmount = 10; // 한 번에 회복할 체력 양

        // 현재 체력에 대한 공개 접근자
        public int CurrentHP { get { return _currentHP; } }

        // 최대 체력에 대한 공개 접근자
        public int MaxHP { get { return MaxHealthPoints; } }

        // 체력 변경 및 사망 이벤트
        public event Action OnHealthChanged;
        public event Action OnDead;

        // 시작 시 호출됨. 현재 체력을 최대 체력으로 초기화
        private void Start()
        {
            _currentHP = MaxHealthPoints;
            OnHealthChanged?.Invoke();
        }

        // 데미지 처리 메소드
        public void Damage(int damagePoints)
        {
            // 현재 체력 감소
            _currentHP -= damagePoints;
            // 마지막 데미지 시간 업데이트
            _lastDamageTime = Time.time;

            // 기존의 체력 회복 코루틴 중단 및 재시작
            if (_restoreRoutine != null)
                StopCoroutine(_restoreRoutine);
            _restoreRoutine = StartCoroutine(RestoreHealthOverTime());

            // 체력이 0 이하가 되면 사망 처리
            if (_currentHP <= 0)
            {
                _currentHP = 0;
                OnDead?.Invoke();
                OnCharacterDeath.Invoke();
            }

            // 체력 변경 이벤트 호출
            OnHealthChanged?.Invoke();
        }

        // 체력을 점진적으로 회복하는 코루틴
        private IEnumerator RestoreHealthOverTime()
        {
            // 지정된 지연 시간(3초) 후에 체력 회복 시작
            yield return new WaitForSeconds(RestoreDelay);

            // 체력이 최대치에 도달하거나 최근 데미지를 받은 후 3초가 지날 때까지 체력 회복
            while (_currentHP < MaxHealthPoints && Time.time - _lastDamageTime >= RestoreDelay)
            {
                RestoreHealth(RestoreAmount);
                yield return new WaitForSeconds(1f);
            }
        }

        /// <summary>
        /// 지정된 양의 체력을 회복하는 메소드
        /// </summary>
        /// <param name="hp">회복할 체력 포인트</param>
        public void RestoreHealth(int hp)
        {
            // 체력 회복
            _currentHP += hp;
            // 최대 체력을 초과하지 않도록 조정
            if (_currentHP > MaxHealthPoints)
                _currentHP = MaxHealthPoints;

            // 체력 변경 이벤트 호출
            OnHealthChanged?.Invoke();
        }

        /// <summary>
        /// 캐릭터의 체력을 완전히 회복하는 메소드 & 한 번에 죽게하는 메소드 - 추후에 필요하면 쓰려고 만들어 놈
        /// </summary>
        public void RestoreFullHealth()
        {
            // 체력을 최대치로 회복
            _currentHP = MaxHealthPoints;

            // 체력 변경 이벤트 호출
            OnHealthChanged?.Invoke();
        }

        public void InflictFullDamage()
        {
            // 체력을 최대치로 회복
            _currentHP = 0;
            OnDead?.Invoke();
            OnCharacterDeath.Invoke();
            // 체력 변경 이벤트 호출
            OnHealthChanged?.Invoke();
        }
    }
}
