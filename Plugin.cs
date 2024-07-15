using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using System.IO;
using System.Reflection;
using BepInEx.Configuration;
using LethalConfig;
using LethalConfig.ConfigItems;
using LethalConfig.ConfigItems.Options;
using UnityEngine;
using LethalLib.Modules;

 namespace BigEyes
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency("evaisa.lethallib", "0.15.1")]
    public class Plugin : BaseUnityPlugin
    {

        const string GUID = "wexop.bigeyes";
        const string NAME = "BigEyes";
        const string VERSION = "1.0.0";

        public static Plugin instance;

        public ConfigEntry<int> chanceSpawnEntry;

        void Awake()
        {
            instance = this;
            
            Logger.LogInfo($"BigEyes starting....");

            string assetDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "bigeyes");
            AssetBundle bundle = AssetBundle.LoadFromFile(assetDir);
            
            Logger.LogInfo($"BigEyes bundle found !");
            
            chanceSpawnEntry = Config.Bind("General", "SpawnChance", 20, "Chance for big eyes to spawn. You need to restart the game.");
            CreateIntConfig(chanceSpawnEntry);

            //bigeyes
            EnemyType bigEyes = bundle.LoadAsset<EnemyType>("Assets/LethalCompany/Mods/BigEyes/BigEyes.asset");
            Logger.LogInfo($"{bigEyes.name} FOUND");
            Logger.LogInfo($"{bigEyes.enemyPrefab} prefab");
            NetworkPrefabs.RegisterNetworkPrefab(bigEyes.enemyPrefab);
            Utilities.FixMixerGroups(bigEyes.enemyPrefab);

            var dic = new Dictionary<Levels.LevelTypes, int>();
            dic.Add(Levels.LevelTypes.All, chanceSpawnEntry.Value);
            Enemies.RegisterEnemy(bigEyes, Enemies.SpawnType.Default, dic);


            
            Logger.LogInfo($"BigEyes is ready!");
        }
        
        private void CreateFloatConfig(ConfigEntry<float> configEntry, float min = 0f, float max = 100f)
        {
            var exampleSlider = new FloatSliderConfigItem(configEntry, new FloatSliderOptions
            {
                Min = min,
                Max = max,
                RequiresRestart = false
            });
            LethalConfigManager.AddConfigItem(exampleSlider);
        }
        
        private void CreateIntConfig(ConfigEntry<int> configEntry, int min = 0, int max = 100)
        {
            var exampleSlider = new IntSliderConfigItem(configEntry, new IntSliderOptions()
            {
                Min = min,
                Max = max,
                RequiresRestart = false
            });
            LethalConfigManager.AddConfigItem(exampleSlider);
        }


    }
}