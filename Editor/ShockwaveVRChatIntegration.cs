using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Dynamics.Contact.Components;

    public class ShockwaveVRChatIntegration : UnityEditor.Editor
    {
        private const string PackageName = "com.shockwave.vrchat.integration";
        private const string CollidersDirectory = "Packages/" + PackageName + "/Prefabs/HapticRegions/";
        private const string RemoveHapticsActionName = "Remove haptics from avatar";

        private static readonly Dictionary<string, HumanBodyBones> PrefabNameToBone = new Dictionary<string, HumanBodyBones>()
        {
            { "ShockwaveChestColliders", HumanBodyBones.Chest },
            { "ShockwaveLeftCalfColliders", HumanBodyBones.LeftLowerLeg },
            { "ShockwaveLeftLowerArmColliders", HumanBodyBones.LeftLowerArm },
            { "ShockwaveLeftThighColliders", HumanBodyBones.LeftUpperLeg },
            { "ShockwaveLeftUpperArmColliders", HumanBodyBones.LeftUpperArm },
            { "ShockwaveRightCalfColliders", HumanBodyBones.RightLowerLeg },
            { "ShockwaveRightLowerArmColliders", HumanBodyBones.RightLowerArm },
            { "ShockwaveRightThighColliders", HumanBodyBones.RightUpperLeg },
            { "ShockwaveRightUpperArmColliders", HumanBodyBones.RightUpperArm },
        };

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

            if (HasShockwaveColliders())
            {
                Debug.LogError($"Found existing haptics on avatar. Use the \"{RemoveHapticsActionName}\" action to remove them. Canceling operation.");
                return;
            }

            var directoryInfo = new DirectoryInfo(CollidersDirectory);
            var fileInfo = directoryInfo.GetFiles("*.prefab");
            bool attachedSuccessfully = true;

            foreach (FileInfo info in fileInfo)
            {
                string relativePath = CollidersDirectory + info.Name;
                GameObject prefab = (GameObject) EditorGUIUtility.Load(relativePath);
                if (prefab == null)
                {
                    Debug.LogError($"Could not load prefab at {relativePath}");
                    attachedSuccessfully = false;
                    continue;
                }

                string prefabName = prefab.name;
                if (!PrefabNameToBone.TryGetValue(prefabName, out var bone))
                {
                    Debug.LogWarning($"Could not find prefab with name {prefabName}");
                    continue;
                }

                Transform parent = animator.GetBoneTransform(bone);
                if (parent == null)
                {
                    Debug.LogError($"Could not find bone {bone} to attach haptic nodes");
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

        private static void GetAllPrefabInstances(GameObject root, string prefabName, List<GameObject> foundPrefabInstances)
        {
            foreach (Transform child in root.transform)
            {
                if (child.gameObject.name == prefabName)
                {
                    foundPrefabInstances.Add(child.gameObject);
                }
                else
                {
                    GetAllPrefabInstances(child.gameObject, prefabName, foundPrefabInstances);
                }
            }
        }

        private static bool HasShockwaveColliders()
        {
            GameObject gameObject = Selection.activeGameObject;

            foreach (string prefabName in PrefabNameToBone.Keys)
            {
                List<GameObject> foundPrefabInstances = new List<GameObject>();
                GetAllPrefabInstances(gameObject, prefabName, foundPrefabInstances);

                if (foundPrefabInstances.Count > 0)
                {
                    return true;
                }
            }

            return false;
        }

        [MenuItem("GameObject/Shockwave/" + RemoveHapticsActionName, false, -1)]
        public static void RemoveShockwaveColliders()
        {
            GameObject gameObject = Selection.activeGameObject;
            bool foundAnyPrefabInstances = false;

            foreach (string prefabName in PrefabNameToBone.Keys)
            {
                List<GameObject> foundPrefabInstances = new List<GameObject>();
                GetAllPrefabInstances(gameObject, prefabName, foundPrefabInstances);

                foreach (GameObject foundPrefabInstance in foundPrefabInstances)
                {
                    DestroyImmediate(foundPrefabInstance);
                    foundAnyPrefabInstances = true;
                }
            }

            if (foundAnyPrefabInstances)
            {
                Debug.Log("Haptics removal completed successfully.");
            }
            else
            {
                Debug.LogWarning("No haptics were found on avatar.");
            }
        }

        [MenuItem("GameObject/Shockwave/(Debug) Disable self-collision", false, 10)]
        public static void DisableSelfCollision()
        {
            GameObject gameObject = Selection.activeGameObject;
            var receivers = gameObject.GetComponentsInChildren<VRCContactReceiver>();
            if (receivers.Length == 0)
            {
                Debug.LogWarning("No VRCContactReceiver components found on selected item. Make sure to first add haptics support to avatar.");
                return;
            }

            foreach (VRCContactReceiver receiver in receivers)
            {
                if (receiver.allowSelf)
                {
                    GameObject.DestroyImmediate(receiver);
                }
            }

            Debug.Log("Self colliders disabled successfully");
        }

        private static T CopyComponent<T>(T original, GameObject destination) where T : Component
        {
            System.Type type = original.GetType();
            Component copy = destination.AddComponent(type);
            System.Reflection.FieldInfo[] fields = type.GetFields();
            foreach (System.Reflection.FieldInfo field in fields)
            {
                field.SetValue(copy, field.GetValue(original));
            }
            return copy as T;
        }

        [MenuItem("GameObject/Shockwave/(Debug) Enable self-collision", false, 10)]
        public static void EnableSelfCollision()
        {
            List<string> RightForearmHapticParameters = new List<string>()
            {
                "Shockwave_52",
                "Shockwave_53",
                "Shockwave_54",
                "Shockwave_55",
            };

            List<string> LeftForearmHapticParameters = new List<string>()
            {
                "Shockwave_44",
                "Shockwave_45",
                "Shockwave_46",
                "Shockwave_47",
            };

            GameObject gameObject = Selection.activeGameObject;
            var receivers = gameObject.GetComponentsInChildren<VRCContactReceiver>();
            if (receivers.Length == 0)
            {
                Debug.LogWarning("No VRCContactReceiver components found on selected item. Make sure to first add haptics support to avatar.");
                return;
            }

            foreach (VRCContactReceiver receiver in receivers)
            {
                if (receiver.allowSelf)
                {
                    Debug.LogWarning("Found existing self-collision components. Canceling operation.");
                    return;
                }
            }

            foreach (VRCContactReceiver receiver in receivers)
            {
                VRCContactReceiver selfReceiver = CopyComponent(receiver, receiver.gameObject);
                selfReceiver.allowSelf = true;
                selfReceiver.allowOthers = false;
                selfReceiver.localOnly = true;

                selfReceiver.collisionTags = new List<string>
                {
                    "Shockwave",
                    "Hand",
                    "Finger"
                };

                // Only enable opposite hand vibration on forearms
                if (RightForearmHapticParameters.Contains(selfReceiver.parameter))
                {
                    selfReceiver.collisionTags.Remove("Hand");
                    selfReceiver.collisionTags.Add("HandL");
                    selfReceiver.collisionTags.Remove("Finger");
                    selfReceiver.collisionTags.Add("FingerL");
                }
                else if (LeftForearmHapticParameters.Contains(selfReceiver.parameter))
                {
                    selfReceiver.collisionTags.Remove("Hand");
                    selfReceiver.collisionTags.Add("HandR");
                    selfReceiver.collisionTags.Remove("Finger");
                    selfReceiver.collisionTags.Add("FingerR");
                }
            }

            Debug.Log("Self colliders enabled successfully");
        }
    }