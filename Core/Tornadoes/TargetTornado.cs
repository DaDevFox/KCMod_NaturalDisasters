using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Disasters
{
    class TargetTornado : Tornado
    {
        public Vector3 target;


        public override void OnStart()
        {
            base.OnStart();
            
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            UpdateDirectionalVelocity();
        }


        protected void UpdateDirectionalVelocity()
        {
            Vector3 diff = target - transform.position;
            Vector3 diffClamped = Vector3.ClampMagnitude(diff, directionalVelocityRange.Max);

            SetDirectionalVelocity(diffClamped);
        }

        


    }
}
