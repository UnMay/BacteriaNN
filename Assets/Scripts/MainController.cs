using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class MainController : MonoBehaviour
{
    public Vector2 area = new Vector2(10f, 10f);

    public GameObject bacteriumPrefab;
    public GameObject boidPrefab;
    public GameObject foodPrefab;
    public Slider slider;
    public Toggle startToggle;
    public TextMeshProUGUI textStart;
    //public Toggle q;
    //public GameObject bacteriumPrefab2;

    private int speedFood;
    private int frame = 0;

    void Start()
    {
        SliderChengeValue();
    }

    public void OnButtonStart()
    {
        if (startToggle.isOn)
        {
            textStart.text = "Стоп";
            Evolution();
        }
        else
        {
            textStart.text = "Старт";
            Stop();
        }
    }

    private void Evolution()
    {
        
        for (int i = 0; i < 100; i++)
        {
           /* if (q.isOn)
            {*/
            Genome genome = new Genome(64);
            GameObject b = Instantiate(bacteriumPrefab, new Vector3(Random.Range(-area.x, area.x), Random.Range(-area.y, area.y), 0), Quaternion.identity);
            b.name = "bacterium";
            b.GetComponent<AI>().Init(genome);
            /*}
            else
            {
                Genome genome = new Genome(16);
                GameObject b = Instantiate(bacteriumPrefab2, new Vector3(Random.Range(-area.x, area.x), Random.Range(-area.y, area.y), 0), Quaternion.identity);
                b.name = "bacterium";
                b.GetComponent<QLearning>().Init(genome);
            }*/
        }
        for (int i = 0; i < 1000; i++)
        {
            GameObject food = Instantiate(foodPrefab, new Vector3(Random.Range(-area.x, area.x), Random.Range(-area.y, area.y), 0), Quaternion.identity);
            food.name = "food";
        }
    }

    private void Stop()
    {
        foreach (GameObject f in GameObject.FindGameObjectsWithTag("food"))
            Destroy(f);
        foreach (GameObject b in GameObject.FindGameObjectsWithTag("bacterium"))
            Destroy(b);

    }

    void FixedUpdate()
    {
        if(frame % speedFood == 0 && startToggle.isOn)
        {
            GameObject food = Instantiate(foodPrefab, new Vector3(Random.Range(-area.x, area.x), Random.Range(-area.y, area.y), 0), Quaternion.identity);
            food.name = "food";
            GameObject food1 = Instantiate(foodPrefab, new Vector3(Random.Range(-area.x, area.x), Random.Range(-area.y, area.y), 0), Quaternion.identity);
            food1.name = "food";
            
        }
        frame++;

    }

    public void SliderChengeValue()
    {
        speedFood = (int)slider.value + 1;
    }
}
