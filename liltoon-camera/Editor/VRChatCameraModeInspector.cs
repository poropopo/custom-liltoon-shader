#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace lilToon
{
    public class VRChatCameraModeInspector : lilToonInspector
    {
        private MaterialProperty vrFilterCombineMode;
        private MaterialProperty vrCameraFilterBehavior;
        private MaterialProperty vrCameraModeEnable;
        private MaterialProperty vrCameraModeTarget;
        private MaterialProperty vrCameraModeMask;
        private MaterialProperty vrCameraMaskEnable;
        private MaterialProperty vrCameraMaskCompareMode;
        private MaterialProperty vrCameraMask;
        private MaterialProperty vrMirrorFilterBehavior;
        private MaterialProperty vrMirrorModeEnable;
        private MaterialProperty vrMirrorModeMask;

        private static bool isShowVRVisibilityFilters;
        private static readonly Dictionary<string, Shader> shaderCache = new Dictionary<string, Shader>();
        private const string shaderName = "VRChatCameraMode";

        protected override void LoadCustomProperties(MaterialProperty[] props, Material material)
        {
            isCustomShader = true;

            ReplaceToCustomShaders();
            isShowRenderMode = !material.shader.name.Contains("Optional");

            vrFilterCombineMode = FindProperty("_VRFilterCombineMode", props);
            vrCameraFilterBehavior = FindProperty("_VRCameraFilterBehavior", props);
            vrCameraModeEnable = FindProperty("_VRCameraModeEnable", props);
            vrCameraModeTarget = FindProperty("_VRCameraModeTarget", props);
            vrCameraModeMask = FindProperty("_VRCameraModeMask", props);
            vrCameraMaskEnable = FindProperty("_VRCameraMaskEnable", props);
            vrCameraMaskCompareMode = FindProperty("_VRCameraMaskCompareMode", props);
            vrCameraMask = FindProperty("_VRCameraMask", props);
            vrMirrorFilterBehavior = FindProperty("_VRMirrorFilterBehavior", props);
            vrMirrorModeEnable = FindProperty("_VRMirrorModeEnable", props);
            vrMirrorModeMask = FindProperty("_VRMirrorModeMask", props);
        }

        protected override void DrawCustomProperties(Material material)
        {
            isShowVRVisibilityFilters = Foldout("VRChat Visibility Filters", "VRChat Visibility Filters", isShowVRVisibilityFilters);
            if(!isShowVRVisibilityFilters) return;

            EditorGUILayout.BeginVertical(boxOuter);
            EditorGUILayout.LabelField("VRChat Filter Combine", customToggleFont);
            EditorGUILayout.BeginVertical(boxInnerHalf);

            m_MaterialEditor.ShaderProperty(vrFilterCombineMode, "Filter Combine Mode");

            EditorGUILayout.HelpBox(
                "Any Enabled Filter = OR. All Enabled Filters = AND.\n" +
                "Show On Match filters use this as an inclusion rule.\n" +
                "Hide On Match filters use this as an exclusion rule, and hiding takes priority.",
                MessageType.Info
            );

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical(boxOuter);
            EditorGUILayout.LabelField("VRChat Camera Mode", customToggleFont);
            EditorGUILayout.BeginVertical(boxInnerHalf);

            int previousCameraBehavior = Mathf.RoundToInt(vrCameraFilterBehavior.floatValue);
            bool wasCameraModeEnabled = vrCameraModeEnable.floatValue >= 0.5f;
            bool wasCameraMaskEnabled = vrCameraMaskEnable.floatValue >= 0.5f;

            m_MaterialEditor.ShaderProperty(vrCameraFilterBehavior, "Filter Action");

            EditorGUILayout.Space();

            m_MaterialEditor.ShaderProperty(vrCameraModeEnable, "Use Camera Mode Filter");
            using(new EditorGUI.DisabledScope(vrCameraModeEnable.floatValue < 0.5f))
            {
                DrawCameraModeMaskField();
            }

            EditorGUILayout.Space();

            m_MaterialEditor.ShaderProperty(vrCameraMaskEnable, "Use Camera Mask Filter");
            using(new EditorGUI.DisabledScope(vrCameraMaskEnable.floatValue < 0.5f))
            {
                m_MaterialEditor.ShaderProperty(vrCameraMaskCompareMode, "Camera Mask Match");
                DrawCameraMaskField();
            }

            AutoFixCameraMaskAfterStateChange(previousCameraBehavior, wasCameraModeEnabled, wasCameraMaskEnabled);

            EditorGUILayout.HelpBox(
                "_VRChatCameraMode values: 0 Normal, 1 VR Handheld, 2 Desktop Handheld, 3 Screenshot.\n" +
                "You can target multiple camera modes at once.\n" +
                "_VRChatCameraMask is the active camera cullingMask and is only valid when CameraMode is not 0.\n" +
                "Target Camera Mask uses a signed 32bit integer. Negative values are valid and preserve high bits.\n" +
                "Set Filter Combine Mode to Any Enabled Filter to hide on camera or mirror, and All Enabled Filters to require both.\n" +
                "Set Filter Action to Hide On Match to hide the material only on the selected camera.",
                MessageType.Info
            );

            if(HasMaskOnlyInvisibleConfiguration())
            {
                EditorGUILayout.HelpBox(
                    "Use Camera Mask Filter by itself with Show On Match and Target Camera Mask = 0 never matches.\n" +
                    "Set a non-zero 32bit mask, or use Fix to default to -1 (0xFFFFFFFF).",
                    MessageType.Warning
                );

                if(GUILayout.Button("Fix Target Camera Mask"))
                {
                    SetIntPropertyValue(vrCameraMask.name, -1);
                }
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical(boxOuter);
            EditorGUILayout.LabelField("VRChat Mirror Mode", customToggleFont);
            EditorGUILayout.BeginVertical(boxInnerHalf);

            m_MaterialEditor.ShaderProperty(vrMirrorFilterBehavior, "Mirror Filter Action");

            EditorGUILayout.Space();

            m_MaterialEditor.ShaderProperty(vrMirrorModeEnable, "Use Mirror Mode Filter");
            using(new EditorGUI.DisabledScope(vrMirrorModeEnable.floatValue < 0.5f))
            {
                DrawMirrorModeMaskField();
            }

            EditorGUILayout.HelpBox(
                "_VRChatMirrorMode values: 0 Normal, 1 VR Mirror, 2 Desktop Mirror.\n" +
                "Set Mirror Filter Action to Hide On Match to hide the material in mirrors.",
                MessageType.Info
            );

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();

            if(HasImpossibleAllFiltersCombination())
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox(
                    "Camera Mode / Camera Mask filters may not match during mirror rendering.\n" +
                    "With All Enabled Filters, if Mirror Mode Filter uses the same Filter Action and Target Mirror Modes does not include Normal, this combination cannot match in typical VRChat rendering.",
                    MessageType.Warning
                );
            }
        }

        protected override void ReplaceToCustomShaders()
        {
            lts         = FindShaderCached(shaderName + "/lilToon");
            ltsc        = FindShaderCached("Hidden/" + shaderName + "/Cutout");
            ltst        = FindShaderCached("Hidden/" + shaderName + "/Transparent");
            ltsot       = FindShaderCached("Hidden/" + shaderName + "/OnePassTransparent");
            ltstt       = FindShaderCached("Hidden/" + shaderName + "/TwoPassTransparent");

            ltso        = FindShaderCached("Hidden/" + shaderName + "/OpaqueOutline");
            ltsco       = FindShaderCached("Hidden/" + shaderName + "/CutoutOutline");
            ltsto       = FindShaderCached("Hidden/" + shaderName + "/TransparentOutline");
            ltsoto      = FindShaderCached("Hidden/" + shaderName + "/OnePassTransparentOutline");
            ltstto      = FindShaderCached("Hidden/" + shaderName + "/TwoPassTransparentOutline");

            ltsoo       = FindShaderCached(shaderName + "/[Optional] OutlineOnly/Opaque");
            ltscoo      = FindShaderCached(shaderName + "/[Optional] OutlineOnly/Cutout");
            ltstoo      = FindShaderCached(shaderName + "/[Optional] OutlineOnly/Transparent");

            ltstess     = FindShaderCached("Hidden/" + shaderName + "/Tessellation/Opaque");
            ltstessc    = FindShaderCached("Hidden/" + shaderName + "/Tessellation/Cutout");
            ltstesst    = FindShaderCached("Hidden/" + shaderName + "/Tessellation/Transparent");
            ltstessot   = FindShaderCached("Hidden/" + shaderName + "/Tessellation/OnePassTransparent");
            ltstesstt   = FindShaderCached("Hidden/" + shaderName + "/Tessellation/TwoPassTransparent");

            ltstesso    = FindShaderCached("Hidden/" + shaderName + "/Tessellation/OpaqueOutline");
            ltstessco   = FindShaderCached("Hidden/" + shaderName + "/Tessellation/CutoutOutline");
            ltstessto   = FindShaderCached("Hidden/" + shaderName + "/Tessellation/TransparentOutline");
            ltstessoto  = FindShaderCached("Hidden/" + shaderName + "/Tessellation/OnePassTransparentOutline");
            ltstesstto  = FindShaderCached("Hidden/" + shaderName + "/Tessellation/TwoPassTransparentOutline");

            ltsl        = FindShaderCached(shaderName + "/lilToonLite");
            ltslc       = FindShaderCached("Hidden/" + shaderName + "/Lite/Cutout");
            ltslt       = FindShaderCached("Hidden/" + shaderName + "/Lite/Transparent");
            ltslot      = FindShaderCached("Hidden/" + shaderName + "/Lite/OnePassTransparent");
            ltsltt      = FindShaderCached("Hidden/" + shaderName + "/Lite/TwoPassTransparent");

            ltslo       = FindShaderCached("Hidden/" + shaderName + "/Lite/OpaqueOutline");
            ltslco      = FindShaderCached("Hidden/" + shaderName + "/Lite/CutoutOutline");
            ltslto      = FindShaderCached("Hidden/" + shaderName + "/Lite/TransparentOutline");
            ltsloto     = FindShaderCached("Hidden/" + shaderName + "/Lite/OnePassTransparentOutline");
            ltsltto     = FindShaderCached("Hidden/" + shaderName + "/Lite/TwoPassTransparentOutline");

            ltsref      = FindShaderCached("Hidden/" + shaderName + "/Refraction");
            ltsrefb     = FindShaderCached("Hidden/" + shaderName + "/RefractionBlur");
            ltsfur      = FindShaderCached("Hidden/" + shaderName + "/Fur");
            ltsfurc     = FindShaderCached("Hidden/" + shaderName + "/FurCutout");
            ltsfurtwo   = FindShaderCached("Hidden/" + shaderName + "/FurTwoPass");
            ltsfuro     = FindShaderCached(shaderName + "/[Optional] FurOnly/Transparent");
            ltsfuroc    = FindShaderCached(shaderName + "/[Optional] FurOnly/Cutout");
            ltsfurotwo  = FindShaderCached(shaderName + "/[Optional] FurOnly/TwoPass");
            ltsgem      = FindShaderCached("Hidden/" + shaderName + "/Gem");
            ltsfs       = FindShaderCached(shaderName + "/[Optional] FakeShadow");

            ltsover     = FindShaderCached(shaderName + "/[Optional] Overlay");
            ltsoover    = FindShaderCached(shaderName + "/[Optional] OverlayOnePass");
            ltslover    = FindShaderCached(shaderName + "/[Optional] LiteOverlay");
            ltsloover   = FindShaderCached(shaderName + "/[Optional] LiteOverlayOnePass");

            ltsm        = FindShaderCached(shaderName + "/lilToonMulti");
            ltsmo       = FindShaderCached("Hidden/" + shaderName + "/MultiOutline");
            ltsmref     = FindShaderCached("Hidden/" + shaderName + "/MultiRefraction");
            ltsmfur     = FindShaderCached("Hidden/" + shaderName + "/MultiFur");
            ltsmgem     = FindShaderCached("Hidden/" + shaderName + "/MultiGem");
        }

        private static Shader FindShaderCached(string shaderPath)
        {
            Shader shader;
            if(!shaderCache.TryGetValue(shaderPath, out shader) || shader == null)
            {
                shader = Shader.Find(shaderPath);
                shaderCache[shaderPath] = shader;
            }

            return shader;
        }

        private void DrawCameraModeMaskField()
        {
            int rawMask = Mathf.RoundToInt(vrCameraModeMask.floatValue);
            int legacyMode = Mathf.RoundToInt(vrCameraModeTarget.floatValue);
            int currentMask = rawMask < 0 ? 1 << Mathf.Clamp(legacyMode, 0, 3) : rawMask;
            int nextMask = currentMask;

            EditorGUILayout.LabelField("Target Camera Modes");
            EditorGUI.indentLevel++;
            nextMask = DrawModeToggle("Normal", nextMask, 1 << 0);
            nextMask = DrawModeToggle("VR Handheld", nextMask, 1 << 1);
            nextMask = DrawModeToggle("Desktop Handheld", nextMask, 1 << 2);
            nextMask = DrawModeToggle("Screenshot", nextMask, 1 << 3);
            EditorGUI.indentLevel--;

            if(nextMask != currentMask)
            {
                vrCameraModeMask.floatValue = nextMask;
            }
        }

        private static int DrawModeToggle(string label, int mask, int bit)
        {
            bool enabled = (mask & bit) != 0;
            bool nextEnabled = EditorGUILayout.ToggleLeft(label, enabled);
            if(nextEnabled)
            {
                return mask | bit;
            }

            return mask & ~bit;
        }

        private void DrawMirrorModeMaskField()
        {
            int currentMask = Mathf.RoundToInt(vrMirrorModeMask.floatValue);
            int nextMask = currentMask;

            EditorGUILayout.LabelField("Target Mirror Modes");
            EditorGUI.indentLevel++;
            nextMask = DrawModeToggle("Normal", nextMask, 1 << 0);
            nextMask = DrawModeToggle("VR Mirror", nextMask, 1 << 1);
            nextMask = DrawModeToggle("Desktop Mirror", nextMask, 1 << 2);
            EditorGUI.indentLevel--;

            if(nextMask != currentMask)
            {
                vrMirrorModeMask.floatValue = nextMask;
            }
        }

        private void DrawCameraMaskField()
        {
            if(!TryGetIntPropertyValue(vrCameraMask.name, out int currentMask, out bool hasMixedValue))
            {
                currentMask = Mathf.RoundToInt(vrCameraMask.floatValue);
                hasMixedValue = vrCameraMask.hasMixedValue;
            }

            bool previousMixedValue = EditorGUI.showMixedValue;
            EditorGUI.showMixedValue = hasMixedValue;
            EditorGUI.BeginChangeCheck();
            int nextMask = EditorGUILayout.IntField("Target Camera Mask", currentMask);
            if(EditorGUI.EndChangeCheck())
            {
                SetIntPropertyValue(vrCameraMask.name, nextMask);
            }
            EditorGUI.showMixedValue = previousMixedValue;

            EditorGUILayout.LabelField("Camera Mask (Hex)", hasMixedValue ? "--" : FormatMaskHex(currentMask));
        }

        private bool HasImpossibleAllFiltersCombination()
        {
            if(Mathf.RoundToInt(vrFilterCombineMode.floatValue) != 1)
            {
                return false;
            }

            if(vrMirrorModeEnable.floatValue < 0.5f)
            {
                return false;
            }

            int cameraBehavior = Mathf.RoundToInt(vrCameraFilterBehavior.floatValue);
            int mirrorBehavior = Mathf.RoundToInt(vrMirrorFilterBehavior.floatValue);
            if(cameraBehavior != mirrorBehavior)
            {
                return false;
            }

            int mirrorMask = Mathf.RoundToInt(vrMirrorModeMask.floatValue);
            return (mirrorMask & (1 << 0)) == 0 && HasCameraFilterThatCannotMatchMirrorRendering();
        }

        private bool HasCameraFilterThatCannotMatchMirrorRendering()
        {
            if(vrCameraMaskEnable.floatValue >= 0.5f)
            {
                return true;
            }

            if(vrCameraModeEnable.floatValue < 0.5f)
            {
                return false;
            }

            int rawMask = Mathf.RoundToInt(vrCameraModeMask.floatValue);
            int legacyMode = Mathf.RoundToInt(vrCameraModeTarget.floatValue);
            int effectiveMask = rawMask < 0 ? 1 << Mathf.Clamp(legacyMode, 0, 3) : rawMask;
            return (effectiveMask & (1 << 0)) == 0;
        }

        private void AutoFixCameraMaskAfterStateChange(int previousCameraBehavior, bool wasCameraModeEnabled, bool wasCameraMaskEnabled)
        {
            bool cameraBehaviorChangedToShow = previousCameraBehavior != 0 && Mathf.RoundToInt(vrCameraFilterBehavior.floatValue) == 0;
            bool cameraModeDisabled = wasCameraModeEnabled && vrCameraModeEnable.floatValue < 0.5f;
            bool cameraMaskEnabled = !wasCameraMaskEnabled && vrCameraMaskEnable.floatValue >= 0.5f;

            if(!cameraBehaviorChangedToShow && !cameraModeDisabled && !cameraMaskEnabled)
            {
                return;
            }

            if(HasMaskOnlyInvisibleConfiguration())
            {
                SetIntPropertyValue(vrCameraMask.name, -1);
            }
        }

        private bool HasMaskOnlyInvisibleConfiguration()
        {
            if(vrCameraMaskEnable.floatValue < 0.5f)
            {
                return false;
            }

            if(vrCameraModeEnable.floatValue >= 0.5f)
            {
                return false;
            }

            if(Mathf.RoundToInt(vrCameraFilterBehavior.floatValue) != 0)
            {
                return false;
            }

            return AnyIntPropertyValueMatches(vrCameraMask.name, 0);
        }

        private bool TryGetIntPropertyValue(string propertyName, out int value, out bool hasMixedValue)
        {
            value = 0;
            hasMixedValue = false;
            Object[] targets = m_MaterialEditor.targets;
            Material firstMaterial = null;

            for(int i = 0; i < targets.Length; i++)
            {
                if(targets[i] is Material material)
                {
                    firstMaterial = material;
                    value = material.GetInt(propertyName);
                    break;
                }
            }

            if(firstMaterial == null)
            {
                return false;
            }

            for(int i = 0; i < targets.Length; i++)
            {
                Material material = targets[i] as Material;
                if(material == null)
                {
                    continue;
                }

                if(material.GetInt(propertyName) != value)
                {
                    hasMixedValue = true;
                    break;
                }
            }

            return true;
        }

        private bool AnyIntPropertyValueMatches(string propertyName, int expectedValue)
        {
            Object[] targets = m_MaterialEditor.targets;
            bool hasMaterial = false;

            for(int i = 0; i < targets.Length; i++)
            {
                Material material = targets[i] as Material;
                if(material == null)
                {
                    continue;
                }

                hasMaterial = true;
                if(material.GetInt(propertyName) == expectedValue)
                {
                    return true;
                }
            }

            return !hasMaterial && Mathf.RoundToInt(vrCameraMask.floatValue) == expectedValue;
        }

        private void SetIntPropertyValue(string propertyName, int value)
        {
            m_MaterialEditor.RegisterPropertyChangeUndo(propertyName);

            Object[] targets = m_MaterialEditor.targets;
            for(int i = 0; i < targets.Length; i++)
            {
                Material material = targets[i] as Material;
                if(material == null)
                {
                    continue;
                }

                material.SetInt(propertyName, value);
                EditorUtility.SetDirty(material);
            }
        }

        private static string FormatMaskHex(int value)
        {
            return "0x" + unchecked((uint)value).ToString("X8");
        }
    }
}
#endif
