using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
public class UIPointLine : MaskableGraphic
{
    internal List<Vector2> Positions = new List<Vector2>();
    public Vector2[] points = new Vector2[] { new Vector2(0, 0), new Vector2(500, -50), new Vector2(1000, -500) };
    public static float weight;
    private float _weight, _weight2;
    public float posTStart, posTEnd;
    int PosStart, PosEnd;
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear(); //一度削除しないと前の線が残る

        _weight = 0;
        for (int i = 1; i < Positions.Count; i++)
        {
            _weight2 = (float)(PosStart + i - 1) / 64 * weight;
            OneLineDraw(i, vh, _weight, _weight2);
            _weight = _weight2;
        }
    }
    void OneLineDraw(int n, VertexHelper vh, float _weight1, float _weight2) //分割した直線の四角形を描く
    {
        var pos1_to_2 = Positions[n] - Positions[n - 1];
        var verticalVector = CalcurateVerticalVector(pos1_to_2);

        var pos1Top = Positions[n - 1] + verticalVector * -_weight1 / 2;
        var pos1Bottom = Positions[n - 1] + verticalVector * _weight1 / 2;
        var pos2Top = Positions[n] + verticalVector * -_weight2 / 2;
        var pos2Bottom = Positions[n] + verticalVector * _weight2 / 2;

        AddVert(vh, pos1Top);
        AddVert(vh, pos1Bottom);
        AddVert(vh, pos2Top);
        AddVert(vh, pos2Bottom);


        vh.AddTriangle(4 * n - 4, 4 * n - 3, 4 * n - 2);
        vh.AddTriangle(4 * n - 3, 4 * n - 2, 4 * n - 1);
    }
    private void AddVert(VertexHelper vh, Vector2 pos)
    {
        var vert = UIVertex.simpleVert;
        vert.position = pos;
        vert.color = color;
        vh.AddVert(vert);
    }

    private Vector2 CalcurateVerticalVector(Vector2 vec)
    {
        if (vec.y == 0)
        {
            return Vector2.up; //0のときは垂直線
        }
        else
        {
            var verticalVector = new Vector2(1.0f, -vec.x / vec.y);
            return verticalVector.normalized;
        }
    }

    protected override void Start()
    {
        _weight = 0;
        posTStart = 0;
        posTEnd = 1;
        PosStart = 1;
        PosEnd = 1;
    }

	private void Update()
    {
        posTStart = Math.Max(0, posTStart);
        posTEnd = Math.Min(1, posTEnd);
        PosStart = (int)(posTStart * 64) + 1; PosEnd = (int)(posTEnd * 64);
        Positions = Enumerable.Range(PosStart, PosEnd - PosStart > 0 ? PosEnd - PosStart : 1)
            .Select(p => GetPoint(points, p / 64f)).ToList();
        SetVerticesDirty();
    }

    public static Vector2 GetPoint(Vector2[] points, float t) //曲線の計算
    {
        var len = points.Length - 1;
        var oneMinusT = 1f - t;
        var lines = points.Select((p, index) => NCR(len, index) * Mathf.Pow(t, index) * Mathf.Pow(oneMinusT, len - index) * p);
        return new Vector2(lines.Sum(l => l.x), lines.Sum(l => l.y));
    }
    public static int NCR(int n, int r) //ペジェ曲線の計算に使う（組み合わせ）
    {
        if (n < r) return 0;
        if (n == r) return 1;

        int x = 1;
        for (int i = 0; i < r; i++)
        {
            x = x * (n - i) / (i + 1);
        }
        return x;
    }
}