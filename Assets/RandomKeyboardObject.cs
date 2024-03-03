using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using System.Threading;
using System.Linq;
//using static UnityEditor.PlayerSettings;
using System.Numerics;
using Unity.Mathematics;

public class RandomKeyboardObject : MonoBehaviour
{
    // For random generation.
    static System.Random random = new System.Random();

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
    Color blackKeyColour = new Color(159.0f / 255.0f, 159.0f / 255.0f, 159.0f / 255.0f);

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

    // For totient and other factoring things:
    static int maxPrimeNeeded = 1000000;
    List<int> totients = Enumerable.Repeat(0, maxPrimeNeeded).ToList();
    List<int> primes = new List<int>();
    Dictionary<int, HashSet<int>> divisors = new Dictionary<int, HashSet<int>>();
    List<List<int>> currentScaledCycleIndexPoly = new List<List<int>>();
    BigInteger totalWeight;


    // Start is called before the first frame update
    void Start()
    {
        // Define the default key list
        defaultKeyList = new List<Button> { cKey, cSharpKey, dKey, dSharpKey, eKey, fKey, fSharpKey, gKey, gSharpKey, aKey, aSharpKey, bKey };
        defaultKeyColours = new List<Color> { whiteKeyColour, blackKeyColour, whiteKeyColour, blackKeyColour, whiteKeyColour, whiteKeyColour, blackKeyColour, whiteKeyColour, blackKeyColour, whiteKeyColour, blackKeyColour, whiteKeyColour };

        // Populate primes and totients
        getPrimes();
        
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
        keyboardWidth = (customKeyboardBase.GetComponent<RectTransform>().rect.width / 800) * Screen.width;
        UnityEngine.Debug.Log("keyboardWidth = " + keyboardWidth);
        UnityEngine.Debug.Log("Screen.width = " + Screen.width);
        //keyboardWidth = customKeyboardBase.GetComponent<RectTransform>().localScale.x;
        float keyWidth = keyboardWidth / numNotes;
        UnityEngine.Debug.Log("keyWidth: " + keyWidth);
        keyHeight = customKeyboardBase.GetComponent<RectTransform>().rect.height;

        UnityEngine.Vector3 positionDelta = new UnityEngine.Vector3(keyWidth, 0, 0);
        UnityEngine.Vector3 firstNotePosition = positionDummy.transform.position + positionDelta * 0.5f;
        for (int i = 0; i < numNotes; i++)
        {
            //UnityEngine.Debug.Log("Current position: " + (firstNotePosition + positionDelta * i));
            GameObject newKey = Instantiate(baseKey, firstNotePosition + positionDelta * i, UnityEngine.Quaternion.identity, customKeyboardBase.transform);
            //newKey.GetComponent<RectTransform>().SetPositionAndRotation(firstNotePosition + positionDelta * i, Quaternion.identity);

            //newKey.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 60.0f*12/numNotes);
            //newKey.GetComponent<RectTransform>().localScale = new Vector3(12.0f/numNotes, 1, 1);
            newKey.GetComponent<RectTransform>().sizeDelta = new UnityEngine.Vector2(37f * 12 / numNotes, 192.4311f);
            UnityEngine.Debug.Log("Width of key: " + newKey.GetComponent<RectTransform>().rect.width);
            newKey.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, keyHeight);
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
        getScaledCycleIndexPoly(numNotes);
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

    public void generateChord_OLD()
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

    int totient(int numIn)
    {
        if (totients[numIn] > 0)
        {
            return totients[numIn];
        }
        else
        {
            // Have to actually calculate the totient
            int totientSofar = 0;
            // Doing this in a naive way for now
            for (int i = 1; i < numIn; i++)
            {
                if (gcd(numIn, i) == 1)
                {
                    totientSofar += 1;
                }
            }
            totients[numIn] = totientSofar;
            return totientSofar;
        }
    }

    void getPrimes()
    {
        // Sieve of Eratosthenes
        
        List<bool> isPrimeList = Enumerable.Repeat(true, maxPrimeNeeded).ToList();
        // 0 and 1 are composite
        isPrimeList[0] = false;
        isPrimeList[1] = false;

        // First few totients
        totients[0] = 1;
        totients[1] = 1;

        // First few factors
        divisors[0] = new HashSet<int> { 1 };
        divisors[1] = new HashSet<int> { 1 };

        // Deal with 2 as a special case
        for (int i = 4; i < maxPrimeNeeded; i += 2)
        {
            isPrimeList[i] = false;
        }
        // Now only need to worry about odd numbers
        for (int i = 5; i < maxPrimeNeeded; i += 2)
        {
            if (isPrimeList[i])
            {
                for (int j = 2 * i; j < maxPrimeNeeded; j += i)
                {
                    isPrimeList[j] = false;
                }
            }
        }
        // Now we've done the sieve and can see which values are prime
        for (int i = 2; i < maxPrimeNeeded; i++)
        {
            if (isPrimeList[i])
            {
                primes.Add(i);
                totients[i] = i - 1;
                divisors[i] = new HashSet<int> { 1, i };

            }
        }
        //UnityEngine.Debug.Log("Primes: " + String.Join(", ", primes));
    }

    HashSet<int> getDivisors (int numIn)
    {
        if (divisors.ContainsKey(numIn))
        {
            return divisors[numIn];
        }
        else if (numIn <= 0)
        {
            // In this context, we don't want 0 or negative numbers.
            return divisors[0] ;
        }
        else
        {
            HashSet<int> newDivisors = new HashSet<int> { 1, numIn };
            double maxToTest = Math.Sqrt(numIn);
            for (int i = 2; i < maxToTest; i++)
            {
                if (numIn % i == 0)
                {
                    newDivisors.Add(i);
                    newDivisors.Add(numIn / i);
                }
            }
            divisors[numIn] = newDivisors;
            return newDivisors;
        }
        
    }

    int gcd(int aIn, int bIn)
    {
        // Euclidean algorithm
        // Code borrowed from https://stackoverflow.com/questions/18541832/c-sharp-find-the-greatest-common-divisor
        int a = aIn;
        int b = bIn;

        while (a != 0 && b != 0)
        {
            if (a > b)
                a %= b;
            else
                b %= a;
        }
        //UnityEngine.Debug.Log("gcd = " + (a + b));
        return a + b; // Since one of a or b will be 0
    }

    void getScaledCycleIndexPoly(int numIn)
    {
        // Need to get divisors
        List<int> currentDivisors = getDivisors(numIn).ToList();
        // Then get the totients as the coefficients
        currentScaledCycleIndexPoly = new List<List<int>>();
        totalWeight = 0;
        for (int i = 0; i < currentDivisors.Count; i++)
        {
            // Note: In this context, we don't care about the subscript of the variable.
            int newTotient = totient(currentDivisors[i]);
            currentScaledCycleIndexPoly.Add(new List<int> { newTotient, numIn / currentDivisors[i] });
            totalWeight += newTotient;
        }
        UnityEngine.Debug.Log("Printing currentScaledCycleIndexPoly: ");
        string toPrint = "";
        for (int i = 0; i < currentScaledCycleIndexPoly.Count; i++)
        {
            toPrint += " + " + currentScaledCycleIndexPoly[i][0].ToString() + "x^" + currentScaledCycleIndexPoly[i][1].ToString();
        }
        UnityEngine.Debug.Log(toPrint);

        totalWeight = 0;
        for (int i = 0; i < currentScaledCycleIndexPoly.Count; i++)
        {
            //totalWeight += currentScaledCycleIndexPoly[i][0] * (int)System.Math.Pow(2, currentScaledCycleIndexPoly[i][1]);
            totalWeight += (BigInteger)currentScaledCycleIndexPoly[i][0] * BigInteger.Pow(2, currentScaledCycleIndexPoly[i][1]);
        }
        UnityEngine.Debug.Log("totalWeight = " + totalWeight);
    }

    public void generateChord()
    {
        // First, must figure out which kind of cycles we're using
        System.Random random = new System.Random();
        //int diceRoll = random.Next(0, totalWeight);
        BigInteger diceRoll = RandomIntegerBelow(totalWeight);
        //int sumSoFar = 0;
        BigInteger sumSoFar = 0;
        int currentIndex = -1;
        while (sumSoFar <= diceRoll)
        {
            currentIndex += 1;
            sumSoFar += (BigInteger)currentScaledCycleIndexPoly[currentIndex][0] * BigInteger.Pow(2, currentScaledCycleIndexPoly[currentIndex][1]);
        }
        // The current index points to the term in the current cycle index polynomial we're using
        // We will take n to be numNotes
        List<int> colouringChoice = new List<int>();
        int numCycles = currentScaledCycleIndexPoly[currentIndex][1];
        for (int i = 0; i < numCycles; i++)
        {
            colouringChoice.Add(random.Next(0, 2));
        }
        string toReturn = "";
        for (int i = 0; i < numNotes; i++)
        {
            toReturn += colouringChoice[i % numCycles].ToString();
        }
        currentChord = makeLeftHeavy(toReturn, numCycles);


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

    string makeLeftHeavy(string inBinString, int numCycles)
    {
        
        List<int> prefixCount = getPrefixSequence(inBinString);
        string currentBest = inBinString;
        string rotatedString = inBinString;
        for (int i = 0; i < numCycles; i++)
        {
            rotatedString = rotatedString.Remove(0, 1) + rotatedString.Substring(0, 1);
            List<int> rotatedPrefixCount = getPrefixSequence(rotatedString);
            bool isLeftHeavy = false;
            for (int j = 0; j < numNotes && !isLeftHeavy; j++)
            {
                if (rotatedPrefixCount[j] < prefixCount[j])
                {
                    isLeftHeavy = false;
                    break;
                }
                else if (rotatedPrefixCount[j] > prefixCount[j])
                {
                    isLeftHeavy = true;
                }
            }
            if (isLeftHeavy)
            {
                prefixCount = rotatedPrefixCount;
                currentBest = rotatedString;
            }
        }
        return currentBest;
    }

    public static BigInteger RandomIntegerBelow(BigInteger N)
    {
        // Source: https://stackoverflow.com/questions/17357760/how-can-i-generate-a-random-biginteger-within-a-certain-range
        byte[] bytes = N.ToByteArray();
        BigInteger R;

        do
        {
            random.NextBytes(bytes);
            bytes[bytes.Length - 1] &= (byte)0x7F; //force sign bit to positive
            R = new BigInteger(bytes);
        } while (R >= N);

        return R;
    }
}
