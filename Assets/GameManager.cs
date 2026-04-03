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
    public Image weapon4Img;
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

    [Header("Augment")]
    public GameObject augmentPanel;     
    public GameObject augmentGroup;
    private float selectionTimer;
    public bool isAugmentActive;

    private List<GameObject> allCards = new List<GameObject>();
    private List<GameObject> activeCards = new List<GameObject>();

    // 중첩 방지용: 현재 실행 중인 증강 코루틴 저장소
    private Dictionary<string, Coroutine> activeAugments = new Dictionary<string, Coroutine>();

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

        itemShop.SetActive(false);
        weaponShop.SetActive(false);
        startZone.SetActive(false);

        foreach(Transform zone in enemyZones)
            zone.gameObject.SetActive(true);

        isBattle = true;
        StartCoroutine(InBattle());
    }

    public void StageEnd()
    {
        player.transform.position = Vector3.up * 1.3f;

        itemShop.SetActive(true);
        weaponShop.SetActive(true);
        startZone.SetActive(true);

        foreach(Transform zone in enemyZones)
            zone.gameObject.SetActive(false);

        isBattle = false;
        stage++;
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

    IEnumerator InBattle()
    {
        if (stage % 5 == 0)
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
        {
            yield return null;
        }
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
    }

    public void CloseAugmentUI()
    {
        isAugmentActive = false;
        selectionTimer = 0;
        augmentPanel.SetActive(false);

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

    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            if (isSetting) CloseSetting();
            else OpenSetting();
        }

        if (isBattle)
        {
            playTime += Time.deltaTime;

            if (!isAugmentActive)
            {
                augmentTimer += Time.deltaTime;

                if (augmentTimer >= 15f)
                {
                    OpenAugmentUI();
                    augmentTimer = 0;
                }
            }
            else
            {
                // 5초 카운트다운 (게임이 안 멈추므로 일반 deltaTime 사용)
                selectionTimer -= Time.deltaTime;

                if (selectionTimer <= 0)
                {
                    // 시간 초과 시 랜덤 자동 선택
                    SelectAugment(Random.Range(0, activeCards.Count));
                    return;
                }
                if (!isSetting)
                {
                    if (Input.GetButtonDown("qDown")) SelectAugment(0);
                    else if (Input.GetButtonDown("eDown")) SelectAugment(1);
                }
            }

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

        weapon1Img.color = new Color(1, 1, 1, player.hasWeapons[0] ? 1 : 0);
        weapon2Img.color = new Color(1, 1, 1, player.hasWeapons[1] ? 1 : 0);
        weapon3Img.color = new Color(1, 1, 1, player.hasWeapons[2] ? 1 : 0);
        weapon4Img.color = new Color(1, 1, 1, player.hasWeapons[3] ? 1 : 0);
        weaponRImg.color = new Color(1, 1, 1, player.hasGrenades > 0 ? 1 : 0);

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
