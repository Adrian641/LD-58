using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NPC : MonoBehaviour
{
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public string[] dialogue;
    private int index;

    public GameObject contButton;
    public GameObject startButton;
    public float wordSpeed;
    public bool playerIsClose;
    public bool canNext;



     void Start()
    {
        canNext = true;   
    }
    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.E) && playerIsClose && canNext)
        {
            if(dialoguePanel.activeInHierarchy)
            {
                canNext = false;
                NextLine();
            }
            else
            {
                dialoguePanel.SetActive(true);
                canNext = false;
                StartCoroutine(TypeLine());
            }
        }

        if (dialogueText.text == dialogue[index])
        {
            canNext = true;
            contButton.SetActive(true);
        }
    }

    public void zeroText()
    {
        dialogueText.text = "";
        index = 0;
        dialoguePanel.SetActive(false);
    }

    IEnumerator TypeLine()
    {
        foreach (char c in dialogue[index].ToCharArray())
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(wordSpeed);
        }
    }

    public void NextLine()
    {
        contButton.SetActive(false);

        if (index < dialogue.Length - 1)
        {
            index++;
            dialogueText.text = "";
            StartCoroutine(TypeLine());
        }
        else
        {
            startButton.SetActive(false);
            zeroText();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {

            if (canNext)
            {
                startButton.SetActive(true);
            }
            else
            {
                startButton.SetActive(false);
            }

            playerIsClose = true;
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            startButton.SetActive(false);
            playerIsClose = false;
            zeroText();
        }
    }
}
