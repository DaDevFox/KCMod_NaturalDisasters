using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using System.Reflection;
using UnityEngine;
using I2.Loc;
using Assets.Code;

namespace Disasters
{
    public static class BuildingUtils
    {
        /// <summary>
        /// Returns a reference to the yield of a building
        /// </summary>
        /// <param name="building"></param>
        /// <returns></returns>
        public static ResourceAmount Yield(this Building building)
        {
            YieldProducer producer = building.GetComponent<YieldProducer>();
            YieldProducerSeason seasonalProducer = building.GetComponent<YieldProducerSeason>();

            if (producer)
                return producer.Yield;
            if (seasonalProducer)
                return ResourceAmount.Make(seasonalProducer.YieldType, seasonalProducer.YieldAmt);

            return new ResourceAmount();
        }

        /// <summary>
        /// Sets the yield of a building to a new ResourceAmount. 
        /// <para>May only be a ResourceAmount with one resource type if targeting a seasonal producer (field/agricultural producer typically). </para>
        /// </summary>
        /// <param name="building"></param>
        /// <param name="amount"></param>
        public static void Yield(this Building building, ResourceAmount amount)
        {
            YieldProducer producer = building.GetComponent<YieldProducer>();
            YieldProducerSeason seasonalProducer = building.GetComponent<YieldProducerSeason>();
            if (producer)
                producer.Yield = amount;
            if (seasonalProducer)
            {
                FreeResourceType type = FreeResourceType.Apples;
                for (int i = 0; i < (int)FreeResourceType.NumTypes; i++)
                    if (amount.Get((FreeResourceType)i) > 0)
                        type = (FreeResourceType)i;
                seasonalProducer.YieldType = type;
                seasonalProducer.YieldAmt = amount.Get(type);
            }
        }
    }
}

namespace Disasters.Events
{
    public class DroughtEvent : ModEvent
    {
        public static event Action start;
        public static event Action end;

        private static int timeRemaining = 0;
        public static bool droughtRunning = false;

        public static float originalWaterHeight = -0.25f;
        public static float droughtWaterHeight = -0.78f;
        public static Color droughtSkyColor = Color.yellow;

        public static Color droughtSunColor = Color.yellow;
        public static float droughtSunIntensity = 0.9f;
        public static float droughtSunShadows = 0.8f;

        public static float droughtBloom = 2f;
        public static MinMax droughtFog = new MinMax(0f, 45f);

        public static Dictionary<string, ResourceAmount> foodBuildingYields = new Dictionary<string, ResourceAmount>()
        {
            { "farm", ResourceAmount.Make(FreeResourceType.Wheat,4)},
            { "orchard", ResourceAmount.Make(FreeResourceType.Apples,18)},
        };

        public override int testFrequency => 1;

        public override Type saveObject => typeof(DroughtSaveData);
        public override string saveID => "drought";

        public override bool Test()
        {
            base.Test();
            
            if (SRand.Range(0, 1f) < Settings.droughtChance)
                return true;

            timeRemaining -= 1;

            if (timeRemaining <= 0)
                End();    

            return false;
        }

