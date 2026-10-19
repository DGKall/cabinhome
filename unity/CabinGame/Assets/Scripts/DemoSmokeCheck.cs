using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace CabinHome
{
    // Opt-in executable integration check. Normal launches never run this sequence.
    public sealed class DemoSmokeCheck : MonoBehaviour
    {
        bool errors;
        IEnumerator Start()
        {
            string[] args=Environment.GetCommandLineArgs();
            int index=Array.IndexOf(args,"-cabinSmoke");
            if(index<0||index+1>=args.Length)yield break;
            string dir=args[index+1]; Directory.CreateDirectory(dir);
            Application.logMessageReceived+=OnLog;
            var demo=GetComponent<DemoController>();
            demo.Automated=true;
            yield return null;
            demo.Home();
            yield return Capture(dir,"00-arrival.png");
            demo.planning.SetActive(!demo.styleLighting);
            if(demo.styleLighting) demo.ShowHud=false;
            for(int i=0;i<4;i++)
            {
                demo.Review(i);
                yield return Capture(dir,"0"+(i+1)+"-review.png");
            }
            demo.planning.SetActive(false);
            if(demo.styleLighting)
            {
                demo.styleLighting.Apply(0);
                transform.position=SitePlan.Ground(73.8f,51.8f,.8f);
                transform.LookAt(SitePlan.Ground(72.6f,53,.22f));
                yield return Capture(dir,"06-stone-detail.png");
                transform.position=SitePlan.Ground(70,58,3);
                transform.LookAt(SitePlan.Ground(66,62,2.6f));
                yield return Capture(dir,"07-tree-detail.png");
            }
            demo.Explore();
            for(int i=0;i<300;i++){demo.Walk(Vector2.up,false,1f/60);yield return null;}
            Vector3 inside=demo.walker.transform.position;
            bool entered=inside.z>60&&inside.z<65.8f&&Mathf.Abs(inside.y-2)<.15f;
            for(int i=0;i<120;i++){demo.Walk(Vector2.up,false,1f/60);yield return null;}
            bool wall=demo.walker.transform.position.z<65.8f;
            yield return Capture(dir,"05-walk-inside.png");
            bool passed=entered&&wall&&!errors;
            string report="URP standalone smoke: "+(passed?"PASS":"FAIL")+"\n"+
                "Walk from path, climb steps, enter cabin: "+entered+" at "+inside+"\n"+
                "North wall blocks player: "+wall+"\nRuntime errors: "+errors+"\n"+
                "Unity: "+Application.unityVersion+"\nGPU: "+SystemInfo.graphicsDeviceName+"\n";
            File.WriteAllText(Path.Combine(dir,"report.txt"),report);
            Debug.Log("CABIN_RUNTIME_SMOKE "+report);
            Application.logMessageReceived-=OnLog;
            Application.Quit(passed?0:1);
        }
        IEnumerator Capture(string dir,string name)
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            var texture=ScreenCapture.CaptureScreenshotAsTexture();
            File.WriteAllBytes(Path.Combine(dir,name),texture.EncodeToPNG());
            Destroy(texture);
        }
        void OnLog(string message,string stack,LogType type)
        {if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors=true;}
    }
}
