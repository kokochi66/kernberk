using System.Collections.Generic;
using UnityEngine;

namespace Story.Core
{
    // 캐릭터 등장/퇴장 방식
    public enum FadeType
    {
        NONE,
        FADE_IN,
        FADE_OUT
    }

    // 장면 전체 페이드 여부
    public enum SceneFadeType
    {
        NONE,
        SCENE_START,
        SCENE_END
    }

    // 캐릭터 배치 위치 (화면 기준)
    public enum VNPosition
    {
        LEFT,
        LEFT_CENTER,
        CENTER,
        RIGHT_CENTER,
        RIGHT
    }
}
