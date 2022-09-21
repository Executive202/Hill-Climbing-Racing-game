using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager> {

    [SerializeField]
    private Image fuelGauge, captureImg;
    private Texture2D textureImg;
    private Sprite spriteImg;

    [SerializeField]
    private GameObject fuelWarning, fadeIn, pauseUI, gameOverUI;

    [SerializeField]
    private Text moneyText, moneyEarnedText, distanceText, totaldistanceText, gameStateText;

    [SerializeField]
    private AudioSource[] audio;

    private int totalMoney, moneyEarned = 0;

    public ObjectManager objectManager;
    public CameraController cameraController;
    private CarController carController;

    public bool GasBtnPressed { get; set; }
    public bool BrakeBtnPressed { get; set; }
    public bool isDie { get; set; }
    public bool ReachGoal { get; set; }

    private void Start() {
        Time.timeScale = 1f;
        isDie = false;
        ReachGoal = false;
        fadeIn.GetComponent<Animator>().SetTrigger("FadeIn");  //페이드 인 애니메이션 실행
        Initialize();
    }

    private void Update() {
        //Press Back to pause the game
        if(Input.GetKeyDown(KeyCode.Escape))  
            GamePause();

        //Continue by calculating the distance you have moved text update
        if(!gameOverUI.activeSelf)
            distanceText.text = (int)(carController.transform.position.x - carController.StartPos.x) + "m / <color=yellow>1427m</color>";

        //After gameover/success, tap again to restart the game
        if(isDie && Input.GetMouseButtonDown(0) && gameOverUI.activeSelf) 
            LoadScene(0);

        //Reproduces sound when engine/brake button is pressed
        if(GasBtnPressed || BrakeBtnPressed)
            PlaySound("engine");
    }

    //Game Initial Settings Function
    private void Initialize() {
        string objName = "";
        int stageIndex = PlayerPrefs.GetInt("Stage"), vehicleIndex = PlayerPrefs.GetInt("Vehicle");

        //Import the selected map
        if(stageIndex.Equals(0)) {
            objName = "Country";
            Camera.main.backgroundColor = new Color(0.5803922f, 0.8470589f, 0.937255f, 0);
        }
        else if(stageIndex.Equals(1)) {
            objName = "Mars";
            Camera.main.backgroundColor = new Color(0.8627452f, 0.6666667f, 0.6666667f, 0);
        }
        else if(stageIndex.Equals(2))
            objName = "Cave";
        objectManager.GetObject(objName);

        //Import/Create Objects for the Selected Vehicle
        if(vehicleIndex.Equals(0)) objName = "HillClimber";
        else if(vehicleIndex.Equals(1)) objName = "Motorcycle";
        CarController vehicle = objectManager.GetObject(objName).GetComponent<CarController>();
        carController = vehicle;

        //Camera Adjustment
        cameraController.vehiclePos = vehicle.gameObject.transform;
        cameraController.SetUp();

        //Retrieve the data of the money you own and update the text
        totalMoney = PlayerPrefs.GetInt("Money");
        moneyText.text = totalMoney.ToString();
    }

    //Fuel Consumption Function
    public void FuelConsume() {
        fuelGauge.fillAmount = carController.Fuel;  //The more you move, the less the fuel gauge will be.
        if(fuelGauge.fillAmount <= 0.6f) {  //Fee gauge color adjustment
            fuelGauge.color = new Color(1, fuelGauge.fillAmount * 0.8f * 2f, 0, 1);  //As the gauge decreases, the gradient effect
            
            if(fuelGauge.fillAmount <= 0.3f) {  //Fuel Low Fuel Warning Animation
                if(!isDie) fuelWarning.SetActive(true);
                if(fuelGauge.fillAmount == 0f)  //Running out of fuel and over-playing the game
                    StartGameOver();
            }
        }
        else {
            fuelGauge.color = new Color((1f - fuelGauge.fillAmount) * 2f, 1, 0, 1);  
            fuelWarning.SetActive(false);
        }
    }

    //Acquiring fuel fills the fuel gauge.
    public void FuelCharge() {
        carController.Fuel = 1;
        fuelGauge.fillAmount = 1; //Gauge bar fills
        PlaySound("refuel"); //play fueling sounds
    }

    //Function when you get coins
    public void GetCoin(int price) {
        totalMoney += price;
        moneyEarned += price;
        moneyText.text = totalMoney.ToString(); //Renewal of the amount in the text
        moneyText.GetComponent<Animator>().SetTrigger("EarnMoney");  //animation 
        PlaySound("coin"); //Play Coin Sound
    }

    //Engine button functions
    public void GasBtn(bool press) {
        GasBtnPressed = press;
    }

    //Brake button function
    public void BrakeBtn(bool press) {
        BrakeBtnPressed = press;
    }

    //Sound reproduction by implication
    public void PlaySound(string audioName) {
        switch(audioName) {
            case "cameraShutter" :
                audio[0].Play();
                break;
            case "coin":
                audio[1].Play();
                break;
            case "crack":
                audio[2].Play();
                break;
            case "refuel":
                audio[3].Play();
                break;
            case "engine":
                audio[4].Play();
                break;
        }
    }

    //Pause the game function
    public void GamePause() {
        pauseUI.SetActive(!pauseUI.activeSelf); //Enable/disable the pause UI
        
        if(pauseUI.activeSelf) Time.timeScale = 0f;
        else Time.timeScale = 1f;
    }

    //Gameover function
    public void StartGameOver() {
        if(!isDie) {
            StartCoroutine(GameOver());
            isDie = true;
        }
    }

    private IEnumerator GameOver() {
        if(!ReachGoal) yield return new WaitForSeconds(4f);

        carController.moveStop = true;
        fuelWarning.SetActive(false);

        //Screenshot of the vehicle when over-game and show it as a UI image
        yield return new WaitForEndOfFrame();
        Texture2D text = new Texture2D(Screen.width / 5, Screen.height / 3, TextureFormat.RGB24, false);
        textureImg = new Texture2D(Screen.width / 5, Screen.height / 3);
        text.ReadPixels(new Rect(-Screen.width / 2, Screen.height / 3 + 15f, Screen.width, Screen.height), 0, 0);
        text.Apply();
        textureImg = text;
        spriteImg = Sprite.Create(textureImg, new Rect(0, 0, textureImg.width, textureImg.height), new Vector2(0, 0));
        captureImg.sprite = spriteImg;

        //Change and activate the text values of the GameOver UI
        if(!ReachGoal) gameStateText.text = "<color=#FF4C4C>Game Over</color>";
        else gameStateText.text = "<color=#FFFF4C>Game Complete!!</color>";
        moneyEarnedText.text = "+" + moneyEarned.ToString() + " COINS";  //Shows the number of coins earned during the game
        totaldistanceText.text = " Distance : " + (int)(carController.transform.position.x - carController.StartPos.x) + "m";
        gameOverUI.SetActive(true);
        
        PlaySound("cameraShutter"); //Camera shutter sound playback
    }

    public void LoadScene(int sceneIndex) {
        PlayerPrefs.SetInt("Money", totalMoney);  //Save the earned coin data
        SceneManager.LoadScene(sceneIndex); 
    }
}