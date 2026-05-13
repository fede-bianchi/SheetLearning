using MusicApp.Domain.Entities;

namespace MusicApp.Application.Interfaces;

public interface INotificationService
{
    Task SendAsync(
        int     recipientUserId,
        string  tipo,
        string  titolo,
        string? corpo      = null,
        string? targetType = null,
        int?    targetId   = null);

    Task SendLezionePropostaAsync(LessonBooking booking, string studentNickname);
    Task SendLezioneConfermataAsync(LessonBooking booking, string teacherNickname);
    Task SendLezioneCancellataAsync(LessonBooking booking, int cancellerUserId);
    Task SendLezioneCompletataAsync(LessonBooking booking, string teacherNickname);

    Task SendCommentoRispostaAsync(Comment reply, string authorNickname,
                                   int parentCommentAuthorId);
    Task SendPostRispostaAsync(Comment comment, string authorNickname,
                               int postAuthorId);

    Task SendLivelloSbloccatoAsync(int userId, string livelloNome,
                                   string esercizioNome, int livelloId);
    Task SendRecordBattuoAsync(int userId, string esercizioNome,
                               int punteggio);

    Task SendMessaggioRicevutoAsync(int recipientUserId, string senderNickname,
                                    string contenutoPreview, int chatId);

    Task SendBundleInScadenzaAsync(int userId, string bundleNome,
                                   int lezioniRimanenti, DateOnly dataScadenza,
                                   int bundlePurchaseId);
    Task SendAbbonamentoInScadenzaAsync(int userId, DateOnly dataFine,
                                        int subscriptionId);

    Task SendModerazioneRicevutaAsync(int targetUserId, string tipoContenuto,
                                      string motivazione);
}
