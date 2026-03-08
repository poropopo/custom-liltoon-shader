//----------------------------------------------------------------------------------------------------------------------
// Custom material properties

#define LIL_CUSTOM_PROPERTIES \
    int _VRCameraFilterBehavior; \
    int _VRCameraModeEnable; \
    int _VRCameraModeTarget; \
    int _VRCameraModeMask; \
    int _VRCameraMaskEnable; \
    int _VRCameraMaskCompareMode; \
    int _VRCameraMask;

#define LIL_CUSTOM_TEXTURES

// VRChat global shader value:
// 0 = Normal
// 1 = VR handheld camera
// 2 = Desktop handheld camera
// 3 = Screenshot camera
float _VRChatCameraMode;
uint _VRChatCameraMask;

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

#define LIL_VRCAMERA_VISIBLE \
    ((!LIL_VRCAMERA_FILTER_ACTIVE) || \
    ((_VRCameraFilterBehavior == 0 && LIL_VRCAMERA_FILTER_MATCH) || \
    (_VRCameraFilterBehavior != 0 && !LIL_VRCAMERA_FILTER_MATCH)))

// Clip every pass up front so the object also disappears from shadows and depth buffers.
#define BEFORE_UNPACK_V2F \
    clip(LIL_VRCAMERA_VISIBLE ? 1.0 : -1.0);
