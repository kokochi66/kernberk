using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Story.Core;
using TMPro;
using System;
using UnityEngine.UI;
using System.IO;

namespace Story.GameSystem
{
    public class VNEventManager : MonoBehaviour
    {
        [Header("필수 참조")]
        public GameObject dialogueBox;
        public TextMeshProUGUI dialogueText;
        public TextMeshProUGUI characterNameText;
        public GameObject nextButton;

        [Header("페이드 및 배경")]
        public UnityEngine.UI.Image sceneFade;
        public UnityEngine.UI.Image background;

        [Header("오디오")]
        public AudioSource bgmPlayer;
        public AudioSource sfxPlayer;

        [Header("캐릭터 슬롯들 (좌, 좌중, 중, 우중, 우)")]
        public List<Transform> characterSlots;
        public Dictionary<string, GameObject> loadedCharacters = new();

        private List<VNEvent> events;
        public int currentEventIndex = 0;
        private string currentBgm = "";
        private bool isTyping = false;
        private bool isNextEventsPlaying = false;
        private Coroutine typingCoroutine;
        private string currentFullText;


        public static string excelFileName = "kernberk_sheet.xlsx";

        void Start()
        {

            bgmPlayer.volume = 0.5f; // BGM은 기본보다 약하게
            sfxPlayer.volume = 0.7f; // 효과음도 너무 크지 않게
            string fullPath = Path.Combine(Application.streamingAssetsPath, excelFileName);
            events = VNEventParser.LoadEventsFromExcelPath(fullPath);
            StartCoroutine(PlayNextEvent());
        }

        void Update()
        {
            // 마우스 왼쪽 클릭 or 엔터키 or 스페이스키 입력 감지
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            {
                if (isTyping)
                {
                    // 스킵 요청: 타이핑 즉시 종료
                    isTyping = false;
                    return;
                }

                if (!isTyping && !isNextEventsPlaying)
                {
                    OnNextClicked();
                }
            }
        }



        public void OnNextClicked()
        {
            if (!isTyping)
                StartCoroutine(PlayNextEvent());
        }

        IEnumerator PlayNextEvent()
        {
            if (!isNextEventsPlaying) {
                isNextEventsPlaying = true;
                if (currentEventIndex >= events.Count)
                {
                    Debug.Log("[VN] 모든 이벤트 종료");
                    yield break;
                }

                VNEvent e = events[currentEventIndex];
                Debug.Log($"[VN] 이벤트 {currentEventIndex} 실행 - Scene: {e.sceneName}, 대사자: {e.characterName}");
                currentEventIndex++;

                yield return new WaitForSeconds(e.delayBefore);
                Debug.Log($"[VN] delayBefore: {e.delayBefore}초 대기 완료");

                // 배경 이미지 설정
                if (!string.IsNullOrEmpty(e.bgImage))
                {
                    Debug.Log($"[VN] 배경 이미지 변경 요청: BG/{e.bgImage}");
                    Sprite bg = Resources.Load<Sprite>("BG/" + e.bgImage);
                    if (bg != null)
                    {
                        background.sprite = bg;
                        background.gameObject.SetActive(true);
                        Debug.Log($"[VN] 배경 이미지 설정 완료: {e.bgImage}");
                    }
                    else
                    {
                        Debug.LogWarning($"[VN] 배경 이미지 로드 실패: BG/{e.bgImage}");
                    }
                }

                // 배경 음악 설정
                if (!string.IsNullOrEmpty(e.bgm))
                {
                    Debug.Log($"[VN] BGM 요청: BGM/{e.bgm}");
                    AudioClip clip = Resources.Load<AudioClip>("BGM/" + e.bgm);
                    if (clip != null && currentBgm != e.bgm)
                    {
                        bgmPlayer.clip = clip;
                        bgmPlayer.volume = e.bgmVolume;
                        bgmPlayer.Play();
                        currentBgm = e.bgm;
                        Debug.Log($"[VN] BGM 재생 시작: {e.bgm} (볼륨: {e.bgmVolume})");
                    }
                    else if (clip == null)
                    {
                        Debug.LogWarning($"[VN] BGM 로드 실패: BGM/{e.bgm}");
                    }
                    else
                    {
                        Debug.Log($"[VN] BGM 동일: {e.bgm} (재생 유지)");
                    }
                }
                else
                {
                    Debug.Log("[VN] BGM 변경 없음, 이전 배경음 계속 유지");
                }

                // 효과음 재생
                if (!string.IsNullOrEmpty(e.sfx))
                {
                    Debug.Log($"[VN] SFX 요청: SFX/{e.sfx}");
                    AudioClip sfx = Resources.Load<AudioClip>("SFX/" + e.sfx);
                    if (sfx != null)
                    {
                        sfxPlayer.PlayOneShot(sfx);
                        Debug.Log($"[VN] SFX 재생: {e.sfx}");
                    }
                    else
                    {
                        Debug.LogWarning($"[VN] SFX 로드 실패: SFX/{e.sfx}");
                    }
                }


                // 캐릭터 배치 및 연출 처리
                for (int i = 0; i < Mathf.Min(e.gameObjectNames.Count, e.positions.Count); i++)
                {
                    string name = e.gameObjectNames[i];
                    VNPosition pos = e.positions[i];

                    // 슬롯 인덱스 범위 검사
                    if ((int)pos >= characterSlots.Count)
                    {
                        Debug.LogWarning($"[VN] 슬롯 인덱스 {pos}가 characterSlots.Count({characterSlots.Count})를 초과함.");
                        continue;
                    }

                    Transform slot = characterSlots[(int)pos];
                    Debug.Log($"[VN] 캐릭터 '{name}' → 슬롯 '{pos}' 위치에 배치 시도.");

                    GameObject go;
                    if (!loadedCharacters.TryGetValue(name, out go))
                    {
                        GameObject prefab = Resources.Load<GameObject>("Characters/" + name);
                        if (prefab == null)
                        {
                            Debug.LogWarning($"[VN] '{name}' 프리팹을 Resources/Characters/ 에서 찾지 못함.");
                            continue;
                        }

                        // go = Instantiate(prefab, slot.position, Quaternion.identity, slot);
                        go = Instantiate(prefab, slot);
                        go.transform.localPosition = prefab.transform.localPosition;

                        loadedCharacters[name] = go;
                        Debug.Log($"[VN] '{name}' 프리팹 인스턴스화 완료.");
                    }

                    RawImage img = go.GetComponent<RawImage>();
                    if (img == null)
                    {
                        Debug.LogWarning($"[VN] '{name}' 오브젝트에 RawImage 컴포넌트가 없음.");
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(e.expression))
                        {
                            // Resources/Characters/Shian_Happy 같은 경로에 Texture2D가 있어야 함
                            Texture expr = Resources.Load<Texture>($"Characters/{name}_{e.expression}");
                            if (expr != null)
                            {
                                img.texture = expr;
                                Debug.Log($"[VN] '{name}' → 표정 변경: '{e.expression}'");
                            }
                            else
                            {
                                Debug.LogWarning($"[VN] '{name}_{e.expression}' 텍스처가 없음.");
                            }
                        }

                        img.gameObject.SetActive(true);
                        Debug.Log($"[VN] '{name}' Active 설정 true");

                        if (e.fadeType == FadeType.FADE_IN)
                        {
                            Debug.Log($"[VN] '{name}' 페이드 인 시작");
                            StartCoroutine(FadeInRawImage(img));
                        }
                        else if (e.fadeType == FadeType.FADE_OUT)
                        {
                            Debug.Log($"[VN] '{name}' 페이드 아웃 시작");
                            StartCoroutine(FadeOutRawImage(img));
                        }
                    }


                }

                if (e.sceneFadeType == SceneFadeType.SCENE_START)
                {
                    Debug.Log("[VN] 장면 페이드 인 시작");
                    StartCoroutine(FadeInScreen());
                    Debug.Log("[VN] 장면 페이드 인 완료");
                }

                if (e.sceneFadeType == SceneFadeType.SCENE_END)
                {
                    Debug.Log("[VN] 장면 페이드 아웃 시작");
                    StartCoroutine(FadeOutScreen());
                    Debug.Log("[VN] 장면 페이드 아웃 완료");
                }

                characterNameText.text = e.characterName;
                dialogueBox.SetActive(true);
                nextButton.SetActive(false);

                isTyping = true;
                typingCoroutine = StartCoroutine(TypeText(e.dialogue));
                yield return typingCoroutine;

                nextButton.SetActive(true);
                isNextEventsPlaying = false;
            }
            
        }

