namespace KV.Server.PublicWeb;
using AutoMapper;
using KV.Server.Dtos.Employees;
using KV.Server.Dtos.Tickets;
using KV.Server.Profiles;
using KV.Server.Tickets;

public class ServerPublicWebAutoMapperProfile : Profile
{
    public ServerPublicWebAutoMapperProfile()
    {
        CreateMap<EmployeeProfile, EmployeeProfileDto>();
        this.CreateMap<Ticket, TicketDetailsDto>()
           .ForMember(dest => dest.CreatorEmail, cfg => cfg.MapFrom(x => x.Creator.Email))
           .ForMember(dest => dest.CreatorFirstName, cfg => cfg.MapFrom(x => x.Creator.Name))
           .ForMember(dest => dest.CreatorLastName, cfg => cfg.MapFrom(x => x.Creator.Surname))
           .ForMember(dest => dest.CreatorPhoneNumber, cfg => cfg.MapFrom(x => x.Creator.PhoneNumber))
           .ForMember(dest => dest.CreatorUserName, cfg => cfg.MapFrom(x => x.Creator.UserName))
           .ForMember(dest => dest.ResponsibleEmail, cfg => cfg.MapFrom(x => x.Responsible.IdentityUser.Email))
           .ForMember(dest => dest.ResponsibleFirstName, cfg => cfg.MapFrom(x => x.Responsible.FirstName))
           .ForMember(dest => dest.ResponsibleLastName, cfg => cfg.MapFrom(x => x.Responsible.LastName))
           .ForMember(dest => dest.ResponsibleJobPost, cfg => cfg.MapFrom(x => x.Responsible.JobPost))
           .ForMember(dest => dest.ResponsiblePhoneNumber,
               cfg => cfg.MapFrom(x => x.Responsible.Phone))
           .ForMember(dest => dest.ResponsibleUserName, cfg => cfg.MapFrom(x => x.Responsible.IdentityUser.UserName))
           .ForMember(dest => dest.ResponsibleFileNameAvatar,
               cfg => cfg.MapFrom(x => x.Responsible.UserAvatarFile.FileName))
           .ForMember(dest => dest.ClientCompanyShortName, cfg => cfg.MapFrom(x => x.TenantProfile.ShortName))
           .ForMember(dest => dest.TicketStatusDisplayName, cfg => cfg.MapFrom(x => x.TicketStatus.DisplayName))
           .ForMember(dest => dest.TicketStatusName, cfg => cfg.MapFrom(x => x.TicketStatus.Name))
           .ForMember(dest => dest.TicketStatusStyle, cfg => cfg.MapFrom(x => x.TicketStatus.Style))
           .ForMember(dest => dest.TicketTypeName, cfg => cfg.MapFrom(x => x.TicketType.Name))
               .ForMember(dest => dest.ResponsibleAvatarId, cfg => cfg.MapFrom(x => x.Responsible.UserAvatarFileId))
           .ForMember(dest => dest.TicketSection, cfg => cfg.MapFrom(x => x.TicketSection.Name))
           .ForMember(dest => dest.Code, opt => opt.MapFrom(src => $"KB-{src.Id}"));
    }
}
