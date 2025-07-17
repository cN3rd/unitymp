using UnityEngine;
using System.Collections.Generic;

namespace HW3.Scripts
{
    public class CharacterVisualController : MonoBehaviour
    {
        [SerializeField] List<Animator> animators;

        private const string ANIMATION_TRIGGER_IDLE = "Idle";
        private const string ANIMATION_TRIGGER_WALK = "Walk";
        private const string ANIMATION_TRIGGER_OnHit = "Hit";
        public void PlayIdleAnimation()
        {
            ApplyAnimation(ANIMATION_TRIGGER_IDLE);
        }

        public void PlayWalkAnimation()
        {
            ApplyAnimation(ANIMATION_TRIGGER_WALK);
        }
        public void PlayOnHitAnimation()
        {
            ApplyAnimation(ANIMATION_TRIGGER_OnHit);
        }
        private void ApplyAnimation(string animationTriggerName)
        {
            foreach (Animator animator in animators)
            {
                if (!animator.runtimeAnimatorController)
                    return;

                animator.SetTrigger(animationTriggerName);
            }
        }

        private void OnValidate()
        {
            if (animators == null || animators.Count == 0)
            {
                animators = new List<Animator>(GetComponentsInChildren<Animator>());
            }
        }
    }
}
