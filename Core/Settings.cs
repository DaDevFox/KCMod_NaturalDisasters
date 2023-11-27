#define ADVANCED_SETTINGS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using Zat.Shared.ModMenu.Interactive;
using Zat.Shared.ModMenu.API;
using Assets.Code;

namespace Disasters
{
    public abstract class WeightedMinMax
    {
        public float min;
        public float max;
        
        public abstract float Weight(float val);

        public WeightedMinMax(float min, float max)
        {
            this.min = min;
            this.max = max;
        }

        public float Rand() => Weight(UnityEngine.Random.Range(min, max));
    }

    /// <summary>
    /// Weights the lower end of the data set when choosing a random number
    /// </summary>
    public class ExponentialMinMax : WeightedMinMax
    {
        private float? anchor;

        public ExponentialMinMax(float min, float max, float? anchor = null) : base(min, max)
        {
            this.anchor = anchor;
        }

        public override float Weight(float val)
        {
            // y = 1/(max) * ([x-min]^2)
            val -= min;
            return (1f / min) * (val * val);
        }
    }

    [Mod("Natural Disasters", "0.1", "Fox")]
    public class Settings
    {
        public static Settings instance { get; private set; }
        public InteractiveConfiguration<Settings> config { get; private set; }
        public ModSettingsProxy proxy { get; private set; }

        public static bool debug = false;

        #region Constants

        public static float waterRestingHeight { get; } = -0.65f;
        public static MinMax earthquakeVariance { get; } = new MinMax(0.1f, 0.9f);
        public static MinMax landElevation { get; } = new MinMax(0f, 0.3f);
        public static MinMax waterElevation { get; } = new MinMax(-2f, -0.25f);

        #endregion

        #region Mod Settings

        // Earthquakes
        public static bool earthquakes => instance != null ? instance.c_earthquakes.active : true;
        public static float earthquakeChance => instance != null ? instance.c_earthquakes.chance : 0.15f;
        public static MinMax earthquakeStrength => instance != null ? new MinMax(instance.c_earthquakes.strengthMin, instance.c_earthquakes.strengthMax) : new MinMax(1f, 4f);

        // Droughts
        public static bool droughts => instance != null ? instance.c_droughts.active : true;
        public static float droughtChance => instance != null ? instance.c_droughts.chance : 0.05f;
        public static MinMax droughtLength => instance != null ? new MinMax(instance.c_droughts.lengthMin, instance.c_droughts.lengthMax) : new MinMax(1f, 5f);
        public static ResourceAmount droughtFoodPenalty
        {
            get
            {
#if ADVANCED_SETTINGS

                if (instance == null)
                {
#endif

                    ResourceAmount amount = new ResourceAmount();

                    amount.Set(FreeResourceType.Wheat, 4);
                    amount.Set(FreeResourceType.Apples, 4);

                    return amount;
#if ADVANCED_SETTINGS    

            }
                else
                {
                    ResourceAmount amount = new ResourceAmount();

                    amount.Set(FreeResourceType.Wheat, instance.c_droughts.fieldPenalty);
                    amount.Set(FreeResourceType.Apples, instance.c_droughts.orchardPenalty);

                    return amount;
                }
#endif

            }


        }




        public static bool droughtsDisableFishing =>
#if ADVANCED_SETTINGS 
            instance != null ? instance.c_droughts.disableFishing : 
            #endif
            false;
        public static bool droughtsAffectWeather =>
#if ADVANCED_SETTINGS
            instance != null ? instance.c_droughts.affectWeather :
#endif
            true;
        public static float droughtFirePenalty =>
#if ADVANCED_SETTINGS
            instance != null ? instance.c_droughts.firePenalty :
#endif
            4f;

        // Meteors
        public static bool meteors => instance != null ? instance.c_meteors.active : true;
        public static float meteorChance => instance != null ? instance.c_meteors.chance : 0.05f;
        public static MinMax meteorSize = new MinMax(2f, 13f);
        public static MinMax meteorArmLength = new MinMax(2f, 9f);

        // Tornadoes
        public static float tornadoChance = 0.1f;

        public static bool happinessMods = true;

#endregion

#region Interactive

        [Category("Earthquakes")]
        public Earthquakes c_earthquakes { get; private set; }

        [Category("Droughts")]
        public Droughts c_droughts { get; private set; }

        [Category("Meteors")]
        public Meteors c_meteors { get; private set; }

        public class Earthquakes
        {
            [Setting("Active", "Whether or not earthquakes will happen")]
            [Toggle(true)]
            public InteractiveToggleSetting s_active { get; private set; }
            public bool active => s_active.Value;

