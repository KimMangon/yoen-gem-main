using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DamageText : MonoBehaviour
{
    public Text DaText;
    public float moveSpeed = 1f;
    public float destroyTime = 0.5f;

    void Start()
    {
        // 일정 시간 후 삭제
        Destroy(gameObject, destroyTime);
    }

    void Update()
    {
        // 위로 이동
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;
    }

    public void Setup(int damage)
    {
        DaText.text = damage.ToString();
    }
}

