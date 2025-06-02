using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ChainSelectAttack : MonoBehaviour
{
    public float maxLength = 10f;
    public float enemyHoverMaxDistance = 1.5f;
    public LayerMask enemyLayerMask;

    private LineRenderer lineRenderer;
    private Camera mainCamera;
    private Transform player;
    private Transform lockedEnemy;


    void OnEnable()
    {
        lineRenderer = GetComponent<LineRenderer>();

        //make line thinner
        lineRenderer.startWidth = 0.05f;

        mainCamera = Camera.main;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("ChainSelectAttack: No GameObject with tag 'Player' found in scene.");
        }

        lockedEnemy = null;
    }

    void Update()
    {
        if (mainCamera == null || player == null) return;

        Vector3 playerPosition = player.position;
        Vector3 mouseWorldPosition = GetMouseWorldPosition();

        Vector3 direction = (mouseWorldPosition - playerPosition).normalized;
        float distance = Mathf.Min(Vector3.Distance(playerPosition, mouseWorldPosition), maxLength);
        Vector3 clampedMousePosition = playerPosition + direction * distance;

        TryLockOrUnlockEnemy(clampedMousePosition);

        Vector3 endPoint = lockedEnemy ? GetEnemyHoverPosition(lockedEnemy) : clampedMousePosition;

        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, playerPosition);
        lineRenderer.SetPosition(1, endPoint);
    }

    private void TryLockOrUnlockEnemy(Vector3 clampedMousePos)
    {
        Vector3 worldMousePos = GetMouseWorldPosition();
        Vector2 mousePos2D = new Vector2(worldMousePos.x, worldMousePos.y);

        // Try to hit an enemy directly under the mouse using Physics2D
        RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero, 0f, enemyLayerMask);

        if (hit.collider != null && hit.collider.CompareTag("Enemy"))
        {
            // Debug.Log($"Enemy {hit.collider.name} hovered at {hit.point}");
            // Debug.Break();
            lockedEnemy = hit.collider.transform;
            return;
        }

        // If already locked, check if mouse moved too far from enemy screen position
        if (lockedEnemy != null)
        {
            Vector3 enemyScreenPos = mainCamera.WorldToScreenPoint(lockedEnemy.position);
            float distFromEnemyScreen = Vector2.Distance(Input.mousePosition, enemyScreenPos);

            if (distFromEnemyScreen > enemyHoverMaxDistance)
            {
                lockedEnemy = null;
            }
        }
    }



    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, mainCamera.nearClipPlane));

        if (worldPosition.z < 0)
        {
            worldPosition.z = 0;
        }

        return worldPosition;
    }

    private Vector3 GetEnemyHoverPosition(Transform enemy)
    {
        return enemy.position;
    }
}
