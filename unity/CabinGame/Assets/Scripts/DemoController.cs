using UnityEngine;
using UnityEngine.InputSystem;

namespace CabinHome
{
    public sealed class DemoController : MonoBehaviour
    {
        public WalkMotor walker;
        public GameObject planning;
        public bool Walking { get; private set; }
        public bool Automated { get; set; }
        public bool ShowHud = true;
        public StyleLighting styleLighting;
        public string ViewName { get; private set; } = "Meadow arrival";
        float yaw, pitch, distance = 27f;
        Vector3 focus = new(75, 3, 62);
        GUIStyle title, small, body, button;
        readonly Color ink = new(.88f,.91f,.83f);

        void Start()
        {
            Application.targetFrameRate = 60;
            Home();
        }

        public void Home()
        {
            Walking = false;
            if(styleLighting) styleLighting.Apply(0);
            focus = new Vector3(75,3,62);
            distance = 27;
            yaw = -32;
            pitch = 16;
            ApplyOrbit();
            ViewName = "Meadow arrival";
        }

        void Update()
        {
            if (Automated) return;
            var keys = Keyboard.current;
            var mouse = Mouse.current;
            if (keys == null) return;
            if (keys.tabKey.wasPressedThisFrame) { if (Walking) Home(); else Explore(); }
            if (keys.rKey.wasPressedThisFrame) Home();
            if (keys.bKey.wasPressedThisFrame) TogglePlanning();
            if (keys.hKey.wasPressedThisFrame) ShowHud = !ShowHud;
            if (keys.escapeKey.wasPressedThisFrame) { Cursor.lockState = CursorLockMode.None; Home(); }
            for (int i=0;i<4;i++) if (keys[(Key)((int)Key.Digit1+i)].wasPressedThisFrame) Review(i);
            Vector2 move = new((keys.dKey.isPressed?1:0)-(keys.aKey.isPressed?1:0),
                (keys.wKey.isPressed?1:0)-(keys.sKey.isPressed?1:0));
            bool looking = mouse != null && mouse.rightButton.isPressed;
            if (looking)
            {
                Vector2 delta = mouse.delta.ReadValue();
                yaw += delta.x * .16f;
                pitch = Mathf.Clamp(pitch - delta.y * .16f, Walking ? -75 : 5, 78);
            }
            if (Walking) Walk(move, keys.leftShiftKey.isPressed, Time.deltaTime);
            else
            {
                focus += Quaternion.Euler(0,yaw,0) * new Vector3(move.x,0,move.y) * (Time.deltaTime*12);
                focus.x = Mathf.Clamp(focus.x,0,160); focus.z = Mathf.Clamp(focus.z,0,120);
                if (mouse != null) distance = Mathf.Clamp(distance - mouse.scroll.ReadValue().y*.025f, 5, 110);
                // Preserve exact review cameras until the user adjusts the view.
                if (looking || move.sqrMagnitude>0 || (mouse != null && mouse.scroll.ReadValue().y!=0)) ApplyOrbit();
            }
        }

        void ApplyOrbit()
        {
            transform.rotation = Quaternion.Euler(pitch,yaw,0);
            transform.position = focus - transform.forward * distance;
            if (transform.position.x>=0 && transform.position.x<=160 && transform.position.z>=0 && transform.position.z<=120)
            {
                var p = transform.position;
                p.y = Mathf.Max(p.y,SitePlan.Height(p.x,p.z)+.8f);
                transform.position=p;
            }
        }

