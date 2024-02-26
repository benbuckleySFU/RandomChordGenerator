using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using System.Threading;

public class RandomKeyboardObject : MonoBehaviour
{
    // Objects for the default 12-key keyboard:
    public Button cKey;
    public Button cSharpKey;
    public Button dKey;
    public Button dSharpKey;
    public Button eKey;
    public Button fKey;
    public Button fSharpKey;
    public Button gKey;
    public Button gSharpKey;
    public Button aKey;
    public Button aSharpKey;
    public Button bKey;
    List<Button> defaultKeyList;
    List<Color> defaultKeyColours;

    Color whiteKeyColour = Color.white;
    Color blackKeyColour = new Color(159.0f/255.0f, 159.0f / 255.0f, 159.0f / 255.0f);

    public GameObject positionDummy;

    int numNotes = 12;

    // Values needed for creating custom keyboard
    float keyboardWidth = 448;
    float keyboardLeft = -224;
    float keyboardRight = 224;
    float keyHeight = 256.43f;
    List<GameObject> customKeyList = new List<GameObject>();
    public GameObject customKeyboardBase;
    public GameObject baseKey;

    public TMP_InputField numNotesField;

    // For generating and displaying chords
    string currentChord = "000000000000";

    // For playing arpeggios
    bool playingArpeggio = true;
    float arpeggioInterval = 1.0f / 10.0f;
    float nextNoteTime = 0f;
    int arpIndex = 0;



    // Start is called before the first frame update
    void Start()
    {
        // Define the default key list
        defaultKeyList = new List<Button> {cKey, cSharpKey, dKey, dSharpKey, eKey, fKey, fSharpKey, gKey, gSharpKey, aKey, aSharpKey, bKey};
        defaultKeyColours = new List<Color> { whiteKeyColour, blackKeyColour, whiteKeyColour, blackKeyColour, whiteKeyColour, whiteKeyColour, blackKeyColour, whiteKeyColour, blackKeyColour, whiteKeyColour, blackKeyColour, whiteKeyColour};

        // Test custom keyboard
        // numNotes = 20;
        //createCustomKeyboard();
        /*
        for (int i = 0; i < 100; i++)
        {
            generateChord();
            UnityEngine.Debug.Log("New chord: " + currentChord);
        }
        */
    }

    // Update is called once per frame
    void Update()
    {
        // Use this for arpeggios
        if (Time.time >= nextNoteTime)
        {
            
            //UnityEngine.Debug.Log("Updating time!");
            if (playingArpeggio)
            {
                playOneArpNote();
            }

        }
    }

    void generateChordsNaive()
    {
        // So, how is this going to work?
        // Generate every binary string of length numNotes. Reject the ones that are already in the hashset.
        HashSet<string> chords = new HashSet<string>();
        // i from 0 to 2^numNotes, exclusive of the last number.
    }

