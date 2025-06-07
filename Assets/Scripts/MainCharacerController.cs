using UnityEngine;

public class MainCharacerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    private Vector2 _movementInput;
    void Update()
    {
        _movementInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;

        transform.position += moveSpeed * Time.deltaTime * (Vector3)_movementInput;
    }
}
