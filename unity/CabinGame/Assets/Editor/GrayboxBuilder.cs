using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using CabinHome;

public static class GrayboxBuilder
{
    const string ScenePath="Assets/Scenes/Meadow.unity";
    const string Generated="Assets/Generated/";
    static Material grass, wood, trim, roof, stone, leaves, path, lake, snow;
    static Transform world;

    [MenuItem("CabinHome/Create initial graybox (once)")]
    public static void Create()
    {
        if(File.Exists(ScenePath)) throw new InvalidOperationException("Meadow already exists. Edit the saved scene; generation will not overwrite it.");
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        EditorSettings.serializationMode=SerializationMode.ForceText;
        PlayerSettings.companyName="CabinHome";
        PlayerSettings.productName="CabinHome";
        PlayerSettings.bundleVersion="0.1.0";
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Standalone,"com.dgkall.cabinhome");
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS,"com.dgkall.cabinhome");
        PlayerSettings.defaultScreenWidth=1280; PlayerSettings.defaultScreenHeight=800;
        PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
        PlayerSettings.resizableWindow=true;
        PlayerSettings.runInBackground=true;
        PlayerSettings.colorSpace=ColorSpace.Linear;
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone,ScriptingImplementation.Mono2x);
        PlayerSettings.SetArchitecture(NamedBuildTarget.Standalone,1);
        var rp=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Settings/PC_RPAsset.asset");
        if(!rp) throw new Exception("Official URP template pipeline asset missing");
        rp.msaaSampleCount=4; rp.shadowDistance=100; rp.renderScale=1;
        GraphicsSettings.defaultRenderPipeline=rp;
        QualitySettings.renderPipeline=rp;
        QualitySettings.vSyncCount=1;
        EditorUtility.SetDirty(rp);

        grass=Mat("Meadow",new Color(.37f,.47f,.30f));
        wood=Mat("Warm timber",new Color(.65f,.53f,.39f));
        trim=Mat("Dark timber",new Color(.25f,.29f,.25f));
        roof=Mat("Slate roof",new Color(.25f,.32f,.40f));
        stone=Mat("Foundation stone",new Color(.44f,.46f,.40f));
        leaves=Mat("Pine canopy",new Color(.22f,.34f,.28f));
        path=Mat("Pale gravel",new Color(.61f,.59f,.43f));
        lake=Mat("Distant water",new Color(.34f,.49f,.56f));
        snow=Mat("Snow peaks",new Color(.73f,.78f,.77f));
        world=new GameObject("Meadow / 160 x 120 m").transform;
        Terrain(); House(); Background(); Dressing();
        var plans=new GameObject("Planning overlays (B to toggle)");
        Outline(plans.transform,"Upgrade plot / 64 x 48 m",48,38,112,86,new Color(.80f,.78f,.48f));
        Outline(plans.transform,"Phase 2 / 120 m2",72,58,82,70,new Color(.54f,.72f,.77f));
        Outline(plans.transform,"Phase 3 / 192 m2",72,58,88,70,new Color(.78f,.58f,.48f));
        Outline(plans.transform,"Workshop / 48 m2",54,63,62,69,new Color(.70f,.67f,.54f));
        Outline(plans.transform,"Greenhouse / 32 m2",96,63,104,67,new Color(.57f,.75f,.65f));
        Outline(plans.transform,"Garden / 120 m2",54,44,66,54,new Color(.57f,.75f,.65f));
        Outline(plans.transform,"Reserve / 168 m2",92,42,106,54,new Color(.70f,.67f,.54f));
        Outline(plans.transform,"Future deck / 48 m2",72,55,88,58,new Color(.78f,.58f,.48f));
        plans.SetActive(false);

        var light=new GameObject("Late afternoon sun").AddComponent<Light>();
        light.type=LightType.Directional; light.intensity=1.65f;
        light.color=new Color(1,.88f,.72f); light.shadows=LightShadows.Soft;
        light.transform.rotation=Quaternion.Euler(28,-40,0); RenderSettings.sun=light;
        RenderSettings.ambientMode=AmbientMode.Trilight;
        RenderSettings.ambientSkyColor=new Color(.52f,.60f,.68f);
        RenderSettings.ambientEquatorColor=new Color(.47f,.48f,.44f);
        RenderSettings.ambientGroundColor=new Color(.25f,.29f,.23f);
        RenderSettings.fog=true; RenderSettings.fogMode=FogMode.Exponential;
        RenderSettings.fogColor=new Color(.66f,.73f,.75f); RenderSettings.fogDensity=.0028f;
        RenderSettings.skybox=null;
        var camera=new GameObject("Main Camera").AddComponent<Camera>(); camera.tag="MainCamera";
        camera.clearFlags=CameraClearFlags.SolidColor; camera.backgroundColor=new Color(.66f,.73f,.75f);
        camera.fieldOfView=55; camera.nearClipPlane=.08f; camera.farClipPlane=1000;
        camera.allowHDR=true; camera.gameObject.AddComponent<AudioListener>();
        camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
        var walker=new GameObject("Walk controller / 1.8 m");
        var cc=walker.AddComponent<CharacterController>();
        cc.height=1.8f; cc.center=Vector3.up*.9f; cc.radius=.25f;
        cc.stepOffset=.24f; cc.slopeLimit=45; cc.skinWidth=.025f;
        walker.transform.position=SitePlan.Ground(76,49,.1f);
        var motor=walker.AddComponent<WalkMotor>();
        var demo=camera.gameObject.AddComponent<DemoController>(); demo.walker=motor; demo.planning=plans; demo.Home();
        camera.gameObject.AddComponent<DemoSmokeCheck>();
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),ScenePath);
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
        EditorBuildSettings.RemoveConfigObject("com.unity.input.settings.actions");
        AssetDatabase.SaveAssets();
        Validate();
        Debug.Log("CABIN_GRAYBOX_CREATED "+ScenePath);
    }

    static Material Mat(string name,Color color)
    {
        var m=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=name};
        m.SetColor("_BaseColor",color); m.SetFloat("_Smoothness",.08f); m.SetFloat("_Metallic",0);
        AssetDatabase.CreateAsset(m,Generated+"Materials/"+name+".mat"); return m;
    }
    static GameObject Box(string name,Vector3 pos,Vector3 size,Material mat,Transform parent=null,bool collision=true)
    {
        var go=GameObject.CreatePrimitive(PrimitiveType.Cube); go.name=name;
        go.transform.SetParent(parent?parent:world); go.transform.position=pos; go.transform.localScale=size;
        go.GetComponent<Renderer>().sharedMaterial=mat;
        if(!collision) UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
        return go;
    }
    static Mesh MeshAsset(string name,List<Vector3> v,List<int> triangles)
    {
        var m=new Mesh{name=name};
        if(v.Count>65535)m.indexFormat=IndexFormat.UInt32;
        m.SetVertices(v);m.SetTriangles(triangles,0);m.RecalculateNormals();m.RecalculateBounds();
        AssetDatabase.CreateAsset(m,Generated+"Meshes/"+name+".asset");return m;
    }
    static GameObject MeshObject(string name,Mesh mesh,Material material,bool collider,Transform parent=null)
    {
        var go=new GameObject(name);go.transform.SetParent(parent?parent:world);
        go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=material;
        if(collider)go.AddComponent<MeshCollider>().sharedMesh=mesh;
        return go;
    }
    static void Terrain()
    {
        var v=new List<Vector3>();var tris=new List<int>();
        const int nx=81,nz=61;
        for(int z=0;z<nz;z++)for(int x=0;x<nx;x++)v.Add(SitePlan.Ground(x*2,z*2));
        for(int z=0;z<nz-1;z++)for(int x=0;x<nx-1;x++)
        {int i=z*nx+x;tris.AddRange(new[]{i,i+nx,i+1,i+1,i+nx,i+nx+1});}
        MeshObject("Playable terrain",MeshAsset("Terrain_2m",v,tris),grass,true);
        // Small segments follow the slope; they do not flatten the terrace.
        for(int z=0;z<55;z++) Box("Path",SitePlan.Ground(76,z+.5f,.018f),new Vector3(2,.035f,1.02f),path,null,false);
    }
    static void House()
    {
        var house=new GameObject("Cabin / Phase 1 / 6 x 8 m").transform;house.SetParent(world);
        Box("Foundation",new Vector3(75,1.52f,62),new Vector3(6,.56f,8),stone,house);
        Box("Floor / top = 2 m",new Vector3(75,1.9f,62),new Vector3(6,.2f,8),wood,house);
        Box("North wall",new Vector3(75,3.6f,65.9f),new Vector3(6,3.2f,.2f),wood,house);
        Box("West wall",new Vector3(72.1f,3.6f,62),new Vector3(.2f,3.2f,7.6f),wood,house);
        // South opening: a real window at x=73.8; open doorway at x=76.
        WallSpan(house,72,72.9f,2,5.2f);
        WallSpan(house,72.9f,74.7f,2,2.9f);
        WallSpan(house,72.9f,74.7f,4.4f,5.2f);
        WallSpan(house,74.7f,75.3f,2,5.2f);
        WallSpan(house,75.3f,76.7f,4.4f,5.2f);
        WallSpan(house,76.7f,78,2,5.2f);
        // East opening, with low sill and lintel.
        Box("East wall south",new Vector3(77.9f,3.6f,59.2f),new Vector3(.2f,3.2f,2.4f),wood,house);
        Box("East wall north",new Vector3(77.9f,3.6f,64.6f),new Vector3(.2f,3.2f,2.8f),wood,house);
        Box("East sill wall",new Vector3(77.9f,2.45f,61.8f),new Vector3(.2f,.9f,2.8f),wood,house);
        Box("East lintel",new Vector3(77.9f,4.8f,61.8f),new Vector3(.2f,.8f,2.8f),wood,house);
        foreach(float x in new[]{72.9f,74.7f}) Box("Window jamb",new Vector3(x,3.65f,57.97f),new Vector3(.12f,1.65f,.28f),trim,house);
        foreach(float y in new[]{2.9f,4.4f})Box("Window rail",new Vector3(73.8f,y,57.97f),new Vector3(1.95f,.12f,.28f),trim,house);
        foreach(float x in new[]{75.3f,76.7f})Box("Door jamb",new Vector3(x,3.2f,57.97f),new Vector3(.12f,2.4f,.28f),trim,house);
        Box("Door lintel",new Vector3(76,4.4f,57.97f),new Vector3(1.55f,.12f,.28f),trim,house);
        foreach(float x in new[]{72.1f,77.9f})foreach(float z in new[]{58.1f,65.9f})
            Box("Corner post",new Vector3(x,3.6f,z),new Vector3(.25f,3.2f,.25f),trim,house);
        float angle=Mathf.Atan2(1.8f,4.4f)*Mathf.Rad2Deg;
        float length=Mathf.Sqrt(4.4f*4.4f+1.8f*1.8f);
        var south=Box("South roof",new Vector3(75,6.1f,59.8f),new Vector3(6.8f,.18f,length),roof,house);
        south.transform.rotation=Quaternion.Euler(-angle,0,0);
        var north=Box("North roof",new Vector3(75,6.1f,64.2f),new Vector3(6.8f,.18f,length),roof,house);
        north.transform.rotation=Quaternion.Euler(angle,0,0);
        var gable=MeshAsset("Gables",new List<Vector3>{new(72,5.2f,58),new(72,5.2f,66),new(72,7,62),new(78,5.2f,58),new(78,7,62),new(78,5.2f,66)},new List<int>{0,1,2,3,4,5});
        MeshObject("Gable ends",gable,wood,false,house);
        Box("Porch",new Vector3(76,1.9f,57),new Vector3(2.4f,.2f,2),wood,house);
        for(int i=0;i<4;i++)
        {
            float top=1.52f+i*.16f;
            Box("Entry step "+(i+1),new Vector3(76,(top+1.2f)/2,54.8f+i*.4f),new Vector3(2,(top-1.2f),.42f),stone,house);
        }
        // A single block makes the interior readable without introducing a furniture system.
        Box("Interior bench placeholder",new Vector3(73,2.23f,64.8f),new Vector3(1.4f,.46f,.55f),trim,house);
    }
    static void WallSpan(Transform house,float x0,float x1,float y0,float y1)
        =>Box("South wall",new Vector3((x0+x1)/2,(y0+y1)/2,58.1f),new Vector3(x1-x0,y1-y0,.2f),wood,house);

    static Mesh ConeMesh()
    {
        var v=new List<Vector3>();var t=new List<int>();
        for(int i=0;i<7;i++)
        {
            float a=i*Mathf.PI*2/7,b=(i+1)*Mathf.PI*2/7;
            int s=v.Count;v.Add(new Vector3(Mathf.Cos(a),0,Mathf.Sin(a)));v.Add(Vector3.up);v.Add(new Vector3(Mathf.Cos(b),0,Mathf.Sin(b)));
            t.AddRange(new[]{s,s+1,s+2});
        }
        return MeshAsset("Low poly cone",v,t);
    }
    static Mesh cone;
    static void Background()
    {
        cone=ConeMesh();
        var background=new GameObject("Outside playable area / background only").transform;
        Box("Lake backdrop",new Vector3(100,-9,-180),new Vector3(500,.2f,260),lake,background,false);
        for(int i=0;i<11;i++)
        {
            var m=MeshObject("Distant peak "+i,cone,i%3==0?snow:roof,false,background);
            m.transform.position=new Vector3(-130+i*47,-15,-290-(i%3)*25);
            m.transform.localScale=new Vector3(55,55+(i*17%60),45);
        }
    }
    static void Dressing()
    {
        var group=new GameObject("Placeholder vegetation and rocks").transform;group.SetParent(world);
        var random=new System.Random(71);
        for(int i=0;i<28;i++)
        {
            float x=i%2==0?28+(float)random.NextDouble()*16:122+(float)random.NextDouble()*18;
            float z=22+(float)random.NextDouble()*65;float h=4+(float)random.NextDouble()*3;
            Vector3 p=SitePlan.Ground(x,z);
            Box("Tree trunk",p+Vector3.up*h*.35f,new Vector3(.3f,h*.7f,.3f),trim,group);
            for(int tier=0;tier<3;tier++)
            {
                var crown=MeshObject("Pine crown",cone,leaves,false,group);
                crown.transform.position=p+Vector3.up*(h*.2f+tier*h*.2f);
                float radius=h*(.27f-tier*.045f);crown.transform.localScale=new Vector3(radius,h*.52f,radius);
            }
        }
        for(int i=0;i<18;i++)
        {
            float x=i%2==0?68-(float)random.NextDouble()*4:82+(float)random.NextDouble()*4;
            float z=12+(float)random.NextDouble()*30;
            var go=GameObject.CreatePrimitive(PrimitiveType.Sphere);go.name="Rock placeholder";
            go.transform.SetParent(group);go.transform.position=SitePlan.Ground(x,z,.15f);
            go.transform.localScale=new Vector3(.6f,.45f,.5f);go.GetComponent<Renderer>().sharedMaterial=stone;
        }
    }
    static void Outline(Transform parent,string label,float x0,float z0,float x1,float z1,Color color)
    {
        var go=new GameObject(label);go.transform.SetParent(parent);
        var line=go.AddComponent<LineRenderer>();line.useWorldSpace=true;line.loop=true;line.widthMultiplier=.20f;
        line.sharedMaterial=Mat(label.Split('/')[0].Trim()+" marker",color);
        line.sharedMaterial.shader=Shader.Find("Universal Render Pipeline/Unlit");
        line.positionCount=4;
        line.SetPositions(new[]{SitePlan.Ground(x0,z0,.08f),SitePlan.Ground(x0,z1,.08f),SitePlan.Ground(x1,z1,.08f),SitePlan.Ground(x1,z0,.08f)});
    }

    [MenuItem("CabinHome/Validate graybox")]
    public static void Validate()
    {
        if(EditorSceneManager.GetActiveScene().path!=ScenePath)EditorSceneManager.OpenScene(ScenePath);
        Physics.SyncTransforms();
        Require(GraphicsSettings.defaultRenderPipeline is UniversalRenderPipelineAsset,"URP configured");
        var terrain=GameObject.Find("Playable terrain").GetComponent<MeshFilter>().sharedMesh;
        Require(Mathf.Approximately(terrain.bounds.size.x,160)&&Mathf.Approximately(terrain.bounds.size.z,120),"160 x 120 terrain");
        var floor=GameObject.Find("Floor / top = 2 m").GetComponent<Renderer>().bounds;
        Require(Mathf.Abs(floor.size.x-6)<.001f&&Mathf.Abs(floor.size.z-8)<.001f&&Mathf.Abs(floor.max.y-2)<.001f,"6 x 8 floor, elevation 2");
        Require(Mathf.Abs(SitePlan.Height(80,86)-SitePlan.Height(80,38)-.6f)<.001f,"terrace height difference 0.6");
        Require(Physics.Raycast(new Vector3(76,8,61),Vector3.down,out var hit,7)&&hit.collider!=null,"cabin collision present");
        Require(!Physics.Raycast(new Vector3(76,3.3f,57),Vector3.forward,2),"doorway clear");
        foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            foreach(var m in r.sharedMaterials)Require(m!=null&&m.shader!=null,"material on "+r.name);
        Require(UnityEngine.Object.FindFirstObjectByType<DemoController>()!=null,"demo controls present");
        Debug.Log("CABIN_GRAYBOX_VALIDATION_PASSED");
    }
    static void Require(bool result,string message){if(!result)throw new Exception("Validation failed: "+message);}

    [MenuItem("CabinHome/Build Mac demo")]
    public static void BuildMac()
    {
        Validate();
        string output=Path.GetFullPath(Path.Combine(Application.dataPath,"../../../Builds/CabinHome.app"));
        Directory.CreateDirectory(Path.GetDirectoryName(output));
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{ScenePath},locationPathName=output,target=BuildTarget.StandaloneOSX,options=BuildOptions.Development});
        if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Build failed: "+report.summary.result);
        Debug.Log("CABIN_MAC_BUILD_PASSED "+output+" bytes="+report.summary.totalSize);
    }
}
