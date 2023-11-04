using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UI_Shop : MonoBehaviour
{
    [SerializeField] public static GameObject self;
    public Shop shop;

    //affichage items
    [Header("affichage items")]
    public GameObject boutonObjetPrefab;
    public Transform conteneurObjets;

    //affichage composents
    [Header("affichage composents")]
    public GameObject itemComposentPrefab;
    public GameObject itemPrincipalPrefab;
    public Transform conteneurComposents;
    private Item itemPrincipalClick;

    //affichage description
    [Header("affichage description")]
    public Image iconDesc;
    public TextMeshProUGUI nomDesc;
    public TextMeshProUGUI statsDesc;
    public TextMeshProUGUI itemDesc;

    private List<GameObject> listButtons = new List<GameObject>();

    private Button selected;

    private void Start()
    {
        shop.initItems();
        createAllItems();
        self = gameObject;
    }

    private void Update()
    {
        if (!shop.player) { return; }

        foreach (GameObject b in listButtons)
        {
            foreach(Item i in shop.player.getItems())
            {
                if (i.unique)
                {
                    if (b.GetComponentInChildren<Text>(true).text == i.nom)
                    {
                        foreach (Image img in b.GetComponentsInChildren<Image>(true))
                        {
                            if (img.name == "sell")
                            {
                                img.gameObject.SetActive(true); ;
                            }
                        }

                        b.GetComponent<Button>().enabled = false;
                        b.GetComponentInChildren<TextMeshProUGUI>().enabled = false;
                        break;
                    }

                }

            }
        }
    }

    //reset un transform
    private void resetTransform(Transform t)
    {
        foreach (Transform child in t )
        {
            Destroy(child.gameObject);
        }
    }


    //creer un bouton
    private GameObject CreateButton(Item objet, Transform transformInstance, GameObject prefab)
    {
        var bouton = Instantiate(prefab, transformInstance);

        int prix = 0;
        if (shop.player && !shop.player.getItems().Contains(objet))
        {
            List<Item> s = new List<Item>(shop.player.getItems());
            foreach (Item i in objet.composents)
            {
                if (s.Contains(i))
                {
                    prix += i.getPrix();
                    s.Remove(i);
                }
                else
                {
                    foreach (Item i2 in i.composents)
                    {
                        if (s.Contains(i2))
                        {
                            prix += i2.getPrix();
                            s.Remove(i2);
                        }
                    }
                }
            }
        }
        bouton.transform.Find("prix").GetComponent<TextMeshProUGUI>().text = (objet.getPrix() - prix).ToString();
        bouton.GetComponentInChildren<Text>(true).text = objet.nom;

        foreach (Image img in bouton.GetComponentsInChildren<Image>())
        {
            if (img.name != "cadenas" && img.name != "contour")
            {
                img.sprite = objet.icone;
            }

            if (img.name == "sell")
            {
                img.gameObject.SetActive(false);
            }
        }

        return bouton;
    }


    //afficher l'arbre des composents
    public void initComposents(Item objet)
    {
        resetTransform(conteneurComposents.transform);

        var itemPrincipale = Instantiate(itemComposentPrefab, conteneurComposents);
        var b = CreateButton(objet, itemPrincipale.transform, itemPrincipalPrefab);
        b.GetComponent<Button>().onClick.AddListener(() => selectButton(b, objet));
        b.GetComponent<Button>().onClick.AddListener(() => initDescription(objet));

        Shop.TrierParPrixDecroissant(objet.composents);
        foreach (Item composent in objet.composents)
        {
            var b2 = CreateButton(composent, itemPrincipale.transform, boutonObjetPrefab);
            b2.GetComponent<Button>().onClick.AddListener(() => selectButton(b2, composent));
            b2.GetComponent<Button>().onClick.AddListener(() => initDescription(composent));
        }

        foreach (Item composent in objet.composents)
        {
            var itemSecond = Instantiate(itemComposentPrefab, conteneurComposents);
            var b3 = CreateButton(composent, itemSecond.transform, itemPrincipalPrefab);
            b3.GetComponent<Button>().onClick.AddListener(() => selectButton(b3, composent));
            b3.GetComponent<Button>().onClick.AddListener(() => initDescription(composent));

            Shop.TrierParPrixDecroissant(composent.composents);
            foreach (Item composent2 in composent.composents)
            {
                var b4 = CreateButton(composent2, itemSecond.transform, boutonObjetPrefab);
                b4.GetComponent<Button>().onClick.AddListener(() => selectButton(b4, composent2));
                b4.GetComponent<Button>().onClick.AddListener(() => initDescription(composent2));
            }
        }

        initDescription(objet);
    }

    //Affichage description
    public void initDescription(Item obj)
    {
        iconDesc.sprite = obj.icone;
        iconDesc.color = Color.white;
        nomDesc.text = obj.nom + " - <color=#FFD700>" + obj.getPrix() + "</color> <color=#C0C0C0>(" + Mathf.RoundToInt(obj.getPrix() * 0.7f) + " • <size=10>70%</size>)</color>";

        itemDesc.text = "";
        int interactions = obj.actifs.Count + obj.passifs.Count;
        foreach (Actif pb in obj.actifs)
        {
            interactions--;
            if (interactions == 0)
            {
                itemDesc.text += pb.description();
            }
            else
            {
                itemDesc.text += pb.description() + "\n\n";
            }

        }

        foreach (ItemInteraction pb in obj.passifs)
        {
            interactions--;
            if (interactions == 0)
            {
                itemDesc.text += pb.description();
            }
            else
            {
                itemDesc.text += pb.description() + "\n\n";
            }

        }

        statsDesc.text = "";
        obj.stats.ForEach(x => statsDesc.text += x.ToString() + " ");
    }

    //Selectionner le bouton
    public void selectButton(GameObject go, Item item) 
    {
        selected = go.GetComponent<Button>();
        StartCoroutine(DoubleClickAchat(selected, item));
    }

    private IEnumerator DoubleClickAchat(Button b, Item item)
    {
        UnityAction achat = () => AcheterObjet(item);
    
        b.onClick.AddListener(achat);
        yield return new WaitForSeconds(0.5f);
        b.onClick.RemoveListener(achat);
    }

    //acheter item
    public void AcheterObjet(Item objet)
    {
        if (objet.unique)
        {
            resetTransform(conteneurComposents);
        }
        else
        {
            initComposents(itemPrincipalClick);
        }

        shop.AcheterObjet(objet);
    }

    //Tries
    private void initConteneurObjets(List<Item> items)
    {
        resetTransform(conteneurObjets.transform);
        Shop.TrierParPrixCroissant(items);
        listButtons.Clear();

        foreach (Item item in items)
        {
            var btn = CreateButton(item, conteneurObjets, boutonObjetPrefab);
            listButtons.Add(btn);
            btn.GetComponent<Button>().onClick.AddListener(() => initComposents(item));
            btn.GetComponent<Button>().onClick.AddListener(() => selectButton(btn, item));
            btn.GetComponent<Button>().onClick.AddListener(() => itemPrincipalClick = item);

        }
    }

    //All items
    public void createAllItems()
    {
        Shop.TrierParPrixCroissant(shop.items);
        initConteneurObjets(shop.items);
    }

    //Mages items
    public void createMagesItems()
    {
        Shop.TrierParPrixCroissant(shop.itemsMages);
        initConteneurObjets(shop.itemsMages);
    }    
    
    //Bruiser items
    public void createBruisersItems()
    {
        Shop.TrierParPrixCroissant(shop.itemsBruisers);
        initConteneurObjets(shop.itemsBruisers);
    }    
    
    //Tireurs items
    public void createTireursItems()
    {
        Shop.TrierParPrixCroissant(shop.itemsTireurs);
        initConteneurObjets(shop.itemsTireurs);
    }   
    
    //Assassins items
    public void createAssassinsItems()
    {
        Shop.TrierParPrixCroissant(shop.itemsAssassins);
        initConteneurObjets(shop.itemsAssassins); 
    }    
    
    //Tanks items
    public void createTanksItems()
    {
        Shop.TrierParPrixCroissant(shop.itemsTanks);
        initConteneurObjets(shop.itemsTanks);
    }    
    
    //Supports items
    public void createSupportsItems()
    { 
        Shop.TrierParPrixCroissant(shop.itemsSupports);
        initConteneurObjets(shop.itemsSupports);
    }

    //ACTIVER DESACTIVER LE SHOP
    public static void showShop(bool etat) { self.SetActive(etat); }
    public static void inverseShopEtat() { self.SetActive(!self.activeSelf); }
}
