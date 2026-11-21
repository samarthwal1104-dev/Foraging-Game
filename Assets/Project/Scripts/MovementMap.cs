using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(210)]
public class MovementMap : MonoBehaviour
{
    public enum Entity { Player, Drone }

    [Header("UI Target (assign RawImage)")]
    public RawImage targetImage;

    // --- NEW VARIABLE ---
    [Header("Map Background")]
    [Tooltip("Assign the high-res top-down map screenshot here. Must be Read/Write Enabled.")]
    public Texture2D baseMapTexture;
    // --------------------

    [Header("Map Settings")]
    [Tooltip("World-space area to visualize (X/Z axes). Expand if trails clip.")]
    public Vector2 worldMinXZ = new Vector2(0f, 0f);
    public Vector2 worldMaxXZ = new Vector2(300f, 300f);

    [Tooltip("Texture size of the map (pixels). Will be overridden by Base Map if provided.")]
    public int textureWidth = 1024;
    public int textureHeight = 1024;

    [Tooltip("Trail thickness in pixels")]
    public int trailThickness = 2;

    [Header("Colors")]
    public Color playerColor = new Color(0.2f, 0.6f, 1f, 1f); // blue
    public Color droneColor = new Color(1f, 0.2f, 0.2f, 1f); // red
    public Color backgroundColor = new Color(0.08f, 0.08f, 0.08f, 1f); // Fallback color

    private Texture2D tex;
    private Color[] bgRow;
    private List<Vector2Int> playerPixels = new List<Vector2Int>();
    private List<Vector2Int> dronePixels = new List<Vector2Int>();

    // --- THIS METHOD IS MODIFIED ---
    private void Start()
    {
        // If a base map is provided, use its dimensions
        if (baseMapTexture != null)
        {
            textureWidth = baseMapTexture.width;
            textureHeight = baseMapTexture.height;
        }

        tex = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;

        if (baseMapTexture != null)
        {
            try
            {
                // Copy the base map pixels onto our trail texture
                tex.SetPixels(baseMapTexture.GetPixels());
            }
            catch (Exception e)
            {
                Debug.LogError($"MovementMap Error: Failed to GetPixels from baseMapTexture. Did you forget to set 'Read/Write Enabled' in its import settings? Error: {e.Message}");
                ClearToBackgroundColor(); // Fallback
            }
        }
        else
        {
            // Fallback: clear to solid background color
            ClearToBackgroundColor();
        }

        tex.Apply();
        if (targetImage != null) targetImage.texture = tex;
    }

    // Helper method for clearing background
    private void ClearToBackgroundColor()
    {
        bgRow = new Color[textureWidth];
        for (int i = 0; i < textureWidth; i++) bgRow[i] = backgroundColor;
        for (int y = 0; y < textureHeight; y++) tex.SetPixels(0, y, textureWidth, 1, bgRow);
    }

    // --- All other methods (AddPoint, SavePNG, WorldToPixel, etc.) are unchanged ---
    // (You can copy-paste them from your old script if needed)

    public void AddPoint(Entity entity, Vector3 worldPos)
    {
        Vector2Int pix = WorldToPixel(worldPos);

        // Append to buffer
        if (entity == Entity.Player) playerPixels.Add(pix);
        else dronePixels.Add(pix);

        // Draw a small line segment between the last two points for smoothness
        if (entity == Entity.Player && playerPixels.Count >= 2)
        {
            DrawLine(playerPixels[playerPixels.Count - 2], playerPixels[playerPixels.Count - 1], playerColor, trailThickness);
        }
        else if (entity == Entity.Drone && dronePixels.Count >= 2)
        {
            DrawLine(dronePixels[dronePixels.Count - 2], dronePixels[dronePixels.Count - 1], droneColor, trailThickness);
        }

        tex.Apply(false);
    }

    public void SavePNG(string extraTag = "")
    {
        try
        {
            string directoryPath = Path.Combine(Application.dataPath, "Project/OutputFiles");
            if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

            string fileName = $"MovementMap_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}{(string.IsNullOrEmpty(extraTag) ? "" : "_" + extraTag)}.png";
            string filePath = Path.Combine(directoryPath, fileName);

            byte[] png = tex.EncodeToPNG();
            File.WriteAllBytes(filePath, png);
            Debug.Log($"MovementMap: saved PNG to {filePath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"MovementMap SavePNG failed: {ex.Message}");
        }
    }

    private Vector2Int WorldToPixel(Vector3 worldPos)
    {
        float nx = Mathf.InverseLerp(worldMinXZ.x, worldMaxXZ.x, worldPos.x);
        float nz = Mathf.InverseLerp(worldMinXZ.y, worldMaxXZ.y, worldPos.z);

        int px = Mathf.Clamp(Mathf.RoundToInt(nx * (textureWidth - 1)), 0, textureWidth - 1);
        int py = Mathf.Clamp(Mathf.RoundToInt(nz * (textureHeight - 1)), 0, textureHeight - 1);
        return new Vector2Int(px, py);
    }

    private void DrawLine(Vector2Int a, Vector2Int b, Color col, int thickness)
    {
        int dx = Mathf.Abs(b.x - a.x);
        int dy = Mathf.Abs(b.y - a.y);
        int sx = a.x < b.x ? 1 : -1;
        int sy = a.y < b.y ? 1 : -1;
        int err = dx - dy;

        int x = a.x;
        int y = a.y;

        while (true)
        {
            DrawDot(new Vector2Int(x, y), col, thickness);
            if (x == b.x && y == b.y) break;
            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x += sx; }
            if (e2 < dx) { err += dx; y += sy; }
        }
    }

    private void DrawDot(Vector2Int p, Color col, int radius)
    {
        int r = Mathf.Max(1, radius);
        for (int oy = -r; oy <= r; oy++)
        {
            for (int ox = -r; ox <= r; ox++)
            {
                int px = p.x + ox;
                int py = p.y + oy;
                if (px < 0 || px >= textureWidth || py < 0 || py >= textureHeight) continue;
                tex.SetPixel(px, py, col);
            }
        }
    }
}