using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{

    public GameObject menuCam;
    public GameObject gameCam;
    public Player player;
    public Boss boss;
    public GameObject itemShop;
    public GameObject weaponShop;
    public GameObject blacksmithShop;
    public GameObject startZone;
    public GameObject damageTextPrefab;
    
    public int stage;
    public float playTime;
    public float augmentTimer;
    public bool isSetting = false;
    public bool isBattle;
    public int enemyCntA;
    public int enemyCntB;
    public int enemyCntC;
    public int enemyCntD;

    public Transform[] enemyZones;
    public Transform canvasTransform;
    public GameObject[] enemies;
    public List<int> enemyList;

    public GameObject menuPanel;
    public GameObject gamePanel;
    public GameObject overPanel;
    public GameObject settingPanel;

    public Text maxScoreTxt;

    public Text scoreTxt;
    public Text stageTxt;
    public Text playTimeTxt;
    public Text playerHealthTxt;
    public Text playerAmmoTxt;
    public Text playerCoinTxt;
    public Image weapon1Img;
    public Image weapon2Img;
    public Image weapon3Img;
    
    public Image weaponRImg;
    public Text enemyATxt;
    public Text enemyBTxt;
    public Text enemyCTxt;

    public RectTransform bossHealthGroup;
    public RectTransform bossHealthBar;

    public Text curScoreText;
    public Text bestText;

    public static GameManager Instance;

    int augmentPickCount;  // 현재 스테이지에서 뽑은 횟수

    [Header("Player Follow Objects")]
    public GameObject rollWeaponGroup;  
    public GameObject grenadeGroup;
    public Text augmentSelectText;

    [Header("Augment")]
    public GameObject augmentPanel;     
    public GameObject augmentGroup;
    private float selectionTimer;
    public bool isAugmentActive;

    public List<GameObject> allCards = new List<GameObject>();
    private List<GameObject> activeCards = new List<GameObject>();

    // 중첩 방지용: 현재 실행 중인 증강 코루틴 저장소
    private Dictionary<string, Coroutine> activeAugments = new Dictionary<string, Coroutine>();

    //0.4버전 업뎃
    [Header("Weapon Swap UI")]
    public GameObject weaponSwapPanel;  // 교환/버리기 팝업 패널
    private int pendingWeaponIndex;     // 획득 대기 중인 무기 인덱스
    private GameObject pendingWeaponObj; // 획득 대기 중인 무기 오브젝트
    public bool isWeaponSwap = false; //무기 교환중 플레이어 조작 관리

    [Header("MapUI")]
    public MapNode currentNode; // 현재 노드
    public bool isGameStarted = false; //게임 시작 후 첫 스타트존 통과 여부
    public bool isMapOpen = false; // 맵 열려있는지
    public GameObject mapPanel; // 맵 패널
    public GameObject restHealthItem; // 휴식맵 체력 아이템 프리팹
    public Transform restItemPos; // 체력템 스폰위치

    [Header("Skill UI")]
    public GameObject skillEGroup;
    public Image skillEImage;    // Skill E Image
    public Image skillECooldown; // Num Image (Fill Radial)
    public Text skillENumText;   // Skill E Num

    void Awake()
    {
        enemyList = new List<int>();
        maxScoreTxt.text = string.Format("{0:n0}", PlayerPrefs.GetInt("MaxScore"));
        Instance = this;

        if (!PlayerPrefs.HasKey("MaxScore"))  // ! 추가
        {
            PlayerPrefs.SetInt("MaxScore", 0);  // 없을 때만 0으로 초기화
        }

        foreach (Transform child in augmentGroup.transform)
        {
            allCards.Add(child.gameObject);
            child.gameObject.SetActive(false);
        }
    }

    public void GameStart()
    {
        menuCam.SetActive(false);
        gameCam.SetActive(true);
        menuPanel.SetActive(false);
        gamePanel.SetActive(true);
        player.gameObject.SetActive(true);
        skillEGroup.SetActive(false);

        // [추가] 게임 시작 시 맵 한 번만 생성
        MapUI mapUI = mapPanel.GetComponentInChildren<MapUI>(true);
        if (mapUI != null) mapUI.GenerateAndDisplay();

        MapGenerator mapGen = mapPanel.GetComponentInChildren<MapGenerator>(true);
        if (mapGen != null)
        {
            MapNode startNode = mapGen.allNodes.Find(n => n.nodeType == MapNode.NodeType.Start);
            if (startNode != null)
            {
                currentNode = startNode;
                startNode.isCleared = true;
                foreach (var nextNode in startNode.nextNodes)
                    nextNode.isAccessible = true;
            }
        }
        isGameStarted = false;

    }

    public void GameOver()
    {
        CloseAugmentUI();

        gamePanel.SetActive(false);
        overPanel.SetActive(true);
        curScoreText.text = scoreTxt.text;

        int maxScore = PlayerPrefs.GetInt("MaxScore");

        if(player.score > maxScore)
        {
            bestText.gameObject.SetActive(true);
            PlayerPrefs.SetInt("MaxScore", player.score);
        }

    }

    public void Restart()
    {
        Time.timeScale = 1f; // 이 줄 추가
        SceneManager.LoadScene(0);
    }



    public void StageStart()
    {
        augmentPickCount = 0; // 스테이지 시작마다 초기화
        startZone.SetActive(false);

        foreach(Transform zone in enemyZones)
            zone.gameObject.SetActive(true);

        isBattle = true;
        StartCoroutine(InBattle());
    }

    public void StageEnd()
    {
        player.transform.position = Vector3.up * 1.3f;

        foreach (Transform zone in enemyZones)
            zone.gameObject.SetActive(false);

        isBattle = false;

        //스타트존 활성화 후 NodeCleared로 맵 열기
        startZone.SetActive(true);
        NodeCleared();
    }

    public void ShowWeaponSwapUI(int weaponIndex, GameObject weaponObj)
    {
        pendingWeaponIndex = weaponIndex;
        pendingWeaponObj = weaponObj;
        if (weaponObj != null)
        {
            // 무기 월드 좌표 → 스크린 좌표 변환
            Vector3 screenPos = Camera.main.WorldToScreenPoint(weaponObj.transform.position);

            // 스크린 좌표 → Canvas 좌표 변환
            RectTransform canvasRect = augmentPanel.transform.parent.GetComponent<RectTransform>();
            Vector2 canvasPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, screenPos, null, out canvasPos);

            // 패널 위치를 무기 아래로 설정
            RectTransform swapRect = weaponSwapPanel.GetComponent<RectTransform>();
            swapRect.anchoredPosition = canvasPos + new Vector2(0, -80f); // -80은 아래 거리
        }
        isWeaponSwap = true;
        weaponSwapPanel.SetActive(true);
    }

    // "교환" 버튼 눌렀을 때 → 어떤 슬롯이랑 바꿀지 선택
    public void SwapWeapon(int slotIndex)
    {
        int oldWeaponIndex = player.weaponSlots[slotIndex];

        // 기존 슬롯 무기 제거 및 무기획득 횟수 초기화
        player.weaponPickCount[oldWeaponIndex] = 0;
        player.hasWeapons[oldWeaponIndex] = false;
        player.weapons[oldWeaponIndex].SetActive(false);

        // 새 무기 배치
        player.weaponSlots[slotIndex] = pendingWeaponIndex;
        player.hasWeapons[pendingWeaponIndex] = true;
        player.weaponPickCount[pendingWeaponIndex]++;

        // 현재 장착 중인 무기였으면 새 무기로 교체
        if (player.equipWeaponIndex == slotIndex)
        {
            player.equipWeapon = player.weapons[pendingWeaponIndex].GetComponent<Weapon>();
            player.weapons[pendingWeaponIndex].SetActive(true);
        }

        if (pendingWeaponObj != null) Destroy(pendingWeaponObj);
        weaponSwapPanel.SetActive(false);
        isWeaponSwap = false; // [추가]
    }

    public void DiscardWeapon()
    {
        //버리기 버튼에 연결
        if (pendingWeaponObj != null) Destroy(pendingWeaponObj);
        weaponSwapPanel.SetActive(false);
        isWeaponSwap = false; // [추가]
    }

    public void EnterNode(MapNode node)
    {
        if (currentNode != null && !currentNode.isCleared) return;

        currentNode = node;
        node.isVisited = true;
        CloseMap();
        startZone.SetActive(false);

        itemShop.SetActive(false);
        weaponShop.SetActive(false);
        blacksmithShop.SetActive(false);

        player.transform.position = Vector3.up * 1.3f;

        player.transform.position = Vector3.up * 1.3f;

        foreach (GameObject rollWeapon in player.rollWeapons)
        {
            if (rollWeapon == null) continue;
            foreach (Orbit orbit in rollWeapon.GetComponentsInChildren<Orbit>())
                orbit.TeleportWithPlayer(player.transform.position);
        }

        foreach (GameObject grenade in player.grenades)
        {
            if (grenade == null) continue;
            Orbit orbit = grenade.GetComponent<Orbit>();
            if (orbit != null) orbit.TeleportWithPlayer(player.transform.position);
        }

        if (node.nodeType == MapNode.NodeType.Mystery)
        {
            int rand = Random.Range(0, 3);
            node.mysteryActualType = rand == 0 ? MapNode.NodeType.Shop :
                                     rand == 1 ? MapNode.NodeType.Rest :
                                                 MapNode.NodeType.Combat;
        }
        MapNode.NodeType actualType = node.nodeType == MapNode.NodeType.Mystery ? node.mysteryActualType : node.nodeType;

        switch (actualType)
        {
            case MapNode.NodeType.Start:
                NodeCleared();
                break;
            case MapNode.NodeType.Combat:
                StageStart();
                break;
            case MapNode.NodeType.Rest:
                // 휴식 구역 활성화
                StartRest();
                break;
            case MapNode.NodeType.Shop:
                // 상점 활성화
                StartShop();
                break;
            case MapNode.NodeType.Boss:
                StartBoss();
                break;
        }
    }

    void StartShop()
    {
        // 상점 오브젝트 활성화
        itemShop.SetActive(true);
        weaponShop.SetActive(true);
        blacksmithShop.SetActive(true);
        startZone.SetActive(true); // 상점 나가면 맵으로
    }

    void StartRest()
    {

        Instantiate(restHealthItem, restItemPos.position, Quaternion.identity);
        startZone.SetActive(true);
    }

    void StartBoss() // [추가]
    {
        isBattle = true;
        StartCoroutine(InBattle());
    }

    public void ShowDamageText(int damage, Vector3 position)
    {
        // 3D 위치를 화면 스크린 좌표로 변환
        Vector3 screenPos = Camera.main.WorldToScreenPoint(position);

        // 텍스트 생성
        GameObject go = Instantiate(damageTextPrefab, screenPos, Quaternion.identity, canvasTransform);

        // 데미지 값 전달
        go.GetComponent<DamageText>().Setup(damage);
    }

    void UpdateWeaponSlotImage(Image slotImg, int slotIndex)
    {
        if (player.weaponSlots[slotIndex] != -1)
        {
            Weapon w = player.weapons[player.weaponSlots[slotIndex]].GetComponent<Weapon>();
            slotImg.sprite = w.icon;
            slotImg.color = Color.white;
        }
        else
        {
            slotImg.sprite = null;
            slotImg.color = new Color(1, 1, 1, 0); // 빈 슬롯은 투명
        }
    }

    IEnumerator InBattle()
    {
        bool isBossStage = currentNode != null && currentNode.nodeType == MapNode.NodeType.Boss;

        if (isBossStage)
        {
            enemyCntD++;

            GameObject instantEnemy = Instantiate(enemies[3], enemyZones[0].position, enemyZones[0].rotation);
            Enemy enemy = instantEnemy.GetComponent<Enemy>();
            enemy.target = player.transform;
            enemy.manger = this;
            boss = instantEnemy.GetComponent<Boss>();
        }
        else
        {
            for (int index = 0; index < stage; index++)
            {
                int ran = Random.Range(0, 3);
                enemyList.Add(ran);

                switch (ran)
                {
                    case 0:
                        enemyCntA++;
                        break;
                    case 1:
                        enemyCntB++;
                        break;
                    case 2:
                        enemyCntC++;
                        break;
                }
            }

            while (enemyList.Count > 0)
            {
                int ranZone = Random.Range(0, 4);
                GameObject instantEnemy = Instantiate(enemies[enemyList[0]], enemyZones[ranZone].position, enemyZones[ranZone].rotation);
                Enemy enemy = instantEnemy.GetComponent<Enemy>();
                enemy.target = player.transform;
                enemy.manger = this;
                enemyList.RemoveAt(0);
                yield return new WaitForSeconds(5f);
            }

        }

        while (enemyCntA + enemyCntB + enemyCntC + enemyCntD > 0)
            yield return null;

        yield return new WaitForSeconds(5f);

        boss = null;
        StageEnd();


    }

    void OpenAugmentUI()
    {
        if (augmentPickCount >= stage + 2) return;

        isAugmentActive = true;
        selectionTimer = 5f;
        augmentPanel.SetActive(true);
        augmentSelectText.gameObject.SetActive(true);

        activeCards.Clear();
        foreach (GameObject card in allCards) card.SetActive(false);

        ShuffleList(allCards);
        for (int i = 0; i < 2; i++)
        {
            allCards[i].SetActive(true);
            activeCards.Add(allCards[i]);
        }

        // 화면 왼쪽부터 순서대로 Q(0), E(1)이 되도록 sibling index 기준 정렬
        activeCards.Sort((a, b) =>
            a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));

        for (int i = 0; i < activeCards.Count; i++)
        {
            int capturedIndex = i;
            Button btn = activeCards[i].GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => SelectAugment(capturedIndex));
        }
    }

    public void CloseAugmentUI()
    {
        isAugmentActive = false;
        selectionTimer = 0;
        augmentPanel.SetActive(false);
        augmentSelectText.gameObject.SetActive(false);

        // 선택 안 된 카드만 바로 끄기
        foreach (GameObject card in activeCards)
            card.SetActive(false);

        activeCards.Clear();
    }

    void SelectAugment(int index)
    {
        if (!isAugmentActive) return;
        if (index < activeCards.Count)
        {
            AugmentCardEffect effect = activeCards[index].GetComponent<AugmentCardEffect>();
            activeCards[index].GetComponent<Augment>().OnClick();
            StartCoroutine(CloseAfterEffect(effect));
        }
    }

    IEnumerator CloseAfterEffect(AugmentCardEffect effect)
    {
        isAugmentActive = false;
        selectionTimer = 0;
        augmentPickCount++;

        // GameManager에서 연출 코루틴 직접 실행
        yield return StartCoroutine(effect.SelectEffect());

        augmentPanel.SetActive(false);
        foreach (GameObject card in activeCards)
            card.SetActive(false);
        activeCards.Clear();

        OpenMap();
    }


    void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[rand];
            list[rand] = temp;
        }
    }

    void OpenSetting()
    {
        isSetting = true;
        Time.timeScale = 0f;
        settingPanel.SetActive(true);

        if (gamePanel.activeSelf) gamePanel.SetActive(false);
        if (isMapOpen) mapPanel.SetActive(false);
        // augmentPanel은 건드리지 않음
    }

    public void CloseSetting()
    {
        isSetting = false;
        Time.timeScale = 1f;
        settingPanel.SetActive(false);

        if (!menuPanel.activeSelf && !overPanel.activeSelf)
            gamePanel.SetActive(true);

        if (isAugmentActive)
            augmentPanel.SetActive(true);

        if (isMapOpen) mapPanel.SetActive(true);
    }

    // 게임 종료 버튼에 연결
    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();

        // 에디터에서 테스트할 때
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void OpenMap()
    {
        isMapOpen = true;
        mapPanel.SetActive(true);
        gamePanel.SetActive(false);
        Time.timeScale = 0f;
    }

    public void CloseMap()
    {
        isMapOpen = false;
        mapPanel.SetActive(false);
        gamePanel.SetActive(true);
        Time.timeScale = 1f;
    }

    public void NodeCleared()
    {
        if (currentNode == null) return;

        currentNode.isCleared = true;
        stage++;
        player.rollSkillCount = 0;
        player.rollSkillTimer = 0f;
        player.isRollSkillReady = true;

        foreach (var node in mapPanel.GetComponentInChildren<MapUI>().GetAllNodes())
        {
            if (node.y == currentNode.y && node != currentNode)
                node.isAccessible = false;
        }

        // 다음 노드들 접근 가능하게
        foreach (var nextNode in currentNode.nextNodes)
        {
            nextNode.isAccessible = true;
        }

        // 맵 UI 업데이트
        MapUI mapUI = mapPanel.GetComponentInChildren<MapUI>();
        if (mapUI != null) mapUI.RefreshMap();

        // 증강창 열기
        OpenAugmentUI();
    }

    void ToggleMap()
    {
        if (!Input.GetButtonDown("mDown")) return;
        if (isBattle) return;
        if (isSetting) return;
        if (!isGameStarted) return;

        if (isMapOpen) CloseMap();
        else OpenMap();
    }

    

    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            if (isSetting) CloseSetting();
            else OpenSetting();
        }

        ToggleMap();

        if (isBattle)
        {
            playTime += Time.deltaTime;
        }
   
    }


    void LateUpdate()
    {

        stageTxt.text = "STAGE" + stage;
        
        int hour = (int)(playTime / 3600);
        int min = (int)((playTime - hour * 3600) / 60);
        int second = (int)(playTime % 60);

        playTimeTxt.text = string.Format("{0:00}", hour) + ":" + string.Format("{0:00}", min) + ":" + string.Format("{0:00}", second);

        scoreTxt.text = string.Format("{0:n0}", player.score);
        playerHealthTxt.text = player.health + " / " + player.maxHealth;
        playerCoinTxt.text = string.Format("{0:n0}", player.coin);

        if (player.equipWeapon == null)
            playerAmmoTxt.text = " / " + player.ammo;
        else if (player.equipWeapon.type == Weapon.Type.Melee)
            playerAmmoTxt.text = " / " + player.ammo;
        else
            playerAmmoTxt.text = player.equipWeapon.curAmmo + " / " + player.ammo;

        UpdateWeaponSlotImage(weapon1Img, 0);
        UpdateWeaponSlotImage(weapon2Img, 1);
        UpdateWeaponSlotImage(weapon3Img, 2);
        weaponRImg.color = new Color(1, 1, 1, player.hasGrenades > 0 ? 1 : 0);

        if (player.rollSkillTimer > 0)
            skillECooldown.fillAmount = player.rollSkillTimer / player.rollSkillCooldown;
        else
            skillECooldown.fillAmount = 0;

        skillENumText.text = (player.rollSkillMaxCount - player.rollSkillCount) + "/" + player.rollSkillMaxCount;

        enemyATxt.text = enemyCntA.ToString();
        enemyBTxt.text = enemyCntB.ToString();
        enemyCTxt.text = enemyCntC.ToString();

        if(boss != null)
        {
            bossHealthGroup.anchoredPosition = Vector3.down * 30;
            bossHealthBar.localScale = new Vector3((float)boss.curHealth / boss.maxHealth, 1, 1);
        }

        else
        {
            bossHealthGroup.anchoredPosition = Vector3.up * 300;
        }

    }














}