            [Setting("Chance", "Chance of an earthquake happening per year")]
            [Slider(0.01f, 1f, 0.15f)]
            public InteractiveSliderSetting s_chance { get; private set; }
            public float chance => s_chance.Value;

            [Setting("Earthquake Strength Min")]
            [Slider(0.1f, 4f, 1f)]
            public InteractiveSliderSetting s_strengthMin { get; private set; }
            public float strengthMin => s_strengthMin.Value;

            [Setting("Earthquake Strength Max")]
            [Slider(0.1f, 4f, 4f)]
            public InteractiveSliderSetting s_strengthMax { get; private set; }
            public float strengthMax => s_strengthMax.Value;
        }

        public class Droughts
        {
            [Setting("Active")]
            [Toggle(true)]
            public InteractiveToggleSetting s_active { get; private set; }
            public bool active => s_active.Value;

            [Setting("Chance", "Chance of a drought happening per year")]
            [Slider(0.01f, 1f, 0.05f)]
            public InteractiveSliderSetting s_chance { get; private set; }
            public float chance => s_chance.Value;

            [Setting("Drought Length Min")]
            [Slider(1f, 10f, 1f)]
            public InteractiveSliderSetting s_lengthMin { get; private set; }
            public float lengthMin => s_lengthMin.Value;

            [Setting("Drought Length Max")]
            [Slider(0.1f, 10f, 5f)]
            public InteractiveSliderSetting s_lengthMax { get; private set; }
            public float lengthMax => s_lengthMax.Value;

#if ADVANCED_SETTINGS

            [Setting("Advanced/Fields", "How much droughts affect the base output of fields; 100% = nullified, no yield")]
            [Slider(1f, 4f, 4f, "nullified", true)]
            public InteractiveSliderSetting s_fieldPenalty { get; private set; }
            public int fieldPenalty => (int)s_fieldPenalty.Value;

            [Setting("Advanced/Orchards", "How much droughts affect the base output of orchards; 100% = nullified, no yield")]
            [Slider(1f, 18, 4f, "22% penalty", true)]
            public InteractiveSliderSetting s_orchardPenalty { get; private set; }
            public int orchardPenalty => (int)s_orchardPenalty.Value;

            [Setting("Advanced/Disable Fishing Huts")]
            [Toggle(false)]
            public InteractiveToggleSetting s_disableFishing { get; private set; }
            public bool disableFishing => (bool)s_disableFishing.Value;

            [Setting("Advanced/Affect Weather")]
            [Toggle(true)]
            public InteractiveToggleSetting s_affectWeather { get; private set; }
            public bool affectWeather => (bool)s_affectWeather.Value;

            [Setting("Advanced/Fires", "How much faster fires spread")]
            [Slider(1f, 6f, 4f)]
            public InteractiveSliderSetting s_firePenalty { get; private set; }
            public float firePenalty => s_firePenalty.Value;

#endif

        }

        public class Meteors
        {
            [Setting("Active")]
            [Toggle(true)]
            public InteractiveToggleSetting s_active { get; private set; }
            public bool active => s_active.Value;

            [Setting("Chance", "Chance of a meteor happening per year")]
            [Slider(0.01f, 1f, 0.05f)]
            public InteractiveSliderSetting s_chance { get; private set; }
            public float chance => s_chance.Value;
        }

#endregion

        public static void Init()
        {
            var config = new InteractiveConfiguration<Settings>();
            Settings.instance = config.Settings;
            Settings.instance.config = config;

            AddListeners();

            ModSettingsBootstrapper.Register(config.ModConfig,
                (proxy, oldSettings) =>
                {
                    OnModRegistered(proxy, oldSettings);
                },
                (ex) => {
                    OnModRegistrationFailed(ex);
                });
        }

        private static void AddListeners()
        {
            Settings.instance.c_earthquakes.s_chance.OnUpdatedRemotely.AddListener((entry) => UpdatePercentageSlider(entry));
            Settings.instance.c_droughts.s_chance.OnUpdatedRemotely.AddListener((entry) => UpdatePercentageSlider(entry));
            Settings.instance.c_meteors.s_chance.OnUpdatedRemotely.AddListener((entry) => UpdatePercentageSlider(entry));

#if ADVANCED_SETTINGS

            Settings.instance.c_droughts.s_fieldPenalty.OnUpdatedRemotely.AddListener(entry => UpdateYieldPenaltySlider(entry, 4));
            Settings.instance.c_droughts.s_orchardPenalty.OnUpdatedRemotely.AddListener(entry => UpdateYieldPenaltySlider(entry, 18));
            Settings.instance.c_droughts.s_firePenalty.OnUpdatedRemotely.AddListener(entry => SetPercentageSlider(entry, entry.slider.value * 100f));

#endif

            AddMinMaxListeners(Settings.instance.c_earthquakes.s_strengthMin, Settings.instance.c_earthquakes.s_strengthMax);
            AddMinMaxListeners(Settings.instance.c_droughts.s_lengthMin, Settings.instance.c_droughts.s_lengthMax);
        }

