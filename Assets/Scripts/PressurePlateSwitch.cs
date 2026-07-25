using UnityEngine;

/// <summary>
/// Switch yang tetap "ditekan" selama ada objek Player ATAU FrozenClone di atasnya.
/// Karena clone bersifat statis (Kinematic), begitu clone berdiri di atasnya,
/// switch otomatis tertekan permanen -- ini yang membuat mekanik "gunakan clone sebagai pemberat" bekerja.
/// </summary>
public class PressurePlateSwitch : MonoBehaviour
{
    [SerializeField] private Door linkedDoor; // drag GameObject Door di sini
    private int objectsOnPlate = 0;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Terima trigger dari Player yang masih hidup maupun clone yang sudah beku
        if (other.CompareTag("Player") || other.CompareTag("Platform"))
        {
            objectsOnPlate++;
            UpdatePlateState();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Platform"))
        {
            objectsOnPlate--;
            UpdatePlateState();
        }
    }

    private void UpdatePlateState()
    {
        bool isPressed = objectsOnPlate > 0;
        if (linkedDoor != null)
            linkedDoor.SetOpen(isPressed);
    }
}