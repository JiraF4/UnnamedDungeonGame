using System;
using System.Collections.Generic;
using System.Linq;
using DeepDungeon.Dungeon;
using Dungeon;
using Dungeon.Tools;
using Godot;

public partial class MapChunkMesh : MeshInstance3D
{
    public Map Map;
    public Vector2I ChunkPosition;
    private Rect2I _mapRect;
    private ArrayMesh _mesh = new();

    private readonly Vector2I _texturesAtlasSize = new(32, 32);
    private const float TextureSize = 1.0f / 32.0f;
    public int MaxTextureId => _texturesAtlasSize.X * _texturesAtlasSize.Y;
    private Vector3[] _vertexNormals;
    private Vector2[] _vertexUvs1;
    private Vector2[] _vertexUvs2;
    private List<Vector3> _verts;
    private List<int> _indices;
    private int[,,,] _faces;

    public MapChunkMesh(Map map, Vector2I chunkPosition)
    {
        Map = map;
        ChunkPosition = chunkPosition;
        _mapRect = new Rect2I(chunkPosition * Map.ChunkSize, new Vector2I(Map.ChunkSize, Map.ChunkSize));
    }

    public void ClearBuffers()
    {
        _faces = null;
        _vertexNormals = null;
        _vertexUvs1 = null;
        _vertexUvs2 = null;
        _verts = null;
        _indices = null;
    }
    
    public void PrepBuffers()
    {
        _faces = new int[Map.ChunkSize, Map.ChunkSize, Map.ChunkSize, 6];
        _faces.Fill4D(-1);
        
        _vertexNormals = new Vector3[Map.ChunkSize * Map.ChunkSize * Map.ChunkSize * MaxTextureId];
        _vertexUvs1 = new Vector2[Map.ChunkSize * Map.ChunkSize * Map.ChunkSize * MaxTextureId];
        _vertexUvs2 = new Vector2[Map.ChunkSize * Map.ChunkSize * Map.ChunkSize * MaxTextureId];
        _vertexNormals.Fill(Vector3.Zero);
        _vertexUvs1.Fill(Vector2.Zero);
        _vertexUvs2.Fill(Vector2.Zero);
        _verts = new List<Vector3>();
        _indices = new List<int>();
    }

    public void ReBake()
    {
        _mesh.ClearSurfaces();
        PrepBuffers();

        PrepareFaces();
        PrepareMeshDate();
        ApplyMesh();
        ClearBuffers();
        AddColliders();
    }

    void AddColliders()
    {
        for (var x = 0; x < Map.ChunkSize; x++)
        {
            for (var z = 0; z < Map.ChunkSize; z++)
            {
                var matrixPosition = new Vector3I(x, 0, z);
                var cell = Map.MapCells[ChunkPosition.X * Map.ChunkSize + x, ChunkPosition.Y * Map.ChunkSize + z];
                if (cell.MapCellType == MapCellType.Empty) continue;
                if (!cell.Neighbours.Any(n => n != null && n.MapCellType == MapCellType.Empty)) continue;
                var staticBody = new StaticBody3D() { Position = new Vector3(x * Map.CellRealSize + Map.CellRealSize/2, Map.CellRealSize*3/2, z * Map.CellRealSize + Map.CellRealSize/2)};
                staticBody.AddChild(new CollisionShape3D() {Shape = Map.BlockCollisionShape});
                AddChild(staticBody);
            }
        }
    }

