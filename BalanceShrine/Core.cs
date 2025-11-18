using Il2Cpp;
using Il2CppAssets.Scripts.Game.MapGeneration;
using Il2CppAssets.Scripts.Game.Spawning;
using Il2CppAssets.Scripts.Steam.LeaderboardsNew;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MelonLoader;
using MelonLoader.Preferences;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Il2CppSystem.Linq.Expressions.Interpreter.CastInstruction.CastInstructionNoT;


[assembly: MelonInfo(typeof(BalanceShrine.BalanceShrine), "BalanceShrine", "1.0.0", "Potatoe_Man", null)]
[assembly: MelonGame("Ved", "Megabonk")]

namespace BalanceShrine
{
    internal static class Preferences
    {
        private const int CMaxShrines = 6;

        internal static MelonPreferences_Category BalanceShrineCategory;

        internal static MelonPreferences_Entry<int> MaxShrines;

        internal static void Setup()
        {
            BalanceShrineCategory = MelonPreferences.CreateCategory("BalanceShrine");

            MaxShrines = BalanceShrineCategory.CreateEntry<int>
                ("MaxShrines", CMaxShrines, description: $"Max amount of shrines, amount is between 1 - MaxShrines", validator: new ValueRange<int>(0, 999));
        }
    }

    public class BalanceShrine : MelonMod
    {
        public override void OnEarlyInitializeMelon()
        {
            Preferences.Setup();
        }

        public override void OnInitializeMelon()
        {
            HarmonyInstance.PatchAll();
            Melon<BalanceShrine>.Logger.Warning("THIS IS A CHEAT MOD!!!");
            Melon<BalanceShrine>.Logger.Warning("Leaderboards are disabled.");
        }

        private SceneState _sceneState;

        private static bool inRound = false;

        private static string ShrineBalance = "ShrineBalance";
        private static GameObject ShrineBal;
        public Vector3 LastValue;

        internal static int MaxShrines => Preferences.MaxShrines.Value;

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            switch (buildIndex)
            {
                case -1: //None
                    _sceneState = SceneState.None;
                    break;
                case 0: //Boot
                    _sceneState = SceneState.Boot;
                    break;
                case 1: //Main menu
                    _sceneState = SceneState.MainMenu;
                    break;
                case 2: //Level
                    _sceneState = SceneState.Level;
                    OnLevelLoaded();
                    break;
                case 3:
                    _sceneState = SceneState.LoadingScreen;
                    break;
            }
        }

        public void print<T>(T msg)
        {
            Melon<BalanceShrine>.Logger.Msg(msg);
        }

        private void OnLevelLoaded()
        {
            MelonCoroutines.Start(WaitForObject("Player"));
        }



        private System.Collections.IEnumerator WaitForObject(string objectName)
        {
            var random = new System.Random();
            var value = random.Next(1, MaxShrines);
            print(value);

            for (int i = 0; i < value; i++)
            {
                if (ShrineBal != null)
                {
                    print(2);
                    var BalanceShrine = GameObject.Instantiate(ShrineBal);
                    SceneManager.MoveGameObjectToScene(BalanceShrine, SceneManager.GetActiveScene());

                    var mapgenobj = GameObject.Find("RandomMapGeneration");
                    var mapgen = mapgenobj.GetComponent<MapGenerationController>();
                    var objectplacer = mapgen.randomObjectPlacer;
                    var randomobject = objectplacer.randomObjects[0];
                    var val1 = randomobject.amount;
                    var val2 = randomobject.prefabs[0];
                    var val3 = randomobject.maxAmount;

                    if (BalanceShrine != null)
                    {
                        print(3);
                        BalanceShrine.transform.position = SpawnPositions.GetRandomSpawnPositionOnMap();

                        while (LastValue == BalanceShrine.transform.position)
                        {
                            BalanceShrine.transform.position = SpawnPositions.GetRandomSpawnPositionOnMap();
                            print(BalanceShrine.transform.position);
                            yield return new WaitForSeconds(0.01f);
                        }

                        LastValue = BalanceShrine.transform.position;

                        BalanceShrine.transform.Find("MinimapIcon (1)").gameObject.SetActive(true);
                        BalanceShrine.transform.Find("AltarIcon (1)").gameObject.SetActive(true);

                        //randomobject.amount = value;
                        //randomobject.maxAmount = value;
                        //randomobject.prefabs[0] = BalanceShrine;

                        //objectplacer.RandomObjectSpawner(randomobject);

                        //randomobject.amount = val1;
                        //randomobject.prefabs[0] = val2;
                        //randomobject.maxAmount = val3;
                    }
                }
            } 
            yield break;
        }

        public override void OnUpdate()
        {
            /*if (Input.GetKeyDown(KeyCode.Q))
            {
                if (ShrineBal != null)
                {
                    var shrine = GameObject.Instantiate(ShrineBal);
                    SceneManager.MoveGameObjectToScene(shrine, SceneManager.GetActiveScene());

                    var otherguy = GameObject.Find("ChargeShrine(Clone)");
                    if (otherguy != null)
                    {
                        print(otherguy.name);

                        if (shrine != null)
                        {
                            shrine.transform.position = SpawnPositions.GetRandomSpawnPositionOnMap(0f);

                            shrine.transform.Find("MinimapIcon (1)").gameObject.SetActive(true);
                            shrine.transform.Find("AltarIcon (1)").gameObject.SetActive(true);
                        }
                    }
                }
            }*/

            if (GameObject.Find(ShrineBalance) != null)
            {
                if (!ShrineBal)
                {
                    print("we found it");
                    ShrineBal = GameObject.Instantiate(GameObject.Find(ShrineBalance));
                    GameObject.DontDestroyOnLoad(ShrineBal);
                    ShrineBal.name = "NewBalShrine";
                    ShrineBal.transform.position = new Vector3(5000f, 5000f, 5000f);
                    print(ShrineBal.name);
                }
            }
        }

        //bool Sus.CheckMods(out string reason)
        [HarmonyLib.HarmonyPatch(typeof(Sus), nameof(Sus.CheckMods))]
        private static class EditSusPatch2
        {
            private static bool _onlyOnce = false;

            private static void Postfix(ref bool __result, ref string reason)
            {
                if (__result)
                    Melon<BalanceShrine>.Logger.Warning($"The internal anti-cheat considers you sus, reason: {reason}");
                else
                    Melon<BalanceShrine>.Logger.Msg("The internal anti-cheat hasn't found anything sus");
            }
        }

        // Harmony patch to disable UploadScore, since your a CHEATER   
        //void SteamLeaderboardsManagerNew.UploadLeaderboardScore(string leaderboardName, int score, Il2CppStructArray<int> details, bool isFriendsLb)
        [HarmonyLib.HarmonyPatch(typeof(SteamLeaderboardsManagerNew), nameof(SteamLeaderboardsManagerNew.UploadLeaderboardScore), new Type[] { typeof(string), typeof(int), typeof(Il2CppStructArray<int>), typeof(bool) })]
        internal static class DisableSteamLeaderboardUploading
        {
            private static bool Prefix(string leaderboardName, int score, Il2CppStructArray<int> details, bool isFriendsLb)
            {
                return false;
            }
        }

        private enum SceneState
        {
            None,
            Boot,
            MainMenu,
            Level,
            LoadingScreen
        }
    }
}