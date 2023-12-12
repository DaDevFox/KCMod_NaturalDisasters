using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Harmony;

namespace Disasters.Events
{
    public class EarthquakeEvent : ModEvent
    {
        public float burnLine_terrafromChance = 0.8f;
        public float burnLine_terraformLikelinessWeightage = 0.2f;

        public float AOE_terraformLikelinessWeightage = 0.6f;
        public float AOE_fireChance = 0.9f;
        public float AOE_rockChance = 0.025f;
        public float AOE_ironChance = 0.025f;
        public float AOE_stoneChance = 0.05f;
        public float AOE_treeKillChance = 0.3f;

        public override int testFrequency => 2;

        public override void Init() {
            base.Init();
        }

        public override bool Test()
        {
            base.Test();
            return Util.Randi() < Settings.earthquakeChance;
        }

        public override void Run()
        {
            base.Run();

            if (!Settings.earthquakes)
                return;

            float radius = 1f;

            int peopleKilled = 0;
            int buildingsDestroyed = 0;

            int landmass = (int)SRand.Range(0, World.inst.NumLandMasses);

            float lineBreak = 1.5f;
            float lineResolution = 1f;

            Cell origin = World.inst.cellsToLandmass[landmass].RandomElement();
            Cell end = World.inst.cellsToLandmass[landmass].RandomElement();

            Cell[] line = GetCraggyLine(origin, end, 10);
            Cell last = null;

            int distance = (int)(origin.Center - end.Center).magnitude;
            int i = line.Length;

            float magnitude = Util.ExponentialWeightedRandom(Settings.earthquakeStrength.Min, Settings.earthquakeStrength.Max, 0.01f * distance) + Settings.earthquakeVariance.Rand();
            float m_weightage = magnitude / 10f;
            float AOE_terraformChance = 0.5f * m_weightage;

            Cam.inst.Shake(new Vector3(magnitude * 2, 0f, magnitude * 2));
            
            Vector3 midpoint = line[line.Length / 2].Center;
            KingdomLog.TryLog("earthquakeStruck", "My lord, " + GetAdjectiveForMagnitude(magnitude, distance) + " earthquake has struck the land! ", KingdomLog.LogStatus.Warning, 1, midpoint);

            foreach (Cell point in line)
            {

                radius = (line.Length - Math.Abs(i - (line.Length / 2))) * m_weightage;
                
                if (last != null)
                {
                    Vector3 difference = -(last.Center - point.Center);
                    Vector3 currentPos = last.Center;
                    float currentDiff = Math.Abs(Vector3.Distance(point.Center, last.Center));

                    while (currentDiff > lineBreak)
                    {
                        currentPos += Vector3.ClampMagnitude(difference, lineResolution);
                        Cell currentCell = World.inst.GetCellData(currentPos);

                        #region Burn Line

                        //int m_radius = (int)(magnitude * radius);
                        //if (m_radius <= 0)
                        //    m_radius = 1;

                        bool terraform = Util.Randi() <= burnLine_terrafromChance;

                        #region Terraform

                        if (terraform)
                        {
                            // weighted random in favor of terraforming tiles into similar type of the original tile
                            bool likeType = Util.Randi() < burnLine_terraformLikelinessWeightage;

                            if(currentCell.Type != ResourceType.Water)
                            {
                                float height = Settings.landElevation.Rand();

                                if (likeType)
                                {
                                    Util.AnnhiliateCell(currentCell,false,true);
                                    Util.SetLandTile(currentCell, 0, height);
                                    Util.SetCellLandmass(currentCell, landmass);
                                }
                                else
                                {
                                    height = Settings.waterElevation.Rand();
                                    Util.AnnhiliateCell(currentCell);
                                    Util.SetWaterTile(currentCell, height);
                                    Util.SetCellLandmass(currentCell, landmass);
                                }

                            }
                            else
                            {
                                float height = Settings.waterElevation.Rand();

                                if (likeType)
                                {
                                    Util.AnnhiliateCell(currentCell,false,true);
                                    Util.SetWaterTile(currentCell, height);
                                    Util.SetCellLandmass(currentCell, landmass);
                                }
                                else
                                {
                                    height = Settings.landElevation.Rand();
                                    Util.AnnhiliateCell(currentCell);
                                    Util.SetLandTile(currentCell, 0, height);
                                    Util.SetCellLandmass(currentCell, landmass);
                                }
                            }
                        }

                        #endregion

                        #endregion

                        #region AOE

                        World.inst.ForEachTileInRadius((int)currentCell.Center.x, (int)currentCell.Center.z, (int)radius, delegate (int x, int z, Cell _cell)
                        {
                             
                            float burnLineDist = Vector3.Distance(currentCell.Center, _cell.Center);

                            // bias all chance events to be less likely further from the source line
                            float weightage = 1f - (burnLineDist / radius);

                            bool c_terraform = Util.Randi() < weightage * AOE_terraformChance;
                            bool c_fire = Util.Randi() < weightage * AOE_fireChance;
                            bool c_rock = Util.Randi() < weightage * AOE_rockChance && _cell.Type != ResourceType.Water;
                            bool c_treeKill = Util.Randi() < weightage * AOE_treeKillChance;

                            
                            if (c_terraform)
                            {

                                // weighted random in favor of terraforming tiles into similar type of the original tile
                                bool likeType = Util.Randi() < AOE_terraformLikelinessWeightage * 10;

                                if (_cell.Type != ResourceType.Water)
                                {
                                    float height = Settings.landElevation.Rand();

                                    if (likeType)
                                    {
                                        Util.AnnhiliateCell(_cell,false,true);
                                        Util.SetLandTile(_cell, 0, height);
                                        Util.SetCellLandmass(_cell, landmass);
                                    }
                                    else
                                    {
                                        height = Settings.waterElevation.Rand();
                                        Util.AnnhiliateCell(_cell);
                                        Util.SetWaterTile(_cell, height);
                                        Util.SetCellLandmass(_cell, landmass);
                                    }

                                }
                                else
                                {
                                    float height = Settings.waterElevation.Rand();

                                    if (likeType)
                                    {
                                        Util.AnnhiliateCell(_cell,false,true);
                                        Util.SetWaterTile(_cell, height);
                                        Util.SetCellLandmass(_cell, landmass);
                                    }
                                    else
                                    {
                                        height = Settings.landElevation.Rand();
                                        Util.AnnhiliateCell(_cell);
                                        Util.SetLandTile(_cell, 0, height);
                                        Util.SetCellLandmass(_cell, landmass);
                                    }
                                }
                            }

                            if (c_fire)
                            {
                                Assets.Code.FireManager.inst.StartFireAt(_cell);
                            }

                            if (c_rock) 
                            {
                                ResourceType type = ResourceType.UnusableStone;
                                if(Util.Randi() < weightage * AOE_stoneChance)
                                {
                                    type = ResourceType.Stone;
                                }
                                if(Util.Randi() < weightage * AOE_ironChance) 
                                {
                                    type = ResourceType.IronDeposit;
                                }
                                Util.SetCellType(_cell, type);
                            }

                            if (c_treeKill)
                            {
                                TreeSystem.inst.DeleteTreesAt(_cell);
                            }
                        });

                        #endregion

                        TerrainGen.inst.FinalizeChanges();
                        


                        currentDiff = Math.Abs(Vector3.Distance(point.Center, currentPos));
                    }
                }
                last = point;
                i--;
            }

            }

