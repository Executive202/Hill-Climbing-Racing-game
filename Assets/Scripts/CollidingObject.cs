using UnityEngine;

public class CollidingObject : MonoBehaviour {

    [SerializeField]
    private int price;

    private void OnTriggerEnter2D(Collider2D collision) {
        if(collision.gameObject.CompareTag("Vehicle")) {
            //When acquiring fuel
            if(gameObject.name.Contains("Fuel")) {  
                GameManager.Instance.FuelCharge();
                gameObject.SetActive(false);
            }
            
            //Reach your target destination and succeed in the game
            else if(gameObject.name.Contains("Goal")) {  
                GameManager.Instance.ReachGoal = true;
                GameManager.Instance.StartGameOver();
            }

            //Earn Coins
            else if(gameObject.name.Contains("Coin")) {  
                GameManager.Instance.GetCoin(price);
                gameObject.SetActive(false);
            }
        }
    }
}