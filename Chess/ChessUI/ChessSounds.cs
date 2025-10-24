using System.IO;
using System.Media;
using ChessEngine;

namespace ChessUI;

public static class ChessSounds
{
    private static readonly string BasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/Sounds");

    private static readonly SoundPlayer MoveSound = new SoundPlayer(Path.Combine(BasePath, "move-self.wav"));
    private static readonly SoundPlayer CaptureSound = new SoundPlayer(Path.Combine(BasePath, "capture.wav"));
    private static readonly SoundPlayer PromoteSound = new SoundPlayer(Path.Combine(BasePath, "promote.wav"));
    private static readonly SoundPlayer CastleSound = new SoundPlayer(Path.Combine(BasePath, "castle.wav"));
    private static readonly SoundPlayer CheckSound = new SoundPlayer(Path.Combine(BasePath, "move-check.wav"));

    static ChessSounds()
    {
        try
        {
            MoveSound.Load();
            CaptureSound.Load();
            PromoteSound.Load();
            CastleSound.Load();
            CheckSound.Load();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Sound load error: {ex.Message}");
        }
    }

    public static void PlaySoundForMove(Move move, Piece? capturedPiece, bool KingInCheck)
    {
        if (KingInCheck)
        {
            PlayCheckSound();
            return;
        }

        switch (move.Type)
        {
            case MoveType.Normal:
                if (capturedPiece == null)
                    PlayMoveSound();
                else
                    PlayCaptureSound();
                break;

            case MoveType.Castling:
                PlayCastleSound();
                break;

            case MoveType.Promotion:
                PlayPromoteSound();
                break;

            case MoveType.EnPassant:
                PlayCaptureSound();
                break;
        }
    }

    private static void PlayMoveSound()
    {
        MoveSound.Play();
    }

    private static void PlayCaptureSound()
    {
        CaptureSound.Play();
    }

    private static void PlayPromoteSound()
    {
        PromoteSound.Play();
    }

    private static void PlayCastleSound()
    {
        CastleSound.Play();
    }

    private static void PlayCheckSound()
    {
        CheckSound.Play();
    }
}