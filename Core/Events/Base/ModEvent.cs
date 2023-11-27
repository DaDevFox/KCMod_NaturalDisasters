using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disasters.Events
{
    public abstract class ModEvent
    {
        public virtual string saveID { get; } = "";
        public virtual Type saveObject { get; }

        public virtual int testFrequency { get; } = 1;

        /// <summary>
        /// Called once every <c>testFrequency</c> years, triggers the event naturally
        /// </summary>
        /// <returns></returns>
        public virtual bool Test() 
        {
            return false;
        }

        /// <summary>
        /// Called during SceneLoaded
        /// </summary>
        public virtual void Init() 
        {

        }

        /// <summary>
        /// Called either naturally, or forced, triggers the event
        /// </summary>
        public virtual void Run() 
        {

        }

        public virtual object OnSave()
        {
            return null;
        }

        public virtual void OnLoaded(object saveData)
        {

        }


    }
}
