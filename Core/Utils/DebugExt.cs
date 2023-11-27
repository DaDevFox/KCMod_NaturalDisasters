using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Disasters
{
    class DebugExt
    {

        private static List<int> IDs = new List<int>();

        public static void Log(string message, bool repeatable = false, KingdomLog.LogStatus type = KingdomLog.LogStatus.Neutral, object GameObjectOrVector3 = null)
        {
            KingdomLog.TryLog(Mod.modID + "_debugmsg-" + IDs.Count + (repeatable ? SRand.Range(0, 1).ToString() : ""), message, type, (repeatable ? 1 : 20), GameObjectOrVector3);
            IDs.Add(1);
        }

        public static void dLog(string message, bool repeatable = false, KingdomLog.LogStatus type = KingdomLog.LogStatus.Neutral, object GameObjectOrVector3 = null)
        {
            if (Mod.debug)
                Log(message, repeatable, type, GameObjectOrVector3);
        }


    }
}
