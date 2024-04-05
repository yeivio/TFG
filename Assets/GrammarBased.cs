using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using static UnityEngine.UI.Image;

public class GrammarBased : GenerationAlgorithm
{
    public string textoGramaticas;
    public string textoInicial;
    private string cadenaInicial;

    public int MinNumRoom;
    public int MaxNumRoom;

    public static string PREMISA_PATTERN = "([a-zA-Z]) *->(.*)";
    public static string CONSECUENTES_PATTERN = " *([a-zA-Z]+) *\\|*";

    Regex regex_premisa = new Regex(PREMISA_PATTERN);
    Regex regex_consecuentes = new Regex(CONSECUENTES_PATTERN);

    private Dictionary<string, List<string>> listaReglas;
    

    private float bucleBreak1;
    private float bucleBreak2;
    private float bucleBreak3;
    
    public override void Generate(int seed = -1)
    {
        cadenaInicial = textoInicial;
        base.GenerateSeed(seed);
        listaReglas = new Dictionary<string, List<string>>();
        RegistrarReglas();

        bucleBreak3 = 0;
        
        while (ComprobarReglas(cadenaInicial) && bucleBreak3 < 100)
        {
            bucleBreak3++;
            cadenaInicial = AplicarReglas(cadenaInicial);
        }
        Debug.Log("Resultado:" + cadenaInicial);
    }

    private bool ComprobarReglas(string cadena)
    {
        return cadena.Any(x => Char.IsUpper(x));
    }

    private string AplicarReglas(string cadena)
    {
        Char antecedente;
        int index = 0;
        while (Char.IsLower(cadena[index]))
        {
            index++;
        }
        antecedente = cadena[index];
        
        // Se elige un consecuente aleatorio entre los posibles.
        int conseucneteIndex = UnityEngine.Random.Range(0, listaReglas[antecedente.ToString()].Count);
        string consecuente = listaReglas[antecedente.ToString()][conseucneteIndex];
        string modificado = cadena.Substring(0, index) + consecuente + cadena.Substring(index + 1);
        string a1 = cadena.Substring(0, index);
        string a2 = cadena.Substring(index + 1);


        return modificado;
    }
    private void RegistrarReglas()
    {
        bucleBreak1 = 0;
        bucleBreak2 = 0;
        Match matchedRules = regex_premisa.Match(textoGramaticas);
        while (matchedRules.Success && bucleBreak1 < 1000)
        {
            bucleBreak1++;
            for (int i = 0; i < matchedRules.Groups.Count; i++)
            {
                Group g = matchedRules.Groups[i];
                if (i == 1)
                {
                    //Debug.Log("se añade clave:" + g.ToString());
                    listaReglas.Add(g.ToString(), new List<string>());
                }

                if (i == 2)
                {
                    string cons = g.ToString();
                    Match matchedcons = regex_consecuentes.Match(cons);

                    while (matchedcons.Success && bucleBreak2 < 1000)
                    {
                        bucleBreak2++;
                        for (int p = 0; p < matchedcons.Groups.Count; p++)
                        {
                            Group grup = matchedcons.Groups[p];
                            if (p != 0 && grup.ToString().TrimStart(' ').Length != 0)
                            {
                                //Debug.Log("se añade en:" + matchedRules.Groups[i - 1].ToString() + "," + grup.ToString());
                                listaReglas[matchedRules.Groups[i - 1].ToString()].Add(grup.ToString());
                            }
                        }

                        matchedcons = matchedcons.NextMatch();
                    }
                }
            }
            matchedRules = matchedRules.NextMatch();
        }

        if(bucleBreak1 > 1000)
        {
            Debug.LogError("Error bucle premisas");
        }
        if(bucleBreak2 > 1000)
        {
            Debug.LogError("Error bucle consecuentes");
        }

    }
}

# region GizmoEditor
#if UNITY_EDITOR
[CustomEditor(typeof(GrammarBased))]
public class ScriptEditorGB : Editor
{
    private GrammarBased gizmoDrawing;

    public override void OnInspectorGUI()
    {
        gizmoDrawing = (GrammarBased)target;
        gizmoDrawing.widthMap = EditorGUILayout.IntSlider("Width", gizmoDrawing.widthMap, 0, 300);
        gizmoDrawing.heightMap = EditorGUILayout.IntSlider("Height", gizmoDrawing.heightMap, 0, 300);
        gizmoDrawing.tileSize = EditorGUILayout.IntSlider("Tile Size", gizmoDrawing.tileSize, 1, 100);
        EditorGUILayout.FloatField("Execution time (ms)", gizmoDrawing.executionTime);
        //DrawDefaultInspector(); // Draw all public variables
        EditorGUILayout.Space();

        gizmoDrawing.seed = EditorGUILayout.IntField("Seed", gizmoDrawing.seed);
        gizmoDrawing.MinNumRoom = EditorGUILayout.IntField("Min Room Num", gizmoDrawing.MinNumRoom);
        gizmoDrawing.MaxNumRoom = EditorGUILayout.IntField("Max Room Num", gizmoDrawing.MaxNumRoom);
        gizmoDrawing.textoGramaticas = EditorGUILayout.TextArea(gizmoDrawing.textoGramaticas , EditorStyles.textArea);
        gizmoDrawing.textoInicial = EditorGUILayout.TextArea(gizmoDrawing.textoInicial, EditorStyles.textArea);

        if (GUILayout.Button("Generate cellular automata"))
        {
            gizmoDrawing.Generate(gizmoDrawing.seed);

        }
        if (GUILayout.Button("Generate random cellular automata"))
        {
            gizmoDrawing.Generate();
        }

        if (GUI.changed)
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
    }
}
#endif
#endregion

