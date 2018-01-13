﻿using System.Collections.Generic;
using UnityEngine;

public class CaveRegion
{
    public int RegionSize { get { return tiles.Count; } }       // 区域大小
    protected List<CaveCoord> tiles = new List<CaveCoord>();    // 所有坐标

    public CaveCoord averageCoord;                              // 平均点
    public Vector2 variance;

    public CaveRegion() { }

    public CaveRegion(List<CaveCoord> tiles)
    {
        SetTiles(tiles);
    }
    /// <summary>
    /// 设置坐标列表
    /// </summary>
    public void SetTiles(List<CaveCoord> tiles)
    {
        this.tiles = tiles;
        UpdateAverageCoord();
    }

    /// <summary>
    /// 更新区域平均点
    /// </summary>
    private void UpdateAverageCoord()
    {
        if (tiles.Count == 0)
        {
            Debug.LogWarning("CaveRegion tiles Count Is Zero.");
            averageCoord = new CaveCoord(0, 0);
            return;
        }
        float x = 0, y = 0;
        for (int i = 0; i < tiles.Count; i++)
        {
            x += tiles[i].tileX;
            y += tiles[i].tileY;
        }
        averageCoord = new CaveCoord(Mathf.RoundToInt(x / tiles.Count), Mathf.RoundToInt(y / tiles.Count));
    }

    /// <summary>
    /// 更新区域所有点的方差，需要确保更新了平均点
    /// </summary>
    public Vector2 UpdateVariance()
    {
        float x = 0, y = 0;

        for (int i = 0; i < tiles.Count; i++)
        {
            x += GameMathf.Pow2(tiles[i].tileX - averageCoord.tileX);
            y += GameMathf.Pow2(tiles[i].tileY - averageCoord.tileY);
        }

        return variance = new Vector2(x / tiles.Count, y / tiles.Count);
    }
}