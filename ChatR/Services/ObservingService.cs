using ChatR.Models;
using ChatR.Models.Constatns;
using ChatR.Repos;

namespace ChatR.Services;

public class ObservingService(
    ObservingRepo observingRepo,
    UserRepo userRepo)
{
    private readonly ObservingRepo _observingRepo = observingRepo;
    private readonly UserRepo _userRepo = userRepo;

    public async Task<Observing?> Add(int userFromId, int userToId)
    {
        if (userFromId <= 0)
            throw new ArgumentException(Errors.ID_MUST_BE_POSITIVE, nameof(userFromId));
        if (userToId <= 0)
            throw new ArgumentException(Errors.ID_MUST_BE_POSITIVE, nameof(userToId));

        // Проверяем существование пользователя (наблюдатель)
        var userFromExists = await _userRepo.GetById(userFromId) != null;
        if (!userFromExists)
            throw new ArgumentException($"Пользователь с ID {userFromId} не найден", nameof(userFromId));
            
        // Проверяем существование пользователя (наблюдаемый)
        var userToExists = await _userRepo.GetById(userToId) != null;
        if (!userToExists)
            throw new ArgumentException($"Пользователь с ID {userToId} не найден", nameof(userToId));

        var exists = await _observingRepo.GetByIdPair(userFromId, userToId) != null;
        if (exists)
            throw new ArgumentException("Заявка уже создана");

        return await _observingRepo.Add(new Observing
        {
            UserFromId = userFromId,
            UserToId = userToId,
        });
    }

    public async Task Delete(int id)
    {
        if (id <= 0)
            throw new ArgumentException(Errors.ID_MUST_BE_POSITIVE, nameof(id));

        var observing = await _observingRepo.GetById(id)
            ?? throw new ArgumentException($"Заявка с ID {id} не найдена", nameof(id));

        await _observingRepo.Delete(observing.Id);
    }

    public async Task<IEnumerable<Observing>> GetUsersFromById(int userId)
    {
        if (userId <= 0)
            throw new ArgumentException(Errors.ID_MUST_BE_POSITIVE, nameof(userId));

        // Проверяем существование пользователя (наблюдатель)
        var userFromExists = await _userRepo.GetById(userId) != null;
        if (!userFromExists)
            throw new ArgumentException($"Пользователь с ID {userId} не найден", nameof(userId));

        return await _observingRepo.GetUsersFromById(userId);
    }

    public async Task<IEnumerable<Observing>> GetUsersToById(int userId)
    {
        if (userId <= 0)
            throw new ArgumentException(Errors.ID_MUST_BE_POSITIVE, nameof(userId));
            
        // Проверяем существование пользователя (наблюдаемый)
        var userToExists = await _userRepo.GetById(userId) != null;
        if (!userToExists)
            throw new ArgumentException($"Пользователь с ID {userId} не найден", nameof(userId));

        return await _observingRepo.GetUsersToById(userId);
    }

    public async Task<int> GetUsersFromCount(int userId)
    {
        if (userId <= 0)
            throw new ArgumentException(Errors.ID_MUST_BE_POSITIVE, nameof(userId));

        // Проверяем существование пользователя (наблюдатель)
        var userFromExists = await _userRepo.GetById(userId) != null;
        if (!userFromExists)
            throw new ArgumentException($"Пользователь с ID {userId} не найден", nameof(userId));

        return await _observingRepo.GetUsersFromCount(userId);
    }
    public async Task<int> GetUsersToCount(int userId)
    {
        if (userId <= 0)
            throw new ArgumentException(Errors.ID_MUST_BE_POSITIVE, nameof(userId));

        // Проверяем существование пользователя (наблюдаемый)
        var userToExists = await _userRepo.GetById(userId) != null;
        if (!userToExists)
            throw new ArgumentException($"Пользователь с ID {userId} не найден", nameof(userId));

        return await _observingRepo.GetUsersToCount(userId);
    }

    public async Task<Observing?> GetByIdPair(int userFromId, int userToId)
    {
        if (userFromId <= 0)
            throw new ArgumentException(Errors.ID_MUST_BE_POSITIVE, nameof(userFromId));
        if (userToId <= 0)
            throw new ArgumentException(Errors.ID_MUST_BE_POSITIVE, nameof(userToId));

        // Проверяем существование пользователя (наблюдатель)
        var userFromExists = await _userRepo.GetById(userFromId) != null;
        if (!userFromExists)
            throw new ArgumentException($"Пользователь с ID {userFromId} не найден", nameof(userFromId));
            
        // Проверяем существование пользователя (наблюдаемый)
        var userToExists = await _userRepo.GetById(userToId) != null;
        if (!userToExists)
            throw new ArgumentException($"Пользователь с ID {userToId} не найден", nameof(userToId));

        return await _observingRepo.GetByIdPair(userFromId, userToId);
    }

    public async Task<IEnumerable<User>> GetMutualObservings(int userId)
    {
        if (userId <= 0)
            throw new ArgumentException(Errors.ID_MUST_BE_POSITIVE, nameof(userId));

        // Проверяем существование пользователя
        var userFromExists = await _userRepo.GetById(userId) != null;
        if (!userFromExists)
            throw new ArgumentException($"Пользователь с ID {userId} не найден", nameof(userId));

        return await _observingRepo.GetMutualObservings(userId);
    }
}