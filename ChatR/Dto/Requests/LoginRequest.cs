namespace ChatR.Dto.Requests;

public record LoginRequest(
    string Email,
    string Password);