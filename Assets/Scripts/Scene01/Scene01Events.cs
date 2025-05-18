using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene01Events : MonoBehaviour
{
    public GameObject fadeScreenIn;
    public GameObject charShian;
    public GameObject charLubelia;
    public GameObject textBox;

    [SerializeField] string textToSpeak;
    [SerializeField] int currentTextLength;
    [SerializeField] int textLength;
    [SerializeField] GameObject mainTextObject;
    [SerializeField] GameObject nextButton;
    [SerializeField] int eventPos = 0;
    [SerializeField] GameObject charName;
    [SerializeField] GameObject fadeOut;

    void Update()
    {
        textLength = TextCreator.charCount;
    }

    void Start()
    {
        Debug.Log("Scene01Events started");
        StartCoroutine(EventStarter());
    }

    IEnumerator EventStarter() 
    {
        // event 0
        fadeScreenIn.SetActive(true);
        yield return new WaitForSeconds(2);
        fadeScreenIn.SetActive(false);
        mainTextObject.SetActive(true);

        StartCoroutine(Event0());
    }

    IEnumerator Event0() 
    {
        charShian.SetActive(true);
        StartCoroutine(TextEvent("시안", "우리가 싸우는 이유는 강자이기 때문이야. 강자는 약자를 지켜줘야만 해"));
        yield return new WaitForSeconds(1);
    }

    IEnumerator Event1() 
    {
        charLubelia.SetActive(true);
        StartCoroutine(TextEvent("루벨리아", "그건 그냥 이상론에 불과한거 아닌가요?"));
        yield return new WaitForSeconds(1);
    }

    IEnumerator Event2() 
    {
        StartCoroutine(TextEvent("시안", "이상론에 불과할지라도, 나는 해야만 하는 일이라고 생각해."));
        yield return new WaitForSeconds(1);
    }

    IEnumerator Event3() {
        StartCoroutine(TextEvent("", "시안의 표정에서는 그녀의 굳은 의지가 보이는 듯 했다."));
    
        yield return new WaitForSeconds(1);
    }

    IEnumerator Event4() 
    {
        
        fadeOut.SetActive(true);
        yield return new WaitForSeconds(2);
    }

    IEnumerator TextEvent(string charNameText, string text)
    {
        nextButton.SetActive(false);
        textBox.SetActive(true);
        charName.GetComponent<TMPro.TMP_Text>().text = charNameText;
        textToSpeak = text;
        textBox.GetComponent<TMPro.TMP_Text>().text = textToSpeak;
        currentTextLength = textToSpeak.Length;
        TextCreator.runTextPrint = true;
        yield return new WaitForSeconds(0.05f);
        yield return new WaitForSeconds(1);
        yield return new WaitUntil(() => textLength  == currentTextLength);
        yield return new WaitForSeconds(0.05f);
        nextButton.SetActive(true);
        eventPos += 1;
        yield return new WaitForSeconds(0.05f);
    }

    public void NextButton() 
    {
        if (eventPos == 1) 
        {
            StartCoroutine(Event1());
        }

        if (eventPos == 2) 
        {
            StartCoroutine(Event2());
        }

        if (eventPos == 3) 
        {
            StartCoroutine(Event3());
        }

        if (eventPos == 4) 
        {
            StartCoroutine(Event4());
        }
    }
}
