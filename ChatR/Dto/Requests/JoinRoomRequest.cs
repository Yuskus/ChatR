using ChatR.Models;

namespace ChatR.Dto.Requests;

public record JoinRoomRequest(
    int UserId,
    RoomRole Role = RoomRole.Member);