        IEnumerator TypeText(string fullText)
        {
            dialogueText.text = "";
            currentFullText = fullText;

            for (int i = 0; i < fullText.Length; i++)
            {
                if (!isTyping) // 스킵 요청되면 중단
                {
                    dialogueText.text = fullText;
                    yield break;
                }

                dialogueText.text += fullText[i];
                yield return new WaitForSeconds(0.03f);
            }

            isTyping = false;
        }


        IEnumerator FadeInScreen()
        {
            sceneFade.gameObject.SetActive(true);
            Color c = sceneFade.color;
            c.a = 1f;
            sceneFade.color = c;

            float t = 1f;
            while (t > 0)
            {
                t -= Time.deltaTime;
                c.a = t;
                sceneFade.color = c;
                yield return null;
            }

            sceneFade.gameObject.SetActive(false);
        }

        IEnumerator FadeOutScreen(float duration = 2f)
        {
            sceneFade.gameObject.SetActive(true);
            Color c = sceneFade.color;
            c.a = 0f;
            sceneFade.color = c;

            float t = 0f;
            while (t < duration)
            {
                float alpha = Mathf.Lerp(0f, 1f, t / duration);
                c.a = alpha;
                sceneFade.color = c;
                t += Time.deltaTime;
                yield return null;
            }

            c.a = 1f;
            sceneFade.color = c;
        }
        public static IEnumerator FadeInRawImage(RawImage img, float duration = 1f)
        {
            if (img == null) yield break;

            Color c = img.color;
            c.a = 0f;
            img.color = c;
            img.gameObject.SetActive(true);

            float t = 0f;
            while (t < duration)
            {
                float alpha = Mathf.Lerp(0f, 1f, t / duration);
                img.color = new Color(c.r, c.g, c.b, alpha);
                t += Time.deltaTime;
                yield return null;
            }

            img.color = new Color(c.r, c.g, c.b, 1f); // 완전 불투명
        }

        public static IEnumerator FadeOutRawImage(RawImage img, float duration = 1f)
        {
            if (img == null) yield break;

            Color c = img.color;
            c.a = 1f;
            img.color = c;

            float t = 0f;
            while (t < duration)
            {
                float alpha = Mathf.Lerp(1f, 0f, t / duration);
                img.color = new Color(c.r, c.g, c.b, alpha);
                t += Time.deltaTime;
                yield return null;
            }

            img.color = new Color(c.r, c.g, c.b, 0f); // 완전 투명
            img.gameObject.SetActive(false);
        }

    }
}