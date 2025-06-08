using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(LineRenderer))]
public class SwipeChainSelectAttack : MonoBehaviour
{
    [field: SerializeField] public float TileSize = 1f;
    [field: SerializeField] public int MaxChainLength = 5;
    [field: SerializeField] public LayerMask EnemyLayerMask { get; private set; }

    public event Action<List<Vector2Int>> OnChainConfirmed;

    private LineRenderer _lineRenderer;
    private Camera _mainCamera;
    private Transform _player;

    private bool _isSwiping = false;
    private Vector2Int _currentGridPos;
    private List<Vector2Int> _swipeChain = new();
    private HashSet<Vector2Int> _visitedPositions = new();
    private List<Transform> _lockedEnemies = new();

    void OnEnable()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.startWidth = 0.05f;
        _lineRenderer.endWidth = 0.05f;

        _mainCamera = Camera.main;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("SwipeChainSelectAttack: No GameObject with tag 'Player' found.");
        }
    }

    void Update()
    {
        if (_mainCamera == null || _player == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            BeginSwipe();
        }
        else if (Input.GetMouseButton(0) && _isSwiping)
        {
            UpdateSwipe();
        }
        else if (Input.GetMouseButtonUp(0) && _isSwiping)
        {
            EndSwipe();
        }

        DrawLine();
    }

    void BeginSwipe()
    {
        _isSwiping = true;
        _swipeChain.Clear();
        _visitedPositions.Clear();
        _lockedEnemies.Clear();

        _currentGridPos = WorldToGrid(_player.position);
        _swipeChain.Add(_currentGridPos);
        _visitedPositions.Add(_currentGridPos);
    }

    void UpdateSwipe()
    {
        Vector2 mouseWorld = GetMouseWorldPosition();
        Vector2Int newGridPos = WorldToGrid(mouseWorld);

        if (newGridPos == _currentGridPos || _swipeChain.Count == 0)
            return;

        Vector2Int dir = newGridPos - _currentGridPos;

        bool shiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (shiftHeld)
        {
            // Only allow diagonal (ordinal) steps
            if (!IsOrdinal(dir))
                return;
        }
        else
        {
            // Only allow cardinal steps
            if (!IsCardinal(dir))
                return;
        }

        // Undo step if going back to previous cell
        if (_swipeChain.Count >= 2 && newGridPos == _swipeChain[_swipeChain.Count - 2])
        {
            _visitedPositions.Remove(_currentGridPos);
            _swipeChain.RemoveAt(_swipeChain.Count - 1);
            _currentGridPos = _swipeChain[_swipeChain.Count - 1];

            if (_lockedEnemies.Count > 0)
            {
                Transform lastEnemy = _lockedEnemies[_lockedEnemies.Count - 1];
                if (WorldToGrid(lastEnemy.position) == newGridPos)
                    _lockedEnemies.RemoveAt(_lockedEnemies.Count - 1);
            }

            return;
        }

        if (_visitedPositions.Contains(newGridPos) || _swipeChain.Count >= MaxChainLength)
            return;

        _swipeChain.Add(newGridPos);
        _visitedPositions.Add(newGridPos);
        _currentGridPos = newGridPos;

        Collider2D hit = Physics2D.OverlapPoint(GridToWorld(newGridPos), EnemyLayerMask);
        if (hit != null && hit.CompareTag("Enemy"))
        {
            _lockedEnemies.Add(hit.transform);
        }
    }

    void EndSwipe()
    {
        _isSwiping = false;
        Debug.Log($"Swipe complete. Chained {_lockedEnemies.Count} enemies.");

        if (_swipeChain.Count > 0)
        {
            OnChainConfirmed?.Invoke(new List<Vector2Int>(_swipeChain));
        }
    }

    void DrawLine()
    {
        Vector3[] linePoints = new Vector3[_swipeChain.Count];
        for (int i = 0; i < _swipeChain.Count; i++)
        {
            linePoints[i] = GridToWorld(_swipeChain[i]);
        }
        _lineRenderer.positionCount = linePoints.Length;
        _lineRenderer.SetPositions(linePoints);
    }

    Vector2Int WorldToGrid(Vector3 worldPos)
    {
        return new Vector2Int(
            Mathf.RoundToInt(worldPos.x / TileSize),
            Mathf.RoundToInt(worldPos.y / TileSize)
        );
    }

    Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x * TileSize, gridPos.y * TileSize, 0f);
    }

    bool IsCardinal(Vector2Int dir)
    {
        return dir == Vector2Int.up || dir == Vector2Int.down ||
                dir == Vector2Int.left || dir == Vector2Int.right;
    }

    bool IsOrdinal(Vector2Int dir)
    {
        return Mathf.Abs(dir.x) == 1 && Mathf.Abs(dir.y) == 1;
    }

    Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreen = Input.mousePosition;
        Vector3 worldPos = _mainCamera.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, -_mainCamera.transform.position.z));
        return new Vector3(worldPos.x, worldPos.y, 0);
    }
}
