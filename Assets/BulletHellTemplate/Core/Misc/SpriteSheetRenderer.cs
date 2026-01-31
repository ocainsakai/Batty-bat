using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace SpriteCaptureTool
{
    /// <summary>
    /// Captures an animation (optionally from four angles) and produces:
    /// 1) A single ordered sprite sheet (left‑to‑right, top‑to‑bottom, maxColumns per row).
    /// 2) A JSON file with weapon‑anchor pixel coordinates for each frame (inside its own cell).
    /// </summary>
    public class SpriteSheetRenderer : MonoBehaviour
    {
        #region Inspector -------------------------------------------------------

        [Header("Render Settings")]
        [Tooltip("Camera used to render individual frames.")]
        public Camera renderCamera;

        [Tooltip("Square size, in pixels, of each frame.")]
        public int frameSize = 512;

        [Tooltip("Desired output framerate.")]
        public float frameRate = 12f;

        [Header("Animation Settings")]
        [Tooltip("Animator holding the clip to capture.")]
        public Animator animator;

        [Tooltip("Name of the clip/state to play.")]
        public string animationName = "Idle";

        [Header("Capture Angles")]
        [Tooltip("Capture four directions: Front, Right, Back, Left.")]
        public bool captureMultiAngles = true;

        [Header("Sprite Sheet Settings")]
        [Tooltip("Maximum number of columns (frames per row).")]
        [Range(1, 20)]
        public int maxColumns = 10;

        [Header("Anchor Settings")]
        [Tooltip("Transform positioned on the hand (or desired weapon anchor).")]
        public Transform anchorTransform;

        #endregion --------------------------------------------------------------

        private readonly List<Texture2D> capturedFrames = new();
        private readonly List<Vector2> anchorPixels = new(); // pixel coords per frame
        private int totalFrameCount;

        private void Start() => StartCoroutine(CaptureAllAngles());

        #region Capture ---------------------------------------------------------

        private IEnumerator CaptureAllAngles()
        {
            Vector3 originalRotation = transform.eulerAngles;

            List<Vector3> angles = new()
            {
                new Vector3(0,   0, 0),   // Front
                new Vector3(0,  90, 0),   // Right
                new Vector3(0, 180, 0),   // Back
                new Vector3(0, 270, 0)    // Left
            };

            int angleCount = captureMultiAngles ? angles.Count : 1;

            for (int i = 0; i < angleCount; i++)
            {
                transform.eulerAngles = angles[i];
                yield return StartCoroutine(CaptureAnimation());
            }

            transform.eulerAngles = originalRotation;
            SaveSpriteSheetAndJson();
        }

        private IEnumerator CaptureAnimation()
        {
            AnimationClip clip = GetAnimationClip(animationName);
            if (clip == null)
            {
                Debug.LogError($"[SpriteSheetRenderer] Clip '{animationName}' not found.");
                yield break;
            }

            totalFrameCount = Mathf.CeilToInt(clip.length * frameRate);
            animator.Play(animationName, 0, 0f);
            yield return null; // wait 1 frame for proper pose

            for (int i = 0; i < totalFrameCount; i++)
            {
                yield return new WaitForEndOfFrame();

                // --- Render texture --------------------------------------------------
                RenderTexture rt = new RenderTexture(frameSize, frameSize, 24);
                renderCamera.targetTexture = rt;

                Texture2D frame = new Texture2D(frameSize, frameSize, TextureFormat.RGBA32, false)
                {
                    filterMode = FilterMode.Point
                };

                renderCamera.Render();
                RenderTexture.active = rt;
                frame.ReadPixels(new Rect(0, 0, frameSize, frameSize), 0, 0);
                frame.Apply();

                renderCamera.targetTexture = null;
                RenderTexture.active = null;
                Destroy(rt);

                capturedFrames.Add(frame);

                // --- Anchor position -------------------------------------------------
                if (anchorTransform != null)
                {
                    Vector3 vp = renderCamera.WorldToViewportPoint(anchorTransform.position);
                    var pixel = new Vector2(vp.x * frameSize, vp.y * frameSize);
                    anchorPixels.Add(pixel);
                }
                else
                {
                    anchorPixels.Add(Vector2.zero);
                }

                yield return new WaitForSeconds(1f / frameRate);
            }

            Debug.Log($"[SpriteSheetRenderer] Captured {totalFrameCount} frames.");
        }

        #endregion --------------------------------------------------------------

        #region Output ----------------------------------------------------------

        private void SaveSpriteSheetAndJson()
        {
            if (capturedFrames.Count == 0)
            {
                Debug.LogWarning("[SpriteSheetRenderer] No frames to save.");
                return;
            }

            int totalFrames = capturedFrames.Count;
            int columns = Mathf.Min(maxColumns, totalFrameCount);
            int rows = Mathf.CeilToInt((float)totalFrames / columns);

            int sheetWidth = columns * frameSize;
            int sheetHeight = rows * frameSize;

            Texture2D sheet = new Texture2D(sheetWidth, sheetHeight, TextureFormat.RGBA32, false);

            for (int i = 0; i < totalFrames; i++)
            {
                int col = i % columns;
                int row = i / columns;

                // sprite placement (origin bottom‑left)
                int xPos = col * frameSize;
                int yPos = (rows - row - 1) * frameSize; // flip Y for texture coords

                sheet.SetPixels(xPos, yPos, frameSize, frameSize, capturedFrames[i].GetPixels());
            }

            sheet.Apply();

            string dir = "Assets/SpriteFrames";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            File.WriteAllBytes($"{dir}/SpriteSheet.png", sheet.EncodeToPNG());
            File.WriteAllText($"{dir}/AnchorPoints.json", BuildAnchorJson(columns, rows));

            Debug.Log($"[SpriteSheetRenderer] ✔ SpriteSheet.png & AnchorPoints.json gerados ({columns}x{rows}).");
        }

        /// <summary>Returns the requested clip from the AnimatorController.</summary>
        private AnimationClip GetAnimationClip(string clipName)
        {
            foreach (var clip in animator.runtimeAnimatorController.animationClips)
                if (clip.name == clipName) return clip;
            return null;
        }

        /// <summary>
        /// Builds a JSON string with anchor pixel coordinates relative to each frame cell.
        /// </summary>
        private string BuildAnchorJson(int columns, int rows)
        {
            List<AnchorFrame> list = new(capturedFrames.Count);

            for (int i = 0; i < anchorPixels.Count; i++)
            {
                int col = i % columns;
                int row = i / columns;
                list.Add(new AnchorFrame
                {
                    frame = i,
                    column = col,
                    row = row,
                    x = anchorPixels[i].x,
                    y = anchorPixels[i].y
                });
            }

            AnchorWrapper wrapper = new() { anchors = list.ToArray() };
            return JsonUtility.ToJson(wrapper, true);
        }

        [System.Serializable]
        private struct AnchorFrame
        {
            public int frame;  // absolute index in spritesheet order
            public int column; // col 0..N
            public int row;    // row 0..M
            public float x;      // pixel inside the frame (origin bottom‑left)
            public float y;
        }

        [System.Serializable]
        private struct AnchorWrapper { public AnchorFrame[] anchors; }

        #endregion -------------------------------------------------------------
    }
}
