using Microsoft.EntityFrameworkCore;
using PokerAPIMPwDB.Common.Results;
using PokerAPIMPwDB.DTO.Table;
using PokerAPIMPwDB.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PokerAPIMPwDB.Infrastructure.Services
{
    public class LobbyService
    {
        private readonly AppDbContext _dbContext;

        public LobbyService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public ServiceResult<List<TableInfoDto>> GetAllTables()
        {
            try
            {
                var tables = _dbContext.Tables
                    .Include(t => t.PlayerSeats)
                    .Select(t => new TableInfoDto
                    {
                        TableId = t.Id,
                        Name = t.Name,
                        MaxPlayers = t.MaxPlayers,
                        PlayerCount = t.PlayerSeats.Count,
                        // SmallBlind = t.SmallBlind,
                        // BigBlind = t.BigBlind,
                        // State = t.State
                    })
                    .ToList();

                return ServiceResult<List<TableInfoDto>>.Success(tables);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<TableInfoDto>>.Fail("Failed to fetch tables: " + ex.Message);
            }
        }

        public ServiceResult<TableInfoDto> GetTable(Guid id)
        {
            var table = _dbContext.Tables
                .Include(t => t.PlayerSeats)
                .FirstOrDefault(t => t.Id == id);

            if (table == null)
                return ServiceResult<TableInfoDto>.Fail("Table not found");

            var dto = new TableInfoDto
            {
                TableId = table.Id,
                Name = table.Name,
                MaxPlayers = table.MaxPlayers,
                PlayerCount = table.PlayerSeats.Count,
                // SmallBlind = table.SmallBlind,
                // BigBlind = table.BigBlind,
                // State = table.State
            };

            return ServiceResult<TableInfoDto>.Success(dto);
        }
    }
}
