namespace KV.Server;
using System.Linq;
using AutoMapper;
using KV.Server.Contracts;
using KV.Server.Dtos.Articles;
using KV.Server.Dtos.Employees;
using KV.Server.Dtos.File;
using KV.Server.Dtos.Region;
using KV.Server.Dtos.Tickets;
using KV.Server.Events;
using KV.Server.File;
using KV.Server.Notifications;
using KV.Server.Profiles;
using KV.Server.Tags;
using KV.Server.Templates;
using KV.Server.Tenants;
using KV.Server.TicketHoursSpent;
using KV.Server.Tickets;

public class ServerApplicationAutoMapperProfile : Profile
{
    public ServerApplicationAutoMapperProfile()
    {
        #region customer user profile

        this.CreateMap<CustomerUserProfile, CustomerUserProfileDto>()
            .ForMember(x => x.TenantProfileDisplayName, cfg => cfg.MapFrom(x => x.TenantProfile.DisplayName))
            .ForMember(x => x.PhoneNumber, cfg => cfg.MapFrom(x => x.IdentityUser.PhoneNumber))
            .ForMember(x => x.Email, cfg => cfg.MapFrom(x => x.Email))
            .ForMember(x => x.IsActive, cfg => cfg.MapFrom(x => x.IdentityUser.IsActive))
            .ForMember(x => x.UserName, cfg => cfg.MapFrom(x => x.IdentityUser.UserName));
        this.CreateMap<CustomerUserProfile, UserDto>()
            .ForMember(x => x.FullName, cfg => cfg.MapFrom(x => $"{x.LastName} {x.MiddleName} {x.FirstName}"))
            .ForMember(x => x.TenantShortName, cfg => cfg.MapFrom(x => x.TenantProfile.ShortName))
            .ForMember(x => x.IsActive, cfg => cfg.MapFrom(x => x.IdentityUser.IsActive))
            .ForMember(x => x.ActualizationTime, cfg => cfg.MapFrom(x => x.LastModificationTime));

        #endregion

        #region customer

        this.CreateMap<TenantProfile, CustomerDto>()
            .ForMember(x => x.ResponsibleManager,
                cfg => cfg.MapFrom((x, d) => $"{x.ResponsibleManager?.LastName} {x.ResponsibleManager?.FirstName} {x.ResponsibleManager?.MiddleName}"))
            .ForMember(x => x.OrganizationName, cfg => cfg.MapFrom(x => x.LongName))
            .ForMember(x => x.ContractOwner, cfg => cfg.MapFrom(x => x.ContactUserProfile));
        this.CreateMap<CustomerDto, TenantProfile>();
        this.CreateMap<CreateUpdateCustomerDto, TenantProfile>();

        #endregion

        #region tenants

        this.CreateMap<TenantProfile, TenantProfileDto>();
        this.CreateMap<CreateUpdateTenantProfileDto, TenantProfile>();

        #endregion

        #region stories

        this.CreateMap<StoryItem, StoryDto>();
        this.CreateMap<StoryItem, DetailedStoryDto>();
        this.CreateMap<SlideItem, SlideDto>();

        #endregion

        #region contracts
        this.CreateMap<ContractServicePackage, ServicePackageDto>()
    .ForMember(x => x.Name, cfg => cfg.MapFrom(x => x.ServicePackage.Name));

        this.CreateMap<Contract, ContractDto>()
     .ForMember(x => x.StatusId, cfg => cfg.MapFrom(x => x.Status.Id))
     .ForMember(x => x.StatusTitle, cfg => cfg.MapFrom(x => x.Status.Title))
     .ForMember(x => x.StatusCode, cfg => cfg.MapFrom(x => x.Status.Code))
     .ForMember(x => x.ServicePackages, cfg => cfg.MapFrom(x => x.ServicePackages))
     .ForMember(x => x.TenantProfileShortName, cfg => cfg.MapFrom(x => x.TenantProfile.ShortName))
     .ForMember(x => x.ServicePackagesData, cfg => cfg.MapFrom(x => x.ServicePackagesData));
        this.CreateMap<ContractDto, Contract>();
        this.CreateMap<CreateUpdateContractDto, Contract>();

        #endregion

        #region service package

        this.CreateMap<ServicePackage, ServicePackageDto>();
        this.CreateMap<ServicePackageDto, ServicePackage>();
        this.CreateMap<CreateUpdateServicePackageDto, ServicePackage>();

        #endregion

        #region

        this.CreateMap<ConstraintType, ConstraintTypeDto>();

        #endregion

        #region contract statuses

        this.CreateMap<ContractStatus, ContractStatusDto>();
        this.CreateMap<CreateUpdateContractStatusDto, ContractStatus>();

        #endregion

        #region notification

        this.CreateMap<UserNotification, UserNotificationItemDto>()
            .ForMember(dest => dest.CategoryNotificationName, cfg => cfg.MapFrom(x => x.NotificationCategory.Name))
            .ForMember(dest => dest.CategoryNotificationStyle, cfg => cfg.MapFrom(x => x.NotificationCategory.Style));
        this.CreateMap<NotificationCategory, NotificationCategoryDto>();
        this.CreateMap<NotificationStatus, NotificationStatusInfoDto>();
        this.CreateMap<CreateUserNotificationDto, UserNotification>();

        #endregion

        #region tickets

        this.CreateMap<Ticket, TicketListItemDto>()
            .ForMember(dest => dest.TicketTypeName, cfg => cfg.MapFrom(x => x.TicketType.Name))
            .ForMember(dest => dest.CreatorFirstName, cfg => cfg.MapFrom(x => x.Creator.Name))
            .ForMember(dest => dest.CreatorLastName, cfg => cfg.MapFrom(x => x.Creator.Surname))
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => $"KB-{src.Id}"))
            .ForMember(dest => dest.CustomerShortName, opt => opt.MapFrom(cfg => cfg.TenantProfile.ShortName))
            .ForMember(dest => dest.ResponsibleFullName, cfg => cfg.MapFrom(x => $"{x.Responsible.GetShortFullName()}"))
            .ForMember(dest => dest.ResponsibleLastName, cfg => cfg.MapFrom(x => x.Responsible.LastName))
            .ForMember(dest => dest.TicketSectionName, cfg => cfg.MapFrom(x => x.TicketSection.Name))
            .ForMember(dest => dest.CreatorFullName, cfg => cfg.MapFrom(x => $"{x.Creator.Surname} {x.Creator.Name}"))
            .ForMember(dest => dest.TicketStatusName, cfg => cfg.MapFrom(x => x.TicketStatus.Name))
            .ForMember(dest => dest.TicketStatusDisplayName, cfg => cfg.MapFrom(x => x.TicketStatus.DisplayName))
            .ForMember(dest => dest.TicketStatusStyle, cfg => cfg.MapFrom(x => x.TicketStatus.Style));
        this.CreateMap<TicketListItemDto, Ticket>();

        this.CreateMap<UploadedFileDto, UploadedFile>();
        this.CreateMap<UploadedFile, UploadedFileDto>();

        this.CreateMap<TicketStatus, TicketStatusDto>();
        this.CreateMap<TicketStatusDto, TicketStatus>();

        this.CreateMap<ConstraintType, ConstraintTypeDto>();
        this.CreateMap<ConstraintTypeDto, ConstraintType>();

        this.CreateMap<TicketHistoryDto, TicketHistory>();
        this.CreateMap<TicketHistory, TicketHistoryDto>()
            .ForMember(dest => dest.FirstName, cfg => cfg.MapFrom(x => x.Creator.Name))
            .ForMember(dest => dest.LastName, cfg => cfg.MapFrom(x => x.Creator.Surname))
            .ForMember(dest => dest.UserName, cfg => cfg.MapFrom(x => x.Creator.UserName))
            .ForMember(dest => dest.IsSpecialistMessage, cfg => cfg.MapFrom((s, d) => {
                return s.Type != TicketHistoryType.CustomerMessage ? true : false;
            }));

        this.CreateMap<TicketType, TicketTypeDto>();
        this.CreateMap<TicketTypeDto, TicketType>();

        this.CreateMap<Tag, TagDto>();
        this.CreateMap<TagDto, Tag>();
        this.CreateMap<CreateUpdateTagDto, Tag>();

        this.CreateMap<CreateUpdateTicketDto, Ticket>();
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
            .ForMember(dest => dest.TicketSection, cfg => cfg.MapFrom(x => x.TicketSection.Name))
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => $"KB-{src.Id}"));

        this.CreateMap<CreateUpdateTicketMessageDto, TicketMessage>();
        this.CreateMap<TicketMessage, TicketMessageDto>()
            .ForMember(x => x.CreatorFirstName, cfg => cfg.MapFrom(x => x.CreatorCustomerUserProfile.FirstName))
            .ForMember(x => x.CreatorLastName, cfg => cfg.MapFrom(x => x.CreatorCustomerUserProfile.LastName))
            .ForMember(x => x.CreatorMiddleName, cfg => cfg.MapFrom(x => x.CreatorCustomerUserProfile.MiddleName))
            .ForMember(x => x.CreatorUserName,
                cfg => cfg.MapFrom(x => x.CreatorCustomerUserProfile.IdentityUser.UserName))
            .ForMember(x => x.CreatorEmail, cfg => cfg.MapFrom(x => x.CreatorCustomerUserProfile.IdentityUser.Email));

        this.CreateMap<CreateUpdateTicketHoursSpentHistoryDto, TicketHoursSpentHistory>();
        this.CreateMap<TicketHoursSpentHistory, TicketHoursSpentHistoryDto>()
            .ForMember(x => x.ConstraintTypeName, cfg => cfg.MapFrom(x => x.ConstraintType.Name));

        this.CreateMap<TicketSection, TicketSectionDto>();
        this.CreateMap<TicketSectionDto, TicketSection>();
        this.CreateMap<CreateUpdateTicketSectionDto, TicketSection>();

        #endregion

        #region Articles

        this.CreateMap<Datum, ArticlesResultDto>()
            .ForMember(x => x.Id, cfg => cfg.MapFrom(x => x.Id))
            .ForMember(x => x.Title, cfg => cfg.MapFrom(x => x.Attributes.Title))
            .ForMember(x => x.Text, cfg => cfg.MapFrom(x => x.Attributes.Text))
            .ForMember(x => x.Slug, cfg => cfg.MapFrom(x => x.Attributes.Slug))
            .ForMember(x => x.Seo, cfg => cfg.MapFrom(x => x.Attributes.Seo))
            .ForMember(x => x.StartDate, cfg => cfg.MapFrom(x => x.Attributes.StartDate))
            .ForMember(x => x.CreationTime, cfg => cfg.MapFrom(x => x.Attributes.CreatedAt))
            .ForMember(x => x.LastModificationTime, cfg => cfg.MapFrom(x => x.Attributes.UpdatedAt))
            .ForMember(x => x.PublishedAt, cfg => cfg.MapFrom(x => x.Attributes.PublishedAt));

        #endregion

        #region event

        this.CreateMap<CreateEventInTicketDto, CalendarEvent>();
        this.CreateMap<CalendarEvent, EventDto>();

        #endregion

        #region contract setting

        this.CreateMap<ContractSetting, ContractSettingDto>();

        #endregion

        #region answer template

        this.CreateMap<AnswerTemplate, AnswerTemplateDto>();
        this.CreateMap<AnswerTemplateDto, AnswerTemplate>();
        this.CreateMap<CreateUpdateAnswerTemplateDto, AnswerTemplate>();

        CreateMap<EmployeeProfile, EmployeeProfileDto>();
        #endregion

        #region region
        CreateMap<Region, RegionDto>();
        #endregion

    }
}
