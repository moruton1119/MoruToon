#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace MoruToon.Editor
{
    /// <summary>
    /// MoruToon Custom ShaderGUI
    /// Template tab system for switching between feature presets.
    /// </summary>
    public class MoruToonGUI : ShaderGUI
    {
        private enum TemplateMode
        {
            Basic = 0,
            UVScroll = 1,
            Dissolve = 2,
            Flipbook = 3,
            LayerBlend = 4,
            SoftParticle = 5,
            StencilPortal = 6,
            ParticleFX = 7,
            Custom = 8
        }

        private static readonly string[] TemplateLabels =
        {
            "Basic", "UV Scroll", "Dissolve", "Flipbook",
            "Layer Blend", "Soft Particle", "Stencil Portal",
            "Particle FX", "Custom"
        };

        // Toggle property names per template
        private static readonly string[][] TemplateToggles =
        {
            // Basic
            new string[] { },
            // UV Scroll (UV Scroll + Emission + Rotation)
            new string[] { "_UVSCROLL_ON", "_ROTATION_ON", "_EMISSION_ON" },
            // Dissolve (Dissolve + Emission + Lifetime)
            new string[] { "_DISSOLVE_ON", "_DISSOLVE_LIFETIME", "_EMISSION_ON" },
            // Flipbook
            new string[] { "_FLIPBOOK_ON", "_EMISSION_ON" },
            // Layer Blend (Layer + UV Scroll + Emission)
            new string[] { "_LAYERBLEND_ON", "_UVSCROLL_ON", "_EMISSION_ON" },
            // Soft Particle
            new string[] { "_SOFTPARTICLES_ON", "_EMISSION_ON" },
            // Stencil Portal (Stencil + Dissolve)
            new string[] { "_STENCIL_ON", "_DISSOLVE_ON" },
            // Particle FX (UV Scroll + Emission + Soft Particle + Lifetime + Color Ramp + HUE Shift)
            new string[] { "_UVSCROLL_ON", "_EMISSION_ON", "_SOFTPARTICLES_ON", "_LIFETIMEFADE_ON", "_COLORRAMP_ON", "_HUESHIFT_ON" },
            // Custom (nothing auto-enabled)
            new string[] { }
        };

        // All toggle keywords
        private static readonly string[] AllToggles =
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
            "_STENCIL_ON"
        };

        // Scroll position for the properties area
        private Vector2 scrollPos;

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            Material material = materialEditor.target as Material;
            if (material == null) return;

            MaterialProperty templateProp = FindProperty("_TemplateMode", properties, false);

            EditorGUI.BeginChangeCheck();

            // === Template Tab Buttons ===
            EditorGUILayout.LabelField("Template", EditorStyles.boldLabel);
            int currentMode = templateProp != null ? (int)templateProp.floatValue : 0;

            int columns = 4;
            for (int i = 0; i < TemplateLabels.Length; i++)
            {
                if (i % columns == 0)
                    EditorGUILayout.BeginHorizontal();

                bool isActive = (currentMode == i);
                Color prevColor = GUI.color;
                if (isActive)
                    GUI.color = new Color(0.3f, 0.8f, 1f);

                if (GUILayout.Button(TemplateLabels[i], isActive ? EditorStyles.miniButtonMid : EditorStyles.miniButtonLeft, GUILayout.Height(24)))
                {
                    currentMode = i;
                    ApplyTemplate(material, i);
                    if (templateProp != null)
                        templateProp.floatValue = i;
                }

                GUI.color = prevColor;

                if ((i + 1) % columns == 0 || i == TemplateLabels.Length - 1)
                    EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(4);

            // === Properties ===
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.MinHeight(400));

            EditorGUILayout.LabelField("Main", EditorStyles.boldLabel);
            DrawProps(materialEditor, properties, "_Color", "_MainTex", "_Brightness");

            // UV Scroll
            if (IsEnabled(material, "_UVSCROLL_ON"))
            {
                Space();
                EditorGUILayout.LabelField("UV Scroll", EditorStyles.boldLabel);
                DrawProps(materialEditor, properties, "_ScrollSpeedU", "_ScrollSpeedV");
            }

            // Rotation
            if (IsEnabled(material, "_ROTATION_ON"))
            {
                Space();
                EditorGUILayout.LabelField("Rotation", EditorStyles.boldLabel);
                DrawProps(materialEditor, properties, "_RotationSpeed");
            }

            // Emission
            if (IsEnabled(material, "_EMISSION_ON"))
            {
                Space();
                EditorGUILayout.LabelField("Emission", EditorStyles.boldLabel);
                DrawProps(materialEditor, properties, "_EmissionMap", "_EmissionColor");
                DrawToggleProp(materialEditor, properties, "_EMISSION_PULSE");
                if (IsEnabled(material, "_EMISSION_PULSE"))
                    DrawProps(materialEditor, properties, "_PulseSpeed", "_PulseMin", "_PulseMax");
            }

            // Dissolve
            if (IsEnabled(material, "_DISSOLVE_ON"))
            {
                Space();
                EditorGUILayout.LabelField("Dissolve", EditorStyles.boldLabel);
                DrawToggleProp(materialEditor, properties, "_DISSOLVE_LIFETIME");
                DrawProps(materialEditor, properties, "_DissolveTex", "_DissolveAmount", "_DissolveEdgeWidth", "_DissolveEdgeColor");
                if (IsEnabled(material, "_DISSOLVE_LIFETIME"))
                    DrawProps(materialEditor, properties, "_DissolveDelay", "_DissolveSpeed");
            }

            // Flipbook
            if (IsEnabled(material, "_FLIPBOOK_ON"))
            {
                Space();
                EditorGUILayout.LabelField("Flipbook", EditorStyles.boldLabel);
                DrawProps(materialEditor, properties, "_FlipbookTilesX", "_FlipbookTilesY", "_FlipbookFPS", "_FlipbookBlend");
            }

            // Layer Blend
            if (IsEnabled(material, "_LAYERBLEND_ON"))
            {
                Space();
                EditorGUILayout.LabelField("Layer Blend", EditorStyles.boldLabel);
                DrawProps(materialEditor, properties, "_SubTex", "_BlendMode", "_SubIntensity", "_SubScrollSpeedU", "_SubScrollSpeedV");
            }

            // Color Ramp
            if (IsEnabled(material, "_COLORRAMP_ON"))
            {
                Space();
                EditorGUILayout.LabelField("Color Ramp", EditorStyles.boldLabel);
                DrawProps(materialEditor, properties, "_ColorRampTex", "_ColorRampIntensity");
            }

            // HUE Shift
            if (IsEnabled(material, "_HUESHIFT_ON"))
            {
                Space();
                EditorGUILayout.LabelField("HUE Shift", EditorStyles.boldLabel);
                DrawProps(materialEditor, properties, "_HueShift", "_HueShiftSpeed");
            }

            // Mask
            if (IsEnabled(material, "_MASK_ON"))
            {
                Space();
                EditorGUILayout.LabelField("Mask", EditorStyles.boldLabel);
                DrawProps(materialEditor, properties, "_VisibleMaskTex", "_VisibleMaskStrength", "_HideMaskTex", "_HideMaskStrength");
            }

            // Particle Lifetime Fade
            if (IsEnabled(material, "_LIFETIMEFADE_ON"))
            {
                Space();
                EditorGUILayout.LabelField("Particle Lifetime Fade", EditorStyles.boldLabel);
                DrawProps(materialEditor, properties, "_LifetimeFadeIn", "_LifetimeFadeOut");
            }

            // Distance Fade
            if (IsEnabled(material, "_DISTANCEFADE_ON"))
            {
                Space();
                EditorGUILayout.LabelField("Distance Fade", EditorStyles.boldLabel);
                DrawProps(materialEditor, properties, "_FadeNear", "_FadeFar");
            }

            // Soft Particle
            if (IsEnabled(material, "_SOFTPARTICLES_ON"))
            {
                Space();
                EditorGUILayout.LabelField("Soft Particle", EditorStyles.boldLabel);
                DrawProps(materialEditor, properties, "_SoftDistance");
            }

            // Stencil
            if (IsEnabled(material, "_STENCIL_ON"))
            {
                Space();
                EditorGUILayout.LabelField("Stencil", EditorStyles.boldLabel);
                DrawProps(materialEditor, properties, "_StencilMode", "_StencilRef", "_StencilComp", "_StencilPass");
            }

            // Rendering Options
            Space();
            EditorGUILayout.LabelField("Rendering Options", EditorStyles.boldLabel);
            DrawProps(materialEditor, properties, "_SrcBlend", "_DstBlend", "_ZWrite", "_Cull");

            // Feature Toggles (Advanced)
            Space();
            EditorGUILayout.LabelField("Feature Toggles", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                foreach (string toggle in AllToggles)
                {
                    DrawToggleProp(materialEditor, properties, toggle);
                }
            }

            EditorGUILayout.EndScrollView();

            if (EditorGUI.EndChangeCheck())
            {
                materialEditor.PropertiesChanged();
            }
        }

        private void ApplyTemplate(Material material, int templateIndex)
        {
            // Disable all feature toggles
            foreach (string toggle in AllToggles)
            {
                if (material.HasProperty(toggle))
                    material.SetFloat(toggle, 0);
            }

            // Enable toggles for the selected template
            foreach (string toggle in TemplateToggles[templateIndex])
            {
                if (material.HasProperty(toggle))
                    material.SetFloat(toggle, 1);
            }

            // Sync shader keywords
            foreach (string toggle in AllToggles)
            {
                bool enabled = material.HasProperty(toggle) && material.GetFloat(toggle) > 0.5f;
                if (enabled)
                    material.EnableKeyword(toggle);
                else
                    material.DisableKeyword(toggle);
            }

            EditorUtility.SetDirty(material);
        }

        private bool IsEnabled(Material material, string toggleName)
        {
            return material.HasProperty(toggleName) && material.GetFloat(toggleName) > 0.5f;
        }

        private void Space(int height = 8)
        {
            EditorGUILayout.Space(height);
        }

        private void DrawProps(MaterialEditor editor, MaterialProperty[] properties, params string[] propNames)
        {
            foreach (string propName in propNames)
            {
                MaterialProperty prop = FindProperty(propName, properties, false);
                if (prop != null)
                    editor.ShaderProperty(prop, prop.displayName);
            }
        }

        private void DrawToggleProp(MaterialEditor editor, MaterialProperty[] properties, string propName)
        {
            MaterialProperty prop = FindProperty(propName, properties, false);
            if (prop != null)
            {
                // Sync keyword with property value
                Material material = editor.target as Material;
                if (material != null)
                {
                    bool currentVal = material.GetFloat(propName) > 0.5f;
                    EditorGUI.BeginChangeCheck();
                    editor.ShaderProperty(prop, prop.displayName);
                    if (EditorGUI.EndChangeCheck())
                    {
                        bool newVal = material.GetFloat(propName) > 0.5f;
                        if (newVal)
                            material.EnableKeyword(propName);
                        else
                            material.DisableKeyword(propName);
                    }
                }
            }
        }
    }
}
#endif
