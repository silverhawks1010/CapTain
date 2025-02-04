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
    private GridRenderer gridRenderer;
    private Canvas canvas;
    private Vector2 dragOffset;
    private float cellSize;
    private float lastClickTime = 0f;
    [SerializeField] private float doubleClickTime = 0.3f;

    private Vector2 startPosition;
    private Vector2Int startGridPosition;
    private bool isDragging = false;
    private bool isLocked = false;

    private bool[] hitCells; // true pour les cellules touchées

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        
        // Configuration initiale du RectTransform
        if (rectTransform != null)
        {
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
        }

        // S'assurer que l'image est configurée correctement
        Image shipImage = GetComponent<Image>();
        if (shipImage != null)
        {
            shipImage.raycastTarget = true;
            shipImage.maskable = true;
        }

        // S'assurer que le CanvasGroup est configuré correctement
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
    }

    private void Start()
    {
        if (gridRenderer == null)
        {
            gridRenderer = FindObjectOfType<GridRenderer>();
        }
        
        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                canvas = FindObjectOfType<Canvas>();
            }
        }

        if (gridRenderer == null)
        {
            Debug.LogError("No GridRenderer found in the scene!", this);
            return;
        }

        if (canvas == null)
        {
            Debug.LogError("No Canvas found in the scene!", this);
            return;
        }

        // Mettre à jour la taille initiale
        cellSize = gridRenderer.CellSize;
        if (Size > 0)
        {
            UpdateSize();
            UpdatePosition();
        }
    }

    private void Update()
    {
        if (gridRenderer != null && !Mathf.Approximately(cellSize, gridRenderer.CellSize))
        {
            cellSize = gridRenderer.CellSize;
            UpdateSize();
            UpdatePosition();
        }
    }

    public void Initialize(int size, Vector2Int position, bool isHorizontal, GridRenderer renderer)
    {
        Size = size;
        GridPosition = position;
        IsVertical = !isHorizontal; // true si le bateau est vertical
        gridRenderer = renderer;
        cellSize = gridRenderer.CellSize;
        hitCells = new bool[size]; // Initialiser le tableau des cellules touchées

        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }

        // Appliquer la rotation initiale
        if (rectTransform != null)
        {
            rectTransform.rotation = IsVertical ? Quaternion.Euler(0, 0, 90) : Quaternion.Euler(0, 0, 0);
        }

        UpdateSize();
        UpdatePosition();

        // Marquer toutes les cellules occupées par le bateau
        for (int i = 0; i < Size; i++)
        {
            Vector2Int pos = GridPosition + (IsVertical ? new Vector2Int(0, i) : new Vector2Int(i, 0));
            gridRenderer.SetCellOccupied(pos.x, pos.y, true);
        }
    }

    private void UpdateSize()
    {
        if (rectTransform == null || Size <= 0) return;

        float margin = 4f; // marge en pixels
        // En horizontal, on applique la taille sur l'axe X
        rectTransform.sizeDelta = new Vector2(cellSize * Size - margin, cellSize - margin);
        // Centrer le pivot pour la rotation
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
    }

    public void UpdatePosition()
    {
        if (gridRenderer == null || rectTransform == null) return;

        Vector2 basePos = gridRenderer.GetCellPosition(GridPosition.x, GridPosition.y);
        
        // Centrer le bateau sur ses cellules
        if (IsVertical)
        {
            basePos.y -= (cellSize * (Size - 1)) / 2f;
        }
        else
        {
            basePos.x += (cellSize * (Size - 1)) / 2f;
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
        if (isLocked) return;

        // Libérer les cellules actuelles
        for (int i = 0; i < Size; i++)
        {
            Vector2Int pos = GridPosition + (IsVertical ? new Vector2Int(0, i) : new Vector2Int(i, 0));
            gridRenderer.SetCellOccupied(pos.x, pos.y, false);
        }

        // Changer l'orientation
        IsVertical = !IsVertical;

        // Vérifier si la nouvelle position est valide
        if (!IsValidPosition(GridPosition))
        {
            // Si la position n'est pas valide, revenir à l'orientation précédente
            IsVertical = !IsVertical;
            
            // Remettre les cellules occupées
            for (int i = 0; i < Size; i++)
            {
                Vector2Int pos = GridPosition + (IsVertical ? new Vector2Int(0, i) : new Vector2Int(i, 0));
                gridRenderer.SetCellOccupied(pos.x, pos.y, true);
            }
            return;
        }

        // Faire pivoter l'image
        if (rectTransform != null)
        {
            rectTransform.rotation = IsVertical ? Quaternion.Euler(0, 0, 90) : Quaternion.Euler(0, 0, 0);
        }

        // Mettre à jour la taille et la position
        UpdateSize();
        UpdatePosition();

        // Marquer les nouvelles cellules comme occupées
        for (int i = 0; i < Size; i++)
        {
            Vector2Int pos = GridPosition + (IsVertical ? new Vector2Int(0, i) : new Vector2Int(i, 0));
            gridRenderer.SetCellOccupied(pos.x, pos.y, true);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isLocked) return;

        isDragging = true;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.8f;

        // Sauvegarder la position initiale
        startPosition = rectTransform.anchoredPosition;
        startGridPosition = GridPosition;

        // Libérer les cellules occupées
        for (int i = 0; i < Size; i++)
        {
            Vector2Int pos = GridPosition + (IsVertical ? new Vector2Int(0, i) : new Vector2Int(i, 0));
            if (pos.x >= 0 && pos.x < 10 && pos.y >= 0 && pos.y < 10)
            {
                gridRenderer.SetCellOccupied(pos.x, pos.y, false);
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        // Convertir la position de la souris en position locale
        Vector2 mousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            gridRenderer.GetComponent<RectTransform>(), 
            eventData.position,
            eventData.pressEventCamera,
            out mousePos
        );

        // Déplacer le bateau en utilisant la souris comme point de référence gauche/haut
        Vector2 newPosition = mousePos;
        if (IsVertical)
        {
            newPosition.y -= (cellSize * (Size - 1)) / 2f;
        }
        else
        {
            newPosition.x += (cellSize * (Size - 1)) / 2f;
        }
        rectTransform.anchoredPosition = newPosition;

        // Obtenir la position de la grille
        Vector2Int newGridPos = gridRenderer.GetGridCoordinates(mousePos);
        
        // Vérifier si la position est valide
        bool isValid = IsValidPosition(newGridPos);
        canvasGroup.alpha = isValid ? 0.8f : 0.4f;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // Convertir la position de la souris en position locale
        Vector2 mousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            gridRenderer.GetComponent<RectTransform>(), 
            eventData.position,
            eventData.pressEventCamera,
            out mousePos
        );

        Vector2Int finalGridPos = gridRenderer.GetGridCoordinates(mousePos);

        if (IsValidPosition(finalGridPos))
        {
            // Mettre à jour la position du bateau sur la grille
            GridPosition = finalGridPos;

            // Calculer la position finale avec le décalage pour le centre du bateau
            Vector2 finalPosition = gridRenderer.GetCellPosition(finalGridPos.x, finalGridPos.y);
            if (IsVertical)
            {
                finalPosition.y -= (cellSize * (Size - 1)) / 2f;
            }
            else
            {
                finalPosition.x += (cellSize * (Size - 1)) / 2f;
            }
            rectTransform.anchoredPosition = finalPosition;

            // Marquer les cellules comme occupées
            for (int i = 0; i < Size; i++)
            {
                Vector2Int pos = GridPosition + (IsVertical ? new Vector2Int(0, i) : new Vector2Int(i, 0));
                gridRenderer.SetCellOccupied(pos.x, pos.y, true);
            }
        }
        else
        {
            // Remettre le bateau à sa position initiale
            ReturnToLastValidPosition();
        }
    }

    private void ReturnToLastValidPosition()
    {
        // Revenir à la dernière position valide
        GridPosition = startGridPosition;
        Vector2 finalPosition = gridRenderer.GetCellPosition(GridPosition.x, GridPosition.y);
        
        // Appliquer le même calcul de position que dans UpdatePosition
        if (IsVertical)
        {
            finalPosition.y += (cellSize * (Size - 1));
        }
        else
        {
            finalPosition.x += (cellSize * (Size - 1)) / 2f;
        }

        rectTransform.anchoredPosition = finalPosition;
        
        // Remettre les cellules occupées
        for (int i = 0; i < Size; i++)
        {
            Vector2Int pos = GridPosition + (IsVertical ? new Vector2Int(0, i) : new Vector2Int(i, 0));
            gridRenderer.SetCellOccupied(pos.x, pos.y, true);
        }
    }

    private bool IsValidPosition(Vector2Int position)
    {
        // Vérifier les limites de la grille
        for (int i = 0; i < Size; i++)
        {
            Vector2Int checkPos = position + (IsVertical ? new Vector2Int(0, i) : new Vector2Int(i, 0));
            
            // Vérifier si la position est dans la grille
            if (checkPos.x < 0 || checkPos.x >= gridRenderer.GridSize || 
                checkPos.y < 0 || checkPos.y >= gridRenderer.GridSize)
            {
                return false;
            }

            // Vérifier si la cellule est occupée (sauf par ce bateau)
            if (gridRenderer.IsCellOccupied(checkPos.x, checkPos.y))
            {
                // Si c'est une des cellules actuellement occupées par ce bateau, c'est ok
                bool isCurrentPosition = false;
                for (int j = 0; j < Size; j++)
                {
                    Vector2Int currentPos = GridPosition + (IsVertical ? new Vector2Int(0, j) : new Vector2Int(j, 0));
                    if (currentPos == checkPos)
                    {
                        isCurrentPosition = true;
                        break;
                    }
                }
                if (!isCurrentPosition)
                {
                    return false;
                }
            }
        }
        return true;
    }

    public bool IsPositionPartOfShip(Vector2Int position)
    {
        for (int i = 0; i < Size; i++)
        {
            Vector2Int shipPos = GridPosition + (IsVertical ? new Vector2Int(0, i) : new Vector2Int(i, 0));
            if (shipPos == position)
            {
                hitCells[i] = true; // Marquer la cellule comme touchée
                return true;
            }
        }
        return false;
    }

    public bool IsSunk()
    {
        // Le bateau est coulé si toutes ses cellules sont touchées
        for (int i = 0; i < Size; i++)
        {
            if (!hitCells[i]) return false;
        }
        return true;
    }

    public void SetVisible(bool visible)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
        }
    }

    public bool IsPositionOnShip(Vector2Int position)
    {
        Vector2Int shipPos = new Vector2Int(
            Mathf.RoundToInt(transform.position.x / gridRenderer.CellSize),
            Mathf.RoundToInt(transform.position.y / gridRenderer.CellSize)
        );

        if (IsVertical)
        {
            return position.y == shipPos.y && 
                   position.x >= shipPos.x && 
                   position.x < shipPos.x + Size;
        }
        else
        {
            return position.x == shipPos.x && 
                   position.y >= shipPos.y && 
                   position.y < shipPos.y + Size;
        }
    }

    public void TakeHit(Vector2Int position)
    {
        Vector2Int localPos = GetLocalPosition(position);
        if (localPos.x >= 0 && localPos.x < Size)
        {
            hitCells[localPos.x] = true;
        }
    }

    private Vector2Int GetLocalPosition(Vector2Int worldPos)
    {
        Vector2Int shipPos = new Vector2Int(
            Mathf.RoundToInt(transform.position.x / gridRenderer.CellSize),
            Mathf.RoundToInt(transform.position.y / gridRenderer.CellSize)
        );

        if (IsVertical)
        {
            return new Vector2Int(worldPos.x - shipPos.x, 0);
        }
        else
        {
            return new Vector2Int(worldPos.y - shipPos.y, 0);
        }
    }
}
