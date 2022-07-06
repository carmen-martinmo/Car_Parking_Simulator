using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[CreateAssetMenu(fileName = "PopupSkin", menuName = "PopupSystem/Popup_skin", order = 0)]
public class PopupSkin : ScriptableObject {

  public Sprite popup_background_image_;
  public Color poup_background_color_;

  public Color popup_text_color_;
  public TMP_FontAsset popup_text_font_;

  [Tooltip("Time between characters.")]
  public float popup_text_speed_;
  public float popup_animation_speed_;

  public float popup_time_scale_;
}