    void PrepareFaces()
    {
        for (var x = 0; x < Map.ChunkSize; x++)
        {
            for (var z = 0; z < Map.ChunkSize; z++)
            {
                var matrixPosition = new Vector3I(x, 0, z);
                var cell = Map.MapCells[ChunkPosition.X * Map.ChunkSize + x, ChunkPosition.Y * Map.ChunkSize + z];
                var textureOffset = cell.TextureOffset;
                if (cell.MapCellType == MapCellType.Empty)
                {
                    _faces[x, 0, z, 4] = 0 + textureOffset;
                    _faces[x, 2, z, 5] = 1 + textureOffset;
                    for (var d = 0; d < 4; d++)
                    {
                        var neighbour = cell.Neighbours[d];
                        if (neighbour != null && neighbour.MapCellType == MapCellType.Wall)
                        {
                            _faces[x, 0, z, d] = 2 + textureOffset;
                            _faces[x, 1, z, d] = 3 + textureOffset;
                            _faces[x, 2, z, d] = 4 + textureOffset;

                            for (var i = 0; i < 2; i++)
                            {
                                var neighbourCell = neighbour.Neighbours[(4 + (d + 1 - (2 * i))) % 4];
                                var neighbourCell2 = neighbourCell.Neighbours[(4 + (d + 2 - (4 * i))) % 4];
                                if (neighbourCell != null && (
                                    neighbourCell.MapCellType == MapCellType.Empty
                                    || (neighbourCell2 != null && (neighbourCell2.MapCellType != MapCellType.Empty || neighbourCell2.TextureOffset != textureOffset)
                                )))
                                {
                                    var offset = i * 3;
                                    if (_faces[x, 0, z, d] == 5) offset = 6;
                                    _faces[x, 0, z, d] = 5 + offset + textureOffset;
                                    _faces[x, 1, z, d] = 6 + offset + textureOffset;
                                    _faces[x, 2, z, d] = 7 + offset + textureOffset;
                                }
                            }

                            if (_faces[x, 0, z, d] == 2 && (Map.MapHolder.MapGenerator.Random.Next() % 20) == 0)
                            {
                                _faces[x, 0, z, d] = 14 + textureOffset;
                                _faces[x, 1, z, d] = 15 + textureOffset;
                                _faces[x, 2, z, d] = 16 + textureOffset;
                            }
                            
                        }
                    }
                }
                else
                    _faces[x, 3, z, 4] = 18;
            }
        }

    }
    void PrepareMeshDate()
    {
        for (var x = 0; x < Map.ChunkSize; x++)
        {
            for (var y = 0; y < Map.ChunkSize; y++)
            {
                for (var z = 0; z < Map.ChunkSize; z++)
                {
                    for (var f = 0; f < 6; f++)
                    {
                        var texture = _faces[x, y, z, f];
                        if (texture != -1)
                        {
                            var matrixPosition = new Vector3I(x, y, z);
                            AddFace(matrixPosition, f, texture);
                        }
                    }               
                }
            }
        }
    }
    void AddFace(Vector3I matrixPosition, int face, int textureId)
    {
        var offsets = new[]
        {
            new Vector3I( 0, 0, 1), // 0
            new Vector3I( 0, 1, 1), // 1
            new Vector3I( 1, 1, 1), // 2
            new Vector3I( 1, 0, 1), // 3
            new Vector3I( 1, 0, 0), // 4
            new Vector3I( 1, 1, 0), // 5
            new Vector3I( 0, 1, 0), // 6
            new Vector3I( 0, 0, 0), // 7
        };
        var faces = new[,]
        {
            {2, 1, 0, 3}, // z+ 0
            {5, 2, 3, 4}, // x+ 1
            {6, 5, 4, 7}, // z- 2
            {1, 6, 7, 0}, // x- 3
            {7, 4, 3, 0}, // y- 4
            {1, 2, 5, 6}, // y+ 5
        };
        var normals = new[]
        {
            new Vector3( 0,  0, -1), // z+ 0
            new Vector3(-1,  0,  0), // x+ 1
            new Vector3( 0,  0,  1), // z- 2
            new Vector3( 1,  0,  0), // x- 3
            new Vector3( 0,  1,  0), // y- 4
            new Vector3( 0, -1,  0), // y+ 5
        };
        var mergeDirections = new[]
        {
            new Vector3I(1, 1, 0), // z 0
            new Vector3I(0, 1, 1), // x 1
            new Vector3I(1, 1, 0), // z 2
            new Vector3I(0, 1, 1), // x 3
            new Vector3I(1, 0, 1), // y 4
            new Vector3I(1, 0, 1), // y 5
        };
        var uvMergeAxis = new[,]
        {
            {0, 1}, // z 0
            {2, 1}, // x 1
            {0, 1}, // z 2
            {2, 1}, // x 3
            {0, 2}, // y 4
            {0, 2}, // y 5
        };

        var mergeOffset = new[]
        {
            new Vector3I(0, 0, 0), // 0
            new Vector3I(0, 0, 0), // 1
            new Vector3I(0, 0, 0), // 2
            new Vector3I(0, 0, 0), // 3
        };
        var mergeSize = new Vector2I(1, 1);
        var mergePosition = matrixPosition;
        for (var mergeDirectionAxis = 0; mergeDirectionAxis < 3; mergeDirectionAxis++)
        {
            var checkPosition = mergePosition;
            var mergeDirection = mergeDirections[face][mergeDirectionAxis];
            if (mergeDirection > 0)
            {
                while (true)
                {
                    checkPosition[mergeDirectionAxis]++;
                    if (IsAllFacesSameTexture(matrixPosition, checkPosition, face, textureId))
                    {
                        for (var vertexIndex = 0; vertexIndex < 4; vertexIndex++)
                        {
                            if (offsets[faces[face, vertexIndex]][mergeDirectionAxis] > 0)
                            {
                                mergeOffset[vertexIndex][mergeDirectionAxis]++;
                            }
                        }
                    }
                    else
                        break;
                    mergePosition = checkPosition;
                }
            }
        }
        SetFacesTexture(matrixPosition, mergePosition, face, -1);
        var mergeRegion = mergePosition - matrixPosition;
        mergeSize = new Vector2I(mergeRegion[uvMergeAxis[face, 0]] + 1, mergeRegion[uvMergeAxis[face, 1]] + 1);

        var vert1 = CreateVertex(matrixPosition + offsets[faces[face, 0]] + mergeOffset[0]);
        var vert2 = CreateVertex(matrixPosition + offsets[faces[face, 1]] + mergeOffset[1]);
        var vert3 = CreateVertex(matrixPosition + offsets[faces[face, 2]] + mergeOffset[2]);
        var vert4 = CreateVertex(matrixPosition + offsets[faces[face, 3]] + mergeOffset[3]);
        AddUv1Quad(vert1, vert2, vert3, vert4, textureId);
        AddUv2Quad(vert1, vert2, vert3, vert4, mergeSize);
        AddNormalsQuad(vert1, vert2, vert3, vert4, normals[face]);
        AddIndicesQuad(vert1, vert2, vert3, vert4);
    }
    void SetFacesTexture(Vector3I from, Vector3I to, int face, int textureId)
    {
        for (var x = from.X; x <= to.X; x++)
        {
            for (var y = from.Y; y <= to.Y; y++)
            {
                for (var z = from.Z; z <= to.Z; z++)
                {
                    _faces[x, y, z, face] = textureId;
                }
            }
        }
    }
    bool IsAllFacesSameTexture(Vector3I from, Vector3I to, int face, int textureId)
    {
        if (from.X < 0) return false;
        if (from.Y < 0) return false;
        if (from.Z < 0) return false;
        if (to.X >= Map.ChunkSize) return false;
        if (to.Y >= Map.ChunkSize) return false;
        if (to.Z >= Map.ChunkSize) return false;
        for (var x = from.X; x <= to.X; x++)
        {
            for (var y = from.Y; y <= to.Y; y++)
            {
                for (var z = from.Z; z <= to.Z; z++)
                {
                    if (_faces[x, y, z, face] != textureId)
                        return false;
                }
            }
        }
        return true;
    }
    
