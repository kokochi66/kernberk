using System.Collections.Generic;
using UnityEngine;
using System;

namespace Story.Core
{
    // 하나의 연출 이벤트 데이터
    [Serializable]
    public class VNEvent
    {
        public string sceneName;         // 이 이벤트가 속한 스토리 씬 이름
        public int eventIndex;
        public string characterName;     // 대사 말하는 캐릭터 이름
        public string dialogue;          // 대사 내용
        public float delayBefore;        // 이벤트 시작 전 대기 시간

        public List<string> gameObjectNames;    // 등장할 캐릭터 오브젝트 이름들
        public List<VNPosition> positions;      // 각 캐릭터의 배치 위치
        public FadeType fadeType;               // 등장/퇴장 방식
        public string expression;               // 표정 (스프라이트 이름 등)

        public SceneFadeType sceneFadeType;     // 장면 전체 페이드 처리
        public string sfx;                      // 효과음 이름
        public string bgm;                      // 배경음악 이름
        public float bgmVolume;
        public string bgImage;                  // 배경 이미지 이름
    }
}