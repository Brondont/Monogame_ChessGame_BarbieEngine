using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace ChessGame.Models
{
  public static class ChessUtils
  {
    public static ChessPiece GetPieceAtTile(ChessTile tile, List<ChessPiece> chessPieces)
    {
      foreach (var piece in chessPieces)
      {
        if (piece.HomeTile == tile)
        {
          return piece;
        }
      }
      return null;
    }

    public static bool IsTileInBounds(int index, int boardSize)
    {
      return index >= 0 && index < boardSize;
    }


    public static bool IsKingInCheck(Player playerColor, List<ChessTile> chessBoard, List<ChessPiece> chessPieces)
    {
      var kingTile = chessPieces.Find(p => p.PieceColor == playerColor && p.Type == "king").HomeTile;
      foreach (var piece in chessPieces)
      {
        if (piece.PieceColor != playerColor && piece.Type != "king")
        {
          var moves = piece.GetLegalMoves(chessBoard, chessPieces);
          if (moves.Contains(kingTile))
          {
            return true;
          }
        }
      }
      return false;
    }


  }

  public class Pawn : ChessPiece
  {
    public Pawn(Player color, ChessTile homeTile) : base("pawn", color, homeTile) { }

    public override List<ChessTile> GetLegalMoves(List<ChessTile> chessBoard, List<ChessPiece> chessPieces)
    {
      var validMoves = new List<ChessTile>();
      int direction = (PieceColor == Player.White) ? -1 : 1;
      int currentIndex = chessBoard.IndexOf(HomeTile);

      if (currentIndex == -1) return validMoves;

      // Move one tile forward
      AddForwardMove(validMoves, direction, currentIndex, chessBoard, chessPieces);

      // Move two tiles forward if at starting position
      AddDoubleForwardMove(validMoves, direction, currentIndex, chessBoard, chessPieces);

      // Capture moves
      AddCaptureMoves(validMoves, direction, currentIndex, chessBoard, chessPieces);

      // TODO: Implement en passant capture

      return validMoves;
    }

    private static void AddForwardMove(List<ChessTile> validMoves, int direction, int currentIndex, List<ChessTile> chessBoard, List<ChessPiece> chessPieces)
    {
      // multiplying by 8 moves the row down by one 
      int forwardIndex = currentIndex + direction * 8;
      if (ChessUtils.IsTileInBounds(forwardIndex, chessBoard.Count) && IsTileFree(forwardIndex, chessBoard, chessPieces))
      {
        validMoves.Add(chessBoard[forwardIndex]);
      }
    }

    private void AddDoubleForwardMove(List<ChessTile> validMoves, int direction, int currentIndex, List<ChessTile> chessBoard, List<ChessPiece> chessPieces)
    {
      // if the pawns are starting position 
      if ((PieceColor == Player.White && HomeTile.TileCoordinate[1] == '2') ||
          (PieceColor == Player.Black && HomeTile.TileCoordinate[1] == '7'))
      {
        int doubleForwardIndex = currentIndex + direction * 16;
        int forwardIndex = currentIndex + direction * 8;
        // check if both tiles infront of the pawn are free 
        if (ChessUtils.IsTileInBounds(doubleForwardIndex, chessBoard.Count) &&
            IsTileFree(doubleForwardIndex, chessBoard, chessPieces) &&
            IsTileFree(forwardIndex, chessBoard, chessPieces))
        {
          validMoves.Add(chessBoard[doubleForwardIndex]);
        }
      }
    }

    private void AddCaptureMoves(List<ChessTile> validMoves, int direction, int currentIndex, List<ChessTile> chessBoard, List<ChessPiece> chessPieces)
    {
      // -1 and + 1 to check the front left and front right 
      int leftCaptureIndex = currentIndex + direction * 8 - 1;
      int rightCaptureIndex = currentIndex + direction * 8 + 1;
      // check if enemy piece exists on the left side
      if (IsValidCapture(leftCaptureIndex, chessBoard, chessPieces))
      {
        validMoves.Add(chessBoard[leftCaptureIndex]);
      }
      // check if enemy piece exists on the right side 
      if (IsValidCapture(rightCaptureIndex, chessBoard, chessPieces))
      {
        validMoves.Add(chessBoard[rightCaptureIndex]);
      }
    }

    private static bool IsTileFree(int index, List<ChessTile> chessBoard, List<ChessPiece> chessPieces)
    {
      return ChessUtils.IsTileInBounds(index, chessBoard.Count) &&
             ChessUtils.GetPieceAtTile(chessBoard[index], chessPieces) == null;
    }

    private bool IsValidCapture(int index, List<ChessTile> chessBoard, List<ChessPiece> chessPieces)
    {
      return ChessUtils.IsTileInBounds(index, chessBoard.Count) &&
             ChessUtils.GetPieceAtTile(chessBoard[index], chessPieces) is ChessPiece piece &&
             piece.PieceColor != this.PieceColor;
    }
  }

  public class Rook : ChessPiece
  {
    public Rook(Player color, ChessTile homeTile) : base("rook", color, homeTile) { }

    public override List<ChessTile> GetLegalMoves(List<ChessTile> chessBoard, List<ChessPiece> chessPieces)
    {
      var validMoves = new List<ChessTile>();
      int currentIndex = chessBoard.IndexOf(HomeTile);

      if (currentIndex == -1) return validMoves;

      // Directions: up, down, left, right
      int[] directions = { -8, 8, -1, 1 };

      // for every direction
      foreach (var direction in directions)
      {
        // for every tile in that direction
        for (int i = 1; i < 8; i++)
        {
          // i acts to navigate in every direction to every tile vertically and hori depeending on whcih direction it is 
          int nextIndex = currentIndex + direction * i;

          if (!IsValidMove(nextIndex, direction, currentIndex, chessBoard.Count))
            break;

          var targetTile = chessBoard[nextIndex];
          var occupyingPiece = ChessUtils.GetPieceAtTile(targetTile, chessPieces);

          if (occupyingPiece == null)
          {
            validMoves.Add(targetTile);
          }
          else
          {
            if (occupyingPiece.PieceColor != this.PieceColor)
            {
              validMoves.Add(targetTile);
            }
            // stop checking futher tiles because its blocked by an early piece 
            break;
          }
        }
      }

      return validMoves;
    }

    private static bool IsValidMove(int nextIndex, int direction, int currentIndex, int boardSize)
    {
      return ChessUtils.IsTileInBounds(nextIndex, boardSize) &&
             (direction == -1 || direction == 1 ? nextIndex / 8 == currentIndex / 8 : true);
    }
  }

  public class Knight : ChessPiece
  {
    public Knight(Player color, ChessTile homeTile) : base("knight", color, homeTile) { }

    public override List<ChessTile> GetLegalMoves(List<ChessTile> chessBoard, List<ChessPiece> chessPieces)
    {
      var validMoves = new List<ChessTile>();
      int currentIndex = chessBoard.IndexOf(HomeTile);

      if (currentIndex == -1) return validMoves;

      // Possible moves
      int[] moveOffsets = { -17, -15, -10, -6, 6, 10, 15, 17 };

      foreach (var offset in moveOffsets)
      {
        int newIndex = currentIndex + offset;

        if (ChessUtils.IsTileInBounds(newIndex, chessBoard.Count) &&
            IsValidKnightMove(currentIndex, newIndex))
        {
          var targetTile = chessBoard[newIndex];
          var occupyingPiece = ChessUtils.GetPieceAtTile(targetTile, chessPieces);

          if (occupyingPiece == null || occupyingPiece.PieceColor != this.PieceColor)
          {
            validMoves.Add(targetTile);
          }
        }
      }

      return validMoves;
    }

    private static bool IsValidKnightMove(int currentIndex, int newIndex)
    {
      int oldX = currentIndex % 8;
      int oldY = currentIndex / 8;
      int newX = newIndex % 8;
      int newY = newIndex / 8;

      return (Math.Abs(oldX - newX) == 1 && Math.Abs(oldY - newY) == 2) ||
             (Math.Abs(oldX - newX) == 2 && Math.Abs(oldY - newY) == 1);
    }
  }

  public class Bishop : ChessPiece
  {
    public Bishop(Player color, ChessTile homeTile) : base("bishop", color, homeTile) { }

    public override List<ChessTile> GetLegalMoves(List<ChessTile> chessBoard, List<ChessPiece> chessPieces)
    {
      var validMoves = new List<ChessTile>();
      int currentIndex = chessBoard.IndexOf(HomeTile);

      if (currentIndex == -1) return validMoves;

      // Define directions for diagonal moves
      int[] directions = { 9, 7, -7, -9 };

      foreach (var direction in directions)
      {
        for (int i = 1; i < 8; i++)
        {
          int newIndex = currentIndex + i * direction;

          if (!IsValidMove(newIndex, direction, currentIndex, chessBoard.Count))
            break;

          var targetTile = chessBoard[newIndex];
          var occupyingPiece = ChessUtils.GetPieceAtTile(targetTile, chessPieces);

          if (occupyingPiece == null)
          {
            validMoves.Add(targetTile);
          }
          else
          {
            if (occupyingPiece.PieceColor != this.PieceColor)
            {
              validMoves.Add(targetTile);
            }
            break;
          }
        }
      }

      return validMoves;
    }

    private static bool IsValidMove(int newIndex, int direction, int currentIndex, int boardSize)
    {
      return ChessUtils.IsTileInBounds(newIndex, boardSize) &&
             Math.Abs(currentIndex % 8 - newIndex % 8) == Math.Abs(currentIndex / 8 - newIndex / 8);
    }
  }

  public class Queen : ChessPiece
  {
    public Queen(Player color, ChessTile homeTile) : base("queen", color, homeTile) { }

    public override List<ChessTile> GetLegalMoves(List<ChessTile> chessBoard, List<ChessPiece> chessPieces)
    {
      var validMoves = new List<ChessTile>();

      // Combine moves of Rook and Bishop
      validMoves.AddRange(new Rook(PieceColor, HomeTile).GetLegalMoves(chessBoard, chessPieces));
      validMoves.AddRange(new Bishop(PieceColor, HomeTile).GetLegalMoves(chessBoard, chessPieces));

      return validMoves;
    }
  }

  public class King : ChessPiece
  {
    public King(Player color, ChessTile homeTile) : base("king", color, homeTile) { }

    public override List<ChessTile> GetLegalMoves(List<ChessTile> chessBoard, List<ChessPiece> chessPieces)
    {
      var validMoves = new List<ChessTile>();
      int currentIndex = chessBoard.IndexOf(HomeTile);

      if (currentIndex == -1) return validMoves;

      // Possible moves
      int[] moveOffsets = { -9, -8, -7, -1, 1, 7, 8, 9 };

      foreach (var offset in moveOffsets)
      {
        int newIndex = currentIndex + offset;

        if (ChessUtils.IsTileInBounds(newIndex, chessBoard.Count) &&
            IsValidKingMove(currentIndex, newIndex))
        {
          var targetTile = chessBoard[newIndex];
          var occupyingPiece = ChessUtils.GetPieceAtTile(targetTile, chessPieces);

          if (occupyingPiece == null || occupyingPiece.PieceColor != this.PieceColor) // check if tile has no piece or the piece of is opposite color 
            validMoves.Add(targetTile);
        }
      }

      return validMoves;
    }
    private static bool IsValidKingMove(int currentIndex, int newIndex)
    {
      int oldX = currentIndex % 8;
      int oldY = currentIndex / 8;
      int newX = newIndex % 8;
      int newY = newIndex / 8;

      // checks if the distance between the current move and next move is one tile only
      return Math.Abs(oldX - newX) <= 1 && Math.Abs(oldY - newY) <= 1;
    }
    // TODO: add castles to the moveset 
  }
}