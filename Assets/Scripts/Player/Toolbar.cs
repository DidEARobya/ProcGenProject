using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using Unity.Loading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class Toolbar : MonoBehaviour
{
    private KeyCode keyPressed = new KeyCode();

    [SerializeField]
    public GameObject toolbar;
    [SerializeField]
    public GameObject[] slots;

    [SerializeField]
    public Sprite emptySlot;

    private int[] itemIndex;
    private int selectedBlockIndex;
    private int currentBlockIndex;

    private WorldManager worldManager;
    private PlayerController playerController;

    public byte currentItem;

    public bool isCreative;

    private void Start()
    {
        itemIndex = new int[slots.Length];
        worldManager = WorldManager.instance;
        playerController = WorldManager.instance.player.gameObject.GetComponent<PlayerController>();
        playerController.toolbar = this;

        if(isCreative == true)
        {
            CreativeMode();
        }
    }
    private void Update()
    {
        Inputs();
    }

    private void CreativeMode()
    {
        AddItem(1, 1);
        AddItem(2, 1);
        AddItem(3, 1);
        AddItem(4, 1);
        AddItem(5, 1);
    }
    public bool AddItem(int item, int amount)
    {
        TextMeshProUGUI amountText = null;
        int current;
        bool isParsed;

        for (int i = 0; i < slots.Length; i++)
        {
            if (itemIndex[i] == item)
            {
                amountText = slots[i].transform.Find("Amount").GetComponent<TextMeshProUGUI>();

                isParsed = int.TryParse(amountText.text, out current);

                if (isParsed == true && current < worldManager.blockData[item].stackSize)
                {
                    current += amount;
                    amountText.text = current.ToString();

                    return true;
                }

            }
        }

        for(int i = 0; i < slots.Length; i++)
        {
            if (itemIndex[i] == 0)
            {
                itemIndex[i] = item;

                amountText = slots[i].transform.Find("Amount").GetComponent<TextMeshProUGUI>();

                current = amount;
                amountText.text = current.ToString();

                slots[i].GetComponent<UnityEngine.UI.Image>().sprite = worldManager.blockData[item].displayImage;

                return true;
            }
        }

        return false;
    }

    public void RemoveItem(int item, int amount)
    {
        if (item == 0)
        {
            return;
        }

        for (int i = 0; i < itemIndex.Length; i++)
        {
            if (itemIndex[i] == item)
            {
                TextMeshProUGUI text = slots[i].transform.Find("Amount").GetComponent<TextMeshProUGUI>();

                int current = int.Parse(text.text);

                if (current - amount <= 0)
                {
                    itemIndex[i] = 0;
                    slots[i].GetComponent<UnityEngine.UI.Image>().sprite = emptySlot;
                    text.text = " ";
                }
                else
                {
                    text.text = (current - amount).ToString();
                }

                break;
            }
        }
    }

    public void RemoveItemAtSlot(int slot, int amount)
    {
        if (slot == 0 || itemIndex[slot - 1] == 0)
        {
            return;
        }

        TextMeshProUGUI text = slots[slot - 1].transform.Find("Amount").GetComponent<TextMeshProUGUI>();

        int current = int.Parse(text.text);

        if (current - amount <= 0)
        {
            itemIndex[slot - 1] = 0;
            slots[slot - 1].GetComponent<UnityEngine.UI.Image>().sprite = emptySlot;
            text.text = " ";
        }
        else
        {
            text.text = (current - amount).ToString();
        }
    }

    private void OnGUI()
    {
        Event keyPress = Event.current;

        if (keyPress.isKey)
        {
            keyPressed = keyPress.keyCode;
        }
    }

    private void Inputs()
    {
        selectedBlockIndex = 0;

        if(keyPressed != KeyCode.None)
        {
            switch(keyPressed)
            {
                case KeyCode.G:
                    RemoveItemAtSlot(currentBlockIndex, 1);
                    break;
                case KeyCode.H:
                    CreativeMode();
                    break;

                case KeyCode.Alpha1:
                    selectedBlockIndex = 1;
                    break;
                case KeyCode.Alpha2:
                    selectedBlockIndex = 2;
                    break;
                case KeyCode.Alpha3:
                    selectedBlockIndex = 3;
                    break;
                case KeyCode.Alpha4:
                    selectedBlockIndex = 4;
                    break;
                case KeyCode.Alpha5:
                    selectedBlockIndex = 5;
                    break;
                case KeyCode.Alpha6:
                    selectedBlockIndex = 6;
                    break;
                case KeyCode.Alpha7:
                    selectedBlockIndex = 7;
                    break;
                case KeyCode.Alpha8:
                    selectedBlockIndex = 8;
                    break;
                case KeyCode.Alpha9:
                    selectedBlockIndex = 9;
                    break;
            }

            keyPressed = KeyCode.None;
        }

        if(selectedBlockIndex != 0 && selectedBlockIndex != currentBlockIndex)
        {
            if(currentBlockIndex != 0)
            {
                slots[currentBlockIndex - 1].transform.Find("Border").gameObject.SetActive(false);
            }

            currentBlockIndex = selectedBlockIndex;
            playerController.toolbarIndex = currentBlockIndex;

            slots[currentBlockIndex - 1].transform.Find("Border").gameObject.SetActive(true);
        }

        if(currentBlockIndex != 0 && itemIndex[currentBlockIndex - 1] != 0)
        {
            playerController.selectedBlockIndex = itemIndex[currentBlockIndex - 1];
            return;
        }

        playerController.selectedBlockIndex = 0;
    }
}
