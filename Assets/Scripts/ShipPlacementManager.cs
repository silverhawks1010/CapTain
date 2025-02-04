using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ShipPlacementManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Canvas gameCanvas;
    public GridRenderer gridRenderer; // Rendu public pour être accessible depuis GameManager
    [SerializeField] private Transform shipContainer;

    [Header("Ship Prefabs")]
    [SerializeField] private GameObject shipPrefab;      // 5 cases
    [SerializeField] private GameObject brigantinPrefab; // 4 cases
    [SerializeField] private GameObject sloopPrefab;     // 3 cases
    [SerializeField] private GameObject chaloupPrefab;   // 2 cases

    private readonly int[] shipSizes = { 5, 4, 3, 3, 2 }; // Tailles standard des bateaux
    private List<Ship> ships = new List<Ship>();
    private bool isEnemyGrid = false;
    private bool isLocked = false;

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

        // Positionner la grille et le ship container
        RectTransform gridRect = gridRenderer.GetComponent<RectTransform>();
        RectTransform containerRect = shipContainer.GetComponent<RectTransform>();
        
        if (gridRect != null && containerRect != null)
        {
            float xOffset = isEnemyGrid ? 120f : -120f;
            gridRect.anchoredPosition = new Vector2(xOffset, gridRect.anchoredPosition.y);
            containerRect.anchoredPosition = new Vector2(xOffset, containerRect.anchoredPosition.y);
        }

        // Si ce n'est pas la grille ennemie, placer les bateaux initiaux
        if (!isEnemyGrid)
        {
            PlaceInitialShips();
        }
    }

    private void PlaceInitialShips()
    {
        // Placer les bateaux initiaux pour le joueur (toujours horizontaux)
        CreateShip(5, new Vector2Int(0, 0), true); // Horizontal
        CreateShip(4, new Vector2Int(0, 1), true); // Horizontal
        CreateShip(3, new Vector2Int(0, 2), true); // Horizontal
        CreateShip(3, new Vector2Int(0, 3), true); // Horizontal
        CreateShip(2, new Vector2Int(0, 4), true); // Horizontal
    }

    public void Initialize(bool isEnemy)
    {
        this.isEnemyGrid = isEnemy;
        if (isEnemy)
        {
            PlaceShipsRandomly();
        }
    }

    public void PlaceShipsRandomly()
    {
        // Nettoyer les bateaux existants et la grille
        foreach (Ship ship in ships)
        {
            Destroy(ship.gameObject);
        }
        ships.Clear();

        // Réinitialiser toutes les cellules
        for (int x = 0; x < gridRenderer.GridSize; x++)
        {
            for (int y = 0; y < gridRenderer.GridSize; y++)
            {
                gridRenderer.SetCellOccupied(x, y, false);
            }
        }

        // Liste des tailles de bateaux à placer (1 de chaque taille)
        List<int> shipSizes = new List<int> { 5, 4, 3, 3, 2 };

        // Pour chaque bateau
        foreach (int size in shipSizes)
        {
            bool placed = false;
            int maxAttempts = 100; // Éviter une boucle infinie
            int attempts = 0;

            while (!placed && attempts < maxAttempts)
            {
                attempts++;

                // Choisir une orientation aléatoire
                bool isHorizontal = Random.Range(0, 2) == 0;

                // Calculer les limites en fonction de l'orientation
                int maxX = isHorizontal ? gridRenderer.GridSize - size : gridRenderer.GridSize - 1;
                int maxY = isHorizontal ? gridRenderer.GridSize - 1 : gridRenderer.GridSize - size;

                // Choisir une position aléatoire
                int x = Random.Range(0, maxX + 1);
                int y = Random.Range(0, maxY + 1);
                Vector2Int position = new Vector2Int(x, y);

                // Vérifier si la position est valide
                bool isValid = true;

                // Vérifier chaque cellule que le bateau occuperait
                for (int i = 0; i < size; i++)
                {
                    Vector2Int checkPos = position + (isHorizontal ? new Vector2Int(i, 0) : new Vector2Int(0, i));
                    
                    // Vérifier la cellule elle-même
                    if (gridRenderer.IsCellOccupied(checkPos.x, checkPos.y))
                    {
                        isValid = false;
                        break;
                    }
                }

                // Si la position est valide, placer le bateau
                if (isValid)
                {
                    CreateShip(size, position, isHorizontal);
                    placed = true;
                }
            }

            if (!placed)
            {
                Debug.LogError($"Failed to place ship of size {size} after {maxAttempts} attempts");
            }
        }
    }

    public void LockShips()
    {
        isLocked = true;
        foreach (var ship in ships)
        {
            if (ship != null)
            {
                // Désactiver le drag & drop et la rotation
                Destroy(ship.GetComponent<CanvasGroup>());
                ship.enabled = false;
            }
        }
    }

    public void HideShips()
    {
        foreach (Ship ship in ships)
        {
            ship.GetComponent<CanvasGroup>().alpha = 0f;
        }
    }

    public void RevealShip(Vector2Int position)
    {
        foreach (var ship in ships)
        {
            if (ship.IsPositionOnShip(position))
            {
                ship.SetVisible(true);
                break;
            }
        }
    }

    public void RevealAllShips()
    {
        foreach (var ship in ships)
        {
            ship.SetVisible(true);
        }
    }

    public bool AreAllShipsSunk()
    {
        foreach (var ship in ships)
        {
            if (!ship.IsSunk())
            {
                return false;
            }
        }
        return true;
    }

    public void CreateShip(int length, Vector2Int startPosition, bool isHorizontal)
    {
        GameObject shipObject = null;

        // Sélectionner le bon prefab en fonction de la taille
        if (length == 5)
        {
            shipObject = Instantiate(shipPrefab, shipContainer);
        }
        else if (length == 4)
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

        // Laisser Ship gérer la taille, la position et l'occupation des cellules
        ship.Initialize(length, startPosition, isHorizontal, gridRenderer);
        ships.Add(ship);
    }
}