        public void Explore()
        {
            Walking=true; yaw=0; pitch=0;
            walker.Teleport(SitePlan.Ground(76,49,.1f));
            Walk(Vector2.zero,false,0);
            ViewName="Walk to the cabin";
        }
        public void Walk(Vector2 move, bool fast, float dt)
        {
            walker.Step(move,yaw,fast,dt);
            transform.SetPositionAndRotation(walker.Eye,Quaternion.Euler(pitch,yaw,0));
        }
        public void TogglePlanning() => planning.SetActive(!planning.activeSelf);
        public void Review(int index)
        {
            Walking=false;
            if(styleLighting) styleLighting.Apply(index);
            transform.position=SitePlan.ReviewPositions[index];
            focus=SitePlan.ReviewTargets[index];
            transform.LookAt(focus);
            distance=Vector3.Distance(transform.position,focus);
            yaw=transform.eulerAngles.y; pitch=transform.eulerAngles.x;
            ViewName=new[]{"01 / Southeast","02 / Expansion overview","03 / Entrance","04 / Window view"}[index];
        }

        void Styles()
        {
            if (title!=null) return;
            title=new GUIStyle(GUI.skin.label){fontSize=34,fontStyle=FontStyle.Bold}; title.normal.textColor=ink;
            small=new GUIStyle(GUI.skin.label){fontSize=11}; small.normal.textColor=new Color(.66f,.76f,.68f);
            body=new GUIStyle(GUI.skin.label){fontSize=14}; body.normal.textColor=ink;
            button=new GUIStyle(GUI.skin.button){fontSize=13};
        }
        void Panel(Rect r)
        {
            var c=GUI.color; GUI.color=new Color(.075f,.13f,.12f,.92f);
            GUI.DrawTexture(r,Texture2D.whiteTexture); GUI.color=c;
        }
        void OnGUI()
        {
            if (!ShowHud) return;
            Styles();
            float scale=Mathf.Min(Screen.width/1280f,Screen.height/720f);
            GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,Vector3.one*scale);
            float w=Screen.width/scale,h=Screen.height/scale;
            Panel(new Rect(24,24,310,127));
            GUI.Label(new Rect(42,36,280,20),styleLighting ? "CABINHOME     /     STYLE STUDY 009" : "CABINHOME     /     FIELD STUDY 01",small);
            GUI.Label(new Rect(40,57,285,48),"Cabinhome.",title);
            GUI.Label(new Rect(42,112,280,25),ViewName,body);
            Panel(new Rect(w-268,24,244,76));
            GUI.Label(new Rect(w-250,36,220,22),styleLighting ? "STYLE PROTOTYPE  /  " + styleLighting.PresetName : "SPATIAL PROTOTYPE  /  v0.1",small);
            GUI.Label(new Rect(w-250,61,220,25),"Cabin 48 m²  ·  Plot 3,072 m²",body);
            if(planning.activeSelf)
            {
                Panel(new Rect(w-268,112,244,194));
                GUI.Label(new Rect(w-250,124,220,22),"PLANNING / PROVISIONAL",small);
                string[] labels={"Plot     64 × 48 m","Phase 2     120 m²","Phase 3     192 m²","Workshop     48 m²","Greenhouse     32 m²","Garden 120 / Reserve 168 m²"};
                for(int i=0;i<labels.Length;i++)GUI.Label(new Rect(w-250,150+i*23,224,24),labels[i],body);
            }
            Panel(new Rect(24,h-122,w-48,98));
            if (GUI.Button(new Rect(40,h-106,150,32),Walking?"Tab  /  Overview":"Tab  /  Walk around",button)) {if(Walking)Home();else Explore();}
            if (GUI.Button(new Rect(200,h-106,140,32),planning.activeSelf?"B  /  Hide plans":"B  /  Show plans",button)) TogglePlanning();
            for(int i=0;i<4;i++) if(GUI.Button(new Rect(354+i*72,h-106,62,32),"View "+(i+1),button)) Review(i);
            if(GUI.Button(new Rect(660,h-106,100,32),"R  /  Reset",button)) Home();
            GUI.Label(new Rect(42,h-63,w-84,24),Walking
                ?"W A S D  move    ·    Hold right mouse + drag  look    ·    Shift  walk faster    ·    H  hide interface"
                :"Hold right mouse + drag  orbit    ·    Scroll  zoom    ·    W A S D  pan    ·    1–4  review views    ·    H  hide interface",body);
            GUI.matrix=Matrix4x4.identity;
        }
    }
}
