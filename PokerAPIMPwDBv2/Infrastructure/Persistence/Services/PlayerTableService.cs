using Microsoft.EntityFrameworkCore;
using PokerAPIMPwDB.Infrastructure.Persistence;
using PokerAPIMPwDB.Infrastructure.Persistence.Entities;
using PokerAPIMPwDB.Domain.Enums;
using PokerAPIMPwDB.Common.Results;
using System;
using System.Threading.Tasks;


public class PlayerServiceTable : IPlayerServiceTable
{
    private readonly AppDbContext _db;

    public PlayerServiceTable(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ServiceResult<Player>> JoinTableAsync(Guid tableId, Guid userId, int buyInAmount, int seatNumber)
    {
        // Ambil table dan user sekaligus dari DB
        var table = await _db.Tables
            .Include(t => t.PlayerSeats)
            .ThenInclude(ps => ps.Player)
            .FirstOrDefaultAsync(t => t.Id == tableId);

        if (table == null)
            return ServiceResult<Player>.Fail("Table not found");

        var user = await _db.Users.FindAsync(userId);
        if (user == null)
            return ServiceResult<Player>.Fail("User not found");

        if (table.PlayerSeats.Count >= table.MaxPlayers)
            return ServiceResult<Player>.Fail("Table is full");

        if (buyInAmount <= 0 || buyInAmount > user.Balance)
            return ServiceResult<Player>.Fail("Invalid buy-in amount");

        // Kurangi balance user → pastikan tracked
        user.Balance -= buyInAmount;
        _db.Users.Update(user);

        // Buat player baru
        var player = new Player
        {
            UserId = user.Id,
            DisplayName = user.UserName!,
            ChipStack = buyInAmount,
            State = PlayerState.Active
        };
        _db.Players.Add(player);

        // Buat seat baru
        var seat = new PlayerSeat
        {
            TableId = table.Id,
            Player = player,
            SeatNumber = seatNumber
        };
        _db.PlayerSeats.Add(seat);

        // Simpan semua dalam satu transaksi
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return ServiceResult<Player>.Fail("Concurrency error: entity was modified or deleted");
        }

        return ServiceResult<Player>.Success(player);
    }


    public async Task<ServiceResult<bool>> LeaveTableAsync(Guid tableId, Guid playerId)
    {
        // 1️⃣ Ambil table beserta player seats
        var table = await _db.Tables
            .Include(t => t.PlayerSeats)
            .ThenInclude(ps => ps.Player)
            .FirstOrDefaultAsync(t => t.Id == tableId);

        if (table == null)
            return ServiceResult<bool>.Fail("Table not found");

        // 2️⃣ Cari seat player
        var seat = table.PlayerSeats.FirstOrDefault(ps => ps.PlayerId == playerId);
        if (seat == null)
            return ServiceResult<bool>.Fail("Player not found at this table");

        // 3️⃣ Kembalikan chip ke user (optional)
        var player = seat.Player;
        if (player != null)
        {
            var user = await _db.Users.FindAsync(player.UserId);
            if (user != null)
            {
                user.Balance += player.ChipStack; // kembalikan chip
                _db.Users.Update(user);
            }
        }

        // 4️⃣ Hapus seat & player
        _db.PlayerSeats.Remove(seat);
        if (player != null)
        {
            _db.Players.Remove(player);
        }

        await _db.SaveChangesAsync();
        return ServiceResult<bool>.Success(true);
    }

}
