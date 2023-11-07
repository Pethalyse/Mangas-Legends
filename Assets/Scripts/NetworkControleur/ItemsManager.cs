using AYellowpaper.SerializedCollections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemsManager : MonoBehaviour
{
    public static ItemsManager instance;

    [SerializeField] private List<Item> items;
    [SerializedDictionary] SerializedDictionary<string, Item> itemsDic = new();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        instance.InitDictionnary();
    }

    private void InitDictionnary()
    {
        items.ForEach(item => { itemsDic.Add(item.name, item); });
        Debug.Log("[Items] initialsés : " + itemsDic.Count);
    }

    public Item GetItem(string name)
    {
        if (!itemsDic.ContainsKey(name)) { return null; }

        return itemsDic[name];
    }
}
