using System;
using System.Collections.Generic;
using UnityEngine;
using XLua;


public static class CustomSettings
{
    [LuaCallCSharp]
    public static List<Type> CustomType = new List<Type>()
{       
    typeof(GameObject),
    typeof(Dictionary<string, int>),
    typeof(Dictionary<string,string>),
    typeof(DG.Tweening.DOTween),
    typeof(DG.Tweening.Ease),
    typeof(DG.Tweening.Tween),
    typeof(DG.Tweening.Sequence),
    typeof(DG.Tweening.Tweener),
    typeof(DG.Tweening.TweenCallback),
    typeof(DG.Tweening.LoopType),
    typeof(DG.Tweening.PathMode),
    typeof(DG.Tweening.PathType),
    typeof(DG.Tweening.RotateMode),
    typeof(DG.Tweening.ScrambleMode),
    typeof(DG.Tweening.TweenExtensions),
    typeof(DG.Tweening.TweenSettingsExtensions),
    typeof(DG.Tweening.ShortcutExtensions),
    typeof(DG.Tweening.ShortcutExtensions43),
    typeof(DG.Tweening.ShortcutExtensions46),
    typeof(DG.Tweening.ShortcutExtensions50),
    
};
}