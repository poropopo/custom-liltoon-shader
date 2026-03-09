//----------------------------------------------------------------------------------------------------------------------
// Custom material properties

#define LIL_CUSTOM_PROPERTIES \
    int _VRFilterCombineMode; \
    int _VRCameraFilterBehavior; \
    int _VRCameraModeEnable; \
    int _VRCameraModeTarget; \
    int _VRCameraModeMask; \
    int _VRCameraMaskEnable; \
    int _VRCameraMaskCompareMode; \
    int _VRCameraMask; \
    int _VRMirrorFilterBehavior; \
    int _VRMirrorModeEnable; \
    int _VRMirrorModeMask;

#define LIL_CUSTOM_TEXTURES

// VRChat global shader value:
// 0 = Normal
// 1 = VR handheld camera
// 2 = Desktop handheld camera
// 3 = Screenshot camera
float _VRChatCameraMode;
uint _VRChatCameraMask;

// VRChat global mirror value:
// 0 = Normal
// 1 = VR mirror
// 2 = Desktop mirror
float _VRChatMirrorMode;

#define LIL_VRCAMERA_FILTER_ACTIVE \
    ((_VRCameraModeEnable != 0) || (_VRCameraMaskEnable != 0))

#define LIL_VRCAMERA_EFFECTIVE_MODE_MASK \
    ((uint)((_VRCameraModeMask < 0) ? (1 << _VRCameraModeTarget) : _VRCameraModeMask))

#define LIL_VRCAMERA_CURRENT_MODE_BIT \
    (_VRChatCameraMode < 0.5 ? 1u : (_VRChatCameraMode < 1.5 ? 2u : (_VRChatCameraMode < 2.5 ? 4u : 8u)))

#define LIL_VRCAMERA_MODE_MATCH \
    ((_VRCameraModeEnable == 0) || ((LIL_VRCAMERA_EFFECTIVE_MODE_MASK & LIL_VRCAMERA_CURRENT_MODE_BIT) != 0u))

#define LIL_VRCAMERA_MASK_MATCH \
    ((_VRCameraMaskEnable == 0) || ( \
        abs(_VRChatCameraMode) >= 0.5 && \
        (uint)_VRCameraMask != 0u && \
        (((_VRCameraMaskCompareMode == 0) && ((_VRChatCameraMask & (uint)_VRCameraMask) != 0u)) || \
        ((_VRCameraMaskCompareMode != 0) && (_VRChatCameraMask == (uint)_VRCameraMask))) \
    ))

#define LIL_VRCAMERA_FILTER_MATCH \
    (LIL_VRCAMERA_MODE_MATCH && LIL_VRCAMERA_MASK_MATCH)

#define LIL_VRMIRROR_FILTER_ACTIVE \
    (_VRMirrorModeEnable != 0)

#define LIL_VRMIRROR_CURRENT_MODE_BIT \
    (_VRChatMirrorMode < 0.5 ? 1u : (_VRChatMirrorMode < 1.5 ? 2u : 4u))

#define LIL_VRMIRROR_MODE_MATCH \
    ((_VRMirrorModeEnable == 0) || (((uint)_VRMirrorModeMask & LIL_VRMIRROR_CURRENT_MODE_BIT) != 0u))

#define LIL_VRCAMERA_SHOW_FILTER_ACTIVE \
    (LIL_VRCAMERA_FILTER_ACTIVE && _VRCameraFilterBehavior == 0)

#define LIL_VRCAMERA_HIDE_FILTER_ACTIVE \
    (LIL_VRCAMERA_FILTER_ACTIVE && _VRCameraFilterBehavior != 0)

#define LIL_VRMIRROR_SHOW_FILTER_ACTIVE \
    (LIL_VRMIRROR_FILTER_ACTIVE && _VRMirrorFilterBehavior == 0)

#define LIL_VRMIRROR_HIDE_FILTER_ACTIVE \
    (LIL_VRMIRROR_FILTER_ACTIVE && _VRMirrorFilterBehavior != 0)

#define LIL_VR_ANY_SHOW_FILTER_ACTIVE \
    (LIL_VRCAMERA_SHOW_FILTER_ACTIVE || LIL_VRMIRROR_SHOW_FILTER_ACTIVE)

#define LIL_VR_ANY_HIDE_FILTER_ACTIVE \
    (LIL_VRCAMERA_HIDE_FILTER_ACTIVE || LIL_VRMIRROR_HIDE_FILTER_ACTIVE)

#define LIL_VR_ANY_SHOW_MATCH \
    ((LIL_VRCAMERA_SHOW_FILTER_ACTIVE && LIL_VRCAMERA_FILTER_MATCH) || \
    (LIL_VRMIRROR_SHOW_FILTER_ACTIVE && LIL_VRMIRROR_MODE_MATCH))

#define LIL_VR_ALL_SHOW_MATCH \
    ((!LIL_VRCAMERA_SHOW_FILTER_ACTIVE || LIL_VRCAMERA_FILTER_MATCH) && \
    (!LIL_VRMIRROR_SHOW_FILTER_ACTIVE || LIL_VRMIRROR_MODE_MATCH))

#define LIL_VR_ANY_HIDE_MATCH \
    ((LIL_VRCAMERA_HIDE_FILTER_ACTIVE && LIL_VRCAMERA_FILTER_MATCH) || \
    (LIL_VRMIRROR_HIDE_FILTER_ACTIVE && LIL_VRMIRROR_MODE_MATCH))

#define LIL_VR_ALL_HIDE_MATCH \
    ((!LIL_VRCAMERA_HIDE_FILTER_ACTIVE || LIL_VRCAMERA_FILTER_MATCH) && \
    (!LIL_VRMIRROR_HIDE_FILTER_ACTIVE || LIL_VRMIRROR_MODE_MATCH))

#define LIL_VR_SHOW_ALLOWED \
    ((!LIL_VR_ANY_SHOW_FILTER_ACTIVE) || \
    ((_VRFilterCombineMode == 0 && LIL_VR_ANY_SHOW_MATCH) || \
    (_VRFilterCombineMode != 0 && LIL_VR_ALL_SHOW_MATCH)))

#define LIL_VR_HIDE_BLOCKED \
    (LIL_VR_ANY_HIDE_FILTER_ACTIVE && \
    ((_VRFilterCombineMode == 0 && LIL_VR_ANY_HIDE_MATCH) || \
    (_VRFilterCombineMode != 0 && LIL_VR_ALL_HIDE_MATCH)))

#define LIL_VRFILTER_VISIBLE \
    (LIL_VR_SHOW_ALLOWED && !LIL_VR_HIDE_BLOCKED)

// Keep the visibility filter on runtime color/shadow/depth passes only.
// Editor-only passes such as Meta, MotionVectors, selection and picking should stay untouched.
#if defined(LIL_PASS_FORWARD) || defined(LIL_PASS_FORWARDADD) || defined(LIL_PASS_SHADOWCASTER) || defined(LIL_PASS_DEPTHONLY) || defined(LIL_PASS_DEPTHNORMALS)
#define BEFORE_UNPACK_V2F \
    clip(LIL_VRFILTER_VISIBLE ? 1.0 : -1.0);
#endif
