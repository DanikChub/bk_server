namespace KV.Server.Web;

using System.Linq;
using Amazon.S3.Model;
using AutoMapper;
using KV.Server.Contracts;
using KV.Server.Dtos.Employees;
using KV.Server.Dtos.Tickets;
using KV.Server.Dtos.Users;
using KV.Server.Profiles;
using KV.Server.Tickets;
using KV.Server.Web.Pages.AnswerTemplates;
using KV.Server.Web.Pages.Contracts;
using KV.Server.Web.Pages.ContractStatuses;
using KV.Server.Web.Pages.Customers;
using KV.Server.Web.Pages.Employees;
using KV.Server.Web.Pages.Stories;
using KV.Server.Web.Pages.Tenants;
using KV.Server.Web.Pages.Tickets;
using KV.Server.Web.Pages.TicketSections;
using KV.Server.Web.Pages.Users;
using Microsoft.AspNetCore.Identity;
using Volo.Abp.Identity;
using Volo.Abp.TenantManagement;
using static global::Server.Web.Pages.Customers.IndexModel;
using static KV.Server.Web.Pages.Contracts.IndexModel;
using static KV.Server.Web.Pages.Customers.DetailsModel;
using static KV.Server.Web.Pages.Stories.CreateSliderModalModel;
using static KV.Server.Web.Pages.Stories.CreateStoryModalModel;
using static KV.Server.Web.Pages.Users.IndexModel;

