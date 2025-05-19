using UnityEngine;

public class EngineSoundController : MonoBehaviour
{
    [Header("Motor Sesleri")]
    [SerializeField] private AudioSource engineStartSound;
    [SerializeField] private AudioSource engineStopSound;
    [SerializeField] private AudioSource engineLoopSound;

    [Header("Motor Ayarlarý")]
    [Range(0f, 10000f)] public float engineRPM = 0f;
    public float idleRPM = 1000f;
    public float maxRPM = 8000f;

    private MotorcycleVehicle motorcycle;
    private bool isEngineRunning = false;
    private bool wasEngineRunning = false;

    private void Awake()
    {
        motorcycle = GetComponent<MotorcycleVehicle>();
        if (motorcycle == null)
        {
            motorcycle = GetComponentInParent<MotorcycleVehicle>();
        }

        // Baþlangýçta tüm sesleri kapat
        if (engineStartSound != null) engineStartSound.Stop();
        if (engineStopSound != null) engineStopSound.Stop();
        if (engineLoopSound != null)
        {
            engineLoopSound.Stop();
            engineLoopSound.loop = true;
        }
    }

    private void Update()
    {
        if (motorcycle != null)
        {
            engineRPM = motorcycle.EngineRPM;
            isEngineRunning = motorcycle.IsEngineRunning;
        }

        HandleEngineState();
        UpdateEngineLoop();
    }

    private void HandleEngineState()
    {
        // Motor durum deðiþikliklerini kontrol et
        if (isEngineRunning && !wasEngineRunning)
        {
            // Motor çalýþmaya baþladý
            PlayEngineStart();
        }
        else if (!isEngineRunning && wasEngineRunning)
        {
            // Motor durdu
            PlayEngineStop();
        }

        wasEngineRunning = isEngineRunning;
    }

    private void PlayEngineStart()
    {
        if (engineStartSound != null && !engineStartSound.isPlaying)
        {
            engineStartSound.Play();
        }

        // Baþlangýç sesi bittikten sonra loop sesini baþlat
        if (engineLoopSound != null)
        {
            engineLoopSound.Play();
            engineLoopSound.volume = 1f;
        }
    }

    private void PlayEngineStop()
    {
        // Loop sesini durdur
        if (engineLoopSound != null)
        {
            engineLoopSound.Stop();
        }

        // Durma sesini çal
        if (engineStopSound != null && !engineStopSound.isPlaying)
        {
            engineStopSound.Play();
        }
    }

    private void UpdateEngineLoop()
    {
        if (!isEngineRunning || engineLoopSound == null) return;

        // RPM oranýný 0-1 arasý normalize et
        float rpmPercent = Mathf.InverseLerp(idleRPM, maxRPM, engineRPM);

        // Sesin perdesini ve þiddetini ayarla
        engineLoopSound.pitch = Mathf.Lerp(0.8f, 1.5f, rpmPercent);
        engineLoopSound.volume = Mathf.Lerp(0.7f, 1f, rpmPercent);
    }
}