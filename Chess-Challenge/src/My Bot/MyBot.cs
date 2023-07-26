using ChessChallenge.API;
using System;
using System.Diagnostics;

public class MyBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();
        int evaluation = Evaluate(board);
        Console.WriteLine(evaluation);
        return moves[0];
    }

    // Piece values
    int[] piece_points = { 0, 100, 300, 300, 500, 900, 0 };

    private int Evaluate(Board board)
    {
        int total=0;
        foreach(PieceList pl in board.GetAllPieceLists()) {
            foreach(Piece p in pl)
            {
                if (!p.IsWhite)
                    total += piece_points[(int)p.PieceType];
                else
                    total -= piece_points[(int)p.PieceType];
            }
        }
        return total;
    }
}