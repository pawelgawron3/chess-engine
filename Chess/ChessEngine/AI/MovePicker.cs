using ChessEngine.Chessboard;
using ChessEngine.Game;
using ChessEngine.Utils;

namespace ChessEngine.AI;

public class MovePicker
{
    private readonly Move[] _moves;
    private readonly int[] _scores;
    private int _index = 0;

    public MovePicker(IEnumerable<Move> moves, GameState state)
    {
        _moves = moves.ToArray();
        _scores = new int[_moves.Length];

        ScoreMoves(state);
        SortMoves();
    }

    private void ScoreMoves(GameState state)
    {
        for (int i = 0; i < _moves.Length; i++)
        {
            var move = _moves[i];
            Piece? capturedPiece = AttackUtils.GetCapturedPiece(state.Board, move);

            if (capturedPiece != null)
            {
                int victim = PieceValues.Value[(int)capturedPiece.Value.Type];
                int attacker = PieceValues.Value[(int)state.Board[move.From.Row, move.From.Column]!.Value.Type];

                // MVV-LVA
                _scores[i] = victim * 10 - attacker;
            }
            else
            {
                _scores[i] = 0;
            }
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
}