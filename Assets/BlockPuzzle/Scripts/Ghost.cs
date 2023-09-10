using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Ghost : MonoBehaviour
{
    [SerializeField] private Tile tile;
    [SerializeField] private Board board;
    [SerializeField] private Piece trackingPiece;

    private Tilemap _tilemap;
    private Vector3Int[] _cells;
    private Vector3Int _position;

    private void Awake()
    {
        _tilemap = GetComponentInChildren<Tilemap>();
        _cells = new Vector3Int[4];
    }

    private void LateUpdate()
    {
        Clear();
        Copy();
        Drop();
        Set();
    }

    private void Clear()
    {
        for (int i = 0; i < _cells.Length; i++)
        {
            var tilePosition = _cells[i] + _position;
            _tilemap.SetTile(tilePosition, null);
        }
    }

    private void Copy()
    {
        for (int i = 0; i < _cells.Length; i++)
        {
            _cells[i] = trackingPiece.Cells[i];
        }
    }

    private void Drop()
    {
        var position = trackingPiece.Position;

        var current = position.y;
        var bottom = -board.BoardSize.y / 2 - 1;

        board.Clear(trackingPiece);

        for (int row = current; row >= bottom; row--)
        {
            position.y = row;
            if (board.IsValidPosition(trackingPiece, position))
            {
                _position = position;
            }
            else
            {
                break;
            }
        }

        board.Set(trackingPiece);

    }

    private void Set()
    {
        for (int i = 0; i < _cells.Length; i++)
        {
            var tilePosition = _cells[i] + _position;
            _tilemap.SetTile(tilePosition, tile);
        }
    }
}