        public override void Init()
        {
            base.Init();

            end += Fires.End;

            originalWaterHeight = (float)typeof(global::Weather).GetField("originalWaterHeight", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(global::Weather.inst);
        }

        public override void Run()
        {
            base.Run();

            if (!Settings.droughts)
                return;

            timeRemaining = (int)Settings.droughtLength.Rand();
            droughtRunning = true;

            KingdomLog.TryLog("drought", "My Lord, a terrible drought has struck our land, for the next <color=yellow>" + timeRemaining.ToString() + " " + (timeRemaining == 1 ? "year" : "years") + "</color>, our harvest will be poor!", KingdomLog.LogStatus.Neutral, 1f);
            start?.Invoke();
        }


        private void End()
        {
            timeRemaining = 0;

            if (droughtRunning)
            {
                droughtRunning = false;

                KingdomLog.TryLog("droughtend", "<color=green>The peasants rejoice! The drought upon our land has ended!</color>", KingdomLog.LogStatus.Neutral);

                end?.Invoke();
            }
        }

        #region Patches

        #region Weather

        [HarmonyPatch(typeof(global::Weather))]
        [HarmonyPatch("Update")]
        class Weather
        {
            static void Postfix(global::Weather __instance)
            {
                // FX
                if (droughtRunning)
                {
                    typeof(global::Weather).GetMethod("LerpSkyColor", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(global::Weather.inst, new object[] { droughtSkyColor, Time.deltaTime * 0.5f });
                    typeof(global::Weather).GetMethod("LerpBloomTo", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(global::Weather.inst, new object[] { droughtBloom, Time.deltaTime * 0.5f });
                    //typeof(global::Weather).GetMethod("LerpFogTo", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(global::Weather.inst, new object[] { droughtFog.Max, droughtFog.Min, Time.deltaTime * 0.5f });

                    global::Weather.inst.Light.color = Color.Lerp(global::Weather.inst.Light.color, droughtSunColor, Time.deltaTime * 0.5f);
                    global::Weather.inst.Light.intensity = Mathf.Lerp(global::Weather.inst.Light.intensity, droughtSunIntensity, Time.deltaTime * 0.5f);
                    global::Weather.inst.Light.shadowStrength = Mathf.Lerp(global::Weather.inst.Light.shadowStrength, droughtSunShadows, Time.deltaTime * 0.5f);
                }

                // Water Height
                if (global::Weather.CurrentWeather == global::Weather.WeatherType.None || global::Weather.CurrentWeather == global::Weather.WeatherType.Snow)
                {
                    if (droughtRunning)
                    {
                        Vector3 vector = __instance.Water.transform.position;
                        vector = Vector3.Lerp(vector, new Vector3(vector.x, droughtWaterHeight, vector.z), Time.deltaTime * 0.25f);
                        __instance.Water.transform.position = vector;
                    }
                    else
                    {
                        Vector3 vector = __instance.Water.transform.position;
                        vector = Vector3.Lerp(vector, new Vector3(vector.x, originalWaterHeight, vector.z), Time.deltaTime * 0.25f);
                        __instance.Water.transform.position = vector;
                    }
                }

                // Don't allow rain
                if(droughtRunning)
                    if (Settings.droughtsAffectWeather)
                        if (global::Weather.CurrentWeather == global::Weather.WeatherType.NormalRain || global::Weather.CurrentWeather == global::Weather.WeatherType.HeavyRain || global::Weather.CurrentWeather == global::Weather.WeatherType.LightningStorm)
                            global::Weather.CurrentWeather = global::Weather.WeatherType.None;

                
            }
        }

        [HarmonyPatch(typeof(global::Weather), "ChangeWeather")]
        class WeatherLightingCorrection
        {
            static bool Prefix(global::Weather.WeatherType type)
            {
                Mod.StartDroughtFadeCoroutine(type);
                
                return false;
            }
        }

        #endregion

        [HarmonyPatch(typeof(Fire), "Tick")]
        static class Fires
        {
            static void Postfix(Fire __instance)
            {
                if (droughtRunning)
                    typeof(Fire).GetField("spreadTime", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, 15f / (2.5f * Settings.droughtFirePenalty));
            }

            public static void End()
            {
                foreach (Fire fire in FireManager.inst.fireContainer.GetComponentsInChildren<Fire>())
                    typeof(Fire).GetField("spreadTime", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(fire, 15f / 2.5f);
            }
        }
        

        #region Food Buildings

        [HarmonyPatch(typeof(Field))]
        [HarmonyPatch("Tick")]
        class Fields
        {
            static void Postfix(Field __instance)
            {
                if (DroughtEvent.droughtRunning)
                {
                    Building b = __instance.GetComponent<Building>();
                    b.Yield(ResourceAmount.Make(FreeResourceType.Wheat, foodBuildingYields[b.UniqueName].Get(FreeResourceType.Wheat) - Settings.droughtFoodPenalty.Get(FreeResourceType.Wheat)));
                }
                else
                {
                    Building b = __instance.GetComponent<Building>();
                    b.Yield (foodBuildingYields[b.UniqueName]);
                }
            }
        }


        [HarmonyPatch(typeof(Orchard))]
        [HarmonyPatch("Tick")]
        class Orchards
        {
            static void Postfix(Orchard __instance)
            {
                if (DroughtEvent.droughtRunning)
                {
                    Building b = __instance.GetComponent<Building>();
                    b.Yield(ResourceAmount.Make(FreeResourceType.Apples, foodBuildingYields[b.UniqueName].Get(FreeResourceType.Apples) - Settings.droughtFoodPenalty.Get(FreeResourceType.Apples)));
                }
                else
                {
                    Building b = __instance.GetComponent<Building>();
                    b.Yield(foodBuildingYields[b.UniqueName]);
                }
            }
        }



        //Disable Fishing Boats during a drought
        [HarmonyPatch(typeof(FishingShip))]
        [HarmonyPatch("Tick")]
        class Fishing
        {
            static void Prefix(FishingShip __instance)
            {
                if (droughtRunning && Settings.droughtsDisableFishing)
                    __instance.status = FishingShip.Status.WaitAtHutFull;
            }
        }

        #region Text

        //Change fishing hut text during drought
        [HarmonyPatch(typeof(FishingHut))]
        [HarmonyPatch("GetExplanation")]
        class FishingText
        {
            static void Postfix(ref string __result)
            {
                if (droughtRunning && Settings.droughtsDisableFishing)
                    __result = "<color=yellow>Waters too shallow to fish: waiting until drought ends to resume </color>";
            }
        }

        // Explain nullified orchard properly
        [HarmonyPatch(typeof(Orchard), "GetExplanation")]
        class OrchardText
        {
            static void Postfix(ref string __result)
            {
                if (droughtRunning && Settings.droughtFoodPenalty.Get(FreeResourceType.Apples) == 18)
                    __result = "<color=yellow>Orchard infertile due to drought; no yield will be produced </color>";
            }
        }

        // Explain nullified orchard properly
        [HarmonyPatch(typeof(Field), "GetExplanation")]
        class FieldText
        {
            static void Postfix(ref string __result)
            {
                if (droughtRunning && Settings.droughtFoodPenalty.Get(FreeResourceType.Wheat) == 4)
                    __result = "<color=yellow>Fields dry due to drought; no yield will be produced </color>";
            }
        }

        #endregion

        #endregion

        #endregion

        #region LoadSave

        public class DroughtSaveData 
        {
            public bool droughtRunning;
            public int timeRemaining;
        }


        public override void OnLoaded(object saveData)
        {
            base.OnLoaded(saveData);
            DroughtSaveData data = saveData as DroughtSaveData;

            DroughtEvent.timeRemaining = data.timeRemaining;
            DroughtEvent.droughtRunning = data.droughtRunning;
        }

        public override object OnSave()
        {
            DroughtSaveData data = new DroughtSaveData();
            data.timeRemaining = DroughtEvent.timeRemaining;
            data.droughtRunning = DroughtEvent.droughtRunning;
            return data;
        }

        #endregion
    }
}
