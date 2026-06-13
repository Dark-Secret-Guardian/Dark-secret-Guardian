using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int prosperity;   // 繁荣度 0-100
    [SerializeField] private int ecology;      // 生态值 0-100
    [SerializeField] private int awakening;    // 觉醒值 0-100
    [SerializeField] private int guardianship; // 守护值 0-100

    void Start()
    {
        prosperity = 50;
        ecology = 75;
        awakening = 0;
        guardianship = 0;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            UpdateValues(10, -5, 0, 0);
        }
    }

    public void UpdateValues(int deltaProsperity, int deltaEcology, int deltaAwakening, int deltaGuardianship)
    {
        prosperity = Mathf.Clamp(prosperity + deltaProsperity, 0, 100);
        ecology = Mathf.Clamp(ecology + deltaEcology, 0, 100);
        awakening = Mathf.Clamp(awakening + deltaAwakening, 0, 100);
        guardianship = Mathf.Clamp(guardianship + deltaGuardianship, 0, 100);
    }
}
