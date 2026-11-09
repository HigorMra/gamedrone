using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class DronePrototypeSceneBuilder
{
    const string ScenePath = "Assets/Scenes/DronePrototype.unity";
    static DronePrototypeSceneBuilder()
    {
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || System.IO.File.Exists(ScenePath)) return;
            BuildScene();
        };
    }

    [MenuItem("Tools/Drone Prototype/Rebuild Demo Scene")]
    public static void BuildScene()
    {
        System.IO.Directory.CreateDirectory("Assets/Scenes");
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        if (AssetDatabase.IsValidFolder("Assets/Generated")) AssetDatabase.DeleteAsset("Assets/Generated");
        AssetDatabase.CreateFolder("Assets", "Generated");
        Material ground = SaveAsset(new Material(Shader.Find("Drone/Vertex Color Lit")) { name="Ground · stained factory yard" });
        Material concrete = Mat("Concrete · weathered", new Color(.37f,.38f,.35f));
        Material concreteDark = Mat("Concrete · damp", new Color(.22f,.25f,.24f));
        Material concreteLight = Mat("Concrete · sunbleached", new Color(.48f,.47f,.41f));
        Material steel = Mat("Steel · oxidized", new Color(.19f,.24f,.23f), .5f, .52f);
        Material rust = Mat("Steel · rust", new Color(.42f,.20f,.11f), .22f, .35f);
        Material glass = Mat("Broken glass edge", new Color(.27f,.42f,.42f), .7f, .72f);
        Material weeds = Mat("Weeds · overgrown", new Color(.22f,.32f,.15f));
        Material graffitiA = Mat("Graffiti · faded orange", new Color(.82f,.29f,.10f));
        Material graffitiB = Mat("Graffiti · oxidized teal", new Color(.10f,.47f,.43f));
        Material graffitiC = Mat("Graffiti · chalk", new Color(.72f,.69f,.57f));
        BuildTerrain(ground);
        BuildIndustrialComplex(concrete, concreteDark, concreteLight, steel, rust, glass, graffitiA, graffitiB, graffitiC);
        BuildSilos(concrete, concreteDark, concreteLight, steel, rust);
        BuildDistrict(concrete, concreteDark, concreteLight, steel, graffitiA, graffitiB);
        BuildWeeds(weeds);
        BuildDrone();
        BuildLighting();
        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        AssetDatabase.SaveAssets();
        Debug.Log("Drone prototype scene created at " + ScenePath);
    }

    static Material Mat(string name, Color color, float metallic=0f, float smooth=0.15f)
    {
        var m = new Material(Shader.Find("Standard")) { name=name, color=color, enableInstancing=true };
        m.SetFloat("_Metallic", metallic);
        m.SetFloat("_Glossiness", smooth);
        return SaveAsset(m);
    }

    static T SaveAsset<T>(T asset) where T : UnityEngine.Object
    {
        string safe = "";
        foreach (char c in asset.name) safe += char.IsLetterOrDigit(c) ? c : '_';
        string extension = asset is Material ? ".mat" : ".asset";
        AssetDatabase.CreateAsset(asset, "Assets/Generated/" + safe + extension);
        return asset;
    }

    static void BuildTerrain(Material material)
    {
        const int n=120; const float extent=280f;
        var vertices=new Vector3[(n+1)*(n+1)]; var colors=new Color[vertices.Length]; var triangles=new int[n*n*6];
        for(int z=0;z<=n;z++) for(int x=0;x<=n;x++)
        {
            float px=(x/(float)n-.5f)*extent, pz=(z/(float)n-.5f)*extent;
            float cracks=Mathf.PerlinNoise(px*.075f+31f,pz*.075f+12f);
            float h=(cracks-.5f)*.24f+Mathf.PerlinNoise(px*.31f,pz*.31f)*.06f;
            int index=z*(n+1)+x;vertices[index]=new Vector3(px,h,pz);
            float stain=Mathf.PerlinNoise(px*.13f+31f,pz*.13f+17f)*.26f+Mathf.PerlinNoise(px*.51f,pz*.51f)*.075f;
            Color concrete=Color.Lerp(new Color(.19f,.21f,.20f),new Color(.39f,.39f,.35f),stain);
            float edge=Mathf.Clamp01((Mathf.Max(Mathf.Abs(px),Mathf.Abs(pz))-70f)/24f);
            colors[index]=Color.Lerp(concrete,new Color(.19f,.25f,.15f),edge*Mathf.PerlinNoise(px*.23f,pz*.23f));
        }
        int t=0; for(int z=0;z<n;z++) for(int x=0;x<n;x++){int a=z*(n+1)+x,b=a+n+1;triangles[t++]=a;triangles[t++]=b;triangles[t++]=a+1;triangles[t++]=a+1;triangles[t++]=b;triangles[t++]=b+1;}
        var mesh=new Mesh{name="Procedural industrial yard",indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.vertices=vertices;mesh.colors=colors;mesh.triangles=triangles;mesh.RecalculateNormals();mesh.RecalculateBounds();SaveAsset(mesh);
        var go=new GameObject("Ground · cracked industrial yard");go.layer=8;go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=material;go.AddComponent<MeshCollider>().sharedMesh=mesh;
    }

    static float GroundY(float x,float z){return (Mathf.PerlinNoise(x*.075f+31f,z*.075f+12f)-.5f)*.24f+Mathf.PerlinNoise(x*.31f,z*.31f)*.06f;}

    static GameObject Box(string name,Vector3 position,Vector3 scale,Material material,float angle=0f)
    {
        var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.position=position;go.transform.localScale=scale;go.transform.rotation=Quaternion.Euler(0,angle,0);go.GetComponent<Renderer>().sharedMaterial=material;return go;
    }

    static void BuildIndustrialComplex(Material concrete,Material damp,Material pale,Material steel,Material rust,Material glass,Material orange,Material teal,Material chalk)
    {
        const float left=-26f,right=26f,front=1f,back=59f,roof=18f;
        // A deep, roofed production hall with open bays and missing window panes.
        for(int i=0;i<=7;i++)
        {
            float z=front+i*(back-front)/7f;
            Box("Hall · concrete portal column L",new Vector3(left,roof*.5f,z),new Vector3(1.4f,roof,1.4f),i%3==0?pale:concrete);
            Box("Hall · concrete portal column R",new Vector3(right,roof*.5f,z),new Vector3(1.4f,roof,1.4f),i%2==0?damp:concrete);
            Box("Hall · roof truss",new Vector3(0,roof-.2f,z),new Vector3(53,.65f,.8f),steel);
            Box("Hall · side lintel L",new Vector3(left,roof-1.4f,z+(back-front)/14f),new Vector3(1.05f,1.0f,(back-front)/7f),pale);
            Box("Hall · side lintel R",new Vector3(right,roof-1.4f,z+(back-front)/14f),new Vector3(1.05f,1.0f,(back-front)/7f),concrete);
        }
        for(int side=-1;side<=1;side+=2)
        {
            float x=side*25.7f;
            for(int bay=0;bay<7;bay++)
            {
                float z=front+3.0f+bay*7.75f;
                Box("Hall · broken lower window wall",new Vector3(x,2.5f,z),new Vector3(.9f,4.7f,6.2f),bay%3==0?damp:concrete);
                Box("Hall · upper facade band",new Vector3(x,15.2f,z),new Vector3(.9f,3.3f,6.2f),bay%2==0?pale:concrete);
                Box("Hall · hanging window mullion",new Vector3(x-side*.52f,8.5f,z+2.85f),new Vector3(.14f,8.3f,.16f),rust);
                if(bay%2==0) Box("Hall · fractured transom",new Vector3(x-side*.5f,12.4f,z-.9f),new Vector3(.13f,.17f,2.8f),steel,3f);
                if(bay%3==0)Box("Hall · last shard of glazing",new Vector3(x-side*.51f,9.2f,z-1.9f),new Vector3(.08f,1.7f,.38f),glass,14f);
            }
        }
        // End walls retain only the structural piers, with a large FPV entrance at the near end.
        foreach(float z in new[]{front,back})
        {
            foreach(float x in new[]{-25f,-18f,-11f,11f,18f,25f})
            {
                Box("Hall · end wall pier",new Vector3(x,8.5f,z),new Vector3(1.3f,17f,1.2f),x%2==0?concrete:pale);
                if(Mathf.Abs(x)>12f)Box("Hall · end wall lower panel",new Vector3(x,2.4f,z),new Vector3(6.8f,4.4f,1.0f),damp);
            }
            Box("Hall · end wall lintel",new Vector3(0,16.4f,z),new Vector3(51,2.7f,1.0f),concrete);
        }
        // Broken roof sheets alternate with daylight openings; the steel ribs frame the flight line.
        for(int row=0;row<6;row++)
        {
            float z=front+4.5f+row*8.5f;
            Box("Hall · roof purlin",new Vector3(0,17.7f,z),new Vector3(52,.3f,.34f),rust);
            if(row%2==0)
            {
                Box("Hall · surviving roof sheet",new Vector3(-15,17.35f,z+3.1f),new Vector3(20,.24f,5.5f),damp);
                Box("Hall · surviving roof sheet",new Vector3(15,17.35f,z+3.1f),new Vector3(17,.24f,5.5f),concrete);
            }
        }
        for(int i=0;i<5;i++)
        {
            float z=10+i*10f;
            Box("Hall · interior column",new Vector3(-11,8.2f,z),new Vector3(.85f,16.4f,.85f),i%2==0?damp:concrete);
            Box("Hall · interior column",new Vector3(11,8.2f,z+3f),new Vector3(.85f,16.4f,.85f),concrete);
            if(i==1||i==4)Box("Hall · fallen brace",new Vector3(-3,9f,z+2),new Vector3(20,.32f,.34f),rust,24f);
        }
        // Abstract spray-painted tags and rust streaks on the entrance and lower side bays.
        Graffiti(new Vector3(-16f,3.0f,.42f),orange,teal,chalk,false);
        Graffiti(new Vector3(18f,3.1f,.42f),teal,orange,chalk,false);
        Graffiti(new Vector3(25.18f,2.9f,28f),orange,teal,chalk,true);
        Graffiti(new Vector3(-25.18f,3.2f,42f),teal,orange,chalk,true);
        // A neighboring roofless machine shop provides a second, tighter flight line.
        BuildRuinedWorkshop(new Vector3(-66f,-1f,28f),concrete,damp,pale,steel,teal,orange);
        BuildRuinedWorkshop(new Vector3(66f,-1f,63f),concrete,damp,pale,steel,orange,teal);
    }

    static void Graffiti(Vector3 origin,Material a,Material b,Material c,bool onXWall)
    {
        for(int i=0;i<13;i++)
        {
            float x=(i-6)*.42f;float y=(i%4)*.24f;
            Vector3 p=onXWall?origin+new Vector3(0,y,x):origin+new Vector3(x,y,0);
            Vector3 s=onXWall?new Vector3(.07f,.08f,1.1f+Mathf.Abs(Mathf.Sin(i))*1.5f):new Vector3(1.1f+Mathf.Abs(Mathf.Sin(i))*1.5f,.08f,.07f);
            Box("Spray paint · abstract tag",p,s,i%3==0?a:i%3==1?b:c,(i%2==0?1:-1)*(i*7%24));
        }
    }

    static void BuildRuinedWorkshop(Vector3 origin,Material concrete,Material damp,Material pale,Material steel,Material teal,Material orange)
    {
        float x=origin.x,z=origin.z;
        Box("Workshop · fractured back wall",new Vector3(x,5.6f,z+13f),new Vector3(31,11f,.8f),concrete);
        Box("Workshop · side wall",new Vector3(x-15.5f,5.6f,z),new Vector3(.8f,11f,26f),damp);
        Box("Workshop · side wall remnant",new Vector3(x+15.5f,3.7f,z+6f),new Vector3(.8f,7.4f,14f),pale);
        for(int i=0;i<5;i++)
        {
            float bx=x-13+i*6.5f;
            Box("Workshop · exposed column",new Vector3(bx,5.7f,z),new Vector3(1.0f,11.4f,1.0f),i%2==0?pale:concrete);
            Box("Workshop · lintel fragment",new Vector3(bx+3.1f,10.9f,z),new Vector3(5.4f,.65f,1.0f),steel, (i%2==0?1:-1)*7f);
        }
        Box("Workshop · broken roof beam",new Vector3(x,11.2f,z+2f),new Vector3(29,.45f,.5f),steel,6f);
        Graffiti(new Vector3(x,2.8f,z+12.45f),orange,teal,pale,false);
    }

    static void BuildSilos(Material concrete,Material damp,Material pale,Material steel,Material rust)
    {
        float[] xs={-39f,-52f,39f,52f};float[] zs={43f,57f,43f,57f};
        for(int i=0;i<xs.Length;i++)
        {
            float x=xs[i],z=zs[i],diameter=i%2==0?12.5f:11.2f,height=i%2==0?25f:22f;
            Cylinder("Silo · abandoned cement tower",new Vector3(x,height*.5f,z),new Vector3(diameter,height,diameter),i%2==0?concrete:pale);
            for(int band=0;band<4;band++)
            {
                float y=3f+band*5.8f;
                Cylinder("Silo · poured concrete seam",new Vector3(x,y,z),new Vector3(diameter+.35f,.3f,diameter+.35f),band%2==0?damp:concrete);
            }
            Cylinder("Silo · corroded crown lip",new Vector3(x,height-.2f,z),new Vector3(diameter+.6f,.55f,diameter+.6f),rust);
            Cylinder("Silo · roof cap",new Vector3(x,height+.2f,z),new Vector3(diameter*.72f,.35f,diameter*.72f),damp);
            // Exterior ladder and a broken inspection platform.
            float ladderX=x+diameter*.51f;
            Box("Silo ladder · rail",new Vector3(ladderX,height*.5f,z-.65f),new Vector3(.11f,height,.11f),rust);
            Box("Silo ladder · rail",new Vector3(ladderX,height*.5f,z+.65f),new Vector3(.11f,height,.11f),rust);
            for(int rung=1;rung<height/1.35f;rung++)Box("Silo ladder · rung",new Vector3(ladderX,height*.5f-height*.5f+rung*1.35f,z),new Vector3(.16f,.09f,1.4f),steel);
            Box("Silo · service catwalk",new Vector3(x,height*.72f,z),new Vector3(diameter+2.7f,.22f,2.0f),steel);
            for(int side=-1;side<=1;side+=2)Box("Silo · broken catwalk rail",new Vector3(x,height*.72f+1.15f,z+side*1.05f),new Vector3(diameter+2.7f,.12f,.12f),rust,side*3f);
        }
    }

    static void BuildDistrict(Material concrete,Material damp,Material pale,Material steel,Material teal,Material orange)
    {
        // Low apartment and power-house silhouettes close the horizon like an industrial Berlin block.
        for(int i=0;i<15;i++)
        {
            float side=i%2==0?-1f:1f;float x=side*(76f+(i%4)*15f);float z=-4f+(i/2)*19f;
            float w=18f+(i%3)*8f,h=15f+(i%5)*7f,d=15f+(i%4)*7f;
            Box("District · abandoned factory shell",new Vector3(x,h*.5f,z),new Vector3(w,h,d),i%3==0?damp:concrete,side*(i%2)*2f);
            int floors=Mathf.Max(2,Mathf.FloorToInt(h/3.7f));int bays=Mathf.Max(2,Mathf.FloorToInt(w/4.2f));
            for(int row=0;row<floors;row++)for(int bay=0;bay<bays;bay++)
            {
                if((row*3+bay+i)%5==0)continue;
                float wx=x-w*.5f+2.4f+bay*4.2f, wy=2.3f+row*3.7f;
                Box("Factory · empty window cavity",new Vector3(wx,wy,z-d*.5f-.06f),new Vector3(1.7f,2.0f,.12f),(row+bay)%4==0?steel:damp);
                if((row+bay+i)%3==0)Box("Factory · broken sill",new Vector3(wx,wy-1.1f,z-d*.5f-.15f),new Vector3(2.4f,.16f,.3f),pale);
            }
            if(i%3==0)Graffiti(new Vector3(x,2.5f,z-d*.5f-.14f),teal,orange,pale,false);
        }
        // A cracked rail yard and two bent gantry rails lead toward the silos.
        for(int track=0;track<3;track++)
        {
            float x=-9f+track*9f;
            Box("Rail yard · corroded rail",new Vector3(x,.18f,45f),new Vector3(.18f,.22f,118f),steel,track==1?1.3f:0f);
            for(int tie=0;tie<24;tie++)Box("Rail yard · sleeper",new Vector3(x,.06f,-11f+tie*5f),new Vector3(1.35f,.12f,.32f),damp);
        }
    }

    static void BuildWeeds(Material green)
    {
        var vertices=new List<Vector3>();var triangles=new List<int>();var colors=new List<Color>();
        for(int i=0;i<360;i++)
        {
            float x=(Mathf.PerlinNoise(i*1.81f,8.1f)-.5f)*175f;float z=(Mathf.PerlinNoise(i*2.37f,43.7f)-.5f)*175f;
            if(Mathf.Abs(x)<8f&&z<60f&&z>-5f)continue;
            Vector3 basePoint=new Vector3(x,GroundY(x,z)-.02f,z);float size=.35f+Mathf.PerlinNoise(i*.51f,i*.23f)*1.5f;
            for(int blade=0;blade<3;blade++)
            {
                float angle=blade*Mathf.PI/3f+i*.37f;Vector3 side=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle))*size*.17f;
                int start=vertices.Count;vertices.Add(basePoint-side);vertices.Add(basePoint+side);vertices.Add(basePoint+Vector3.up*size+side*.35f);
                triangles.Add(start);triangles.Add(start+1);triangles.Add(start+2);
                Color color=Color.Lerp(new Color(.10f,.16f,.09f),new Color(.38f,.43f,.20f),Mathf.PerlinNoise(i*.37f,blade*.83f));colors.Add(color);colors.Add(color);colors.Add(color);
            }
        }
        var mesh=new Mesh{name="Procedural weeds in concrete seams"};mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.SetColors(colors);mesh.RecalculateNormals();SaveAsset(mesh);
        var go=new GameObject("Weeds · reclaiming the factory yard");go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=green;
    }

    static GameObject Cylinder(string name,Vector3 position,Vector3 scale,Material material)
    {
        var go=GameObject.CreatePrimitive(PrimitiveType.Cylinder);go.name=name;go.transform.position=position;go.transform.localScale=scale;go.GetComponent<Renderer>().sharedMaterial=material;return go;
    }

    static void BuildDrone()
    {
        var drone=new GameObject("FPV quad · Player");drone.layer=2;drone.transform.position=new Vector3(0,8,-20);drone.AddComponent<Rigidbody>();drone.AddComponent<CapsuleCollider>().radius=.32f;var controller=drone.AddComponent<DroneFlightController>();
        Material carbon=Mat("Carbon fiber",new Color(.025f,.035f,.04f),.65f,.75f);Material accent=Mat("Safety lime",new Color(.72f,.93f,.33f),.15f,.5f);Material prop=Mat("Propeller composite",new Color(.12f,.15f,.14f),.2f,.45f);
        Part(drone.transform,"Core",PrimitiveType.Cube,new Vector3(0,0,0),new Vector3(.55f,.22f,.72f),carbon);
        for(int i=0;i<4;i++){float sx=(i<2?-1:1),sz=(i%2==0?1:-1);var arm=Part(drone.transform,"Carbon arm",PrimitiveType.Cube,new Vector3(sx*.55f,.02f,sz*.52f),new Vector3(.95f,.1f,.13f),carbon);arm.transform.localRotation=Quaternion.Euler(0,sx*sz*36,0);Part(drone.transform,"Motor",PrimitiveType.Cylinder,new Vector3(sx*.91f,.1f,sz*.87f),new Vector3(.27f,.14f,.27f),accent);var rotor=Part(drone.transform,"Rotor",PrimitiveType.Cube,new Vector3(sx*.91f,.22f,sz*.87f),new Vector3(1.12f,.025f,.12f),prop);var spin=rotor.AddComponent<DroneRotorSpin>();spin.SetReverse(i%2==0);}
        var lens=Part(drone.transform,"FPV camera",PrimitiveType.Sphere,new Vector3(0,-.015f,.4f),new Vector3(.24f,.2f,.22f),accent);
        var camObj=new GameObject("FPV Camera");camObj.tag="MainCamera";camObj.transform.SetParent(drone.transform,false);camObj.transform.localPosition=new Vector3(0,5.1f,-11.3f);camObj.transform.localRotation=Quaternion.Euler(18,0,0);var cam=camObj.AddComponent<Camera>();cam.fieldOfView=78;cam.farClipPlane=420;cam.allowHDR=true;cam.allowMSAA=true;camObj.AddComponent<AudioListener>();var follow=camObj.AddComponent<DroneFollowCamera>();follow.SetTarget(drone.transform);
        var hudObj=new GameObject("Flight HUD");hudObj.AddComponent<FlightHud>().SetDrone(controller);
    }

    static GameObject Part(Transform parent,string name,PrimitiveType type,Vector3 pos,Vector3 scale,Material material){var go=GameObject.CreatePrimitive(type);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=pos;go.transform.localScale=scale;go.GetComponent<Renderer>().sharedMaterial=material;Object.DestroyImmediate(go.GetComponent<Collider>());return go;}

    static void BuildLighting()
    {
        QualitySettings.antiAliasing=4;QualitySettings.shadowDistance=150f;QualitySettings.shadowResolution=ShadowResolution.High;
        RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.40f,.45f,.46f);RenderSettings.ambientEquatorColor=new Color(.29f,.31f,.29f);RenderSettings.ambientGroundColor=new Color(.10f,.11f,.10f);RenderSettings.fog=true;RenderSettings.fogColor=new Color(.42f,.45f,.43f);RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=.0028f;
        var sun=new GameObject("Sun · overcast industrial daylight");sun.transform.rotation=Quaternion.Euler(48,-28,0);var light=sun.AddComponent<Light>();light.type=LightType.Directional;light.color=new Color(.83f,.86f,.84f);light.intensity=1.12f;light.shadows=LightShadows.Soft;light.shadowStrength=.62f;RenderSettings.sun=light;
        var sky=new Material(Shader.Find("Skybox/Procedural")){name="Atmosphere · overcast Berlin"};sky.SetFloat("_SunSize",.01f);sky.SetFloat("_AtmosphereThickness",1.7f);sky.SetFloat("_Exposure",.88f);sky.SetColor("_SkyTint",new Color(.61f,.68f,.68f));sky.SetColor("_GroundColor",new Color(.26f,.28f,.25f));SaveAsset(sky);RenderSettings.skybox=sky;
    }
}
