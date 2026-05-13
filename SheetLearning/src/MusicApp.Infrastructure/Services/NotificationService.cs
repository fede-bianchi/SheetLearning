using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;
using MusicApp.Application.Notifications;
using MusicApp.Domain.Entities;

namespace MusicApp.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repo;
    private readonly INotificationPushService _pushService;

    public NotificationService(INotificationRepository repo, INotificationPushService pushService)
    {
        _repo = repo;
        _pushService = pushService;
    }

    public async Task SendAsync(
        int     recipientUserId,
        string  tipo,
        string  titolo,
        string? corpo      = null,
        string? targetType = null,
        int?    targetId   = null)
    {
        var notification = await _repo.CreateAsync(new Notification
        {
            UserId     = recipientUserId,
            Tipo       = tipo,
            Titolo     = titolo,
            Corpo      = corpo,
            TargetType = targetType,
            TargetId   = targetId,
            IsRead     = false,
            IsArchived = false,
            CreatedAt  = DateTime.UtcNow
        });

        var dto = MapToDto(notification);

        _ = _pushService.PushAsync(recipientUserId, dto);

        var count = await _repo.GetUnreadCountAsync(recipientUserId);
        _ = _pushService.PushUnreadCountAsync(recipientUserId, count);
    }

    public Task SendLezionePropostaAsync(
        LessonBooking booking, string studentNickname)
    {
        var dataOra = booking.Slot.DataOraInizio.ToString("dd/MM/yyyy HH:mm");
        return SendAsync(
            recipientUserId : booking.TeacherId,
            tipo            : NotificationTypes.LezionePropostaInsegnante,
            titolo          : "Nuova richiesta di lezione",
            corpo           : $"{studentNickname} ha richiesto una lezione per il {dataOra}.",
            targetType      : "lesson_booking",
            targetId        : booking.Id);
    }

    public Task SendLezioneConfermataAsync(
        LessonBooking booking, string teacherNickname)
    {
        var dataOra = booking.Slot.DataOraInizio.ToString("dd/MM/yyyy HH:mm");
        return SendAsync(
            recipientUserId : booking.StudentId,
            tipo            : NotificationTypes.LezioneConfermata,
            titolo          : "Lezione confermata",
            corpo           : $"La tua lezione del {dataOra} con {teacherNickname} è stata confermata.",
            targetType      : "lesson_booking",
            targetId        : booking.Id);
    }

    public Task SendLezioneCancellataAsync(
        LessonBooking booking, int cancellerUserId)
    {
        int recipientId = cancellerUserId == booking.StudentId
            ? booking.TeacherId
            : booking.StudentId;

        var dataOra = booking.Slot.DataOraInizio.ToString("dd/MM/yyyy HH:mm");
        return SendAsync(
            recipientUserId : recipientId,
            tipo            : NotificationTypes.LezioneCancellata,
            titolo          : "Lezione cancellata",
            corpo           : $"La lezione del {dataOra} è stata cancellata.",
            targetType      : "lesson_booking",
            targetId        : booking.Id);
    }

    public Task SendLezioneCompletataAsync(
        LessonBooking booking, string teacherNickname)
    {
        var dataOra = booking.Slot.DataOraInizio.ToString("dd/MM/yyyy HH:mm");
        return SendAsync(
            recipientUserId : booking.StudentId,
            tipo            : NotificationTypes.LezioneCompletata,
            titolo          : "Lezione completata — lascia una valutazione",
            corpo           : $"La lezione del {dataOra} con {teacherNickname} è stata completata. "
                            + "Hai 48 ore per lasciare un feedback.",
            targetType      : "lesson_booking",
            targetId        : booking.Id);
    }

    public Task SendCommentoRispostaAsync(
        Comment reply, string authorNickname, int parentCommentAuthorId)
    {
        if (reply.UserId == parentCommentAuthorId)
            return Task.CompletedTask;

        var preview = TruncatePreview(reply.Contenuto, 80);
        return SendAsync(
            recipientUserId : parentCommentAuthorId,
            tipo            : NotificationTypes.CommentiRisposta,
            titolo          : $"{authorNickname} ha risposto al tuo commento",
            corpo           : preview,
            targetType      : "comment",
            targetId        : reply.Id);
    }

    public Task SendPostRispostaAsync(
        Comment comment, string authorNickname, int postAuthorId)
    {
        if (comment.UserId == postAuthorId)
            return Task.CompletedTask;

        var preview = TruncatePreview(comment.Contenuto, 80);
        return SendAsync(
            recipientUserId : postAuthorId,
            tipo            : NotificationTypes.PostRisposta,
            titolo          : $"{authorNickname} ha commentato il tuo post",
            corpo           : preview,
            targetType      : "post",
            targetId        : comment.PostId);
    }

    public Task SendLivelloSbloccatoAsync(
        int userId, string livelloNome, string esercizioNome, int livelloId)
        => SendAsync(
            recipientUserId : userId,
            tipo            : NotificationTypes.LivelloSbloccato,
            titolo          : "Nuovo livello sbloccato!",
            corpo           : $"Hai sbloccato «{livelloNome}» in {esercizioNome}.",
            targetType      : "level",
            targetId        : livelloId);

    public Task SendRecordBattuoAsync(
        int userId, string esercizioNome, int punteggio)
        => SendAsync(
            recipientUserId : userId,
            tipo            : NotificationTypes.RecordBattuto,
            titolo          : "Nuovo record personale!",
            corpo           : $"Hai battuto il tuo record in {esercizioNome} con {punteggio} punti.",
            targetType      : null,
            targetId        : null);

    public Task SendMessaggioRicevutoAsync(
        int recipientUserId, string senderNickname,
        string contenutoPreview, int chatId)
    {
        var preview = TruncatePreview(contenutoPreview, 80);
        return SendAsync(
            recipientUserId : recipientUserId,
            tipo            : NotificationTypes.MessaggioRicevuto,
            titolo          : $"Nuovo messaggio da {senderNickname}",
            corpo           : preview,
            targetType      : "chat",
            targetId        : chatId);
    }

    public Task SendBundleInScadenzaAsync(
        int userId, string bundleNome, int lezioniRimanenti,
        DateOnly dataScadenza, int bundlePurchaseId)
        => SendAsync(
            recipientUserId : userId,
            tipo            : NotificationTypes.BundleInScadenza,
            titolo          : "Il tuo bundle sta per scadere",
            corpo           : $"Il bundle «{bundleNome}» scade il {dataScadenza:dd/MM/yyyy}. "
                            + $"Hai ancora {lezioniRimanenti} lezioni disponibili.",
            targetType      : "lesson_bundle_purchase",
            targetId        : bundlePurchaseId);

    public Task SendAbbonamentoInScadenzaAsync(
        int userId, DateOnly dataFine, int subscriptionId)
        => SendAsync(
            recipientUserId : userId,
            tipo            : NotificationTypes.AbbonamentoInScadenza,
            titolo          : "Il tuo piano Pro sta per scadere",
            corpo           : $"Il tuo abbonamento Pro scade il {dataFine:dd/MM/yyyy}. "
                            + "Rinnova per continuare ad accedere a tutti i vantaggi.",
            targetType      : "subscription",
            targetId        : subscriptionId);

    public Task SendModerazioneRicevutaAsync(
        int targetUserId, string tipoContenuto, string motivazione)
        => SendAsync(
            recipientUserId : targetUserId,
            tipo            : NotificationTypes.ModerazioneRicevuta,
            titolo          : "Un tuo contenuto è stato rimosso",
            corpo           : $"Un tuo {tipoContenuto} è stato rimosso dall'amministrazione. "
                            + $"Motivo: {motivazione}.",
            targetType      : null,
            targetId        : null);

    private static NotificationDto MapToDto(Notification n)
        => new(
            n.Id,
            n.Tipo,
            n.Titolo,
            n.Corpo,
            n.TargetType,
            n.TargetId,
            n.IsRead,
            n.IsArchived,
            n.CreatedAt,
            n.ReadAt
        );

    private static string TruncatePreview(string text, int maxLength)
        => text.Length <= maxLength
            ? text
            : text[..maxLength] + "…";
}
