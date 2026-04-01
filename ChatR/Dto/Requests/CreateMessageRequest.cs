namespace ChatR.Dto.Requests;

public record CreateMessageRequest(
    string Content,
    int UserId);
