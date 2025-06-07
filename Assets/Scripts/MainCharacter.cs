using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCharacter : MonoBehaviour
{
    private SwipeChainSelectAttack _swipeChainSelectAttack;
    private bool _isMoving = false;
    private float _moveSpeed = 5f;
    void Awake()
    {
        _swipeChainSelectAttack = FindFirstObjectByType<SwipeChainSelectAttack>();

        _swipeChainSelectAttack.OnChainConfirmed += ExecuteChain;
    }


    public void ExecuteChain(List<Vector2Int> gridPath)
    {
        if (_isMoving) return;
        StartCoroutine(DashThroughGridPath(gridPath));
    }


    [SerializeField] private float baseMoveTime = 0.1f; // base time for orthogonal move
    [SerializeField] private float diagonalMultiplier = 1.5f;
    [SerializeField] private float gridCellSize = 1f; // size of one grid cell
    [SerializeField] private Vector3 gridOrigin = Vector3.zero; // where grid starts



    private IEnumerator DashThroughGridPath(List<Vector2Int> path)
    {
        _isMoving = true;

        Vector2Int currentGrid = WorldToGrid(transform.position);

        foreach (var nextGrid in path)
        {
            Vector3 nextWorld = GridToWorld(nextGrid);

            float moveTime = baseMoveTime;

            // Check if diagonal
            Vector2Int delta = nextGrid - currentGrid;
            if (Mathf.Abs(delta.x) == 1 && Mathf.Abs(delta.y) == 1)
            {
                moveTime *= diagonalMultiplier;
            }

            yield return MoveTo(nextWorld, moveTime);

            // Attack anything at nextGrid
            TryAttackAt(nextGrid);

            currentGrid = nextGrid;
        }

        _isMoving = false;
    }

    private IEnumerator MoveTo(Vector3 destination, float time)
    {
        Vector3 start = transform.position;
        float elapsed = 0f;

        while (elapsed < time)
        {
            transform.position = Vector3.Lerp(start, destination, elapsed / time);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = destination;
    }

    private Vector2Int WorldToGrid(Vector3 worldPos)
    {
        Vector3 local = worldPos - gridOrigin;
        return new Vector2Int(
            Mathf.RoundToInt(local.x / gridCellSize),
            Mathf.RoundToInt(local.y / gridCellSize)
        );
    }

    private Vector3 GridToWorld(Vector2Int gridPos)
    {
        return gridOrigin + new Vector3(gridPos.x * gridCellSize, gridPos.y * gridCellSize, 0);
    }


    private void TryAttackAt(Vector2Int gridPos)
    {
        Vector3 worldPos = GridToWorld(gridPos);
        LayerMask enemyLayer = LayerMask.GetMask("Enemy");
        Collider2D hit = Physics2D.OverlapCircle(worldPos, 0.1f, enemyLayer);

        if (hit != null && hit.CompareTag("Enemy"))
        {
            Debug.Log($"Attacked {hit.name} at {gridPos}");
            Destroy(hit.gameObject);
        }
    }


}
