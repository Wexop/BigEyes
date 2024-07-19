using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using System.IO;
using System.Reflection;
using BepInEx.Configuration;
using BigEyes.Utils;
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
        const string VERSION = "1.1.1";

        public static Plugin instance;

        public ConfigEntry<string> spawnMoonRarity;

        void Awake()
        {
            instance = this;
            
            Logger.LogInfo($"BigEyes starting....");

            string assetDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "bigeyes");
            AssetBundle bundle = AssetBundle.LoadFromFile(assetDir);
            
            Logger.LogInfo($"BigEyes bundle found !");
            
            spawnMoonRarity = Config.Bind("General", "SpawnChance", 
                "Modded:75,ExperimentationLevel:50,AssuranceLevel:50,VowLevel:75,OffenseLevel:75,MarchLevel:75,RendLevel:100,DineLevel:125,TitanLevel:150,Adamance:100,Embrion:150,Artifice:200", 
                "Chance for big eyes to spawn for any moon, example => assurance:100,offense:50 . You need to restart the game.");
            CreateStringConfig(spawnMoonRarity);

            //bigeyes
            EnemyType bigEyes = bundle.LoadAsset<EnemyType>("Assets/LethalCompany/Mods/BigEyes/BigEyes.asset");
            Logger.LogInfo($"{bigEyes.name} FOUND");
            Logger.LogInfo($"{bigEyes.enemyPrefab} prefab");
            NetworkPrefabs.RegisterNetworkPrefab(bigEyes.enemyPrefab);
            Utilities.FixMixerGroups(bigEyes.enemyPrefab);

            TerminalNode terminalNodeBigEyes = new TerminalNode();
            terminalNodeBigEyes.creatureName = "BigEyes";
            terminalNodeBigEyes.displayText = "Don't wake him, or he will be very angry...";

            TerminalKeyword terminalKeywordBigEyes = new TerminalKeyword();
            terminalKeywordBigEyes.word = "BigEyes";
            
            
            RegisterUtil.RegisterEnemyWithConfig(spawnMoonRarity.Value, bigEyes,terminalNodeBigEyes , terminalKeywordBigEyes, bigEyes.PowerLevel, bigEyes.MaxCount);


            
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
        
        private void CreateStringConfig(ConfigEntry<string> configEntry)
        {
            var exampleSlider = new TextInputFieldConfigItem(configEntry, new TextInputFieldOptions()
            {
                RequiresRestart = true
            });
            LethalConfigManager.AddConfigItem(exampleSlider);
        }


    }
}