#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace MoruToon.Editor
{
    /// <summary>
    /// MoruToon Custom ShaderGUI
    /// lilToon風のFoldoutベースUI
    /// </summary>
    public class MoruToonGUI : ShaderGUI
    {
        // ============================================
        // Template Presets
        // ============================================
        private static readonly string[] PresetNames =
        {
            "Basic", "UV Scroll", "Dissolve", "Flipbook",
            "Layer Blend", "Soft Particle", "Stencil Portal",
            "Particle FX", "Custom"
        };

        private static readonly string[][] PresetFeatures =
        {
            new[] { "_EMISSION_ON" },
            new[] { "_UVSCROLL_ON", "_ROTATION_ON", "_EMISSION_ON" },
            new[] { "_DISSOLVE_ON", "_DISSOLVE_LIFETIME", "_EMISSION_ON" },
            new[] { "_FLIPBOOK_ON", "_EMISSION_ON" },
            new[] { "_LAYERBLEND_ON", "_UVSCROLL_ON", "_EMISSION_ON" },
            new[] { "_SOFTPARTICLES_ON", "_EMISSION_ON" },
            new[] { "_DISSOLVE_ON", "_EMISSION_ON" },
            new[] { "_UVSCROLL_ON", "_EMISSION_ON", "_EMISSION_PULSE", "_SOFTPARTICLES_ON",
                    "_LIFETIMEFADE_ON", "_COLORRAMP_ON", "_HUESHIFT_ON" },
            new string[] { }
        };

        private static readonly string[] AllFeatures =
        {
            "_UVSCROLL_ON", "_ROTATION_ON",
            "_EMISSION_ON", "_EMISSION_PULSE",
            "_DISSOLVE_ON", "_DISSOLVE_LIFETIME",
            "_FLIPBOOK_ON",
            "_LAYERBLEND_ON",
            "_SOFTPARTICLES_ON",
            "_COLORRAMP_ON",
            "_HUESHIFT_ON",
            "_DISTANCEFADE_ON",
            "_LIFETIMEFADE_ON",
            "_MASK_ON"
        };

        // ============================================
        // Foldout States (永続化されてリロードで初期化されるのは許容)
        // ============================================
        private bool _showMain = true;
        private bool _showUVScroll;
        private bool _showEmission;
        private bool _showDissolve;
        private bool _showFlipbook;
        private bool _showLayerBlend;
        private bool _showSoftParticle;
        private bool _showColorRamp;
        private bool _showHueShift;
        private bool _showDistanceFade;
        private bool _showLifetimeFade;
        private bool _showMask;
        private bool _showStencil;
        private bool _showRendering;
        private bool _showAdvanced;

        // ============================================
        // Styles (lilToon風)
        // ============================================
        private GUIStyle _foldoutStyle;
        private GUIStyle _categoryStyle;
        private GUIStyle _boxOuterStyle;
        private GUIStyle _boxInnerStyle;
        private GUIStyle _searchStyle;
        private Texture2D _lineTex;
        private string _searchWord = "";

        private void InitStyles()
        {
            if (_foldoutStyle == null)
            {
                // ShurikenModuleTitle風のFoldoutスタイル
                _foldoutStyle = new GUIStyle("ShurikenModuleTitle")
                {
                    font = EditorStyles.label.font,
                    fontSize = EditorStyles.label.fontSize,
                    fontStyle = FontStyle.Bold,
                    border = new RectOffset(15, 7, 4, 4),
                    contentOffset = new Vector2(20f, -2f),
                    fixedHeight = 24
                };
            }
            if (_categoryStyle == null)
            {
                _categoryStyle = new GUIStyle(EditorStyles.label)
                {
                    fontStyle = FontStyle.Bold,
                    fontSize = 13
                };
            }
            if (_boxOuterStyle == null)
            {
                _boxOuterStyle = new GUIStyle(GUI.skin.box)
                {
                    padding = new RectOffset(6, 6, 6, 6),
                    margin = new RectOffset(2, 2, 2, 2)
                };
            }
            if (_boxInnerStyle == null)
            {
                _boxInnerStyle = new GUIStyle(GUI.skin.textArea)
                {
                    padding = new RectOffset(8, 8, 4, 4),
                    margin = new RectOffset(0, 0, 2, 2)
                };
            }
            if (_searchStyle == null)
            {
                _searchStyle = new GUIStyle(EditorStyles.toolbarSearchField);
            }
            if (_lineTex == null)
            {
                _lineTex = new Texture2D(1, 1);
                _lineTex.SetPixel(0, 0, new Color(0.3f, 0.3f, 0.3f, 0.5f));
                _lineTex.Apply();
            }
        }

        private void DrawLine()
        {
            Rect r = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(r, new Color(0.3f, 0.3f, 0.3f, 0.5f));
            EditorGUILayout.Space(2);
        }

        // ============================================
        // Foldout描画（lilToon風のクリック展開ボックス）
        // ============================================
        private bool Foldout(string title, bool display)
        {
            Rect rect = GUILayoutUtility.GetRect(16f, 24f, _foldoutStyle);
            rect.width += 8f;
            rect.x -= 8f;
            GUI.Box(rect, title, _foldoutStyle);

            // 矢印アイコン
            Rect toggleRect = new Rect(rect.x + 4f, rect.y + 4f, 13f, 13f);
            if (Event.current.type == EventType.Repaint)
            {
                EditorStyles.foldout.Draw(toggleRect, false, false, display, false);
            }

            // クリック判定
            rect.width -= 24;
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                display = !display;
                Event.current.Use();
            }

            return display;
        }

        // ============================================
        // メインGUI
        // ============================================
        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            Material material = materialEditor.target as Material;
            if (material == null) return;

            InitStyles();

            MaterialProperty templateProp = FindProperty("_TemplateMode", properties, false);

            EditorGUI.BeginChangeCheck();

            // === 検索ボックス ===
            EditorGUILayout.Space(2);
            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("🔍", GUILayout.Width(20));
                _searchWord = EditorGUILayout.TextField(_searchWord, _searchStyle);
            }
            EditorGUILayout.Space(4);

            // === プリセット選択 ===
            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Preset", GUILayout.Width(50));
                int currentPreset = templateProp != null ? (int)templateProp.floatValue : 0;
                int newPreset = EditorGUILayout.Popup(currentPreset, PresetNames);
                if (newPreset != currentPreset)
                {
                    ApplyPreset(material, newPreset);
                    if (templateProp != null)
                        templateProp.floatValue = newPreset;
                }
            }
            EditorGUILayout.Space(6);

            // ============================================================
            // Main Color
            // ============================================================
            GUILayout.Label("Main", _categoryStyle);
            _showMain = Foldout("Main Color / メインカラー", _showMain);
            if (_showMain)
            {
                EditorGUILayout.BeginVertical(_boxOuterStyle);
                DrawProp(materialEditor, properties, "_Color");
                DrawProp(materialEditor, properties, "_MainTex");
                DrawProp(materialEditor, properties, "_Brightness");
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space(4);

            // ============================================================
            // UV / Animation
            // ============================================================
            GUILayout.Label("UV / Animation", _categoryStyle);

            if (IsOn(material, "_UVSCROLL_ON") || IsInSearch("uvscroll"))
            {
                _showUVScroll = Foldout("UV Scroll / UVスクロール", _showUVScroll);
                if (_showUVScroll)
                {
                    EditorGUILayout.BeginVertical(_boxOuterStyle);
                    DrawToggleProp(materialEditor, properties, "_UVSCROLL_ON");
                    if (IsOn(material, "_UVSCROLL_ON"))
                    {
                        DrawProp(materialEditor, properties, "_ScrollSpeedU");
                        DrawProp(materialEditor, properties, "_ScrollSpeedV");
                    }
                    DrawLine();
                    DrawToggleProp(materialEditor, properties, "_ROTATION_ON");
                    if (IsOn(material, "_ROTATION_ON"))
                        DrawProp(materialEditor, properties, "_RotationSpeed");
                    EditorGUILayout.EndVertical();
                }
            }

            if (IsOn(material, "_FLIPBOOK_ON") || IsInSearch("flipbook"))
            {
                _showFlipbook = Foldout("Flipbook / フリップブック", _showFlipbook);
                if (_showFlipbook)
                {
                    EditorGUILayout.BeginVertical(_boxOuterStyle);
                    DrawToggleProp(materialEditor, properties, "_FLIPBOOK_ON");
                    if (IsOn(material, "_FLIPBOOK_ON"))
                    {
                        DrawProp(materialEditor, properties, "_FlipbookTilesX");
                        DrawProp(materialEditor, properties, "_FlipbookTilesY");
                        DrawProp(materialEditor, properties, "_FlipbookFPS");
                        DrawProp(materialEditor, properties, "_FlipbookBlend");
                    }
                    EditorGUILayout.EndVertical();
                }
            }

            EditorGUILayout.Space(4);

            // ============================================================
            // Color / Emission
            // ============================================================
            GUILayout.Label("Color / Emission", _categoryStyle);

            _showEmission = Foldout("Emission / 発光", _showEmission);
            if (_showEmission)
            {
                EditorGUILayout.BeginVertical(_boxOuterStyle);
                DrawToggleProp(materialEditor, properties, "_EMISSION_ON");
                if (IsOn(material, "_EMISSION_ON"))
                {
                    DrawProp(materialEditor, properties, "_EmissionMap");
                    DrawProp(materialEditor, properties, "_EmissionColor");
                    DrawLine();
                    DrawToggleProp(materialEditor, properties, "_EMISSION_PULSE");
                    if (IsOn(material, "_EMISSION_PULSE"))
                    {
                        DrawProp(materialEditor, properties, "_PulseSpeed");
                        DrawProp(materialEditor, properties, "_PulseMin");
                        DrawProp(materialEditor, properties, "_PulseMax");
                    }
                }
                EditorGUILayout.EndVertical();
            }

            if (IsOn(material, "_COLORRAMP_ON") || IsInSearch("ramp"))
            {
                _showColorRamp = Foldout("Color Ramp / カラーランプ", _showColorRamp);
                if (_showColorRamp)
                {
                    EditorGUILayout.BeginVertical(_boxOuterStyle);
                    DrawToggleProp(materialEditor, properties, "_COLORRAMP_ON");
                    if (IsOn(material, "_COLORRAMP_ON"))
                    {
                        DrawProp(materialEditor, properties, "_ColorRampTex");
                        DrawProp(materialEditor, properties, "_ColorRampIntensity");
                    }
                    EditorGUILayout.EndVertical();
                }
            }

            if (IsOn(material, "_HUESHIFT_ON") || IsInSearch("hue"))
            {
                _showHueShift = Foldout("HUE Shift / 色相変更", _showHueShift);
                if (_showHueShift)
                {
                    EditorGUILayout.BeginVertical(_boxOuterStyle);
                    DrawToggleProp(materialEditor, properties, "_HUESHIFT_ON");
                    if (IsOn(material, "_HUESHIFT_ON"))
                    {
                        DrawProp(materialEditor, properties, "_HueShift");
                        DrawProp(materialEditor, properties, "_HueShiftSpeed");
                    }
                    EditorGUILayout.EndVertical();
                }
            }

            if (IsOn(material, "_LAYERBLEND_ON") || IsInSearch("layer"))
            {
                _showLayerBlend = Foldout("Layer Blend / レイヤー合成", _showLayerBlend);
                if (_showLayerBlend)
                {
                    EditorGUILayout.BeginVertical(_boxOuterStyle);
                    DrawToggleProp(materialEditor, properties, "_LAYERBLEND_ON");
                    if (IsOn(material, "_LAYERBLEND_ON"))
                    {
                        DrawProp(materialEditor, properties, "_SubTex");
                        DrawProp(materialEditor, properties, "_BlendMode");
                        DrawProp(materialEditor, properties, "_SubIntensity");
                        DrawProp(materialEditor, properties, "_SubScrollSpeedU");
                        DrawProp(materialEditor, properties, "_SubScrollSpeedV");
                    }
                    EditorGUILayout.EndVertical();
                }
            }

            EditorGUILayout.Space(4);

            // ============================================================
            // Effects
            // ============================================================
            GUILayout.Label("Effects", _categoryStyle);

            if (IsOn(material, "_DISSOLVE_ON") || IsInSearch("dissolve"))
            {
                _showDissolve = Foldout("Dissolve / ディゾルブ", _showDissolve);
                if (_showDissolve)
                {
                    EditorGUILayout.BeginVertical(_boxOuterStyle);
                    DrawToggleProp(materialEditor, properties, "_DISSOLVE_ON");
                    if (IsOn(material, "_DISSOLVE_ON"))
                    {
                        DrawToggleProp(materialEditor, properties, "_DISSOLVE_LIFETIME");
                        DrawProp(materialEditor, properties, "_DissolveTex");
                        if (IsOn(material, "_DISSOLVE_LIFETIME"))
                        {
                            DrawProp(materialEditor, properties, "_DissolveDelay");
                            DrawProp(materialEditor, properties, "_DissolveSpeed");
                        }
                        else
                        {
                            DrawProp(materialEditor, properties, "_DissolveAmount");
                        }
                        DrawLine();
                        DrawProp(materialEditor, properties, "_DissolveEdgeWidth");
                        DrawProp(materialEditor, properties, "_DissolveEdgeColor");
                    }
                    EditorGUILayout.EndVertical();
                }
            }

            if (IsOn(material, "_SOFTPARTICLES_ON") || IsInSearch("soft"))
            {
                _showSoftParticle = Foldout("Soft Particle / ソフトパーティクル", _showSoftParticle);
                if (_showSoftParticle)
                {
                    EditorGUILayout.BeginVertical(_boxOuterStyle);
                    DrawToggleProp(materialEditor, properties, "_SOFTPARTICLES_ON");
                    if (IsOn(material, "_SOFTPARTICLES_ON"))
                        DrawProp(materialEditor, properties, "_SoftDistance");
                    EditorGUILayout.EndVertical();
                }
            }

            if (IsOn(material, "_DISTANCEFADE_ON") || IsInSearch("distance"))
            {
                _showDistanceFade = Foldout("Distance Fade / 距離フェード", _showDistanceFade);
                if (_showDistanceFade)
                {
                    EditorGUILayout.BeginVertical(_boxOuterStyle);
                    DrawToggleProp(materialEditor, properties, "_DISTANCEFADE_ON");
                    if (IsOn(material, "_DISTANCEFADE_ON"))
                    {
                        DrawProp(materialEditor, properties, "_FadeNear");
                        DrawProp(materialEditor, properties, "_FadeFar");
                    }
                    EditorGUILayout.EndVertical();
                }
            }

            if (IsOn(material, "_LIFETIMEFADE_ON") || IsInSearch("lifetime"))
            {
                _showLifetimeFade = Foldout("Lifetime Fade / ライフタイムフェード", _showLifetimeFade);
                if (_showLifetimeFade)
                {
                    EditorGUILayout.BeginVertical(_boxOuterStyle);
                    DrawToggleProp(materialEditor, properties, "_LIFETIMEFADE_ON");
                    if (IsOn(material, "_LIFETIMEFADE_ON"))
                    {
                        DrawProp(materialEditor, properties, "_LifetimeFadeIn");
                        DrawProp(materialEditor, properties, "_LifetimeFadeOut");
                    }
                    EditorGUILayout.EndVertical();
                }
            }

            if (IsOn(material, "_MASK_ON") || IsInSearch("mask"))
            {
                _showMask = Foldout("Mask / マスク", _showMask);
                if (_showMask)
                {
                    EditorGUILayout.BeginVertical(_boxOuterStyle);
                    DrawToggleProp(materialEditor, properties, "_MASK_ON");
                    if (IsOn(material, "_MASK_ON"))
                    {
                        DrawProp(materialEditor, properties, "_VisibleMaskTex");
                        DrawProp(materialEditor, properties, "_VisibleMaskStrength");
                        DrawLine();
                        DrawProp(materialEditor, properties, "_HideMaskTex");
                        DrawProp(materialEditor, properties, "_HideMaskStrength");
                    }
                    EditorGUILayout.EndVertical();
                }
            }

            EditorGUILayout.Space(4);

            // ============================================================
            // Advanced
            // ============================================================
            GUILayout.Label("Advanced", _categoryStyle);

            _showStencil = Foldout("Stencil / ステンシル", _showStencil);
            if (_showStencil)
            {
                EditorGUILayout.BeginVertical(_boxOuterStyle);
                DrawProp(materialEditor, properties, "_StencilRef");
                DrawProp(materialEditor, properties, "_StencilComp");
                DrawProp(materialEditor, properties, "_StencilPass");
                EditorGUILayout.EndVertical();
            }

            _showRendering = Foldout("Rendering / 描画設定", _showRendering);
            if (_showRendering)
            {
                EditorGUILayout.BeginVertical(_boxOuterStyle);
                DrawProp(materialEditor, properties, "_SrcBlend");
                DrawProp(materialEditor, properties, "_DstBlend");
                DrawLine();
                DrawProp(materialEditor, properties, "_ZWrite");
                DrawProp(materialEditor, properties, "_Cull");
                EditorGUILayout.EndVertical();
            }

            // === Advanced: 機能トグル ===
            _showAdvanced = Foldout("Feature Toggles / 機能の個別ON/OFF", _showAdvanced);
            if (_showAdvanced)
            {
                EditorGUILayout.BeginVertical(_boxOuterStyle);
                EditorGUILayout.LabelField("Enable / Disable individual features:", EditorStyles.miniLabel);
                EditorGUILayout.Space(2);

                // 2カラムでトグルを並べる
                for (int i = 0; i < AllFeatures.Length; i += 2)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        DrawToggleProp(materialEditor, properties, AllFeatures[i]);
                        if (i + 1 < AllFeatures.Length)
                            DrawToggleProp(materialEditor, properties, AllFeatures[i + 1]);
                    }
                }
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space(6);

            // === フッター ===
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("📖 Documentation", EditorStyles.miniButton))
                    Application.OpenURL("https://github.com/moruton1119/MoruToon#readme");
                GUILayout.FlexibleSpace();
            }

            if (EditorGUI.EndChangeCheck())
            {
                materialEditor.PropertiesChanged();
            }
        }

        // ============================================
        // Helpers
        // ============================================

        private void ApplyPreset(Material material, int presetIndex)
        {
            foreach (string t in AllFeatures)
            {
                if (material.HasProperty(t))
                    material.SetFloat(t, 0);
            }

            foreach (string t in PresetFeatures[presetIndex])
            {
                if (material.HasProperty(t))
                    material.SetFloat(t, 1);
            }

            foreach (string t in AllFeatures)
            {
                bool on = material.HasProperty(t) && material.GetFloat(t) > 0.5f;
                if (on)
                    material.EnableKeyword(t);
                else
                    material.DisableKeyword(t);
            }

            EditorUtility.SetDirty(material);
        }

        private bool IsOn(Material mat, string prop)
        {
            return mat.HasProperty(prop) && mat.GetFloat(prop) > 0.5f;
        }

        private bool IsInSearch(string keyword)
        {
            return !string.IsNullOrEmpty(_searchWord) &&
                   keyword.ToLower().Contains(_searchWord.ToLower());
        }

        private void DrawProp(MaterialEditor ed, MaterialProperty[] props, string name)
        {
            if (!string.IsNullOrEmpty(_searchWord))
            {
                if (!name.ToLower().Contains(_searchWord.ToLower()))
                    return;
            }

            MaterialProperty p = FindProperty(name, props, false);
            if (p != null)
                ed.ShaderProperty(p, p.displayName);
        }

        private void DrawToggleProp(MaterialEditor ed, MaterialProperty[] props, string name)
        {
            MaterialProperty p = FindProperty(name, props, false);
            if (p == null) return;

            Material mat = ed.target as Material;
            if (mat == null) return;

            EditorGUI.BeginChangeCheck();
            ed.ShaderProperty(p, p.displayName);
            if (EditorGUI.EndChangeCheck())
            {
                bool on = mat.GetFloat(name) > 0.5f;
                if (on)
                    mat.EnableKeyword(name);
                else
                    mat.DisableKeyword(name);
            }
        }
    }
}
#endif
