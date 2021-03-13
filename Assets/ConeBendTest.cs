using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConeBendTest : MonoBehaviour
{
    public GameObject transformPrefab;

    public GameObject point;

    public GameObject P0;

    public GameObject P1;

    public GameObject P2;

    public GameObject P3;
     
    private List<(Vector3,Transform)> vs;

    private List<GameObject> gs;

    List<GameObject> helpers;

    private List<Vector3> vertices;
    // Start is called before the first frame update
    void Start()
    {
        Init();
    }
    void Init()
    {
        P0.transform.LookAt(P1.transform);
        P1.transform.LookAt(P2.transform);
        P2.transform.LookAt(P3.transform);

        List<GameObject> spheres = new List<GameObject>();

        spheres.AddRange(new List<GameObject> { P0, P1, P2, P3 });
        vertices = new List<Vector3>();
        gs = new List<GameObject>();
        vs = new List<(Vector3, Transform)>();

        vs.Add((P0.transform.position, P0.transform));
        for (int i = 0; i < spheres.Count - 1; i++)
        {
            spheres[i].transform.LookAt(spheres[i + 1].transform);
            Vector3 pos = spheres[i+1].transform.position - (spheres[i].transform.forward);
            Quaternion rot = spheres[i].transform.rotation;
            Vector3 pos2 = spheres[i+1].transform.position + (spheres[i + 1].transform.forward);
            Quaternion rot2 = spheres[i + 1].transform.rotation;
            Transform t = Instantiate(transformPrefab.transform, pos, rot);
            Transform t2 = Instantiate(transformPrefab.transform, pos2, rot2);
            vs.AddRange(GetBendedPoints(t, t2));
        }
        int p = 1;
        foreach ((Vector3,Transform) v in vs)
        {
            GameObject ig = Instantiate(point);
            ig.transform.position = v.Item1;
            ig.transform.rotation = v.Item2.transform.rotation;
            gs.Add(ig);
            vertices.AddRange(CreateCircleAroundPoint(ig.transform, 20,.3f));
            p++;
        }
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.vertices = vertices.ToArray();
        mesh.triangles = CreateTriangles(mesh.vertices);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        transform.position = new Vector3(0,0,0);
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Init();
        }
    }
    int[] CreateTriangles(Vector3[] vertices)
    {
        List<int> triangleList = new List<int>();
        for (int i = 0; i < vertices.Length - 20; ++i)
        {
            if (i % 20 == 19)
            {
                triangleList.Add(i);
                triangleList.Add(i - 19);
                triangleList.Add(i + 20);

                triangleList.Add(i - 19);
                triangleList.Add(i + 1);
                triangleList.Add(i + 20);
            }
            else
            {
                triangleList.Add(i);
                triangleList.Add(i + 1);
                triangleList.Add(i + 20);

                if (i < vertices.Length - 21)
                {
                    triangleList.Add(i + 1); //A
                    triangleList.Add(i + 21);//B
                    triangleList.Add(i + 20);//C
                }
            }

        }

        int[] triangle_array = new int[triangleList.Count];

        int j = 0;

        foreach (int triangle_point in triangleList)
        {
            triangle_array[j] = triangle_point;
            ++j;
        }
        return triangle_array;
    }
    List<(Vector3,Transform tr)> GetBendedPoints(Transform tOne, Transform tTwo)
    {
        List<(Vector3,Transform)> vList = new List<(Vector3, Transform)>();
        int interpolationFactor = 10;
        float tStep = .1f;
        Vector3 N0 = tOne.transform.forward;
        Vector3 N1 = tTwo.transform.forward;
        Vector3 A0 = tOne.transform.position;
        Vector3 A1 = tOne.transform.forward;
        Vector3 A2 = 3.0f * (tTwo.transform.position - tOne.transform.position) - N1 - 2.0f * N0;
        Vector3 A3 = N1 + N0 - 2.0f * (tTwo.transform.position - tOne.transform.position);
        
        for(int t = 0; t < interpolationFactor - 1; t++)
        {
            Transform trans = Instantiate(transformPrefab).transform;
            trans.rotation = Quaternion.Lerp(tOne.transform.rotation, tTwo.transform.rotation, tStep);
            Vector3 pPos = A0 + (A1 * tStep) + (A2 * tStep * tStep) + (A3 * tStep * tStep * tStep);
            vList.Add((pPos,trans));
            tStep += .1f;
        }
        return vList;
    }
    List<Vector3> CreateCircleAroundPoint(Transform transform, int nbPointsInCircle, float width)
    {
        List<Vector3> vertexPositions = new List<Vector3>();
        List<Quaternion> rotations = new List<Quaternion>();
        Vector3 center = transform.position;
        Vector3 up = transform.forward;
        Vector3 initPosition = Vector3.Cross(transform.right, up) * width;
        for (int i = 0; i < nbPointsInCircle; i++)
        {
            Quaternion q = Quaternion.AngleAxis(((float)360 / (float)nbPointsInCircle) * i, up);
            Vector3 pos = initPosition;
            pos = q * pos;
            pos += transform.position;
            vertexPositions.Add(pos);
            rotations.Add(transform.rotation);
            GameObject p = Instantiate(point);
            p.transform.position = pos;
            p.transform.rotation = transform.rotation;
        }
        return vertexPositions;
    }
}
