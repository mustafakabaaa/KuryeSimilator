using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;

public class DeliveryPoint : MonoBehaviour
{
    public string orderID; // Bu teslimat noktasýnýn baðlý olduðu sipariþin ID'si
    public Light deliveryLight; // Teslimat noktasýndaki ýþýk
    public float blinkInterval = 0.5f; // Iþýðýn yanýp sönme aralýðý
    public TextMeshProUGUI deliveryText; // Teslimat noktasýndaki metin (TextMeshPro)
    public float rotationSpeed = 50f; // Metnin dönme hýzý

    private bool isBlinking = false;
    private bool isPlayerInRange = false;
    private Coroutine blinkCoroutine;
    private Coroutine rotateCoroutine;

    private void Start()
    {
        // Iþýk ve metni baþlangýçta gizle
        if (deliveryLight != null)
        {
            deliveryLight.enabled = false;
        }
        else
        {
            Debug.LogError("Delivery Light is not assigned!");
        }

        if (deliveryText != null)
        {
            deliveryText.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("Delivery Text is not assigned!");
        }

        // Teslimat noktasýný OrderManager'a kaydet
        if (OrderManager.Instance != null)
        {
            OrderManager.Instance.RegisterDeliveryPoint(orderID, this);
        }
        else
        {
            Debug.LogError("OrderManager.Instance is null!");
        }
    }

    private void Update()
    {
        // "E" tuþuna basýldýðýnda ve oyuncu alanýn içindeyse teslimat yap
        if (isPlayerInRange && Keyboard.current.eKey.wasPressedThisFrame)
        {
            DeliverOrder();
        }
    }

    public void StartBlinking()
    {
        isBlinking = true;
        if (blinkCoroutine == null)
        {
            blinkCoroutine = StartCoroutine(BlinkLight());
        }
    }

    public void StopBlinking()
    {
        isBlinking = false;
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }

        if (deliveryLight != null)
        {
            deliveryLight.enabled = false; // Iþýðý kapat
        }

        if (deliveryText != null)
        {
            deliveryText.gameObject.SetActive(false); // Metni gizle
        }
    }

    private IEnumerator BlinkLight()
    {
        while (isBlinking)
        {
            if (deliveryLight != null)
            {
                deliveryLight.enabled = !deliveryLight.enabled; // Iþýðý yanýp söndür
            }
            yield return new WaitForSeconds(blinkInterval);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Eðer oyuncu trigger'a girdiyse
        {
            isPlayerInRange = true;

            // Iþýðý sürekli yansýn (yanýp sönme durdurulsun)
            if (deliveryLight != null)
            {
                deliveryLight.enabled = true;
            }

            // Metni göster ve döndürme Coroutine'ini baþlat
            if (deliveryText != null)
            {
                deliveryText.gameObject.SetActive(true);
                rotateCoroutine = StartCoroutine(RotateText());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) // Eðer oyuncu trigger'dan çýktýysa
        {
            isPlayerInRange = false;

            // Metni gizle ve döndürme Coroutine'ini durdur
            if (deliveryText != null)
            {
                deliveryText.gameObject.SetActive(false);
                if (rotateCoroutine != null)
                {
                    StopCoroutine(rotateCoroutine);
                    rotateCoroutine = null;
                }
            }

            // Iþýðý tekrar yanýp sönsün
            if (isBlinking && deliveryLight != null)
            {
                deliveryLight.enabled = true; // Iþýðý aç
            }
        }
    }

    private IEnumerator RotateText()
    {
        while (true)
        {
            deliveryText.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            yield return null;
        }
    }

    private void DeliverOrder()
    {
        if (OrderManager.Instance != null)
        {
            // Sipariþi tamamla
            OrderManager.Instance.CompleteOrder(orderID);
        }
        else
        {
            Debug.LogError("OrderManager.Instance is null!");
        }
    }
}