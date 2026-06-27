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
        // Template modes
        private enum TemplateMode
        {
            Basic = 0,
            UVScroll = 1,
            Dissolve = 2,
            Flipbook = 3,
            LayerBlend = 4,
            SoftParticle = 5,
            Custom = 6
        }

        // Template labels
        private static readonly string[] TemplateLabels =
        {
            "Basic", "UV Scroll", "Dissolve", "Flipbook", "Layer Blend", "Soft Particle", "Custom"
        };

        // Toggle property names per template
        private static readonly string[][] TemplateToggles =
        {
            // Basic
            new string[] { },
            // UV Scroll
            new string[] { "_UVSCROLL_ON", "_EMISSION_ON" },
            // Dissolve
            new string[] { "_DISSOLVE_ON", "_EMISSION_ON" },
            // Flipbook
            new string[] { "_FLIPBOOK_ON" },
            // Layer Blend
            new string[] { "_LAYERBLEND_ON", "_UVSCROLL_ON" },
            // Soft Particle
            new string[] { "_SOFTPARTICLES_ON" },
            // Custom (all off, user toggles manually)
            new string[] { }
        };

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            Material material = materialEditor.target as Material;
            if (material == null) return;

            // Find the template mode property
            MaterialProperty templateProp = FindProperty("_TemplateMode", properties, false);

            EditorGUI.BeginChangeCheck();

            // Draw template tab buttons
            GUILayout.Label("Template", EditorStyles.boldLabel);
            int currentMode = templateProp != null ? (int)templateProp.floatValue : 0;

            using (new GUILayout.HorizontalScope())
            {
                for (int i = 0; i < TemplateLabels.Length; i++)
                {
                    bool isActive = (currentMode == i);
                    GUI.color = isActive ? new Color(0.3f, 0.8f, 1f) : Color.white;

                    if (GUILayout.Button(TemplateLabels[i], isActive ? EditorStyles.miniButtonMid : EditorStyles.miniButton))
                    {
                        currentMode = i;
                        ApplyTemplate(material, i);
                        if (templateProp != null)
                            templateProp.floatValue = i;
                    }
                }
            }
            GUI.color = Color.white;

            GUILayout.Space(8);

            // Draw properties based on active features
            GUILayout.Label("Properties", EditorStyles.boldLabel);

            // Always show: Main section
            DrawSection(materialEditor, properties, "Main", new string[]
            {
                "_Color", "_MainTex", "_Brightness"
            });

            // Rendering Options
            DrawSection(materialEditor, properties, "Rendering Options", new string[]
            {
                "_SrcBlend", "_DstBlend", "_ZWrite", "_Cull"
            });

            // UV Scroll
            if (IsFeatureEnabled(material, "_UVSCROLL_ON"))
            {
                DrawSection(materialEditor, properties, "UV Scroll", new string[]
                {
                    "_ScrollSpeedU", "_ScrollSpeedV"
                });
            }

            // Emission
            if (IsFeatureEnabled(material, "_EMISSION_ON"))
            {
                DrawSection(materialEditor, properties, "Emission", new string[]
                {
                    "_EmissionMap", "_EmissionColor", "_EMISSION_PULSE", "_PulseSpeed", "_PulseMin", "_PulseMax"
                });
            }

            // Dissolve
            if (IsFeatureEnabled(material, "_DISSOLVE_ON"))
            {
                DrawSection(materialEditor, properties, "Dissolve", new string[]
                {
                    "_DissolveTex", "_DissolveAmount", "_DissolveEdgeWidth", "_DissolveEdgeColor"
                });
            }

            // Flipbook
            if (IsFeatureEnabled(material, "_FLIPBOOK_ON"))
            {
                DrawSection(materialEditor, properties, "Flipbook", new string[]
                {
                    "_FlipbookTilesX", "_FlipbookTilesY", "_FlipbookFPS", "_FlipbookBlend"
                });
            }

            // Layer Blend
            if (IsFeatureEnabled(material, "_LAYERBLEND_ON"))
            {
                DrawSection(materialEditor, properties, "Layer Blend", new string[]
                {
                    "_SubTex", "_BlendMode", "_SubIntensity"
                });
            }

            // Soft Particle
            if (IsFeatureEnabled(material, "_SOFTPARTICLES_ON"))
            {
                DrawSection(materialEditor, properties, "Soft Particle", new string[]
                {
                    "_SoftDistance"
                });
            }

            if (EditorGUI.EndChangeCheck())
            {
                materialEditor.PropertiesChanged();
            }
        }

        private void ApplyTemplate(Material material, int templateIndex)
        {
            // Disable all feature toggles first
            string[] allToggles = { "_UVSCROLL_ON", "_EMISSION_ON", "_EMISSION_PULSE", "_DISSOLVE_ON", "_FLIPBOOK_ON", "_LAYERBLEND_ON", "_SOFTPARTICLES_ON" };
            foreach (string toggle in allToggles)
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

            // Set shader keywords to match
            foreach (string toggle in allToggles)
            {
                bool enabled = material.HasProperty(toggle) && material.GetFloat(toggle) > 0.5f;
                string keyword = toggle.Replace("_ON", "_ON");
                if (enabled)
                    material.EnableKeyword(keyword);
                else
                    material.DisableKeyword(keyword);
            }

            EditorUtility.SetDirty(material);
        }

        private bool IsFeatureEnabled(Material material, string toggleName)
        {
            return material.HasProperty(toggleName) && material.GetFloat(toggleName) > 0.5f;
        }

        private void DrawSection(MaterialEditor editor, MaterialProperty[] properties, string title, string[] propertyNames)
        {
            GUILayout.Space(4);
            GUILayout.Label(title, EditorStyles.boldLabel);

            foreach (string propName in propertyNames)
            {
                MaterialProperty prop = FindProperty(propName, properties, false);
                if (prop != null)
                {
                    editor.ShaderProperty(prop, prop.displayName);
                }
            }
        }
    }
}
#endif
