using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disasters.Events
{
    public static class EventManager
    {
        public static List<ModEvent> events = new List<ModEvent>()
        {
            new EarthquakeEvent(),
            new DroughtEvent(),
            //new TornadoEvent(),
            new MeteorEvent()
        };

        public static void OnYearEnd() 
        {
            foreach (ModEvent _event in events) 
                if (_event.Test() && Player.inst.CurrYear % _event.testFrequency == 0)
                    TryRun(_event);
        }

        public static void Init()
        {
            foreach (ModEvent _event in events) 
                _event.Init();

            Broadcast.OnLoadedEvent.Listen(OnLoaded);
            Broadcast.OnSaveEvent.Listen(OnSave);
        }

        /// <summary>
        /// Triggers an event of type <c>_event</c>. 
        /// <para>If trigger first is enabled, will trigger the first registered event found of type <c>_event</c>, if not, will trigger a random registered event of type <c>_event</c></para>
        /// <para>If include descendants is enabled, subclasses of type <c>_event</c> will also be included</para>
        /// </summary>
        /// <param name="_event"></param>
        /// <param name="triggerFirst"></param>
        public static void TriggerEvent(Type _event, bool triggerFirst = false, bool includeDescendants = false)
        {
            if (triggerFirst)
            {
                foreach (ModEvent __event in events)
                    if ((!includeDescendants && __event.GetType() == _event) || 
                        (includeDescendants && __event.GetType().IsSubclassOf(_event)))
                        TryRun(__event);
            }
            else
            {
                List<ModEvent> _events = new List<ModEvent>();
                foreach (ModEvent __event in events)
                    if ((!includeDescendants && __event.GetType() == _event) ||
                        (includeDescendants && __event.GetType().IsSubclassOf(_event)))
                        _events.Add(__event);
                TryRun(_events[UnityEngine.Random.Range(0, _events.Count - 1)]);
            }
        }

        public static void OnLoaded(object sender, OnLoadedEvent loadedEvent)
        {
            foreach (ModEvent _event in events) 
            {
                Type saveObjType = _event.saveObject;
                string data = LoadSave.ReadDataGeneric(Mod.modID, _event.saveID);
                if (data != null)
                    _event.OnLoaded(Newtonsoft.Json.JsonConvert.DeserializeObject(data, saveObjType));
            }
        }

        public static void OnSave(object sender, OnSaveEvent saveEvent)
        {
            foreach (ModEvent _event in events)
                LoadSave.SaveDataGeneric(Mod.modID, _event.saveID,Newtonsoft.Json.JsonConvert.SerializeObject(_event.OnSave()));
        }

        

        private static void TryRun(ModEvent _event)
        {
            try
            {
                _event.Run();
            }
            catch (Exception ex)
            {
                Mod.helper.Log("Error triggering Event " + _event.saveID + "\n" + ex.Message + "\n" + ex.StackTrace);
            }
        }

    }
}