public class ServerWebAutoMapperProfile : Profile
{
    public ServerWebAutoMapperProfile()
    {
        #region search
        this.CreateMap<Contract, ContractDto>()
     .ForMember(x => x.StatusId, cfg => cfg.MapFrom(x => x.Status.Id))
     .ForMember(x => x.StatusTitle, cfg => cfg.MapFrom(x => x.Status.Title))
     .ForMember(x => x.StatusCode, cfg => cfg.MapFrom(x => x.Status.Code))
     .ForMember(x => x.TenantProfileShortName, cfg => cfg.MapFrom(x => x.TenantProfile.ShortName))
     .ForMember(x => x.ServicePackagesData, cfg => cfg.MapFrom(x => x.ServicePackagesData));


        CreateMap<CreateContactVM, CreateUpdateContractDto>();

        this.CreateMap<Pages.Tickets.IndexModel.SearchViewModel, GetTicketsListRequestDto>()
            .ForMember(dest => dest.SearchTicketId, cfg => cfg.MapFrom(x => x.TicketId))
            .ForMember(dest => dest.SearchSubject, cfg => cfg.MapFrom(x => x.Subject))
            .ForMember(dest => dest.SearchCreationTime, cfg => cfg.MapFrom(x => x.CreationTime))
            .ForMember(dest => dest.SearchDueDate, cfg => cfg.MapFrom(x => x.DueDate))
            .ForMember(dest => dest.SearchCustomerShortName, cfg => cfg.MapFrom(x => x.CustomerShortName))
            .ForMember(dest => dest.SearchCustomerLongName, cfg => cfg.MapFrom(x => x.CustomerLongName))
            .ForMember(dest => dest.SearchCreatorFullName, cfg => cfg.MapFrom(x => x.CreatorFullName))
            .ForMember(dest => dest.SearchResponsibleFullName, cfg => cfg.MapFrom(x => x.ResponsibleFullName))
            .ForMember(dest => dest.SearchTicketStatusId, cfg => cfg.MapFrom(x => x.UpperTicketStatusId))
            .ForMember(dest => dest.SearchTicketTypeId, cfg => cfg.MapFrom(x => x.TicketTypeId))
            .ForMember(dest => dest.SearchTicketSectionId, cfg => cfg.MapFrom(x => x.TicketSectionName))
            .ForMember(dest => dest.SearchStr, cfg => cfg.MapFrom(x => x.SearchStr));

        this.CreateMap<CreateUserVM, CreateUpdateUserDto>();
        this.CreateMap<CustomerUserProfileDto, CreateUpdateEmployeeViewModel>();
        this.CreateMap<CreateUpdateEmployeeViewModel, CreateUpdateCustomerUserProfileDto>();
        this.CreateMap<ContractSearchViewModel, GetContractListRequestDto>()
            .ForMember(dest => dest.SearchName, cfg => cfg.MapFrom(x => x.Name))
            .ForMember(dest => dest.SearchContractStartDate, cfg => cfg.MapFrom(x => x.ContractStartDate))
            .ForMember(dest => dest.SearchContractFinishDate, cfg => cfg.MapFrom(x => x.ContractFinishDate))
            .ForMember(dest => dest.SearchStatusId, cfg => cfg.MapFrom(x => x.StatusId))
            .ForMember(dest => dest.SearchTenantProfileShortName, cfg => cfg.MapFrom(x => x.TenantProfileShortName))
            .ForMember(dest => dest.SearchServicePackageId, cfg => cfg.MapFrom(x => x.ServicePackageId));

        this.CreateMap<CustomerSearchViewModel, GetCustomerListRequestDto>()
            .ForMember(dest => dest.SearchShortName, cfg => cfg.MapFrom(x => x.ShortName))
            .ForMember(dest => dest.SearchLongName, cfg => cfg.MapFrom(x => x.LongName))
            .ForMember(dest => dest.SearchAddress, cfg => cfg.MapFrom(x => x.Address))
            .ForMember(dest => dest.SearchStr, cfg => cfg.MapFrom(x => x.SearchStr))
            .ForMember(dest => dest.SearchResponsibleManager, cfg => cfg.MapFrom(x => x.ResponsibleManager));

        this.CreateMap<UserSearchViewModel, GetUserListRequestDto>()
            .ForMember(dest => dest.SearchFullName, cfg => cfg.MapFrom(x => x.FullName))
            .ForMember(dest => dest.SearchJobPost, cfg => cfg.MapFrom(x => x.JobPost))
            .ForMember(dest => dest.SearchDirection, cfg => cfg.MapFrom(x => x.Direction))
            .ForMember(dest => dest.SearchTenantShortName, cfg => cfg.MapFrom(x => x.TenantShortName))
            .ForMember(dest => dest.SearchActualizationTime, cfg => cfg.MapFrom(x => x.ActualizationTime))
            .ForMember(dest => dest.SearchResponsibleManagerId, cfg => cfg.MapFrom(x => x.ResponsibleManagerId));
        this.CreateMap<SearchStatisticsViewModel, GetCustomerStatisticsListRequestDto>()
            .ForMember(dest => dest.StartPeriod, cfg => cfg.MapFrom(x => x.StartPeriod))
            .ForMember(dest => dest.EndPeriod, cfg => cfg.MapFrom(x => x.EndPeriod))
            .ForMember(dest => dest.TenantId, cfg => cfg.MapFrom(x => x.TenantId));

        #endregion

        this.CreateMap<Tag, TagDto>();

        #region tenants

        this.CreateMap<TenantProfileDto, CreateUpdateTenantProfileDto>();
        this.CreateMap<CreateTenantProfileViewModel, CreateUpdateTenantProfileDto>();
        this.CreateMap<CreateTenantProfileViewModel, TenantCreateDto>()
            .ForMember(dest => dest.Name, cfg => cfg.MapFrom(x => x.ShortName));

        #endregion

        #region story

        this.CreateMap<StoryDto, UpdateStoryViewModel>();
        this.CreateMap<UpdateStoryViewModel, CreateUpdateStoryDto>();
        this.CreateMap<CreateStoryViewModel, CreateUpdateStoryDto>();

        #endregion

        #region slider

        this.CreateMap<SlideDto, UpdateSliderViewModel>();
        this.CreateMap<UpdateSliderViewModel, CreateUpdateSlideDto>();
        this.CreateMap<CreateSliderViewModel, CreateUpdateSlideDto>();

        #endregion

        #region contracts

        this.CreateMap<ContractDto, CreateUpdateContractDto>();
        this.CreateMap<ContractDto, UpdateContractViewModel>();

        this.CreateMap<CreateContactViewModel, CreateUpdateContractDto>();
        this.CreateMap<UpdateContractViewModel, CreateUpdateContractDto>();

        #endregion

        #region tenant contracts

        this.CreateMap<ContractDto, CreateUpdateContractDto>();
        this.CreateMap<ContractDto, UpdateTenantContractViewModel>();

        this.CreateMap<CreateContactViewModel, CreateUpdateContractDto>();
        this.CreateMap<UpdateTenantContractViewModel, CreateUpdateContractDto>();

        #endregion

        #region contract statuses

        this.CreateMap<ContractStatusDto, CreateUpdateContractStatusDto>();
        this.CreateMap<ContractStatusDto, UpdateContractStatusViewModel>();

        this.CreateMap<UpdateContractStatusViewModel, CreateUpdateContractStatusDto>();
        this.CreateMap<CreateContractStatusViewModel, CreateUpdateContractStatusDto>();

        #endregion

        #region template answer

        this.CreateMap<AnswerTemplateDto, CreateUpdateAnswerTemplateDto>();
        this.CreateMap<AnswerTemplateDto, UpdateAnswerTemplateViewModel>();

        this.CreateMap<UpdateAnswerTemplateViewModel, CreateUpdateAnswerTemplateDto>();
        this.CreateMap<CreateAnswerTemplateViewModel, CreateUpdateAnswerTemplateDto>();

        #endregion

        #region ticket section

        this.CreateMap<TicketSectionDto, CreateUpdateTicketSectionDto>();
        this.CreateMap<TicketSectionDto, UpdateTicketSectionViewModel>();

        this.CreateMap<UpdateTicketSectionViewModel, CreateUpdateTicketSectionDto>();
        this.CreateMap<CreateTicketSectionViewModel, CreateUpdateTicketSectionDto>();

        #endregion

        #region customer

        this.CreateMap<IdentityUserDto, Volo.Abp.Identity.IdentityUser>();
        this.CreateMap<CustomerDto, CustomerViewModel>()
            .ForMember(x=> x.ContractOwner, dest => dest.MapFrom(x => $"{x.ContractOwner.LastName} {x.ContractOwner.FirstName} {x.ContractOwner.MiddleName}"))
            .ForMember(x=> x.ContractEmail, dest => dest.MapFrom(src => src.Email));
        this.CreateMap<CustomerDto, UpdateCustomerViewModal>()
             .ForMember(x => x.ContractEmail, dest => dest.MapFrom(src => src.Email));
        this.CreateMap<UpdateCustomerViewModal, CreateUpdateCustomerDto>()
            .ForMember(x => x.PhoneNumber, dest => dest.MapFrom(src=> src.ContractPhoneNumber))
            .ForMember(x => x.Email, dest => dest.MapFrom(src=> src.ContractEmail));
        this.CreateMap<CustomerModal, CreateUpdateCustomerDto>();
        this.CreateMap<CreateCustomerViewModal, CreateUpdateCustomerDto>()
            .ForMember(x => x.PhoneNumber, dest => dest.MapFrom(src => src.ContractPhoneNumber))
            .ForMember(x => x.Email, dest => dest.MapFrom(src => src.ContractEmail));
        this.CreateMap<EditCustomerModal, CreateUpdateCustomerDto>();
        this.CreateMap<CustomerDto, EditCustomerModal>();
        this.CreateMap<CreateUpdateCustomerDto, CreateUpdateCustomerDto>()
            .ForMember(x => x.PhoneNumber, dest => dest.MapFrom(src => src.PhoneNumber))
            .ForMember(x => x.Email, dest => dest.MapFrom(src => src.Email));
        this.CreateMap<EditCustomerModal, CreateUpdateCustomerDto>();

        #endregion

        #region employee

        this.CreateMap<CustomerUserProfileDto, UpdateEmployeeViewModel>();
        this.CreateMap<CreateEmployeeViewModel, CustomerUserProfileDto>();

        this.CreateMap<CreateEmployeeViewModel, CreateUpdateCustomerUserProfileDto>();
        this.CreateMap<UpdateEmployeeViewModel, CreateUpdateCustomerUserProfileDto>();

        #endregion

        #region ticket history

        this.CreateMap<TicketHistoryDto, EditTicketHistoryViewModel>();

        #endregion

        this.CreateMap<Ticket, TicketDetailsDto>()
            .ForMember(dest => dest.ServicePackages, cfg => cfg.MapFrom(x => x.ServicePackages))
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
              .ForMember(dest => dest.ResponsibleAvatarId, cfg => cfg.MapFrom(x => x.Responsible.UserAvatarFileId))
          .ForMember(dest => dest.Code, opt => opt.MapFrom(src => $"KB-{src.Id}"));
        this.CreateMap<CreateTicketVM, CreateUpdateTicketDto>();
        this.CreateMap<CreateEventVM, CreateEventInTicketDto>();

        this.CreateMap<EmployeeProfile, EmployeeProfileDto>()
             .ForMember(dest => dest.Login, cfg => cfg.MapFrom(x => x.IdentityUser.UserName))
             .ForMember(dest => dest.Email, cfg => cfg.MapFrom(x => x.Email));
        this.CreateMap<CreateEmployeeVM, CreateUpdateEmployeeProfileDto>();
    }
}
