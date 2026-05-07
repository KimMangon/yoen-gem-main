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
    public GameObject endingPanel;

    public GameObject firstSetGroup;

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
    public Text endingScoreText;
    public Text endingBestText;
    public Text endingDifficultyText;

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
    public Transform[] restItemPositions; // 체력템 스폰위치

    [Header("Skill UI")]
    public GameObject skillEGroup;
    public Image skillEImage;    // Skill E Image
    public Image skillECooldown; // Num Image (Fill Radial)
    public Text skillENumText;   // Skill E Num

    [Header("Difficulty")]
    public GameObject difficultyPanel;
    public DifficultyData[] difficulties;
    public DifficultyData currentDifficulty;
    public Text hardLevelText;
    public Text selectedDifficultyText;
    public Text difficultyDescText;
    public GameObject diffTooltipButton;
    public Text diffTooltipText;
    private int hardLevel = 0;
    private bool isHardSelected = false;
    //ather main menu
    public GameObject titleImage;
    public GameObject maxScoreImage;
    public GameObject maxScoreText;
    public GameObject startButton;

    [Header("Resolution")]
    public GameObject resolutionPanel;
    public Text resolutionText;
    public Toggle fullscreenToggle;

    private int[] resolutionWidths = { 1920, 1600, 1280, 1024 };
    private int[] resolutionHeights = { 1080, 900, 720, 768 };
    private int currentResolutionIndex = 1;

    void Awake()
    {
        enemyList = new List<int>();
        maxScoreTxt.text = string.Format("{0:n0}", PlayerPrefs.GetInt("MaxScore"));
        Instance = this;

        if (!PlayerPrefs.HasKey("MaxScore"))  // ! 추가
        {
            PlayerPrefs.SetInt("MaxScore", 0);  // 없을 때만 0으로 초기화
        }

        if (PlayerPrefs.HasKey("ResolutionIndex"))
        {
            currentResolutionIndex = PlayerPrefs.GetInt("ResolutionIndex");
            bool isFullscreen = PlayerPrefs.GetInt("Fullscreen") == 1;
            Screen.SetResolution(
                resolutionWidths[currentResolutionIndex],
                resolutionHeights[currentResolutionIndex],
                isFullscreen
            );
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

    public void ShowEnding()
    {
        StartCoroutine(ShowEndingDelay());
    }

    IEnumerator ShowEndingDelay()
    {
        yield return new WaitForSeconds(4f);

        if (player.isDead) yield break;

        gamePanel.SetActive(false);
        endingPanel.SetActive(true);
        Time.timeScale = 0f;

        endingScoreText.text = scoreTxt.text;
        endingDifficultyText.text = currentDifficulty.difficultyName;
        if (currentDifficulty == difficulties[0]) // 이지
            endingDifficultyText.color = new Color32(255, 68, 68, 255); // #FF4444
        else // 하드
            endingDifficultyText.color = new Color32(139, 0, 0, 255); // #8B0000

        int maxScore = PlayerPrefs.GetInt("MaxScore");
        if (player.score > maxScore)
        {
            endingBestText.gameObject.SetActive(true);
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
        StartCoroutine(InBattle(false));
    }

    void StartBoss()
    {
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

        player.ResetShopState();

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
                StageStart(); // StageStart에서 InBattle() 호출
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
                isBattle = true;
                StartCoroutine(InBattle(true)); // [변경] true 전달
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
        
        int ranPos = Random.Range(0, restItemPositions.Length);
        Instantiate(restHealthItem, restItemPositions[ranPos].position, Quaternion.identity);
        startZone.SetActive(true);
    }


    public void ShowDamageText(int damage, Vector3 position)
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(position);

        screenPos += new Vector3(Random.Range(-30f, 30f), Random.Range(-20f, 20f), 0);

        GameObject go = Instantiate(damageTextPrefab, screenPos, Quaternion.identity, canvasTransform);
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

    IEnumerator InBattle(bool isBossStage = false)
    {

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
            for (int index = 0; index < stage + currentDifficulty.extraEnemyPerStage; index++)
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

        if (player.isDead) yield break;
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

        if (currentNode.nodeType == MapNode.NodeType.Boss)
        {
            startZone.SetActive(false);
            ShowEnding();
            return;
        }

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

    public void OpenDifficultyPanel()
    {
        hardLevel = 0;
        hardLevelText.text = "하드 " + hardLevel + "단계";
        selectedDifficultyText.text = "현재 난이도 <color=#FF4444>이지</color>";
        difficultyDescText.text = "<color=#FF4444>이지</color>\n적 체력 -50% 적 공격력 -50% 골드 획득량 +50%";

        titleImage.SetActive(false);
        maxScoreImage.SetActive(false);
        maxScoreText.SetActive(false);
        startButton.SetActive(false);
        diffTooltipButton.SetActive(false);

        difficultyPanel.SetActive(true);
    }

    public void SelectEasy()
    {
        currentDifficulty = difficulties[0];
        selectedDifficultyText.text = "현재 난이도 <color=#FF4444>이지</color>";
        difficultyDescText.text = "<color=#FF4444>이지</color>\n적 체력 -50% 적 공격력 -50% 골드 획득량 +50%";
        diffTooltipButton.SetActive(false);
    }

    public void SelectHard()
    {
        currentDifficulty = difficulties[hardLevel + 1];
        selectedDifficultyText.text = "현재 난이도 <color=#8B0000>하드 " + hardLevel + "단계</color>";
        UpdateHardDesc();
    }

    public void ConfirmDifficulty() // 체크 버튼에 연결
    {
        if (currentDifficulty == null) currentDifficulty = difficulties[0]; // 기본값 이지
        difficultyPanel.SetActive(false);
        GameStart();
    }

    public void ChangeHardLevel(int dir) // -1 또는 +1
    {
        hardLevel = Mathf.Clamp(hardLevel + dir, 0, 4);
        hardLevelText.text = "하드 " + hardLevel + "단계";
        selectedDifficultyText.text = "현재 난이도 <color=#8B0000>하드 " + hardLevel + "단계</color>";
        currentDifficulty = difficulties[hardLevel + 1];
        diffTooltipButton.SetActive(true);
        UpdateHardDesc();
    }

    void UpdateHardDesc()
    {
        switch (hardLevel)
        {
            case 0:
                difficultyDescText.text = "<color=#8B0000>하드 0단계</color>\n게임의 기본 난이도입니다.";
                break;
            case 1:
                difficultyDescText.text = "<color=#8B0000>하드 1단계</color>\n추가되는 고난 적 체력 +30%";
                break;
            case 2:
                difficultyDescText.text = "<color=#8B0000>하드 2단계</color>\n추가되는 고난 적 공격력 +30%";
                break;
            case 3:
                difficultyDescText.text = "<color=#8B0000>하드 3단계</color>\n추가되는 고난 골드 획득량 -30%";
                break;
            case 4:
                difficultyDescText.text = "<color=#8B0000>하드 4단계</color>\n추가되는 고난 라운드당 생성되는 적 +1";
                break;
        }
    }

    public void ShowTooltip()
    {
        diffTooltipText.text = "고난: 해당 단계에서 추가되는 패널티입니다.\n높은 단계의 고난은 이전 단계의 모든 고난을 포함합니다.";
        diffTooltipText.gameObject.SetActive(true);
    }

    public void HideTooltip()
    {
        diffTooltipText.gameObject.SetActive(false);
    }

    public void OpenResolutionPanel()
    {
        firstSetGroup.SetActive(false);
        resolutionPanel.SetActive(true);
        // 현재 해상도 표시
        resolutionText.text = resolutionWidths[currentResolutionIndex] + " x " + resolutionHeights[currentResolutionIndex];
        fullscreenToggle.isOn = Screen.fullScreen;
    }

    public void CloseResolutionPanel()
    {
        resolutionPanel.SetActive(false);
        firstSetGroup.SetActive(true);
    }

    public void ChangeResolution(int dir)
    {
        currentResolutionIndex = Mathf.Clamp(currentResolutionIndex + dir, 0, resolutionWidths.Length - 1);
        resolutionText.text = resolutionWidths[currentResolutionIndex] + " x " + resolutionHeights[currentResolutionIndex];
    }

    public void ApplyResolution()
    {
        Screen.SetResolution
       (
       resolutionWidths[currentResolutionIndex],
       resolutionHeights[currentResolutionIndex],
       fullscreenToggle.isOn 
       );

        // [추가] 설정 저장
        PlayerPrefs.SetInt("ResolutionIndex", currentResolutionIndex);
        PlayerPrefs.SetInt("Fullscreen", fullscreenToggle.isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            if (isSetting)
            {
                // [변경] 해상도 패널 열려있으면 해상도 패널만 닫기
                if (resolutionPanel.activeSelf)
                    CloseResolutionPanel();
                else
                    CloseSetting();
            }
            else
                OpenSetting();
        }

        ToggleMap();

        if (isBattle)
        {
            playTime += Time.deltaTime;
        }

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.F1)) // 현재 노드 즉시 클리어
        {
            // 모든 적 즉시 제거
            Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
            foreach (Enemy e in enemies)
                Destroy(e.gameObject);

            enemyCntA = 0;
            enemyCntB = 0;
            enemyCntC = 0;
            enemyCntD = 0;

            NodeCleared();
        }
        if (Input.GetKeyDown(KeyCode.F2)) ShowEnding();  // 엔딩 즉시 표시
        if (Input.GetKeyDown(KeyCode.F3)) player.health = player.maxHealth; // 체력 풀회복
#endif

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
