using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks.Sources;

public class MyBot : IChessBot
{
// Piece values: null, pawn, knight, bishop, rook, queen, king
    static int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };

    private static int evaluate(Board board)
    {
        int whiteScore = 0;
        PieceList[] pieces = board.GetAllPieceLists();
        for (int i = 0; i < 6; i++)
        {
            foreach (Piece piece in pieces[i])
            {
                whiteScore += pieceValues[(int)piece.PieceType];
            }

        }
        int blackScore = 0;
        for (int i = 6; i < pieces.Length; i++)
        {
            foreach (Piece piece in pieces[i])
            {
                blackScore += pieceValues[(int)piece.PieceType];
            }

        }
        int p = board.IsWhiteToMove ? 1 : -1;
        return (whiteScore - blackScore) * p;
    }
    private static int Quiesce(Board board, int alpha, int beta)
    {
        int stand_pat = evaluate(board);
        if(stand_pat >= beta)
        {
            return beta;
        }
        if(alpha < stand_pat)
        {
            alpha = stand_pat;
        }

        Move[] moves = board.GetLegalMoves(true);
        
        foreach (Move move in moves)
        {
            board.MakeMove(move);
            int score = -Quiesce(board, -beta, -alpha);
            board.UndoMove(move);

            if(score >= beta)
            {
                return beta;
            }
            if(score > alpha)
            {
                alpha = score; 
            }
        }
        return alpha; 
    }

    private static int search(Board board, int depth, int alpha, int beta)
    {
        if (depth == 0)
        {
            return evaluate(board);
        }

        Move[] moves = board.GetLegalMoves();
        if (moves.Length == 0 && board.IsInCheck()) return int.MinValue;
        else if (moves.Length == 0) return 0;

        foreach (Move move in moves)
        {
            board.MakeMove(move);
            int eval = -search(board, depth - 1,-beta,-alpha);
            board.UndoMove(move);
            if(eval >= beta)
            {
                return beta;
            }
            alpha = Math.Max(alpha, eval);

        }
        return alpha;
    }

    public Move Think(Board board, Timer timer)
    {
        int depth = 4;
        int prevEval = int.MinValue;

        Move[] moves = board.GetLegalMoves();
        Move maxMove = moves[0];
        foreach (Move move in moves)
        {
            board.MakeMove(move);
            int eval = -search(board, depth, int.MinValue, int.MaxValue);
            board.UndoMove(move);

            if(eval > prevEval)
            {
                maxMove = move;
            }
            prevEval = eval;
        }
        if(maxMove == moves[0])
        {
            System.Console.WriteLine("Same as first");
        }
        return maxMove;
    }
}