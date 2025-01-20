using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ShipPlacementManager : MonoBehaviour
{
    [SerializeField] private Canvas gameCanvas;
    [SerializeField] private GridRenderer gridRenderer;
    [SerializeField] private GameObject shipPrefab;
    [SerializeField] private GameObject chaloupPrefab;
    [SerializeField] private GameObject sloopPrefab;
    [SerializeField] private GameObject brigantinPrefab;
    [SerializeField] private Transform shipContainer;
    
    private readonly int[] shipSizes = { 4, 3, 3, 2 }; // Tailles standard des bateaux
    private List<Ship> ships = new List<Ship>();

    private void Start()
    {
        if (gridRenderer == null)
        {
            Debug.LogError("ShipPlacementManager: GridRenderer is missing!");
            return;
        }
        if (shipContainer == null)
        {
            Debug.LogError("ShipPlacementManager: ShipContainer is missing!");
            return;
        }
        if (chaloupPrefab == null || sloopPrefab == null || brigantinPrefab == null)
        {
            Debug.LogError("ShipPlacementManager: Ship prefabs are missing!");
            return;
        }

        PlaceShipsRandomly();
    }

    private void PlaceShipsRandomly()
    {
        int i = 0;
        foreach (int size in shipSizes)
        {
            Vector2Int position = new Vector2Int(0, 9-i);
            CreateShip(size, position, true);
            i++;
        }
    }

    private void CreateShip(int length, Vector2Int startPosition, bool isHorizontal)
    {
        GameObject shipObject = null;

        // Sélectionner le bon prefab en fonction de la taille
        if (length == 4)
        {
            shipObject = Instantiate(brigantinPrefab, shipContainer);
        }
        else if (length == 3)
        {
            shipObject = Instantiate(sloopPrefab, shipContainer);
        }
        else if (length == 2)
        {
            shipObject = Instantiate(chaloupPrefab, shipContainer);
        }

        if (shipObject == null)
        {
            Debug.LogError($"ShipPlacementManager: Failed to create ship of length {length}");
            return;
        }

        // S'assurer que tous les composants UI nécessaires sont présents
        RectTransform shipRect = shipObject.GetComponent<RectTransform>();
        if (shipRect == null)
        {
            shipRect = shipObject.AddComponent<RectTransform>();
        }

        Image shipImage = shipObject.GetComponent<Image>();
        if (shipImage == null)
        {
            shipImage = shipObject.AddComponent<Image>();
        }

        CanvasGroup canvasGroup = shipObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = shipObject.AddComponent<CanvasGroup>();
        }

        // Configurer le RectTransform du bateau
        shipRect.anchorMin = new Vector2(0.5f, 0.5f);
        shipRect.anchorMax = new Vector2(0.5f, 0.5f);
        shipRect.pivot = new Vector2(0.5f, 0.5f);

        // Calculer la taille du bateau en fonction de la taille des cellules
        float cellSize = gridRenderer.CellSize;
        float margin = 4f; // marge en pixels
        if (isHorizontal)
        {
            shipRect.sizeDelta = new Vector2(cellSize * length - margin, cellSize - margin);
        }
        else
        {
            shipRect.sizeDelta = new Vector2(cellSize - margin, cellSize * length - margin);
        }

        // Positionner le bateau sur la grille
        Vector2 startPos = gridRenderer.GetCellPosition(startPosition.x, startPosition.y);
        if (isHorizontal)
        {
            startPos.x += (cellSize * (length - 1)) / 2f;
        }
        else
        {
            startPos.y -= (cellSize * (length - 1)) / 2f;
        }
        
        shipRect.anchoredPosition = startPos;

        // Configurer l'image du bateau
        if (shipImage != null)
        {
            shipImage.raycastTarget = true;
            shipImage.maskable = true;
            shipImage.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            
            if (shipImage.sprite == null)
            {
                shipImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
            }
        }

        // Configurer le CanvasGroup
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        Ship ship = shipObject.GetComponent<Ship>();
        if (ship == null)
        {
            ship = shipObject.AddComponent<Ship>();
        }

        ship.Initialize(length, startPosition);
        ships.Add(ship);

        // Marquer les cellules comme occupées
        for (int i = 0; i < length; i++)
        {
            Vector2Int pos = startPosition + (isHorizontal ? new Vector2Int(i, 0) : new Vector2Int(0, i));
            gridRenderer.SetCellOccupied(pos.x, pos.y, true);
        }
    }
}
