namespace KV.Server;
using System;
using System.Threading.Tasks;
using KV.Server.Dtos.Users;
using Volo.Abp.Application.Dtos;

public interface IUsersAppService
{
    Task<PagedResultDto<UserDto>> GetListAsync(GetUserListRequestDto input);
    Task BlockUserAsync(Guid userId);
    Task UnBlockUserAsync(Guid userId);
    Task DeleteUserAsync(Guid userId);
    Task<CustomerUserProfileDto> GetUserByIdAsync(Guid? userId);
    Task CreateUserByTenantIdAsync(CreateUpdateUserDto dto);

}
