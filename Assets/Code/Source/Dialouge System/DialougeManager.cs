using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace B1TJam2025
{
    public class DialougeManager : MonoBehaviour
    {
        [Header("Settings")]

        [SerializeField]
        [Tooltip("Keycodes for progressing dialouge")]
        private List<KeyCode> _progressDialougeInputs;

        [Header("Refrences")]

        [SerializeField]
        [Tooltip("The dialouge writer that writes to the box")]
        private DialougeWriter _writer;

        [SerializeField]
        [Tooltip("The dialouge box object")]
        private GameObject _dialougeBox;

        //private vars
        private Coroutine _conversationRoutine = null;
        private bool _input = false;

        //events
        private Action<string> onTextClear;
        private void AddListenerToOnTextClear(Action<string> callback) => onTextClear += callback;
        private void RemoveListenerFromOnTextClear(Action<string> callback) => onTextClear -= callback;

        private void Awake()
        {
            if(_writer == null) _writer = FindAnyObjectByType<DialougeWriter>(); //if not assigned, finds in scene
        }

        private void Update()
        {
            //input detection
            _input = false;
            foreach (KeyCode keyCode in _progressDialougeInputs)
            {
                //if progress button is pressed while writing instantly finishes text writing
                if (Input.GetKeyDown(keyCode))
                {
                    _input = true;
                    break;
                }
            }
        }


        public void InitiateConversation(Conversation conversation, bool overrideCurrent = false)
        {
            if(_conversationRoutine != null)
            {
                if (overrideCurrent)
                {
                    StopCoroutine(_conversationRoutine);
                    _conversationRoutine = StartCoroutine(ConversationRoutine(conversation));
                }
            }
            else _conversationRoutine = StartCoroutine(ConversationRoutine(conversation));
        }

        private IEnumerator ConversationRoutine(Conversation c)
        {
            WaitForEndOfFrame frameDelay = new();
            _dialougeBox.SetActive(true);

            for(int i = 0; i < c.Lines.Count; i++)
            {
                _writer.WriteText(c.Lines[i]);

                //while writing out
                while(_writer.WritingState == DialougeWriter.State.Writing)
                {
                    if (_input)
                    {
                        _writer.InstantFinishText();
                        _input = false;
                    }
                    yield return frameDelay;
                }

                //while written out
                while (_writer.WritingState == DialougeWriter.State.Complete)
                {
                    // clears text, triggers event. 
                    if (_input) 
                    {
                        _writer.ClearText();

                        //event trigger
                        if (c.Lines[i].FireEventOnClear) onTextClear?.Invoke(c.Lines[i].EventCode);

                        _input = false;
                        break;
                    }
                    yield return frameDelay;
                }
            }

            _dialougeBox.SetActive(false);
            _conversationRoutine = null;
        }

    }
}
