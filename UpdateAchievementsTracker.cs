using Kitchen;
using KitchenAchievementTracker.Utils;
using KitchenMods;
using Steamworks.Data;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace KitchenAchievementTracker
{
    [StructLayout(LayoutKind.Sequential, Size = 1)]
    public struct SAchievementsTracker : IComponentData, IModComponent { }

    [InternalBufferCapacity(26)]
    public struct CAchievementItem : IBufferElementData
    {
        public FixedString32 Identifier;
        public bool State;
    }

    public class UpdateAchievementsTracker : GameSystemBase, IModSystem
    {
        protected override void OnUpdate()
        {
            if (!RequireEntity<SAchievementsTracker>(out Entity singletonEntity))
            {
                singletonEntity = EntityManager.CreateEntity(
                    typeof(SAchievementsTracker),
                    typeof(CPosition),
                    typeof(CDoNotPersist),
                    typeof(CPersistThroughSceneChanges),
                    typeof(CRequiresView));

                Set(singletonEntity, new SAchievementsTracker());

                Set(singletonEntity, new CPosition(new Vector3(0.5f, 1f, 0f)));

                Set(singletonEntity, new CRequiresView()
                {
                    Type = Main.AchievementsTrackerViewType,
                    ViewMode = ViewMode.Screen
                });
                return;
            }

            if (!RequireBuffer(singletonEntity, out DynamicBuffer<CAchievementItem> buffer))
            {
                buffer = EntityManager.AddBuffer<CAchievementItem>(singletonEntity);
            }

            buffer.Clear();
            foreach (Achievement achievement in SteamworksUtils.GetAchievements())
            {
                buffer.Add(new CAchievementItem()
                {
                    Identifier = achievement.Identifier,
                    State = achievement.State
                });
            }
        }
    }
}
