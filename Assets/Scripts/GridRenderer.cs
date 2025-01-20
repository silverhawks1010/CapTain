using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GridRenderer : MonoBehaviour
{
    [SerializeField] private int gridSize = 10;
    [SerializeField] private Color gridColor = Color.white;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private float heightPercentage = 0.95f; // Augmenté à 95% de la hauteur disponible

    private float cellSize;  // Will be calculated based on container size
    public float CellSize => cellSize;

    private GridCell[,] grid;
    private RectTransform rectTransform;
    private float lastHeight;
    private bool isUpdating = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // Fixer les anchors au centre
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
        }
        
        lastHeight = GetParentHeight();
        InitializeGridOnce();
    }

    private float GetParentHeight()
    {
        if (transform.parent == null) return 800f;

        RectTransform parentRect = transform.parent.GetComponent<RectTransform>();
        if (parentRect == null) return 800f;

        return parentRect.rect.height;
    }

    private void InitializeGridOnce()
    {
        if (grid != null) return;

        if (cellPrefab == null)
        {
            Debug.LogError("GridRenderer: cellPrefab is not assigned!");
            return;
        }

        CalculateCellSize();
        CreateGrid();
    }

    private void CalculateCellSize()
    {
        if (isUpdating) return;
        isUpdating = true;

        float parentHeight = GetParentHeight();
        // On utilise toute la hauteur disponible
        float availableHeight = parentHeight * heightPercentage;
        // On divise par le nombre de cellules sans marge supplémentaire
        cellSize = availableHeight / gridSize;

        if (rectTransform != null)
        {
            float totalSize = cellSize * gridSize;
            rectTransform.sizeDelta = new Vector2(totalSize, totalSize);
        }

        isUpdating = false;
    }

    private void Update()
    {
        float currentHeight = GetParentHeight();
        if (!Mathf.Approximately(currentHeight, lastHeight) && !isUpdating)
        {
            lastHeight = currentHeight;
            CalculateCellSize();
            UpdateGridCellSizes();
        }
    }

    private void UpdateGridCellSizes()
    {
        if (isUpdating || grid == null) return;
        isUpdating = true;

        float totalSize = cellSize * gridSize;
        float startX = -totalSize / 2f;
        float startY = totalSize / 2f;

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                if (grid[x, y] != null)
                {
                    RectTransform cellRect = grid[x, y].GetComponent<RectTransform>();
                    if (cellRect != null)
                    {
                        cellRect.sizeDelta = new Vector2(cellSize, cellSize);
                        float posX = startX + (x * cellSize) + (cellSize / 2f);
                        float posY = startY - (y * cellSize) - (cellSize / 2f);
                        cellRect.anchoredPosition = new Vector2(posX, posY);
                    }
                }
            }
        }

        isUpdating = false;
    }

    private void CreateGrid()
    {
        if (grid != null)
        {
            foreach (var cell in grid)
            {
                if (cell != null && cell.gameObject != null)
                {
                    Destroy(cell.gameObject);
                }
            }
        }

        grid = new GridCell[gridSize, gridSize];
        float totalSize = cellSize * gridSize;
        float startX = -totalSize / 2f;
        float startY = totalSize / 2f;

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                CreateCell(x, y, startX, startY);
            }
        }
    }

    private void CreateCell(int x, int y, float startX, float startY)
    {
        GameObject cell = Instantiate(cellPrefab, transform);
        RectTransform cellRect = cell.GetComponent<RectTransform>();

        if (cellRect != null)
        {
            cellRect.anchorMin = new Vector2(0.5f, 0.5f);
            cellRect.anchorMax = new Vector2(0.5f, 0.5f);
            cellRect.pivot = new Vector2(0.5f, 0.5f);
            cellRect.sizeDelta = new Vector2(cellSize, cellSize);

            float posX = startX + (x * cellSize) + (cellSize / 2f);
            float posY = startY - (y * cellSize) - (cellSize / 2f);
            cellRect.anchoredPosition = new Vector2(posX, posY);
        }

        GridCell gridCell = cell.GetComponent<GridCell>();
        if (gridCell == null)
        {
            gridCell = cell.AddComponent<GridCell>();
        }
        
        gridCell.Initialize(x, y);
        grid[x, y] = gridCell;

        Button button = cell.GetComponent<Button>();
        if (button != null)
        {
            int xPos = x;
            int yPos = y;
            button.onClick.AddListener(() => OnCellClicked(xPos, yPos));
        }
    }

    public Vector2Int GetGridCoordinates(Vector2 localPosition)
    {
        float totalSize = gridSize * cellSize;
        float startX = -totalSize / 2f;
        float startY = totalSize / 2f;

        float relativeX = localPosition.x - startX;
        float relativeY = startY - localPosition.y;

        int x = Mathf.FloorToInt(relativeX / cellSize);
        int y = Mathf.FloorToInt(relativeY / cellSize);

        return new Vector2Int(
            Mathf.Clamp(x, 0, gridSize - 1),
            Mathf.Clamp(y, 0, gridSize - 1)
        );
    }

    public Vector2 GetCellPosition(int x, int y)
    {
        if (x < 0 || x >= gridSize || y < 0 || y >= gridSize)
            return Vector2.zero;

        float totalSize = gridSize * cellSize;
        float startX = -totalSize / 2f;
        float startY = totalSize / 2f;

        return new Vector2(
            startX + (x * cellSize) + (cellSize / 2f),
            startY - (y * cellSize) - (cellSize / 2f)
        );
    }

    public bool IsCellOccupied(int x, int y)
    {
        if (x >= 0 && x < gridSize && y >= 0 && y < gridSize && grid[x, y] != null)
        {
            return grid[x, y].IsOccupied;
        }
        return false;
    }

    public void SetCellOccupied(int x, int y, bool occupied)
    {
        if (x >= 0 && x < gridSize && y >= 0 && y < gridSize && grid[x, y] != null)
        {
            grid[x, y].IsOccupied = occupied;
        }
    }

    private void OnCellClicked(int x, int y)
    {
        Debug.Log($"Cell clicked at: {x}, {y}");
    }

    private void OnDestroy()
    {
        if (grid != null)
        {
            foreach (var cell in grid)
            {
                if (cell != null)
                {
                    var button = cell.GetComponent<Button>();
                    if (button != null)
                    {
                        button.onClick.RemoveAllListeners();
                    }
                }
            }
        }
    }
}
