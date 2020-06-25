using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QLearning : MonoBehaviour
{
    public static int[] skillsTotal = new int[4];

    public GameObject bacteriumPrefab;

    public int foodSkill = 0;
    public int attackSkill = 0;
    public int defSkill = 0;
    public float energy = 10;
    public float age = 0;

    private int inputsCount = 4;
    private Genome genome;
    private NN nn;

    private Rigidbody2D rb;

    private Vector2 alternativVelosity;
    private Vector3 alternativePositioin;
    private Vector3[] vectors;

    public void Init(Genome g)
    {
        genome = g;
        Color col = new Color(0.1f, 0.1f, 0.25f, 0.8f);
        float size = 0.75f;
        for (int i = 0; i < Genome.skillCount; i++)
        {
            skillsTotal[g.skills[i]]++;
            if (g.skills[i] == 0)
            {
                foodSkill++;
                col.g += 0.2f;
            }
            else if (g.skills[i] == 1)
            {
                attackSkill++;
                col.r += 0.35f;
            }
            else if (g.skills[i] == 2)
            {
                defSkill++;
                col.b += 0.25f;
            }
            else if (g.skills[i] == 3)
            {
                size += 0.5f;
            }
        }
        transform.localScale = new Vector3(size, size, size);
        gameObject.GetComponent<SpriteRenderer>().color = col;
        nn = new NN(inputsCount, 4);
        for (int i = 0; i < inputsCount; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                nn.layers[0].weights[i, j] = genome.weights[i + j * inputsCount];
            }
        }
        newlayers = nn.layers;
    }

    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        transform.eulerAngles = new Vector3(0f, 0f, Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg - 90);
        age += Time.deltaTime;
    }
    float[] inputs = new float[4];
    
    Layer[] newlayers;
    private float[] DoDo(Vector3 position)
    {
        float vision = 5f + attackSkill;
        float[] inputs = new float[inputsCount];
        inputs = new float[4];
        vectors = new Vector3[4];
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, vision);

        // количество соседних объектов
        float[] neighboursCount = new float[4];

        // вектара к центрам масс еды, красного, зеленого и синего
        
        for (int i = 0; i < 4; i++)
        {
            neighboursCount[i] = 0;
            vectors[i] = new Vector3(0f, 0f, 0f);
        }
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject == gameObject) continue;
            if (colliders[i].gameObject.name == "food")
            {
                neighboursCount[0]++;
                vectors[0] += colliders[i].gameObject.transform.position - transform.position;
            }
            else if (colliders[i].gameObject.name == "bacterium")
            {
                AI ai = colliders[i].gameObject.GetComponent<AI>();
                neighboursCount[1] += ai.attackSkill / 3f;
                vectors[1] += (colliders[i].gameObject.transform.position - transform.position) * ai.attackSkill;
                neighboursCount[2] += ai.foodSkill / 3f;
                vectors[2] += (colliders[i].gameObject.transform.position - transform.position) * ai.foodSkill;
                neighboursCount[3] += ai.defSkill / 3f;
                vectors[3] += (colliders[i].gameObject.transform.position - transform.position) * ai.defSkill;
            }
            
        }
        for (int i = 0; i < 4; i++)
        {
            if (neighboursCount[i] > 0)
            {
                vectors[i] /= neighboursCount[i] * vision;
                inputs[i] = vectors[i].magnitude;
            }
            else
            {
                inputs[i] = 0f;
            }
        }
        return neighboursCount;
    }

    void FixedUpdate()
    {
        float[] neighboursCount = DoDo(alternativePositioin);
        float max = QFunction(inputs, neighboursCount);
        neighboursCount = DoDo(transform.position);
        if (max > QFunction(inputs, neighboursCount))
            nn.layers = newlayers;
        float[] outputs = nn.FeedForward(inputs);

        //
        newlayers = nn.layers;
        Layer[] tmplayers = nn.layers;
        for (int i = 0; i < inputsCount; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (UnityEngine.Random.value < 0.1) newlayers[0].weights[i, j]  += UnityEngine.Random.Range(-0.5f, -0.5f); 
            }
        }

        nn.layers = newlayers;

        Vector2 target = new Vector2(0, 0);
        for (int i = 0; i < 4; i++)
        {
            if (neighboursCount[i] > 0)
            {
                Vector2 dir = new Vector2(vectors[i].x, vectors[i].y);
                dir.Normalize();
                target += dir * outputs[i];
            }
        }
        if (target.magnitude > 1f) target.Normalize();
        Vector2 velocity = rb.velocity;
        velocity += target * (0.25f + attackSkill * 0.05f);
        velocity *= 0.98f;
        alternativVelosity = velocity;
        alternativePositioin = transform.position + new Vector3(alternativVelosity.x / 50, alternativVelosity.y / 50);
        nn.layers = tmplayers;


         target = new Vector2(0, 0);
        for (int i = 0; i < 4; i++)
        {
            if (neighboursCount[i] > 0)
            {
                Vector2 dir = new Vector2(vectors[i].x, vectors[i].y);
                dir.Normalize();
                target += dir * outputs[i];
            }
        }
        if (target.magnitude > 1f) target.Normalize();
        velocity = rb.velocity;
        velocity += target * (0.25f + attackSkill * 0.05f);
        velocity *= 0.98f;
        rb.velocity = velocity;
        energy -= Time.deltaTime;
        if (energy < 0f)
        {
            Kill();
        }
    }


    private float QFunction(float[] inputs, float[] neighboursCount)
    {
        float[] array = new float[] { foodSkill - inputs[0], attackSkill - inputs[1] - neighboursCount[1], attackSkill - inputs[2], attackSkill - inputs[3] - neighboursCount[3] };

        float max = float.MinValue;
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] > max)
            {
                // найден больший элемент
                max = array[i];
            }
        }
        return max;
        
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (foodSkill == 0) return;
        if (col.gameObject.name == "food")
        {
            Eat(foodSkill);
            Destroy(col.gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (age < 1f) return;
        if (attackSkill == 0) return;
        if (col.gameObject.name == "bacterium")
        {
            AI ai = col.gameObject.GetComponent<AI>();
            if (ai.age < 1f) return;
            float damage = Mathf.Max(0f, attackSkill - ai.defSkill);
            damage *= 4f;
            damage = Mathf.Min(damage, ai.energy);
            ai.energy -= damage * 1.25f;
            Eat(damage);
            if (ai.energy == 0f) ai.Kill();
        }
    }

    public void Kill()
    {
        for (int i = 0; i < Genome.skillCount; i++)
        {
            skillsTotal[genome.skills[i]]--;
        }
        Destroy(gameObject);
    }

    private void Eat(float food)
    {
        energy += food;
        if (energy > 16)
        {
            energy *= 0.5f;
            GameObject b = (GameObject)Object.Instantiate(Resources.Load("m1", typeof(GameObject)), new Vector3(0, 0, 0), Quaternion.identity);
            b.transform.position = transform.position;
            b.name = "bacterium";
            Genome g = new Genome(genome);
            g.Mutate(0.5f);
            CloneWeights(g);
            AI ai = b.GetComponent<AI>();
            ai.Init(g);
            ai.energy = energy;
        }
    }

    private void CloneWeights(Genome g)
    {
        for (int i = 0; i < inputsCount; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                g.weights[i + j * inputsCount] = nn.layers[0].weights[i, j];
            }
        }
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                g.weights[i + j * 8 + inputsCount * 8] = nn.layers[1].weights[i, j];
            }
        }
    }

}
