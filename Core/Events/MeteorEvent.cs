using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disasters.Events
{
    public class MeteorEvent : ModEvent
    {
        public override int testFrequency => 1;


        private float intensity = 1f;
        private float terraformChance = 0.6f;
        private float fireChance = 10f;
        private float treeKillChance = 1.5f;
        private float destroyBuildingChance = 0.4f;
        private float rockChance = 0.05f;

        private float stoneChance = 0.07f;
        private float ironChance = 0.05f;


        public bool terraform = true;
        public bool fires = true;
        public bool killTrees = true;
        public bool destroyBuildings = true;
        public bool rocks = true;


        public override bool Test()
        {
            return SRand.Range(0f, 1f) < Settings.meteorChance;
        }

        public override void Run()
        {
            base.Run();

            if (!Settings.meteors)
                return;


            int landmass = (int)SRand.Range(0, World.inst.NumLandMasses);
            Cell origin = World.inst.cellsToLandmass[landmass].RandomElement();

            int radius = (int)Settings.meteorSize.Rand();

            int startX = origin.x - radius;
            int startZ = origin.z - radius;
            int endX = origin.x + radius;
            int endZ = origin.z + radius;


            if (startX < 0 || endX > World.inst.GridWidth ||
                startZ < 0 || endZ > World.inst.GridHeight)
            {
                DebugExt.dLog($"Meteor strike too big for cell ({origin.x},{origin.z})");
                return;
            }


            KingdomLog.TryLog("meteorStrike", $"My lord, a rock from the heavens has made impact on our land!", KingdomLog.LogStatus.Warning, 1, origin.Center);

            // Core impact
            World.inst.ForEachTileInRadius(origin.x, origin.z, radius, (int x, int z, Cell cell) => 
            {
                if (cell != null)
                {
                    float weightage = 1f - (Util.DistanceSqrdXZ(x, z, origin.x, origin.z) / (float)(radius * radius));

                    bool c_terraform = Util.Randi() < terraformChance * weightage * intensity;
                    bool c_fire = Util.Randi() < fireChance * weightage * intensity;
                    bool c_destroyBuildings = Util.Randi() < destroyBuildingChance * weightage * intensity;
                    bool c_treeKill = Util.Randi() < treeKillChance * weightage * intensity;
                    bool c_rock = Util.Randi() < rockChance * weightage * intensity;

                    DebugExt.dLog($"{Util.Randi()}: w:{weightage} [d:{Util.DistanceSqrdXZ(x, z, origin.x, origin.z)}, r:{radius}]", true, KingdomLog.LogStatus.Neutral, cell.Center);

                    if (c_destroyBuildings && destroyBuildings)
                        Util.AnnhiliateCell(cell, false, true);

                    if (c_terraform && terraform)
                        Util.MessCell(cell, landmass);

                    if (c_fire && fires)
                        Assets.Code.FireManager.inst.StartFireAt(cell);

                    if (c_treeKill)
                        TreeSystem.inst.FellAllTrees(cell);

                    if (c_rock)
                    {
                        ResourceType type = ResourceType.UnusableStone;
                        if (Util.Randi() < weightage * stoneChance)
                            type = ResourceType.Stone;
                        else if (Util.Randi() < weightage * ironChance)
                            type = ResourceType.IronDeposit;
                        
                        Util.SetCellType(cell, type);
                    }
                }
            });
        }


    }
}
