using UnityEngine;

public class MarkerRotate : MonoBehaviour
{
    public float rotateSpeed = 90f;
    public float pulseSpeed = 2f;    // 커졌다 작아지는 속도
    public float pulseAmount = 0.2f; // 크기 변화량
    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        // 회전
        transform.Rotate(0, 0, rotateSpeed * Time.unscaledDeltaTime);

        // 커졌다 작아졌다
        float scale = 1f + Mathf.Sin(Time.unscaledTime * pulseSpeed) * pulseAmount;
        transform.localScale = originalScale * scale;
    }


}
