using Kitchen;
using KitchenAchievementTracker.Utils;
using KitchenMods;
using MessagePack;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace KitchenAchievementTracker
{
    public class AchievementsTrackerView : UpdatableObjectView<AchievementsTrackerView.ViewData>
    {
        public class UpdateView : IncrementalViewSystemBase<ViewData>, IModSystem
        {
            EntityQuery Views;

            protected override void Initialise()
            {
                base.Initialise();
                Views = GetEntityQuery(typeof(SAchievementsTracker), typeof(CAchievementItem), typeof(CLinkedView));
            }

            protected override void OnUpdate()
            {
                using NativeArray<Entity> entities = Views.ToEntityArray(Allocator.Temp);
                using NativeArray<CLinkedView> views = Views.ToComponentDataArray<CLinkedView>(Allocator.Temp);

                for (int i = 0; i < views.Length; i++)
                {
                    Entity entity = entities[i];
                    CLinkedView view = views[i];
                    DynamicBuffer<CAchievementItem> buffer = GetBuffer<CAchievementItem>(entity);

                    List<(string, bool)> achievements = new List<(string, bool)>();
                    for (int j = 0; j < buffer.Length; j++)
                    {
                        CAchievementItem achievementItem = buffer[j];
                        achievements.Add((achievementItem.Identifier.ToString(), achievementItem.State));
                    }

                    SendUpdate(view, new ViewData()
                    {
                        AchievementsStatus = achievements
                    });
                }
            }
        }

        [MessagePackObject(false)]
        public class ViewData : ISpecificViewData, IViewData.ICheckForChanges<ViewData>
        {
            [Key(0)] public List<(string, bool)> AchievementsStatus;

            public IUpdatableObject GetRelevantSubview(IObjectView view) => view.GetSubView<AchievementsTrackerView>();

            public bool IsChangedFrom(ViewData check)
            {
                int count = AchievementsStatus.Count;
                if (count != check.AchievementsStatus.Count)
                    return true;
                if (AchievementsStatus.Select(x => x.Item1).Intersect(check.AchievementsStatus.Select(x => x.Item1)).Count() != count)
                    return true;
                for (int i = 0; i < count; i++)
                {
                    if (AchievementsStatus[i] != check.AchievementsStatus[i])
                        return true;
                }
                return false;
            }
        }

        const int ICONS_PER_ROW = 26;

        GameObject _container;

        Dictionary<string, Material> _materials = new Dictionary<string, Material>();

        public GameObject Template;

        public Material MissingMaterial;

        public Dictionary<string, bool> Achievements = new Dictionary<string, bool>();

        protected override void UpdateData(ViewData data)
        {
            Achievements = data.AchievementsStatus.ToDictionary(x => x.Item1, x => x.Item2);

            if (_container == null)
            {
                _container = new GameObject("Icons Container");
                _container.transform.SetParent(transform);
                _container.transform.Reset();
                _container.transform.localPosition = new Vector3(0f, -0.52f);
                _container.transform.localScale = Vector3.one * 0.3f;
            }

            Transform containerTransform = _container.transform;

            for (int i = containerTransform.childCount - 1; i > -1; i--)
            {
                Object.Destroy(containerTransform.GetChild(i).gameObject);
            }

            if (Template == null)
                return;

            Vector3 root = new Vector3(0.5f - data.AchievementsStatus.Count / 2f, -0.5f);

            for (int i = 0; i < data.AchievementsStatus.Count; i++)
            {
                (string identifier, bool state) = data.AchievementsStatus[i];
                int row = i / ICONS_PER_ROW;
                int col = i % ICONS_PER_ROW;

                GameObject iconGO = GameObject.Instantiate(Template, containerTransform, worldPositionStays: true);
                iconGO.transform.SetParent(containerTransform);
                iconGO.name = $"{identifier}";
                iconGO.transform.Reset();
                iconGO.transform.localPosition = root + new Vector3(col, -row, 0f);

                iconGO.GetComponent<AchievementIcon>()?.SetIcon(identifier, state);
            }
        }

        private void OnDestroy()
        {
            if (_materials == null)
                return;

            foreach (Material material in _materials.Values)
            {
                if (material)
                    Object.Destroy(material);
            }
            _materials.Clear();
        }
    }
}
