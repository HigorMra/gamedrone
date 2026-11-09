using UnityEngine;

public sealed class FlightHud : MonoBehaviour
{
    [SerializeField] DroneFlightController drone;
    public void SetDrone(DroneFlightController value) => drone = value;
    GUIStyle label, value, help;

    void OnGUI()
    {
        if (!drone) return;
        EnsureStyles();

        float scale = Mathf.Clamp(Screen.height / 900f, .7f, 1.35f);
        float left = 26f * scale;
        float top = 24f * scale;
        float labelHeight = 20f * scale;
        float valueHeight = 37f * scale;
        float gap = 8f * scale;

        GUI.Label(new Rect(left, top, 210f * scale, labelHeight), "VELOCIDADE  KM/H", label);
        GUI.Label(new Rect(left, top + labelHeight, 210f * scale, valueHeight), Mathf.RoundToInt(drone.Speed * 3.6f).ToString("00"), value);
        float altitudeTop = top + labelHeight + valueHeight + gap;
        GUI.Label(new Rect(left, altitudeTop, 210f * scale, labelHeight), "ALTITUDE  M", label);
        GUI.Label(new Rect(left, altitudeTop + labelHeight, 210f * scale, valueHeight), drone.Altitude.ToString("00.0"), value);

        string controls = "WASD / SETAS  VOAR     CLIQUE  NOVA TRAJETÓRIA     SHIFT  TURBO     Q / E  ALTURA     R  REINICIAR";
        float footerHeight = 30f * scale;
        GUI.Label(new Rect(12f * scale, Screen.height - footerHeight - 12f * scale, Screen.width - 24f * scale, footerHeight), controls, help);
    }

    void EnsureStyles()
    {
        if (label != null) return;
        label = new GUIStyle(GUI.skin.label)
        {
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(.72f, .82f, .73f) }
        };
        value = new GUIStyle(label)
        {
            fontSize = 26,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(.84f, .96f, .66f) }
        };
        help = new GUIStyle(label)
        {
            fontSize = 11,
            alignment = TextAnchor.LowerCenter,
            wordWrap = true,
            normal = { textColor = Color.white }
        };
    }
}
