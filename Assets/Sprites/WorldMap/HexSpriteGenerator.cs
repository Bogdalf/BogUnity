using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class HexSpriteGenerator : MonoBehaviour
{
    [Header("Hex Settings")]
    [SerializeField] private int hexSize = 128; // Texture size
    [SerializeField] private int borderThickness = 8;
    [SerializeField] private Color borderColor = Color.white;

    [Header("Output")]
    [SerializeField] private string spriteName = "HexBorder";
    [SerializeField] private string savePath = "Assets/Sprites/WorldMap/";

#if UNITY_EDITOR
    [ContextMenu("Generate Hex Border Sprite")]
    public void GenerateHexSprite()
    {
        // Create texture
        Texture2D texture = new Texture2D(hexSize, hexSize, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;

        // Clear to transparent
        Color[] colors = new Color[hexSize * hexSize];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = Color.clear;
        }
        texture.SetPixels(colors);

        // Draw hex border
        DrawHexBorder(texture, hexSize, borderThickness, borderColor);

        texture.Apply();

        // Save as PNG
        byte[] bytes = texture.EncodeToPNG();

        // Ensure directory exists
        if (!System.IO.Directory.Exists(savePath))
        {
            System.IO.Directory.CreateDirectory(savePath);
        }

        string fullPath = savePath + spriteName + ".png";
        System.IO.File.WriteAllBytes(fullPath, bytes);

        Debug.Log($"Hex sprite saved to: {fullPath}");

        // Refresh asset database
        AssetDatabase.Refresh();

        // Import and configure as sprite
        TextureImporter importer = AssetImporter.GetAtPath(fullPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100;
            importer.filterMode = FilterMode.Bilinear;
            importer.alphaIsTransparency = true;
            AssetDatabase.WriteImportSettingsIfDirty(fullPath);
            AssetDatabase.ImportAsset(fullPath, ImportAssetOptions.ForceUpdate);
        }

        Debug.Log("Hex border sprite generated successfully!");
    }

    private void DrawHexBorder(Texture2D texture, int size, int thickness, Color color)
    {
        float centerX = size / 2f;
        float centerY = size / 2f;
        float radius = size / 2f - thickness / 2f - 2; // Slight padding

        // Flat-top hexagon vertices
        Vector2[] vertices = new Vector2[6];
        for (int i = 0; i < 6; i++)
        {
            float angle = 60f * i * Mathf.Deg2Rad;
            vertices[i] = new Vector2(
                centerX + radius * Mathf.Cos(angle),
                centerY + radius * Mathf.Sin(angle)
            );
        }

        // Draw lines between vertices
        for (int i = 0; i < 6; i++)
        {
            Vector2 start = vertices[i];
            Vector2 end = vertices[(i + 1) % 6];
            DrawThickLine(texture, start, end, thickness, color);
        }
    }

    private void DrawThickLine(Texture2D texture, Vector2 start, Vector2 end, int thickness, Color color)
    {
        int steps = Mathf.CeilToInt(Vector2.Distance(start, end));

        for (int i = 0; i <= steps; i++)
        {
            float t = i / (float)steps;
            Vector2 point = Vector2.Lerp(start, end, t);

            // Draw circle at this point for thickness
            DrawCircle(texture, point, thickness / 2f, color);
        }
    }

    private void DrawCircle(Texture2D texture, Vector2 center, float radius, Color color)
    {
        int minX = Mathf.Max(0, Mathf.FloorToInt(center.x - radius));
        int maxX = Mathf.Min(texture.width - 1, Mathf.CeilToInt(center.x + radius));
        int minY = Mathf.Max(0, Mathf.FloorToInt(center.y - radius));
        int maxY = Mathf.Min(texture.height - 1, Mathf.CeilToInt(center.y + radius));

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                if (distance <= radius)
                {
                    // Smooth edge with alpha
                    float alpha = 1f - Mathf.Clamp01(distance - radius + 1f);
                    Color pixelColor = color;
                    pixelColor.a *= alpha;

                    Color existingColor = texture.GetPixel(x, y);
                    Color blendedColor = Color.Lerp(existingColor, pixelColor, pixelColor.a);

                    texture.SetPixel(x, y, blendedColor);
                }
            }
        }
    }
#endif
}