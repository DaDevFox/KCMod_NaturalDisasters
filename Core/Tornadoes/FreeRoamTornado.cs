using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace Disasters
{
    class FreeRoamTornado : TargetTornado
    {
        public float boundRadius = 40f;
        public float changeTargetChance = 0.02f;
        public float changeTargetInterval = 3f;
        public int landmass;

        private float changeDirectionTime = 0f;
        
        public override void OnStart()
        {
            base.OnStart();
            TryRandomTarget();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            if(changeDirectionTime > changeTargetInterval)
            {
                TestChangeTarget();
                changeDirectionTime = 0f;
            }
            changeDirectionTime += Time.deltaTime;
        }

        private void TestChangeTarget()
        {
            if (Util.Randi() <= changeTargetChance)
            {
                TryRandomTarget();
            }
        }

        private void TryRandomTarget()
        {
            Vector3 pos = World.inst.cellsToLandmass[landmass].RandomElement().Center;
            base.target = pos;
            base.UpdateDirectionalVelocity();
            DebugExt.Log("Target: " + target.ToString(),true,KingdomLog.LogStatus.Neutral,target);
        }


    }
}
