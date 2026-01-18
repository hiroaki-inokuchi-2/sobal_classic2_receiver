# Work Log

## 2025-09-27
- Goal: Enable slide switching via right-hand swipe in Unity project.
- Scope: `mocopi VR Receiver` project, `ReceiverSample` scene.
- Change: Added `RightHandSwipeSlideController` component to `SlideAnimationManager` and wired `rightHand` to the avatar right-hand transform and `slideAnimation` to the existing `ImageAnimation` component.
- Files touched: `mocopi VR Receiver/Assets/MocopiReceiver/Samples/ReceiverSample/Scenes/ReceiverSample.unity`.
- Notes: Parameters left at defaults (min distance 0.25, max duration 0.4, cooldown 0.6, vertical 0.15, depth 0.2). Verify in Unity and tune if needed.

## 2026-01-12
- Goal: Increase swipe distance, shorten swipe time window, and map right/left hand swipe directions to slide navigation.
- Change: Updated `RightHandSwipeSlideController` to track right and left hands separately; right-hand right-to-left advances, left-hand left-to-right goes back; added auto-lookup for left hand by name.
- Change: Updated swipe thresholds in scene (min distance 0.35, max duration 0.3).
- Files touched: `mocopi VR Receiver/Assets/Scripts/RightHandSwipeSlideController.cs`, `mocopi VR Receiver/Assets/MocopiReceiver/Samples/ReceiverSample/Scenes/ReceiverSample.unity`.
- Notes: If left hand is not found automatically, assign it in the inspector or change `leftHandName`.

## 2026-01-12
- Update: Restored swipe max duration to 0.4 and added clarifying comments for left-hand lookup and swipe resets.
- Files touched: `mocopi VR Receiver/Assets/Scripts/RightHandSwipeSlideController.cs`, `mocopi VR Receiver/Assets/MocopiReceiver/Samples/ReceiverSample/Scenes/ReceiverSample.unity`.

## 2026-01-12
- Update: Translated newly added code comments to Japanese per request.
- Files touched: `mocopi VR Receiver/Assets/Scripts/RightHandSwipeSlideController.cs`.

## 2026-01-12
- Update: Added detailed Japanese comments explaining swipe detection flow, thresholds, and left/right behavior.
- Files touched: `mocopi VR Receiver/Assets/Scripts/RightHandSwipeSlideController.cs`.

## 2026-01-12
- Update: Added on-screen debug overlay in `RightHandSwipeSlideController` showing hand names, tracking state, deltas, cooldowns, and thresholds.
- Files touched: `mocopi VR Receiver/Assets/Scripts/RightHandSwipeSlideController.cs`.

## 2026-01-12
- Update: Added agent workflow steps to `/Users/gotouyasushiakira1/AGENTS.md` as requested.
- Files touched: `/Users/gotouyasushiakira1/AGENTS.md`, `WORKLOG.md`.

## 2026-01-12
- Update: Added runtime UI Text debug overlay creation and updates so swipe debug info shows on the Canvas; OnGUI now fallback-only.
- Files touched: `mocopi VR Receiver/Assets/Scripts/RightHandSwipeSlideController.cs`.

## 2026-01-12
- Update: Changed swipe debug overlay text color to red for better visibility.
- Files touched: `mocopi VR Receiver/Assets/Scripts/RightHandSwipeSlideController.cs`.

## 2026-01-12
- Update: Force debug Text color/size to refresh every frame so Inspector changes (red text) take effect.
- Files touched: `mocopi VR Receiver/Assets/Scripts/RightHandSwipeSlideController.cs`.

## 2026-01-12
- Update: Added a semi-transparent background panel behind the debug text to improve readability.
- Files touched: `mocopi VR Receiver/Assets/Scripts/RightHandSwipeSlideController.cs`.

## 2026-01-18
- Check: Verified left-hand object name in avatar prefab; `WRIST LEFT` exists in `Assets/Avatar/RaynosChanAvatar.prefab` and matches `leftHandName` lookup.
- Files touched: `WORKLOG.md`.

## 2026-01-18
- Guidance: Explained how to locate objects wired to LaserPointerScript in Unity (Hierarchy/Project search and Inspector references).
- Files touched: `WORKLOG.md`.

## 2026-01-18
- Update: Reduced swipe minimum distance to 0.12 for smaller gesture triggering; synced code default and scene value.
- Files touched: `mocopi VR Receiver/Assets/Scripts/RightHandSwipeSlideController.cs`, `mocopi VR Receiver/Assets/MocopiReceiver/Samples/ReceiverSample/Scenes/ReceiverSample.unity`.

## 2026-01-18
- Update: Revised AGENTS.md to require steps 1-4 before code changes and allow skipping steps for very simple tasks.
- Files touched: `/Users/gotouyasushiakira1/AGENTS.md`.

## 2026-01-18
- Update: Added swipe speed threshold and slide boundary checks; shortened max swipe duration; added CanSlide to ImageAnimation.
- Files touched: `mocopi VR Receiver/Assets/Animation/ImageAnimation.cs`, `mocopi VR Receiver/Assets/Scripts/RightHandSwipeSlideController.cs`, `mocopi VR Receiver/Assets/MocopiReceiver/Samples/ReceiverSample/Scenes/ReceiverSample.unity`.

