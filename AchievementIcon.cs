using KitchenAchievementTracker.Utils;
using UnityEngine;

namespace KitchenAchievementTracker
{
    public class AchievementIcon : MonoBehaviour
    {
        public MeshRenderer Renderer;

        public void SetIcon(string identifier, bool state)
        {
            gameObject.SetActive(true);
            Renderer.material = SteamworksUtils.GetAchievementIconMaterial(identifier, state);
        }
    }
}