        Cell[] GetCraggyLine(Cell a, Cell b, int segments)
        {

            MinMax variance = new MinMax(-2f, 2f);

            Cell[] result = new Cell[segments + 1];
            result[0] = a;
            Vector3 vec1 = a.Center;
            Vector3 vec2 = b.Center;

            for (int i = 1; i < segments; i++)
            {
                Vector3 vec = Vector3.Lerp(vec1, vec2, 1 / (float)segments * i);
                vec.x += SRand.Range(variance.Min, variance.Max);
                vec.z += SRand.Range(variance.Min, variance.Max); ;
                result[i] = World.inst.GetCellData((int)vec.x, (int)vec.z);
            }


            result[segments] = b;

            return result;
            
        }


        private string GetAdjectiveForMagnitude(float magnitude, int length)
        {
            string magnitudeAdjective = "";
            bool magnitudeSignificant = false;
            if (magnitude > 4.0f)
                magnitudeSignificant = true;

            if (magnitude > 9.0f)
            {
                magnitudeAdjective = "absolutely monstrous";
            }
            else if (magnitude > 7.4f)
            {
                magnitudeAdjective = "devestating";
            }
            else if (magnitude > 5.7f)
            {
                magnitudeAdjective = "tremerous";
            }
            else if (magnitude > 4.0f)
            {
                magnitudeAdjective = "frightful";
            }
            else if (magnitude > 2f)
            {
                magnitudeAdjective = "slight";
            }
            else
                magnitudeAdjective = "pathetic";


            string sizeAdjective = "";
            bool sizeSignificant = false;
            if (length >= 15)
                sizeSignificant = true;

            if(length > 50)
            {
                sizeAdjective = "a massive";
            }
            else if (length >= 30)
            {
                sizeAdjective = "a large";
            }
            else if (length >= 15)
            {
                sizeAdjective = "a moderately-sized";
            }
            else if (length >= 5)
            {
                sizeAdjective = "a small";
            }
            else
            {
                sizeAdjective = "a tiny";
            }

            string connective = SRand.CoinFlip() ? " and " : ", ";
            if (magnitudeSignificant != sizeSignificant)
                connective = " but ";

            return $"{sizeAdjective}{connective}{magnitudeAdjective}";
        }
        



        [HarmonyPatch(typeof(Building), "OnPlacement")]
        public class BuildHeightPatch
        {
            static void Postfix(Building __instance)
            {
                __instance.ForEachTileInBounds((x, z, cell) =>
                {
                    if (cell.Type != ResourceType.Water)
                    {
                        TerrainGen.inst.SetLandTile((int)cell.Center.x, (int)cell.Center.z);
                        TerrainGen.inst.AddNoise(cell);
                    }
                });
            }
        }

    }
}
