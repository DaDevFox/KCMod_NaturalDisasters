using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Disasters
{
    static class Util
    {

        #region Terraformation

        /// <summary>
        /// Toggles the cell type (land to water and water to land) and messes up the cell (adds visual spikes/dips)
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="landmass"></param>
        public static void MessCell(Cell cell, int landmass)
        {
            if (cell.Type != ResourceType.Water)
            {
                float height = Settings.waterElevation.Rand();
                Util.AnnhiliateCell(cell);
                Util.SetWaterTile(cell, height);
                Util.SetCellLandmass(cell, landmass);
            }
            else
            {
                float height = Settings.landElevation.Rand();
                Util.AnnhiliateCell(cell);
                Util.SetLandTile(cell, 0, height);
                Util.SetCellLandmass(cell, landmass);
            }
        }

        public static void AnnhiliateCell(Cell cell, bool vanishBuildings = true, bool wreckBuildings = false)
        {
            if (cell.OccupyingStructure.Count > 0)
            {
                if (vanishBuildings)
                {
                    VanishColumn(cell);
                    VanishSubStructure(cell);
                }
                else if(wreckBuildings)
                {
                    WreckColumn(cell);
                    WreckSubStructure(cell);
                }
            }

            
            if (cell.Type != ResourceType.Water) {
                
                if (cell.Type == ResourceType.WitchHut)
                {
                    GameObject.Destroy(World.inst.GetWitchHutAt(cell));
                }
                if (cell.Type == ResourceType.Stone || cell.Type == ResourceType.IronDeposit || cell.Type == ResourceType.UnusableStone)
                {
                    World.inst.RemoveStone(cell);
                }
                GameObject cave = World.inst.GetCaveAt(cell);
                if (cave)
                {
                    GameObject.Destroy(cave);
                }

                TreeSystem.inst.DeleteTreesAt(cell);
                cell.Type = ResourceType.None;
            }
        }

        public static void SetCellType(Cell cell, ResourceType type, bool vanishBuildings = true, bool wreckBuildings = true) 
        {
            switch (type)
            {
                case ResourceType.None:
                    Util.AnnhiliateCell(cell, vanishBuildings, wreckBuildings);
                    Util.SetLandTile(cell);
                    break;
                case ResourceType.Wood:
                    Util.AnnhiliateCell(cell, vanishBuildings, wreckBuildings);
                    Util.SetLandTile(cell);
                    TreeSystem.inst.GrowTree(cell);
                    break;
                case ResourceType.Stone:
                    Util.AnnhiliateCell(cell, vanishBuildings, wreckBuildings);
                    Util.SetLandTile(cell);
                    World.inst.PlaceStone(cell, ResourceType.Stone);
                    break;
                case ResourceType.Water:
                    Util.AnnhiliateCell(cell, vanishBuildings, wreckBuildings);
                    Util.SetWaterTile(cell);
                    break;
                case ResourceType.UnusableStone:
                    Util.AnnhiliateCell(cell, vanishBuildings, wreckBuildings);
                    Util.SetLandTile(cell);
                    World.inst.PlaceStone(cell, ResourceType.UnusableStone);
                    break;
                case ResourceType.IronDeposit:
                    Util.AnnhiliateCell(cell, vanishBuildings, wreckBuildings);
                    Util.SetLandTile(cell);
                    World.inst.PlaceStone(cell,  ResourceType.IronDeposit);
                    break;
                case ResourceType.EmptyCave:
                    Util.AnnhiliateCell(cell, vanishBuildings, wreckBuildings);
                    Util.SetLandTile(cell);
                    World.inst.AddEmptyCave((int)cell.Center.x, (int)cell.Center.z);
                    break;
                case ResourceType.WolfDen:
                    Util.AnnhiliateCell(cell, vanishBuildings, wreckBuildings);
                    Util.SetLandTile(cell);
                    World.inst.AddWolfDen((int)cell.Center.x, (int)cell.Center.z);
                    break;
                case ResourceType.WitchHut:
                    Util.AnnhiliateCell(cell, vanishBuildings, wreckBuildings);
                    Util.SetLandTile(cell);
                    World.inst.AddWitchHut((int)cell.Center.x, (int)cell.Center.z);
                    break;
                default:
                    break;
            }
            cell.StorePostGenerationType();
        }

        public static void SetLandTile(Cell cell, int fertility = 0, float height = 0f) 
        {
            TerrainGen.inst.SetLandTile((int)cell.Center.x,(int)cell.Center.z);
            cell.Type = ResourceType.None;
            TerrainGen.inst.SetFertileTile((int)cell.Center.x, (int)cell.Center.z,fertility);
            TerrainGen.inst.SetTileHeight(cell, height);
        }

        public static void SetWaterTile(Cell cell, float height = -0.5f) 
        {
            bool invalid = height > TerrainGen.waterHeightTestThresold || cell.Type == ResourceType.Water;
            if (!invalid) {

                TerrainGen.inst.SetWaterTile((int)cell.Center.x, (int)cell.Center.z);
                TerrainGen.inst.SetTileHeight(cell, height);

                if (height < TerrainGen.waterHeightDeep) 
                {
                    cell.deepWater = true;
                }
                else
                {
                    cell.deepWater = false;
                }
            }
        }

        public static void SetCellLandmass(Cell cell, int landmassIdx) 
        {
            if (cell.landMassIdx > 0)
            {
                try
                {
                    World.inst.cellsToLandmass[cell.landMassIdx].RemoveAt(World.inst.cellsToLandmass[cell.landMassIdx].IndexOf(cell));
                }
                catch (Exception ex)
                {
                    Mod.helper.Log(ex.Message + "\n" + ex.StackTrace);
                }
            }

            cell.landMassIdx = landmassIdx;
            World.inst.cellsToLandmass[landmassIdx].Add(cell);
        }

        public static void WreckColumn(Cell cell)
        {
            List<Rubble.BuildingState> states;
            states = cell.BottomStructure.GetComponent<Rubble>().GetBuildingStates();
            World.inst.WreckColumn(cell, states);
        }

        public static void VanishColumn(Cell cell)
        {
            try
            {
                foreach (Building structure in cell.OccupyingStructure)
                {
                    World.inst.VanishBuilding(structure);
                }
            }
            catch (Exception ex)
            {
                Mod.helper.Log(ex.Message + "\n" + ex.StackTrace);
            }
            
        }

        public static void WreckSubStructure(Cell cell)
        {
            foreach(Building structure in cell.SubStructure)
            {
                World.inst.WreckBuilding(structure);
            }
        }

        public static void VanishSubStructure(Cell cell)
        {
            foreach (Building structure in cell.SubStructure)
            {
                World.inst.VanishBuilding(structure);
            }
        }

        #endregion

        public static int GetPlayerStartLandmass()
        {
            foreach (int landmass in Player.inst.PlayerLandmassOwner.ownedLandMasses.data)
            {
                if (Player.inst.DoesAnyBuildingHaveUniqueNameOnLandMass("keep", landmass, false))
                {
                    return landmass;
                }
            }
            return 0;
        }

        #region Randomizations

        /// <summary>
        /// Returns a random float between 0 and 1
        /// </summary>
        /// <returns></returns>
        public static float Randi()
        {
            return SRand.Range(0f, 1f);
        }

        /// <summary>
        /// Scaling < 1 means logarithmic type curve, >1 means 1/x type curve
        /// </summary>
        /// <param name="value"></param>
        /// <param name="scaling"></param>
        /// <returns></returns>
        public static float ExponentialWeight(float value, float scaling)
        {
            return Mathf.Pow(scaling, value - 1) * value;
        }

        public static float ExponentialWeightedRandom(float min, float max, float scaling)
        {
            float rand = SRand.Range(0f, 1f);
            return (Mathf.Pow(scaling, rand - 1) * rand) * (max - min) + min;
        }

        //A^(x-1) * x
        //A is constant scalar; x is input

        public static float LinearWeightedRandom(float min, float max, float increment = 1f)
        {
            if (min == max)
                return min;

            List<float> options = new List<float>();

            for (float i = max; i > min; i -= increment)
            {
                float num = max - i;

                for (float k = 0; k < num; k += increment)
                {

                    options.Add(i);
                }
            }

            return options[SRand.Range(0, options.Count - 1)];
        }


        //BROKEN
        public static float WeightedRandom(float min, float max, float weight = 0f, float increment = 1f)
        {
            List<float> options = new List<float>();

            bool valid = max > min;
            

            if (valid)
            {

                DebugExt.Log("1", true);

                float total = Util.RoundToFactor(max - min, increment);
                int _break = (int)total + 1;


                for (float i = min; i < max; i += increment)
                {


                    
                    float num = Util.RoundToFactor(-Math.Abs(weight - i) + total, increment);

                    DebugExt.Log("1-2: " + total, true);
                    DebugExt.Log("1-2: " + i, true);
                    DebugExt.Log("1-2: " + -Math.Abs(weight - i), true);

                    int counter = 0;

                    if (num != 0 && num != increment)
                    {
                        for (float k = 0; k < num; k += increment)
                        {
                            options.Add(Util.RoundToFactor(i, increment));
                            counter++;
                            if(counter > _break)
                            {
                                options.Add(Util.RoundToFactor(i, increment));
                                break;
                            }
                        }
                    }
                    else
                    {
                        options.Add(Util.RoundToFactor(i, increment));
                    }

                    DebugExt.Log("1-3: " + num, true);
                }

                DebugExt.Log(options.ToString(), true);

                return SRand.Range(0, options.Count);
            }
            else
            {
                DebugExt.Log("Invalid arguments: " + min.ToString() + ", " + max.ToString() + ", " + weight.ToString() + ", " + increment.ToString());
                return min;
            }
        }

        public static int WeightedRandom(int min, int max, int weight = 0)
        {
            List<int> options = new List<int>();

            bool valid = max > min;
            if (valid)
            {
                for (int i = min; i < max; i++)
                {

                    

                    for (int k = 0; k < Math.Abs(i - weight); k++)
                    {
                        options.Add(i);
                    }
                }

                DebugExt.Log(options.ToString(), true);

                return SRand.Range(0, options.Count);
            }
            else
            {
                return min;
            }

            
        }

        #endregion

        public static float RoundToFactor(float a, float factor)
        {
            return Mathf.Round(a/factor) * factor;
        }



        public static double DegreesToRadians(double degrees)
        {
            double radians = (Math.PI / 180) * degrees;
            return radians;
        }

        public static float DegreesToRadians(float degrees)
        {
            float radians = (float)(Math.PI / 180) * degrees;
            return radians;
        }

        public static float DistanceXZ(float ax, float az, float bx, float bz)
        {
            return (float)Math.Sqrt(DistanceSqrdXZ(ax, az, bx, bz));
        }

        public static float DistanceSqrdXZ(float ax, float az, float bx, float bz) => Math.Abs((ax - bx) * (ax - bx)) + Math.Abs((az - bz) * (az - bz));

        public static float Vector3MaxValue(Vector3 vec)
        {
            return Math.Max(Math.Max(vec.x, vec.y),vec.z);
        }
    }
}
