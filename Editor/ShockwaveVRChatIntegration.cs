﻿using System.IO;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace Editor
{
    public class ShockwaveVRChatIntegration : UnityEditor.Editor
    {
        private const string PackageName = "com.shockwave.vrchat.integration";
        private const string RemoveHapticsActionName = "Remove haptics from avatar";

        [MenuItem("GameObject/Shockwave/Add haptics to avatar", false, -1)]
        public static void AttachShockwaveColliders()
        {
            GameObject gameObject = Selection.activeGameObject;

            if (gameObject.GetComponent<VRCAvatarDescriptor>() == null)
            {
                Debug.LogError("Select an avatar with a VRCAvatarDescriptor");
                return;
            }

            Animator animator = gameObject.GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogError("Avatar must have an Animator");
                return;
            }

            var existingHapticRegions = gameObject.GetComponentsInChildren<HapticRegion>();
            if (existingHapticRegions.Length > 0)
            {
                Debug.LogError($"Found existing haptics on avatar. Use the \"{RemoveHapticsActionName}\" action to remove them. Canceling operation.");
                return;
            }

            string collidersDirectory = $"Packages/{PackageName}/Prefabs/HapticRegions/";
            var directoryInfo = new DirectoryInfo(collidersDirectory);
            var fileInfo = directoryInfo.GetFiles("*.prefab");
            bool attachedSuccessfully = true;

            foreach (FileInfo info in fileInfo)
            {
                string relativePath = collidersDirectory + info.Name;
                GameObject prefab = (GameObject) EditorGUIUtility.Load(relativePath);
                if (prefab == null)
                {
                    Debug.LogError($"Could not load prefab at {relativePath}");
                    attachedSuccessfully = false;
                    continue;
                }

                var hapticRegion = prefab.GetComponent<HapticRegion>();
                Transform parent = animator.GetBoneTransform(hapticRegion.bones);
                if (parent == null)
                {
                    Debug.LogError($"Could not find bone {hapticRegion.bones} to attach haptic nodes");
                    attachedSuccessfully = false;
                    continue;
                }

                Debug.Log("Attach " + info.Name);
                GameObject spawnedPrefab = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

                Undo.RegisterCreatedObjectUndo(spawnedPrefab, "Added Prefab");
                Undo.SetTransformParent(spawnedPrefab.transform, parent, "Added Prefab");
                spawnedPrefab.transform.localPosition = prefab.transform.localPosition;
                spawnedPrefab.transform.localRotation = prefab.transform.localRotation;
                spawnedPrefab.transform.localScale = prefab.transform.localScale;
            }

            if (attachedSuccessfully)
            {
                Debug.Log("<color=green>Haptics integration completed successfully.</color>");
            }
            else
            {
                Debug.Log("Haptics integration completed with errors.");
            }
        }

        [MenuItem("GameObject/Shockwave/" + RemoveHapticsActionName, false, -1)]
        public static void RemoveShockwaveColliders()
        {
            GameObject gameObject = Selection.activeGameObject;

            var existingHapticRegions = gameObject.GetComponentsInChildren<HapticRegion>();
            if (existingHapticRegions.Length == 0)
            {
                Debug.LogWarning("No haptics were found on avatar. Canceling operation.");
                return;
            }

            foreach (HapticRegion hapticRegion in existingHapticRegions)
            {
                DestroyImmediate(hapticRegion.gameObject);
            }

            Debug.Log("Haptics removal completed successfully.");
        }
    }
}