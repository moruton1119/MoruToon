#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace MoruToon.Editor
{
    /// <summary>
    /// MoruToon Custom ShaderGUI
    /// lilToon風のFoldoutベースUI + 用途別プリセット
    /// </summary>
    public class MoruToonGUI : ShaderGUI
    {
        // ============================================
        // 用途別プリセット（ユーザーが「作りたいもの」で選べる）
        // ============================================
        private class PresetConfig
        {
            public string name;
            public string description;
            public string[] features;
            public float srcBlend = 5;    // SrcAlpha
            public float dstBlend = 10;   // One (Additive)
            public float zWrite = 0;
            public float cull = 0;        // Off
        }

        private static readonly PresetConfig[] Presets =
        {
            // --- Particle系 ---
            new PresetConfig
            {
                name = "🔥 Fire / 炎",
                description = "フリップブック炎アニメ＋発光パルス＋ソフトパーティクル",
                features = new[] { "_FLIPBOOK_ON", "_EMISSION_ON", "_EMISSION_PULSE", "_SOFTPARTICLES_ON", "_LIFETIMEFADE_ON" },
                srcBlend = 5, dstBlend = 10,
            },
            new PresetConfig
            {
                name = "❄️ Ice / 氷",
                description = "UVスクロール＋発光＋ソフトパーティクル",
                features = new[] { "_UVSCROLL_ON", "_EMISSION_ON", "_SOFTPARTICLES_ON", "_LIFETIMEFADE_ON" },
                srcBlend = 5, dstBlend = 10,
            },
            new PresetConfig
            {
                name = "✨ Light / 光",
                description = "発光パルス＋ソフトパーティクル＋ライフタイムフェード",
                features = new[] { "_EMISSION_ON", "_EMISSION_PULSE", "_SOFTPARTICLES_ON", "_LIFETIMEFADE_ON" },
                srcBlend = 5, dstBlend = 10,
            },
            new PresetConfig
            {
                name = "⚡ Energy / エネルギー",
                description = "UVスクロール＋回転＋発光パルス＋カラーランプ＋HUE Shift",
                features = new[] { "_UVSCROLL_ON", "_ROTATION_ON", "_EMISSION_ON", "_EMISSION_PULSE", "_COLORRAMP_ON", "_HUESHIFT_ON", "_SOFTPARTICLES_ON" },
                srcBlend = 5, dstBlend = 10,
            },
            new PresetConfig
            {
                name = "💨 Smoke / 煙",
                description = "フリップブック煙アニメ＋ソフトパーティクル",
                features = new[] { "_FLIPBOOK_ON", "_SOFTPARTICLES_ON", "_LIFETIMEFADE_ON" },
                srcBlend = 2, dstBlend = 0, // One*Zero = overwrite alpha blend風
            },
            new PresetConfig
            {
                name = "🌟 Trail / トレイル",
                description = "UVスクロール＋発光＋カラーランプ＋ソフトパーティクル",
                features = new[] { "_UVSCROLL_ON", "_EMISSION_ON", "_COLORRAMP_ON", "_SOFTPARTICLES_ON" },
                srcBlend = 5, dstBlend = 10,
            },
            new PresetConfig
            {
                name = "💥 Explosion / 爆発",
                description = "フリップブック爆発アニメ＋発光＋ソフトパーティクル",
                features = new[] { "_FLIPBOOK_ON", "_EMISSION_ON", "_SOFTPARTICLES_ON", "_LIFETIMEFADE_ON" },
                srcBlend = 5, dstBlend = 10,
            },
            new PresetConfig
            {
                name = "🔮 Magic Circle / 魔法陣",
                description = "UVスクロール＋回転＋発光パルス＋レイヤー合成",
                features = new[] { "_UVSCROLL_ON", "_ROTATION_ON", "_EMISSION_ON", "_EMISSION_PULSE", "_LAYERBLEND_ON" },
                srcBlend = 5, dstBlend = 10,
            },
            // --- Gimmick系 ---
            new PresetConfig
            {
                name = "🚪 Stencil Portal / ポータル",
                description = "ディゾルブ＋発光＋UVスクロール。ステンシルでポータル表現",
                features = new[] { "_DISSOLVE_ON", "_EMISSION_ON", "_UVSCROLL_ON" },
                srcBlend = 5, dstBlend = 10,
            },
            new PresetConfig
            {
                name = "👋 Dissolve / 消失",
                description = "ライフタイム連携ディゾルブ＋縁発光",
                features = new[] { "_DISSOLVE_ON", "_DISSOLVE_LIFETIME", "_EMISSION_ON" },
                srcBlend = 5, dstBlend = 10,
            },
            new PresetConfig
            {
                name = "🎭 Hologram / ホログラム",
                description = "UVスクロール＋発光＋HUE Shift＋カラーランプ＋距離フェード",
                features = new[] { "_UVSCROLL_ON", "_EMISSION_ON", "_HUESHIFT_ON", "_COLORRAMP_ON", "_DISTANCEFADE_ON" },
                srcBlend = 5, dstBlend = 10,
            },
            // --- Basic ---
            new PresetConfig
            {
                name = "⚪ Basic / 基本",
                description = "最小構成。発光のみ",
                features = new[] { "_EMISSION_ON" },
                srcBlend = 5, dstBlend = 10,
            },
            new PresetConfig
            {
                name = "⚙ Custom / カスタム",
                description = "全機能を手動でON/OFF",
                features = new string[] { },
            },
        };

        private static string[] PresetDisplayNames
        {
            get
            {
                string[] names = new string[Presets.Length];
                for (int i = 0; i < Presets.Length; i++)
                    names[i] = Presets[i].name;
                return names;
            }
        }

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
        // Foldout States
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
        // Styles
        // ============================================
        private GUIStyle _foldoutStyle;
        private GUIStyle _categoryStyle;
        private GUIStyle _boxOuterStyle;
        private GUIStyle _presetLabelStyle;
        private GUIStyle _descStyle;
        private GUIStyle _searchStyle;
        private string _searchWord = "";

        private void InitStyles()
        {
            if (_foldoutStyle == null)
            {
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
            if (_presetLabelStyle == null)
            {
                _presetLabelStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 12,
                };
            }
            if (_descStyle == null)
            {
                _descStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    fontStyle = FontStyle.Italic,
                    normal = { textColor = new Color(0.6f, 0.7f, 0.8f) }
                };
            }
            if (_searchStyle == null)
            {
                _searchStyle = new GUIStyle(EditorStyles.toolbarSearchField);
            }
        }

        private void DrawLine()
        {
            Rect r = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(r, new Color(0.3f, 0.3f, 0.3f, 0.5f));
            EditorGUILayout.Space(2);
        }

        private bool Foldout(string title, bool display)
        {
            Rect rect = GUILayoutUtility.GetRect(16f, 24f, _foldoutStyle);
            rect.width += 8f;
            rect.x -= 8f;
            GUI.Box(rect, title, _foldoutStyle);

            Rect toggleRect = new Rect(rect.x + 4f, rect.y + 4f, 13f, 13f);
            if (Event.current.type == EventType.Repaint)
                EditorStyles.foldout.Draw(toggleRect, false, false, display, false);

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
            int currentPreset = templateProp != null ? (int)templateProp.floatValue : 0;
            // 範囲外チェック（プリセット数が変わった時の対策）
            if (currentPreset < 0 || currentPreset >= Presets.Length) currentPreset = 0;

            EditorGUI.BeginChangeCheck();

            // === 検索ボックス ===
            EditorGUILayout.Space(2);
            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("🔍", GUILayout.Width(20));
                _searchWord = EditorGUILayout.TextField(_searchWord, _searchStyle);
            }
            EditorGUILayout.Space(6);

            // === プリセット選択（ボタン式タブ） ===
            EditorGUILayout.LabelField("Template / テンプレート", _presetLabelStyle);
            EditorGUILayout.Space(2);

            // プリセットボタンをグリッド配置
            int columns = 3;
            for (int i = 0; i < Presets.Length; i++)
            {
                if (i % columns == 0)
                    EditorGUILayout.BeginHorizontal();

                bool isActive = (currentPreset == i);
                Color prevColor = GUI.color;

                if (isActive)
                    GUI.color = new Color(0.35f, 0.75f, 1f, 0.3f);
                else
                    GUI.color = new Color(0.8f, 0.8f, 0.8f, 0.15f);

                if (GUILayout.Button(Presets[i].name, EditorStyles.miniButton, GUILayout.Height(28)))
                {
                    currentPreset = i;
                    ApplyPreset(material, i);
                    if (templateProp != null)
                        templateProp.floatValue = i;
                }

                GUI.color = prevColor;

                if ((i + 1) % columns == 0 || i == Presets.Length - 1)
                    EditorGUILayout.EndHorizontal();
            }

            // 選択中プリセットの説明
            EditorGUILayout.Space(2);
            EditorGUILayout.LabelField("▸ " + Presets[currentPreset].description, _descStyle);
            EditorGUILayout.Space(8);

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

            if (IsOn(material, "_EMISSION_ON") || IsInSearch("emission"))
            {
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

            _showAdvanced = Foldout("Feature Toggles / 機能の個別ON/OFF", _showAdvanced);
            if (_showAdvanced)
            {
                EditorGUILayout.BeginVertical(_boxOuterStyle);
                EditorGUILayout.LabelField("Enable / Disable individual features:", EditorStyles.miniLabel);
                EditorGUILayout.Space(2);

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
        // ApplyPreset（機能ON/OFF + ブレンドモード等も自動設定）
        // ============================================
        private void ApplyPreset(Material material, int presetIndex)
        {
            PresetConfig preset = Presets[presetIndex];

            // 全機能オフ
            foreach (string t in AllFeatures)
            {
                if (material.HasProperty(t))
                    material.SetFloat(t, 0);
            }

            // プリセットの機能をオン
            foreach (string t in preset.features)
            {
                if (material.HasProperty(t))
                    material.SetFloat(t, 1);
            }

            // キーワード同期
            foreach (string t in AllFeatures)
            {
                bool on = material.HasProperty(t) && material.GetFloat(t) > 0.5f;
                if (on)
                    material.EnableKeyword(t);
                else
                    material.DisableKeyword(t);
            }

            // ブレンドモード等も自動設定
            if (material.HasProperty("_SrcBlend"))
                material.SetFloat("_SrcBlend", preset.srcBlend);
            if (material.HasProperty("_DstBlend"))
                material.SetFloat("_DstBlend", preset.dstBlend);
            if (material.HasProperty("_ZWrite"))
                material.SetFloat("_ZWrite", preset.zWrite);
            if (material.HasProperty("_Cull"))
                material.SetFloat("_Cull", preset.cull);

            EditorUtility.SetDirty(material);
        }

        // ============================================
        // Helpers
        // ============================================
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
