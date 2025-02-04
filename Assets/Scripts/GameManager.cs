using UnityEngine;
using UnityEngine.Events;

public enum GameState
{
    PlacingShips,
    PlayerTurn,
    AITurn,
    GameOver
}

public enum AIDifficulty
{
    Easy,
    Medium
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("References")]
    [SerializeField] private ShipPlacementManager playerPlacementManager;
    [SerializeField] private ShipPlacementManager enemyPlacementManager;
    [SerializeField] private GameObject enemyGridObject;
    [SerializeField] private UIController uiController;

    [Header("Game Events")]
    public UnityEvent<Vector2Int> onCellHit = new UnityEvent<Vector2Int>();
    public UnityEvent<Vector2Int> onCellMiss = new UnityEvent<Vector2Int>();
    public UnityEvent<bool> onGameOver = new UnityEvent<bool>(); // true si le joueur gagne

    private GameState currentState = GameState.PlacingShips;
    private AIPlayer aiPlayer;
    private bool isGameStarted = false;
    

    private void Awake()
    {
        instance = this;
        aiPlayer = new AIPlayer();
        Debug.Log("GameManager Awake - Instance set");
    }

    private void Start()
    {
        // Vérifier que toutes les références sont configurées
        if (playerPlacementManager == null) Debug.LogError("GameManager: playerPlacementManager not set!");
        if (enemyPlacementManager == null) Debug.LogError("GameManager: enemyPlacementManager not set!");
        if (enemyGridObject == null) Debug.LogError("GameManager: enemyGridObject not set!");
        if (uiController == null) Debug.LogError("GameManager: uiController not set!");

        // Configurer les événements de clic sur la grille ennemie
        if (enemyPlacementManager.gridRenderer != null)
        {
            enemyPlacementManager.gridRenderer.CellClicked.AddListener(OnEnemyGridCellClicked);
        }
    }

    public void StartGame(AIDifficulty difficulty = AIDifficulty.Easy)
    {
        Debug.Log("StartGame called");
        if (isGameStarted)
        {
            Debug.Log("Game already started, ignoring StartGame call");
            return;
        }
        
        isGameStarted = true;
        Debug.Log("Starting game...");

        // Initialiser l'IA avec la difficulté choisie
        aiPlayer.Initialize(difficulty, playerPlacementManager.gridRenderer.GridSize);

        // Activer la grille ennemie
        if (enemyGridObject != null)
        {
            Debug.Log("Activating enemy grid");
            enemyGridObject.SetActive(true);
        }

        // Verrouiller les bateaux du joueur
        if (playerPlacementManager != null)
        {
            Debug.Log("Locking player ships");
            playerPlacementManager.LockShips();
        }

        // Initialiser la grille ennemie
        if (enemyPlacementManager != null)
        {
            Debug.Log("Setting up enemy grid");
            enemyPlacementManager.Initialize(true);
            enemyPlacementManager.PlaceShipsRandomly();
            enemyPlacementManager.HideShips();
        }

        // Démarrer le tour du joueur
        currentState = GameState.PlayerTurn;
        uiController.UpdateTurnDisplay(true);
    }

    private void OnEnemyGridCellClicked(Vector2Int cellPos)
    {
        if (currentState != GameState.PlayerTurn || !isGameStarted) return;

        var grid = enemyPlacementManager.gridRenderer;
        
        // Vérifier si la cellule a déjà été touchée
        if (grid.IsCellHit(cellPos.x, cellPos.y)) return;

        // Marquer la cellule comme touchée
        bool isHit = grid.IsCellOccupied(cellPos.x, cellPos.y);
        grid.SetCellHit(cellPos.x, cellPos.y, isHit);

        if (isHit)
        {
            onCellHit.Invoke(cellPos);
            enemyPlacementManager.RevealShip(cellPos);

            // Vérifier si tous les bateaux ennemis sont coulés
            if (enemyPlacementManager.AreAllShipsSunk())
            {
                GameOver(true);
                return;
            }
        }
        else
        {
            onCellMiss.Invoke(cellPos);
        }

        // Passer au tour de l'IA
        currentState = GameState.AITurn;
        uiController.UpdateTurnDisplay(false);
        
        // Laisser un petit délai avant le tour de l'IA
        Invoke("AITurn", 1f);
    }

    private void AITurn()
    {
        if (!isGameStarted || currentState != GameState.AITurn) return;

        var grid = playerPlacementManager.gridRenderer;

        // Obtenir la cible de l'IA
        Vector2Int target = aiPlayer.GetNextTarget();
        bool isHit = grid.IsCellOccupied(target.x, target.y);

        // Marquer la cellule comme touchée
        grid.SetCellHit(target.x, target.y, isHit);

        // Informer l'IA du résultat
        aiPlayer.ProcessResult(target, isHit);

        if (isHit)
        {
            onCellHit.Invoke(target);
            
            // Vérifier si tous les bateaux du joueur sont coulés
            if (playerPlacementManager.AreAllShipsSunk())
            {
                GameOver(false);
                return;
            }
        }
        else
        {
            onCellMiss.Invoke(target);
        }

        // Retour au tour du joueur
        currentState = GameState.PlayerTurn;
        uiController.UpdateTurnDisplay(true);
    }

    private void GameOver(bool playerWins)
    {
        isGameStarted = false;
        currentState = GameState.GameOver;
        onGameOver.Invoke(playerWins);
        
        // Révéler tous les bateaux ennemis
        enemyPlacementManager.RevealAllShips();

        // Afficher l'écran de fin de partie
        if (uiController != null)
        {
            uiController.ShowGameOver(playerWins);
        }
    }
}
