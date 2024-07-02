using UnityEngine;
using DiasGames.Components;

namespace DiasGames.Abilities
{
    public class DyingZombieAbility : AbstractAbility
    {
        private IMover _mover = null;

        public bool isDie = false;

        private void Awake()
        {
            _mover = GetComponent<IMover>();
        }

        public override bool ReadyToRun()
        {
            return isDie;
        }

        public override void OnStartAbility()
        {
            SetAnimationState("DyingZombie");
        }

        public override void UpdateAbility()
        {
            _mover.StopMovement();
        }
    }
}
