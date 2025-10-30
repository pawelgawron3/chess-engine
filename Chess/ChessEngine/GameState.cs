﻿namespace ChessEngine;

public class GameState
{
    public Board Board { get; }
    public Player CurrentPlayer { get; internal set; } = Player.White;
    public Position? SelectedPosition { get; private set; }
    public Result? GameResult { get; internal set; }
    public GameServices Services { get; }

    public event Action<MoveRecord>? MoveMade;

    public event Action<Result?>? OnGameEnded;

    internal void RaiseMoveMade(MoveRecord record) => MoveMade?.Invoke(record);

    internal void RaiseGameEnded(Result? result) => OnGameEnded?.Invoke(result);

    public GameState()
    {
        Board = new Board();
        Board.Initialize();
        Services = new GameServices(this);
    }

    public IEnumerable<Move> GetLegalMoves() => LegalMoveGenerator.GenerateLegalMoves(this);

    public IEnumerable<Move> GetLegalMovesForPiece() =>
        (SelectedPosition != null) ? LegalMoveGenerator.GenerateLegalMovesForPiece(this) : Enumerable.Empty<Move>();

    public void SelectPosition(Position pos) => SelectedPosition = pos;

    public void ClearSelection() => SelectedPosition = null;

    public bool TryMakeMove(Move move) => Services.MakeMove(move);

    public void TryUndoMove() => Services.UndoMove();
}