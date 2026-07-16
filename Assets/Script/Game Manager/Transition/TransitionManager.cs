using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager instance;

    [Header("UI setting")]
    public Image fadeMask;

    [Header("速度設定")]
    [Tooltip("淡出成全黑的時間（秒）")]
    public float fadeOutDuration = 0.5f;
    [Tooltip("新場景淡入變透明的時間（秒）")]
    public float fadeInDuration = 0.5f;

    private bool isTransitioning = false; // 安全鎖

    private void Awake()
    {
        // 確保跨場景唯一性
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // 遊戲剛啟動或首度載入時，自動執行一次淡入（從黑變透明）
        StartCoroutine(FadeIn());
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 每次新場景載入成功後，會自動觸發這個淡入
        StartCoroutine(FadeIn());
    }

    /// <summary>
    /// 【核心修改】：外部（LevelManager）調用的方法，單純只觸發變黑動畫
    /// </summary>
    public void StartFadeOut()
    {
        if (isTransitioning) return; 
        StartCoroutine(FadeOutAnimation());
    }

    // 專心負責「變黑」的動畫，不再去干涉場景切換！
    private IEnumerator FadeOutAnimation()
    {
        isTransitioning = true; // 鎖定系統
        fadeMask.gameObject.SetActive(true);
        
        float timer = 0f;
        Color color = fadeMask.color;

        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Clamp01(timer / fadeOutDuration);
            fadeMask.color = color;
            yield return null;
        }

        // 確保最終是完全不透明的黑
        color.a = 1f;
        fadeMask.color = color;
    }

    // 專心負責「變透明」的動畫
    private IEnumerator FadeIn()
    {
        fadeMask.gameObject.SetActive(true);
        float timer = fadeInDuration;
        Color color = fadeMask.color;

        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            color.a = Mathf.Clamp01(timer / fadeInDuration);
            fadeMask.color = color;
            yield return null;
        }

        fadeMask.gameObject.SetActive(false);
        isTransitioning = false; // 解開安全鎖，新場景正式開始
    }
}