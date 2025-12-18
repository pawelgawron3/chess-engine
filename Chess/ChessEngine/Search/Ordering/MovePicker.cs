using ChessEngine.Core.Chessboard;
using ChessEngine.Core.Moves;
using ChessEngine.Core.Rules;
using ChessEngine.Evaluation.Tables;
using ChessEngine.Game;

namespace ChessEngine.Search.Ordering;

public struct MovePicker
{
    private const int CAPTURE_BASE = 10_000;
    private const int KILLER_SCORE1 = 5_000;
    private const int KILLER_SCORE2 = 4_000;

    private readonly Move[] _moves;
    private readonly int[] _scores;
    private readonly int _depth;
    private readonly Move? _ttMove;
    private readonly GameStateEngine _state;
    private int _index = 0;

    public MovePicker(IEnumerable<Move> moves, GameStateEngine state, int depth, Move? ttMove)
    {
        _moves = moves.ToArray();
        _scores = new int[_moves.Length];
        _depth = depth;
        _ttMove = ttMove;
        _state = state;

        ScoreMoves();
        SortMoves();
    }

    public bool TryGetNext(out Move move)
    {
        if (_index >= _moves.Length)
        {
            move = default;
            return false;
        }

        move = _moves[_index++];
        return true;
    }

    private readonly void ScoreMoves()
    {
        for (int i = 0; i < _moves.Length; i++)
        {
            var move = _moves[i];

            if (_ttMove != null && move == _ttMove.Value)
            {
                _scores[i] = int.MaxValue;
                continue;
            }

            Piece? capturedPiece = AttackUtils.GetCapturedPiece(_state.Board, move);
            if (capturedPiece != null)
            {
                Piece victim = capturedPiece.Value;
                Piece attacker = _state.Board[move.From]!.Value;

                _scores[i] = CAPTURE_BASE
                    + PieceValues.Value[(int)victim.Type] * 10
                    - PieceValues.Value[(int)attacker.Type];
                continue;
            }

            if (KillerMoves.Get(_depth, 0) == move)
            {
                _scores[i] = KILLER_SCORE1;
                continue;
            }

            if (KillerMoves.Get(_depth, 1) == move)
            {
                _scores[i] = KILLER_SCORE2;
                continue;
            }

            _scores[i] = HistoryHeuristicTable.Get(move);
        }
    }

    private readonly void SortMoves()
    {
        for (int i = 1; i < _moves.Length; i++)
        {
            int score = _scores[i];
            Move move = _moves[i];
            int j = i - 1;

            while (j >= 0 && _scores[j] < score)
            {
                _scores[j + 1] = _scores[j];
                _moves[j + 1] = _moves[j];
                j--;
            }

            _scores[j + 1] = score;
            _moves[j + 1] = move;
        }
    }
}