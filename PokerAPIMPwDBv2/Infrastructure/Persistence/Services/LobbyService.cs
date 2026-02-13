using Microsoft.EntityFrameworkCore;
using PokerAPIMPwDB.Common.Results;
using PokerAPIMPwDB.DTO.Table;
using PokerAPIMPwDB.Infrastructure.Persistence;
using PokerAPIMPwDB.Infrastructure.Persistence.Repositories;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokerAPIMPwDB.Infrastructure.Services
{
    public class LobbyService
    {
        private readonly ITableRepository _tableRepo;
        private readonly IMapper _mapper;

        public LobbyService(ITableRepository tableRepo, IMapper mapper)
        {
            _tableRepo = tableRepo;
            _mapper = mapper;
        }

        public async Task<ServiceResult<List<TableInfoDto>>> GetAllTablesAsync()
        {
            try
            {
                var tables = await _tableRepo.GetActiveTablesAsync();
                var dtos = _mapper.Map<List<TableInfoDto>>(tables);
                return ServiceResult<List<TableInfoDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<TableInfoDto>>.Fail("Failed to fetch tables: " + ex.Message);
            }
        }

        public async Task<ServiceResult<TableInfoDto>> GetTableAsync(Guid id)
        {
            var table = await _tableRepo.GetTableWithSeatsAsync(id);

            if (table == null)
                return ServiceResult<TableInfoDto>.Fail("Table not found");

            var dto = _mapper.Map<TableInfoDto>(table);
            return ServiceResult<TableInfoDto>.Success(dto);
        }
    }
}
