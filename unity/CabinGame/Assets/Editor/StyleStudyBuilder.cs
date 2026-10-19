using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CabinHome;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

public static class StyleStudyBuilder
{
    public const string ScenePath = "Assets/LocalArt/Style009.unity";
    const string Art = "Assets/LocalArt/";
    static string Repo => Path.GetFullPath(Path.Combine(Application.dataPath,"../../.."));
    static System.Random Rng = new System.Random(9009);
    static float Rand(float a,float b) => a+(float)Rng.NextDouble()*(b-a);
    static Color C(string hex) { ColorUtility.TryParseHtmlString(hex,out var c);return c; }
    static Material Mat(string name,string hex)
    {
        var m=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=name};
        m.color=C(hex);m.SetFloat("_Smoothness",.04f);m.SetFloat("_Metallic",0);m.enableInstancing=true;
        AssetDatabase.CreateAsset(m,Art+name+".mat");return m;
    }
    static GameObject Box(string name,Vector3 p,Vector3 size,Material m,Transform parent)
    {
        var o=GameObject.CreatePrimitive(PrimitiveType.Cube);o.name=name;o.transform.SetParent(parent);
        o.transform.position=p;o.transform.localScale=size;o.GetComponent<Renderer>().sharedMaterial=m;
        UnityEngine.Object.DestroyImmediate(o.GetComponent<Collider>());return o;
    }
    static GameObject MeshObject(string name,Mesh mesh,Material m,Transform parent)
    {
        mesh.name=name;AssetDatabase.CreateAsset(mesh,Art+name+".asset");
        var o=new GameObject(name);o.transform.SetParent(parent);o.AddComponent<MeshFilter>().sharedMesh=mesh;o.AddComponent<MeshRenderer>().sharedMaterial=m;return o;
    }
    static Mesh Mesh(List<Vector3> v,List<int> t)
    {
        var m=new Mesh();if(v.Count>65535)m.indexFormat=IndexFormat.UInt32;
        m.SetVertices(v);m.SetTriangles(t,0);m.RecalculateNormals();m.RecalculateBounds();return m;
    }
    static GameObject Import(string id,Material material)
    {
        string source=Path.Combine(Repo,"assets-source/exports",id);
        File.Copy(Path.Combine(source,id+".fbx"),Art+id+".fbx",false);
        if(id=="VEG_003") File.Copy(Path.Combine(source,id+"_BaseColor.png"),Art+id+"_BaseColor.png",false);
        AssetDatabase.Refresh();
        var importer=(ModelImporter)AssetImporter.GetAtPath(Art+id+".fbx");
        importer.materialImportMode=ModelImporterMaterialImportMode.None;
        importer.importAnimation=false;importer.globalScale=1;importer.bakeAxisConversion=true;
        importer.isReadable=false;importer.SaveAndReimport();
        if(id=="VEG_003")
        {
            var ti=(TextureImporter)AssetImporter.GetAtPath(Art+id+"_BaseColor.png");
            ti.maxTextureSize=1024;ti.sRGBTexture=true;ti.mipmapEnabled=true;ti.alphaSource=TextureImporterAlphaSource.None;ti.SaveAndReimport();
            material.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Art+id+"_BaseColor.png"));
        }
        var asset=AssetDatabase.LoadAssetAtPath<GameObject>(Art+id+".fbx");
        var instance=(GameObject)PrefabUtility.InstantiatePrefab(asset);
        foreach(var renderer in instance.GetComponentsInChildren<Renderer>())renderer.sharedMaterial=material;
        var bounds=instance.GetComponentsInChildren<Renderer>()[0].bounds;
        if(id=="PRP_001" && (Mathf.Abs(bounds.size.y-.403f)>.02f || Mathf.Abs(bounds.size.x-.593f)>.02f))throw new Exception("Rock FBX scale/orientation mismatch: "+bounds);
        if(id=="VEG_003" && Mathf.Abs(bounds.size.y-6)>.02f)throw new Exception("Tree FBX scale/orientation mismatch: "+bounds);
        if(Mathf.Abs(bounds.min.y)>.02f)throw new Exception("Bottom origin mismatch: "+id);
        // Preserve FBX axis/unit correction on a child; gameplay moves a neutral root.
        var wrapper=new GameObject(id);instance.transform.SetParent(wrapper.transform,false);
        var prefab=PrefabUtility.SaveAsPrefabAsset(wrapper,Art+id+".prefab");UnityEngine.Object.DestroyImmediate(wrapper);return prefab;
    }
    static GameObject Place(GameObject prefab,Transform parent,float x,float z,float scale,float angle,float width=1)
    {
        var o=(GameObject)PrefabUtility.InstantiatePrefab(prefab);o.transform.SetParent(parent);
        o.transform.position=SitePlan.Ground(x,z,-.035f);o.transform.rotation=Quaternion.Euler(0,angle,0);
        o.transform.localScale=new Vector3(scale*width,scale,scale*width);return o;
    }

    [MenuItem("CabinHome/Create 009 style study (once)")]
    public static void Create()
    {
        if(File.Exists(ScenePath))throw new InvalidOperationException("Style study already exists; edit the saved scene. No overwrite performed.");
        foreach(string id in new[]{"PRP_001","VEG_003"})
            if(!File.Exists(Path.Combine(Repo,"assets-source/exports",id,id+".fbx")))throw new FileNotFoundException("Prepare the local Hunyuan sample first: "+id);
        Rng=new System.Random(9009);
        // Cloning keeps the existing graybox artifact and its shared materials intact.
        EditorSceneManager.OpenScene("Assets/Scenes/Meadow.unity");
        Directory.CreateDirectory(Art);AssetDatabase.Refresh();
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),ScenePath);
        var root=new GameObject("009 / authored style study").transform;
        var wood=Mat("Warm plastered timber","#B89A79");var trim=Mat("Recessed wood","#6C6859");
        var roof=Mat("Blue slate","#667784");var stone=Mat("Blue grey stone","#808C93");
        var gravel=Mat("Warm path","#9D9B7D");var water=Mat("Lake blue","#748E9F");
        var canopy=Mat("Needle texture","#B7C6B5");
        var grass=Mat("Sage meadow","#737F53");grass.shader=Shader.Find("CabinHome/Study Meadow");grass.SetColor("_BaseColor",C("#737F53"));grass.SetColor("_PatchColor",C("#929365"));
        foreach(var renderer in UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include,FindObjectsSortMode.None))
        {
            var old=renderer.sharedMaterial;if(!old)continue;
            if(old.name=="Warm timber")renderer.sharedMaterial=wood;
            else if(old.name=="Dark timber")renderer.sharedMaterial=trim;
            else if(old.name=="Slate roof")renderer.sharedMaterial=roof;
            else if(old.name=="Foundation stone")renderer.sharedMaterial=stone;
            else if(old.name=="Meadow")renderer.sharedMaterial=grass;
            else if(old.name=="Pale gravel")renderer.sharedMaterial=gravel;
            else if(old.name=="Distant water")renderer.sharedMaterial=water;
        }
        UnityEngine.Object.DestroyImmediate(GameObject.Find("Placeholder vegetation and rocks"));
        UnityEngine.Object.DestroyImmediate(GameObject.Find("Outside playable area / background only"));
        var tree=Import("VEG_003",canopy);var rock=Import("PRP_001",stone);
        var trees=new GameObject("Tree groups / shared mesh").transform;trees.SetParent(root);
        // Keep the center and future expansion clear. Wider view includes edge groups.
        float[,] anchors={{66,62},{64,67},{68,70},{61,57},{63,74},{91,64},{94,68},{93,76},{88,80},{49,43},{46,54},{116,55},{121,69},{40,83},{129,38},{54,78}};
        for(int i=0;i<anchors.GetLength(0);i++)Place(tree,trees,anchors[i,0],anchors[i,1],Rand(.82f,1.28f),Rand(0,360),.82f);
        var rocks=new GameObject("Stone groups / cleaned PRP_001").transform;rocks.SetParent(root);
        float[,] stones={{72.6f,53},{73.2f,52.5f},{71.9f,53.4f},{79.7f,50.2f},{80.4f,50},{80,51},{68,57},{85,61},{86,60.6f},{68,46},{82,42},{83,43}};
        for(int i=0;i<stones.GetLength(0);i++)Place(rock,rocks,stones[i,0],stones[i,1],Rand(.7f,1.65f),Rand(0,360));
        NaturalPath(root,gravel);CabinDetail(root,wood,trim,roof);Plants(root);Background(root,water);
        var camera=Camera.main;camera.clearFlags=CameraClearFlags.Skybox;
        var lighting=camera.gameObject.AddComponent<StyleLighting>();lighting.sun=RenderSettings.sun;
        lighting.sky=new Material(Shader.Find("CabinHome/Study Sky")){name="009 sky"};
        AssetDatabase.CreateAsset(lighting.sky,Art+"009 sky.mat");
        camera.GetComponent<DemoController>().styleLighting=lighting;lighting.Apply(0);
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());AssetDatabase.SaveAssets();Validate();
        Debug.Log("CABIN_STYLE_CREATED "+ScenePath);
    }
    public static void NaturalPath(Transform root,Material material)
    {
        foreach(var o in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Where(t=>t.name=="Path").ToArray())UnityEngine.Object.DestroyImmediate(o.gameObject);
        var v=new List<Vector3>();var t=new List<int>();
        for(int i=0;i<=55;i++)
        {
            float wobble=Mathf.Sin(i*1.71f)*.07f;
            v.Add(SitePlan.Ground(75+wobble,i,.026f));v.Add(SitePlan.Ground(77+wobble*.5f,i,.026f));
            if(i>0){int a=i*2;t.AddRange(new[]{a-2,a,a-1,a-1,a,a+1});}
        }
        MeshObject("Continuous gravel path",Mesh(v,t),material,root);
    }
    static void CabinDetail(Transform root,Material wood,Material trim,Material roof)
    {
        var details=new GameObject("Cabin joinery / same 6 x 8 shell").transform;details.SetParent(root);
        // Shallow board seams are restricted to existing solid wall sections.
        foreach(var wall in UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r=>r.name.Contains("wall") && r.sharedMaterial==wood).ToArray())
        {
            Bounds b=wall.bounds;bool eastWest=b.size.x<b.size.z;
            for(float y=2.28f;y<5.2f;y+=.32f)
            {
                if(y<=b.min.y+.025f||y>=b.max.y-.025f)continue;
                Vector3 p=b.center; p.y=y;
                Vector3 size;
                if(eastWest){p.x=b.center.x<75?b.min.x-.006f:b.max.x+.006f;size=new Vector3(.012f,.012f,b.size.z);}
                else{p.z=b.center.z<62?b.min.z-.006f:b.max.z+.006f;size=new Vector3(b.size.x,.012f,.012f);}
                Box("Subtle board seam",p,size,trim,details);
            }
        }
        for(float x=71.85f;x<78.2f;x+=.65f)
        {
            var south=Box("Roof seam south",new Vector3(x,6.2f,59.8f),new Vector3(.025f,.025f,4.76f),roof,details);
            south.transform.rotation=Quaternion.Euler(-Mathf.Atan2(1.8f,4.4f)*Mathf.Rad2Deg,0,0);
            var north=Box("Roof seam north",new Vector3(x,6.2f,64.2f),new Vector3(.025f,.025f,4.76f),roof,details);
            north.transform.rotation=Quaternion.Euler(Mathf.Atan2(1.8f,4.4f)*Mathf.Rad2Deg,0,0);
        }
        Box("South window mullion",new Vector3(73.8f,3.65f,57.92f),new Vector3(.07f,1.5f,.13f),trim,details);
        Box("South window crossbar",new Vector3(73.8f,3.65f,57.92f),new Vector3(1.8f,.07f,.13f),trim,details);
        // Side-facing pane leaves the CAM04 south-window sight line unobstructed.
        var glow=Mat("Warm window glass","#EFC58C");glow.EnableKeyword("_EMISSION");glow.SetColor("_EmissionColor",C("#FFC77A")*.5f);
        Box("East warm pane",new Vector3(77.88f,3.65f,61.8f),new Vector3(.02f,1.5f,2.7f),glow,details);
        Box("East window mullion",new Vector3(78.02f,3.65f,61.8f),new Vector3(.15f,1.5f,.08f),trim,details);
        Box("East window crossbar",new Vector3(78.02f,3.65f,61.8f),new Vector3(.15f,.08f,2.8f),trim,details);
        var lamp=new GameObject("Warm interior light").AddComponent<Light>();lamp.transform.SetParent(details);
        lamp.type=LightType.Point;lamp.range=7;lamp.intensity=3.5f;lamp.color=C("#FFD39C");lamp.transform.position=new Vector3(75.5f,4,61);lamp.shadows=LightShadows.None;
    }
    static void Tri(List<Vector3> v,List<int> t,Vector3 a,Vector3 b,Vector3 c,bool back=false)
    {int i=v.Count;v.Add(a);v.Add(b);v.Add(c);t.AddRange(new[]{i,i+1,i+2});if(back){int j=v.Count;v.Add(c);v.Add(b);v.Add(a);t.AddRange(new[]{j,j+1,j+2});}}
    static void Plants(Transform parent)
    {
        var greens=new[]{Mat("Grass olive","#7D8757"),Mat("Grass sage","#637A58"),Mat("Grass dry tips","#A3A171")};
        var petals=new[]{Mat("Flowers ivory","#E5DFC6"),Mat("Flowers lavender","#AAA2C3")};var heart=Mat("Flower pollen","#CDBB72");
        var verts=new List<Vector3>[6];var tris=new List<int>[6];for(int i=0;i<6;i++){verts[i]=new List<Vector3>();tris[i]=new List<int>();}
        for(int cluster=0;cluster<330;cluster++)
        {
            float x=Rand(65,88),z=Rand(39,72);
            if((x>71.4f&&x<79&&z>54&&z<67)||(x>74.7f&&x<77.3f))continue;
            // More plants at margins; preserve the open center.
            if(x>78.8f&&x<84&&z>55&&Rand(0,1)<.65f)continue;
            int color=cluster%3;
            for(int blade=0;blade<9;blade++)
            {
                float px=x+Rand(-.65f,.65f),pz=z+Rand(-.65f,.65f),h=Rand(.12f,.32f);Vector3 p=SitePlan.Ground(px,pz,.01f);
                Vector3 right=Quaternion.Euler(0,Rand(0,360),0)*Vector3.right*Rand(.018f,.035f);
                Vector3 tip=p+Vector3.up*h+new Vector3(Rand(-.08f,.08f),0,Rand(-.08f,.08f));
                Tri(verts[color],tris[color],p-right,tip,p+right,true);
            }
            if(cluster%3!=0)continue;
            for(int f=0;f<4;f++)
            {
                Vector3 bottom=SitePlan.Ground(x+Rand(-.5f,.5f),z+Rand(-.5f,.5f),.01f);Vector3 top=bottom+Vector3.up*Rand(.19f,.38f);
                Tri(verts[1],tris[1],bottom-Vector3.right*.008f,top,bottom+Vector3.right*.008f,true);
                int c=cluster%2+3;float radius=Rand(.035f,.06f);
                for(int p=0;p<5;p++)
                {
                    float angle=p*Mathf.PI*2/5;Vector3 a=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));Vector3 b=new Vector3(-a.z,0,a.x);
                    Tri(verts[c],tris[c],top,top+a*radius+b*radius*.38f,top+a*radius-b*radius*.38f,true);
                }
                Tri(verts[5],tris[5],top+new Vector3(-.013f,.005f,-.01f),top+new Vector3(0,.005f,.015f),top+new Vector3(.013f,.005f,-.01f),true);
            }
        }
        var mats=new[]{greens[0],greens[1],greens[2],petals[0],petals[1],heart};
        for(int i=0;i<6;i++)MeshObject("Meadow plant batch "+i,Mesh(verts[i],tris[i]),mats[i],parent);
    }
    static void Background(Transform parent,Material water)
    {
        var root=new GameObject("Painted distance / outside playable area").transform;root.SetParent(parent);
        Box("Lake beyond southern slope",new Vector3(95,-6,-130),new Vector3(480,.1f,200),water,root);
        string[] colors={"#8394A4","#718994","#5D7C81"};
        for(int layer=0;layer<3;layer++)
        {
            var v=new List<Vector3>();var t=new List<int>();float z=-270+layer*66;
            for(int i=0;i<31;i++)
            {
                float x=-270+i*25;float height=layer==0?Rand(33,78):Rand(9,31);
                v.Add(new Vector3(x,-12,z));v.Add(new Vector3(x,height,z+Rand(-20,20)));
                if(i>0){int a=i*2;t.AddRange(new[]{a-2,a,a-1,a,a+1,a-1});}
            }
            MeshObject("Distance ridge "+layer,Mesh(v,t),Mat("Ridge tone "+layer,colors[layer]),root);
            if(layer==0)
            {
                var sv=new List<Vector3>();var st=new List<int>();
                for(int i=1;i<30;i++)
                {
                    Vector3 peak=v[i*2+1];if(peak.y<51)continue;
                    Vector3 l=Vector3.Lerp(peak,v[(i-1)*2+1],.5f),r=Vector3.Lerp(peak,v[(i+1)*2+1],.5f);
                    Tri(sv,st,peak,l+Vector3.down*9,r+Vector3.down*8,true);
                }
                MeshObject("Broken snowline",Mesh(sv,st),Mat("Distant snow","#C4CED0"),root);
            }
        }
    }
    [MenuItem("CabinHome/Validate 009 study")]
    public static void Validate()
    {
        if(EditorSceneManager.GetActiveScene().path!=ScenePath)EditorSceneManager.OpenScene(ScenePath);
        var renderers=UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
        foreach(var r in renderers)foreach(var m in r.sharedMaterials)
            if(!m||!m.shader||!m.shader.isSupported||ShaderUtil.ShaderHasError(m.shader))throw new Exception("Invalid style material: "+r.name);
        var filters=UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None);
        long triangleInstances=filters.Sum(f=>(long)f.sharedMesh.triangles.Length/3);
        var unique=filters.Select(f=>f.sharedMesh).Distinct().ToArray();
        foreach(var f in filters.Where(f=>f.sharedMesh.name.StartsWith("VEG_003")))
        {
            var bounds=f.GetComponent<Renderer>().bounds;
            if(bounds.size.y<4||bounds.size.y>8.1f)throw new Exception("Placed tree scale/axis mismatch: "+bounds);
        }
        var floor=GameObject.Find("Floor / top = 2 m").GetComponent<Renderer>().bounds;
        if(Mathf.Abs(floor.size.x-6)>.001f||Mathf.Abs(floor.size.z-8)>.001f||Mathf.Abs(floor.max.y-2)>.001f)throw new Exception("Cabin dimensions changed");
        Physics.SyncTransforms();if(Physics.Raycast(new Vector3(76,3.3f,57),Vector3.forward,2))throw new Exception("Entrance obstructed");
        string report=$"009 style editor validation: PASS\nMesh renderers: {renderers.Length}\nUnique meshes: {unique.Length}\nTriangle instances (entire loaded scene, not visible-frame cost): {triangleInstances}\nUnique materials: {renderers.SelectMany(r=>r.sharedMaterials).Distinct().Count()}\nRock instances: {filters.Count(f=>f.sharedMesh.name.StartsWith("PRP_001"))}\nTree instances: {filters.Count(f=>f.sharedMesh.name.StartsWith("VEG_003"))}\nCabin 6 x 8m, floor top 2m, clear door: PASS\n";
        Directory.CreateDirectory(Path.Combine(Repo,"Builds"));File.WriteAllText(Path.Combine(Repo,"Builds/style-validation.txt"),report);
        Debug.Log("CABIN_STYLE_VALIDATION_PASSED\n"+report);
    }
    [MenuItem("CabinHome/Build Mac 009 study")]
    public static void BuildMac()
    {
        Validate();string path=Path.Combine(Repo,"Builds/CabinHome-Style009.app");
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{ScenePath},locationPathName=path,target=BuildTarget.StandaloneOSX,options=BuildOptions.Development});
        if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Style build failed: "+report.summary.result);
        Debug.Log("CABIN_STYLE_BUILD_PASSED bytes="+report.summary.totalSize);
    }
}
