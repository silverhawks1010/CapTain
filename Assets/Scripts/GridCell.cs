using UnityEngine;
using UnityEngine.UI;

public class GridCell : MonoBehaviour
{
    private static readonly Color OCCUPIED_COLOR = new Color(1f, 0f, 0f, 1f);
    private static readonly Color UNOCCUPIED_COLOR = new Color(0.4f, 0.8f, 1f, 1f);

    public int X { get; private set; }
    public int Y { get; private set; }
    private bool isOccupied;
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private Color missColor = Color.white;
    private Image cellImage;
    
    public bool IsOccupied
    {
        get => isOccupied;
        set
        {
            if (isOccupied != value)
            {
                isOccupied = value;
                UpdateCellColor();
            }
        }
    }

    private void Awake()
    {
        cellImage = GetComponent<Image>();
        if (cellImage != null)
        {
            cellImage.color = UNOCCUPIED_COLOR;
            // S'assurer que l'image n'est pas transparente
            cellImage.raycastTarget = true;
            cellImage.maskable = false;
        }
    }

    public void Initialize(int x, int y)
    {
        X = x;
        Y = y;
        IsOccupied = false;
        name = $"Cell_{x}_{y}";
    }

    private void UpdateCellColor()
    {
        if (cellImage != null)
        {
            cellImage.color = isOccupied ? OCCUPIED_COLOR : UNOCCUPIED_COLOR;
        }
    }

    public void SetHitState(bool isHit)
    {
        // Créer un marqueur de tir (X rouge pour touché, point blanc pour manqué)
        GameObject marker = new GameObject("HitMarker");
        marker.transform.SetParent(transform);
        
        Image markerImage = marker.AddComponent<Image>();
        RectTransform markerRect = marker.GetComponent<RectTransform>();
        
        // Configurer le RectTransform
        markerRect.anchorMin = new Vector2(0.5f, 0.5f);
        markerRect.anchorMax = new Vector2(0.5f, 0.5f);
        markerRect.pivot = new Vector2(0.5f, 0.5f);
        
        if (isHit)
        {
            // Créer un X rouge pour une touche
            markerImage.sprite = Resources.Load<Sprite>("X"); // Assurez-vous d'avoir un sprite X
            markerImage.color = hitColor;
            markerRect.sizeDelta = new Vector2(20, 20);
        }
        else
        {
            // Créer un point blanc pour un tir manqué
            markerImage.sprite = Resources.Load<Sprite>("Dot"); // Assurez-vous d'avoir un sprite point
            markerImage.color = missColor;
            markerRect.sizeDelta = new Vector2(10, 10);
        }
    }
}
