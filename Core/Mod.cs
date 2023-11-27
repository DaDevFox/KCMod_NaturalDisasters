using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using System.Reflection;
using UnityEngine;

namespace Disasters
{
    public class Mod : MonoBehaviour
    {
        public static bool debug = false;

        public static Mod mod { get; private set; }
        public static string modID { get; } = "naturaldisastersmod";
        public static bool inited { get; private set; } = false;

        public static KCModHelper helper { get; private set; } = null;

        void Preload(KCModHelper _helper)
        {
            helper = _helper;
            mod = this;

            var harmony = HarmonyInstance.Create("harmony");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            Application.logMessageReceived += LogReceived;

            AssetBundleManager.UnpackAssetBundle();

            Log("Preload");
        }

        private void LogReceived(string condition, string stackTrace, LogType type)
        {
            if(type == LogType.Exception)
                Log(condition + "\n" + stackTrace);
        }

        public static void Log(object message)
        {
            if(helper != null)
                helper.Log(message.ToString());
        }

        void SceneLoaded(KCModHelper _helper)
        {
            //Events.EventManager.Init();
            //Settings.Init();
        }

        void Update()
        {
            if (!inited)
                Init();

            if (Settings.debug)
            {
                if (Input.GetKeyDown(KeyCode.T))
                    Events.EventManager.TriggerEvent(typeof(Events.EarthquakeEvent));
                
                if (Input.GetKeyDown(KeyCode.E))
                    Events.EventManager.TriggerEvent(typeof(Events.DroughtEvent));
                    
                if (Input.GetKeyDown(KeyCode.R))
                    Events.EventManager.TriggerEvent(typeof(Events.TornadoEvent));
                
                if(Input.GetKeyDown(KeyCode.M))
                    Events.EventManager.TriggerEvent(typeof(Events.MeteorEvent));
            }
        }

        private void Init()
        {
            Events.EventManager.Init();
            Settings.Init();
            inited = true;
        }


        [HarmonyPatch(typeof(Player), "OnNewYear")]
        public class YearPatch
        {

            static void Postfix()
            {
                Events.EventManager.OnYearEnd();
            }
        }

        #region Coroutines

        public static void StartDroughtFadeCoroutine(Weather.WeatherType type)
        {
            mod.StartCoroutine("DroughtFadeCoroutine", type);
        }

        private IEnumerator DroughtFadeCoroutine(Weather.WeatherType type)
        {
            float time = Weather.inst.TransitionTime;
            float elapsed = 0f;

            Color originalLightColor = (Color)typeof(global::Weather).GetField("originalLightColor", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(global::Weather.inst);
            float originalLightIntensity = (float)typeof(global::Weather).GetField("originalLightIntensity", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(global::Weather.inst);
            float originalLightShadowStrength = (float)typeof(global::Weather).GetField("originalLightShadowStrength", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(global::Weather.inst);

            float calculatedRainEmission = (float)typeof(global::Weather).GetField("calculatedRainEmission", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(global::Weather.inst);

            Timer lightningTimer = (Timer)typeof(global::Weather).GetField("lightningTimer", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(global::Weather.inst);

            global::Weather.inst.Rain.Stop();
            global::Weather.inst.Snow.Stop();
            Color endValue = originalLightColor;
            float endValue2 = originalLightIntensity;
            float endValue3 = originalLightShadowStrength;
            float endValue4 = 0f;
            if (type != global::Weather.WeatherType.Snow)
            {
                if (type != global::Weather.WeatherType.NormalRain)
                {
                    if (type == global::Weather.WeatherType.HeavyRain)
                    {
                        global::Weather.inst.Rain.emissionRate = calculatedRainEmission;
                        global::Weather.inst.Rain.startSize = 0.8f;
                        endValue4 = 1f;
                        global::Weather.inst.Rain.Play();
                        endValue = new Color(0.5f, 0.65f, 0.85f);
                        endValue2 = 0.75f;
                        endValue3 = 0.4f;
                    }
                }
                else
                {
                    global::Weather.inst.Rain.emissionRate = calculatedRainEmission / 3f;
                    global::Weather.inst.Rain.startSize = 0.5f;
                    endValue4 = 0.8f;
                    global::Weather.inst.Rain.Play();
                    endValue = new Color(0.8f, 0.8f, 1f);
                    endValue2 = 0.9f;
                    endValue3 = 0.45f;
                }
            }
            else
            {
                global::Weather.inst.Snow.Play();
            }

            global::Weather.inst.currentWeather = type;
            if (lightningTimer.Enabled && global::Weather.inst.currentWeather != global::Weather.WeatherType.HeavyRain)
            {
                global::Weather.inst.lightningMadeFire = false;
            }

            lightningTimer.Enabled = (global::Weather.inst.currentWeather == global::Weather.WeatherType.HeavyRain || global::Weather.inst.currentWeather == global::Weather.WeatherType.LightningStorm);
            global::Weather.inst.Invoke("DeferNotifyBuildingsWeatherChanged", 5f);

            Color baseLightColor = Color.white;
            float baseIntensity = -1f;
            float baseShadowStrength = -1f;
            float baseStormAlpha = -1f;

            while (elapsed < time)
            {
                if (baseLightColor == Color.white)
                    baseLightColor = Weather.inst.Light.color;
                Weather.inst.Light.color = Color.Lerp(baseLightColor, endValue, elapsed/time);

                if (baseIntensity == -1f)
                    baseIntensity = Weather.inst.Light.intensity;
                Weather.inst.Light.intensity = Mathf.Lerp(baseIntensity, endValue2, elapsed / time);
                if(baseShadowStrength == -1f)
                    baseShadowStrength = Weather.inst.Light.shadowStrength;
                Weather.inst.Light.shadowStrength = Mathf.Lerp(baseShadowStrength, endValue3, elapsed / time);
                if (baseStormAlpha == -1f)
                    baseStormAlpha = CloudSystem.inst.stormAlpha;
                CloudSystem.inst.stormAlpha = Mathf.Lerp(baseStormAlpha, endValue4, elapsed / time);

                elapsed += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }








        #endregion

    }
}
