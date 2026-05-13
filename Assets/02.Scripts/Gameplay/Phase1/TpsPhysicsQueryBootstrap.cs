using SystemicOverload.Combat;
using SystemicOverload.Interaction;
using UnityEngine;

namespace SystemicOverload.Phase1
{
    /// <summary>
    /// TPS 플레이어에 Physics Query 실습 컴포넌트를 자동 부착합니다.
    /// </summary>
    public static class TpsPhysicsQueryBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsurePhysicsQueryComponents()
        {
            InputProvider playerInputProvider = Object.FindFirstObjectByType<InputProvider>();
            if (playerInputProvider == null)
            {
                return;
            }

            GameObject playerObject = playerInputProvider.gameObject;
            EnsureComponent<TpsRayInteractor>(playerObject);
            EnsureComponent<TpsMeleeAttackComponent>(playerObject);
            EnsureComponent<TpsMagicSphereCastComponent>(playerObject);
            EnsureComponent<TpsGroundAoeSkillComponent>(playerObject);
            EnsureComponent<TpsDashCollisionComponent>(playerObject);
        }

        private static void EnsureComponent<T>(GameObject targetObject) where T : Component
        {
            if (targetObject.GetComponent<T>() != null)
            {
                return;
            }

            targetObject.AddComponent<T>();
        }
    }
}
