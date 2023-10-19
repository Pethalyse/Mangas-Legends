using UnityEngine;

public class CameraControleurFollow : MonoBehaviour
{
    public Transform target; // Le transform du champion que vous souhaitez suivre.

    private Vector3 offset; // Distance entre la caméra et le champion au début.

    private bool isLock = true;

    private int screenWidth;
    private int screenHeight;

    public float moveSpeed = 10f;
    public float borderThickness = 10.0f;

    public float scrollSpeed = 5.0f;

    public bool getIsLock() { return isLock; }

    private void Update()
    {
        if (!target)
        {
            if (!GameManager.GetLocalPlayer()) { return; }
            target = GameManager.GetLocalPlayer().transform;
            if (!target) { return; }
            offset = new Vector3(0, Mathf.Abs(transform.position.y - target.position.y), 0);
        }
        else
        {
            if (Input.GetButtonDown("DelockCam")) // Change le verrouillage de la caméra lorsque la touche "Y" est enfoncée.
            {
                isLock = !isLock;
            }

            screenWidth = Screen.width;
            screenHeight = Screen.height;
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return; // Si la cible est nulle, ne pas suivre.
        }

        if (!isLock)
        {

            if (Input.mousePosition.x < borderThickness)
            {
                transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);
            }
            else if (Input.mousePosition.x > screenWidth - borderThickness)
            {
                transform.Translate(Vector3.right * moveSpeed * Time.deltaTime);
            }

            if (Input.mousePosition.y < borderThickness)
            {
                transform.Translate(Vector3.down * moveSpeed * Time.deltaTime);
            }
            else if (Input.mousePosition.y > screenHeight - borderThickness)
            {
                transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);
            }
        }
        else
        {

            Vector3 rotatedOffset = Quaternion.Euler(45, 0, 0) * offset;
            transform.position = target.position + rotatedOffset;
            transform.LookAt(target);
        }

        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        Vector3 scrollMovement = new Vector3(0, scrollInput * scrollSpeed * Time.deltaTime, 0);
        transform.Translate(scrollMovement);

    }
}
