using UnityEngine;
using System.Collections.Generic;

public class AIPlayer
{
    private AIDifficulty difficulty;
    private int gridSize;
    private bool[,] shotGrid; // true si la cellule a déjà été ciblée
    private List<Vector2Int> successfulHits; // Liste des cellules touchées
    private List<Vector2Int> potentialTargets; // Pour le mode moyen, cellules à cibler en priorité

    private static readonly Vector2Int[] adjacentDirections = new Vector2Int[]
    {
        new Vector2Int(1, 0),  // droite
        new Vector2Int(-1, 0), // gauche
        new Vector2Int(0, 1),  // haut
        new Vector2Int(0, -1)  // bas
    };

    public void Initialize(AIDifficulty difficulty, int gridSize)
    {
        this.difficulty = difficulty;
        this.gridSize = gridSize;
        shotGrid = new bool[gridSize, gridSize];
        successfulHits = new List<Vector2Int>();
        potentialTargets = new List<Vector2Int>();
    }

    public Vector2Int GetNextTarget()
    {
        if (difficulty == AIDifficulty.Easy || (potentialTargets.Count == 0 && successfulHits.Count == 0))
        {
            return GetRandomTarget();
        }
        else
        {
            return GetSmartTarget();
        }
    }

    private Vector2Int GetRandomTarget()
    {
        List<Vector2Int> availableCells = new List<Vector2Int>();
        
        // Trouver toutes les cellules non ciblées
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                if (!shotGrid[x, y])
                {
                    availableCells.Add(new Vector2Int(x, y));
                }
            }
        }

        if (availableCells.Count == 0) return Vector2Int.zero;

        // Choisir une cellule au hasard
        int randomIndex = Random.Range(0, availableCells.Count);
        return availableCells[randomIndex];
    }

    private Vector2Int GetSmartTarget()
    {
        // S'il y a des cibles potentielles, en choisir une
        if (potentialTargets.Count > 0)
        {
            int index = Random.Range(0, potentialTargets.Count);
            Vector2Int target = potentialTargets[index];
            potentialTargets.RemoveAt(index);
            return target;
        }

        // S'il y a des touches réussies, chercher autour
        if (successfulHits.Count > 0)
        {
            Vector2Int lastHit = successfulHits[successfulHits.Count - 1];
            
            // Vérifier les cellules adjacentes
            foreach (Vector2Int dir in adjacentDirections)
            {
                Vector2Int adjacent = lastHit + dir;
                
                // Vérifier si la cellule est valide et n'a pas été ciblée
                if (IsValidCell(adjacent) && !shotGrid[adjacent.x, adjacent.y])
                {
                    potentialTargets.Add(adjacent);
                }
            }

            if (potentialTargets.Count > 0)
            {
                int index = Random.Range(0, potentialTargets.Count);
                Vector2Int target = potentialTargets[index];
                potentialTargets.RemoveAt(index);
                return target;
            }
        }

        // Si aucune cible intelligente n'est trouvée, revenir au tir aléatoire
        return GetRandomTarget();
    }

    public void ProcessResult(Vector2Int target, bool isHit)
    {
        // Marquer la cellule comme ciblée
        shotGrid[target.x, target.y] = true;

        if (isHit)
        {
            successfulHits.Add(target);
            
            // En mode moyen, ajouter les cellules adjacentes comme cibles potentielles
            if (difficulty == AIDifficulty.Medium)
            {
                foreach (Vector2Int dir in adjacentDirections)
                {
                    Vector2Int adjacent = target + dir;
                    if (IsValidCell(adjacent) && !shotGrid[adjacent.x, adjacent.y] 
                        && !potentialTargets.Contains(adjacent))
                    {
                        potentialTargets.Add(adjacent);
                    }
                }
            }
        }
    }

    private bool IsValidCell(Vector2Int cell)
    {
        return cell.x >= 0 && cell.x < gridSize && cell.y >= 0 && cell.y < gridSize;
    }
}
