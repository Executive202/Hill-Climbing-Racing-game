using UnityEngine;

public class CameraController : MonoBehaviour {
    public Transform vehiclePos;
    private Vector3 offset;

    //Vehicle position and camera settings at the start of the game
    public void SetUp() {
        vehiclePos.position = new Vector3(Camera.main.ViewportToWorldPoint(new Vector3(0, 0)).x + 3f, vehiclePos.position.y, 0);
        CarController carController = vehiclePos.gameObject.GetComponent<CarController>();
        carController.StartPos = vehiclePos.position;
        offset = transform.position - vehiclePos.position;
    }

    private void Update() {
        //The camera follows the vehicle
        transform.position = vehiclePos.position + offset;
    }
}