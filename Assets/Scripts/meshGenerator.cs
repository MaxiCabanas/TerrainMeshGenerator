using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class meshGenerator : MonoBehaviour
{
    [Range(3,300)]
    public int xMeshSize = 50;
    [Range(3,300)]
    public int zMeshSize = 50;
    public Gradient gradient;
    private float minHeight;
    private float maxHeight;
    [Header("Noise")]
    public float perlinHeight = 3f;
    public float perlinScale = 50f;
    public float xOffset = 0;
    public float zOffset = 0;
    private float finalScale = 0.05f;
    private float lastScale = 0;
    private Mesh mesh;
    [HideInInspector]
    public float[] noise;
    [HideInInspector]
    public Vector3[] vertices;
    [HideInInspector]
    public int[] triangles;
    [HideInInspector]
    public Color[] vertexColors;
    public float seed;
    public bool liveUpdate = false;
    void Start()
    {
        mesh = new Mesh();//mesh nuevo y vacio
        GetComponent<MeshFilter>().mesh = mesh;//asigno el mesh al filter

        CreateMesh();
    }
    public void GenerateSeed()
    {
        seed = Random.Range(0,10000);
    }
    public void CreateMesh()
    {
        GenerateNoise();//Genera una matriz numerica con PerlinNoise, se usa para definir la altura de los vertices del terreno.
        CreateShape();//Genera una matriz de vectores y numeros, que representan los vertices y triangulos del mesh. Ademas genero una matriz de colores basado en el gradiente definido en inspector.
        UpdateMesh();//Creo el mesh con los datos generados.
    }    

    void GenerateNoise()
    {
        noise = new float[(xMeshSize+1)*(zMeshSize+1)];


        if(lastScale != perlinScale)
        {
            // esto lo hago para no tener que manejar numeros tan chiquitos en el inspector.
            finalScale = perlinScale*0.001f;
            lastScale = perlinScale;
        }
            
        for(int i=0,z=0;z<=zMeshSize;z++)
        {
            for(int x=0;x<=xMeshSize;x++)
            {
                //Obtengo un valor para el vertice.
                noise[i] = Mathf.PerlinNoise(x*finalScale+xOffset+seed,z*finalScale+zOffset+seed)*perlinHeight;
                
                //para darle el aspecto low poly o escalonado al terreno, hago que los vertices solo tengan valores pares.
                if(Mathf.Floor(noise[i])%2 == 0)
                    noise[i] = Mathf.Floor(noise[i]);
                else
                    noise[i] = Mathf.Ceil(noise[i]);
                
                i++;
            }
        }
    }

    void CreateShape()
    {
        //Generacion de vertices del mesh
        vertices = new Vector3[(xMeshSize+1)*(zMeshSize+1)];
        minHeight = noise[0];
        maxHeight = noise[0];

        for(int i=0,z=0;z<=zMeshSize;z++)
        {
            for(int x=0;x<=xMeshSize;x++)
            {
                vertices[i] = new Vector3(x,noise[i],z);

                //Guardo el minimo y maximo de altura para poder obtener el color desde el gradiente mas adelante.
                if(noise[i] < minHeight)
                    minHeight = noise[i];
                if(noise[i] > maxHeight)
                    maxHeight = noise[i];

                i++;
            }
        }

        // Generacion de triangulos del mesh:
        //En unity, los triangulos se generan en sentido horario, para que los normal del mesh queden correctamente orientados.
        triangles = new int[xMeshSize*zMeshSize*6];
        int vert = 0;
        int tris = 0;

        for(int z= 0;z<zMeshSize;z++)
        {
            for(int x=0;x<xMeshSize;x++)
            {
                // Un triangulo no es mas que 3 numeros enteros, que representan los indices de los vertices de la matriz (mesh), ordenados linealmente.
                // Por ejemplo, si a la matriz:
                // (0,0)----(0,1)
                //   |        |
                //   |        |
                // (1,0)----(1,1)
                // le asignamos un indice a cada componente, en lugar de una coordenada, podemos representarla como:
                // 2-----3
                // |     |
                // |     |
                // 0-----1
                // Entonces podemos decir que los vectores (0,2,1) y (1,2,3) son los dos triangulos de la matriz anterior:
                // 2----3
                // | \  |
                // |  \ |
                // 0----1
                // Esta seccion del codigo me genera dos triangulos. Como son almacenados en un arreglo unidimensional, usando la variable "vert" puedo ir generando estos dos triangulos en toda la matris de vertices.
                
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + xMeshSize + 1;
                triangles[tris + 2] = vert + 1;

                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xMeshSize + 1;
                triangles[tris + 5] = vert + xMeshSize + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }

        // Colores para cada vertice
        vertexColors = new Color[vertices.Length];

        for(int i=0,z=0;z<=zMeshSize;z++)
        {
            for(int x=0;x<=xMeshSize;x++)
            {
                //Como ya conozco la altura mas baja y la mas alta de todo el terreno, puedo normalizar cada otro valor entre estos dos (float entre 0 y 1) y asi recorrer el gradiente.
                float height = Mathf.InverseLerp(minHeight,maxHeight,noise[i]);
                vertexColors[i] = gradient.Evaluate(height);
                i++;
            }
        }
    }

    void UpdateMesh()
    {
        mesh.Clear();//limpio
        mesh.vertices = vertices;//asigno vertices
        mesh.triangles = triangles;//creo los triangulos
        mesh.colors = vertexColors;//asigno colores

        //Esto es recomendable hacerlo siempre que se genere un mesh procedural.
        //A grandes rasgos, se intenta reordenar la data del mesh en memoria para tener un acceso mas optimo.
        mesh.Optimize();

        //Unity recalcula automaticamente el vector de cada normal para no tener bugs con la luz <3
        mesh.RecalculateNormals();
    }

    void OnValidate()
    {
        //Si se prende el bool "liveUpdate" cada vez que se modifique un valor en el inspector (excepto el seed) el mesh se va a actualizar inmediatamente
        //(no apto para compus potato)
        if(liveUpdate)
            CreateMesh();
    }
}
