using UnityEngine;
using UnityEngine.UI;

public class GridCell : MonoBehaviour
{
    private static readonly Color OCCUPIED_COLOR = new Color(1f, 0f, 0f, 1f);
    private static readonly Color UNOCCUPIED_COLOR = new Color(0.4f, 0.8f, 1f, 1f);

    public int X { get; private set; }
    public int Y { get; private set; }
    private bool isOccupied;
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
}
