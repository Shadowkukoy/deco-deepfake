using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Text;

namespace Deepfakes.Typography.TypeWriter
{
    [RequireComponent(typeof(TMP_Text))]
    public class TypeWriter : MonoBehaviour
    {
        private TMP_Text _textBox;
        //This is just some sample text for working with before implementation
        private AudioClip typewriterAudio = null;

        // Start is called before the first frame update
        private int _currentVisibleCharacterIndex; //The position of the terminal
        private Coroutine _typewriterCoroutine;
        private bool _readyForNewText = true;


        private WaitForSeconds _simpleDelay; //To get that type writer delay
        private WaitForSeconds _interpunctuationDelay; //Incase we want to stop the type writer midway for some reason

        [Header("Typewriter Settings")]
        [SerializeField] private float charPerSecond = 50;
        [SerializeField] private float interpunctuationDelay = 0.02f;

        //To skip stuff
        public bool CurrentlySkipping { get; private set; }
        private bool AdvanceInput { get { return Input.GetMouseButton(1) || Input.anyKey; } }

        private WaitForSeconds _skipDelay;

        [Header("Skip Options")]
        [SerializeField] private bool quickSkip;
        [SerializeField] [Min(1)] private int skipSpeedup = 10;

        private WaitForSeconds _textboxFullEventDelay;
        [SerializeField] [Range(0.1f, 0.5f)] private float sendDoneDelay = 0.25f;
        private int _pauseInCharacters;

        public event Action CompleteTextRevealed;
        public event Action<char> CharacterRevealed;

        public bool automatic = false;

        private GlobalControlScript globalControl;
        public bool sound = true;

        private void Awake()
        {
            _textBox = GetComponent<TMP_Text>();



            _simpleDelay = new WaitForSeconds(1 / charPerSecond);
            _interpunctuationDelay = new WaitForSeconds(interpunctuationDelay);

            _skipDelay = new WaitForSeconds(1 / (charPerSecond * skipSpeedup));
            _textboxFullEventDelay = new WaitForSeconds(sendDoneDelay);

            typewriterAudio = (AudioClip)Resources.Load("keyboard-noise-quiet");
        }

        private void Enable()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(LoadNextText);
        }

        private void Disable()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(LoadNextText);
        }

        public void StopTypeWriter()
        {
            StopCoroutine(_typewriterCoroutine);
            _readyForNewText = true;
            CurrentlySkipping = false;
        }

        private void Update()
        {
            if (!automatic && Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.Space)) 
            {
                if (_textBox.maxVisibleCharacters != _textBox.textInfo.characterCount - 1)
                    Skip();
            }   
        }

        public void LoadNextText(UnityEngine.Object obj) 
        {
            if(!_readyForNewText)
                return;

            globalControl = GameObject.Find("GlobalControl").GetComponent<GlobalControlScript>();
            quickSkip = globalControl.quickTextSkip;

            _readyForNewText = false;
            //Typewriter becomes weird if you have 2 going off at the same time
            if (_typewriterCoroutine != null)
                StopCoroutine(_typewriterCoroutine);

            
            _textBox.maxVisibleCharacters = 0;
            _currentVisibleCharacterIndex = 0;

            _typewriterCoroutine = StartCoroutine(TypeWriterCoroutine());
        }

        private IEnumerator DestroyAudio(GameObject gameObjAudio)
        {
            // first wait for the audio clip to finish
            AudioSource audioSource = gameObjAudio.GetComponent<AudioSource>();
            yield return new WaitForSeconds(audioSource.clip.length);

            // now destroy audio source then game object
            Destroy(audioSource);
            Destroy(gameObjAudio);
        }

        private IEnumerator TypeWriterCoroutine() 
        {
            TMP_TextInfo textInfo = _textBox.textInfo;
            _pauseInCharacters = -1;

            while (_currentVisibleCharacterIndex < textInfo.characterCount + 1) 
            {
                if (_currentVisibleCharacterIndex % 5 == 0 && UIManager.soundOn && sound)
                {
                    GameObject tmpGameObjAudio = new GameObject();
                    tmpGameObjAudio.AddComponent<AudioSource>();
                    AudioSource tmpAudioSource = tmpGameObjAudio.GetComponent<AudioSource>();
                    tmpAudioSource.clip = typewriterAudio;
                    tmpAudioSource.Play();
                    StartCoroutine(DestroyAudio(tmpGameObjAudio));
                }

                var lastCharIndex = textInfo.characterCount - 1;

                if (_currentVisibleCharacterIndex == lastCharIndex) 
                {
                    _textBox.maxVisibleCharacters++;
                    yield return _textboxFullEventDelay;
                    CompleteTextRevealed?.Invoke();
                    _readyForNewText = true;
                    yield break;
                }
                char character = textInfo.characterInfo[_currentVisibleCharacterIndex].character; //instead of checking the text box.text value as our text might contain text like \n
                _textBox.maxVisibleCharacters++;

                if (automatic &&
                    (character == '?' || character == '.' || character == ',' || character == ';' ||
                    character == ':' || character == '!' || character == '-'))
                {
                    yield return _interpunctuationDelay;
                }
                if (!automatic && character == '>' && _currentVisibleCharacterIndex != 0)
                {
                    _pauseInCharacters = 1;
                }
                else if (automatic && character == '>')
                {
                    yield return new WaitForSeconds(1);
                }
                if (_pauseInCharacters == 0)
                {
                    yield return new WaitForSeconds(0.2f);
                    var oldText = _textBox.text;
                    var blinkingCursorRoutine = StartCoroutine(BlinkingCursor());
                    yield return new WaitUntil(() => AdvanceInput);
                    StopCoroutine(blinkingCursorRoutine);
                    //We must reset he text as the cursor actually overrides the text!
                    _textBox.text = oldText;
                    _pauseInCharacters = -1;
                }
                else
                {
                    yield return CurrentlySkipping ? _skipDelay : _simpleDelay;
                }
                _pauseInCharacters--;
                CharacterRevealed?.Invoke(character);
                _currentVisibleCharacterIndex++;
            }
            CompleteTextRevealed?.Invoke();
        }

        private IEnumerator BlinkingCursor()
        {
            StringBuilder builder = new StringBuilder(_textBox.text);
            while (true)
            {
                builder[_currentVisibleCharacterIndex] = '_';
                _textBox.text = builder.ToString();
                yield return new WaitForSeconds(0.2f);
                builder[_currentVisibleCharacterIndex] = ' ';
                _textBox.text = builder.ToString();
                yield return new WaitForSeconds(0.2f);
            }
        }

        void Skip()
        {
            if (CurrentlySkipping)
                return;

            CurrentlySkipping = true;

            if (!quickSkip)
            {
                StartCoroutine(SkipSpeedupReset());
                return;
            }

            StopCoroutine(_typewriterCoroutine);
            _textBox.maxVisibleCharacters = _textBox.textInfo.characterCount;
            _readyForNewText = true;
            CompleteTextRevealed?.Invoke();
        }

        private IEnumerator SkipSpeedupReset()
        {
            while (AdvanceInput)
            {
                yield return null;
            }
            CurrentlySkipping = false;
        }

        // Update is called once per frame
        //Note if game is bugging try commenting out to check if this is causing it, if we want to implement it
        //another way is to input action for a button click in UI  Action maps Control schemes in Unity

    }
}
