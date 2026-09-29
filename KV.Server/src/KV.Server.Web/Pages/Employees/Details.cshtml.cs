namespace KV.Server.Web.Pages.Employees;
using System;
using System.ComponentModel.DataAnnotations;
using KV.Server.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class DetailsModel : ServerPageModel
{
    public DetailsModel()
    {
    }

    [HiddenInput][BindProperty] public Guid Id { get; set; }

    public void OnGet(Guid id) => this.Id = id;

    public class EmployeeViewModel
    {
        [HiddenInput] public Guid Id { get; set; }

        [MaxLength(256)] public virtual string LastName { get; set; }

        [MaxLength(256)] public virtual string MiddleName { get; set; }

        [MaxLength(512)] public virtual string Street { get; set; }

        [MaxLength(256)] public virtual string City { get; set; }

        [MaxLength(256)] public virtual string Country { get; set; }

        public virtual string JobPost { get; set; }

        public virtual DateTime? DateOfBirthDay { get; set; }

        public virtual string Note { get; set; }
    }
}
