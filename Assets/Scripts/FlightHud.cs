using UnityEngine;

/// <summary>Resolution-aware FPV speed/altitude display made from camera-space meshes.</summary>
public sealed class FlightHud : MonoBehaviour
{
    [SerializeField] DroneFlightController drone;
    [SerializeField] float overlayDistance = 1.35f;

    static readonly int[] DigitMasks = { 0x3f, 0x06, 0x5b, 0x4f, 0x66, 0x6d, 0x7d, 0x07, 0x7f, 0x6f };
    readonly Renderer[][] speedDigits = new Renderer[3][];
    readonly Renderer[][] altitudeDigits = new Renderer[2][];
    Transform overlay;
    Camera hudCamera;
    Material litMaterial;
    Material dimMaterial;
    int lastSpeed = -1;
    int lastAltitude = -1;

    public void SetDrone(DroneFlightController value) => drone = value;

    void Start()
    {
        if (!drone) return;
        hudCamera = Camera.main;
        if (!hudCamera) return;

        Shader shader = Shader.Find("Unlit/Color");
        if (!shader) shader = Shader.Find("Standard");
        litMaterial = new Material(shader) { name = "FPV HUD · phosphor green", color = new Color(.78f, 1f, .52f) };

        var root = new GameObject("FPV HUD · camera-space geometry");
        overlay = root.transform;
        overlay.SetParent(hudCamera.transform, false);

        dimMaterial = new Material(shader) { name = "FPV HUD · inactive segments", color = new Color(.10f, .18f, .12f) };
        CreateLabel("KMH", 0f, -.075f, .018f);
        CreateLabel("ALTM", 0f, -.48f, .018f);

        for (int i = 0; i < speedDigits.Length; i++)
            speedDigits[i] = CreateDigit(i * .205f, -.285f, dimMaterial);
        for (int i = 0; i < altitudeDigits.Length; i++)
            altitudeDigits[i] = CreateDigit(i * .205f, -.70f, dimMaterial);

        PositionOverlay();
        RefreshDigits();
    }

    void LateUpdate()
    {
        if (!overlay || !hudCamera || !drone) return;
        PositionOverlay();
        RefreshDigits();
    }

    void PositionOverlay()
    {
        float scale = Mathf.Clamp(Screen.height / 900f, .7f, 1.35f);
        float halfHeight = Mathf.Tan(hudCamera.fieldOfView * .5f * Mathf.Deg2Rad) * overlayDistance;
        float halfWidth = halfHeight * hudCamera.aspect;
        overlay.localPosition = new Vector3(-halfWidth + .13f * scale, halfHeight - .12f * scale, overlayDistance);
        overlay.localRotation = Quaternion.identity;
        overlay.localScale = Vector3.one * scale;
    }

    Renderer[] CreateDigit(float x, float y, Material dimMaterial)
    {
        var segments = new Renderer[7];
        segments[0] = CreateBar(x, y + .12f, .135f, .018f, dimMaterial);
        segments[1] = CreateBar(x + .068f, y + .06f, .018f, .105f, dimMaterial);
        segments[2] = CreateBar(x + .068f, y - .06f, .018f, .105f, dimMaterial);
        segments[3] = CreateBar(x, y - .12f, .135f, .018f, dimMaterial);
        segments[4] = CreateBar(x - .068f, y - .06f, .018f, .105f, dimMaterial);
        segments[5] = CreateBar(x - .068f, y + .06f, .018f, .105f, dimMaterial);
        segments[6] = CreateBar(x, y, .135f, .018f, dimMaterial);
        return segments;
    }

    Renderer CreateBar(float x, float y, float width, float height, Material material)
    {
        GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bar.name = "HUD segment";
        bar.transform.SetParent(overlay, false);
        bar.transform.localPosition = new Vector3(x, y, 0f);
        bar.transform.localScale = new Vector3(width, height, .012f);
        bar.GetComponent<Collider>().enabled = false;
        Renderer renderer = bar.GetComponent<Renderer>();
        renderer.sharedMaterial = material;
        return renderer;
    }

    void CreateLabel(string text, float x, float y, float cell)
    {
        for (int character = 0; character < text.Length; character++)
        {
            string[] rows = Glyph(text[character]);
            for (int row = 0; row < rows.Length; row++)
            for (int column = 0; column < rows[row].Length; column++)
            {
                if (rows[row][column] != '1') continue;
                GameObject pixel = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pixel.name = "HUD label pixel";
                pixel.transform.SetParent(overlay, false);
                pixel.transform.localPosition = new Vector3(x + character * cell * 4f + column * cell, y - row * cell, 0f);
                pixel.transform.localScale = Vector3.one * (cell * .78f);
                pixel.GetComponent<Collider>().enabled = false;
                pixel.GetComponent<Renderer>().sharedMaterial = litMaterial;
            }
        }
    }

    static string[] Glyph(char character)
    {
        switch (character)
        {
            case 'K': return new[] { "101", "110", "100", "110", "101" };
            case 'M': return new[] { "101", "111", "111", "101", "101" };
            case 'H': return new[] { "101", "111", "101", "101", "101" };
            case 'A': return new[] { "010", "101", "111", "101", "101" };
            case 'L': return new[] { "100", "100", "100", "100", "111" };
            case 'T': return new[] { "111", "010", "010", "010", "010" };
            default: return new[] { "000", "000", "000", "000", "000" };
        }
    }

    void RefreshDigits()
    {
        int speed = Mathf.Clamp(Mathf.RoundToInt(drone.Speed * 3.6f), 0, 999);
        int altitude = Mathf.Clamp(Mathf.RoundToInt(drone.Altitude), 0, 99);
        if (speed != lastSpeed)
        {
            SetNumber(speedDigits, speed);
            lastSpeed = speed;
        }
        if (altitude != lastAltitude)
        {
            SetNumber(altitudeDigits, altitude);
            lastAltitude = altitude;
        }
    }

    void SetNumber(Renderer[][] digits, int number)
    {
        for (int index = 0; index < digits.Length; index++)
        {
            int divisor = (int)Mathf.Pow(10, digits.Length - index - 1);
            int numeral = number / divisor % 10;
            int mask = DigitMasks[numeral];
            for (int segment = 0; segment < 7; segment++)
                digits[index][segment].sharedMaterial = (mask & (1 << segment)) != 0 ? litMaterial : dimMaterial;
        }
    }

    void OnDestroy()
    {
        if (litMaterial) Destroy(litMaterial);
        if (dimMaterial) Destroy(dimMaterial);
    }
}
