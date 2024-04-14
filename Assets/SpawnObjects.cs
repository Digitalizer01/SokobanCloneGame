using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnObjects : MonoBehaviour
{
    public GameObject BoxObject;
    public GameObject CharacterObject;
    public GameObject TargetObject;
    public GameObject WallObject;
    [Serializable]
    public struct MatrixObjects
    {
        public GameObject Object;
        public int Value;
    }
    public MatrixObjects[] Objects;
    private int[,] _matrix;

    public void FillMatrix()
    {
        var currentScene = SceneManager.GetActiveScene();
        var currentSceneName = currentScene.name;
        string filePath = "C:/SokobanLevels/" + currentSceneName + ".csv";
        int numRows = File.ReadAllLines(filePath).Length;
        string[] firstRow = File.ReadLines(filePath).First().Split(',');
        int numCols = firstRow.Length;

        // Inicializar la matriz con el tamaño obtenido
        _matrix = new int[numRows, numCols];

        StreamReader inp_stm = new StreamReader(filePath);
        int row = 0;
        while(!inp_stm.EndOfStream)
        {
            string[] inp_ln = inp_stm.ReadLine().Split(',');
            for(int i = 0; i < inp_ln.GetLength(0); i++)
            {
                _matrix[row, i] = int.Parse(inp_ln[i]);
            }
            row++;
        }
        inp_stm.Close();
    }

    public void SpawnObjectsMatrix()
    {
        // Obtiene el ancho del sprite del prefab.
        float spriteWidth = getObjectForValue(1).GetComponent<SpriteRenderer>().bounds.size.x / 2;
        // Obtiene la altura del sprite del prefab.
        float spriteHeight = getObjectForValue(1).GetComponent<SpriteRenderer>().bounds.size.y / 2;

        // Calcula el punto inicial donde se instanciarán los objetos.
        Vector3 startPosition = this.gameObject.transform.position;

        // Itera sobre la cantidad de objetos y los instancia uno al lado del otro.
        for (int i = 0; i < _matrix.GetLength(0); i++) // Itera por las filas
        {
            for (int j = 0; j < _matrix.GetLength(1); j++) // Luego por las columnas
            {
                // Calcula la posición del objeto actual en la iteración.
                Vector3 position = startPosition + new Vector3(spriteWidth * j, -spriteHeight * i, 0f);
                GameObject newObject;
                if(_matrix[i, j] != -1)
                {
                    // Instancia el objeto y le asigna la posición.
                    newObject = Instantiate(getObjectForValue(_matrix[i, j]), position, Quaternion.identity);
                    newObject.tag = getObjectForValue(_matrix[i, j]).tag;

                    // Modifica la escala del objeto para hacerlo de la mitad del tamaño original
                    newObject.transform.localScale = new Vector3(0.5f, 0.5f, 1f);

                    StringBuilder builder = new StringBuilder();
                    builder.Append(i);
                    builder.Append("_");
                    builder.Append(j);
                    newObject.name = builder.ToString();
                }
            }
        }
    }


    private GameObject getObjectForValue(int value)
    {
        foreach (MatrixObjects matrixObject in Objects)
        {
            if(matrixObject.Value.Equals(value))
            {
                return matrixObject.Object;
            }
        }
        return null;
    }
}
