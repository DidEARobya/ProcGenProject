using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Loading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

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
        AddItem(1);
        AddItem(2);
        AddItem(3);
        AddItem(4);
    }
    public bool AddItem(int item)
    {
        bool added = false;
        int slotIndex = 0;

        for(int i = 0; i < slots.Length - 1; i++)
        {
            if (itemIndex[i] == 0)
            {
                itemIndex[i] = item;
                slotIndex = i;
                added = true;

                break;
            }
        }

        if(added == false)
        {
            return false;
        }

        slots[slotIndex].GetComponent<Image>().sprite = worldManager.blockData[item].displayImage;

        return true;
    }

    public void RemoveItem(int item)
    {
        if (item == 0)
        {
            return;
        }

        for(int i = 0; i < itemIndex.Length; i++)
        {
            if (itemIndex[i] == item)
            {
                itemIndex[i] = 0;
                slots[i].GetComponent<Image>().sprite = emptySlot;
            }
        }
    }
    public void RemoveItemAtSlot(int slot)
    {
        if (slot == 0 || itemIndex[slot - 1] == 0)
        {
            return;
        }

        itemIndex[slot - 1] = 0;
        slots[slot - 1].GetComponent<Image>().sprite = emptySlot;
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
                    RemoveItemAtSlot(currentBlockIndex);
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
