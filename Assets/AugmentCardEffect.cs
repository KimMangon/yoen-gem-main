using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AugmentCardEffect : MonoBehaviour
{
    public Image cardImage;       // 카드 이미지 (인스펙터에서 연결)
    
    public float duration = 0.5f;  // 연출 총 시간

    public void PlaySelectEffect()
    {
        StartCoroutine(SelectEffect());
    }

    public IEnumerator SelectEffect()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 bigScale = originalScale * 1.3f; // 1.3배로 커짐
        Color startColor = cardImage.color;
        float elapsed = 0f;

        // 커지면서 빛남
        while (elapsed < duration / 2)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration / 2);
            transform.localScale = Vector3.Lerp(originalScale, bigScale, t);
            cardImage.color = Color.Lerp(startColor, Color.white, t);
            yield return null;
        }

        // 투명해지며 원래 크기로
        elapsed = 0f;
        while (elapsed < duration / 2)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration / 2);
            transform.localScale = Vector3.Lerp(bigScale, originalScale, t);
            cardImage.color = Color.Lerp(Color.white, new Color(1, 1, 1, 0), t);
            yield return null;
        }

        // 원래 상태로 복구
        transform.localScale = originalScale;
        cardImage.color = startColor;
    }
}