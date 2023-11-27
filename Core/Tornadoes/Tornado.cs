using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Disasters
{
    public class Tornado : MonoBehaviour
    {
        public bool active { get; set; } = true;
        public virtual float deathSpeedThreshold { get; set; } = 0.05f;

        public Vector3 spawnPos = Vector3.zero;

        /// <summary>
        /// How long the tornado particles linger after death
        /// </summary>
        public virtual float lingerTime { get; set; } = 20f;

        /// <summary>
        /// Maximum amount of time a tornado can spend under a certain velocity, <c>deathSpeedThreshold</c> without dying
        /// </summary>
        public virtual float deathTimeMax { get; set; } = 20f;
        /// <summary>
        /// Accumulation of time spent below the <c>deathSpeedThreshold</c>; kills tornado if it is greater <c>deathTimeMax</c>
        /// </summary>
        public float deathTime { get; private set; } = 0f;

        public virtual float tornadoMaxTime { get; set; } = 60f;
        private float timeRunning = 0;

        private float rotationSpeed = 2f;
        public virtual float maxRotationSpeed { get; set; } = 2f;
        public virtual float rotationRadius { get; set; } = 1f;

        public virtual float windSpeed { get; set; } = 0.15f;

        public Vector3 directionalVelocity { get; protected set; }
        public Vector3 intendedDirectionalVelocity;
        public virtual float directionalVelocityStrength { get; set; } = 2f;

        public virtual MinMax directionalVelocityRange { get; set; } = new MinMax(-1, 1);

        public virtual float drag { get; set; } = 0.87f;

        private float rotation;

        private ParticleSystem tornadoParticles = null;


        public void Update()
        {
            if (active)
            {
                Vector3 movement = CalcMovement();
                transform.position += movement;
                CalcVelocity();
                CalcStrength();
                CheckDeath();
            }
            else
            {

            }

            timeRunning += Time.deltaTime;
            OnUpdate();
        }

        public virtual void OnUpdate()
        {

        }

        public void Start()
        {
            spawnPos = transform.position;
            DebugExt.Log("Tornado Spawn",true,KingdomLog.LogStatus.Neutral,gameObject);
            if (gameObject.transform.Find("Base") != null)
            {
                tornadoParticles = gameObject.transform.Find("Base").GetComponent<ParticleSystem>();
            }
            //TryRandomDirectionalVelocity();
            UpdateParticles();
            OnStart();
        }

        public virtual void OnStart()
        {

        }

        

        protected void CalcStrength()
        {
            float max = Util.Vector3MaxValue(directionalVelocity);
            if(max < deathSpeedThreshold)
            {
                deathTime += Time.deltaTime;
            }
            else
            {
                deathTime -= Time.deltaTime;
            }
            
            float percent = directionalVelocityRange.Max / Math.Abs(max);
            rotationSpeed = maxRotationSpeed * percent;
        }

        protected void CheckDeath() 
        {
            if (active)
            {
                if (timeRunning > tornadoMaxTime)
                {
                    StartCoroutine("KillOverTime");
                }
            }
            else
            {
                if (deathTime > deathTimeMax)
                {
                    StartCoroutine("KillOverTime");
                }
            }
        }


        public void Kill()
        {
            DebugExt.dLog("Tornado ded", true, KingdomLog.LogStatus.Neutral, transform.position);
            Events.TornadoEvent.OnTornadoDeath(gameObject);
            GameObject.Destroy(this.gameObject);
        }


        protected virtual void OnKillEnd()
        {

        }

        /// <summary>
        /// Called multiple times while the tornado is dying
        /// </summary>
        protected virtual void OnKill()
        {
            DebugExt.dLog("tornado dying");
        }

        private IEnumerator KillOverTime()
        {
            OnKill();

            tornadoParticles.Stop();
            yield return new WaitForSeconds(lingerTime);

            OnKillEnd();
            Kill();
        }


        protected void UpdateParticles()
        {
            if (!tornadoParticles.isPlaying)
            {
                DebugExt.Log("Playing particles");
                tornadoParticles.Play();
            }
        }


        #region Movement

        protected void CalcVelocity()
        {
            directionalVelocity = Vector3.Lerp(directionalVelocity, intendedDirectionalVelocity, Time.deltaTime * windSpeed);
            if (directionalVelocity == intendedDirectionalVelocity)
            {
                directionalVelocity *= drag;
            }
        }

        protected void SetDirectionalVelocity(Vector3 value)
        {
            intendedDirectionalVelocity = value;
        }

        protected void TryRandomDirectionalVelocity()
        {
            float x = directionalVelocityRange.Rand();
            float z = directionalVelocityRange.Rand();
            SetDirectionalVelocity( new Vector3(x, 0, z) * directionalVelocityStrength);
        }

        public void TryTurnAwayFrom(Vector3 pos)
        {
            Vector3 diff = pos - transform.position;
            SetDirectionalVelocity(Vector3.ClampMagnitude(diff, directionalVelocityRange.Max));
        }



        protected Vector3 CalcMovement()
        {
            Vector3 movement = Vector3.zero;
            movement += CalcRotationalMovement();
            movement += directionalVelocity;

            movement *= Time.deltaTime;

            return movement;
        }


        protected Vector3 CalcRotationalMovement()
        {
            Vector3 movement = Vector3.zero;

            float rotation = Util.DegreesToRadians(rotationSpeed % 360);
            this.rotation += rotation;

            float xVal = (float)Math.Sin(this.rotation);
            float zVal = (float)Math.Cos(this.rotation);


            movement.x += float.IsNaN(xVal) ? 0 : xVal;
            movement.z += float.IsNaN(zVal) ? 0 : zVal;
            movement *= rotationRadius;

            return movement;
        }

        #endregion

    }
}
