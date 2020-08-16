using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combine : MonoBehaviour {

    //將mesh合併在一起，可順利減少batches和Shadow casters，用於優化
    //將此程式放進母物件中即可
    //代碼出處：https://grrava.blogspot.com/2014/08/combine-meshes-in-unity.html

    // Use this for initialization
    void Start () {
        Matrix4x4 myTransform = transform.worldToLocalMatrix;
        //用於存放要合併的mesh對象
        Dictionary<string, List<CombineInstance>> combines = new Dictionary<string, List<CombineInstance>>();
        //用於存放要合併的material
        Dictionary<string, Material> namedMaterials = new Dictionary<string, Material>();
        //得到自身以及所有子物件中的MeshRenderer
        MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
        foreach (var meshRenderer in meshRenderers)
        {
            foreach (var material in meshRenderer.sharedMaterials)
                //如果material非空且還未放進combines裡，就將之加進combines和namedMaterials裡
                if (material != null && !combines.ContainsKey(material.name))
                {
                    combines.Add(material.name, new List<CombineInstance>());
                    namedMaterials.Add(material.name, material);
                }
        }

        //得到自身以及所有子物件中的MeshFilter
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        foreach (var filter in meshFilters)
        {
            if (filter.sharedMesh == null)
                continue;
            var filterRenderer = filter.GetComponent<Renderer>();
            if (filterRenderer.sharedMaterial == null)
                continue;
            if (filterRenderer.sharedMaterials.Length > 1)
                continue;
            CombineInstance ci = new CombineInstance
            {
                mesh = filter.sharedMesh,
                transform = myTransform * filter.transform.localToWorldMatrix
            };
            //將mesh按照不同的sharedMaterial.name進行結合，會將不同的Material的物件放在不同的combines裡
            combines[filterRenderer.sharedMaterial.name].Add(ci);

            //將已經合併好的物件的mesh Renderer銷毀
            Destroy(filterRenderer);
        }

        //針對組合的material(材質)數量來進行新增合併用物件
        /*假設要合併的物件中有3種不同的material設定，就會跑三次迴圈，產生三個Combined mesh分別放三種material的物件*/
        foreach (Material m in namedMaterials.Values)
        {
            //建立一個名為Combined mesh的物件(用來存放合併的mesh Renderer)
            var go = new GameObject("Combined mesh");
            //設定物件的座標
            go.transform.parent = transform;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            //將物件加上MeshFilter
            var filter = go.AddComponent<MeshFilter>();
            //mesh.CombineMeshes(要合併的mesh數組、是否將所有mesh合併為單一mesh、CombineInstance的變換矩陣是否被使用)
            filter.mesh.CombineMeshes(combines[m.name].ToArray(), true, true);

            //將物件加上MeshRenderer
            var arenderer = go.AddComponent<MeshRenderer>();
            //設定material
            arenderer.material = m;
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
