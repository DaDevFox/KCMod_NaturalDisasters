using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Disasters
{
    static class AssetBundleManager
    {
        public static AssetBundle assetBundle;
        public static void UnpackAssetBundle()
        {
            assetBundle = KCModHelper.LoadAssetBundle(Mod.helper.modPath + "/assetbundle/", "naturaldisastersassets");
            if (assetBundle == null) {
                Mod.helper.Log("AssetBundle failed to load");
            }
        }

        public static object GetAssetByPath(string path) 
        {
            return assetBundle.LoadAsset(path);
        }


        /// <summary>
        /// Gets an asset by its name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static object GetAsset(string name) 
        {
            if (assetBundle.Contains(name))
            {
                object Asset = null;
                string[] paths = assetBundle.GetAllAssetNames();
                for (int i = 0; i < paths.Length; i++)
                {
                    string[] pathParts = paths[i].Split('/');
                    string assetName = pathParts[pathParts.Length - 1];
                    if(assetName.ToLower() == name.ToLower())
                    {
                        Asset = assetBundle.LoadAsset(paths[i]);
                    }
                }
                return Asset;
            }
            else {
                Mod.helper.Log("Asset not found: " + name);
                return null;
            }
        }

    }
}
