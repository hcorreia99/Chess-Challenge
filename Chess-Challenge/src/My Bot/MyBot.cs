using ChessChallenge.API;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks.Sources;

public class MyBot : IChessBot
{
// Piece values: null, pawn, knight, bishop, rook, queen, king
    static int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };

    Board board;
    int plusInfinity = int.MaxValue-1;
    int minusInfinity = int.MinValue+1;
    Move bestMove;
    int posEvaluated = 0;
    int bestEval;
    float endgameMaterialStart = pieceValues[3] * 2 + pieceValues[3] + pieceValues[2];
    Dictionary<Move,int> orderedMoves = new Dictionary<Move,int>();

    private int evaluate()
    {
        int whiteScore = 0;
        int blackScore = 0;
        PieceList[] pieces = board.GetAllPieceLists();
        int numWhitePieces = 0;
        int numBlackPieces = 0;
        int numWhiteNonPawns = 0;
        int numBlackNonPawns = 0;
        foreach(PieceList list in pieces)
        {
            foreach (Piece piece in list)
            {
                if (piece.IsWhite)
                {
                    numWhitePieces++;
                    whiteScore += pieceValues[(int)piece.PieceType];
                    if (!piece.IsPawn)
                    {
                        numWhiteNonPawns++; 
                    }
                }
                else {
                    numBlackPieces++;
                    blackScore += pieceValues[(int)piece.PieceType];
                    if (!piece.IsPawn)
                    {
                        numBlackNonPawns++; 
                    }
                }

            }

        }

        int p = board.IsWhiteToMove ? 1 : -1;
        float blackEndgameWeight = (1 / numWhiteNonPawns)*10;
        float whiteEndgameWeight = (1 / numBlackNonPawns)*10;
        //float blackEndgameWeight = 1 - Math.Min(1, (1 / endgameMaterialStart) * numBlackNonPawns);
        //float whiteEndgameWeight = 1 - Math.Min(1, (1 / endgameMaterialStart) * numWhiteNonPawns);
        //Console.WriteLine(blackEndgameWeight); 
        //Console.WriteLine(whiteEndgameWeight); 
        Square wKingSquare = board.GetKingSquare(true);
        Square bKingSquare = board.GetKingSquare(false);

        int dstBetweenKings = Math.Abs(bKingSquare.File - wKingSquare.File) + Math.Abs(bKingSquare.Rank - wKingSquare.Rank);
        int whiteKingFile = Math.Max(3 - wKingSquare.File, wKingSquare.File - 4);
        int whiteKingRank = Math.Max(3 - wKingSquare.Rank, wKingSquare.Rank - 4);

        blackScore += (int)((whiteKingFile + whiteKingRank+ (14-dstBetweenKings))*whiteEndgameWeight);

        int blackKingFile = Math.Max(3 - bKingSquare.File, bKingSquare.File - 4);
        int backKingRank = Math.Max(3 - bKingSquare.Rank, bKingSquare.Rank - 4);


        whiteScore += (int)((backKingRank + blackKingFile + (14-dstBetweenKings))*blackEndgameWeight);


        posEvaluated++; 
        return (whiteScore - blackScore) * p;
    }
    private int Quiesce(int alpha, int beta)
    {
        int stand_pat = evaluate();
        if(stand_pat >= beta)
        {
            return beta;
        }
        if(alpha < stand_pat)
        {
            alpha = stand_pat;
        }

        Move[] moves = board.GetLegalMoves(true);
        OrderMoves(moves);
        
        foreach (Move move in moves)
        {
            board.MakeMove(move);
            int score = -Quiesce(-beta, -alpha);
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

    private int search(int depth, int alpha, int beta, int distanceFromRoot)
    {
        if (depth == 0)
        {
            //return evaluate();
            return Quiesce(alpha, beta);
        }

        Move[] moves = board.GetLegalMoves();
        OrderMoves(moves);
        if (moves.Length == 0 && board.IsInCheck()) return int.MinValue;
        else if (moves.Length == 0) return 0;

        foreach (Move move in moves)
        {
            board.MakeMove(move);
            int eval = -search(depth - 1, -beta, -alpha, distanceFromRoot+1);
            board.UndoMove(move);
            if(eval >= beta)
            {
                return beta;
            }
            if(eval > alpha)
            {
                alpha = eval;
                if(distanceFromRoot == 0)
                {
                    bestMove = move;
                    bestEval = alpha;
                }
            }

        }
        return alpha;
    }

    public int rankMove(Move a)
    {
        int moveScore = 0;
        if (a.IsCapture)
        {
             moveScore += 10 * pieceValues[(int)a.CapturePieceType] - pieceValues[(int)a.MovePieceType];
        }

        if (a.IsPromotion)
        {
            moveScore += pieceValues[(int)a.PromotionPieceType];
        }

        if (board.SquareIsAttackedByOpponent(a.TargetSquare))
        {
            moveScore -= pieceValues[(int)a.MovePieceType];
        }
        return moveScore;
    }

    public void OrderMoves(Move[] moves)
    {
        orderedMoves.Clear();
        foreach (Move move in moves)
        {
            orderedMoves.Add(move, rankMove(move));
        }
        var movesList = orderedMoves.ToList();
        movesList.Sort( (a,b) => -(a.Value - b.Value));
        int i = 0;
        movesList.ForEach((a) =>
        {
            moves[i] = a.Key; i++;
        });
    }
    private int search(int depth)
    {
        if (depth == 0)
        {
            return evaluate();
        }

        Move[] moves = board.GetLegalMoves();
        if (moves.Length == 0 && board.IsInCheck()) return int.MinValue;
        else if (moves.Length == 0) return 0;
        int best = int.MinValue+1;

        foreach (Move move in moves)
        {
            board.MakeMove(move);
            int eval = -search(depth - 1);
            board.UndoMove(move);
            if(eval > best)
            {
                best = eval;
            
            }

        }
        return best;
    }

    public Move Think(Board board, Timer timer)
    {
        this.board = board;
        int depth = 5;

        search(depth, minusInfinity, plusInfinity,0);
        Console.WriteLine(posEvaluated);
        posEvaluated = 0;
        return this.bestMove;
    }
}