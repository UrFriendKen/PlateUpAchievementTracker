using Steamworks;
using Steamworks.Data;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace KitchenAchievementTracker.Utils
{
    internal static class SteamworksUtils
    {
        private static Dictionary<string, Texture2D> _achievementIcons;

        private static Dictionary<string, Material> _achievementMaterials = new Dictionary<string, Material>();

        struct AchievementStateCombo
        {
            public string Identifier;
            public Achievement Achievement;
            public bool State;
        }

        public static IEnumerable<Achievement> GetAchievements()
        {
            if (!SteamClient.IsValid)
                return new List<Achievement>();
            return SteamUserStats.Achievements;
        }

        public static bool ClearAchievements()
        {
            try
            {
                foreach (Achievement achievement in GetAchievements())
                {
                    Main.LogWarning($"Clearing {achievement.Identifier}...");
                    achievement.Clear();
                }
                Main.LogInfo($"Cleared all achievements!");
                return true;
            }
            catch (System.Exception ex)
            {
                Main.LogError($"{ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }

        public static Texture2D GetAchievementIcon(string identifier, bool state = true)
        {
            if (_achievementIcons == null)
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                _achievementIcons = new Dictionary<string, Texture2D>();
                _achievementIcons =
                    GetAchievements()
                    .SelectMany(achievement => new bool[] { false, true }, (achievement, state) => new AchievementStateCombo()
                    {
                        Identifier = GetAchievementStateIdentifier(achievement.Identifier, state),
                        Achievement = achievement,
                        State = state
                    })
                    .ToDictionary(combo => combo.Identifier, combo =>
                    {
                        using (Stream resStream = assembly.GetManifestResourceStream($"KitchenAchievementTracker.resources.{combo.Achievement.Identifier}.png"))
                        {
                            Bitmap bitmap = resStream != null ? new Bitmap(resStream) : new Bitmap(1, 1);
                            Texture2D texture = bitmap.ToTexture2D(isColored: !combo.State, coloredAlpha: 0.8f, grayscaleAlpha: 0.2f);
                            texture.name = combo.Identifier;
                            return texture;
                        }
                    });
            }
            _achievementIcons.TryGetValue(GetAchievementStateIdentifier(identifier, state), out Texture2D icon);
            return icon;
        }

        public static string GetAchievementStateIdentifier(string identifier, bool state)
        {
            return $"{identifier}_{state}";
        }

        public static Material GetAchievementIconMaterial(string identifier, bool state)
        {
            string stateIdentifier = SteamworksUtils.GetAchievementStateIdentifier(identifier, state);
            if (!_achievementMaterials.TryGetValue(stateIdentifier, out Material material))
            {
                try
                {
                    Shader flatImage = Shader.Find("Flat Image");
                    material = new Material(flatImage);
                    _achievementMaterials.Add(stateIdentifier, material);
                    material.SetTexture("_Image", SteamworksUtils.GetAchievementIcon(identifier, state));
                    material.name = stateIdentifier;
                }
                catch (System.Exception ex)
                {
                    Main.LogError($"{ex.Message}\n{ex.StackTrace}");
                    _achievementMaterials.Add(stateIdentifier, null);
                }
            }
            return material;
        }

        public static Texture2D ToTexture2D(this Steamworks.Data.Image image, bool isColored = true, float coloredAlpha = 1f, float grayscaleAlpha = 1f)
        {
            Texture2D texture = new Texture2D((int)image.Width, (int)image.Height);
            for (int i = 0; i < texture.width; i++)
            {
                for (int j = 0; j < texture.height; j++)
                {
                    Steamworks.Data.Color swColor = image.GetPixel(i, j);
                    texture.SetPixel(i, texture.height - 1 - j, GetColor(swColor));

                    UnityEngine.Color GetColor(Steamworks.Data.Color swColor)
                    {
                        float r = swColor.r / 255f;
                        float g = swColor.g / 255f;
                        float b = swColor.b / 255f;
                        float a = swColor.a / 255f;
                        switch (isColored)
                        {
                            case true:
                                return new UnityEngine.Color(r, g, b, a * coloredAlpha);
                            case false:
                                float l = 0.2989f*r + 0.5870f*g + 0.1140f*b;
                                return new UnityEngine.Color(l, l, l, a * grayscaleAlpha);
                        }
                    }
                }
            }
            texture.Apply();
            return texture;
        }

        public static Texture2D ToTexture2D(this Bitmap bitmap, bool isColored = true, float coloredAlpha = 1f, float grayscaleAlpha = 1f)
        {
            Texture2D texture = new Texture2D((int)bitmap.Width, (int)bitmap.Height);
            for (int i = 0; i < texture.width; i++)
            {
                for (int j = 0; j < texture.height; j++)
                {
                    System.Drawing.Color swColor = bitmap.GetPixel(i, j);
                    texture.SetPixel(i, texture.height - 1 - j, GetColor(swColor));

                    UnityEngine.Color GetColor(System.Drawing.Color sdColor)
                    {
                        float r = sdColor.R / 255f;
                        float g = sdColor.G / 255f;
                        float b = sdColor.B / 255f;
                        float a = sdColor.A / 255f;
                        switch (isColored)
                        {
                            case true:
                                return new UnityEngine.Color(r, g, b, a * coloredAlpha);
                            case false:
                                float l = 0.2989f * r + 0.5870f * g + 0.1140f * b;
                                return new UnityEngine.Color(l, l, l, a * grayscaleAlpha);
                        }
                    }
                }
            }
            texture.Apply();
            return texture;
        }
    }
}
