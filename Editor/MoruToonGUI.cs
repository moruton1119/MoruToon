#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace MoruToon.Editor
{
    /// <summary>
    /// MoruToon Custom ShaderGUI
    /// 設定タブ + テンプレートタブの2画面構成
    /// </summary>
    public class MoruToonGUI : ShaderGUI
    {
        // ============================================
        // Preset Definition
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

        // ============================================
        // Built-in Presets (用途別)
        // ============================================
        private static readonly PresetConfig[] BuiltInPresets =
        {
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
                srcBlend = 2, dstBlend = 0,
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
            new PresetConfig
            {
                name = "🚪 Stencil Portal / ポータル",
                description = "ディゾルブ＋発光＋UVスクロール",
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
            new PresetConfig
            {
                name = "⚪ Basic / 基本",
                description = "最小構成。発光のみ",
                features = new[] { "_EMISSION_ON" },
                srcBlend = 5, dstBlend = 10,
            },
        };

        // カスタムテンプレート（ユーザーが追加したもの）
        private List<PresetConfig> _customPresets = new List<PresetConfig>();

        // 全テンプレート（Built-in + Custom）
        private List<PresetConfig> AllPresets
        {
            get
            {
                List<PresetConfig> all = new List<PresetConfig>(BuiltInPresets);
                all.AddRange(_customPresets);
                return all;
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
            "_MASK_ON",
            "_BLACKTRANSPARENCY_ON"
        };

        // ============================================
        // Tab State
        // ============================================
        private enum MainTab { Settings, Template }
        private MainTab _currentTab = MainTab.Settings;

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
        private bool _showBlackTransparency;
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

        // カスタムテンプレート保存用の名前入力
        private string _newPresetName = "";

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
                _presetLabelStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 12 };
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
        // カスタムテンプレートの保存・読み込み
        // ============================================
        private string GetCustomPresetDir()
        {
            return Path.Combine(Application.persistentDataPath, "MoruToon", "Presets");
        }

        private void SaveCustomPreset(string name, Material material)
        {
            PresetConfig preset = new PresetConfig
            {
                name = "📦 " + name,
                description = "User custom preset",
            };

            List<string> features = new List<string>();
            foreach (string f in AllFeatures)
            {
                if (material.HasProperty(f) && material.GetFloat(f) > 0.5f)
                    features.Add(f);
            }
            preset.features = features.ToArray();

            if (material.HasProperty("_SrcBlend")) preset.srcBlend = material.GetFloat("_SrcBlend");
            if (material.HasProperty("_DstBlend")) preset.dstBlend = material.GetFloat("_DstBlend");
            if (material.HasProperty("_ZWrite")) preset.zWrite = material.GetFloat("_ZWrite");
            if (material.HasProperty("_Cull")) preset.cull = material.GetFloat("_Cull");

            _customPresets.Add(preset);
            SaveCustomPresetsToDisk();
        }

        private void SaveCustomPresetsToDisk()
        {
            try
            {
                string dir = GetCustomPresetDir();
                Directory.CreateDirectory(dir);

                foreach (var preset in _customPresets)
                {
                    string safeName = preset.name.Replace("📦 ", "").Replace("/", "_").Replace(" ", "_");
                    string path = Path.Combine(dir, safeName + ".json");

                    PresetData data = new PresetData
                    {
                        name = preset.name,
                        description = preset.description,
                        features = preset.features,
                        srcBlend = preset.srcBlend,
                        dstBlend = preset.dstBlend,
                        zWrite = preset.zWrite,
                        cull = preset.cull
                    };

                    File.WriteAllText(path, JsonUtility.ToJson(data, true));
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"MoruToon: Failed to save custom presets: {e.Message}");
            }
        }

        private void LoadCustomPresetsFromDisk()
        {
            try
            {
                string dir = GetCustomPresetDir();
                if (!Directory.Exists(dir)) return;

                _customPresets.Clear();
                string[] files = Directory.GetFiles(dir, "*.json");
                foreach (string file in files)
                {
                    string json = File.ReadAllText(file);
                    PresetData data = JsonUtility.FromJson<PresetData>(json);
                    if (data != null && !string.IsNullOrEmpty(data.name))
                    {
                        _customPresets.Add(new PresetConfig
                        {
                            name = data.name,
                            description = data.description ?? "User custom preset",
                            features = data.features ?? new string[] { },
                            srcBlend = data.srcBlend,
                            dstBlend = data.dstBlend,
                            zWrite = data.zWrite,
                            cull = data.cull
                        });
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"MoruToon: Failed to load custom presets: {e.Message}");
            }
        }

        [System.Serializable]
        private class PresetData
        {
            public string name;
            public string description;
            public string[] features;
            public float srcBlend = 5;
            public float dstBlend = 10;
            public float zWrite = 0;
            public float cull = 0;
        }

        // ============================================
        // メインGUI
        // ============================================
        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            Material material = materialEditor.target as Material;
            if (material == null) return;

            InitStyles();

            // カスタムテンプレート読み込み（初回）
            if (_customPresets.Count == 0 && !Directory.Exists(GetCustomPresetDir()))
            {
                // 初回は何もしない
            }
            else if (_customPresets.Count == 0)
            {
                LoadCustomPresetsFromDisk();
            }

            MaterialProperty templateProp = FindProperty("_TemplateMode", properties, false);
            int currentPreset = templateProp != null ? (int)templateProp.floatValue : -1;

            EditorGUI.BeginChangeCheck();

            // === タブバー ===
            EditorGUILayout.Space(2);
            using (new GUILayout.HorizontalScope())
            {
                // Settings タブ（左）
                Color prevColor = GUI.color;
                if (_currentTab == MainTab.Settings)
                    GUI.color = new Color(0.35f, 0.75f, 1f, 0.3f);
                if (GUILayout.Button("⚙ Settings / 設定", EditorStyles.miniButtonLeft, GUILayout.Height(28)))
                    _currentTab = MainTab.Settings;
                GUI.color = prevColor;

                // Template タブ（右）
                if (_currentTab == MainTab.Template)
                    GUI.color = new Color(0.35f, 0.75f, 1f, 0.3f);
                if (GUILayout.Button("📋 Template / テンプレート", EditorStyles.miniButtonRight, GUILayout.Height(28)))
                    _currentTab = MainTab.Template;
                GUI.color = prevColor;
            }
            EditorGUILayout.Space(6);

            if (_currentTab == MainTab.Template)
            {
                DrawTemplateTab(material, templateProp, currentPreset);
            }
            else
            {
                DrawSettingsTab(materialEditor, properties, material);
            }

            if (EditorGUI.EndChangeCheck())
            {
                materialEditor.PropertiesChanged();
            }
        }

        // ============================================
        // Settings Tab（全プロパティ）
        // ============================================
        private void DrawSettingsTab(MaterialEditor materialEditor, MaterialProperty[] properties, Material material)
        {
            // === 検索ボックス ===
            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("🔍", GUILayout.Width(20));
                _searchWord = EditorGUILayout.TextField(_searchWord, _searchStyle);
            }
            EditorGUILayout.Space(6);

            // ============================================================
            // Main
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

            _showStencil = Foldout("ステンシル / Stencil（ポータル・マスキング）", _showStencil);
            if (_showStencil)
            {
                EditorGUILayout.BeginVertical(_boxOuterStyle);
                EditorGUILayout.HelpBox("ステンシルは「特定の場所だけに描画する」ための機能です。\nポータル効果やマスキングに使います。", MessageType.Info);
                DrawLine();
                DrawProp(materialEditor, properties, "_StencilRef");
                DrawProp(materialEditor, properties, "_StencilComp");
                DrawProp(materialEditor, properties, "_StencilPass");
                EditorGUILayout.EndVertical();
            }

            _showRendering = Foldout("描画設定 / Rendering", _showRendering);
            if (_showRendering)
            {
                EditorGUILayout.BeginVertical(_boxOuterStyle);

                // === ブレンドモードプリセット ===
                EditorGUILayout.LabelField("表示方法 / Blend Mode", EditorStyles.miniLabel);

                using (new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("✨ 加算（光る）", EditorStyles.miniButton, GUILayout.Height(24), GUILayout.ExpandWidth(true)))
                        SetBlendMode(material, 5, 10);
                    if (GUILayout.Button("🌫️ 半透明", EditorStyles.miniButton, GUILayout.Height(24), GUILayout.ExpandWidth(true)))
                        SetBlendMode(material, 5, 6);
                }
                using (new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("⬛ 上書き（不透明）", EditorStyles.miniButton, GUILayout.Height(24), GUILayout.ExpandWidth(true)))
                        SetBlendMode(material, 1, 0);
                    if (GUILayout.Button("🎨 乗算（暗く）", EditorStyles.miniButton, GUILayout.Height(24), GUILayout.ExpandWidth(true)))
                        SetBlendMode(material, 2, 0);
                }
                using (new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("🔆 スクリーン（明るく）", EditorStyles.miniButton, GUILayout.Height(24), GUILayout.ExpandWidth(true)))
                        SetBlendMode(material, 4, 1);
                    if (GUILayout.Button("📖 乗算半透明", EditorStyles.miniButton, GUILayout.Height(24), GUILayout.ExpandWidth(true)))
                        SetBlendMode(material, 2, 6);
                }

                DrawLine();

                // クイック設定
                EditorGUILayout.LabelField("クイック設定 / Quick Set", EditorStyles.miniLabel);
                using (new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("🔴 カットアウト", EditorStyles.miniButton, GUILayout.Height(24), GUILayout.ExpandWidth(true)))
                    {
                        SetBlendMode(material, 1, 0);
                        if (material.HasProperty("_ZWrite")) material.SetFloat("_ZWrite", 1);
                    }
                    if (GUILayout.Button("🔵 透明（両面）", EditorStyles.miniButton, GUILayout.Height(24), GUILayout.ExpandWidth(true)))
                    {
                        SetBlendMode(material, 5, 6);
                        if (material.HasProperty("_ZWrite")) material.SetFloat("_ZWrite", 0);
                        if (material.HasProperty("_Cull")) material.SetFloat("_Cull", 0);
                    }
                }
                using (new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("🟢 光る（両面）", EditorStyles.miniButton, GUILayout.Height(24), GUILayout.ExpandWidth(true)))
                    {
                        SetBlendMode(material, 5, 10);
                        if (material.HasProperty("_ZWrite")) material.SetFloat("_ZWrite", 0);
                        if (material.HasProperty("_Cull")) material.SetFloat("_Cull", 0);
                    }
                    GUILayout.Space(0);
                }

                DrawLine();

                // === 黒透過トグル ===
                DrawToggleProp(materialEditor, properties, "_BLACKTRANSPARENCY_ON");
                if (IsOn(material, "_BLACKTRANSPARENCY_ON"))
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        DrawProp(materialEditor, properties, "_BlackThreshold");
                    }
                    using (new GUILayout.HorizontalScope())
                    {
                        DrawProp(materialEditor, properties, "_BlackSoftness");
                    }
                }

                DrawLine();

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
        }

        // ============================================
        // Template Tab（テンプレート選択のみ）
        // ============================================
        private void DrawTemplateTab(Material material, MaterialProperty templateProp, int currentPreset)
        {
            List<PresetConfig> allPresets = AllPresets;

            EditorGUILayout.LabelField("Select Template / テンプレートを選択", _presetLabelStyle);
            EditorGUILayout.Space(4);

            // プリセットボタン（グリッド）
            int columns = 3;
            for (int i = 0; i < allPresets.Count; i++)
            {
                if (i % columns == 0)
                    EditorGUILayout.BeginHorizontal();

                bool isActive = (currentPreset == i);
                Color prevColor = GUI.color;

                if (isActive)
                    GUI.color = new Color(0.35f, 0.75f, 1f, 0.3f);
                else
                    GUI.color = new Color(0.8f, 0.8f, 0.8f, 0.15f);

                if (GUILayout.Button(allPresets[i].name, EditorStyles.miniButton, GUILayout.Height(36)))
                {
                    currentPreset = i;
                    ApplyPreset(material, allPresets[i]);
                    if (templateProp != null)
                        templateProp.floatValue = i;
                }

                GUI.color = prevColor;

                if ((i + 1) % columns == 0 || i == allPresets.Count - 1)
                    EditorGUILayout.EndHorizontal();
            }

            // 選択中プリセットの詳細
            if (currentPreset >= 0 && currentPreset < allPresets.Count)
            {
                EditorGUILayout.Space(8);
                using (new GUILayout.VerticalScope(_boxOuterStyle))
                {
                    EditorGUILayout.LabelField(allPresets[currentPreset].name, _presetLabelStyle);
                    EditorGUILayout.Space(2);
                    EditorGUILayout.LabelField(allPresets[currentPreset].description, _descStyle);
                    EditorGUILayout.Space(4);

                    EditorGUILayout.LabelField("Enabled Features:", EditorStyles.miniLabel);
                    if (allPresets[currentPreset].features.Length > 0)
                    {
                        foreach (string f in allPresets[currentPreset].features)
                        {
                            string displayName = f.Replace("_ON", "").Replace("_", " ").Trim();
                            EditorGUILayout.LabelField("  ✓ " + displayName, EditorStyles.miniLabel);
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField("  (変更なし)", EditorStyles.miniLabel);
                    }
                }
            }

            EditorGUILayout.Space(8);
            DrawLine();

            // === カスタムテンプレート保存 ===
            EditorGUILayout.LabelField("Save Current as Template / 現在の設定をテンプレートに保存", _presetLabelStyle);
            EditorGUILayout.Space(2);
            using (new GUILayout.HorizontalScope())
            {
                _newPresetName = EditorGUILayout.TextField("Name", _newPresetName);
                if (GUILayout.Button("Save", EditorStyles.miniButton, GUILayout.Width(60)))
                {
                    if (!string.IsNullOrEmpty(_newPresetName.Trim()))
                    {
                        SaveCustomPreset(_newPresetName.Trim(), material);
                        _newPresetName = "";
                        EditorUtility.DisplayDialog("MoruToon", "テンプレートを保存しました！\nTemplate saved!", "OK");
                    }
                }
            }
            EditorGUILayout.HelpBox("現在の設定（有効な機能・ブレンドモード等）をテンプレートとして保存できます。", MessageType.Info);

            // カスタムテンプレート削除
            if (_customPresets.Count > 0)
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField("Custom Templates / カスタムテンプレート", EditorStyles.miniLabel);
                for (int i = 0; i < _customPresets.Count; i++)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField(_customPresets[i].name, EditorStyles.miniLabel);
                        if (GUILayout.Button("Delete", EditorStyles.miniButton, GUILayout.Width(50)))
                        {
                            _customPresets.RemoveAt(i);
                            SaveCustomPresetsToDisk();
                            break;
                        }
                    }
                }
            }

            EditorGUILayout.Space(6);
            EditorGUILayout.HelpBox("テンプレートを選ぶと、機能が自動でONになります。\n細かい調整は「⚙ Settings」タブで行ってください。", MessageType.Info);
        }

        // ============================================
        // ApplyPreset（機能ON/OFF + ブレンドモード設定）
        // ============================================
        private void ApplyPreset(Material material, PresetConfig preset)
        {
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

            // ブレンドモード等
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
        private void SetBlendMode(Material material, float src, float dst)
        {
            if (material.HasProperty("_SrcBlend"))
                material.SetFloat("_SrcBlend", src);
            if (material.HasProperty("_DstBlend"))
                material.SetFloat("_DstBlend", dst);
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
