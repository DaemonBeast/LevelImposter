﻿using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime.Attributes;

namespace LevelImposter.Core
{
    /// <summary>
    /// Component to animate GIF data in-game
    /// </summary>
    public class GIFAnimator : MonoBehaviour
    {
        public GIFAnimator(IntPtr intPtr) : base(intPtr)
        {
        }

        private static readonly List<string> AUTOPLAY_BLACKLIST = new()
        {
            "util-vent1",
            "util-vent2",
            "sab-doorv",
            "sab-doorh",
            "util-cam"
        };

        private bool _isAnimating = false;
        private float[]? _delays;
        private Sprite[]? _frames;
        private SpriteRenderer? _spriteRenderer;
        private Coroutine? _animationCoroutine = null;

        public bool IsAnimating => _isAnimating;

        /// <summary>
        /// Initializes the component with GIF data
        /// </summary>
        /// <param name="element">Element that is initialized</param>
        /// <param name="sprites">Array of sprites representing each frame</param>
        /// <param name="frameTimes">Array of floats representing the times each frame is visible</param>
        [HideFromIl2Cpp]
        public void Init(LIElement element, Sprite[] sprites, float[] frameTimes)
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _frames = sprites;
            _delays = frameTimes;
            Play(true);
            if (AUTOPLAY_BLACKLIST.Contains(element.type))
                Stop();
        }

        /// <summary>
        /// Plays the GIF animation
        /// </summary>
        /// <param name="repeat">True if the GIF should repeat. False otherwise</param>
        public void Play(bool repeat = false, bool reverse = false)
        {
            if (_frames == null || _delays == null)
                LILogger.Warn($"{name} does not have a frame sprites or delays");
            if (_spriteRenderer == null)
                LILogger.Warn($"{name} does not have a spriteRenderer");
            if (_animationCoroutine != null)
                StopCoroutine(_animationCoroutine);
            _animationCoroutine = StartCoroutine(CoAnimate(repeat, reverse).WrapToIl2Cpp());
        }

        /// <summary>
        /// Stops the GIF animation
        /// </summary>
        public void Stop(bool reversed = false)
        {
            if (_animationCoroutine != null)
                StopCoroutine(_animationCoroutine);
            _isAnimating = false;
            if (_spriteRenderer != null && _frames != null)
                _spriteRenderer.sprite = _frames[reversed ? _frames.Length - 1 : 0];
        }

        /// <summary>
        /// Coroutine to run GIF animation
        /// </summary>
        /// <param name="repeat">TRUE if animation should loop</param>
        /// <param name="reverse">TRUE if animation should run in reverse</param>
        /// <returns>IEnumerator for Unity Coroutine</returns>
        [HideFromIl2Cpp]
        private IEnumerator CoAnimate(bool repeat, bool reverse)
        {
            if (_frames == null || _delays == null || _spriteRenderer == null)
                yield break;
            _isAnimating = true;
            int t = 0;
            while (_isAnimating)
            {
                int frame = reverse ? _frames.Length - t - 1 : t;
                _spriteRenderer.sprite = _frames[frame];
                yield return new WaitForSeconds(_delays[frame]);
                t = (t + 1) % _frames.Length;
                if (t == 0 && !repeat)
                    Stop(!reverse);
            }
        }

        public void OnDestroy()
        {
            _delays = null;
            _frames = null;
            _spriteRenderer = null;
            _animationCoroutine = null;
        }
    }
}