    void ApplyMesh()
    {
        var surfaceArray = new Godot.Collections.Array();
        surfaceArray.Resize((int)Mesh.ArrayType.Max);
        surfaceArray[(int)Mesh.ArrayType.Vertex] = _verts.ToArray();
        surfaceArray[(int)Mesh.ArrayType.Normal] = _vertexNormals.Take(_verts.Count).ToArray();
        surfaceArray[(int)Mesh.ArrayType.TexUV] = _vertexUvs1.Take(_verts.Count).ToArray();
        surfaceArray[(int)Mesh.ArrayType.TexUV2] = _vertexUvs2.Take(_verts.Count).ToArray();
        surfaceArray[(int)Mesh.ArrayType.Index] = _indices.ToArray();
        MaterialOverride = Map.MapHolder.TestMaterial;
        _mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);
        CastShadow = ShadowCastingSetting.On;
        Mesh = _mesh;
    }

    int CreateVertex(Vector3I matrixPosition)
    {
        var vertexIndex = _verts.Count;
        _verts.Add(new Vector3(matrixPosition.X * Map.CellRealSize, matrixPosition.Y * Map.CellRealSize, matrixPosition.Z * Map.CellRealSize));
        return vertexIndex;
    }
    void AddUv1Quad(int vert1, int vert2, int vert3, int vert4, int textureIndex)
    {
        var texturePosition = new Vector2(textureIndex % _texturesAtlasSize.X, textureIndex / _texturesAtlasSize.X) * TextureSize;
        _vertexUvs1[vert1] = new Vector2(          0, 0          ) + texturePosition;
        _vertexUvs1[vert2] = new Vector2(TextureSize, 0          ) + texturePosition;
        _vertexUvs1[vert3] = new Vector2(TextureSize, TextureSize) + texturePosition;
        _vertexUvs1[vert4] = new Vector2(          0, TextureSize) + texturePosition;
    }
    void AddUv2Quad(int vert1, int vert2, int vert3, int vert4, Vector2I mergeSize)
    {
        _vertexUvs2[vert1] = new Vector2(0, 0);
        _vertexUvs2[vert2] = new Vector2(mergeSize.X, 0);
        _vertexUvs2[vert3] = new Vector2(mergeSize.X, mergeSize.Y);
        _vertexUvs2[vert4] = new Vector2(0, mergeSize.Y);
    }
    void AddIndicesQuad(int vert1, int vert2, int vert3, int vert4)
    {
        _indices.Add(vert1);
        _indices.Add(vert2);
        _indices.Add(vert3);
        _indices.Add(vert1);
        _indices.Add(vert3);
        _indices.Add(vert4);
    }
    void AddNormalsQuad(int vert1, int vert2, int vert3, int vert4, Vector3 newNormal)
    {
        _vertexNormals[vert1] = newNormal;
        _vertexNormals[vert2] = newNormal;
        _vertexNormals[vert3] = newNormal;
        _vertexNormals[vert4] = newNormal;
    }
}