    void createCustomKeyboard()
    {
        // Destroy old keyboard
        for (int i = 0; i < customKeyList.Count; i++)
        {
            Destroy(customKeyList[i]);
        }
        // Make custom keyboard active
        customKeyboardBase.SetActive(true);
        customKeyList = new List<GameObject>();
        float keyWidth = keyboardWidth / numNotes;
        
        Vector3 positionDelta = new Vector3(keyWidth, 0,0);
        Vector3 firstNotePosition = positionDummy.transform.position + positionDelta * 0.5f;
        for (int i = 0; i < numNotes; i++)
        {
            UnityEngine.Debug.Log("Current position: " + (firstNotePosition + positionDelta * i));
            GameObject newKey = Instantiate(baseKey, firstNotePosition + positionDelta * i, Quaternion.identity, customKeyboardBase.transform);
            //newKey.GetComponent<RectTransform>().SetPositionAndRotation(firstNotePosition + positionDelta * i, Quaternion.identity);
            newKey.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, keyWidth);
            // Set pitch
            float newPitch = (float)Math.Pow(2, i / (1.0f * numNotes));
            newKey.GetComponent<AudioSource>().pitch = newPitch;
            customKeyList.Add(newKey);
        }
    }
    void setKeyboard(int numNotesIn)
    {
        currentChord = "";
        numNotes = Math.Max(numNotesIn, 1); // Must have at least one note
        if (numNotes == 12)
        {
            customKeyboardBase.SetActive(false);
        }
        else
        {
            createCustomKeyboard();
        }
    }

    public void getNumNotesInput()
    {
        string numNotesIn = numNotesField.text;
        bool success = int.TryParse(numNotesIn, out numNotes);
        if (success)
        {
            setKeyboard(numNotes);
        }
    }

    public void generateChord()
    {
        bool isLeftHeavy = false;
        string toReturn = "";
        while (!isLeftHeavy)
        {
            // First, generate a random string of 1s and 0s of length numNotes
            toReturn = "";
            for (int i = 0; i < numNotes; i++)
            {
                int toAppend = UnityEngine.Random.Range(0, 2);
                toReturn += toAppend.ToString();
            }

            // Then, check if it's in its most left-heavy rotation. If it is, accept it!
            List<int> prefixCount = getPrefixSequence(toReturn);
            string rotatedString = toReturn;
            isLeftHeavy = true;
            for (int i = 1; i < numNotes && isLeftHeavy; i++)
            {
                rotatedString = rotatedString.Remove(0, 1) + rotatedString.Substring(0, 1);
                List<int> rotatedPrefixCount = getPrefixSequence(rotatedString);
                for (int j = 0; j < numNotes && isLeftHeavy; j++)
                {
                    if (rotatedPrefixCount[j] > prefixCount[j])
                    {
                        isLeftHeavy = false;
                    }
                }
            }


        }


        currentChord = toReturn;

        if (numNotes == 12)
        {
            for (int i = 0; i < numNotes; i++)
            {
                if (currentChord[i] == '1')
                {
                    defaultKeyList[i].GetComponent<Image>().color = new Color(243.0f / 255.0f, 175.0f / 255.0f, 162.0f / 255.0f);
                }
                else
                {
                    defaultKeyList[i].GetComponent<Image>().color = defaultKeyColours[i];
                }
            }
        }
        else
        {
            for (int i = 0; i < numNotes; i++)
            {
                if (currentChord[i] == '1')
                {
                    customKeyList[i].GetComponent<Image>().color = new Color(243.0f / 255.0f, 175.0f / 255.0f, 162.0f / 255.0f);
                }
                else
                {
                    customKeyList[i].GetComponent<Image>().color = whiteKeyColour;
                }
            }
        }
        
    }

    List<int> getPrefixSequence(string binString)
    {
        List<int> toReturn = new List<int>();
        int currentCount = 0;
        for (int i = 0; i < binString.Length; i++)
        {
            if (binString[i] == '1')
            {
                currentCount += 1;
            }
            toReturn.Add(currentCount);
        }
        return toReturn;
    }

    public void playChord()
    {
        if (numNotes == 12)
        {
            for (int i = 0; i < currentChord.Length; i++)
            {
                if (currentChord[i] == '1')
                {
                    defaultKeyList[i].GetComponent<AudioSource>().Play();
                }
            }
        }   
        else
        {
            for (int i = 0; i < currentChord.Length; i++)
            {
                if (currentChord[i] == '1')
                {
                    customKeyList[i].GetComponent<AudioSource>().Play();
                }
            }
        }
    }

    public void playArpeggio()
    {
        playingArpeggio = true;
        arpIndex = 0;
        nextNoteTime = Time.time;
        /*
        float arpeggioInterval = 1.0f / 10.0f;
        float startTime = Time.time;
        float nextNoteTime = startTime;
        for (int i = 0; i < currentChord.Length; i++)
        {
            if (currentChord[i] == '1')
            {
                if (numNotes == 12)
                {
                    defaultKeyList[i].GetComponent<AudioSource>().Play();
                }
                else
                {
                    customKeyList[i].GetComponent<AudioSource>().Play();
                }
                Thread.Sleep(100);
                nextNoteTime = nextNoteTime + arpeggioInterval;
            }
        }
        */
    }


    public void playOneArpNote()
    {
        // Find next index for which there is a note.
        // If it doesn't exist, then set playingArpeggio = false
        bool notePlayed = false;
        for (int i = arpIndex; !notePlayed && i < currentChord.Length; i++)
        {
            if (currentChord[i] == '1')
            {
                if (numNotes == 12)
                {
                    defaultKeyList[i].GetComponent<AudioSource>().Play();
                }
                else
                {
                    customKeyList[i].GetComponent<AudioSource>().Play();
                }
                notePlayed = true;
                nextNoteTime += arpeggioInterval;
                arpIndex = i + 1;
            }
        }
        if (!notePlayed)
        {
            // This means the loop finished without finding any more notes.
            playingArpeggio = false;
        }
    }
}
