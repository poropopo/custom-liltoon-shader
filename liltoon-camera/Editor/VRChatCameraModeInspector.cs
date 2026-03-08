#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace lilToon
{
    public class VRChatCameraModeInspector : lilToonInspector
    {
        private MaterialProperty vrCameraFilterBehavior;
        private MaterialProperty vrCameraModeEnable;
        private MaterialProperty vrCameraModeTarget;
        private MaterialProperty vrCameraModeMask;
        private MaterialProperty vrCameraMaskEnable;
        private MaterialProperty vrCameraMaskCompareMode;
        private MaterialProperty vrCameraMask;

        private static bool isShowVRCameraMode;
        private const string shaderName = "VRChatCameraMode";

        protected override void LoadCustomProperties(MaterialProperty[] props, Material material)
        {
            isCustomShader = true;

            ReplaceToCustomShaders();
            isShowRenderMode = !material.shader.name.Contains("Optional");

            vrCameraFilterBehavior = FindProperty("_VRCameraFilterBehavior", props);
            vrCameraModeEnable = FindProperty("_VRCameraModeEnable", props);
            vrCameraModeTarget = FindProperty("_VRCameraModeTarget", props);
            vrCameraModeMask = FindProperty("_VRCameraModeMask", props);
            vrCameraMaskEnable = FindProperty("_VRCameraMaskEnable", props);
            vrCameraMaskCompareMode = FindProperty("_VRCameraMaskCompareMode", props);
            vrCameraMask = FindProperty("_VRCameraMask", props);
        }

        protected override void DrawCustomProperties(Material material)
        {
            isShowVRCameraMode = Foldout("VRChat Camera Mode", "VRChat Camera Mode", isShowVRCameraMode);
            if(!isShowVRCameraMode) return;

            EditorGUILayout.BeginVertical(boxOuter);
            EditorGUILayout.LabelField("VRChat Camera Mode", customToggleFont);
            EditorGUILayout.BeginVertical(boxInnerHalf);

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

                int currentMask = Mathf.RoundToInt(vrCameraMask.floatValue);
                int nextMask = EditorGUILayout.IntField("Target Camera Mask", currentMask);
                if(nextMask != currentMask)
                {
                    vrCameraMask.floatValue = nextMask;
                }
            }

            EditorGUILayout.HelpBox(
                "_VRChatCameraMode values: 0 Normal, 1 VR Handheld, 2 Desktop Handheld, 3 Screenshot.\n" +
                "You can target multiple camera modes at once.\n" +
                "_VRChatCameraMask is the active camera cullingMask and is only valid when CameraMode is not 0.\n" +
                "Set Filter Action to Hide On Match to hide the material only on the selected camera.",
                MessageType.Info
            );

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();
        }

        protected override void ReplaceToCustomShaders()
        {
            lts         = Shader.Find(shaderName + "/lilToon");
            ltsc        = Shader.Find("Hidden/" + shaderName + "/Cutout");
            ltst        = Shader.Find("Hidden/" + shaderName + "/Transparent");
            ltsot       = Shader.Find("Hidden/" + shaderName + "/OnePassTransparent");
            ltstt       = Shader.Find("Hidden/" + shaderName + "/TwoPassTransparent");

            ltso        = Shader.Find("Hidden/" + shaderName + "/OpaqueOutline");
            ltsco       = Shader.Find("Hidden/" + shaderName + "/CutoutOutline");
            ltsto       = Shader.Find("Hidden/" + shaderName + "/TransparentOutline");
            ltsoto      = Shader.Find("Hidden/" + shaderName + "/OnePassTransparentOutline");
            ltstto      = Shader.Find("Hidden/" + shaderName + "/TwoPassTransparentOutline");

            ltsoo       = Shader.Find(shaderName + "/[Optional] OutlineOnly/Opaque");
            ltscoo      = Shader.Find(shaderName + "/[Optional] OutlineOnly/Cutout");
            ltstoo      = Shader.Find(shaderName + "/[Optional] OutlineOnly/Transparent");

            ltstess     = Shader.Find("Hidden/" + shaderName + "/Tessellation/Opaque");
            ltstessc    = Shader.Find("Hidden/" + shaderName + "/Tessellation/Cutout");
            ltstesst    = Shader.Find("Hidden/" + shaderName + "/Tessellation/Transparent");
            ltstessot   = Shader.Find("Hidden/" + shaderName + "/Tessellation/OnePassTransparent");
            ltstesstt   = Shader.Find("Hidden/" + shaderName + "/Tessellation/TwoPassTransparent");

            ltstesso    = Shader.Find("Hidden/" + shaderName + "/Tessellation/OpaqueOutline");
            ltstessco   = Shader.Find("Hidden/" + shaderName + "/Tessellation/CutoutOutline");
            ltstessto   = Shader.Find("Hidden/" + shaderName + "/Tessellation/TransparentOutline");
            ltstessoto  = Shader.Find("Hidden/" + shaderName + "/Tessellation/OnePassTransparentOutline");
            ltstesstto  = Shader.Find("Hidden/" + shaderName + "/Tessellation/TwoPassTransparentOutline");

            ltsl        = Shader.Find(shaderName + "/lilToonLite");
            ltslc       = Shader.Find("Hidden/" + shaderName + "/Lite/Cutout");
            ltslt       = Shader.Find("Hidden/" + shaderName + "/Lite/Transparent");
            ltslot      = Shader.Find("Hidden/" + shaderName + "/Lite/OnePassTransparent");
            ltsltt      = Shader.Find("Hidden/" + shaderName + "/Lite/TwoPassTransparent");

            ltslo       = Shader.Find("Hidden/" + shaderName + "/Lite/OpaqueOutline");
            ltslco      = Shader.Find("Hidden/" + shaderName + "/Lite/CutoutOutline");
            ltslto      = Shader.Find("Hidden/" + shaderName + "/Lite/TransparentOutline");
            ltsloto     = Shader.Find("Hidden/" + shaderName + "/Lite/OnePassTransparentOutline");
            ltsltto     = Shader.Find("Hidden/" + shaderName + "/Lite/TwoPassTransparentOutline");

            ltsref      = Shader.Find("Hidden/" + shaderName + "/Refraction");
            ltsrefb     = Shader.Find("Hidden/" + shaderName + "/RefractionBlur");
            ltsfur      = Shader.Find("Hidden/" + shaderName + "/Fur");
            ltsfurc     = Shader.Find("Hidden/" + shaderName + "/FurCutout");
            ltsfurtwo   = Shader.Find("Hidden/" + shaderName + "/FurTwoPass");
            ltsfuro     = Shader.Find(shaderName + "/[Optional] FurOnly/Transparent");
            ltsfuroc    = Shader.Find(shaderName + "/[Optional] FurOnly/Cutout");
            ltsfurotwo  = Shader.Find(shaderName + "/[Optional] FurOnly/TwoPass");
            ltsgem      = Shader.Find("Hidden/" + shaderName + "/Gem");
            ltsfs       = Shader.Find(shaderName + "/[Optional] FakeShadow");

            ltsover     = Shader.Find(shaderName + "/[Optional] Overlay");
            ltsoover    = Shader.Find(shaderName + "/[Optional] OverlayOnePass");
            ltslover    = Shader.Find(shaderName + "/[Optional] LiteOverlay");
            ltsloover   = Shader.Find(shaderName + "/[Optional] LiteOverlayOnePass");

            ltsm        = Shader.Find(shaderName + "/lilToonMulti");
            ltsmo       = Shader.Find("Hidden/" + shaderName + "/MultiOutline");
            ltsmref     = Shader.Find("Hidden/" + shaderName + "/MultiRefraction");
            ltsmfur     = Shader.Find("Hidden/" + shaderName + "/MultiFur");
            ltsmgem     = Shader.Find("Hidden/" + shaderName + "/MultiGem");
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

            if(nextMask != currentMask || rawMask < 0)
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
    }
}
#endif
