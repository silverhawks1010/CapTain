using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Ship : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public int Size { get; private set; }
    public bool IsVertical { get; private set; }
    public Vector2Int GridPosition { get; private set; }

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;
    private GridRenderer gridRenderer;
    private Canvas canvas;
    private Vector2 dragOffset;

    [SerializeField] private float widthOffset = 15f;
    [SerializeField] private float horizontalBackOffset = 25f;
    [SerializeField] private float horizontalBottomOffset = 25f;
    [SerializeField] private float verticalBackOffset = 15f;
    [SerializeField] private float verticalBottomOffset = 15f;
    [SerializeField] private float doubleClickTime = 0.3f;

    private float cellSize;
    private float lastClickTime = 0f;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        gridRenderer = FindObjectOfType<GridRenderer>();
        
        canvas = GetComponentInParent<Canvas>();
        
        if (canvas == null)
        {
            canvas = FindObjectOfType<Canvas>();
        }
        
        if (canvas == null)
        {
            Debug.LogError("No Canvas found in the scene!");
        }

        if (gridRenderer != null)
        {
            cellSize = gridRenderer.CellSize;
        }
        else
        {
            Debug.LogError("No GridRenderer found!");
        }
    }

    public void Initialize(int size, Vector2Int startPosition)
    {
        Size = size;
        IsVertical = false;
        GridPosition = startPosition;
        
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = IsVertical 
                ? new Vector2(cellSize + widthOffset, cellSize * Size)
                : new Vector2(cellSize * Size, cellSize + widthOffset);
        }
        
        UpdatePosition();
    }

    public void UpdatePosition()
    {
        if (gridRenderer == null) return;

        float backOffset = IsVertical ? verticalBackOffset : horizontalBackOffset;
        float bottomOffset = IsVertical ? verticalBottomOffset : horizontalBottomOffset;

        Vector2 basePos = gridRenderer.GetCellPosition(GridPosition.x, GridPosition.y);
        if (IsVertical)
        {
            basePos.y += backOffset;
            basePos.x += bottomOffset + 5f;
        }
        else
        {
            basePos.x -= backOffset;
            basePos.y -= bottomOffset;
        }
        
        rectTransform.anchoredPosition = basePos;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        float timeSinceLastClick = Time.time - lastClickTime;
        
        if (timeSinceLastClick <= doubleClickTime)
        {
            RotateShip();
        }
        
        lastClickTime = Time.time;
    }

    public void RotateShip()
    {
        for (int i = 0; i < Size; i++)
        {
            Vector2Int pos = GridPosition + (IsVertical ? new Vector2Int(0, i) : new Vector2Int(i, 0));
            gridRenderer.SetCellOccupied(pos.x, pos.y, false);
        }

        bool wasVertical = IsVertical;
        IsVertical = !IsVertical;
        
        if (rectTransform != null)
        {
            rectTransform.rotation = IsVertical ? Quaternion.Euler(0, 0, -90) : Quaternion.Euler(0, 0, 0);
            
            rectTransform.sizeDelta = new Vector2(cellSize * Size, cellSize + widthOffset);
        }

        if (!CanPlaceShip(GridPosition))
        {
            IsVertical = wasVertical;
            if (rectTransform != null)
            {
                rectTransform.rotation = IsVertical ? Quaternion.Euler(0, 0, 90) : Quaternion.Euler(0, 0, 0);
                rectTransform.sizeDelta = new Vector2(cellSize * Size, cellSize + widthOffset);
            }
        }

        UpdatePosition();

        for (int i = 0; i < Size; i++)
        {
            Vector2Int pos = GridPosition + (IsVertical ? new Vector2Int(0, i) : new Vector2Int(i, 0));
            gridRenderer.SetCellOccupied(pos.x, pos.y, true);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPosition = rectTransform.anchoredPosition;
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;

        Vector2 mousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            gridRenderer.GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera,
            out mousePos
        );
        dragOffset = rectTransform.anchoredPosition - mousePos;

        for (int i = 0; i < Size; i++)
        {
            Vector2Int pos = GridPosition + (IsVertical ? new Vector2Int(0, i) : new Vector2Int(i, 0));
            gridRenderer.SetCellOccupied(pos.x, pos.y, false);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas == null || gridRenderer == null) return;

        Vector2 mousePos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            gridRenderer.GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera,
            out mousePos))
        {
            rectTransform.anchoredPosition = mousePos + dragOffset;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        Vector2 mousePos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            gridRenderer.GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera,
            out mousePos))
        {
            Vector2 finalPos = mousePos + dragOffset;
            Vector2Int newGridPos = gridRenderer.GetGridCoordinates(finalPos);

            if (CanPlaceShip(newGridPos))
            {
                GridPosition = newGridPos;
                UpdatePosition();
                
                for (int i = 0; i < Size; i++)
                {
                    Vector2Int pos = GridPosition + (IsVertical ? new Vector2Int(0, i) : new Vector2Int(i, 0));
                    gridRenderer.SetCellOccupied(pos.x, pos.y, true);
                }
            }
            else
            {
                rectTransform.anchoredPosition = originalPosition;
                
                for (int i = 0; i < Size; i++)
                {
                    Vector2Int pos = GridPosition + (IsVertical ? new Vector2Int(0, i) : new Vector2Int(i, 0));
                    gridRenderer.SetCellOccupied(pos.x, pos.y, true);
                }
            }
        }
    }

    private bool CanPlaceShip(Vector2Int pos)
    {
        for (int i = 0; i < Size; i++)
        {
            Vector2Int checkPos = pos + (IsVertical ? new Vector2Int(0, i) : new Vector2Int(i, 0));
            if (checkPos.x < 0 || checkPos.x >= 10 || checkPos.y < 0 || checkPos.y >= 10)
                return false;
            
            if (gridRenderer.IsCellOccupied(checkPos.x, checkPos.y))
                return false;
        }
        return true;
    }
}
