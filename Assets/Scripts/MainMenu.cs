using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {

    [SerializeField]
    private GameObject scrollView, scrollbar, purchaseUI, fadeOut;

    [SerializeField]
    private GameObject[] Contents, Stages, Vehicles;

    private GameObject[] content;

    [SerializeField]
    private Text moneyText, cantBuyText;

    [SerializeField]
    private AudioSource audio;

    private float scroll_pos = 0, distance;
    private float[] pos;

    private int selectedMenuIndex, selectedIndex;
    private bool changeIndex = true, start = true;

    private void Start() {
        //Initialize if no data is found
        if(!PlayerPrefs.HasKey("Stage")) {
            PlayerPrefs.SetInt("Stage", 0);
            PlayerPrefs.SetInt("Vehicle", 0);
            PlayerPrefs.SetInt("Stage_Mars", 0);
            PlayerPrefs.SetInt("Stage_Cave", 0);
            PlayerPrefs.SetInt("Vehicle_Motorcycle", 0);
            PlayerPrefs.SetInt("Money", 5000);
        }
        LoadData();
        MenuChange(1);  //Initially, scroll view as vehicle content
        start = false;
    }

    //Initialize, Scroll View Ads Beside Content
    private void LoadData() {
        Stages[1].transform.GetChild(1).gameObject.SetActive(PlayerPrefs.GetInt("Stage_Mars").Equals(0));
        Stages[1].GetComponent<Button>().enabled = PlayerPrefs.GetInt("Stage_Mars").Equals(0);
        Stages[2].transform.GetChild(1).gameObject.SetActive(true);
        Stages[3].transform.GetChild(1).gameObject.SetActive(true);
        Vehicles[1].transform.GetChild(1).gameObject.SetActive(PlayerPrefs.GetInt("Vehicle_Motorcycle").Equals(0));
        Vehicles[1].GetComponent<Button>().enabled = PlayerPrefs.GetInt("Vehicle_Motorcycle").Equals(0);
        Vehicles[2].transform.GetChild(1).gameObject.SetActive(true);
        Vehicles[3].transform.GetChild(1).gameObject.SetActive(true);

        moneyText.text = PlayerPrefs.GetInt("Money").ToString();
    }

    private void Update() {
        //Using scroll views
        if(Input.GetMouseButton(0)) {
            scroll_pos = scrollbar.GetComponent<Scrollbar>().value;
        }
        else {
            for(int i = 0; i < pos.Length; i++) {
                if(scroll_pos < pos[i] + (distance / 2) && scroll_pos > pos[i] - (distance / 2)) {
                    scrollbar.GetComponent<Scrollbar>().value = Mathf.Lerp(scrollbar.GetComponent<Scrollbar>().value, pos[i], 0.1f);
                    selectedIndex = i;
                }
            }
            changeIndex = true;
        }

        //The selected content will be larger in size, and the rest of the content will be smaller in size.
        for(int i = 0; i < pos.Length; i++) {
            if(scroll_pos < pos[i] + (distance / 2) && scroll_pos > pos[i] - (distance / 2)) {
                content[i].transform.localScale = Vector2.Lerp(content[i].transform.localScale, new Vector2(1.2f, 1.2f), 0.1f);
                for(int j = 0; j < pos.Length; j++)
                    if(j != i)
                        content[j].transform.localScale = Vector2.Lerp(content[j].transform.localScale, new Vector2(0.8f, 0.8f), 0.1f);

                if(changeIndex) {  //Save selected content to data
                    SaveSelectedData(i);
                    changeIndex = false;
                }
            }
        }
    }

    //Pressing the Stage/Vehicle button causes the type of content in the scroll view to change..
    public void MenuChange(int index) {
        //If you try to change the content type with unlocked content selected, you will be asked if you want to buy it..
        if(!CheckPurchased() && !start) {
            if(!(selectedMenuIndex == 0 && selectedIndex > 1 || selectedMenuIndex == 1 && selectedIndex > 1)) {
                purchaseUI.SetActive(true);
                return;
            }
        }    
        selectedMenuIndex = index;  //Save the selected content type to a variable

        pos = new float[Contents[index].transform.childCount];
        distance = 1f / (pos.Length - 1f);
        for(int i = 0; i < pos.Length; i++)
            pos[i] = distance * i;

        if(index.Equals(0)) { ///Change the content type to the Stage
            content = Stages;
            scroll_pos = scrollbar.GetComponent<Scrollbar>().value = pos[PlayerPrefs.GetInt("Stage")];
        }
        else if(index.Equals(1)) {  //Change the content type to Vehicle
            content = Vehicles;
            scroll_pos = scrollbar.GetComponent<Scrollbar>().value = pos[PlayerPrefs.GetInt("Vehicle")];
        }

        foreach(var obj in Contents)
            obj.SetActive(false);
        Contents[index].SetActive(true);
        scrollView.GetComponent<ScrollRect>().content = Contents[index].GetComponent<RectTransform>();
    }

    //Buy something that is not unlocked and change your data.
    public void Purchase() {
        int price, moneyOwned = PlayerPrefs.GetInt("Money");
        if(selectedMenuIndex.Equals(0)) {  //Stage
            price = int.Parse(Stages[selectedIndex].transform.GetChild(1).gameObject.transform.GetChild(1).GetComponent<Text>().text);
            if(moneyOwned - price < 0) { cantBuyText.GetComponent<Animator>().SetTrigger("warning"); return; }
            if(selectedIndex.Equals(1)) PlayerPrefs.SetInt("Stage_Mars", 1);
            
        }
        else {  //vehicle
            price = int.Parse(Vehicles[selectedIndex].transform.GetChild(1).gameObject.transform.GetChild(1).GetComponent<Text>().text);
            if(moneyOwned - price < 0) { cantBuyText.GetComponent<Animator>().SetTrigger("warning"); return; }
            PlayerPrefs.SetInt("Vehicle_Motorcycle", 1);
        }
        PlayerPrefs.SetInt("Money", moneyOwned - price);
        audio.Play();
        LoadData();
    }

    //Checks whether the selected content has been unlocked (purchased)
    private bool CheckPurchased() {
        if(selectedMenuIndex.Equals(0)) {
            if(selectedIndex != 0)
                return !Stages[selectedIndex].transform.GetChild(1).gameObject.activeSelf;
        }
        else { 
            if(selectedIndex != 0) 
                return !Vehicles[selectedIndex].transform.GetChild(1).gameObject.activeSelf;
        }
        return true;
    }

    //Save the selected content index to the data
    private void SaveSelectedData(int index) {
        if(selectedMenuIndex.Equals(0)) {
            if(!CheckPurchased()) return; 
            PlayerPrefs.SetInt("Stage", index);
        }
        else {
            if(!CheckPurchased()) return; 
            PlayerPrefs.SetInt("Vehicle", index);
        }
    }

    //Press the Start Game button to start the game
    public void GameStart() {
        if(!CheckPurchased()) {
            if(!(selectedMenuIndex == 0 && selectedIndex > 1 || selectedMenuIndex == 1 && selectedIndex > 1)) {
                purchaseUI.SetActive(true);
                return;
            }
        }
        fadeOut.GetComponent<Animator>().SetTrigger("FadeOut");
    }
}