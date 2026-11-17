using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Znalica.Domain;

namespace Znalica.Presentation
{
    /// <summary>
    /// Plays a phrase as a queue of clips on one AudioSource at the voice volume, and replays the last one for the
    /// speaker button. A missing clip or no audio device never stops the game: the text stays on screen.
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

        /// <summary>Voice volume, 0.0 to 1.0.</summary>
        public float Volume
        {
            get => _volume;
            set
            {
                _volume = Mathf.Clamp01(value);
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
            if (_playing != null)
            {
                StopCoroutine(_playing);
            }

            _source.Stop();
            _playing = StartCoroutine(PlayQueue(clipNames));
        }

        public void ReplayLast()
        {
            Play(_last);
        }

        private IEnumerator PlayQueue(IReadOnlyList<string> clipNames)
        {
            foreach (string name in clipNames)
            {
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
