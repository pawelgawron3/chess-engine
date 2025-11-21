using ChessEngine.Chessboard;
using ChessEngine.Game;
using ChessEngine.Utils;

namespace ChessEngine.AI;

public class MovePicker
{
    private const int CAPTURE_BASE = 10_000;
    private const int KILLER_SCORE1 = 5_000;
    private const int KILLER_SCORE2 = 4_000;

    private readonly Move[] _moves;
    private readonly int[] _scores;
    private readonly int _depth;
    private int _index = 0;

    public MovePicker(IEnumerable<Move> moves, GameState state, int depth)
    {
        _moves = moves.ToArray();
        _scores = new int[_moves.Length];
        _depth = depth;

        ScoreMoves(state);
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

    private void ScoreMoves(GameState state)
    {
        for (int i = 0; i < _moves.Length; i++)
        {
            var move = _moves[i];

            if (AttackUtils.IsCapture(state.Board, move))
            {
                Piece? capturedPiece = AttackUtils.GetCapturedPiece(state.Board, move);

                int victim = PieceValues.Value[(int)capturedPiece!.Value.Type];
                int attacker = PieceValues.Value[(int)state.Board[move.From]!.Value.Type];

                _scores[i] = CAPTURE_BASE + victim * 10 - attacker;
                continue;
            }

            if (KillerMoves.KillerMovesTable[_depth, 0] != null &&
                KillerMoves.KillerMovesTable[_depth, 0]!.Value.Equals(move))
            {
                _scores[i] = KILLER_SCORE1;
                continue;
            }

            if (KillerMoves.KillerMovesTable[_depth, 1] != null &&
                KillerMoves.KillerMovesTable[_depth, 1]!.Value.Equals(move))
            {
                _scores[i] = KILLER_SCORE2;
                continue;
            }

            _scores[i] = 0;
        }
    }

    private void SortMoves()
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