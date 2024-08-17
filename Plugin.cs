using BepInEx;
using System.IO;
using System.Reflection;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BigEyes.Utils;
using com.github.zehsteam.SellMyScrap;
using com.github.zehsteam.SellMyScrap.ScrapEaters;
using LethalConfig;
using LethalConfig.ConfigItems;
using LethalConfig.ConfigItems.Options;
using UnityEngine;
using LethalLib.Modules;

 namespace BigEyes
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency("evaisa.lethallib", "0.15.1")]
    [BepInDependency("com.github.zehsteam.SellMyScrap",BepInDependency.DependencyFlags.SoftDependency) ]
    public class BigEyesPlugin : BaseUnityPlugin
    {

        const string GUID = "wexop.bigeyes";
        const string NAME = "BigEyes";
        const string VERSION = "1.3.4";

        public bool isSellMyScrapIsHere;
        public static string SellMyScrapReferenceChain = "com.github.zehsteam.SellMyScrap";

        public static BigEyesPlugin instance;

        public ConfigEntry<string> spawnMoonRarity;
        public ConfigEntry<string> scrapMoonRarity;

        public ConfigEntry<float> smallEyesScrapVolume;
        
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

        public ConfigEntry<int> scrapEaterWeight;
        public ConfigEntry<int> maxBigEyesSpawnNb;

        void Awake()
        {
            instance = this;
            
            Logger.LogInfo($"BigEyes starting....");

            string assetDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "bigeyes");
            AssetBundle bundle = AssetBundle.LoadFromFile(assetDir);
            
            Logger.LogInfo($"BigEyes bundle found !");
            
            LoadConfigs();
            RegisterMonster(bundle);
            RegisterScrap(bundle);
            
            if (Chainloader.PluginInfos.ContainsKey(SellMyScrapReferenceChain))
            {
                Debug.Log("SellMyScrap found !");
                isSellMyScrapIsHere = true;
                LoadScrapEater(bundle);
            }
            
            
            Logger.LogInfo($"BigEyes is ready!");
        }

        void LoadScrapEater(AssetBundle bundle)
        {
            
            scrapEaterWeight = Config.Bind("SellMyScrap", "bigEyesScrapEaterWeight", 
                1, 
                "BigEyes scrap eater weight");
            CreateIntConfig(scrapEaterWeight);
            
            
            GameObject BigEyesScrapEater = bundle.LoadAsset<GameObject>("Assets/LethalCompany/Mods/BigEyes/BigEyesScrapEaterPrefab.prefab");
            Debug.Log($"{BigEyesScrapEater.name} FOUND");
            ScrapEaterManager.AddScrapEater(BigEyesScrapEater, () => scrapEaterWeight.Value);
            ConfigHelper.AddScrapEaterConfigItem("BigEyesScrapEater",
                (value) =>
                {
                    scrapEaterWeight.Value = int.Parse(value);
                }, () => scrapEaterWeight.Value.ToString()
                );

        }

        void LoadConfigs()
        {
            
            //GENERAL
            spawnMoonRarity = Config.Bind("General", "SpawnRarity", 
                "Modded:40,ExperimentationLevel:20,AssuranceLevel:20,VowLevel:20,OffenseLevel:25,MarchLevel:25,RendLevel:30,DineLevel:30,TitanLevel:50,Adamance:50,Embrion:50,Artifice:55", 
                "Chance for big eyes to spawn for any moon, example => assurance:100,offense:50 . You need to restart the game.");
            CreateStringConfig(spawnMoonRarity, true);
            
            scrapMoonRarity = Config.Bind("General", "ScrapSpawnRarity", 
                "Modded:15,ExperimentationLevel:10,AssuranceLevel:10,VowLevel:15,OffenseLevel:15,MarchLevel:15,RendLevel:20,DineLevel:20,TitanLevel:20,Adamance:20,Embrion:30,Artifice:30", 
                "Chance for big eyes to spawn for any moon, example => assurance:100,offense:50 . You need to restart the game.");
            CreateStringConfig(scrapMoonRarity, true);
            
            smallEyesScrapVolume = Config.Bind("General", "smallEyesScrapVolume", 0.7f,
                "SmallEyes scrap item sound volume. You need to restart the game.");
            CreateFloatConfig(smallEyesScrapVolume, 0f, 1f);
            
            //MONSTER BEHAVIOR CONFIGS
            
            maxBigEyesSpawnNb = Config.Bind("Custom Behavior", "maxSpawnNumber", 
                2, 
                "BigEyes max spawn number in one moon. You don't need to restart the game !");
            CreateIntConfig(maxBigEyesSpawnNb, 1, 20);
            
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
                100f, 
                "BigEyes vision with. You don't need to restart the game !");
            CreateFloatConfig(visionWidthEntry, 1f, 500f);
            
            searchSpeedEntry = Config.Bind("Custom Behavior", "searchSpeed", 
                5f, 
                "BigEyes speed on search phase. See NavMeshAgent from Unity for more infos. You don't need to restart the game !");
            CreateFloatConfig(searchSpeedEntry);
            
            angrySpeedEntry = Config.Bind("Custom Behavior", "angrySpeed", 
                9f, 
                "BigEyes speed on angry phase. See NavMeshAgent from Unity for more infos. You don't need to restart the game !");
            CreateFloatConfig(angrySpeedEntry);
            
            normalAccelerationEntry = Config.Bind("Custom Behavior", "normalAcceleration", 
                10f, 
                "BigEyes acceleration on angry phase. See NavMeshAgent from Unity for more infos. You don't need to restart the game !");
            CreateFloatConfig(normalAccelerationEntry);
            
            angryAccelerationEntry = Config.Bind("Custom Behavior", "angryAcceleration", 
                12f, 
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

            bigEyes.MaxCount = maxBigEyesSpawnNb.Value;
            
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
        
        void RegisterScrap(AssetBundle bundle)
        {
            //smalleyes
            Item smallEyes = bundle.LoadAsset<Item>("Assets/LethalCompany/Mods/BigEyes/SmallEyesItem.asset");
            Logger.LogInfo($"{smallEyes.name} FOUND");
            Logger.LogInfo($"{smallEyes.spawnPrefab} prefab");
            NetworkPrefabs.RegisterNetworkPrefab(smallEyes.spawnPrefab);
            Utilities.FixMixerGroups(smallEyes.spawnPrefab);


            RegisterUtil.RegisterScrapWithConfig(scrapMoonRarity.Value, smallEyes ); 

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