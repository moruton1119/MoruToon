#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace MoruToon.Editor
{
    /// <summary>
    /// MoruToon Custom ShaderGUI
    /// タブでテンプレートを選ぶと、プロパティがガラッと切り替わるUI
    /// </summary>
    public class MoruToonGUI : ShaderGUI
    {
        // ============================================
        // Template Definition
        // ============================================
        private enum Template
        {
            Basic,
            UVScroll,
            Dissolve,
            Flipbook,
            LayerBlend,
            SoftParticle,
            Stencil,
            ParticleFX,
            Custom
        }

        private static readonly string[] TabLabels =
        {
            "✨ Basic", "🔄 UV Scroll", "🔥 Dissolve", "📺 Flipbook",
            "🎭 Layer", "🫧 Soft", "🚪 Stencil", "🎆 FX", "⚙ Custom"
        };

        // 各テンプレートで有効にする機能
        private static readonly string[][] TemplateFeatures =
        {
            // Basic
            new[] { "_EMISSION_ON" },
            // UV Scroll
            new[] { "_UVSCROLL_ON", "_ROTATION_ON", "_EMISSION_ON" },
            // Dissolve
            new[] { "_DISSOLVE_ON", "_DISSOLVE_LIFETIME", "_EMISSION_ON" },
            // Flipbook
            new[] { "_FLIPBOOK_ON", "_EMISSION_ON" },
            // Layer Blend
            new[] { "_LAYERBLEND_ON", "_UVSCROLL_ON", "_EMISSION_ON" },
            // Soft Particle
            new[] { "_SOFTPARTICLES_ON", "_EMISSION_ON" },
            // Stencil
            new[] { "_DISSOLVE_ON", "_EMISSION_ON" },
            // Particle FX (フルセット)
            new[] { "_UVSCROLL_ON", "_EMISSION_ON", "_EMISSION_PULSE", "_SOFTPARTICLES_ON",
                    "_LIFETIMEFADE_ON", "_COLORRAMP_ON", "_HUESHIFT_ON" },
            // Custom
            new string[] { }
        };

        // 全トグル
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

        // スクロール用
        private Vector2 _scroll;
        // 前回のタブ（切り替わり検知用）
        private int _lastTab = -1;

        // タブのスタイル用
        private static GUIStyle _tabActiveStyle;
        private static GUIStyle _tabNormalStyle;
        private static GUIStyle _headerStyle;
        private static GUIStyle _boxStyle;

        private void InitStyles()
        {
            if (_tabActiveStyle == null)
            {
                _tabActiveStyle = new GUIStyle(EditorStyles.miniButtonMid);
                _tabActiveStyle.fontStyle = FontStyle.Bold;
                _tabActiveStyle.normal.textColor = new Color(0.35f, 0.75f, 1f);
                _tabActiveStyle.normal.background = MakeTex(2, 2, new Color(0.17f, 0.36f, 0.53f));

                _tabNormalStyle = new GUIStyle(EditorStyles.miniButtonMid);
                _tabNormalStyle.normal.textColor = new Color(0.65f, 0.65f, 0.65f);
            }
            if (_headerStyle == null)
            {
                _headerStyle = new GUIStyle(EditorStyles.boldLabel);
                _headerStyle.fontSize = 13;
                _headerStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f);
            }
            if (_boxStyle == null)
            {
                _boxStyle = new GUIStyle(GUI.skin.box);
                _boxStyle.normal.background = MakeTex(2, 2, new Color(0.22f, 0.22f, 0.22f, 0.5f));
            }
        }

        private Texture2D MakeTex(int w, int h, Color col)
        {
            Color[] pix = new Color[w * h];
            for (int i = 0; i < pix.Length; i++) pix[i] = col;
            Texture2D tex = new Texture2D(w, h);
            tex.SetPixels(pix);
            tex.Apply();
            return tex;
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            Material material = materialEditor.target as Material;
            if (material == null) return;

            InitStyles();

            MaterialProperty templateProp = FindProperty("_TemplateMode", properties, false);
            int currentTab = templateProp != null ? (int)templateProp.floatValue : 0;

            EditorGUI.BeginChangeCheck();

            // ============================================
            // タブバー（上部固定）
            // ============================================
            EditorGUILayout.Space(2);
            using (new GUILayout.HorizontalScope())
            {
                for (int i = 0; i < TabLabels.Length; i++)
                {
                    bool active = (currentTab == i);
                    GUIStyle style = active ? _tabActiveStyle : _tabNormalStyle;
                    if (GUILayout.Button(TabLabels[i], style, GUILayout.Height(28)))
                    {
                        currentTab = i;
                        ApplyTemplate(material, i);
                        if (templateProp != null)
                            templateProp.floatValue = i;
                    }
                }
            }
            EditorGUILayout.Space(4);

            // ============================================
            // プロパティ領域（スクロール可）
            // ============================================
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            Template tpl = (Template)currentTab;

            // --- 共通: Main ---
            DrawHeader("🎨 Main");
            DrawProp(materialEditor, properties, "_Color");
            DrawProp(materialEditor, properties, "_MainTex");
            DrawProp(materialEditor, properties, "_Brightness");

            // テンプレートごとのプロパティ表示
            switch (tpl)
            {
                case Template.Basic:
                    DrawBasicTemplate(materialEditor, properties, material);
                    break;
                case Template.UVScroll:
                    DrawUVScrollTemplate(materialEditor, properties, material);
                    break;
                case Template.Dissolve:
                    DrawDissolveTemplate(materialEditor, properties, material);
                    break;
                case Template.Flipbook:
                    DrawFlipbookTemplate(materialEditor, properties, material);
                    break;
                case Template.LayerBlend:
                    DrawLayerBlendTemplate(materialEditor, properties, material);
                    break;
                case Template.SoftParticle:
                    DrawSoftParticleTemplate(materialEditor, properties, material);
                    break;
                case Template.Stencil:
                    DrawStencilTemplate(materialEditor, properties, material);
                    break;
                case Template.ParticleFX:
                    DrawParticleFXTemplate(materialEditor, properties, material);
                    break;
                case Template.Custom:
                    DrawCustomTemplate(materialEditor, properties, material);
                    break;
            }

            // --- 共通: Rendering Options ---
            EditorGUILayout.Space(6);
            DrawHeader("⚙ Rendering Options");
            using (new GUILayout.VerticalScope(_boxStyle))
            {
                DrawProp(materialEditor, properties, "_SrcBlend");
                DrawProp(materialEditor, properties, "_DstBlend");
                DrawProp(materialEditor, properties, "_ZWrite");
                DrawProp(materialEditor, properties, "_Cull");
            }

            EditorGUILayout.EndScrollView();

            if (EditorGUI.EndChangeCheck())
            {
                materialEditor.PropertiesChanged();
            }
        }

        // ============================================
        // Template Drawing Methods
        // ============================================

        private void DrawBasicTemplate(MaterialEditor ed, MaterialProperty[] props, Material mat)
        {
            EditorGUILayout.Space(4);
            DrawHeader("✨ Emission");
            using (new GUILayout.VerticalScope(_boxStyle))
            {
                DrawToggleProp(ed, props, "_EMISSION_ON");
                if (IsOn(mat, "_EMISSION_ON"))
                {
                    DrawProp(ed, props, "_EmissionMap");
                    DrawProp(ed, props, "_EmissionColor");
                }
            }
        }

        private void DrawUVScrollTemplate(MaterialEditor ed, MaterialProperty[] props, Material mat)
        {
            EditorGUILayout.Space(4);
            DrawHeader("🔄 UV Scroll");
            using (new GUILayout.VerticalScope(_boxStyle))
            {
                DrawProp(ed, props, "_ScrollSpeedU");
                DrawProp(ed, props, "_ScrollSpeedV");
            }

            DrawHeader("🔁 Rotation");
            using (new GUILayout.VerticalScope(_boxStyle))
            {
                DrawProp(ed, props, "_RotationSpeed");
            }

            if (IsOn(mat, "_EMISSION_ON"))
            {
                DrawHeader("✨ Emission");
                using (new GUILayout.VerticalScope(_boxStyle))
                {
                    DrawProp(ed, props, "_EmissionMap");
                    DrawProp(ed, props, "_EmissionColor");
                    DrawToggleProp(ed, props, "_EMISSION_PULSE");
                    if (IsOn(mat, "_EMISSION_PULSE"))
                    {
                        DrawProp(ed, props, "_PulseSpeed");
                        DrawProp(ed, props, "_PulseMin");
                        DrawProp(ed, props, "_PulseMax");
                    }
                }
            }
        }

        private void DrawDissolveTemplate(MaterialEditor ed, MaterialProperty[] props, Material mat)
        {
            EditorGUILayout.Space(4);
            DrawHeader("🔥 Dissolve");
            using (new GUILayout.VerticalScope(_boxStyle))
            {
                DrawToggleProp(ed, props, "_DISSOLVE_LIFETIME");
                DrawProp(ed, props, "_DissolveTex");
                if (!IsOn(mat, "_DISSOLVE_LIFETIME"))
                    DrawProp(ed, props, "_DissolveAmount");
                else
                {
                    DrawProp(ed, props, "_DissolveDelay");
                    DrawProp(ed, props, "_DissolveSpeed");
                }
                DrawProp(ed, props, "_DissolveEdgeWidth");
                DrawProp(ed, props, "_DissolveEdgeColor");
            }

            if (IsOn(mat, "_EMISSION_ON"))
            {
                DrawHeader("✨ Emission");
                using (new GUILayout.VerticalScope(_boxStyle))
                {
                    DrawProp(ed, props, "_EmissionMap");
                    DrawProp(ed, props, "_EmissionColor");
                }
            }
        }

        private void DrawFlipbookTemplate(MaterialEditor ed, MaterialProperty[] props, Material mat)
        {
            EditorGUILayout.Space(4);
            DrawHeader("📺 Flipbook Animation");
            using (new GUILayout.VerticalScope(_boxStyle))
            {
                DrawProp(ed, props, "_FlipbookTilesX");
                DrawProp(ed, props, "_FlipbookTilesY");
                DrawProp(ed, props, "_FlipbookFPS");
                DrawProp(ed, props, "_FlipbookBlend");
            }

            if (IsOn(mat, "_EMISSION_ON"))
            {
                DrawHeader("✨ Emission");
                using (new GUILayout.VerticalScope(_boxStyle))
                {
                    DrawProp(ed, props, "_EmissionMap");
                    DrawProp(ed, props, "_EmissionColor");
                }
            }
        }

        private void DrawLayerBlendTemplate(MaterialEditor ed, MaterialProperty[] props, Material mat)
        {
            EditorGUILayout.Space(4);
            DrawHeader("🎭 Layer Blend");
            using (new GUILayout.VerticalScope(_boxStyle))
            {
                DrawProp(ed, props, "_SubTex");
                DrawProp(ed, props, "_BlendMode");
                DrawProp(ed, props, "_SubIntensity");
                DrawProp(ed, props, "_SubScrollSpeedU");
                DrawProp(ed, props, "_SubScrollSpeedV");
            }

            if (IsOn(mat, "_UVSCROLL_ON"))
            {
                DrawHeader("🔄 UV Scroll");
                using (new GUILayout.VerticalScope(_boxStyle))
                {
                    DrawProp(ed, props, "_ScrollSpeedU");
                    DrawProp(ed, props, "_ScrollSpeedV");
                }
            }
        }

        private void DrawSoftParticleTemplate(MaterialEditor ed, MaterialProperty[] props, Material mat)
        {
            EditorGUILayout.Space(4);
            DrawHeader("🫧 Soft Particle");
            using (new GUILayout.VerticalScope(_boxStyle))
            {
                DrawProp(ed, props, "_SoftDistance");
            }

            if (IsOn(mat, "_EMISSION_ON"))
            {
                DrawHeader("✨ Emission");
                using (new GUILayout.VerticalScope(_boxStyle))
                {
                    DrawProp(ed, props, "_EmissionMap");
                    DrawProp(ed, props, "_EmissionColor");
                }
            }
        }

        private void DrawStencilTemplate(MaterialEditor ed, MaterialProperty[] props, Material mat)
        {
            EditorGUILayout.Space(4);
            DrawHeader("🚪 Stencil");
            using (new GUILayout.VerticalScope(_boxStyle))
            {
                DrawProp(ed, props, "_StencilRef");
                DrawProp(ed, props, "_StencilComp");
                DrawProp(ed, props, "_StencilPass");
            }

            if (IsOn(mat, "_DISSOLVE_ON"))
            {
                DrawHeader("🔥 Dissolve");
                using (new GUILayout.VerticalScope(_boxStyle))
                {
                    DrawProp(ed, props, "_DissolveTex");
                    DrawProp(ed, props, "_DissolveAmount");
                    DrawProp(ed, props, "_DissolveEdgeWidth");
                    DrawProp(ed, props, "_DissolveEdgeColor");
                }
            }
        }

        private void DrawParticleFXTemplate(MaterialEditor ed, MaterialProperty[] props, Material mat)
        {
            EditorGUILayout.Space(4);
            DrawHeader("🔄 UV Scroll");
            using (new GUILayout.VerticalScope(_boxStyle))
            {
                DrawProp(ed, props, "_ScrollSpeedU");
                DrawProp(ed, props, "_ScrollSpeedV");
                DrawProp(ed, props, "_RotationSpeed");
            }

            DrawHeader("✨ Emission");
            using (new GUILayout.VerticalScope(_boxStyle))
            {
                DrawProp(ed, props, "_EmissionMap");
                DrawProp(ed, props, "_EmissionColor");
                DrawToggleProp(ed, props, "_EMISSION_PULSE");
                if (IsOn(mat, "_EMISSION_PULSE"))
                {
                    DrawProp(ed, props, "_PulseSpeed");
                    DrawProp(ed, props, "_PulseMin");
                    DrawProp(ed, props, "_PulseMax");
                }
            }

            DrawHeader("🫧 Soft Particle");
            using (new GUILayout.VerticalScope(_boxStyle))
            {
                DrawProp(ed, props, "_SoftDistance");
            }

            DrawHeader("⏳ Lifetime Fade");
            using (new GUILayout.VerticalScope(_boxStyle))
            {
                DrawProp(ed, props, "_LifetimeFadeIn");
                DrawProp(ed, props, "_LifetimeFadeOut");
            }

            DrawHeader("🎨 Color Ramp");
            using (new GUILayout.VerticalScope(_boxStyle))
            {
                DrawProp(ed, props, "_ColorRampTex");
                DrawProp(ed, props, "_ColorRampIntensity");
            }

            DrawHeader("🌈 HUE Shift");
            using (new GUILayout.VerticalScope(_boxStyle))
            {
                DrawProp(ed, props, "_HueShift");
                DrawProp(ed, props, "_HueShiftSpeed");
            }
        }

        private void DrawCustomTemplate(MaterialEditor ed, MaterialProperty[] props, Material mat)
        {
            EditorGUILayout.Space(4);
            DrawHeader("⚙ All Features (Custom)");

            // トグルでON/OFF → 有効なセクションだけ表示
            foreach (string feature in AllFeatures)
            {
                DrawToggleProp(ed, props, feature);
            }

            EditorGUILayout.Space(6);

            // 有効になっている機能のプロパティを表示
            if (IsOn(mat, "_UVSCROLL_ON"))
            {
                DrawHeader("🔄 UV Scroll");
                using (new GUILayout.VerticalScope(_boxStyle))
                {
                    DrawProp(ed, props, "_ScrollSpeedU");
                    DrawProp(ed, props, "_ScrollSpeedV");
                }
            }
            if (IsOn(mat, "_ROTATION_ON"))
            {
                DrawHeader("🔁 Rotation");
                using (new GUILayout.VerticalScope(_boxStyle))
                {
                    DrawProp(ed, props, "_RotationSpeed");
                }
            }
            if (IsOn(mat, "_EMISSION_ON"))
            {
                DrawHeader("✨ Emission");
                using (new GUILayout.VerticalScope(_boxStyle))
                {
                    DrawProp(ed, props, "_EmissionMap");
                    DrawProp(ed, props, "_EmissionColor");
                    DrawToggleProp(ed, props, "_EMISSION_PULSE");
                    if (IsOn(mat, "_EMISSION_PULSE"))
                    {
                        DrawProp(ed, props, "_PulseSpeed");
                        DrawProp(ed, props, "_PulseMin");
                        DrawProp(ed, props, "_PulseMax");
                    }
                }
            }
            if (IsOn(mat, "_DISSOLVE_ON"))
            {
                DrawHeader("🔥 Dissolve");
                using (new GUILayout.VerticalScope(_boxStyle))
                {
                    DrawToggleProp(ed, props, "_DISSOLVE_LIFETIME");
                    DrawProp(ed, props, "_DissolveTex");
                    DrawProp(ed, props, "_DissolveAmount");
                    DrawProp(ed, props, "_DissolveEdgeWidth");
                    DrawProp(ed, props, "_DissolveEdgeColor");
                    if (IsOn(mat, "_DISSOLVE_LIFETIME"))
                    {
                        DrawProp(ed, props, "_DissolveDelay");
                        DrawProp(ed, props, "_DissolveSpeed");
                    }
                }
            }
            if (IsOn(mat, "_FLIPBOOK_ON"))
            {
                DrawHeader("📺 Flipbook");
                using (new GUILayout.VerticalScope(_boxStyle))
                {
                    DrawProp(ed, props, "_FlipbookTilesX");
                    DrawProp(ed, props, "_FlipbookTilesY");
                    DrawProp(ed, props, "_FlipbookFPS");
                    DrawProp(ed, props, "_FlipbookBlend");
                }
            }
            if (IsOn(mat, "_LAYERBLEND_ON"))
            {
                DrawHeader("🎭 Layer Blend");
                using (new GUILayout.VerticalScope(_boxStyle))
                {
                    DrawProp(ed, props, "_SubTex");
                    DrawProp(ed, props, "_BlendMode");
                    DrawProp(ed, props, "_SubIntensity");
                    DrawProp(ed, props, "_SubScrollSpeedU");
                    DrawProp(ed, props, "_SubScrollSpeedV");
                }
            }
            if (IsOn(mat, "_SOFTPARTICLES_ON"))
            {
                DrawHeader("🫧 Soft Particle");
                using (new GUILayout.VerticalScope(_boxStyle))
                {
                    DrawProp(ed, props, "_SoftDistance");
                }
            }
            if (IsOn(mat, "_COLORRAMP_ON"))
            {
                DrawHeader("🎨 Color Ramp");
                using (new GUILayout.VerticalScope(_boxStyle))
                {
                    DrawProp(ed, props, "_ColorRampTex");
                    DrawProp(ed, props, "_ColorRampIntensity");
                }
            }
            if (IsOn(mat, "_HUESHIFT_ON"))
            {
                DrawHeader("🌈 HUE Shift");
                using (new GUILayout.VerticalScope(_boxStyle))
                {
                    DrawProp(ed, props, "_HueShift");
                    DrawProp(ed, props, "_HueShiftSpeed");
                }
            }
            if (IsOn(mat, "_DISTANCEFADE_ON"))
            {
                DrawHeader("📏 Distance Fade");
                using (new GUILayout.VerticalScope(_boxStyle))
                {
                    DrawProp(ed, props, "_FadeNear");
                    DrawProp(ed, props, "_FadeFar");
                }
            }
            if (IsOn(mat, "_LIFETIMEFADE_ON"))
            {
                DrawHeader("⏳ Lifetime Fade");
                using (new GUILayout.VerticalScope(_boxStyle))
                {
                    DrawProp(ed, props, "_LifetimeFadeIn");
                    DrawProp(ed, props, "_LifetimeFadeOut");
                }
            }
            if (IsOn(mat, "_MASK_ON"))
            {
                DrawHeader("🎭 Mask");
                using (new GUILayout.VerticalScope(_boxStyle))
                {
                    DrawProp(ed, props, "_VisibleMaskTex");
                    DrawProp(ed, props, "_VisibleMaskStrength");
                    DrawProp(ed, props, "_HideMaskTex");
                    DrawProp(ed, props, "_HideMaskStrength");
                }
            }
        }

        // ============================================
        // Helper Methods
        // ============================================

        private void ApplyTemplate(Material material, int templateIndex)
        {
            // 全オフ
            foreach (string t in AllFeatures)
            {
                if (material.HasProperty(t))
                    material.SetFloat(t, 0);
            }

            // テンプレートの機能をオン
            foreach (string t in TemplateFeatures[templateIndex])
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

            EditorUtility.SetDirty(material);
        }

        private bool IsOn(Material mat, string prop)
        {
            return mat.HasProperty(prop) && mat.GetFloat(prop) > 0.5f;
        }

        private void DrawHeader(string title)
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField(title, _headerStyle);
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
