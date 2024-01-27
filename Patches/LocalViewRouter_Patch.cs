using HarmonyLib;
using Kitchen;
using System.Collections.Generic;
using UnityEngine;

namespace KitchenAchievementTracker.Patches
{
    [HarmonyPatch]
    static class LocalViewRouter_Patch
    {
        private static GameObject _prefabsContainer;

        private static Dictionary<ViewType, GameObject> _prefabs = new Dictionary<ViewType, GameObject>();

        [HarmonyPatch(typeof(LocalViewRouter), "GetPrefab")]
        [HarmonyPrefix]
        static bool GetPrefab_Prefix(ViewType view_type, ref GameObject __result)
        {
            if (_prefabsContainer == null)
            {
                _prefabsContainer = new GameObject("Achievement Tracker Prefabs");
                _prefabsContainer.SetActive(false);
                _prefabsContainer.hideFlags = HideFlags.HideAndDontSave;
            }

            if (_prefabs.TryGetValue(view_type, out GameObject prefab))
            {
                __result = prefab;
                return false;
            }

            if (view_type == Main.AchievementsTrackerViewType)
            {
                int uiLayer = LayerMask.NameToLayer("UI");

                GameObject trackerGO = new GameObject("Achievements Tracker");
                AchievementsTrackerView trackerView = trackerGO.AddComponent<AchievementsTrackerView>();
                trackerGO.layer = uiLayer;

                GameObject achievementIconTemplate = new GameObject("Template");
                achievementIconTemplate.transform.SetParent(trackerGO.transform);
                achievementIconTemplate.transform.Reset();
                achievementIconTemplate.SetActive(false);
                achievementIconTemplate.layer = uiLayer;

                GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
                plane.layer = uiLayer;
                plane.transform.SetParent(achievementIconTemplate.transform);
                plane.transform.Reset();
                plane.transform.localRotation = Quaternion.Euler(90f, 180f, 0f);
                plane.transform.localScale = Vector3.one * 0.1f;
                MeshCollider meshCollider = plane.GetComponent<MeshCollider>();
                if (meshCollider)
                    Object.DestroyImmediate(meshCollider);

                AchievementIcon achievementIcon = achievementIconTemplate.AddComponent<AchievementIcon>();
                achievementIcon.Renderer = plane.GetComponent<MeshRenderer>();

                trackerView.Template = achievementIconTemplate;

                trackerGO.transform.SetParent(_prefabsContainer.transform);
                trackerGO.transform.Reset();

                _prefabs.Add(Main.AchievementsTrackerViewType, trackerGO);
                __result = trackerGO;
                return false;
            }
            return true;
        }
    }
}
