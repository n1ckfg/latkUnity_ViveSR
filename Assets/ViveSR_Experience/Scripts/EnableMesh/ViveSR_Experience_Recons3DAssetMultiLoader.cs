using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace Vive.Plugin.SR.Experience
{
    public class ViveSR_Experience_Recons3DAssetMultiLoader : MonoBehaviour
    {
        public bool isAllColliderReady;
        public MeshRenderer[] meshRnds;
        public MeshRenderer[] cldRnds;
        System.Action done;

        private void LoadColliderDoneCallBack(GameObject go, bool updateIsReady)
        {
            if (ViveSR_StaticColliderPool.ProcessDataAndGenColliderInfo(go) == true)
            {
                ViveSR_StaticColliderPool cldPool = go.AddComponent<ViveSR_StaticColliderPool>();
                Rigidbody rigid = go.AddComponent<Rigidbody>();
                rigid.isKinematic = true;
                rigid.useGravity = false;

                cldPool.OrganizeHierarchy();

                cldRnds = go.GetComponentsInChildren<MeshRenderer>(true);
            }
            if(updateIsReady) isAllColliderReady = true;
        }

        public GameObject[] LoadColliderObjs(string dirPath)
        {
            isAllColliderReady = false;

            List<GameObject> outputObjs = new List<GameObject>();
            DirectoryInfo dir = new DirectoryInfo(dirPath);
            if (!dir.Exists)
            {
                Debug.Log(dirPath + " does not exist.");
            }
            else
            {
                FileInfo[] files = dir.GetFiles("*_cld.obj");
                for (int i = 0; i < files.Length; i++)
                {
                    FileInfo file = files[i];
                    string filePath = dirPath + "/" + file.Name;
                    Debug.Log(filePath);

                    GameObject go = OBJLoader.LoadOBJFile(filePath, LoadColliderDoneCallBack, (i == files.Length - 1));
                    go.SetActive(false);
                    outputObjs.Add(go);
                }
            }
            return outputObjs.ToArray();
        }

        public GameObject[] LoadSemanticColliderObjs(string dirPath)
        {
            isAllColliderReady = false;

            ViveSR_SceneUnderstanding.ImportSceneObjects(dirPath);

            List<GameObject> outputObjs = new List<GameObject>();
            DirectoryInfo dir = new DirectoryInfo(dirPath);
            if (!dir.Exists)
            {
                Debug.Log(dirPath + " does not exist.");
            }
            else
            {
                string[] fileNames = ViveSR_SceneUnderstanding.GetColliderFileNames();
                for (int i = 0; i < fileNames.Length; i++)
                {
                    FileInfo file = new FileInfo(dirPath + "/" + fileNames[i]);
                    //Debug.Log(file.FullName);
                    if (!file.Exists)
                    {
                        Debug.Log(file.FullName + " does not exist.");
                        if (i == fileNames.Length - 1)
                        {
                            isAllColliderReady = true;
                            return outputObjs.ToArray();
                        }
                        else
                        {
                            continue;
                        }
                    }

                    GameObject go = OBJLoader.LoadOBJFile(file.FullName, LoadColliderDoneCallBack, (i == fileNames.Length - 1));
                    go.SetActive(false);
                    outputObjs.Add(go);
                }
                
            }
            return outputObjs.ToArray();
        }
    }
}