## 2026-01-18
- Update: Added instruction to AGENTS.md to include slightly excessive comments when changing code.
- Files touched: `/Users/gotouyasushiakira1/AGENTS.md`.

## 2026-01-18
- Update: Added swipe zone gating (chest-front area) with configurable center/size and debug display; wired default zone values in ReceiverSample scene.
- Files touched: `mocopi VR Receiver/Assets/Scripts/RightHandSwipeSlideController.cs`, `mocopi VR Receiver/Assets/MocopiReceiver/Samples/ReceiverSample/Scenes/ReceiverSample.unity`.

## 2026-01-18
- Update: Added "chest height only" swipe zone mode so only vertical band is enforced; scene now enables this mode.
- Files touched: `mocopi VR Receiver/Assets/Scripts/RightHandSwipeSlideController.cs`, `mocopi VR Receiver/Assets/MocopiReceiver/Samples/ReceiverSample/Scenes/ReceiverSample.unity`.

## 2026-01-18
- Update: Adjusted swipe zone height band to allow Y=1.0..1.5 by setting center to 1.25 and size Y to 0.5; synced scene values.
- Files touched: `mocopi VR Receiver/Assets/Scripts/RightHandSwipeSlideController.cs`, `mocopi VR Receiver/Assets/MocopiReceiver/Samples/ReceiverSample/Scenes/ReceiverSample.unity`.

## 2026-01-18
- Update: Adjusted swipe height band to Y=1.1..1.25 by setting center to 1.175 and size Y to 0.15; synced scene values.
- Files touched: `mocopi VR Receiver/Assets/Scripts/RightHandSwipeSlideController.cs`, `mocopi VR Receiver/Assets/MocopiReceiver/Samples/ReceiverSample/Scenes/ReceiverSample.unity`.

## 2026-01-18
- Update: Adjusted swipe height band to Y=1.0..1.35 by setting center to 1.175 and size Y to 0.35; synced scene values.
- Files touched: `mocopi VR Receiver/Assets/Scripts/RightHandSwipeSlideController.cs`, `mocopi VR Receiver/Assets/MocopiReceiver/Samples/ReceiverSample/Scenes/ReceiverSample.unity`.

## 2026-01-18
- Update: Added clap SE playback on wrist-to-wrist collision with cooldown and AudioSource auto-setup; assigned default clip to wrist/ankle hitbox listeners in avatar prefab.
- Files touched: `mocopi VR Receiver/Assets/Scripts/BoxColiderEventListener.cs`, `mocopi VR Receiver/Assets/Avatar/RaynosChanAvatar.prefab`.

## 2026-01-18
- Update: Added on-screen clap debug overlay (detection status, counts, cooldown) to BoxColiderEventListener for verifying clap events.
- Files touched: `mocopi VR Receiver/Assets/Scripts/BoxColiderEventListener.cs`.

## 2026-01-18
- Update: Moved clap debug overlay downward to avoid overlap and added debug states for NotReady/NullCollider/NotWrist.
- Files touched: `mocopi VR Receiver/Assets/Scripts/BoxColiderEventListener.cs`.

## 2026-01-18
- Update: Added ClapDistanceDetector script to detect claps by hand distance (no colliders) with on-screen debug; wired it on RaynosChanAvatar with wrist references and default clip.
- Files touched: `mocopi VR Receiver/Assets/Scripts/ClapDistanceDetector.cs`, `mocopi VR Receiver/Assets/Scripts/ClapDistanceDetector.cs.meta`, `mocopi VR Receiver/Assets/Avatar/RaynosChanAvatar.prefab`.

## 2026-01-18
- Fix: Simplified ClapDistanceDetector debug string building and switched AddComponent to non-generic calls to avoid compiler tuple parsing errors.
- Files touched: `mocopi VR Receiver/Assets/Scripts/ClapDistanceDetector.cs`.

## 2026-01-18
- Update: ClapDistanceDetector now plays SE only after 3 consecutive detections; added requiredConsecutiveClaps and debug display of streak; prefab wired to 3.
- Files touched: `mocopi VR Receiver/Assets/Scripts/ClapDistanceDetector.cs`, `mocopi VR Receiver/Assets/Avatar/RaynosChanAvatar.prefab`.

## 2026-01-18
- Update: Added 2-second window requirement for 3 consecutive claps; tracked first clap time and exposed consecutiveClapWindow; prefab set to 2s.
- Files touched: `mocopi VR Receiver/Assets/Scripts/ClapDistanceDetector.cs`, `mocopi VR Receiver/Assets/Avatar/RaynosChanAvatar.prefab`.

## 2026-01-18
- Update: Made clap window counting time-based (rolling window) instead of resetting on first clap timing; keep detection history and only expire old entries.
- Files touched: `mocopi VR Receiver/Assets/Scripts/ClapDistanceDetector.cs`.
