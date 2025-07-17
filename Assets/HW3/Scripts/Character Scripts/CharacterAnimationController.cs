using System;
using UnityEngine;
using System.Collections.Generic;

namespace HW3.Scripts
{
    public class CharacterAnimationController : MonoBehaviour
    {
        [SerializeField] private CharacterInput characterInput;
        [SerializeField] private CharacterHealth characterHealth;

        private void Awake()
        {
            characterInput.OnMove += PlayWalkAnimation;
            characterInput.OnAttack += PlayAttackAnimation;
            characterHealth.OnTakingDamage += PlayOnHitAnimation;
        }

        private void OnDestroy()
        {
            characterInput.OnMove -= PlayWalkAnimation;
            characterInput.OnAttack -= PlayAttackAnimation;
            characterHealth.OnTakingDamage -= PlayOnHitAnimation;
        }

        [SerializeField] private List<Animator> animators;

        private const string AnimationTriggerIdle = "Idle";
        private const string AnimationTriggerWalk = "Speed";
        private const string AnimationTriggerOnHit = "Hit";
        private const string AnimationTriggerAttack = "Attack";
        public void PlayIdleAnimation() => ApplyAnimation(AnimationTriggerIdle);

        public void PlayAttackAnimation() => ApplyAnimation(AnimationTriggerAttack);

        public void PlayWalkAnimation(float value) => ApplyFloatAnimation(AnimationTriggerWalk, value);

        public void PlayOnHitAnimation() => ApplyAnimation(AnimationTriggerOnHit);

        private void ApplyAnimation(string animationTriggerName)
        {
            foreach (Animator animator in animators)
            {
                if (!animator.runtimeAnimatorController)
                    return;

                animator.SetTrigger(animationTriggerName);
            }
        }

        private void ApplyFloatAnimation(string animationTriggerFloatName, float value)
        {
            foreach (Animator animator in animators)
            {
                if (!animator.runtimeAnimatorController)
                    return;

                animator.SetFloat(animationTriggerFloatName, value);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (animators == null || animators.Count == 0)
            {
                animators = new List<Animator>(GetComponentsInChildren<Animator>());
            }
        }
#endif
    }
}
