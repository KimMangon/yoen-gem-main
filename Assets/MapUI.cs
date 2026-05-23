using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapUI : MonoBehaviour
{
    [Header("노드 설정")]
    public GameObject nodeButtonPrefab;  // 노드 버튼 프리팹
    public RectTransform content;        // Scroll View의 Content

    [Header("노드 타입별 스프라이트")]
    public Sprite combatSprite;
    public Sprite shopSprite;
    public Sprite restSprite;
    public Sprite bossSprite;
    public Sprite startSprite;
    public Sprite mysterySprite;
    public Sprite treasureSprite;
    // 추후 추가 가능

    [Header("마커")]
    public GameObject currentNodeMarker; // 현재 노드 마커 프리팹
    private GameObject markerInstance;   // 생성된 마커 오브젝트

    [Header("노드 간격")]
    public float nodeSpacingX = 100f;   // 가로 간격
    public float nodeSpacingY = 100f;   // 세로 간격

    [Header("점선 설정")]
    public GameObject dotPrefab;         // 점선용 점 프리팹
    public float dotSpacing = 15f;       // 점 간격

    private MapGenerator mapGenerator;
    private List<GameObject> nodeObjects = new List<GameObject>();
    private List<GameObject> dotObjects = new List<GameObject>();



    // 노드와 버튼 오브젝트 매핑
    private Dictionary<MapNode, GameObject> nodeButtonMap = new Dictionary<MapNode, GameObject>();

    void Start()
    {
        
    }

    public void GenerateAndDisplay()
    {
        
        if (mapGenerator == null)
            mapGenerator = GetComponent<MapGenerator>();

        foreach (var obj in nodeObjects) Destroy(obj);
        foreach (var obj in dotObjects) Destroy(obj);
        nodeObjects.Clear();
        dotObjects.Clear();
        nodeButtonMap.Clear();

        // 맵 생성
        List<MapNode> allNodes = mapGenerator.GenerateMap();

        // Content 크기 설정
        float totalWidth = mapGenerator.width * nodeSpacingX;
        float totalHeight = mapGenerator.height * nodeSpacingY;

        content.sizeDelta = new Vector2(totalWidth, totalHeight);

        //점선 생성
        foreach (var node in allNodes)
            foreach (var nextNode in node.nextNodes)
                DrawDottedLine(node, nextNode);

        //노드 버튼 나중에 생성 (점선 위에 올라오도록)
        foreach (var node in allNodes)
        {
            GameObject btnObj = Instantiate(nodeButtonPrefab, content);
            RectTransform rect = btnObj.GetComponent<RectTransform>();

            // 노드 위치 설정
            rect.anchoredPosition = new Vector2(
                (node.x - (mapGenerator.width - 1) * 0.5f) * nodeSpacingX,
                node.y * nodeSpacingY - totalHeight * 0.5f + nodeSpacingY * 0.5f
            );

            // 노드 이미지 설정
            Image img = btnObj.GetComponentInChildren<Image>();
            img.sprite = GetSprite(node.nodeType);

            // 잠긴 노드는 어둡게
            img.color = node.isAccessible ? Color.white : new Color(0.4f, 0.4f, 0.4f);

            // 버튼 클릭 이벤트
            Button btn = btnObj.GetComponent<Button>();
            MapNode capturedNode = node;
            btn.onClick.AddListener(() => OnNodeClicked(capturedNode));
            btn.interactable = node.isAccessible;

            AddButtonAnimation(btnObj);

            nodeObjects.Add(btnObj);
            nodeButtonMap[node] = btnObj;

            nodeObjects.Add(btnObj);
            nodeButtonMap[node] = btnObj;
        }

        UpdateCurrentNodeMarker();

    }

    // 노드 클릭했을 때
    void OnNodeClicked(MapNode node)
    {
        if (!node.isAccessible) return;

        // GameManager에 선택한 노드 전달
        GameManager.Instance.EnterNode(node);
    }

    public List<MapNode> GetAllNodes()
    {
        return new List<MapNode>(nodeButtonMap.Keys);
    }

    // 점선 그리기
    void DrawDottedLine(MapNode from, MapNode to)
    {
        // [변경] 노드 좌표와 동일한 중앙 기준 계산으로 통일
        float totalWidth = mapGenerator.width * nodeSpacingX;
        float totalHeight = mapGenerator.height * nodeSpacingY;

        Vector2 startPos = new Vector2(
             (from.x - (mapGenerator.width - 1) * 0.5f) * nodeSpacingX,
             from.y * nodeSpacingY - totalHeight * 0.5f + nodeSpacingY * 0.5f
         );
        Vector2 endPos = new Vector2(
            (to.x - (mapGenerator.width - 1) * 0.5f) * nodeSpacingX,
            to.y * nodeSpacingY - totalHeight * 0.5f + nodeSpacingY * 0.5f
        );

        float distance = Vector2.Distance(startPos, endPos);
        int dotCount = Mathf.FloorToInt(distance / dotSpacing);
        float angle = Mathf.Atan2(endPos.y - startPos.y, endPos.x - startPos.x) * Mathf.Rad2Deg;

        for (int i = 1; i < dotCount; i++)
        {
            float t = (float)i / dotCount;
            Vector2 pos = Vector2.Lerp(startPos, endPos, t);

            GameObject dot = Instantiate(dotPrefab, content);
            RectTransform dotRect = dot.GetComponent<RectTransform>();
            dotRect.anchoredPosition = pos;
            dotRect.rotation = Quaternion.Euler(0, 0, angle);

            dotObjects.Add(dot);
        }
    }

    // 타입별 스프라이트 반환
    Sprite GetSprite(MapNode.NodeType type)
    {
        switch (type)
        {
            case MapNode.NodeType.Combat: return combatSprite;
            case MapNode.NodeType.Shop: return shopSprite;
            case MapNode.NodeType.Rest: return restSprite;
            case MapNode.NodeType.Boss: return bossSprite;
            case MapNode.NodeType.Start: return startSprite;
            case MapNode.NodeType.Mystery: return mysterySprite;
            case MapNode.NodeType.Treasure: return treasureSprite;
            default: return combatSprite;
        }
    }

    // 노드 클리어 후 UI 업데이트
    public void RefreshMap()
    {
        foreach (var pair in nodeButtonMap)
        {
            MapNode node = pair.Key;
            GameObject btnObj = pair.Value;

            Image img = btnObj.GetComponentInChildren<Image>();
            img.color = node.isAccessible ? Color.white : new Color(0.4f, 0.4f, 0.4f);

            Button btn = btnObj.GetComponent<Button>();
            btn.interactable = node.isAccessible 
                && !node.isCleared && node != GameManager.Instance.currentNode;

            // [추가] 새로 활성화된 노드에 애니메이션 추가
            if (node.isAccessible && !node.isCleared)
            {
                if (btnObj.GetComponent<EventTrigger>() == null)
                    AddButtonAnimation(btnObj);
            }
        }

        UpdateCurrentNodeMarker();
    
    }

    void UpdateCurrentNodeMarker()
    {
        if (markerInstance != null)
            Destroy(markerInstance);

        MapNode current = GameManager.Instance.currentNode;
        if (current == null) return;

        if (nodeButtonMap.ContainsKey(current))
        {
            GameObject btnObj = nodeButtonMap[current];
            markerInstance = Instantiate(currentNodeMarker, btnObj.transform);
            markerInstance.transform.localPosition = Vector3.zero;
        }
    }

    void AddButtonAnimation(GameObject btnObj)
    {
        Button btn = btnObj.GetComponent<Button>();
        if (!btn.interactable) return;

        EventTrigger trigger = btnObj.AddComponent<EventTrigger>();

        // 마우스 올렸을 때 - 살짝 커지고 하얗게
        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
        enterEntry.eventID = EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((data) => StartCoroutine(ScaleTo(btnObj, 1.15f, 0.1f, true)));
        trigger.triggers.Add(enterEntry);

        // 마우스 나갔을 때 - 원래 크기로
        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((data) => StartCoroutine(ScaleTo(btnObj, 1f, 0.1f, false)));
        trigger.triggers.Add(exitEntry);

        // 클릭했을 때 - 살짝 작아졌다 원래 크기로
        EventTrigger.Entry clickEntry = new EventTrigger.Entry();
        clickEntry.eventID = EventTriggerType.PointerDown;
        clickEntry.callback.AddListener((data) => StartCoroutine(ClickScale(btnObj)));
        trigger.triggers.Add(clickEntry);
    }

    IEnumerator ScaleTo(GameObject obj, float targetScale, float duration, bool brighten)
    {
        if (obj == null) yield break;

        Vector3 startScale = obj.transform.localScale;
        Vector3 endScale = Vector3.one * targetScale;
        Image img = obj.GetComponentInChildren<Image>();
        Color startColor = img.color;
        Color endColor = brighten ? new Color(1f, 1f, 1f, 1f) : startColor;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (obj == null) yield break;
            elapsed += Time.unscaledDeltaTime; // [중요] 맵이 TimeScale 0이라 unscaled 사용
            float t = elapsed / duration;
            obj.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            if (brighten) img.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }
        obj.transform.localScale = endScale;
    }

    IEnumerator ClickScale(GameObject obj)
    {
        if (obj == null) yield break;

        // 살짝 작아지기
        yield return StartCoroutine(ScaleTo(obj, 0.9f, 0.05f, false));
        // 원래 크기로
        yield return StartCoroutine(ScaleTo(obj, 1f, 0.05f, false));
    }

}
