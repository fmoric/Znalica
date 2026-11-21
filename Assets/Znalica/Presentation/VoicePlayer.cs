using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Znalica.Domain;

namespace Znalica.Presentation
{
    /// <summary>
    /// Plays a phrase as a queue of clips on one AudioSource at the voice volume, and replays the last one for the
    /// speaker button. At volume 0 nothing is played and nothing is waited on. A missing clip or no audio device
    /// never stops the game: the text stays on screen.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public sealed class VoicePlayer : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float _volume = 1f;

        private AudioSource _source;
        private VoiceLibrary _library;
        private IProblemLog _problems;
        private IReadOnlyList<string> _last = new string[0];
        private Coroutine _playing;
        private readonly Queue<string> _pending = new Queue<string>();

        /// <summary>Voice volume, 0.0 to 1.0.</summary>
        public float Volume
        {
            get => _volume;
            set
            {
                _volume = VoiceVolume.Clamp(value);
                if (_source != null)
                {
                    _source.volume = _volume;
                }
            }
        }

        public void Initialize(VoiceLibrary library, IProblemLog problems)
        {
            _library = library;
            _problems = problems;
            _source = GetComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.volume = _volume;
        }

        /// <summary>Stops what is playing and plays the clips in order.</summary>
        public void Play(IReadOnlyList<string> clipNames)
        {
            _last = clipNames;
            _pending.Clear();
            if (_playing != null)
            {
                StopCoroutine(_playing);
                _playing = null;
            }

            _source.Stop();
            if (VoiceVolume.IsSilent(_volume))
            {
                return;
            }

            _playing = StartCoroutine(PlayQueue(clipNames));
        }

        /// <summary>
        /// Plays the clips after what is playing has finished, without cutting it off; at once if nothing is playing. At
        /// volume 0 it does nothing. What <see cref="ReplayLast"/> replays then is what was playing followed by these clips.
        /// </summary>
        public void PlayAfterCurrent(IReadOnlyList<string> clipNames)
        {
            if (_playing == null)
            {
                Play(clipNames);
                return;
            }

            if (VoiceVolume.IsSilent(_volume))
            {
                return;
            }

            var all = new List<string>(_last);
            all.AddRange(clipNames);
            _last = all;
            foreach (string name in clipNames)
            {
                _pending.Enqueue(name);
            }
        }

        /// <summary>Stops what is playing. What <see cref="ReplayLast"/> replays is kept.</summary>
        public void Stop()
        {
            if (_playing != null)
            {
                StopCoroutine(_playing);
                _playing = null;
            }

            _pending.Clear();
            _source.Stop();
        }

        public void ReplayLast()
        {
            Play(_last);
        }

        private IEnumerator PlayQueue(IReadOnlyList<string> clipNames)
        {
            foreach (string first in clipNames)
            {
                _pending.Enqueue(first);
            }

            while (_pending.Count > 0)
            {
                string name = _pending.Dequeue();
                if (_library == null || !_library.TryGetClip(name, out AudioClip clip))
                {
                    _problems.Report("No voice clip named " + name + ".");
                    continue;
                }

                _source.clip = clip;
                _source.Play();
                yield return new WaitForSeconds(clip.length);
            }

            _playing = null;
        }
    }
}
