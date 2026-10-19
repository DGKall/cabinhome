using UnityEngine;
using UnityEngine.Rendering;

namespace CabinHome
{
    // Static art-direction presets, not a weather or time-of-day simulation.
    public sealed class StyleLighting : MonoBehaviour
    {
        public Light sun;
        public Material sky;
        public string PresetName { get; private set; } = "Dusk";
        static Color C(string hex) { ColorUtility.TryParseHtmlString(hex, out var c); return c; }
        public void Apply(int view)
        {
            bool blue = view == 1, overcast = view == 2;
            PresetName = blue ? "Blue hour" : overcast ? "Soft overcast" : "Dusk";
            sun.transform.rotation = Quaternion.Euler(blue ? 12 : overcast ? 48 : 19, -48, 0);
            sun.intensity = blue ? .65f : overcast ? .95f : 1.35f;
            sun.color = C(blue ? "#ABB8ED" : overcast ? "#E4E7E5" : "#FFD6A7");
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = C(blue ? "#7889BA" : overcast ? "#ABBCC2" : "#A0B1C0");
            RenderSettings.ambientEquatorColor = C(blue ? "#647495" : overcast ? "#87968C" : "#90958C");
            RenderSettings.ambientGroundColor = C("#3E4947");
            RenderSettings.fogColor = C(blue ? "#7987AC" : overcast ? "#ADBDC0" : "#B0B9B9");
            RenderSettings.fogDensity = overcast ? .004f : .0023f;
            sky.SetColor("_Top", C(blue ? "#536C9C" : overcast ? "#93AAB4" : "#7B99B4"));
            sky.SetColor("_Horizon", C(blue ? "#ABB0CE" : overcast ? "#CDD2CC" : "#E5C4B1"));
            sky.SetColor("_Cloud", C(blue ? "#969EC1" : overcast ? "#D3D8D4" : "#F1D6BF"));
            RenderSettings.skybox = sky;
        }
    }
}
