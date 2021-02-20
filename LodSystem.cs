using UnityEngine;

public class LodSystem : MonoBehaviour
{
    public float MaxDistance;
    public GameObject Player;
    GameObject CurrentActive;
    public GameObject[] Objects;
    float step;

    private void Start()
    {
        step = MaxDistance / (Objects.Length - 1);
        CurrentActive = Instantiate(Objects[0], transform.position, Objects[0].transform.rotation);
        CurrentActive.transform.parent = this.transform;
    }
    private void FixedUpdate()
    {
        float DistToPlayer = Vector3.Distance(Player.transform.position, transform.position);
        int indexobj = (int)((float)DistToPlayer / (float)step);
        if (indexobj != 0){
            Destroy(CurrentActive);
            if (CurrentActive != Objects[indexobj]){
                CurrentActive = Instantiate(Objects[indexobj], transform.position, Objects[0].transform.rotation);
                CurrentActive.transform.parent = this.transform;
            }
        }
        else{
            Destroy(CurrentActive);
            CurrentActive = Instantiate(Objects[0], transform.position, Objects[0].transform.rotation);
            CurrentActive.transform.parent = this.transform;
        }
    }
}
