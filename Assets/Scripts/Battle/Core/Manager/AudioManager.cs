using UnityEngine;

namespace Battle.Core.Manager
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance;

        public AudioSource bgmPlayer;
        public AudioSource sfxPlayer;

        private string currentBgm = "";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬이 바뀌어도 유지
        }

        public void PlayBGM(string bgmName, float volume = 1f)
        {
            if (string.IsNullOrEmpty(bgmName)) return;
            if (currentBgm == bgmName) return;

            AudioClip clip = Resources.Load<AudioClip>("BGM/" + bgmName);
            if (clip == null)
            {
                Debug.LogWarning("[AudioManager] BGM 로드 실패: " + bgmName);
                return;
            }

            bgmPlayer.clip = clip;
            bgmPlayer.volume = volume;
            bgmPlayer.Play();
            currentBgm = bgmName;
        }

        public void StopBGM()
        {
            bgmPlayer.Stop();
            currentBgm = "";
        }

        public void PlaySFX(string sfxName)
        {
            if (string.IsNullOrEmpty(sfxName)) return;

            AudioClip clip = Resources.Load<AudioClip>("SFX/" + sfxName);
            if (clip == null)
            {
                Debug.LogWarning("[AudioManager] SFX 로드 실패: " + sfxName);
                return;
            }

            sfxPlayer.PlayOneShot(clip);
        }
    }

}