        private static void AddMinMaxListeners(InteractiveSliderSetting min, InteractiveSliderSetting max)
        {
            min.OnUpdatedRemotely.AddListener((entry) =>
            {
                UpdateSlider(entry);
                OnMinMaxUpdate(min, max);
            });

            max.OnUpdatedRemotely.AddListener((entry) =>
            {
                UpdateSlider(entry);
                OnMinMaxUpdate(min, max);
            });
        }

        private static void OnModRegistered(ModSettingsProxy proxy, SettingsEntry[] oldSettings)
        {
            Settings.instance.proxy = proxy;
            Settings.instance.config.Install(proxy, oldSettings);

            UpdateAll();

            Mod.Log("Mod registration to ModMenu Succesful");
        }

        private static void OnModRegistrationFailed(Exception ex)
        {
            Mod.helper.Log("Mod registration to ModMenu failed");
            Mod.helper.Log(ex.ToString());
        }

        public static void UpdateSlider(SettingsEntry entry, float factor = 0.01f)
        {
            entry.slider.label = Util.RoundToFactor(entry.slider.value, 0.01f).ToString();
            Update(entry);
        }

        public static void Update(SettingsEntry entry)
        {
            Settings.instance.proxy.UpdateSetting(entry, () => { }, (ex) => Mod.helper.Log(ex.ToString()));
        }

        public static void UpdatePercentageSlider(SettingsEntry slider, float factor = 0.1f)
        {
            slider.slider.label = $"{Util.RoundToFactor(slider.slider.value * 100f, factor)}%";
            Settings.instance.proxy.UpdateSetting(slider, () => { }, (ex) => Mod.helper.Log(ex.ToString()));
        }

        /// <summary>
        /// Sets a slider to label it with the percentage value <c>percent</c> (1xx not 0.xx)
        /// </summary>
        /// <param name="slider"></param>
        /// <param name="percent"></param>
        /// <param name="factor">rounds the value to that factor</param>
        public static void SetPercentageSlider(SettingsEntry slider, float percent, float factor = 0.1f)
        {
            slider.slider.label = $"{Util.RoundToFactor(percent, factor)}%";
            Settings.instance.proxy.UpdateSetting(slider, () => { }, (ex) => Mod.helper.Log(ex.ToString()));
        }

        public static void UpdateYieldPenaltySlider(SettingsEntry slider, int maximumYieldLoss = 4)
        {
            slider.slider.label = (int)slider.slider.value == maximumYieldLoss ? "nullified" :  $"{Util.RoundToFactor(slider.slider.value / maximumYieldLoss, 0.01f)}% penalty";
            Settings.instance.proxy.UpdateSetting(slider, () => { }, (ex) => Mod.helper.Log(ex.ToString()));
        }

        public static void SetMinMax(InteractiveSliderSetting min, InteractiveSliderSetting max, MinMax newValue)
        {
            min.Value = newValue.Min;
            max.Value = newValue.Max;
        }

        public static void OnMinMaxUpdate(InteractiveSliderSetting min, InteractiveSliderSetting max)
        {
            if (min.Value > max.Value)
            {
                max.Value = min.Value;
                min.TriggerUpdate();
            }

            if (max.Value < min.Value)
            {
                min.Value = max.Value;
                max.TriggerUpdate();
            }
        }

        private static void UpdateAll()
        {
            Settings.instance.c_earthquakes.s_chance.TriggerUpdate();
            Settings.instance.c_droughts.s_chance.TriggerUpdate();

            Settings.instance.c_earthquakes.s_strengthMin.TriggerUpdate();
            Settings.instance.c_earthquakes.s_strengthMax.TriggerUpdate();

            Settings.instance.c_droughts.s_lengthMin.TriggerUpdate();
            Settings.instance.c_droughts.s_lengthMax.TriggerUpdate();

#if ADVANCED_SETTINGS

            Settings.instance.c_droughts.s_fieldPenalty.TriggerUpdate();
            Settings.instance.c_droughts.s_orchardPenalty.TriggerUpdate();
            Settings.instance.c_droughts.s_firePenalty.TriggerUpdate();
#endif

        }
    }
}
