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
    public class BigEyesPlugin : BaseUnityPlugin
    {

        const string GUID = "wexop.bigeyes";
        const string NAME = "BigEyes";
        const string VERSION = "1.2.0";

        public static BigEyesPlugin instance;

        public ConfigEntry<string> spawnMoonRarity;
        
        public ConfigEntry<float> minSleepTimeEntry;
        public ConfigEntry<float> maxSleepTimeEntry;
        
        public ConfigEntry<float> minSearchTimeEntry;
        public ConfigEntry<float> maxSearchTimeEntry;
        
        public ConfigEntry<float> wakeUpTimeEntry;
        public ConfigEntry<float> visionWidthEntry;
        
        public ConfigEntry<float> searchSpeedEntry;
        public ConfigEntry<float> angrySpeedEntry;
        
        public ConfigEntry<float> normalAccelerationEntry;
        public ConfigEntry<float> angryAccelerationEntry;
        
        public ConfigEntry<float> angularSpeedEntry;
        
        public ConfigEntry<float> chaseTime;
        
        public ConfigEntry<float> openDoorMutliplierNormalEntry;
        public ConfigEntry<float> openDoorMutliplierAngryEntry;

        void Awake()
        {
            instance = this;
            
            Logger.LogInfo($"BigEyes starting....");

            string assetDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "bigeyes");
            AssetBundle bundle = AssetBundle.LoadFromFile(assetDir);
            
            Logger.LogInfo($"BigEyes bundle found !");
            
            LoadConfigs();
            RegisterMonster(bundle);
            
            
            Logger.LogInfo($"BigEyes is ready!");
        }

        void LoadConfigs()
        {
            spawnMoonRarity = Config.Bind("General", "SpawnRarity", 
                "Modded:75,ExperimentationLevel:50,AssuranceLevel:50,VowLevel:75,OffenseLevel:75,MarchLevel:75,RendLevel:100,DineLevel:125,TitanLevel:150,Adamance:100,Embrion:150,Artifice:200", 
                "Chance for big eyes to spawn for any moon, example => assurance:100,offense:50 . You need to restart the game.");
            CreateStringConfig(spawnMoonRarity, true);
            
            //MONSTER BEHAVIOR CONFIGS
            
            minSleepTimeEntry = Config.Bind("Custom Behavior", "minSleepDuration", 
                10f, 
                "BigEyes min sleep phase duration. You don't need to restart the game !");
            CreateFloatConfig(minSleepTimeEntry);
            
            maxSleepTimeEntry = Config.Bind("Custom Behavior", "maxSleepDuration", 
                25f, 
                "BigEyes max sleep phase duration. You don't need to restart the game !");
            CreateFloatConfig(maxSleepTimeEntry);
            
            minSearchTimeEntry = Config.Bind("Custom Behavior", "minSearchDuration", 
                10f, 
                "BigEyes min search phase duration. You don't need to restart the game !");
            CreateFloatConfig(minSearchTimeEntry);
            
            maxSearchTimeEntry = Config.Bind("Custom Behavior", "maxSearchDuration", 
                25f, 
                "BigEyes max search phase duration. You don't need to restart the game !");
            CreateFloatConfig(maxSearchTimeEntry);
            
            wakeUpTimeEntry = Config.Bind("Custom Behavior", "wakeUpDuration", 
                2f, 
                "BigEyes wake up duration, where he can't detect any player. You don't need to restart the game !");
            CreateFloatConfig(wakeUpTimeEntry);
            
            visionWidthEntry = Config.Bind("Custom Behavior", "visionWidth", 
                150f, 
                "BigEyes vision with. You don't need to restart the game !");
            CreateFloatConfig(visionWidthEntry, 1f, 500f);
            
            searchSpeedEntry = Config.Bind("Custom Behavior", "searchSpeed", 
                5f, 
                "BigEyes speed on search phase. See NavMeshAgent from Unity for more infos. You don't need to restart the game !");
            CreateFloatConfig(searchSpeedEntry);
            
            angrySpeedEntry = Config.Bind("Custom Behavior", "angrySpeed", 
                10f, 
                "BigEyes speed on angry phase. See NavMeshAgent from Unity for more infos. You don't need to restart the game !");
            CreateFloatConfig(angrySpeedEntry);
            
            normalAccelerationEntry = Config.Bind("Custom Behavior", "normalAcceleration", 
                10f, 
                "BigEyes acceleration on angry phase. See NavMeshAgent from Unity for more infos. You don't need to restart the game !");
            CreateFloatConfig(normalAccelerationEntry);
            
            angryAccelerationEntry = Config.Bind("Custom Behavior", "angryAcceleration", 
                13f, 
                "BigEyes acceleration on angry phase. See NavMeshAgent from Unity for more infos. You don't need to restart the game !");
            CreateFloatConfig(angryAccelerationEntry);
            
            angularSpeedEntry = Config.Bind("Custom Behavior", "angularSpeed", 
                400f, 
                "BigEyes angularSpeed. See NavMeshAgent from Unity for more infos. You don't need to restart the game !");
            CreateFloatConfig(angularSpeedEntry, 1f, 1500f);
            
            chaseTime = Config.Bind("Custom Behavior", "chaseDuration", 
                4f, 
                "BigEyes chase duration when he detect a player. You don't need to restart the game !");
            CreateFloatConfig(chaseTime);
            
            openDoorMutliplierNormalEntry = Config.Bind("Custom Behavior", "openDoorMultiplierNormal", 
                1.5f, 
                "BigEyes open door multiplier on search phase. You don't need to restart the game !");
            CreateFloatConfig(openDoorMutliplierNormalEntry);
            
            openDoorMutliplierAngryEntry = Config.Bind("Custom Behavior", "openDoorMultiplierAngry", 
                0.8f, 
                "BigEyes open door multiplier on angry phase. You don't need to restart the game !");
            CreateFloatConfig(openDoorMutliplierAngryEntry);
            
        }

        void RegisterMonster(AssetBundle bundle)
        {
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
        
        private void CreateStringConfig(ConfigEntry<string> configEntry, bool requireRestart = false)
        {
            var exampleSlider = new TextInputFieldConfigItem(configEntry, new TextInputFieldOptions()
            {
                RequiresRestart = requireRestart
            });
            LethalConfigManager.AddConfigItem(exampleSlider);
        }


    }
}