#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace SystemicOverload.EditorTools
{
    /// <summary>
    /// Phase 1 로코모션용 Animator Controller와 플레이스홀더 클립을 생성합니다. 실제 클립으로 교체하면 됩니다.
    /// </summary>
    public static class Phase1LocomotionAnimatorFactory
    {
        public const string ControllerAssetPath = "Assets/Animation/Phase1/Player_Locomotion_Placeholder.controller";
        private const string RootFolder = "Assets/Animation/Phase1";

        [MenuItem("Tools/Systemic Overload/Animation/Create Phase1 Locomotion Placeholder Controller")]
        public static void CreatePlaceholderController()
        {
            EnsureFolderPath(RootFolder);

            string idleClipPath = $"{RootFolder}/Placeholder_Idle.anim";
            string moveClipPath = $"{RootFolder}/Placeholder_Move.anim";
            string attackClipPath = $"{RootFolder}/Placeholder_Attack.anim";

            AnimationClip idleClip = BuildPlaceholderClip("Placeholder_Idle", 0.0f, 0.12f);
            AnimationClip moveClip = BuildPlaceholderClip("Placeholder_Move", 0.05f, 0.12f);
            AnimationClip attackClip = BuildPlaceholderClip("Placeholder_Attack", 0.12f, 0.25f);

            SaveClip(idleClip, idleClipPath);
            SaveClip(moveClip, moveClipPath);
            SaveClip(attackClip, attackClipPath);

            if (File.Exists(ControllerAssetPath))
            {
                AssetDatabase.DeleteAsset(ControllerAssetPath);
            }

            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerAssetPath);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
            controller.AddParameter("AttackTrig", AnimatorControllerParameterType.Trigger);

            AnimatorStateMachine root = controller.layers[0].stateMachine;

            BlendTree blendTree = new BlendTree
            {
                name = "LocomotionBlend",
                blendType = BlendTreeType.Simple1D,
                blendParameter = "Speed",
                useAutomaticThresholds = false
            };

            AssetDatabase.AddObjectToAsset(blendTree, controller);
            blendTree.AddChild(idleClip);
            blendTree.AddChild(moveClip);
            // Unity 6에서는 BlendTree.SetThreshold가 제거되었으므로 children 배열로 임계값을 설정합니다.
            ChildMotion[] blendChildren = blendTree.children;
            blendChildren[0].threshold = 0.0f;
            blendChildren[1].threshold = 1.0f;
            blendTree.children = blendChildren;

            AnimatorState locomotionState = root.AddState("Locomotion");
            locomotionState.motion = blendTree;

            AnimatorState attackState = root.AddState("Attack");
            attackState.motion = attackClip;

            root.defaultState = locomotionState;

            AnimatorStateTransition attackToLoco = attackState.AddTransition(locomotionState);
            attackToLoco.hasExitTime = true;
            attackToLoco.exitTime = 0.2f;
            attackToLoco.duration = 0.08f;
            attackToLoco.hasFixedDuration = true;

            AnimatorStateTransition anyToAttack = root.AddAnyStateTransition(attackState);
            anyToAttack.AddCondition(AnimatorConditionMode.If, 0.0f, "AttackTrig");
            anyToAttack.duration = 0.05f;
            anyToAttack.canTransitionToSelf = false;

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[Phase1LocomotionAnimatorFactory] 생성 완료: {ControllerAssetPath}");
        }

        private static AnimationClip BuildPlaceholderClip(string clipName, float valueY, float durationSeconds)
        {
            var clip = new AnimationClip
            {
                name = clipName,
                frameRate = 60.0f
            };

            AnimationCurve curve = AnimationCurve.Linear(0.0f, valueY, Mathf.Max(0.016f, durationSeconds), valueY);
            clip.SetCurve(string.Empty, typeof(Transform), "localPosition.y", curve);
            return clip;
        }

        private static void SaveClip(AnimationClip clip, string assetPath)
        {
            AnimationClip existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);
            if (existing != null)
            {
                AssetDatabase.DeleteAsset(assetPath);
            }

            AssetDatabase.CreateAsset(clip, assetPath);
        }

        private static void EnsureFolderPath(string assetPath)
        {
            string[] segments = assetPath.Split('/');
            if (segments.Length == 0 || segments[0] != "Assets")
            {
                throw new IOException("Asset path는 Assets 루트로 시작해야 합니다.");
            }

            string currentPath = "Assets";
            for (int segmentIndex = 1; segmentIndex < segments.Length; segmentIndex++)
            {
                string nextSegment = segments[segmentIndex];
                string nextPath = currentPath + "/" + nextSegment;
                if (!AssetDatabase.IsValidFolder(nextPath))
                {
                    AssetDatabase.CreateFolder(currentPath, nextSegment);
                }

                currentPath = nextPath;
            }
        }
    }
}
#endif
