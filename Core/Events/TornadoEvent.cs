using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Disasters.Events
{
    class TornadoEvent : ModEvent
    {
        public static bool tornadoRunning = false;
        public static GameObject tornadoPrefab;
        public static Tornado tornado;

        public override string saveID => "tornado";
        public override Type saveObject => typeof(TornadoEventSaveData);

        public override void Init()
        {
            base.Init();

            tornadoPrefab = AssetBundleManager.GetAsset("Tornado.prefab") as GameObject;
        }


        public override bool Test()
        {
            return Util.Randi() < Settings.tornadoChance;
        }

        public override void Run()
        {
            base.Run();

            if (!tornadoRunning)
            {
                tornadoRunning = true;
                DebugExt.Log("creating tornado");
                int landmass = SRand.Range(0, World.inst.NumLandMasses);
                Vector3 pos = World.inst.cellsToLandmass[landmass].RandomElement().Center;

                tornado = SpawnFreeRoamTornado(pos);
            }

        }

        private FreeRoamTornado SpawnFreeRoamTornado(Vector3 position)
        {
            GameObject tornadoObj = GameObject.Instantiate(tornadoPrefab,position, Quaternion.identity, World.inst.caveContainer.transform);
            FreeRoamTornado tornado = tornadoObj.AddComponent<FreeRoamTornado>();
            return tornado;
        }


        public static void OnTornadoDeath(GameObject tornado)
        {
            tornadoRunning = false;
            TornadoEvent.tornado = null;
        }


        #region SaveLoad

        public class TornadoEventSaveData
        {
            public bool tornadoRunning;
        }

        #endregion


    }
}
