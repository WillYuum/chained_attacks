using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteSheetAnimator : MonoBehaviour
{
    public Sprite[] frames;               // Array of sprite frames
    public float frameRate = 10f;         // Frames per second

    private Sprite defaultSprite; // Optional default sprite to show when not animating

    private SpriteRenderer spriteRenderer;
    private int currentFrame;
    private float timer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (frames == null || frames.Length == 0) return;

        timer += Time.deltaTime;
        if (timer >= 1f / frameRate)
        {
            timer -= 1f / frameRate;
            currentFrame = (currentFrame + 1) % frames.Length;
            spriteRenderer.sprite = frames[currentFrame];
        }
    }

    public void ToggleAnimation(bool play)
    {
        if (play)
        {
            currentFrame = 0;
            timer = 0f;
            spriteRenderer.sprite = frames[currentFrame];
        }
        else
        {
            spriteRenderer.sprite = defaultSprite;
        }
    }
}
