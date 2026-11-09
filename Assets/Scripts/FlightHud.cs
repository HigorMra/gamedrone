using UnityEngine;

public sealed class FlightHud : MonoBehaviour
{
    [SerializeField] DroneFlightController drone;
    public void SetDrone(DroneFlightController value) => drone = value;
    GUIStyle label, value, help;
    void OnGUI()
    {
        if (!drone) return;
        if (label == null)
        {
            label = new GUIStyle(GUI.skin.label) { fontSize = 12, normal = { textColor = new Color(.72f, .82f, .73f) } };
            value = new GUIStyle(label) { fontSize = 26, fontStyle = FontStyle.Bold, normal = { textColor = new Color(.84f, .96f, .66f) } };
            help = new GUIStyle(label) { fontSize = 12, alignment = TextAnchor.LowerCenter, normal = { textColor = Color.white } };
        }
        GUI.Label(new Rect(28, 25, 180, 24), "VELOCIDADE  KM/H", label);
        GUI.Label(new Rect(28, 46, 180, 38), (drone.Speed * 3.6f).ToString("00"), value);
        GUI.Label(new Rect(28, 88, 180, 24), "ALTITUDE  M", label);
        GUI.Label(new Rect(28, 109, 180, 38), drone.Altitude.ToString("00"), value);
        GUI.Label(new Rect(0, Screen.height - 42, Screen.width, 30), "WASD / SETAS  VOAR     CLIQUE  NOVA TRAJETÓRIA     SHIFT  TURBO     Q / E  ALTURA     R  REINICIAR", help);
    }
}
