using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Piece : MonoBehaviour
{
    [SerializeField] private float stepDelay = 1f;
    [SerializeField] private float lockDelay = 0.5f;

    private Board _board;
    private int _rotationIndex;
    private float _stepTime;
    private float _lockTime;

    public TetrominoData Data { get; set; }
    public Vector3Int Position { get; set; }
    public Vector3Int[] Cells { get; set; }

    public void Initialized(Board board, TetrominoData data, Vector3Int position)
    {
        _board = board;
        Data = data;
        Position = position;
        _rotationIndex = 0;
        _stepTime = Time.time + stepDelay;
        _lockTime = 0f;

        if (Cells == null)
        {
            Cells = new Vector3Int[Data.cells.Length];
        }

        for (int i = 0; i < Data.cells.Length; i++)
        {
            Cells[i] = (Vector3Int)Data.cells[i];
        }
    }

    private void Update()
    {
        _board.Clear(this);

        _lockTime += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            Rotate(-1);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            Rotate(1);
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            Move(Vector2Int.left);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            Move(Vector2Int.right);
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            Move(Vector2Int.down);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            HardDrop();
        }

        if (Time.time >= _stepTime)
        {
            Step();
        }

        _board.Set(this);
    }

    private void Step()
    {
        _stepTime = Time.time + stepDelay;
        Move(Vector2Int.down);

        if (_lockTime >= lockDelay)
        {
            Lock();
        }
    }

    private void Lock()
    {
        _board.Set(this);
        _board.ClearLines();
        _board.SpawnPiece();
    }

    private void HardDrop()
    {
        while (Move(Vector2Int.down))
        {
            continue;
        }

        Lock();
    }

    private bool Move(Vector2Int translation)
    {
        Vector3Int newPosition = Position;
        newPosition.x += translation.x;
        newPosition.y += translation.y;

        bool valid = _board.IsValidPosition(this, newPosition);
        if (valid)
        {
            Position = newPosition;
            _lockTime = 0f;
        }

        return valid;
    }

    private void Rotate(int direction)
    {
        int originalRotation = _rotationIndex;
        _rotationIndex = Wrap(_rotationIndex + direction, 0, 4);

        ApplyRotationMatrix(direction);
        if (!TestWallKick(_rotationIndex, direction))
        {
            _rotationIndex = originalRotation;
            ApplyRotationMatrix(-direction);
        }
    }

    private void ApplyRotationMatrix(int direction)
    {
        for (int i = 0; i < Cells.Length; i++)
        {
            Vector3 cell = Cells[i];
            int x, y;
            switch (Data.tetromino)
            {
                case Tetromino.I:
                case Tetromino.O:
                    cell.x -= 0.5f;
                    cell.y -= 0.5f;
                    x = Mathf.CeilToInt((cell.x * GameData.RotationMatrix[0] * direction) + (cell.y * GameData.RotationMatrix[1] * direction));
                    y = Mathf.CeilToInt((cell.x * GameData.RotationMatrix[2] * direction) + (cell.y * GameData.RotationMatrix[3] * direction));
                    break;
                default:
                    x = Mathf.RoundToInt((cell.x * GameData.RotationMatrix[0] * direction) + (cell.y * GameData.RotationMatrix[1] * direction));
                    y = Mathf.RoundToInt((cell.x * GameData.RotationMatrix[2] * direction) + (cell.y * GameData.RotationMatrix[3] * direction));
                    break;
            }
            Cells[i] = new Vector3Int(x, y, 0);
        }
    }
    private bool TestWallKick(int rotationIndex, int rotationDirection)
    {
        var wallKickIndex = GetWallKickIndex(rotationIndex, rotationDirection);
        for (int i = 0; i < Data.wallKicks.GetLength(1); i++)
        {
            Vector2Int translation = Data.wallKicks[wallKickIndex, i];
            if (Move(translation))
            {
                return true;
            }
        }
        return false;
    }

    private int GetWallKickIndex(int rotationIndex, int rotationDirection)
    {
        var wallKickIndex = rotationIndex * 2;
        if (rotationDirection < 0)
        {
            wallKickIndex--;
        }
        return Wrap(wallKickIndex, 0, Data.wallKicks.GetLength(0));
    }

    private int Wrap(int input, int min, int max)
    {
        if (input < min)
        {
            return max - (min - input) % (max - min);
        }
        else
        {
            return min + (input - min) % (max - min);
        }
    }
}
